using System;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;

using comeconv.Prop;
using comeconv.Proc;
using comeconv.Util;

namespace comeconv
{
    public partial class Form1 : Form
    {
        public static Props props;                   //設定

        //0処理待ち 1録画準備 2録画中 3再接続 4中断 5変換処理中 9終了
        private static int ProgramStatus { get; set; } //プログラム状態

        //dispose するもの
        //private NicoDb _ndb = null;                   //NicoDb

        private ExecPsInfo epi = null;                //実行／保存ファイル情報

        private readonly object lockObject = new object();  //情報表示用
        private readonly object lockObject2 = new object(); //実行ファイルのログ用
        private string LogFile;
        private string LogFile2;
        private string LogFile3;

        public Form1(string[] args)
        {
            InitializeComponent();
            this.Text = Ver.GetFullVersion();
        }

        protected override void OnLoad(EventArgs e)
        {
            try
            {
                base.OnLoad(e);

                //設定データー読み込み
                props = new Props();
                props.LoadData();
                if (!Directory.Exists(Props.GetSettingDirectory()))
                    props.SaveData();
                var dfile = Path.Combine(Props.GetSettingDirectory(), props.SacNGLists);
                //NGWordファイルをコピー
                if (!File.Exists(dfile))
                {
                    var sfile = GetExecFile(props.SacNGLists);
                    File.Copy(sfile, dfile);
                }
                SetForm();
            }
            catch (Exception Ex)
            {
                AddLog("初期処理エラー。\r\n" + Ex.Message, 2);
                return;
            }

        }


        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {

            //_ndb?.Dispose();

        }

        private async void tabPage1_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop, false);

            try
            {
                GetForm();
                var exec_file = props.ExecFile;
                exec_file = GetExecFile(exec_file);
                if (!File.Exists(exec_file))
                {
                    AddLog("FFmpeg.exe がありません。", 2);
                }

                LogFile = Props.GetLogfile("D:\\home\\tmp", "conv");

                //1ファイルずつ順次実行する
                for (int i = 0; i < files.Length; i++)
                {
                    await Task.Run(() => ConvXml(files[i]));
                }
            }
            catch (Exception Ex)
            {
                //if (_ndb != null)
                //{
                //    _ndb.Dispose();
                //}
                AddLog("ドラッグ＆ドロップできません。\r\n" + Ex.Message, 2);
            }

        }

        private void tabPage1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.All;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //OKボタンが押されたら設定を保存
            GetForm();
            var result = props.SaveData(); //設定ファイルに保存
            result = Form1.props.LoadData(); //親フォームの設定データを更新
        }

        private void textBox1_Validated(object sender, EventArgs e)
        {
            string ttt = textBox1.Text;
            if (int.TryParse(ttt, out var i))
                textBox1.Text = ttt;
            else
                textBox1.Text = Properties.Settings.Default.SacCommLen.ToString();
        }

        private void textBox2_Validated(object sender, EventArgs e)
        {
            // 15 15.4 15.454445
            string ttt = textBox2.Text;
            if (!ttt.Contains("."))
                ttt += ".0";
            ttt += "00";
            // 15.000 15.400 15.4565455000
            ttt = ttt.Substring(0, ttt.IndexOf('.') + 3);
            if (float.TryParse(ttt, out var f))
                textBox2.Text = ttt;
            else
                textBox2.Text = Properties.Settings.Default.SacVpos.ToString("0.00");
        }

        private void tabPage1_Click(object sender, EventArgs e)
        {

        }
    }
}