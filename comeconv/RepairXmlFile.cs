using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Linq;
using System.Diagnostics;
using System.Web;
using System.Globalization;
using System.Text.RegularExpressions;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using comeconv.Prop;
using comeconv.Util;

namespace comeconv
{
    public class RepairXmlFile : IDisposable
    {
        #region IDisposable Support
        private bool disposedValue = false; // 重複する呼び出しを検出するには

        private Form1 _form = null;
        private Props _props = null;

        //Debug
        public bool IsDebug { get; set; }

        public RepairXmlFile(Form1 fo, Props props)
        {
            IsDebug = false;

            this._form = fo;
            this._props = props;
        }

        ~RepairXmlFile()
        {
            this.Dispose();
        }

        //壊れたXML形式のコメントファイルを読み込み復旧を試みる
        public bool XmlRepair(string sfile, string dfile)
        {
            var enc = new System.Text.UTF8Encoding(false);

            try
            {
                if (!File.Exists(sfile))
                {
                    return false;
                }
                using (var sr = new StreamReader(sfile, enc))
                using (var sw = new StreamWriter(dfile, true, enc))
                {
                    string line;
                    while (!sr.EndOfStream) // ファイルが最後になるまで順に読み込み
                    {
                        line = sr.ReadLine();
                        if (line.TrimStart().StartsWith("<chat "))
                        {
                            while (!line.EndsWith("</chat>"))
                            {
                                line += "\r\n" + sr.ReadLine();
                            }
                            //チャットの処理
                            {
                                var ttt = RepairChatData(line, _props).TrimEnd();
                                if (!string.IsNullOrEmpty(ttt))
                                    sw.WriteLine(ttt);
                                //else
                                //_form.AddLog("deleted:" + line, 9);
                            }
                        }
                        else if (line.TrimStart().StartsWith("<thread "))
                        {
                            sw.WriteLine(line);
                        }
                        else
                        {
                            sw.WriteLine(line);
                        }
                    }
               }
            }
            catch (Exception Ex)
            {
                DebugWrite.Writeln(nameof(XmlRepair), Ex);
                return false;
            }
            return true;
        }

        private string RepairChatData(string chat, Props props)
        {
            var data = new Dictionary<string, string>();
            if (string.IsNullOrEmpty(chat))
                return "";

            try
            {
                var ttt = chat.Replace("&amp;lt;", "&lt;").Replace("&amp;gt;", "&gt;");
                ttt = Utils.Encode(Utils.Decode(ttt));
                ttt = ttt.Replace("=&quot;", "=\"").Replace("&quot; ", "\" ");
                ttt = ttt.Replace("&quot;&gt;", "\">");
                ttt = ttt.Replace("&lt;chat ", "<chat ").Replace("&lt;/chat&gt;", "</chat>");
                return ttt;
            }
            catch (Exception Ex)
            {
                DebugWrite.Writeln(nameof(RepairChatData), Ex);
                return "";
            }
        }


        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: マネージド状態を破棄します (マネージド オブジェクト)。
                }

                // TODO: アンマネージド リソース (アンマネージド オブジェクト) を解放し、下のファイナライザーをオーバーライドします。
                // TODO: 大きなフィールドを null に設定します。

                disposedValue = true;
            }
        }

        // TODO: 上の Dispose(bool disposing) にアンマネージド リソースを解放するコードが含まれる場合にのみ、ファイナライザーをオーバーライドします。
        // ~ConvComment() {
        //   // このコードを変更しないでください。クリーンアップ コードを上の Dispose(bool disposing) に記述します。
        //   Dispose(false);
        // }

        // このコードは、破棄可能なパターンを正しく実装できるように追加されました。
        public void Dispose()
        {
            // このコードを変更しないでください。クリーンアップ コードを上の Dispose(bool disposing) に記述します。
            Dispose(true);
            // TODO: 上のファイナライザーがオーバーライドされる場合は、次の行のコメントを解除してください。
            // GC.SuppressFinalize(this);
        }
        #endregion

    }
}
