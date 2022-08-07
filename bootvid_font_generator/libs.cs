using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace bootvid_font_generator
{
    class libs
    {
        public const int BI_RGB = 0;
        public const int DIB_RGB_COLORS = 0;
        public const int LOGPIXELSY = 90;

        public const int
            FW_NORMAL = 400, ANSI_CHARSET = 0,
            OUT_DEFAULT_PRECIS = 0, CLIP_DEFAULT_PRECIS = 0,
            NONANTIALIASED_QUALITY = 3, FIXED_PITCH = 1;

        public enum DIB_Color_Mode : uint
        {
            DIB_RGB_COLORS = 0,
            DIB_PAL_COLORS = 1
        }

        public enum BitmapCompressionMode : uint
        {
            BI_RGB = 0,
            BI_RLE8 = 1,
            BI_RLE4 = 2,
            BI_BITFIELDS = 3,
            BI_JPEG = 4,
            BI_PNG = 5
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct BITMAPINFOHEADER
        {
            public uint biSize;
            public int biWidth;
            public int biHeight;
            public ushort biPlanes;
            public ushort biBitCount;
            public BitmapCompressionMode biCompression;
            public uint biSizeImage;
            public int biXPelsPerMeter;
            public int biYPelsPerMeter;
            public uint biClrUsed;
            public uint biClrImportant;

            public void Init()
            {
                biSize = (uint)Marshal.SizeOf(this);
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct RGBQUAD
        {
            public byte rgbBlue;
            public byte rgbGreen;
            public byte rgbRed;
            public byte rgbReserved;
        }

        public struct BITMAPINFO
        {
            public BITMAPINFOHEADER bmiHeader;
            public RGBQUAD bmiColors;
        }

        public unsafe struct PBITMAPINFO
        {
            public BITMAPINFOHEADER bmiHeader;

            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 1, ArraySubType = UnmanagedType.Struct)]
            public RGBQUAD[] bmiColors;
        }

        /// <summary>
        ///        Belirtilen aygıtla uyumlu bir bellek aygıtı bağlamı (DC) oluşturur.
        /// </summary>
        /// <param name="hdc">Varolan bir DC'ye yönelik bir tanıtıcı. Bu tanıtıcı NULL ise, 
        ///        işlev, uygulamanın geçerli ekranıyla uyumlu bir bellek DC'si oluşturur.</param>
        /// <returns>
        ///        İşlev başarılı olursa, dönüş değeri bir bellek DC'sinin tanıtıcısıdır.
        ///        İşlev başarısız olursa, dönüş değeri <see cref="System.IntPtr.Zero"/> olur.
        /// </returns>
        [DllImport("gdi32.dll", EntryPoint = "CreateCompatibleDC", SetLastError = true)]
        public static extern IntPtr CreateCompatibleDC([In] IntPtr hdc);

        /// <summary>
        ///        Belirtilen aygıt bağlamıyla ilişkili aygıtla uyumlu bir bit eşlem oluşturur.
        /// </summary>
        /// <param name="hdc">Bir aygıt bağlamına yönelik bir tanıtıcı.</param>
        /// <param name="nWidth">Piksel cinsinden bit eşlem genişliği.</param>
        /// <param name="nHeight">Piksel cinsinden bit eşlem yüksekliği.</param>
        /// <returns>
        ///        İşlev başarılı olursa, dönüş değeri uyumlu bit eşlem (DDB) tanıtıcısıdır. 
        ///        İşlev başarısız olursa, dönüş değeri <see cref="System.IntPtr.Zero"/> olur.
        /// </returns>
        [DllImport("gdi32.dll", EntryPoint = "CreateCompatibleBitmap")]
        public static extern IntPtr CreateCompatibleBitmap([In] IntPtr hdc, int nWidth, int nHeight);

        /// <summary>
        ///        Belirtilen aygıt içeriğine (DC) bir nesne seçer. Yeni nesne, aynı türden 
        ///        önceki nesnenin yerini alır.
        /// </summary>
        /// <param name="hdc">DC'ye bir tanıtıcı.</param>
        /// <param name="hgdiobj">Seçilecek nesnenin tanıtıcısı.</param>
        /// <returns>
        ///   <para>Seçilen nesne bir bölge değilse ve işlev başarılıysa, dönüş değeri 
        ///         değiştirilen nesnenin tanıtıcısıdır. Seçilen nesne bir bölgeyse ve işlev 
        ///         başarılıysa, döndürülen değer aşağıdaki değerlerden biridir.</para>
        ///   <para>SIMPLEREGION - Bölge tek bir dikdörtgenden oluşur.</para>
        ///   <para>COMPLEXREGION - Bölge birden fazla dikdörtgenden oluşur.</para>
        ///   <para>NULLREGION - Bölge boştur.</para>
        ///   <para>Bir hata oluşursa ve seçilen nesne bir bölge değilse, dönüş değeri 
        ///   <c>NULL</c> olur. Aksi takdirde, <c>HGDI_ERROR</c> olur.</para>
        /// </returns>
        /// <remarks>
        ///   <para>Bu işlev, belirtilen türde önceden seçilen nesneyi döndürür. Bir uygulama, 
        ///         yeni nesneyle çizimi tamamladıktan sonra her zaman yeni bir nesneyi orijinal, 
        ///         varsayılan nesneyle değiştirmelidir.</para>
        ///   <para>Bir uygulama aynı anda birden fazla DC'ye tek bir bit eşlem seçemez.</para>
        ///   <para>ICM: Seçilen nesne bir fırça veya kalem ise, renk yönetimi gerçekleştirilir.</para>
        /// </remarks>
        [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
        public static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("gdi32.dll", EntryPoint = "BitBlt", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool BitBlt([In] IntPtr hdc, int nXDest, int nYDest, int nWidth, int nHeight, [In] IntPtr hdcSrc, int nXSrc, int nYSrc, uint dwRop);

        [DllImport("user32.dll")]
        public static extern bool ReleaseDC(IntPtr hWnd, IntPtr hDC);

        [DllImport("gdi32.dll")]
        public static extern int GetDeviceCaps(IntPtr hdc, int nIndex);

        [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
        public static extern bool DeleteDC(IntPtr hdc);

        [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
        public static extern bool DeleteObject(IntPtr hObject);

        [DllImport("gdi32.dll", EntryPoint = "GetDIBits")]
        public static extern int GetDIBits([In] IntPtr hdc,
                                    [In] IntPtr hbmp,
                                    uint uStartScan,
                                    uint cScanLines,
                                    [Out] uint[] lpvBits,
                                    ref PBITMAPINFO lpbi,
                                    DIB_Color_Mode uUsage);

        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateFont(int nHeight, int nWidth, int nEscapement,
           int nOrientation, int fnWeight, uint fdwItalic, uint fdwUnderline, uint
           fdwStrikeOut, uint fdwCharSet, uint fdwOutputPrecision, uint
           fdwClipPrecision, uint fdwQuality, uint fdwPitchAndFamily, string lpszFace);

        [DllImport("gdi32.dll")]
        public static extern uint SetTextColor(IntPtr hdc, int crColor);

        [DllImport("gdi32.dll")]
        public static extern uint SetBkColor(IntPtr hdc, int crColor);

        [DllImport("gdi32.dll", CharSet = CharSet.Auto)]
        public static extern bool TextOut(IntPtr hdc, int nXStart, int nYStart,
        string lpString, int cbString);

        [DllImport("Kernel32.dll", EntryPoint = "RtlZeroMemory", SetLastError = false)]
        public static extern void ZeroMemory(IntPtr dest, IntPtr size);

        [DllImportAttribute("<Unknown>", EntryPoint = "GetPointer")]
        public static extern IntPtr GetPointer();
    }
}
