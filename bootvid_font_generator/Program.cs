/*
 * PROJECT:     ReactOS BootVid Font Generator Utility
 * LICENSE:     GNU GPLv2 or any later version as published by the Free Software Foundation
 * PURPOSE:     Generates the FontData array for the bootdata.c file of bootvid.dll
 * COPYRIGHT:   Copyright 2016 Colin Finck <colin@reactos.org>
 * 
 * PORTED:      Ported to C# by Mükremin BAKİ. (3.8.2022)
 */

/*
 * Enable this #define if you want to dump the generated character on screen
 */
//#define DUMP_CHAR_ON_SCREEN

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace bootvid_font_generator
{
    class Program
    {
        // Windows original Blue Screen font is "Lucida Console" at FONT_SIZE 10 with no offsets.
        public const string FONT_NAME_DEF = "Lucida Console";   // "DejaVu Sans Mono" // "Anonymous Pro"
        public const int FONT_SIZE_DEF = 10;
        public const int X_OFFSET_DEF = 0;                      // 0                  // 1
        public const int Y_OFFSET_DEF = 0;

        public const int HEIGHT = 13;   // Must be == BOOTCHAR_HEIGHT (see reactos/drivers/base/bootvid/precomp.h)
        public const int WIDTH = 8;     //  8 bits == 1 byte

        static StreamWriter errorStd, outStd;

#if DUMP_CHAR_ON_SCREEN
        static void DumpCharacterOnScreen(uint[] BmpBits)
        {
            int i, j;
        
            for (i = 0; i < HEIGHT; i++)
            {
                for (j = WIDTH; --j >= 0;)
                {
                    if ((BmpBits[i] >> j & 0x1) != 0)
                        Console.Write(' ');
                    else
                        Console.Write('#');
                }
        
                Console.Write('\n');
            }
        }
#else
        static int iBegin = 0;
        static void DumpCharacterFontData(uint[] BmpBits)
        {
            int i;
        
            Console.Out.Write("    ");
        
            for (i = 0; i < HEIGHT; i++)
                Console.Out.Write("0x{0:X2}, ", BmpBits[i]);
        
            Console.Out.Write(" // {0:D}\n", iBegin);
            iBegin += HEIGHT;
        }
#endif

        static void Main(string[] args)
        {
            Console.Title = "ReactOS BootVid Font Generator Utility";

#if !DUMP_CHAR_ON_SCREEN
            Console.Title += " - Font Generator";
#else
            Console.Title += " - Font Viewer";
#endif
            if (args.Length > 4 || (args.Length >= 1 && (args[0] == "/?")))
            {
                Console.Write("Usage: {0} \"font name\" [font size] [X-offset] [Y-offset]\n" +
                    "Default font name is: \"{1}\"\n" +
                    "Default font size is: {2:D}\n" +
                    "Default X-offset  is: {3:D}\n" +
                    "Default Y-offset  is: {4:D}\n",
                    args[0],
                    FONT_NAME_DEF, FONT_SIZE_DEF, X_OFFSET_DEF, Y_OFFSET_DEF);

                Environment.Exit(-1);
            }
            else if (args.Length == 1 && args[0] == "-about")
            {
                Console.WriteLine("ReactOS BootVid Font Generator Utility\n"+
                    "Copyright 2016 Colin Finck <colin@reactos.org>\n"+
                    "Ported to C# by Mükremin BAKİ. (3.8.2022)");

                Environment.Exit(-1);
            }
                

#if !DUMP_CHAR_ON_SCREEN
            FileStream outfile = new FileStream("output.txt", FileMode.OpenOrCreate),
                errorFile = new FileStream("error.log", FileMode.OpenOrCreate);

            errorStd = new StreamWriter(errorFile);
            outStd = new StreamWriter(outfile);

            Console.SetError(errorStd);
            Console.SetOut(outStd);
#else
            // If the characters are to be viewed from the console, set up the Console properly.
            cmdHelper.EnterDOS8x8_MODE();
            Console.Clear();
#endif

            DumpFont((args.Length <= 1) ? FONT_NAME_DEF : args[0],
                     (args.Length <= 2) ? FONT_SIZE_DEF : Int32.Parse(args[1]),
                     (args.Length <= 3) ? X_OFFSET_DEF : Int32.Parse(args[2]),
                     (args.Length <= 4) ? Y_OFFSET_DEF : Int32.Parse(args[3]));

#if !DUMP_CHAR_ON_SCREEN
            Console.Error.Close();
            Console.Out.Close();
#endif
            Environment.Exit(0);
        }

        static void DumpFont(string FontName, int FontSize, int XOffset, int YOffset)
        {
            int iHeight;
            IntPtr hDC = IntPtr.Zero;
            IntPtr hFont = IntPtr.Zero;

            uint[] BmpBits = new uint[HEIGHT];
            ushort c;

            hDC = libs.CreateCompatibleDC(IntPtr.Zero);
            if (!IntBool(hDC))
            {
                Console.Error.Write("CreateCompatibleDC failed with error {0:D}!\n", Marshal.GetLastWin32Error());
                goto Cleanup;
            }
            iHeight = -MulDiv(FontSize, libs.GetDeviceCaps(hDC, libs.LOGPIXELSY), 72);
            hFont = libs.CreateFont(iHeight, 0, 0, 0, libs.FW_NORMAL, 0, 0, 0,
                                libs.ANSI_CHARSET, libs.OUT_DEFAULT_PRECIS, libs.CLIP_DEFAULT_PRECIS,
                                libs.NONANTIALIASED_QUALITY, libs.FIXED_PITCH, FontName);
            if (!IntBool(hFont))
            {
                Console.Error.Write("CreateFont failed with error {0:D}!\n", Marshal.GetLastWin32Error());
                goto Cleanup;
            }

            for (c = 0; c < 256; c++)
            {
                PlotCharacter(hDC, hFont, XOffset, YOffset, (char)c, BmpBits);

#if DUMP_CHAR_ON_SCREEN
                DumpCharacterOnScreen(BmpBits);
                Console.Write("\nPress any key to continue...\n");
                Console.SetCursorPosition(12, 4); Console.Write("Index: {0}", c);
                Console.SetCursorPosition(12, 5); Console.Write("Char: {0}", ((char)c).ToString().PadLeft(2));
                Console.ReadKey();
                Console.Clear();
#else
                DumpCharacterFontData(BmpBits);
#endif
            }

        Cleanup:
            if (IntBool(hFont))
                libs.DeleteObject(hFont);

            if (IntBool(hDC))
                libs.DeleteDC(hDC);
        }

        static bool PlotCharacter(IntPtr hDC, IntPtr hFont, int XOffset, int YOffset, char Character, uint[] BmpBits)
        {
            bool bReturnValue = false;
            IntPtr hOldBmp;
            IntPtr hOldFont;
            IntPtr hBmp = IntPtr.Zero;
            byte[] BmpInfo;

            unsafe{ BmpInfo = new byte[sizeof(libs.BITMAPINFO) + sizeof(libs.RGBQUAD)]; }


            int size = Marshal.SizeOf(typeof(libs.BITMAPINFO));
            IntPtr ptr = IntPtr.Zero;
            ptr = Marshal.AllocHGlobal(size);
            Marshal.Copy(BmpInfo, 0, ptr, size);

            libs.PBITMAPINFO pBmpInfo = (libs.PBITMAPINFO)Marshal.PtrToStructure(ptr, typeof(libs.PBITMAPINFO));

            hBmp = libs.CreateCompatibleBitmap(hDC, WIDTH, HEIGHT);
            if (!IntBool(hBmp))
            {
                Console.Error.Write("CreateCompatibleBitmap failed with error {0:D}!\n", Marshal.GetLastWin32Error());
                goto Cleanup;
            }

            hOldBmp = libs.SelectObject(hDC, hBmp);
            hOldFont = libs.SelectObject(hDC, hFont);
            libs.SetBkColor(hDC, RGB(0, 0, 0));
            libs.SetTextColor(hDC, RGB(255, 255, 255));
            libs.TextOut(hDC, XOffset, YOffset, Character.ToString(), 1);

            Array.Clear(BmpInfo, 0, BmpInfo.Length);

            unsafe
            {
                pBmpInfo.bmiHeader.biSize = (uint)sizeof(libs.BITMAPINFOHEADER); //pBmpInfo.bmiHeader.Init();
                pBmpInfo.bmiHeader.biHeight = -HEIGHT;
                pBmpInfo.bmiHeader.biWidth = WIDTH;
                pBmpInfo.bmiHeader.biCompression = libs.BitmapCompressionMode.BI_RGB;
                pBmpInfo.bmiHeader.biBitCount = 1;
                pBmpInfo.bmiHeader.biPlanes = 1;
            }

            bReturnValue = true;

            if (libs.GetDIBits(hDC, hBmp, 0, HEIGHT, BmpBits, ref pBmpInfo, libs.DIB_Color_Mode.DIB_RGB_COLORS) < 0)
            {
                Console.Error.Write("GetDIBits failed with error {0:D}!\n", Marshal.GetLastWin32Error());
                bReturnValue = false;
            }

            libs.SelectObject(hDC, hOldBmp);
            libs.SelectObject(hDC, hOldFont);

        Cleanup:
            if (IntBool(hBmp))
                libs.DeleteObject(hBmp);

            return bReturnValue;
        }

        public static bool IntBool(IntPtr ptr)
        {
            if (ptr != IntPtr.Zero)
                return true;
            return false;
        }

        public static int RGB(int r, int g, int b)
        {
            return ((int)(((byte)(b) | ((ushort)((byte)(g)) << 8)) | (((uint)(byte)(r)) << 16)));
        }

        public static int MulDiv(int nNumber, int nNumerator, int nDenominator)
        {
            return unchecked((int)(System.Math.BigMul(nNumber, nNumerator) / nDenominator));
        }
    }
}
