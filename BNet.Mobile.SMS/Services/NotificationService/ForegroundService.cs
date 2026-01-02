using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Service.QuickSettings;
using Android.Views;
using Android.Widget;
using AndroidX.Core.App;
using BNet.Mobile.SMS.Services.APIService;
using BNet.Mobile.SMS.Services.ServiceBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace BNet.Mobile.SMS.Services.NotificationService
{

    [Service]
    public class ForegroundService : Service
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
                var channel = new NotificationChannel(channelId, "Foreground Service Channel", NotificationImportance.Low)
                {
                    LockscreenVisibility = NotificationVisibility.Private
                };
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
                .SetPriority((int)NotificationPriority.Max)
                .SetOngoing(true)
                .SetAutoCancel(false)
                .SetSmallIcon(Resource.Mipmap.ic_launcher_foreground)
                .SetContentIntent(pendingIntent);

            var notification = notificationBuilder.Build();
            StartForeground(1001, notification);



            TimeTrigger timeTrigger = new TimeTrigger(); 
            // create a timer
            Timer timer = new Timer(500); // 1000ms = 1 second
            timer.Elapsed += async (sender, e) =>
            {
                //Service Bus Timer
                timeTrigger.ProcessSendingMessages();
                timeTrigger.ProcessSavingMessages();
            };
            timer.Start();


            //API Server For Client Connection
            APIServer aPIServer = new APIServer();
            aPIServer.Start();

            return StartCommandResult.Sticky;
        }

        public void StartMyForeGroundService()
        {
            var intent = new Intent(Android.App.Application.Context, typeof(ForegroundService));

            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                // ✅ Android 8.0+ requires StartForegroundService
                Android.App.Application.Context.StartForegroundService(intent);
            }
            else
            {
                // ✅ Older Android uses StartService
                Android.App.Application.Context.StartService(intent);
            }
        }

        public void StopMyForeGroundService()
        {
            var intent = new Intent(Android.App.Application.Context, typeof(ForegroundService));
            Android.App.Application.Context.StopService(intent);
        }

        static bool ServiceRunning = false;

        public bool IsRunning()
        {
            return ServiceRunning;
        }

        public override void OnCreate()
        {
            base.OnCreate();
            ServiceRunning = true;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            ServiceRunning = false;
        }
    }
}
