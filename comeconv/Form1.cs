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
                //if (!File.Exists(exec_file))
                //{
                //    AddLog("実行ファイルがありません。", 2);
                //    return;
                //}

                //LogFile = Props.GetLogfile(save_dir, "conv");
                //LogFile2 = Props.GetExecLogfile(save_dir, "conv");
                LogFile = null;
                LogFile2 = null;
                LogFile3 = null;

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
    }
}