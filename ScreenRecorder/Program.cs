using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Accord.Math;
using Accord.Video.FFMPEG;


namespace ScreenRecorder
{
    class Program
    {
        [DllImport("gdi32.dll")]
        static extern int GetDeviceCaps(IntPtr hdc, int nIndex);

        static void Main(string[] args)
        {
            if (AskedForHelp(args))
            {
                Console.WriteLine();
                Console.WriteLine(@"┌──────────────────────────────────────────────────────────────────────────────┐");
                Console.WriteLine(@"│ DrCircuit's Time Lapse Screen Recorder!              ////      V 1.0         │");
                Console.WriteLine(@"├──────────────────────────────────────────────────────────────────────────────┤");
                Console.WriteLine(@"│                                                                              │");
                Console.WriteLine(@"│   USAGE: ScreenRecorder.exe <filename> <frames per second> <bits per second> │");
                Console.WriteLine(@"│      (all are optional)                                                      │");
                Console.WriteLine(@"│                                                                              │");
                Console.WriteLine(@"│   Press Escape at any time inside the console window to stop recording.      │");
                Console.WriteLine(@"│                                                                              │");
                Console.WriteLine(@"└──────────────────────────────────────────────────────────────────────────────┘");
                Console.WriteLine();
                return;
            }
            Settings cfg = Parse(args);
            var dim = GetScreenRes();
            try
            {
                using (VideoFileWriter vfw = new VideoFileWriter())
                {
                
                    vfw.Open(cfg.FileName, dim.Horizontal , dim.Vertical, Rational.Round(cfg.FrameRate), VideoCodec.H264, cfg.BitRate);
                    while(true)
                    {
                        if (Console.KeyAvailable)
                        {
                            if(Console.ReadKey(true).Key == ConsoleKey.Escape)
                                break;
                        }
                        using (var bmp = GrabScreen(dim))
                        {
                            vfw.WriteVideoFrame(bmp);
                        }
                    
                    }

                    vfw.Close();
                    Console.WriteLine("Bailing...");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            
        }

        private static ScreenRes GetScreenRes()
        {
            Graphics g = Graphics.FromHwnd(IntPtr.Zero);
            IntPtr desktop = g.GetHdc();
            var width = GetDeviceCaps(desktop, (int)GDI.HorisontalResolutionDesktop);
            var height = GetDeviceCaps(desktop, (int)GDI.VerticalResolutionDesktop);
            return new ScreenRes{ Vertical = (height-1)/2*2, Horizontal = (width-1)/2*2};

        }
        private static float DetermineScaleFactor()
        {
            using (Graphics g = Graphics.FromHwnd(IntPtr.Zero))
            {
                IntPtr desktop = g.GetHdc();
                var screenHeight = GetDeviceCaps(desktop, (int) GDI.VerticalResolutionScreen);
                var desktopHeight = GetDeviceCaps(desktop, (int) GDI.VerticalResolutionDesktop);
                float scale = desktopHeight / (float) screenHeight;
                return scale;
            }
        }

        private static bool AskedForHelp(string[] args)
        {
            return args.Contains("-h") || args.Contains("/h");
        }

        private static Settings Parse(string[] args)
        {
            
            var settings = new Settings();
            if (args.Length == 0)
                return settings;
            if (args[0]!= null)
            {
                settings.FileName = args[0];
            }
            if (args.Length == 1)
                return settings;
            if (args[1] != null)
            {
                settings.FrameRate = Int32.Parse(args[1]);
            }
            if (args.Length == 2)
                return settings;
            if (args[2] != null)
            {
                settings.BitRate = Int32.Parse(args[2]);
            }

            return settings;
        }
        

        private static Bitmap GrabScreen(ScreenRes dim)
        {
            Bitmap bmp = new Bitmap(dim.Horizontal, dim.Vertical, PixelFormat.Format24bppRgb);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.CopyFromScreen(Screen.PrimaryScreen.Bounds.X, Screen.PrimaryScreen.Bounds.Y, 0, 0, bmp.Size, CopyPixelOperation.SourceCopy);
            }

            return bmp;
        }
    }

    internal class ScreenRes    
    {
        public int Vertical { get; set; }
        public int Horizontal { get; set; }
    }


    internal enum GDI
    {
        VerticalResolutionScreen = 10,
        HorisontalResolutionScreen = 11,
        VerticalResolutionDesktop = 117,
        HorisontalResolutionDesktop = 118
    }
    internal class Settings
    {
        private string _fileName;
        private int _frameRate;
        private int _bitRate;
        public int FrameRate
        {
            get => _frameRate == 0 ? 10 : _frameRate;
            set => _frameRate = value;
        }

        public string FileName
        {
            get => _fileName ?? $"{DateTime.Now:yy-MM-dd hh-mm-ss}-video.mp4";
            set => _fileName = value;
        }

        public int BitRate {
            get => _bitRate == 0 ? 1200000 : _bitRate;
            set => _bitRate = value;
        }
    }
}
