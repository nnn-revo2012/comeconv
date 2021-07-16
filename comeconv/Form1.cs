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

        //0処理待ち 1処理中 2中断
        private static int ProgramStatus { get; set; } //プログラム状態

        //dispose するもの
        //private NicoDb _ndb = null;                   //NicoDb

        private ExecPsInfo epi = null;                //実行／保存ファイル情報

        private readonly object lockObject = new object();  //情報表示用
        private string LogFile;
        //private string LogFile2;
        //private string LogFile3;

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
                tabControl1.SelectedTab = tabControl1.TabPages[props.DispTab];
                ProgramStatus = 0;
            }
            catch (Exception Ex)
            {
                AddLog("初期処理エラー。\r\n" + Ex.Message, 2);
                return;
            }

        }


        private async void tabPage1_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop, false);

            try
            {
                if (ProgramStatus == 1) return;

                GetForm();
                ClearLog();

                var exec_file = props.ExecFile;
                if (!File.Exists(exec_file))
                {
                    exec_file = GetExecFile(exec_file);
                    if (!File.Exists(exec_file))
                        AddLog("FFmpeg.exe がありません。", 2);
                }
                props.ExecFile = exec_file;
                LogFile = null;

                ProgramStatus = 1;
                //1ファイルずつ順次実行する
                for (int i = 0; i < files.Length; i++)
                {
                    var filetype = Utils.IsFileType(files[i]);
                    if (filetype == 0)
                        await Task.Run(() => ConvXml(files[i]));
                    else if (filetype == 1)
                        await Task.Run(() => ConvVideo(files[i]));
                }
                ProgramStatus = 0;

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
                e.Effect = DragDropEffects.All;
            else
                e.Effect = DragDropEffects.None;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                //OKボタンが押されたら設定を保存
                GetForm();
                var result = props.SaveData(); //設定ファイルに保存
                result = Form1.props.LoadData(); //親フォームの設定データを更新
            }
            catch (Exception Ex)
            {
                DebugWrite.Writeln(nameof(button1_Click), Ex);
            }
        }

        private void textBox1_Validated(object sender, EventArgs e)
        {
            try
            {
                if (int.TryParse(textBox1.Text, out var i))
                    textBox1.Text = i.ToString();
                else
                    textBox1.Text = props.SacCommLen.ToString();
            }
            catch (Exception Ex)
            {
                DebugWrite.Writeln(nameof(textBox1_Validated), Ex);
            }
        }

        private void textBox2_Validated(object sender, EventArgs e)
        {
            try
            {
                // 15 15.4 15.454445
                if (double.TryParse(textBox2.Text, out var dbl))
                    textBox2.Text = dbl.ToString("0.00");
                else
                    textBox2.Text = ((double)props.SacVpos / 100D).ToString("0.00");
            }
            catch (Exception Ex)
            {
                DebugWrite.Writeln(nameof(textBox2_Validated), Ex);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                var textfile = Path.Combine(Props.GetSettingDirectory(), props.SacNGLists);
                var notepad = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Windows), "System32", "notepad.exe");
                OpenProcess.OpenProgram(textfile, notepad);
            }
            catch (Exception Ex)
            {
                DebugWrite.Writeln(nameof(button2_Click), Ex);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                using (var openFileDialog1 = new System.Windows.Forms.OpenFileDialog())
                {
                    //openFileDialog1.FileName = Properties.Settings.Default.DefExecFile;
                    openFileDialog1.InitialDirectory = @"C:\";
                    openFileDialog1.Filter = "実行ファイル(*.exe;*.com)|*.exe;*.com|すべてのファイル(*.*)|*.*";
                    openFileDialog1.FilterIndex = 1;
                    openFileDialog1.Title = "開くファイルを選択してください";
                    //ダイアログボックスを閉じる前に現在のディレクトリを復元するようにする
                    openFileDialog1.RestoreDirectory = true;

                    if (openFileDialog1.ShowDialog(this) == DialogResult.OK)
                    {
                        //選択されたファイルを表示する
                        textBox3.Text = openFileDialog1.FileName;
                    }
                }
            }
            catch (Exception Ex)
            {
                DebugWrite.Writeln(nameof(button3_Click), Ex);
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
                this.panel1.Enabled = true;
            else
                this.panel1.Enabled = false;
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked)
                this.panel2.Enabled = true;
            else
                this.panel2.Enabled = false;
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox3.Checked)
                this.textBox1.Enabled = true;
            else
                this.textBox1.Enabled = false;
        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox4.Checked)
                this.textBox2.Enabled = true;
            else
                this.textBox2.Enabled = false;
        }

        private void 設定フォルダーを開くOToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Directory.Exists(Props.GetSettingDirectory()))
            {
                Process.Start(Props.GetSettingDirectory());
            }
        }

        private void 終了XToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private async void tabPage3_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop, false);

            try
            {
                if (ProgramStatus == 1) return;

                GetForm();
                ClearLog();

                var exec_file = props.ExecFile;
                if (!File.Exists(exec_file))
                {
                    exec_file = GetExecFile(exec_file);
                    if (!File.Exists(exec_file))
                        AddLog("FFmpeg.exe がありません。", 2);
                }
                props.ExecFile = exec_file;
                LogFile = null;

                ProgramStatus = 1;
                //1ファイルずつ順次実行する
                for (int i = 0; i < files.Length; i++)
                {
                    var filetype = Utils.IsFileType(files[i]);
                    if (filetype == 0)
                        await Task.Run(() => ConvXml(files[i], "twitch"));
                    else if (filetype == 1)
                        await Task.Run(() => ConvVideo(files[i]));
                }
                ProgramStatus = 0;

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

        private void tabPage3_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.All;
            else
                e.Effect = DragDropEffects.None;

        }

        private void checkBox9_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox9.Checked)
                this.panel4.Enabled = true;
            else
                this.panel4.Enabled = false;
        }

        private void checkBox11_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox11.Checked)
                this.textBox6.Enabled = true;
            else
                this.textBox6.Enabled = false;
        }

        private void checkBox10_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox10.Checked)
                this.textBox5.Enabled = true;
            else
                this.textBox5.Enabled = false;
        }

        private void textBox6_Validated(object sender, EventArgs e)
        {
            try
            {
                if (int.TryParse(textBox6.Text, out var i))
                    textBox6.Text = i.ToString();
                else
                    textBox6.Text = props.TwiCommLen.ToString();
            }
            catch (Exception Ex)
            {
                DebugWrite.Writeln(nameof(textBox6_Validated), Ex);
            }
        }

        private void textBox5_Validated(object sender, EventArgs e)
        {
            try
            {
                // 15 15.4 15.454445
                if (double.TryParse(textBox5.Text, out var dbl))
                    textBox5.Text = dbl.ToString("0.00");
                else
                    textBox5.Text = ((double)props.TwiVpos / 100D).ToString("0.00");
            }
            catch (Exception Ex)
            {
                DebugWrite.Writeln(nameof(textBox5_Validated), Ex);
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (props.IsSaveData)
            {
                GetForm();
                props.SaveData();
            }
        }
    }
}