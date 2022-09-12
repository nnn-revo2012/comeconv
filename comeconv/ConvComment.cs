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

    public class ConvComment : IDisposable
    {

        #region IDisposable Support
        private bool disposedValue = false; // 重複する呼び出しを検出するには

        private Form1 _form = null;
        private Props _props = null;
        private static Regex _RegGift = new Regex("\"([^\"]+)\" (\\d+) \"([^\"]*)\" \"([^\"]+)\" ?(\\d*)", RegexOptions.Compiled);
        private static Regex _RegInfo = new Regex("(\\d+) (.+)$", RegexOptions.Compiled);

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
                    bool isfirst = true;
                    bool needpacket = false; 
                    string line;
                    while (!sr.EndOfStream) // ファイルが最後になるまで順に読み込み
                    {
                        line = sr.ReadLine();
                        if (isfirst)
                        {
                            if (line.TrimStart().StartsWith("<chat ") ||
                                line.TrimStart().StartsWith("<thread "))
                            {
                                BeginXmlDoc(sw);
                                needpacket = true;
                            }
                            isfirst = false;
                        }
                        if (line.TrimStart().StartsWith("<chat "))
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
                        else if (line.TrimStart().StartsWith("<thread "))
                        {
                            sw.WriteLine(line);
                        }
                        else
                        {
                            sw.WriteLine(line);
                        }
                    }
                    if (needpacket)
                    {
                        EndXmlDoc(sw);
                    }
                }
            }
            catch (Exception Ex)
            {
                DebugWrite.Writeln(nameof(SacXmlConvert), Ex);
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
                var ttt = WebUtility.HtmlDecode(xdoc.Element("chat").Value.ToString());
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
                if (data.ContainsKey("premium") &&
                    (data["premium"] == "2" || data["premium"] == "3"))
                {
                    //Giftの処理
                    // /gift ajisairandom 999999 "＊＊＊＊" 300 "" "ランダムあじさい" 1
                    if (ttt.StartsWith("/gift "))
                    {
                        if (props.IsSacGift)
                        {
                            if (props.SacConvApp < 1 || props.SacConvApp > 2)
                            {
                                ttt = _RegGift.Match(ttt).Groups[1].Value + "さん:"
                                    + _RegGift.Match(ttt).Groups[4].Value
                                    + "(+" + _RegGift.Match(ttt).Groups[2].Value + ")";
                                data["mail"] = "184 white shita";
                                data["premium"] = "1";
                            }
                        }
                        else
                        {
                            del_flg = true;
                        }
                    }
                    else if (ttt.StartsWith("/emotion "))
                    {
                        if (props.IsSacEmotion)
                        {
                            if (props.SacConvApp < 1 || props.SacConvApp > 2)
                            {
                                ttt = ttt.Substring(9);
                                if (props.IsSacEmoji)
                                {
                                    if (ttt.Contains("🍀"))
                                        ttt = ttt.Replace("🍀", "クローバー");
                                    if (ttt.Contains("🌻"))
                                        ttt = ttt.Replace("🌻", "ひまわり");
                                    if (ttt.Contains("🌸"))
                                        ttt = ttt.Replace("🌸", "さくら");
                                }
                                data["mail"] = "184 white shita";
                                data["premium"] = "1";
                            }
                        }
                        else
                        {
                            del_flg = true;
                        }
                    }
                    else if (ttt.StartsWith("/nicoad "))
                    {
                        if (props.IsSacNicoAd)
                        {
                            if (props.SacConvApp < 1 || props.SacConvApp > 2)
                            {
                                var jo = JObject.Parse(ttt.Substring(8));
                                if (jo["message"] != null)
                                {
                                    ttt = jo["message"].ToString();
                                    data["mail"] = "184 white shita small";
                                    data["premium"] = "1";
                                }
                            }
                        }
                        else
                        {
                            del_flg = true;
                        }
                    }
                    else if (ttt.StartsWith("/cruise "))
                    {
                        if (props.IsSacCruise)
                        {
                            if (props.SacConvApp < 1 || props.SacConvApp > 2)
                            {
                                ttt = ttt.Substring(8).Trim('"');
                                if (!data.ContainsKey("mail"))
                                    data["mail"] = "white shita small";
                                else
                                    data["mail"] = data["mail"] + " white shita small";
                                data["premium"] = "1";
                            }
                        }
                        else
                        {
                            del_flg = true;
                        }
                    }
                    else if (ttt.StartsWith("/quote "))
                    {
                        if (props.IsSacCruise)
                        {
                            if (props.SacConvApp < 1 || props.SacConvApp > 2)
                            {
                                ttt = ttt.Substring(7).Trim('"');
                                if (!data.ContainsKey("mail"))
                                    data["mail"] = "white shita small";
                                else
                                    data["mail"] = data["mail"] + " white shita small";
                                data["premium"] = "1";
                            }
                        }
                        else
                        {
                            del_flg = true;
                        }
                    }
                    else if (ttt.StartsWith("/info "))
                    {
                        if (props.IsSacInfo)
                        {
                            if (props.SacConvApp < 1 || props.SacConvApp > 2)
                            {
                                ttt = _RegInfo.Match(ttt.Substring(6)).Groups[2].Value;
                                if (!data.ContainsKey("mail"))
                                    data["mail"] = "white shita small";
                                else
                                    data["mail"] = data["mail"] + " white shita small";
                                data["premium"] = "1";
                            }
                        }
                        else
                        {
                            del_flg = true;
                        }
                    }
                    else if (ttt.StartsWith("/spi "))
                    {
                        if (props.IsSacInfo)
                        {
                            if (props.SacConvApp < 1 || props.SacConvApp > 2)
                            {
                                ttt = ttt.Substring(5).Trim('"');
                                if (!data.ContainsKey("mail"))
                                    data["mail"] = "white shita small";
                                else
                                    data["mail"] = data["mail"] + " white shita small";
                                data["premium"] = "1";
                            }
                        }
                        else
                        {
                            del_flg = true;
                        }
                    }
                    else if (ttt.StartsWith("/vote ") && props.IsSimpleVote)
                    {
                        switch (SimpleVote.IsVote(ttt))
                        {
                            case "stop":
                                del_flg = true;
                                break;
                            case "error":
                                break;
                            default:
                                if (SimpleVote.SetVote(ttt))
                                {
                                    ttt = SimpleVote.ShowVote();
                                }
                                break;
                        }
                    }
                    if (ttt.StartsWith("/"))
                    {
                        if (!props.IsSacSystem)
                        {
                            del_flg = true;
                        }
                    }
                    // 運営コマンドはすべてpremium="1"
                    if (props.SacConvApp > 2 
                        && (data["premium"] == "2" || data["premium"] == "3") 
                        && del_flg != true)
                    {
                        if (!data.ContainsKey("mail"))
                            data["mail"] = "white ue";
                        else
                            data["mail"] = data["mail"] + " white ue";
                        data["premium"] = "1";
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
                }

                // NicoConvAssの場合mail=のsmall,bigを削除
                if (props.SacConvApp ==3 && data.ContainsKey("mail"))
                {
                    data["mail"] = data["mail"].Replace("small", "").Replace("big", "");
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

                //SacNGWordsの処理
                foreach (var ngword in props.SacNGWords)
                {
                    if (ttt.IndexOf(ngword) > -1)
                    {
                        del_flg = true;
                        break;
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

        public static void BeginXmlDoc(StreamWriter sw)
        {
            sw.Write("<?xml version='1.0' encoding='UTF-8'?>\r\n");
            sw.Write("<packet>\r\n");
        }

        public static void EndXmlDoc(StreamWriter sw)
        {
            sw.Write("</packet>\r\n");
        }

        public static string Table2Xml(IDictionary<string, string> data)
        {
            var result = string.Empty;
            var content = string.Empty;
            if (data.Count <= 0)
                return result;
            else
                result = "<chat";

            try
            {
                string value;
                foreach (var it in data)
                {
                    value = it.Value.ToString();
                    switch (it.Key.ToString())
                    {
                        case "thread":
                            result += " " + it.Key.ToString() + @"=""" + value + @"""";
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
                                result += " " + it.Key.ToString() + @"=""" + Utils.Encode(value) + @"""";
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
                            content = Utils.Encode(value);
                            break;
                        default:
                            result += " " + it.Key.ToString() + @"=""" + Utils.Encode(value) + @"""";
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
