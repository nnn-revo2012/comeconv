using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Linq;
using System.Diagnostics;
using System.Web;
using System.Globalization;
using System.Text.RegularExpressions;


namespace comeconv.Util
{
    public class Utils
    {

        //サロゲートペア＆結合文字 検出＆文字除去
        //\ud83d\ude0a
        //か\u3099
        public static string DelEmoji(string s, string t = "")
        {
            if (!IsSurrogatePair(s)) return s;

            StringBuilder sb = new StringBuilder();
            TextElementEnumerator tee = StringInfo.GetTextElementEnumerator(s);

            tee.Reset();
            while (tee.MoveNext())
            {
                string te = tee.GetTextElement();
                if (Char.IsSurrogate(te[0]))
                    sb.Append(t); //サロゲートペアまたは結合文字の場合
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

        //ファイル名の後ろに日付時間を付加する
        public static string GetLogfile(string dir, string filename)
        {
            var tmp = Path.GetFileNameWithoutExtension(filename)
                + "_" + System.DateTime.Now.ToString("yyMMdd_HHmmss")
                + Path.GetExtension(filename);
            return Path.Combine(dir, tmp);
        }

        private static readonly DateTime UNIX_EPOCH = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        public static long GetUnixTime(DateTime localtime)
        {
            DateTime utc = localtime.ToUniversalTime();
            return (long)(((TimeSpan)(utc - UNIX_EPOCH)).TotalSeconds);
        }

        public static DateTime GetUnixToDateTime(long unix)
        {
            return UNIX_EPOCH.AddSeconds(unix).ToLocalTime();
        }

        //保存ファイルにシーケンスNoをつける (file名.xml(n).org の形式)
        public static string GetBackupFileName(string filename, string ext)
        {
            var dir = Path.GetDirectoryName(filename);
            var ext2 = Path.GetExtension(filename); //元の拡張子
            var backup = Path.Combine(dir, Path.GetFileNameWithoutExtension(filename) + ext2 + ext);
            if (!File.Exists(backup)) return backup;

            var ii = 1;
            //同名ファイル名がないかチェック
            while (IsExistFile(backup, ext2 + ext, ii)) ++ii;

            return Path.Combine(dir, Path.GetFileNameWithoutExtension(backup) + "(" + ii.ToString() + ")" + ext2 + ext);
        }

        //保存ファイルにシーケンスNoをつける (file名.json -> file名.xml(n) の形式)
        public static string GetNewFileName(string filename, string ext)
        {
            var dir = Path.GetDirectoryName(filename);
            var ext2 = Path.GetExtension(filename); //元の拡張子
            var newfile = Path.Combine(dir, Path.GetFileNameWithoutExtension(filename) + ext);
            if (!File.Exists(newfile)) return newfile;

            var ii = 1;
            //同名ファイル名がないかチェック
            while (IsExistFile(newfile, ext, ii)) ++ii;

            return Path.Combine(dir, Path.GetFileNameWithoutExtension(newfile) + "(" + ii.ToString() + ")" + ext);
        }

        //同名ファイル名がないかチェック
        public static bool IsExistFile(string filename, string ext, int seq)
        {
            var fn = Path.Combine(Path.GetDirectoryName(filename), Path.GetFileNameWithoutExtension(filename) + "(" + seq.ToString() + ")" +ext);

            return !File.Exists(fn) ? false : true;
        }

        //ファイルの種類を返す
        public static int IsFileType(string filename)
        {
            var result = -1;

            var ext = Path.GetExtension(filename);
            if (ext == ".xml" || ext == ".json" || ext == ".jsonl" || ext == ".txt")
                result = 0;
            else if (ext == ".ts" || ext == ".flv" || ext == ".mp4")
                result = 1;

            return result;
        }

        public static readonly Regex RgxCDJsonl = new Regex("^\\{(\'|\").*(\'|\")\\: ", RegexOptions.Compiled | RegexOptions.Singleline);
        private static readonly Regex RgxCDJson = new Regex("^\\[\n +\\{\n", RegexOptions.Compiled | RegexOptions.Singleline);
        private static char[] _read_buf = new char[256];
        //Twitchのコメントファイルの種類を返す
        public static int IsTwitchFileType(string filename)
        {
            var enc = new System.Text.UTF8Encoding(false);
            var result = -1;

            using (var sr = new StreamReader(filename, enc))
            {
                var len = sr.ReadBlock(_read_buf, 0, 32);
                if (len > 0)
                {
                    var str = new string(_read_buf);
                    if (RgxCDJsonl.IsMatch(str))
                        result = 0;
                    else if (RgxCDJson.IsMatch(str))
                        result = 1;
                    else if (str.StartsWith("{\"streamer\":{\"name\":"))
                        result = 5;
                    else
                        result = 10;
                }
            }

            return result;
        }

        //Xmlファイルが壊れてないか調べる
        public static bool CanXmlRead(string file)
        {
            try
            {
                if (!File.Exists(file))
                {
                    return false;
                }
                XDocument xdoc = XDocument.Load(file);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        //ニコニコのXmlファイルの形式を返す
        public static int IsXmlFileType(string filename)
        {
            var enc = new System.Text.UTF8Encoding(false);
            var result = -1;

            using (var sr = new StreamReader(filename, enc))
            {
                var len = sr.ReadBlock(_read_buf, 0, 256);
                if (len > 0)
                {
                    var str = new string(_read_buf);
                    if (str.IndexOf("user_name=") > -1)
                    {
                        //10 ファイル修復機能で修正してない 11 修正済
                        result = str.IndexOf("youtube_icon_url=") > -1 ? 10 : 11;
                    }
                    else
                        result = 0;
                }
            }

            return result;
        }

        //特殊文字をエンコードする
        public static string Encode(string s)
        {
            if (string.IsNullOrEmpty(s)) return null;
            s = s.Replace("&", "&amp;");
            s = s.Replace("<", "&lt;");
            s = s.Replace(">", "&gt;");
            s = s.Replace("\"", "&quot;");

            return s;
        }
        //特殊文字をデコードする
        public static string Decode(string s)
        {
            if (string.IsNullOrEmpty(s)) return null;
            s = s.Replace("&lt;", "<");
            s = s.Replace("&gt;", ">");
            s = s.Replace("&quot;", "\"");
            s = s.Replace("&amp;", "&");

            return s;
        }
    }
}
