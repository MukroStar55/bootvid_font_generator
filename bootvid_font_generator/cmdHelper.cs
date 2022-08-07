using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace bootvid_font_generator
{
    static class cmdHelper
    {
        [StructLayout(LayoutKind.Sequential)]
        internal struct COORD
        {
            internal short X;
            internal short Y;

            internal COORD(short x, short y)
            {
                X = x;
                Y = y;
            }
        }

        private const int LF_FACESIZE = 32;
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal unsafe struct CONSOLE_FONT_INFO_EX
        {
            internal uint cbSize;
            internal uint nFont;
            internal COORD dwFontSize;
            internal int FontFamily;
            internal int FontWeight;
            internal fixed char FaceName[LF_FACESIZE];
        }

        private const int FF_DONTCARE = 0x00;
        private const int FW_DONTCARE = 0;

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool SetCurrentConsoleFontEx(
            IntPtr consoleOutput,
            bool maximumWindow,
            ref CONSOLE_FONT_INFO_EX consoleCurrentFontEx);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetStdHandle(int nStdHandle);

        private const int STD_OUTPUT_HANDLE = -11;
        private static IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);

        public static void EnterDOS8x8_MODE()
        {
            unsafe
            {
                string fntStr = "Terminal";

                IntPtr hnd = GetStdHandle(STD_OUTPUT_HANDLE);
                if (hnd != INVALID_HANDLE_VALUE)
                {
                    CONSOLE_FONT_INFO_EX cfi = new CONSOLE_FONT_INFO_EX();
                    cfi.cbSize = (uint)Marshal.SizeOf(cfi);

                    CONSOLE_FONT_INFO_EX newInfo = new CONSOLE_FONT_INFO_EX();
                    newInfo.cbSize = (uint)Marshal.SizeOf(newInfo);
                    newInfo.FontFamily = FF_DONTCARE;

                    IntPtr ptr = new IntPtr(newInfo.FaceName);
                    Marshal.Copy(fntStr.ToCharArray(), 0, ptr, fntStr.Length);

                    newInfo.dwFontSize = new COORD(8, 8);
                    newInfo.FontWeight = FW_DONTCARE;

                    SetCurrentConsoleFontEx(hnd, false, ref newInfo);
                }
            }
        }
    }
}
