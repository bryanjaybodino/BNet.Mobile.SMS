using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BNet.Mobile.SMS.Services.BatteryOptimizationServices
{
    internal class OptimizeBattery
    {
        //var batteryHelper = new OptimizeBattery(this);
        //batteryHelper.RequestIgnoreBatteryOptimization();



        private readonly Context _context;

        public OptimizeBattery(Context context)
        {
            _context = context;
        }

        public void RequestIgnoreBatteryOptimization()
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.M) // Android 6+
            {
                var pm = (PowerManager)_context.GetSystemService(Context.PowerService);
                if (!pm.IsIgnoringBatteryOptimizations(_context.PackageName))
                {
                    Intent intent = new Intent();
                    intent.SetAction(Android.Provider.Settings.ActionIgnoreBatteryOptimizationSettings);

                    // If context is not an Activity, need to set FLAG_ACTIVITY_NEW_TASK
                    if (!(_context is Android.App.Activity))
                        intent.SetFlags(ActivityFlags.NewTask);

                    _context.StartActivity(intent);
                }
            }
        }
    }
}