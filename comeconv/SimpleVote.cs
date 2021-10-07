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

    public static class SimpleVote
    {
        private static readonly Regex _RegVote = new Regex("^\\/vote +(\\w+) +(.*)$", RegexOptions.Compiled);
        private static string status { get; set; }
        private static int count { get; set; }
        private static string[] vote { get; set; }
        private static string[] vres { get; set; }

        private static void InitVote()
        {
            status = "";
            count = 0;
        }

        public static string IsVote(string data)
        {
            if (!data.StartsWith("/vote "))
                return "error";
            return _RegVote.Match(data).Groups[1].Value;
        }

        private static string[] SplitVote(string data)
        {
            var li = new List<string>();

            try
            {
                string[] ddd = data.Trim().Split(' ');
                for (var i = 0; i < ddd.Length; i++)
                {
                    if (string.IsNullOrEmpty(ddd[i]))
                        continue;
                    if (ddd[i].StartsWith("\""))
                    {
                        StringBuilder sb = new StringBuilder(ddd[i].TrimStart('"'));
                        while (++i < ddd.Length)
                        {
                            sb.Append(" ");
                            if (!string.IsNullOrEmpty(ddd[i]) &&
                                ddd[i].EndsWith("\""))
                            {
                                sb.Append(ddd[i].TrimEnd('"'));
                                break;
                            }
                            else
                            {
                                sb.Append(ddd[i]);
                            }
                        }
                        li.Add(sb.ToString());
                    }
                    else
                    {
                        li.Add(ddd[i]);
                    }
                }

                return li.ToArray();
            }
            catch (Exception Ex)
            {
                DebugWrite.Writeln(nameof(SplitVote), Ex);
                return li.ToArray();
            }
        }

        public static bool SetVote(string data)
        {
            string isvote = IsVote(data);
            if (isvote == "error")
                return false;
            // voteコマンドの何か調べる
            // start ならInitしてデーターをセット
            if (isvote == "start")
            {
                InitVote();
                status = isvote;
                var ddd = _RegVote.Match(data).Groups[2].Value;
                vote = SplitVote(ddd);
                count = vote.Count() - 1;
            }
            // showresult ならresultをセット
            else if (isvote == "showresult")
            {
                var ddd = _RegVote.Match(data).Groups[2].Value;
                vres = SplitVote(ddd);
                if (status != "start")
                {
                    InitVote();
                    count = vres.Count() - 1;
                    vote = new string[count + 1];
                    for (var i = 1; i < count; i++)
                        vote[i] = $"質問{i}";
                }
                status = isvote;
            }
            // stop なら？
            else if (isvote == "stop")
            {
                status = "";
                count = 0;
            }
            else
            {
                status = "";
                count = 0;
                return false;
            }
            return true;
        }
        public static string ShowVote()
        {
            string result = "";
            string[] data = new string[10];
            float f;

            try
            {
                if (status == "start")
                {
                    data[0] = "\r\nQ." + vote[0] + "\r\n";
                    for (var i = 1; i <= count; i++)
                    {
                        data[i] = $"{i}.{vote[i]}";
                    }
                }
                else if (status == "showresult")
                {
                    data[0] = "\r\n結果\r\n";
                    for (var i = 1; i <= count; i++)
                    {
                        if (float.TryParse(vres[i], out f))
                            f /= 10f;
                        else
                            f = 0.0f;
                        data[i] = $"{i}.{vote[i]}({f:0.0}%)";
                    }
                }
                else
                {
                    return result;
                }

                result = data[0] + data[1];
                if (count > 1)
                    result += "　" + data[2];
                if (count == 3 || count >= 5)
                    result += "　" + data[3];
                result += "\r\n";

                if (count == 4)
                    result += data[3] + "　" + data[4];
                if (count >= 5)
                    result += data[4] + "　" + data[5];
                if (count >= 6)
                    result += "　" + data[6];
                result += "\r\n";

                if (count >= 7)
                    result += data[7];
                if (count >= 8)
                    result += "　" + data[8];
                if (count >= 9)
                    result += "　" + data[9];
                result += "\r\n";

                return result;
            }
            catch (Exception Ex)
            {
                DebugWrite.Writeln(nameof(ShowVote), Ex);
                return result;
            }
        }

    }

}
