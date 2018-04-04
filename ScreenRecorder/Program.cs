using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Windows.Forms;
using Accord.Math;
using Accord.Video.FFMPEG;

namespace ScreenRecorder
{
    class Program
    {
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

            using (VideoFileWriter vfw = new VideoFileWriter())
            {
                vfw.Open(cfg.FileName, Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height, Rational.Round(cfg.FrameRate), VideoCodec.H264, cfg.BitRate);
                while(true)
                {
                    using (var bmp = GrabScreen())
                    {
                        vfw.WriteVideoFrame(bmp);
                    }
                    if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Escape)
                        break;
                }

                vfw.Close();
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

        private static Bitmap GrabScreen()
        {
            Bitmap bmp = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height, PixelFormat.Format24bppRgb);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.CopyFromScreen(Screen.PrimaryScreen.Bounds.X, Screen.PrimaryScreen.Bounds.Y, 0, 0, bmp.Size, CopyPixelOperation.SourceCopy);
            }

            return bmp;
        }
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
