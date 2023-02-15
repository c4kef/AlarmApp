using SkiaSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace AlarmApp
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class MainPage : ContentPage, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public List<int> Hours { get; set; }
        public List<int> Minutes { get; set; }
        public List<int> Seconds { get; set; }

        private int _hour;
        public int Hour 
        {
            get
            {
                return _hour;
            }
            set
            {
                _hour = value;

                OnPropertyChanged(nameof(Hour));

                if (IsReady)
                {
                    MessagingCenter.Send<object, string>(this, "AlarmSetup", $"Start|{GetNextAlarm()}");
                    File.WriteAllText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "timesave.data"), $"{value}|{Minute}|{Second}");
                }
            }
        }

        private int _minute;
        public int Minute
        {
            get
            {
                return _minute;
            }
            set
            {
                _minute = value;

                OnPropertyChanged(nameof(Minute));

                if (IsReady)
                {
                    MessagingCenter.Send<object, string>(this, "AlarmSetup", $"Start|{GetNextAlarm()}");
                    File.WriteAllText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "timesave.data"), $"{Hour}|{value}|{Second}");
                }
            }
        }

        private int _second;
        public int Second
        {
            get
            {
                return _second;
            }
            set
            {
                _second = value;

                OnPropertyChanged(nameof(Second));

                if (IsReady)
                {
                    MessagingCenter.Send<object, string>(this, "AlarmSetup", $"Start|{GetNextAlarm()}");
                    File.WriteAllText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "timesave.data"), $"{Hour}|{Minute}|{value}");
                }
            }
        }

        public bool IsReady = false;

        public MainPage()
        {
            BindingContext = this;
            InitializeComponent();
            
            Hours = new List<int>();
            Hours.AddRange(Enumerable.Range(0, 24));

            Minutes = new List<int>();
            Minutes.AddRange(Enumerable.Range(0, 60));

            Seconds = new List<int>();
            Seconds.AddRange(Enumerable.Range(0, 60));

            OnPropertyChanged(nameof(Hours));
            OnPropertyChanged(nameof(Minutes));
            OnPropertyChanged(nameof(Seconds));

            MessagingCenter.Subscribe<object, string>(this, "AlarmUI",
            (sender, arg) =>
            {
                if (arg == "VisibleStop")
                    LayoutStop.IsVisible = true;
            });

            Device.StartTimer(TimeSpan.FromSeconds(1 / 60f), () =>
            {
                canvasView.InvalidateSurface();
                return true;
            });

            Device.StartTimer(TimeSpan.FromMilliseconds(1000), () =>
            {
                slider.TranslationX = -80;
                slider.TranslateTo(80, 0, 800, Easing.Linear);

                return true;
            });

            MessagingCenter.Send<object, string>(this, "AlarmReceiver", "Status");

            IsReady = true;

            if (File.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "timesave.data")))
            {
                var time = File.ReadAllText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "timesave.data")).Split('|');
                Hour = int.Parse(time[0]);
                Minute = int.Parse(time[1]);
                Second = int.Parse(time[2]);
            }
            else
            {
                Hour = DateTime.Now.Hour;
                Minute = DateTime.Now.Minute;
                Second = DateTime.Now.Second;
            }
        }

        SKPath path = new SKPath();
        float arcLength = 105;

        private DateTime GetNextAlarm()
        {
            DateTime today = DateTime.Today;
            DateTime possibleDate = new DateTime(today.Year, today.Month, today.Day, Hour, Minute, Second);

            if (DateTime.Now > possibleDate)
                return possibleDate.AddDays(1);

            return possibleDate;
        }

        private SKPaint GetPaintColor(SKPaintStyle style, string hexColor, float strokeWidth = 0, SKStrokeCap cap = SKStrokeCap.Round, bool IsAntialias = true)
        {
            return new SKPaint
            {
                Style = style,
                StrokeWidth = strokeWidth,
                Color = string.IsNullOrWhiteSpace(hexColor) ? SKColors.White : SKColor.Parse(hexColor),
                StrokeCap = cap,
                IsAntialias = IsAntialias
            };
        }

        private void canvas_PaintSurface(object sender, SkiaSharp.Views.Forms.SKPaintSurfaceEventArgs e)
        {
            SKImageInfo info = e.Info;
            SKSurface surface = e.Surface;
            SKCanvas canvas = surface.Canvas;

            SKPaint strokePaint = GetPaintColor(SKPaintStyle.Stroke, null, 10, SKStrokeCap.Square);
            SKPaint dotPaint = GetPaintColor(SKPaintStyle.Fill, "#DE0469");
            SKPaint hrPaint = GetPaintColor(SKPaintStyle.Stroke, "#262626", 4, SKStrokeCap.Square);
            SKPaint minPaint = GetPaintColor(SKPaintStyle.Stroke, "#DE0469", 2, SKStrokeCap.Square);
            SKPaint bgPaint = GetPaintColor(SKPaintStyle.Fill, "#FFFFFF");

            canvas.Clear();

            SKRect arcRect = new SKRect(10, 10, info.Width - 10, info.Height - 10);
            SKRect bgRect = new SKRect(25, 25, info.Width - 25, info.Height - 25);
            canvas.DrawOval(bgRect, bgPaint);

            strokePaint.Shader = SKShader.CreateLinearGradient(
                               new SKPoint(arcRect.Left, arcRect.Top),
                               new SKPoint(arcRect.Right, arcRect.Bottom),
                               new SKColor[] { SKColor.Parse("#DE0469"), SKColors.Transparent },
                               new float[] { 0, 1 },
                               SKShaderTileMode.Repeat);

            path.ArcTo(arcRect, 45, arcLength, true);
            canvas.DrawPath(path, strokePaint);

            canvas.Translate(info.Width / 2, info.Height / 2);
            canvas.Scale(info.Width / 200f);

            canvas.Save();
            canvas.RotateDegrees(240);
            canvas.DrawCircle(0, -75, 2, dotPaint);
            canvas.Restore();

            DateTime dateTime = DateTime.Now;

            //Draw hour hand
            canvas.Save();
            canvas.RotateDegrees(30 * dateTime.Hour + dateTime.Minute / 2f);
            canvas.DrawLine(0, 5, 0, -60, hrPaint);
            canvas.Restore();

            //Draw minute hand
            canvas.Save();
            canvas.RotateDegrees(6 * dateTime.Minute + dateTime.Second / 10f);
            canvas.DrawLine(0, 10, 0, -90, minPaint);
            canvas.Restore();

            canvas.DrawCircle(0, 0, 5, dotPaint);

            secondsTxt.Text = dateTime.Second.ToString("00");
        }

        private void Stop(object sender, EventArgs e)
        {
            Hour = Hour;
            Minute = Minute;
            Second = Second;
            MessagingCenter.Send<object, string>(this, "AlarmReceiver", "Stop");
            LayoutStop.IsVisible = false;
        }
    }
}
