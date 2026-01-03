using Android;
using Android.App;
using Android.Content.PM;
using Android.OS;
using AndroidX.Core.App; // For ActivityCompat
using AndroidX.Core.Content; // For ContextCompat

namespace BNet.Mobile.SMS.Services.PermissionServices
{
    internal class MyPermission
    {
        private readonly Activity _activity;
        private const int RequestSmsPermissionsId = 1001;

        // List of SMS permissions
        private readonly string[] SmsPermissions =
        {
            Manifest.Permission.SendSms,
            Manifest.Permission.ReceiveSms,
            Manifest.Permission.ReadSms,
            Manifest.Permission.PostNotifications //"android.permission.POST_NOTIFICATIONS"
        };

        public MyPermission(Activity activity)
        {
            _activity = activity;
        }

        public bool EnsurePermissions()
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
            {
                bool allGranted = true;

                foreach (var permission in SmsPermissions)
                {
                    if (ContextCompat.CheckSelfPermission(_activity, permission) != Permission.Granted)
                    {
                        allGranted = false;
                        break;
                    }
                }

                if (!allGranted)
                {
                    ActivityCompat.RequestPermissions(_activity, SmsPermissions, RequestSmsPermissionsId);
                    return false; // Permissions not yet granted, request sent
                }
            }

            return true; // All permissions granted
        }
    }
}
