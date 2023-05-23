using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Configuration;
using System.IO;

namespace comeconv.Prop
{

    public class Props
    {
        public bool IsDebug { get; set; }

        public string ExecFile { get; set; }
        public string ExecCommand { get; set; }
        public string BreakCommand { get; set; }
        public bool IsSacEmoji { get; set; }
        public string SacEmojiMode { get; set; }
        public bool IsSacPremium { get; set; }
        public string SacPremiumMode { get; set; }
        public bool IsSacCommLen { get; set; }
        public int SacCommLen { get; set; }
        public bool IsSacVpos { get; set; }
        public long SacVpos { get; set; }
        public bool IsSacGift { get; set; }
        public bool IsSacEmotion { get; set; }
        public bool IsSacNicoAd { get; set; }
        public bool IsSacCruise { get; set; }
        public bool IsSacInfo { get; set; }
        public bool IsSacSystem { get; set; }

        public string SacNGLists { get; set; }
        public string SacVideoMode { get; set; }
        public int SacConvApp { get; set; }
        public bool IsLogging { get; set; }
        public bool IsSaveData { get; set; }
        public string DispTab { get; set; }
        public bool IsSimpleVote { get; set; }

        public bool IsTwiEmoji { get; set; }
        public string TwiEmojiMode { get; set; }
        public bool IsTwiCommLen { get; set; }
        public int TwiCommLen { get; set; }
        public bool IsTwiVpos { get; set; }
        public long TwiVpos { get; set; }
        public bool IsTwiCommType { get; set; }
        public bool IsTwiGift { get; set; }
        public bool IsTwiSystem { get; set; }


        public IDictionary<string, string> SacVideoList;
        public IDictionary<string, string> SacAppList;
        public IList<string> SacNGWords;
        public IDictionary<string, string> DispTabList;

        public Props()
        {
            SacVideoList = new Dictionary<string, string>();
            SacAppList = new Dictionary<string, string>();
            SacNGWords = new List<string>();
            DispTabList = new Dictionary<string, string>();
        }

        public bool LoadData()
        {
            try
            {
                this.ExecFile = Properties.Settings.Default.ExecFile;
                this.ExecCommand = Properties.Settings.Default.ExecCommand;
                this.BreakCommand = Properties.Settings.Default.BreakCommand;

                this.IsSacEmoji = Properties.Settings.Default.IsSacEmoji;
                this.SacEmojiMode = Properties.Settings.Default.SacEmojiMode;
                this.IsSacPremium = Properties.Settings.Default.IsSacPremium;
                this.SacPremiumMode = Properties.Settings.Default.SacPremiumMode;
                this.IsSacCommLen = Properties.Settings.Default.IsSacCommLen;
                this.SacCommLen = Properties.Settings.Default.SacCommLen;
                this.IsSacVpos = Properties.Settings.Default.IsSacVpos;
                this.SacVpos = Properties.Settings.Default.SacVpos;
                this.IsSacGift = Properties.Settings.Default.IsSacGift;
                this.IsSacEmotion = Properties.Settings.Default.IsSacEmotion;
                this.IsSacNicoAd = Properties.Settings.Default.IsSacNicoAd;
                this.IsSacCruise = Properties.Settings.Default.IsSacCruise;
                this.IsSacInfo = Properties.Settings.Default.IsSacInfo;
                this.IsSacSystem = Properties.Settings.Default.IsSacSystem;
                this.SacNGLists = Properties.Settings.Default.SacNGLists;
                this.SacVideoMode = Properties.Settings.Default.SacVideoMode;
                SacVideoList = ReadVideoList(Properties.Settings.Default.SacVideoList);
                this.SacConvApp = Properties.Settings.Default.SacConvApp;
                SacAppList = ReadVideoList(Properties.Settings.Default.SacAppList);
                this.IsLogging = Properties.Settings.Default.IsLogging;
                this.IsSaveData = Properties.Settings.Default.IsSaveData;
                this.DispTab = Properties.Settings.Default.DispTab;
                this.IsSimpleVote = Properties.Settings.Default.IsSimpleVote;

                this.IsTwiEmoji = Properties.Settings.Default.IsTwiEmoji;
                this.TwiEmojiMode = Properties.Settings.Default.TwiEmojiMode;
                this.IsTwiCommLen = Properties.Settings.Default.IsTwiCommLen;
                this.TwiCommLen = Properties.Settings.Default.TwiCommLen;
                this.IsTwiVpos = Properties.Settings.Default.IsTwiVpos;
                this.TwiVpos = Properties.Settings.Default.TwiVpos;
                this.IsTwiCommType = Properties.Settings.Default.IsTwiCommType;
                this.IsTwiGift = Properties.Settings.Default.IsTwiGift;
                this.IsTwiSystem = Properties.Settings.Default.IsTwiSystem;

            }
            catch (Exception Ex)
            {
                DebugWrite.Writeln(nameof(LoadData), Ex);
                return false;
            }
            return true;
        }

