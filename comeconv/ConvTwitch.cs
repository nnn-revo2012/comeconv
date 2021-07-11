﻿using System;
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

    public class ConvTwitch : IDisposable
    {

        #region IDisposable Support
        private bool disposedValue = false; // 重複する呼び出しを検出するには

        private Form1 _form = null;
        private Props _props = null;

        //Debug
        public bool IsDebug { get; set; }

        public ConvTwitch(Form1 fo, Props props)
        {
            IsDebug = false;

            this._form = fo;
            this._props = props;
        }

        ~ConvTwitch()
        {
            this.Dispose();
        }

        //Twitch形式のコメントファイルを読み込み変換する
        public bool TwitchConvert(string sfile, string dfile)
        {
            var result = false;
            try
            {
                if (!File.Exists(sfile))
                    return false;

                var filetype = Utils.IsTwitchFileType(sfile);
                if (filetype == -1)
                    return false;

                switch (filetype)
                {
                    case 0:
                        result = TwitchConvertChatDownloader(sfile, dfile);
                        break;
                    case 1:
                        //result = TwitchConvertTwitchDownloaderJson(sfile, dfile);
                        break;
                    case 2:
                        //result = TwitchConvertTwitchDownloaderText(sfile, dfile);
                        break;
                }
            }
            catch (Exception Ex)
            {
                DebugWrite.Writeln(nameof(TwitchConvert), Ex);
                return false;
            }
            return result;
        }

        private string ConvChatData(IDictionary<string, string> data, Props props)
        {
            var del_flg = false;

            try
            {
                var ttt = HttpUtility.HtmlDecode(data["content"]);
                if (!data.ContainsKey("vpos"))
                    data["vpos"] = "0";
                if (!data.ContainsKey("date_usec"))
                    data["date_usec"] = "0";

                if (props.IsTwiVpos)
                {
                    if (long.TryParse(data["vpos"], out var ll))
                        data["vpos"] = (ll + props.TwiVpos).ToString();
                }
                //コメント長
                if (props.IsTwiCommLen)
                {
                    if (ttt.Length > props.TwiCommLen)
                        del_flg = true;
                }
                //絵文字処理
                if (props.IsTwiEmoji)
                {
                    if (Utils.IsSurrogatePair(ttt))
                    {
                        if (props.TwiEmojiMode == "tdel")
                            del_flg = true;
                        else
                            ttt = Utils.DelEmoji(ttt, "　");
                    }
                }

                data["content"] = ttt;
                if (del_flg)
                    return "";
                else
                    return ConvComment.Table2Xml(data);
            }
            catch (Exception Ex)
            {
                DebugWrite.Writeln(nameof(ConvChatData), Ex);
                return "";
            }
        }

        public bool TwitchConvertChatDownloader(string sfile, string dfile)
        {
            var enc = new System.Text.UTF8Encoding(false);

            try
            {
                using (var sr = new StreamReader(sfile, enc))
                using (var sw = new StreamWriter(dfile, true, enc))
                {
                    string line;
                    ConvComment.BeginXmlDoc(sw);
                    while (!sr.EndOfStream) // ファイルが最後になるまで順に読み込み
                    {
                        line = sr.ReadLine();
                        if (line.TrimStart().StartsWith("{'message_id':"))
                        {
                            //チャットの処理
                            {
                                var data = new Dictionary<string, string>();
                                var jo = JObject.Parse(line.Replace(": None", ": 'None'"));
                                data.Add("threadid", jo["message_id"].ToString());
                                data.Add("vpos", ((long)((double)jo["time_in_seconds"] * 100D)).ToString());
                                var utime = jo["timestamp"].ToString();
                                if (utime.Length > 6)
                                {
                                    //01 234567
                                    data.Add("date", utime.Substring(0, utime.Length - 6));
                                    data.Add("date_usec", utime.Substring(utime.Length - 6));
                                }
                                if (jo["author"]["colour"] != null)
                                    data.Add("color", jo["author"]["colour"].ToString());
                                data.Add("user_id", jo["author"]["name"].ToString());
                                data.Add("name", jo["author"]["display_name"].ToString());
                                data.Add("content", jo["message"].ToString());
                                if (data.Count() > 0)
                                {
                                    var ttt = ConvChatData(data, _props).TrimEnd();
                                    if (!string.IsNullOrEmpty(ttt))
                                        sw.WriteLine(ttt);
                                    //else
                                    //_form.AddLog("deleted:" + line, 9);
                                }
                            }
                        }
                    }
                    ConvComment.EndXmlDoc(sw);
                }

            }
            catch (Exception Ex)
            {
                DebugWrite.Writeln(nameof(TwitchConvertChatDownloader), Ex);
                return false;
            }
            return true;
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
