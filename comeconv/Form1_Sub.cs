using System;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;

using comeconv.Prop;
using comeconv.Proc;
using comeconv.Util;

namespace comeconv
{
    public partial class Form1 : Form
    {
        private static Regex rbRegex = new Regex("^rB_(.+)$", RegexOptions.Compiled);


        //ログウインドウ初期化
        private void ClearLog()
        {
            this.Invoke(new Action(() =>
            {
                lock (lockObject)
                {
                    listBox1.Items.Clear();
                    listBox1.TopIndex = 0;
                }
            }));
        }

        //ログウインドウ書き込み
        public void AddLog(string s, int num)

        {
            this.Invoke(new Action(() =>
            {
                lock (lockObject)
                {
                    if (num == 1)
                    {
                        if (listBox1.Items.Count > 50)
                        {
                            listBox1.Items.RemoveAt(0);
                            listBox1.TopIndex = listBox1.Items.Count - 1;
                        }
                        listBox1.Items.Add(s);
                        listBox1.TopIndex = listBox1.Items.Count - 1;
                    }
                    else if (num == 2) //エラー
                    {
                        MessageBox.Show(s + "\r\n",
                            "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else if (num == 3) //注意
                    {
                        MessageBox.Show(s + "\r\n",
                            "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    //if (props.IsLogging && LogFile != null)
                    //    System.IO.File.AppendAllText(LogFile, System.DateTime.Now.ToString("HH:mm:ss ") + s + "\r\n");
                }
            }));
        }

        //実行プロセスのログ書き込み
        public void AddExecLog(string s)
        {
            this.Invoke(new Action(() =>
            {
                lock (lockObject2)
                {
                    //textBox7.Text = s;
                    //if (props.IsLogging && LogFile2 != null)
                    //    System.IO.File.AppendAllText(LogFile2, System.DateTime.Now.ToString("HH:mm:ss ") + s);
                }
            }));
        }

        public void EnableButton(bool flag)
        {
            //true 中断→録画開始
            this.Invoke(new Action(() =>
            {
                /*
                if (flag)
                {
                    this.textBox1.Enabled = true;
                    //this.button2.Enabled = true;
                    this.button1.Text = "録画開始";
                    this.button1.Focus();
                }
                else
                {
                    this.textBox1.Enabled = false;
                    //this.button2.Enabled = false;
                    this.button1.Text = "中断";
                    this.button1.Focus();
                }
                */
            }));
        }
 
        //変数→フォーム
        private void SetForm()
        {
            try
            {
                foreach (Control co in panel1.Controls)
                {
                    if (co.GetType().Name == "RadioButton")
                    {
                        if (rbRegex.Replace(co.Name.ToString(), "$1") == props.SacEmojiMode.ToString())
                            ((RadioButton)co).Checked = true;
                    }
                }
                foreach (Control co in panel2.Controls)
                {
                    if (co.GetType().Name == "RadioButton")
                    {
                        if (rbRegex.Replace(co.Name.ToString(), "$1") == props.SacPremiumMode.ToString())
                            ((RadioButton)co).Checked = true;
                    }
                }
                checkBox1.Checked = props.IsSacEmoji;
                checkBox2.Checked = props.IsSacPremium;
                checkBox3.Checked = props.IsSacCommLen;
                checkBox4.Checked = props.IsSacVpos;
                checkBox5.Checked = props.IsSacGift;
                checkBox6.Checked = props.IsSacEmotion;
                checkBox7.Checked = props.IsSacNicoAd;

                textBox1.Text = props.SacCommLen.ToString();
                textBox2.Text = props.SacVpos.ToString("0.00");

                foreach (var item in props.SacVideoList.Keys)
                    comboBox1.Items.Add(item);
                comboBox1.SelectedItem = props.SacVideoMode;
                var ngfile = Path.Combine(Props.GetSettingDirectory(), props.SacNGLists);
                props.SacNGWords = ReadNGData<string>(ngfile);
            }
            catch (Exception Ex)
            {
                DebugWrite.Writeln(nameof(SetForm), Ex);
                return;
            }

            return;
        }

        //フォーム→変数
        private void GetForm()
        {
            try
            {
                foreach (Control co in panel1.Controls)
                {
                    if (co.GetType().Name == "RadioButton")
                    {
                        if ((bool)((RadioButton)co).Checked)
                            //if (rbRegex.Replace(co.Name.ToString(), "$1") == props.SacEmojiMode.ToString())
                            //    ((RadioButton)co).Checked = true;
                            props.SacEmojiMode = rbRegex.Replace(co.Name.ToString(), "$1");
                    }
                }
                foreach (Control co in panel2.Controls)
                {
                    if (co.GetType().Name == "RadioButton")
                    {
                        if ((bool)((RadioButton)co).Checked)
                            //if (rbRegex.Replace(co.Name.ToString(), "$1") == props.SacPremiumMode.ToString())
                            //    ((RadioButton)co).Checked = true;
                            props.SacPremiumMode = rbRegex.Replace(co.Name.ToString(), "$1");
                    }
                }

                props.IsSacEmoji = checkBox1.Checked;
                props.IsSacPremium = checkBox2.Checked;
                props.IsSacCommLen = checkBox3.Checked;
                props.IsSacVpos = checkBox4.Checked;
                props.IsSacGift = checkBox5.Checked;
                props.IsSacEmotion = checkBox6.Checked;
                props.IsSacNicoAd = checkBox7.Checked;

                if (int.TryParse(textBox1.Text, out var i))
                    props.SacCommLen = i;
                else
                    props.SacCommLen = Properties.Settings.Default.SacCommLen;
                if (long.TryParse(textBox2.Text.Replace(".", ""), out var lo))
                    props.SacVpos = lo;
                else
                    props.SacVpos = Properties.Settings.Default.SacVpos;

                props.SacVideoMode = comboBox1.SelectedItem.ToString();
                var ngfile = Path.Combine(Props.GetSettingDirectory(), props.SacNGLists);
                props.SacNGWords = ReadNGData<string>(ngfile);
            }
            catch (Exception Ex)
            {
                DebugWrite.Writeln(nameof(GetForm), Ex);
                return;
            }

            return;
        }

        //NGLists.txtを読み込み
        private List<string> ReadNGData<T>(string r_file)
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
                        if (!string.IsNullOrEmpty(line))
                            lists.Add(line);
                    }
                }
            }
            catch (Exception Ex)
            {
                DebugWrite.Writeln(nameof(ReadNGData), Ex);
                return lists;
            }

            return lists;
        }