        public bool SaveData()
        {
            try
            {
                Properties.Settings.Default.ExecFile = this.ExecFile;
                Properties.Settings.Default.ExecCommand = this.ExecCommand;
                Properties.Settings.Default.BreakCommand = this.BreakCommand;

                Properties.Settings.Default.IsSacEmoji = this.IsSacEmoji;
                Properties.Settings.Default.SacEmojiMode = this.SacEmojiMode;
                Properties.Settings.Default.IsSacPremium = this.IsSacPremium;
                Properties.Settings.Default.SacPremiumMode = this.SacPremiumMode;
                Properties.Settings.Default.IsSacCommLen = this.IsSacCommLen;
                Properties.Settings.Default.SacCommLen = this.SacCommLen;
                Properties.Settings.Default.IsSacVpos = this.IsSacVpos;
                Properties.Settings.Default.SacVpos = this.SacVpos;
                Properties.Settings.Default.IsSacGift = this.IsSacGift;
                Properties.Settings.Default.IsSacEmotion = this.IsSacEmotion;
                Properties.Settings.Default.IsSacNicoAd = this.IsSacNicoAd;
                Properties.Settings.Default.IsSacCruise = this.IsSacCruise;
                Properties.Settings.Default.IsSacInfo = this.IsSacInfo;
                Properties.Settings.Default.IsSacSystem = this.IsSacSystem;
                Properties.Settings.Default.SacNGLists = this.SacNGLists;
                Properties.Settings.Default.SacVideoMode = this.SacVideoMode;
                Properties.Settings.Default.SacConvApp = this.SacConvApp;
                Properties.Settings.Default.IsLogging = this.IsLogging;
                Properties.Settings.Default.IsSaveData = this.IsSaveData;
                Properties.Settings.Default.DispTab = this.DispTab;
                Properties.Settings.Default.IsSimpleVote = this.IsSimpleVote;

                Properties.Settings.Default.IsTwiEmoji = this.IsTwiEmoji;
                Properties.Settings.Default.TwiEmojiMode = this.TwiEmojiMode;
                Properties.Settings.Default.IsTwiCommLen = this.IsTwiCommLen;
                Properties.Settings.Default.TwiCommLen = this.TwiCommLen;
                Properties.Settings.Default.IsTwiVpos = this.IsTwiVpos;
                Properties.Settings.Default.TwiVpos = this.TwiVpos;
                Properties.Settings.Default.IsTwiCommType = this.IsTwiCommType;
                Properties.Settings.Default.IsTwiGift = this.IsTwiGift;
                Properties.Settings.Default.IsTwiSystem = this.IsTwiSystem;

                Properties.Settings.Default.Save();
             }
            catch (Exception Ex)
            {
                DebugWrite.Writeln(nameof(SaveData), Ex);
                return false;
            }
            return true;
        }

        public bool ReloadData(string accountdbfile)
        {
            Properties.Settings.Default.Reload();
            return this.LoadData();
        }

        public bool ResetData(string accountdbfile)
        {
            var result = false;
            Properties.Settings.Default.Reset();
            result = this.LoadData();
            return result;
        }

        //動画変換リストを読み込み
        private Dictionary<string, string> ReadVideoList(string file)
        {
            var dic = new Dictionary<string, string>();

            try
            {
                foreach (var item in file.Split(';'))
                {
                    var ttt = item.Split('\t');
                    dic.Add(ttt[0], ttt[1]);
                }
            }
            catch (Exception Ex)
            {
                DebugWrite.Writeln(nameof(ReadVideoList), Ex);
                return dic;
            }
            return dic;
        }

        public string SplitVideoItem(string item)
        {
            return item.Split('(')[0];
        }

        //NGLists.txtを読み込み
        public List<string> ReadNGList(string r_file)
        {
            var enc = new System.Text.UTF8Encoding(false);
            var lists = new List<string>();

            try
            {
                using (var sr = new StreamReader(r_file, enc))
                {
                    string line;
                    while (!sr.EndOfStream) // 1行ずつ読み出し
                    {
                        line = sr.ReadLine();

                        if (string.IsNullOrEmpty(line) || line.StartsWith("//") ||
                            line.StartsWith("/info") || line.StartsWith("/spi"))
                            continue;
                        else
                            lists.Add(line);
                    }
                }
            }
            catch (Exception Ex)
            {
                DebugWrite.Writeln(nameof(ReadNGList), Ex);
                return lists;
            }
            return lists;
        }

        //設定ファイルの場所をGet
        public static string GetSettingDirectory()
        {
            //設定ファイルの場所
            var config = ConfigurationManager.OpenExeConfiguration(
                ConfigurationUserLevel.PerUserRoamingAndLocal);
            return Path.GetDirectoryName(config.FilePath);
        }

        //アプリケーションの場所をGet
        public static string GetApplicationDirectory()
        {
            //アプリケーションの場所
            var tmp = System.Reflection.Assembly.GetExecutingAssembly().Location;
            return Path.GetDirectoryName(tmp);
        }

        //ログファイル名をGet
        public static string GetLogfile(string dir, string filename)
        {
            var tmp = Path.GetFileNameWithoutExtension(filename) + "_" + System.DateTime.Now.ToString("yyMMdd_HHmmss") + ".log";
            return Path.Combine(dir, tmp);
        }

        //実行ログファイル名をGet
        public static string GetExecLogfile(string dir, string filename)
        {
            var tmp = Path.GetFileNameWithoutExtension(filename) + "_exec_" + System.DateTime.Now.ToString("yyMMdd_HHmmss") + ".log";
            return Path.Combine(dir, tmp);
        }

        public static string GetDirSepString()
        {
            return Path.DirectorySeparatorChar.ToString();
        }

    }
}
