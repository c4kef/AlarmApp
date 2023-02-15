using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Content;
using Java.Lang;
using Android.Provider;
using static Android.App.AlarmManager;
using Xamarin.Forms;
using Android.Support.V4.App;
using Android.Media;
using Android.Graphics;
using System.Collections.Generic;
using System.Runtime.Remoting.Contexts;

namespace AlarmApp.Droid
{
    [Activity(Label = "AlarmApp", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        public const string TitleKey = "title";
        public const string MessageKey = "message";
        public static List<string> Requests;
        PendingIntent pi;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            if (Requests != null)
                foreach (var request in Requests)
                { }
            
            pi = PendingIntent.GetBroadcast(this, 0, new Intent(this, typeof(MyReceiver)), 0);
            MessagingCenter.Subscribe<object, string>(this, "AlarmSetup",
            (sender, arg) =>
            {
                var data = arg.Split("|");
                if (data[0] == "Start")
                    Start(DateTime.Parse(data[1]));
            });

            Requests = new List<string>();
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;
            base.OnCreate(savedInstanceState);

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            LoadApplication(new App());
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            var activity = Forms.Context as Activity;
            Intent i = new Intent(activity, typeof(MainActivity));
            string title = data.GetStringExtra(TitleKey);
            string message = data.GetStringExtra(MessageKey);


            Toast.MakeText(this, title, ToastLength.Short).Show();
            Toast.MakeText(this, message, ToastLength.Short).Show();

            base.OnActivityResult(requestCode, resultCode, data);
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        private void Start(DateTime date)
        {
            AlarmManager alarmManager = (AlarmManager)GetSystemService(AlarmService);
            alarmManager.Cancel(pi);

            var alarmInfo = new AlarmClockInfo((long)date.ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds, pi);

            alarmManager.SetAlarmClock(alarmInfo, pi);
            Toast.MakeText(this, $"Будильник проиграет {(date.Day == DateTime.Now.Day ? "сегодня" : "завтра")} в {date.Hour.ToString("#00")}:{date.Minute.ToString("#00")}:{date.Second.ToString("#00")}", ToastLength.Short).Show();
        }
    }
}