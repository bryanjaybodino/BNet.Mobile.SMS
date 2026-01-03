using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using BNet.Mobile.SMS.Services.FloatingServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BNet.Mobile.SMS.Services.ExitAppServices
{
    public class RecentTasks
    {
        public static void RemoveAppFromRecentTasks()
        {
            // Get current activity safely
            var activity = Xamarin.Essentials.Platform.CurrentActivity;
            if (activity == null)
            {
                // Could not get current activity, maybe log or silently return
                return;
            }

            if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop) // API 21+
            {
                // Remove from recent tasks
                activity.FinishAndRemoveTask();
            }
            else
            {
                // For older Android versions, just finish affinity
                activity.FinishAffinity();
            }
        }
        public static void MoveTaskToBack()
        {
            var activity = Xamarin.Essentials.Platform.CurrentActivity;
            activity?.MoveTaskToBack(true); // Sends app to background without killing service
        }
    }
}