using System;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;

using comeconv.Prop;
using comeconv.Proc;

namespace comeconv
{
    public partial class Form1 : Form
    {
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

        private void StartConv(string filename)
        {
            //if (filename.IndexOf(".xml") < 0) return;

            try
            {
                AddLog("出力開始します。", 1);
                using (var conv = new ConvComment(this, filename, null))
                {
                    conv.FileCopy(filename, "D:\\home\\tmp\\aaa.xml");
                }
                AddLog("コメント出力終了しました。", 1);
            }
            catch (Exception Ex)
            {
                AddLog("出力処理エラー。\r\n" + Ex.Message, 2);
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