        //コメントファイルを変換
        private void ConvXml(string filename)
        {
            //if (filename.IndexOf(".xml") < 0) return;

            try
            {
                var newfile = Path.Combine(Path.GetDirectoryName(filename), Path.GetRandomFileName());
                var renamefile = Utils.GetLogfile(Path.GetDirectoryName(filename), Path.GetFileName(filename));

                AddLog("コメント変換開始します。", 1);
                using (var conv = new ConvComment(this, props))
                {
                    if (conv.SacXmlConvert(filename, newfile))
                    {
                        if (!File.Exists(renamefile))
                            File.Move(newfile, renamefile);
                        AddLog("コメント変換終了しました。", 1);
                    }
                    else
                    {
                        AddLog("コメント変換に失敗しました。", 1);
                    }
                }
            }
            catch (Exception Ex)
            {
                AddLog("コメント変換処理エラー。\r\n" + Ex.Message, 2);
            }
        }

        //実行ファイルと同じフォルダにある指定ファイルのフルパスをGet
        private string GetExecFile(string file)
        {
            var fullAssemblyName = this.GetType().Assembly.Location;
            if (Path.GetFileName(file) == file)
                return Path.Combine(Path.GetDirectoryName(fullAssemblyName), file);
            return file;
        }

    }
}
