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

namespace comeconv
{

    //サロペートペア＆結合文字 検出＆文字除去
    //\ud83d\ude0a
    //か\u3099
    public class HttpUtilityEx2
    {
        public static string HtmlDelete(string s, string t = "")
        {
            if (!IsSurrogatePair(s)) return s;

            StringBuilder sb = new StringBuilder();
            TextElementEnumerator tee = StringInfo.GetTextElementEnumerator(s);

            tee.Reset();
            while (tee.MoveNext())
            {
                string te = tee.GetTextElement();
                if (1 < te.Length)
                    sb.Append(t); //サロペートペアまたは結合文字の場合
                else
                    sb.Append(te);
            }
            return sb.ToString();
        }

        public static bool IsSurrogatePair(string s)
        {
            StringInfo si = new StringInfo(s);
            return si.LengthInTextElements < s.Length;
        }
    }

    public class ConvComment : IDisposable
    {

        #region IDisposable Support
        private bool disposedValue = false; // 重複する呼び出しを検出するには

        private Form1 _form = null;
        private string _sfile = null;
        private string _dfile = null;

        //Debug
        public bool IsDebug { get; set; }

        public ConvComment(Form1 fo, string sfile, string dfile)
        {
            IsDebug = false;

            this._form = fo;
            this._sfile = sfile;
            this._dfile = dfile;
        }

        ~ConvComment()
        {
            this.Dispose();
        }

        //XML形式のコメントファイルを読み込み変換する
        public bool FileCopy(string sfile, string dfile)
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
                        if (line.StartsWith("<chat "))
                        {
                            while (!line.EndsWith("</chat>"))
                            {
                                line += "\r\n" + sr.ReadLine();
                            }
                            //チャットの処理
                            {
                                var data = new Dictionary<string, string>();
                                var xdoc = XDocument.Parse(line);
                                //var aaa = xdoc.Descendants("chat");
                                foreach (var ele in xdoc.Element("chat").Attributes())
                                {
                                    if (ele.Name.ToString() == "premium")
                                    {
                                        if (ele.Value.ToString() == "8" ||
                                            ele.Value.ToString() == "24")
                                            continue;
                                        else if (ele.Value.ToString() == "9" ||
                                            ele.Value.ToString() == "25")
                                            ele.Value = "1";
                                    }
                                    data[ele.Name.ToString()] = ele.Value.ToString();
                                }
                                //var aaa = HttpUtilityEx2.HtmlDecode(xdoc.Element("chat").Value.ToString());
                                data["content"] = HttpUtilityEx2.HtmlDelete(xdoc.Element("chat").Value.ToString(), "　");
                                line = Table2Xml(data).TrimEnd();
                            }
                        }
                        else if (line.StartsWith("<thread "))
                        {
                            //thread 
                        }
                        sw.WriteLine(line);
                    }
                }

            }
            catch (Exception Ex)
            {
                MessageBox.Show(Ex.Message);
                return false;
            }
            return true;

        }

        public string Table2Xml(IDictionary<string, string> data)
        {
            var result = string.Empty;
            var content = string.Empty;
            if (data.Count <= 0)
                return result;

            try
            {
                string value;
                foreach (var it in data)
                {
                    value = it.Value.ToString();
                    switch (it.Key.ToString())
                    {
                        case "thread":
                            result = "<chat " + it.Key.ToString() + @"=""" + value + @"""";
                            break;
                        case "no":
                            if (int.Parse(value) > -1)
                                result += " " + it.Key.ToString() + @"=""" + value + @"""";
                            break;
                        case "mail":
                            if (value != "")
                                result += " " + it.Key.ToString() + @"=""" + value + @"""";
                            break;
                        case "name":
                            if (value != "")
                                result += " " + it.Key.ToString() + @"=""" + value + @"""";
                            break;
                        case "premium":
                            if (value != "0")
                                result += " " + it.Key.ToString() + @"=""" + value + @"""";
                            break;
                        case "anonymity":
                            if (value != "0")
                                result += " " + it.Key.ToString() + @"=""" + value + @"""";
                            break;
                        case "score":
                            if (value != "0")
                                result += " " + it.Key.ToString() + @"=""" + value + @"""";
                            break;
                        case "origin":
                            if (value != "")
                                result += " " + it.Key.ToString() + @"=""" + value + @"""";
                            break;
                        case "locale":
                            if (value != "")
                                result += " " + it.Key.ToString() + @"=""" + value + @"""";
                            break;
                        case "content":
                            content = HttpUtility.HtmlEncode(value);
                            break;
                        default:
                            result += " " + it.Key.ToString() + @"=""" + value + @"""";
                            break;
                    }
                }
                result += ">" + content + "</chat>\r\n";
            }
            catch (Exception Ex)
            {
                DebugWrite.Writeln(nameof(Table2Xml), Ex);
                return result;
            }

            return result;
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
