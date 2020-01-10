using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;


namespace RunningWindow
{
    class Program
    {
        const int WM_COPYDATA = 0x004A; // 固定数值，不可更改

        [DllImport("User32.dll", EntryPoint = "FindWindow")]
        private static extern int FindWindow(string lpClassName, string lpWindowName);

        [DllImport("User32.dll", EntryPoint = "SendMessage")]
        private static extern int SendMessage(int hWnd, int Msg, int wParam, ref COPYDATASTRUCT lParam);

        public struct COPYDATASTRUCT
        {
            public IntPtr dwData;
            public int cbData;
            [MarshalAs(UnmanagedType.LPStr)]
            public string lpData;
        }

        static void Main(string[] args)
        {
            //Console.WriteLine("Hello CMM_Interpreter");
            //foreach(string arg in args)
            //{
            //    Console.WriteLine(arg);
            //}
            for(int i = 0; i < num; i++)
            {
                args = Console.ReadLine();
            }
        }
    }
}
