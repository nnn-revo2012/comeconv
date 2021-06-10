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

namespace comeconv.Util
{
    public class Utils
    {

        //サロペートペア＆結合文字 検出＆文字除去
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


        //ファイル名の後ろに日付時間を付加する
        public static string GetLogfile(string dir, string filename)
        {
            var tmp = Path.GetFileNameWithoutExtension(filename)
                + "_" + System.DateTime.Now.ToString("yyMMdd_HHmmss")
                + Path.GetExtension(filename);
            return Path.Combine(dir, tmp);
        }

        //保存ファイルにシーケンスNoをつける
        public static string GetBackupFileName(string filename, string ext)
        {
            var dir = Path.GetDirectoryName(filename);
            var backup = Path.Combine(dir, Path.GetFileNameWithoutExtension(filename) + ext);
            if (!File.Exists(backup)) return backup;

            var ii = 1;
            //同名ファイル名がないかチェック
            while (IsExistFile(backup, ii)) ++ii;

            return Path.Combine(dir, Path.GetFileNameWithoutExtension(backup) + "(" + ii.ToString() + ")" + ext);

        }

        //同名ファイル名がないかチェック
        public static bool IsExistFile(string file, int seq)
        {
            var dir = Path.GetDirectoryName(file);
            var fn = Path.Combine(dir, Path.GetFileNameWithoutExtension(file) + "(" + seq.ToString() + ")" + Path.GetExtension(file));

            return !File.Exists(fn) ? false : true;
        }

    }
}
