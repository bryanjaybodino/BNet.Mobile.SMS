using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using BNet.Mobile.SMS.Services.ForegroundServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BNet.Mobile.SMS.Services.BroadCastReceiverServices
{
    [BroadcastReceiver(Enabled = true, Exported = false)]
    [IntentFilter(new[] { Intent.ActionBootCompleted })]
    public class RebootReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            if (intent.Action == Intent.ActionBootCompleted)
            {
                try
                {
                    // Toast to indicate boot completed (optional)
                    Toast.MakeText(context, "Device Rebooted - Starting Service", ToastLength.Short).Show();

                    var serviceIntent = new Intent(context, typeof(ForegroundTasks));

                    if (Build.VERSION.SdkInt >= BuildVersionCodes.O) // Android 8.0+
                    {
                        context.StartForegroundService(serviceIntent);
                    }
                    else
                    {
                        context.StartService(serviceIntent);
                    }
                }
                catch (Exception ex)
                {
                    // Log or handle exceptions if necessary
                    Toast.MakeText(context, $"Error starting service: {ex.Message}", ToastLength.Long).Show();
                }
            }
        }
    }
}