using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Media;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;
using Xamarin.Forms;

namespace AlarmApp.Droid
{
    [BroadcastReceiver]
    public class MyReceiver : BroadcastReceiver
    {
        const string channelId = "default";
        const string channelName = "Default";
        const string channelDescription = "The default channel for notifications.";

        int messageId = 0;
        NotificationManager manager;

        public void Show(Context context, string title, string message)
        {
            Intent intent = new Intent(context, typeof(MainActivity));
            PendingIntent pendingIntent = PendingIntent.GetActivity(context, 1, intent, PendingIntentFlags.UpdateCurrent);
            MainActivity.Requests.Add("Stop");
            
            NotificationCompat.Builder builder = new NotificationCompat.Builder(context, channelId)
                .SetContentTitle(title)
                .SetContentIntent(pendingIntent)
                .SetContentText(message)
                .SetSmallIcon(Resource.Drawable.navigation_empty_icon)
                .SetDefaults((int)NotificationDefaults.Vibrate);

            Notification notification = builder.Build();
            manager.Notify(messageId++, notification);
        }

        bool CreateNotificationChannel(Context context)
        {
            manager = (NotificationManager)context.GetSystemService("notification");

            if (manager == null)
                return false;

            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                var channelNameJava = new Java.Lang.String(channelName);
                var channel = new NotificationChannel(channelId, channelNameJava, NotificationImportance.Default)
                {
                    Description = channelDescription
                };
                manager.CreateNotificationChannel(channel);
                return true;
            }

            return false;
        }

        public override void OnReceive(Context context, Intent intent)
        {
            MessagingCenter.Send<object, string>(context, "AlarmUI", "VisibleStop");
            Toast.MakeText(context, "Будильник!", ToastLength.Short).Show();

            var notifUri = RingtoneManager.GetDefaultUri(RingtoneType.Alarm);
            var ringtone = RingtoneManager.GetRingtone(context, notifUri);

            if (ringtone == null)
            {
                notifUri = RingtoneManager.GetDefaultUri(RingtoneType.Ringtone);
                ringtone = RingtoneManager.GetRingtone(context, notifUri);
            }

            ringtone?.Play();

            MessagingCenter.Subscribe<object, string>(context, "AlarmReceiver",
            (sender, arg) =>
            {
                if (arg == "Stop")
                    ringtone?.Stop();

                if (arg == "Status")
                    if ((ringtone?.IsPlaying is null ? false : ringtone?.IsPlaying) == true)
                        MessagingCenter.Send<object, string>(this, "AlarmUI", "VisibleStop");
            });

            try
            {
                if (CreateNotificationChannel(context))
                    Show(context, "Будильник", "Нажмите чтобы перейти в приложение");
            }
            catch
            {
                Intent intentt = Android.App.Application.Context.PackageManager.GetLaunchIntentForPackage("com.c4ke.alarmapp");
                intentt.SetFlags(ActivityFlags.NewTask);
                Android.App.Application.Context.StartActivity(intentt);
            }
        }
    }
}