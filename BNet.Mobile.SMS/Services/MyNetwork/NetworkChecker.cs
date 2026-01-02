using Android.App;
using Android.Content;
using Android.Net;
using Android.Net.Wifi;
using Android.OS;
using Android.Runtime;
using Android.Text.Format;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BNet.Mobile.SMS.Services.MyNetwork
{
    internal class NetworkChecker
    {
        public int PortNumber()
        {
            return 8030;
        }
        public string IPAddress()
        {
            WifiManager wifiMgr = (WifiManager)Android.App.Application.Context.GetSystemService(Context.WifiService);
            WifiInfo wifiInfo = wifiMgr.ConnectionInfo;
            int ip = wifiInfo.IpAddress;
            return Formatter.FormatIpAddress(ip);
        }
        public bool HasInternet()
        {
            bool isMobileDataEnabled = IsMobileDataEnabled(Android.App.Application.Context);
            bool isWifiEnabled = IsWifiEnabled(Android.App.Application.Context);
            if (isWifiEnabled)
            {
                return true;
            }
            else if (isMobileDataEnabled)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public string LocalConnection()
        {
            return "http://" + IPAddress() + ":" + PortNumber();
        }

        bool IsMobileDataEnabled(Context context)
        {
            try
            {
                ConnectivityManager connectivityManager = (ConnectivityManager)context.GetSystemService(Context.ConnectivityService);
                NetworkInfo mobileInfo = connectivityManager.GetNetworkInfo(ConnectivityType.Mobile);

                return mobileInfo != null && mobileInfo.IsConnected;
            }
            catch
            {
                return false;
            }
        }
        bool IsWifiEnabled(Context context)
        {
            try
            {
                WifiManager wifiManager = (WifiManager)context.GetSystemService(Context.WifiService);
                return wifiManager != null && wifiManager.IsWifiEnabled;
            }
            catch
            {
                return false;
            }
        }
    }
}