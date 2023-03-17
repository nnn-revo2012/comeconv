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
using System.Net;
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
                        result = TwitchConvertChatDownloader(sfile, dfile);
                        break;
                    case 5:
                        result = TwitchConvertTwitchDownloaderJson(sfile, dfile);
                        break;
                    case 10:
                        result = TwitchConvertTwitchDownloaderText(sfile, dfile);
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
                var ttt = WebUtility.HtmlDecode(data["content"]);
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

        private static Regex _RegLocations = new Regex("^(\\d+)\\-(\\d+)", RegexOptions.Compiled);
        public bool TwitchConvertChatDownloader(string sfile, string dfile)
        {
            var enc = new System.Text.UTF8Encoding(false);

            try
            {
                using (var sr = new StreamReader(sfile, enc))
                using (var sw = new StreamWriter(dfile, true, enc))
                {
                    StringBuilder sb = new StringBuilder();
                    string line;
                    int count = 0;
                    ConvComment.BeginXmlDoc(sw);
                    while (!sr.EndOfStream) // ファイルが最後になるまで順に読み込み
                    {
                        line = sr.ReadLine();
                        if (line == "[" || line == "]") continue;
                        else if (line.StartsWith("    {"))
                        {
                            sb.Clear();
                            sb.Append(line.TrimStart());
                            while (!line.StartsWith("    }"))
                            {
                                line = sr.ReadLine();
                                sb.Append(line.TrimStart());
                            }
                            line = sb.ToString();
                            line = line.TrimEnd().TrimEnd(',');
                        }
                        if (Utils.RgxCDJsonl.IsMatch(line.TrimStart()))
                        {
                            //チャットの処理
                            {
                                var data = new Dictionary<string, string>();
                                var jo = JObject.Parse(line.Replace(": None", ": 'None'"));
                                if (jo["author"] != null)
                                {
                                    if (jo["author"].Count() <= 0)
                                        continue;
                                }
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
                                if (jo["author"]["display_name"] != null)
                                    data.Add("name", jo["author"]["display_name"].ToString());
                                string message = jo["message"].ToString();
                                if (!_props.IsTwiCommType)
                                {
                                    data.Add("content", message);
                                }
                                else
                                {
                                    if (jo["emotes"] != null)
                                    {
                                        if (jo["action_type"] != null)
                                        {
                                            foreach (var emt in jo["emotes"])
                                            {
                                                if (!(bool)emt["is_custom_emoji"])
                                                    message = message.Replace(emt["name"].ToString(), emt["id"].ToString());
                                                else
                                                    message = message.Replace(emt["name"].ToString(), "");
                                            }
                                        }
                                        else
                                        {
                                            foreach (var emt in jo["emotes"])
                                            {
                                                string name = emt["name"].ToString();
                                                if (string.IsNullOrEmpty(name))
                                                {
                                                    var msg = jo["message"].ToString();
                                                    var loc = emt["locations"].ToString();
                                                    int start = Int32.Parse(_RegLocations.Match(loc).Groups[1].ToString());
                                                    int end = Int32.Parse(_RegLocations.Match(loc).Groups[2].ToString());
                                                    System.Text.Encoding hEncoding = System.Text.Encoding.GetEncoding("UTF-8");
                                                    byte[] btBytes = hEncoding.GetBytes(msg);
                                                    name = hEncoding.GetString(btBytes, start, end-start+1);
                                                }
                                                message = message.Replace(name, "");
                                            }
                                        }
                                    }
                                    if (jo["message_type"].ToString() == "paid_message")
                                    {
                                        if (_props.IsTwiGift)
                                        {
                                            message = jo["money"]["text"] + " " + message;
                                            data.Add("mail", "shita");
                                        }
                                        else
                                        {
                                            message = "";
                                        }
                                    }
                                    if (message.Length <= 0)
                                        continue;
                                    data.Add("content", message);
                                    if (data["content"].Contains(" gifted ") ||
                                    data["content"].Contains(" gifting "))
                                    {
                                        if (_props.IsTwiGift)
                                            data.Add("mail", "shita");
                                        else
                                            continue;
                                    }
                                    if (data["content"].Contains(" subscribed ") ||
                                        data["content"].Contains(" is now live!"))
                                    {
                                        if (_props.IsTwiSystem)
                                            data.Add("mail", "shita");
                                        else
                                            continue;
                                    }
                                }
                                if (data.Count() > 0)
                                {
                                    count++;
                                    var ttt = ConvChatData(data, _props).TrimEnd();
                                    if (!string.IsNullOrEmpty(ttt))
                                        sw.WriteLine(ttt);
                                    //else
                                    //_form.AddLog("deleted:" + line, 9);
                                }
                            }
                        }
                    }
                    _form.AddLog("コメント数: " + count, 1);
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

        private static Regex _RegUtc = new Regex("^\\[(.+ UTC)\\] (.+)\\: (.*)", RegexOptions.Compiled);
        private static Regex _RegRelative = new Regex("^\\[(\\d+\\:\\d+\\:\\d+)\\] (.+)\\: (.*)", RegexOptions.Compiled);
        public bool TwitchConvertTwitchDownloaderText(string sfile, string dfile)
        {
            var enc = new System.Text.UTF8Encoding(false);
            var timestamp = -1;  //0:UTC 1:Relative -1:ERROR

            try
            {
                using (var sr = new StreamReader(sfile, enc))
                {
                    string line;
                    line = sr.ReadLine();
                    if (!string.IsNullOrEmpty(_RegUtc.Match(line).Groups[1].ToString()))
                        timestamp = 0;
                    else if (!string.IsNullOrEmpty(_RegRelative.Match(line).Groups[1].ToString()))
                        timestamp = 1;
                    if (timestamp == -1)
                    {
                        return false;
                    }
                }

                using (var sr = new StreamReader(sfile, enc))
                using (var sw = new StreamWriter(dfile, true, enc))
                {
                    string line;
                    long last_vpos = 0;
                    long last_unixtime = 0;
                    int count = 0;
                    ConvComment.BeginXmlDoc(sw);
                    while (!sr.EndOfStream) // ファイルが最後になるまで順に読み込み
                    {
                        line = sr.ReadLine();
                        if (line.TrimStart().StartsWith("["))
                        {
                            //チャットの処理
                            {
                                var data = new Dictionary<string, string>();
                                switch (timestamp)
                                {
                                    case 0:
                                        var localtime = DateTime.Parse(_RegUtc.Match(line).Groups[1].ToString().Replace(" UTC", "Z"));
                                        var unixtime = Utils.GetUnixTime(localtime);
                                        if (last_unixtime > 0L)
                                        {
                                            var vpos = last_vpos + (unixtime - last_unixtime) * 100L;
                                            data.Add("vpos", vpos.ToString());
                                            last_vpos = vpos;
                                        }
                                        else
                                            data.Add("vpos", "0");
                                        last_unixtime = unixtime;
                                        data.Add("date", unixtime.ToString());
                                        //data.Add("ddd", Utils.GetUnixToDateTime(unixtime).ToString());
                                        data.Add("date_usec", "0");
                                        data.Add("user_id", _RegUtc.Match(line).Groups[2].ToString());
                                        data.Add("name", _RegUtc.Match(line).Groups[2].ToString());
                                        data.Add("content", _RegUtc.Match(line).Groups[3].ToString());
                                        break;
                                    case 1:
                                        var rtime = TimeSpan.Parse(_RegRelative.Match(line).Groups[1].ToString());
                                        data.Add("vpos", ((long)(rtime.TotalSeconds * 100D)).ToString());
                                        data.Add("date", "0");
                                        data.Add("date_usec", "0");
                                        //data.Add("ttt", _RegRelative.Match(line).Groups[1].ToString());
                                        data.Add("user_id", _RegRelative.Match(line).Groups[2].ToString());
                                        data.Add("name", _RegRelative.Match(line).Groups[2].ToString());
                                        data.Add("content", _RegRelative.Match(line).Groups[3].ToString());
                                        break;
                                }
                                if (data.Count() > 0)
                                {
                                    count++;
                                    var ttt = ConvChatData(data, _props).TrimEnd();
                                    if (!string.IsNullOrEmpty(ttt))
                                        sw.WriteLine(ttt);
                                    //else
                                    //_form.AddLog("deleted:" + line, 9);
                                }
                            }
                        }
                    }
                    _form.AddLog("コメント数: " + count, 1);
                    ConvComment.EndXmlDoc(sw);
                
                }
            }
            catch (Exception Ex)
            {
                DebugWrite.Writeln(nameof(TwitchConvertTwitchDownloaderText), Ex);
                return false;
            }
            return true;
        }

        public bool TwitchConvertTwitchDownloaderJson(string sfile, string dfile)
        {
            var enc = new System.Text.UTF8Encoding(false);
            long starttime = 0L;

            try
            {
                var json = JObject.Parse(File.ReadAllText(sfile));
                if (json["video"]["start"] != null)
                {
                    if (double.TryParse(json["video"]["start"].ToString(), out var dbl))
                        starttime = (long)(dbl * 100D);
                }
                var comments = (JArray)json["comments"];
                if (comments.Count() < 1)
                    return false;
                _form.AddLog("コメント数: " + comments.Count(), 1);
                using (var sw = new StreamWriter(dfile, true, enc))
                {
                    ConvComment.BeginXmlDoc(sw);
                    foreach (var item in comments)
                    {
                        //チャットの処理
                        var data = new Dictionary<string, string>();
                        data.Add("threadid", item["_id"].ToString());
                        var vpos = (long)((double)item["content_offset_seconds"] * 100D);
                        data.Add("vpos", (vpos - starttime).ToString());
                        var localtime = DateTime.Parse(item["created_at"].ToString());
                        data.Add("date", Utils.GetUnixTime(localtime).ToString());
                        data.Add("date_usec", "0");
                        if (!string.IsNullOrEmpty(item["message"]["user_color"].ToString()))
                            data.Add("color", item["message"]["user_color"].ToString());
                        data.Add("user_id", item["commenter"]["name"].ToString());
                        data.Add("name", item["commenter"]["display_name"].ToString());
                        if (!_props.IsTwiCommType)
                            data.Add("content", item["message"]["body"].ToString());
                        else
                        {
                            var ttt = "";
                            var isText = false;
                            foreach (var ddd in item["message"]["fragments"])
                            {
                                if (ddd["emoticon"].Count() <= 0)
                                {
                                    ttt += ddd["text"].ToString();
                                    isText = true;
                                }
                            }
                            if (isText)
                                data.Add("content", ttt);
                            else
                                continue;
                            if (data["content"].Contains(" gifted ") ||
                            data["content"].Contains(" gifting "))
                            {
                                if (_props.IsTwiGift)
                                    data.Add("mail", "shita");
                                else
                                    continue;
                            }
                            if (data["content"].Contains(" subscribed ") ||
                                data["content"].Contains(" is now live!"))
                            {
                                if (_props.IsTwiSystem)
                                    data.Add("mail", "shita");
                                else
                                    continue;
                            }
                        }
                        if (data.Count() > 0)
                        {
                            var ttt = ConvChatData(data, _props).TrimEnd();
                            if (!string.IsNullOrEmpty(ttt))
                                sw.WriteLine(ttt);
                            //else
                            //_form.AddLog("deleted:" + line, 9);
                        }
                    }
                    ConvComment.EndXmlDoc(sw);
                }
                json = null;
            }
            catch (Exception Ex)
            {
                DebugWrite.Writeln(nameof(TwitchConvertTwitchDownloaderJson), Ex);
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
