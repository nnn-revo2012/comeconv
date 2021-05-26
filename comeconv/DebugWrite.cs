using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Net;

namespace comeconv
{
    public class DebugWrite
    {
        public static void Writeln(string method, Exception Ex)
        {
            Debug.WriteLine(method + "() Error: " + Ex.Message);
            Debug.WriteLine("StackTrace: ");
            Debug.WriteLine(Ex.StackTrace);
            Debug.WriteLine("");
        }

        public static void WriteWebln(string method, WebException Ex)
        {
            Debug.WriteLine(method + "() Error: " + Ex.Status.ToString());
            Debug.WriteLine("StackTrace: ");
            Debug.WriteLine(Ex.StackTrace);
            Debug.WriteLine("");
        }
    }
}
