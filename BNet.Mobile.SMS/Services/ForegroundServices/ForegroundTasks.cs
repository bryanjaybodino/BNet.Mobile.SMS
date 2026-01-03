using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Service.QuickSettings;
using Android.Views;
using Android.Widget;
using AndroidX.Core.App;
using BNet.Mobile.SMS.Services.FloatingServices;
using BNet.Mobile.SMS.Services.HttpServices;
using BNet.Mobile.SMS.Services.ServiceBus;
using BNet.Mobile.SMS.Services.TempDataServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using static Android.OS.PowerManager;


namespace BNet.Mobile.SMS.Services.ForegroundServices
{

    [Service]
    public class ForegroundTasks : Service
    {
        public override IBinder OnBind(Intent intent)
        {
            throw new NotImplementedException();
        }

        public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId)
        {
            string channelId = "ForeGroundServiceChannel";
            var notificationManager = (NotificationManager)GetSystemService(NotificationService);

            // ✅ Create notification channel only on Android 8.0+
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                var channel = new NotificationChannel(channelId, "Foreground Service Channel", NotificationImportance.Min)
                {
                    LockscreenVisibility = NotificationVisibility.Private,
                };

                channel.SetShowBadge(false);
                notificationManager.CreateNotificationChannel(channel);
            }

            // ✅ Handle PendingIntent flags for Android 12+
            PendingIntentFlags pendingIntentFlags = PendingIntentFlags.UpdateCurrent;
            if (Build.VERSION.SdkInt >= BuildVersionCodes.R)
            {
                pendingIntentFlags |= PendingIntentFlags.Immutable;
            }

            var pendingIntent = PendingIntent.GetActivity(
                this,
                0,
                new Intent(this, typeof(MainActivity)),
                pendingIntentFlags
            );

            var notificationBuilder = new NotificationCompat.Builder(this, channelId)
                .SetContentTitle("Background Service")
                .SetContentText("SMS Integration is running")
                .SetPriority((int)NotificationPriority.Min)
                .SetOngoing(true)
                .SetAutoCancel(false)
                .SetOnlyAlertOnce(true)
                .SetSmallIcon(Resource.Drawable.icon)
                .SetCategory(Notification.CategoryService)
                .SetContentIntent(pendingIntent);

            var notification = notificationBuilder.Build();
            StartForeground(1002, notification);
            return StartCommandResult.Sticky;
        }

        public static void StartService()
        {
            var intent = new Intent(Android.App.Application.Context, typeof(ForegroundTasks));
            Android.App.Application.Context.StartService(intent);
            Properties.SetIsBackgroundService(true);
        }

        public static void StopService()
        {
            var intent = new Intent(Android.App.Application.Context, typeof(ForegroundTasks));
            Android.App.Application.Context.StopService(intent);
            Properties.SetIsBackgroundService(false);

        }
        public override void OnCreate()
        {
            WebServer.Start();
            base.OnCreate();
        }
        public override void OnDestroy()
        {
            var notificationManager = (NotificationManager)GetSystemService(NotificationService);
            notificationManager.Cancel(1002); // SAME ID as StartForeground
            StopForeground(true);
            Properties.SetIsBackgroundService(false);
            base.OnDestroy();
        }
    }
}
