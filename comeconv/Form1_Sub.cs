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
                    if (LogFile != null)
                        System.IO.File.AppendAllText(LogFile, System.DateTime.Now.ToString("HH:mm:ss ") + s + "\r\n");
                }
            }));
        }

        //実行プロセスのログ書き込み
        public void AddExecLog(string s)
        {
            AddLog(s, 1);
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
                textBox3.Text = props.ExecFile;
                textBox4.Text = props.BreakCommand;
                checkBox8.Checked = props.IsLogging;

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
                textBox2.Text = ((double)props.SacVpos / 100D).ToString("0.00");
                foreach (var item in props.SacVideoList.Keys)
                    comboBox1.Items.Add(item);
                comboBox1.SelectedItem = props.SacVideoMode;
                var ngfile = Path.Combine(Props.GetSettingDirectory(), props.SacNGLists);
                props.SacNGWords = props.ReadNGList(ngfile);
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
                props.ExecFile = textBox3.Text;
                props.BreakCommand = textBox4.Text;
                props.IsLogging=  checkBox8.Checked;

                foreach (Control co in panel1.Controls)
                {
                    if (co.GetType().Name == "RadioButton")
                    {
                        if ((bool)((RadioButton)co).Checked)
                            props.SacEmojiMode = rbRegex.Replace(co.Name.ToString(), "$1");
                    }
                }
                foreach (Control co in panel2.Controls)
                {
                    if (co.GetType().Name == "RadioButton")
                    {
                        if ((bool)((RadioButton)co).Checked)
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
                props.SacNGWords = props.ReadNGList(ngfile);
            }
            catch (Exception Ex)
            {
                DebugWrite.Writeln(nameof(GetForm), Ex);
                return;
            }

            return;
        }

        //コメントファイルを変換
        private void ConvXml(string filename, string mode = "sac")
        {
            var result = false;
            try
            {
                //元のファイルをrename
                var backupfile = Utils.GetBackupFileName(filename, ".org");
                File.Move(filename, backupfile);

                AddLog("コメント変換開始します。", 1);
                AddLog("元ファイル: " + Path.GetFileName(backupfile), 1);
                AddLog("変換ファイル: " + Path.GetFileName(filename), 1);
                using (var conv = new ConvComment(this, props))
                {
                    if (mode == "twitch")
                        result = conv.TwitchConvert(backupfile, filename);
                    else
                        result = conv.SacXmlConvert(backupfile, filename);
                    if (result)
                    {
                        AddLog("コメント変換終了しました。", 1);
                    }
                    else
                    {
                        AddLog("コメント変換に失敗しました。", 1);
                    }
                }
                return;
            }
            catch (Exception Ex)
            {
                DebugWrite.Writeln(nameof(ConvXml), Ex);
                return;
            }
        }

        //動画ファイルを変換
        private void ConvVideo(string filename)
        {
            try
            {
                //保存ファイル名作成
                epi = new ExecPsInfo();
                epi.Exec = props.ExecFile;
                epi.Arg = "-i \"%INFILE%\" " + props.SacVideoList[props.SacVideoMode];
                epi.SaveFile = filename;
                epi.Ext2 = "." + props.SplitVideoItem(props.SacVideoMode);
                epi.SaveFile2 = Path.Combine(Path.GetDirectoryName(epi.SaveFile), Path.GetFileNameWithoutExtension(epi.SaveFile) + epi.Ext2);
                if (Path.GetExtension(filename) == epi.Ext2)
                {
                    //元のファイルをrename
                    epi.SaveFile = Utils.GetBackupFileName(filename, ".org");
                    File.Move(filename, epi.SaveFile);
                }

                AddLog("動画変換開始します。", 1);
                AddLog("元ファイル: " + Path.GetFileName(epi.SaveFile), 1);
                AddLog("変換ファイル: " + Path.GetFileName(epi.SaveFile2), 1);
                //映像ファイル出力処理
                if (ExecFFmpeg(epi))
                {
                    AddLog("動画変換終了しました。", 1);
                }
                else
                {
                    AddLog("動画変換に失敗しました。", 1);
                }
            }
            catch (Exception Ex)
            {
                DebugWrite.Writeln(nameof(ConvVideo), Ex);
                return;
            }
        }

        public bool ExecFFmpeg(ExecPsInfo epi)
        {
            ExecConvert ecv = null;
            var result = false;
            try
            {
                ecv = new ExecConvert(this);
                var arg = epi.Arg;
                arg = arg.Replace("%INFILE%", epi.SaveFile);
                arg = arg.Replace("%OUTFILE%", epi.SaveFile2);
                if (epi.SaveFile == epi.SaveFile2)
                {
                    AddLog("変換元と変換先のファイルが同じです。", 1);
                    ecv.Dispose();
                    ecv = null;
                }
                else
                {
                    ecv.ExecPs(epi.Exec, arg);
                    result = true;
                }
            }
            catch (Exception Ex)
            {
                DebugWrite.Writeln(nameof(ExecFFmpeg), Ex);
            }
            finally
            {
                var endflg = false;
                while (!endflg)
                {
                    if (ecv != null)
                    {
                        if (ecv.PsStatus >= 1)
                        {
                            ecv.Dispose();
                            ecv = null;
                        }
                        else
                        {
                            endflg = false;
                            Task.Delay(1000).Wait();
                        }
                    }
                    else
                    {
                        endflg = true;
                    }
                }
            }
            return result;
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
