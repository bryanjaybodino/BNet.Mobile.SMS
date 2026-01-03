using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.Core.App;
using BNet.Mobile.SMS.Services.ForegroundServices;
using BNet.Mobile.SMS.Services.HttpServices;
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
        PowerManager.WakeLock wakeLock;
        public override void OnCreate()
        {
            base.OnCreate();
            var pm = (PowerManager)GetSystemService(PowerService);
            wakeLock = pm.NewWakeLock(
                WakeLockFlags.Partial,
                "BNet:WebServerWakeLock"
            );
            wakeLock.Acquire();
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
            ForegroundTasks.StartService();
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
            ForegroundTasks.StopService();
            Properties.SetIsBackgroundService(false);

        }
        public override void OnDestroy()
        {
            try
            {
                windowManager.RemoveView(iconView);
                if (wakeLock?.IsHeld == true)
                {
                    wakeLock.Release();
                }

                base.OnDestroy();
            }
            catch { }
        }

        int DpToPx(int dp) => (int)(dp * Resources.DisplayMetrics.Density);
        public override IBinder OnBind(Intent intent) => null;
    }
}