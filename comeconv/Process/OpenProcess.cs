using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;

namespace comeconv.Proc
{
    class OpenProcess
    {

        public static void OpenProgram(string textfile, string programpath)
        {

            if (string.IsNullOrEmpty(textfile)) return;
            Process process = null;

            try
            {
               if (File.Exists(programpath))
               {
                    int num = programpath.LastIndexOf("\\");
                    if (num < 0)
                    {
                        Process.Start(programpath,textfile);
                    }
                    else
                    {
                        process = new Process();
                        process.StartInfo.FileName = programpath;
                        process.StartInfo.Arguments = textfile;
                        process.StartInfo.WorkingDirectory = programpath.Substring(0, num + 1);
                        process.Start();
                    }
                }
                else
                {
                    var ttt = programpath + "がありません";
                }
            }
            catch (Exception Ex)
            {
                DebugWrite.Writeln(nameof(OpenProgram), Ex);
            }
        }

        public static void OpenWeb(string liveurl, string browserpath, bool b_flg)
        {

            if (string.IsNullOrEmpty(liveurl)) return;
            Process process = null;

            try
            {
                if (b_flg)
                {
                    Process.Start(liveurl);
                }
                else if (File.Exists(browserpath))
                {
                    int num = browserpath.LastIndexOf("\\");
                    if (num < 0)
                    {
                        Process.Start(browserpath, liveurl);
                    }
                    else
                    {
                        process = new Process();
                        process.StartInfo.FileName = browserpath;
                        process.StartInfo.Arguments = liveurl;
                        process.StartInfo.WorkingDirectory = browserpath.Substring(0, num + 1);
                        process.Start();
                    }
                }
                else
                {
                    var ttt = browserpath + "がありません";
                }
            }
            catch (Exception Ex)
            {
                DebugWrite.Writeln(nameof(OpenWeb), Ex);
            }
        }

    }

}
