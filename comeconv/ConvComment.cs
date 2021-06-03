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

    public class ConvComment : IDisposable
    {

        #region IDisposable Support
        private bool disposedValue = false; // 重複する呼び出しを検出するには

        private Form1 _form = null;
        private Props _props = null;
        private Regex _RegGift = new Regex("\"([^\"]+)\" (\\d+) \"([^\"]*)\" \"([^\"]+)\" ?(\\d*)", RegexOptions.Compiled);

        //Debug
        public bool IsDebug { get; set; }

        public ConvComment(Form1 fo, Props props)
        {
            IsDebug = false;

            this._form = fo;
            this._props = props;
        }

        ~ConvComment()
        {
            this.Dispose();
        }

        //XML形式のコメントファイルを読み込み変換する
        public bool SacXmlConvert(string sfile, string dfile)
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
                                var ttt = ConvChatData(line, _props).TrimEnd();
                                if (!string.IsNullOrEmpty(ttt))
                                    sw.WriteLine(ttt);
                                //else
                                //_form.AddLog("deleted:" + line, 9);
                            }
                        }
                        else if (line.StartsWith("<thread "))
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
                DebugWrite.Writeln(nameof(Table2Xml), Ex);
                return false;
            }
            return true;

        }

        private string ConvChatData(string chat, Props props)
        {
            var del_flg = false;
            var data = new Dictionary<string, string>();
            var xdoc = XDocument.Parse(chat);

            try
            {
                foreach (var ele in xdoc.Element("chat").Attributes())
                {
                    if (ele.Name.ToString() == "premium")
                    {
                        if (props.IsSacPremium)
                        {
                            if (ele.Value.ToString() == "9" ||
                                ele.Value.ToString() == "25")
                                ele.Value = "1";
                            else if (ele.Value.ToString() == "8" ||
                                ele.Value.ToString() == "24")
                            {
                                if (props.SacPremiumMode == "pdel")
                                    del_flg = true;
                                continue;
                            }
                        }
                    }
                    data[ele.Name.ToString()] = ele.Value.ToString();
                }
                var ttt = xdoc.Element("chat").Value.ToString();
                if (!data.ContainsKey("vpos"))
                    data["vpos"] = "0";
                if (!data.ContainsKey("date_usec"))
                    data["date_usec"] = "0";
                //vpos先
                if (props.IsSacVpos)
                {
                    if (long.TryParse(data["vpos"], out var ll))
                        data["vpos"] = (ll + props.SacVpos).ToString();
                }
                //SacNGWordsの処理
                if (data.ContainsKey("premium") &&
                    (data["premium"] == "2" || data["premium"] == "3"))
                {
                    //Giftの処理
                    // /gift ajisairandom 999999 "＊＊＊＊" 300 "" "ランダムあじさい" 1
                    if (ttt.StartsWith("/gift "))
                    {
                        if (props.IsSacGift)
                            del_flg = true;
                        else
                        {
                            var gift = HttpUtility.HtmlDecode(ttt);
                            ttt = _RegGift.Match(gift).Groups[1] + "さん:"
                                + _RegGift.Match(gift).Groups[4]
                                + "(+" + _RegGift.Match(gift).Groups[2] + ")";
                            data["mail"] = "184 white shita medium";
                            data["premium"] = "1";
                        }
                    }
                    if (ttt.StartsWith("/emotion "))
                    {
                        if (props.IsSacEmotion)
                            del_flg = true;
                        else
                        {
                            ttt = HttpUtility.HtmlDecode(ttt).Substring(9);
                            if (ttt.Contains("🍀"))
                                ttt = ttt.Replace("🍀", "クローバー");
                            data["mail"] = data["mail"] + " white shita medium";
                            data["premium"] = "1";
                        }
                    }
                    if (ttt.StartsWith("/nicoad "))
                    {
                        if (props.IsSacNicoAd)
                            del_flg = true;
                        else
                        {
                            var jo = JObject.Parse(HttpUtility.HtmlDecode(ttt).Substring(8));
                            if (jo["message"] != null)
                            {
                                ttt = jo["message"].ToString();
                                data["mail"] = data["mail"] + " white shita small";
                                data["premium"] = "1";
                            }
                        }
                    }

                    foreach (var ngword in props.SacNGWords)
                    {
                        if (ttt.IndexOf(ngword) > -1)
                        {
                            del_flg = true;
                            break;
                        }
                    }
                }
                else
                {
                    //コメント長
                    if (props.IsSacCommLen)
                    {
                        if (ttt.Length > props.SacCommLen)
                            del_flg = true;
                    }
                    //絵文字処理
                    if (props.IsSacEmoji)
                    {
                        if (Utils.IsSurrogatePair(ttt))
                        {
                            if (props.SacEmojiMode == "edel")
                                del_flg = true;
                            else
                                ttt = Utils.DelEmoji(ttt, "　");
                        }
                    }
                }
                data["content"] = ttt;
                if (del_flg)
                    return "";
                else
                    return Table2Xml(data);
            }
            catch (Exception Ex)
            {
                DebugWrite.Writeln(nameof(ConvChatData), Ex);
                return "";
            }
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