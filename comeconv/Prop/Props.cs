using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Configuration;
using System.IO;

namespace comeconv.Prop
{

    public class Props
    {
        //定数設定
        public static readonly string[][] ReplaceWords =
            {
            new[] {"?PID?","lv1234567","ProgramId。lv1234567のような文字列。"},
            new[] {"?UNAME?","ユーザ名","ユーザ名。公式の場合、official"},
            new[] {"?UID?","ユーザID","ユーザID。nicovideo.jp/user/に続く数字の列。公式の場合、official"},
            new[] {"?CNAME?","コミュニティ名","コミュニティ名。公式の場合、official"},
            new[] {"?CID?","コミュニティID","コミュニティID。co1234のような文字列。公式の場合、official"},
            new[] {"?TITLE?","放送タイトル","放送タイトル。"},
            new[] {"?YEAR?","2019","年4桁(開演時刻)"},
            new[] {"?MONTH?","09","月2桁(開演時刻)"},
            new[] {"?DAY?","01","日2桁(開演時刻)"},
            new[] {"?DAY8?","20190901","年4桁,月2桁,日2桁"},
            new[] {"?DAY6?","190901","年2桁,月2桁,日2桁"},
            new[] {"?HOUR?","18","時2桁"},
            new[] {"?MINUTE?","31","分2桁"},
            new[] {"?SECOND?","02","秒2桁"},
            new[] {"?TIME6?","183102","時2桁,分2桁,秒2桁"},
            new[] {"?TIME4?","3102","時2桁,分2桁"}
            //new[] {"", "",""}
            };

        public static readonly IDictionary<string, string> PropLists =
            new Dictionary<string, string>()
        {
            // "community"
            {"comId", "community.id"}, // "co\d+"
            // "program"
            {"beginTime", "program.beginTime"}, // integer
            {"description", "program.description"}, // 放送説明
            {"endTime", "program.endTime"}, // integer
            {"isFollowerOnly", "program.isFollowerOnly"}, // bool
            {"isPrivate", "program.isPrivate"}, // bool
            {"mediaServerType","program.mediaServerType"}, // "DMC"
            {"nicoliveProgramId", "program.nicoliveProgramId"}, // "lv\d+"
            {"openTime", "program.openTime"}, // integer
            {"providerType", "program.providerType"}, // "community"
            {"status", "program.status"}, //
            {"userName", "program.supplier.name"}, // ユーザ名
            {"userPageUrl", "program.supplier.pageUrl"}, // "https://www.nicovideo.jp/user/\d+"
            {"title", "program.title"}, // title
            {"vposBaseTime", "program.vposBaseTime"}, // integer
            // "site"
            {"serverTime", "site.serverTime"}, // integer
            // "socialGroup"
            {"socDescription", "socialGroup.description"}, // コミュ説明
            {"socId", "socialGroup.id"}, // "co\d+" or "ch\d+"
            {"socLevel", "socialGroup.level"}, // integer
            {"socName", "socialGroup.name"}, // community name
            {"socType", "socialGroup.type"}, // "community"
            // "user"
            {"accountType", "user.accountType"}, // "premium"
            {"isLoggedIn", "user.isLoggedIn"}, // bool
        };

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
        public string SacNGLists { get; set; }
        public string SacVideoMode { get; set; }
        public bool IsLogging { get; set; }

        public IDictionary<string, string> SacVideoList;
        public IList<string> SacNGWords;

        public Props()
        {
            SacVideoList = new Dictionary<string, string>();
            SacNGWords = new List<string>();
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
                this.SacNGLists = Properties.Settings.Default.SacNGLists;
                this.SacVideoMode = Properties.Settings.Default.SacVideoMode;
                this.IsLogging = Properties.Settings.Default.IsLogging;
                //TEST compoboxの内容をDicに読み込む
                SacVideoList["mp4"] = "aaaaaaa";
                SacVideoList["flv"] = "bbbbbbb";
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
                Properties.Settings.Default.SacNGLists = this.SacNGLists;
                Properties.Settings.Default.SacVideoMode = this.SacVideoMode;
                Properties.Settings.Default.IsLogging = this.IsLogging;
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

        public static string GetProviderType(string type)
        {
            var result = "？？";
            switch (type)
            {
                case "community":
                    result = "コミュニティ";
                    break;
                case "user":
                    result = "コミュニティ";
                    break;
                case "channel":
                    result = "チャンネル";
                    break;
                case "official":
                    result = "公式生放送";
                    break;
                case "cas":
                    result = "実験放送";
                    break;
                default:
                    break;
            }
            return result;
        }

        public static long GetUnixTime() {
            return (long)(((TimeSpan)(DateTime.Now - new DateTime(1970, 1, 1))).TotalSeconds);
        }

        public static DateTime GetUnixToDateTime(long unix) {
            DateTime UNIX_EPOCH = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            return UNIX_EPOCH.AddSeconds(unix).ToLocalTime();
        }

        public static long GetLongParse(string ttt)
        {
            double dd = -1.0D;
            double.TryParse(ttt, out dd);
            return (long )dd;

        }

    }
}
