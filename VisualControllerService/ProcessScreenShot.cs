using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace VisualControllerService
{
    public class ProcessScreenShot
    {
        public Process CurrentProcess { get; set; }

        public IEnumerable<Process> GetProcesses()
        {
            return Process.GetProcesses().Where(p=>!string.IsNullOrEmpty(p.MainWindowTitle));
        }

        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowDC(IntPtr hwnd);

        [DllImport("gdi32.dll")]
        public static extern bool DeleteDC(IntPtr hdc);

        [DllImport("user32.dll")]
        public static extern bool ReleaseDC(IntPtr hWnd, IntPtr hDc);

        [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetWindowRect(IntPtr hWnd, ref RECT lpRect);

        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        // 导入Windows API函数
        [DllImport("user32.dll")]
        static extern bool SendMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);

        const uint WM_LBUTTONDOWN = 0x0201;
        const uint WM_LBUTTONUP = 0x0202;
        const uint WM_LBUTTONDBLCLK = 0x0203;

        public ImageSource CaptureWindow(Process process)
        {
            this.CurrentProcess = process;
            IntPtr mainWindowHandle = process.MainWindowHandle;
            ShowWindow(mainWindowHandle, 1); // 3 代表 SW_MAXIMIZE
            SetForegroundWindow(mainWindowHandle);
            IntPtr windowDC = GetWindowDC(mainWindowHandle);
            //// 等待一段时间确保窗口已经最大化
            //System.Threading.Thread.Sleep(1000);
            RECT windowRect = new RECT();
            GetWindowRect(mainWindowHandle, ref windowRect);
            int width = windowRect.Right - windowRect.Left;
            int height = windowRect.Bottom - windowRect.Top;

            // 截图
            Bitmap bitmap = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);


            Console.WriteLine($"width:{width}, height:{height}, size:{bitmap.Size}");
            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                graphics.CopyFromScreen(windowRect.Left, windowRect.Top, 0, 0, bitmap.Size);
            }
            //bitmap.Save("screenshot.png", ImageFormat.Png);

            ReleaseDC(mainWindowHandle, windowDC);
            // 还原窗口
            ShowWindow(mainWindowHandle, 11); // 9 代表 SW_RESTORE
            var source = BitmapToImageSource(bitmap);
            bitmap.Dispose();
            return source;

        }



        private ImageSource BitmapToImageSource(Bitmap bitmap)
        {
            // 使用BitmapImage并设置其源为Bitmap
            BitmapImage bitmapImage = new BitmapImage();
            using (var memoryStream = new MemoryStream())
            {
                bitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Bmp);
                memoryStream.Position = 0;
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memoryStream;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
            }
            return bitmapImage;
        }
        [DllImport("user32.dll", EntryPoint = "SendMessageA")]
        public static extern int SendMessage(IntPtr hwnd, int wMsg, IntPtr wParam, IntPtr lParam);

       
        public void MouseClick(int x, int y)
        {
            IntPtr mainWindowHandle = CurrentProcess.MainWindowHandle;
            ShowWindow(mainWindowHandle, 1); // 3 代表 SW_MAXIMIZE
            SetForegroundWindow(mainWindowHandle);
            RECT windowRect = new RECT();
            GetWindowRect(mainWindowHandle, ref windowRect);
            Thread.Sleep(1000);
           // SetCursorPos((windowRect.Left + x), (windowRect.Top + y));
            SendMessage(mainWindowHandle, 0x201, IntPtr.Zero, new IntPtr(x + (y << 16)));
            SendMessage(mainWindowHandle, 0x202, IntPtr.Zero, new IntPtr(x + (y << 16)));

        }


        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;                             //最左坐标
            public int Top;                             //最上坐标
            public int Right;                           //最右坐标
            public int Bottom;                        //最下坐标
        }
    }
}
