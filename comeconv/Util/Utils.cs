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
            var ext2 = Path.GetExtension(filename);
            var backup = Path.Combine(dir, Path.GetFileNameWithoutExtension(filename) + ext2 + ext);
            if (!File.Exists(backup)) return backup;

            var ii = 1;
            //同名ファイル名がないかチェック
            while (IsExistFile(backup, ext2+ext, ii)) ++ii;

            return Path.Combine(dir, Path.GetFileNameWithoutExtension(backup) + "(" + ii.ToString() + ")" + ext2 + ext);
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
            if (ext == ".xml" || ext == ".json")
                result = 0;
            else if (ext == ".ts" || ext == ".flv" || ext == ".mp4")
                result = 1;

            return result;
        }
    }
}
