using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.Core.App;
using BNet.Mobile.SMS.Services.ForegroundServices;
using BNet.Mobile.SMS.Services.TempDataServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BNet.Mobile.SMS.Services.FloatingServices
{
    [Service]
    public class FloatingIcon : Service
    {
        IWindowManager windowManager;
        View iconView;

        public override void OnCreate()
        {
            base.OnCreate();
            //StartForeground(1, CreateNotification());
            windowManager = GetSystemService(WindowService).JavaCast<IWindowManager>();
            var imageView = new ImageView(this);
            imageView.SetImageResource(Resource.Drawable.icon);
            imageView.SetPadding(0, 0, 0, 0);
            imageView.Alpha = 0.85f;

            int size = DpToPx(0);// Make it Invisible

            var layoutParams = new WindowManagerLayoutParams(size, size,
                Build.VERSION.SdkInt >= BuildVersionCodes.O ? WindowManagerTypes.ApplicationOverlay : WindowManagerTypes.Phone,
                WindowManagerFlags.NotFocusable | WindowManagerFlags.LayoutNoLimits, Format.Translucent);
            layoutParams.Gravity = GravityFlags.Left | GravityFlags.CenterVertical;
            imageView.Click += (s, e) => RestoreApp();
            iconView = imageView;
            windowManager.AddView(iconView, layoutParams);
        }

        void RestoreApp()
        {
            var intent = new Intent(this, typeof(MainActivity));
            intent.AddFlags(ActivityFlags.NewTask);
            StartActivity(intent);
            StopService();
            StopSelf();
        }
        public static void StartService()
        {
            var intent = new Intent(Android.App.Application.Context, typeof(FloatingIcon));
            Android.App.Application.Context.StartService(intent);
            Properties.SetIsBackgroundService(true);
        }

        public static void StopService()
        {
            var intent = new Intent(Android.App.Application.Context, typeof(FloatingIcon));
            Android.App.Application.Context.StopService(intent);
            Properties.SetIsBackgroundService(false);
           
        }
        public override void OnDestroy()
        {
            try
            {
                base.OnDestroy();
                windowManager.RemoveView(iconView);
            }
            catch { }
        }

        int DpToPx(int dp) => (int)(dp * Resources.DisplayMetrics.Density);
        public override IBinder OnBind(Intent intent) => null;


        //Notification CreateNotification()
        //{
        //    const string channelId = "floating_icon";

        //    if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
        //    {
        //        var channel = new NotificationChannel(channelId,"Floating Icon",NotificationImportance.Low);
        //        GetSystemService(NotificationService).JavaCast<NotificationManager>().CreateNotificationChannel(channel);
        //    }

        //    // ✅ Handle PendingIntent flags for Android 12+
        //    PendingIntentFlags pendingIntentFlags = PendingIntentFlags.UpdateCurrent;
        //    if (Build.VERSION.SdkInt >= BuildVersionCodes.R)
        //    {
        //        pendingIntentFlags |= PendingIntentFlags.Immutable;
        //    }

        //    var pendingIntent = PendingIntent.GetActivity(
        //        this,
        //        0,
        //        new Intent(this, typeof(MainActivity)),
        //        pendingIntentFlags
        //    );

        //    return new NotificationCompat.Builder(this, channelId)
        //    .SetContentTitle("Background Service")
        //    .SetContentText("SMS Integration is running")
        //    .SetPriority((int)NotificationPriority.Max)
        //    .SetOngoing(true)
        //    .SetAutoCancel(false)
        //    .SetSmallIcon(Resource.Drawable.icon)
        //    .SetContentIntent(pendingIntent)
        //    .Build();
        //}
    }
}