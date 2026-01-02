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
using Xamarin.Essentials;

namespace BNet.Mobile.SMS.Services.TempData
{
    internal class Properties
    {

        public string DatabaseConnection()
        {
            try
            {
                if (SecureStorage.GetAsync("DatabaseConnection") != null)
                {
                    using (var data = SecureStorage.GetAsync("DatabaseConnection"))
                    {
                        data.Wait();
                        return data.Result;

                    }
                }
                else
                {
                    return "";
                }
            }
            catch { return ""; }

        }
        public bool IsMySQLEnabled()
        {
            try
            {
                if (SecureStorage.GetAsync("IsMySQLEnabled") != null)
                {
                    using (var data = SecureStorage.GetAsync("IsMySQLEnabled"))
                    {
                        data.Wait();
                        if (data.Result.ToUpper() == "TRUE")
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    return false;
                }
            }
            catch { return false; }
        }
        public void SetDatabaseConnection(string data)
        {
            try
            {
                SecureStorage.SetAsync("DatabaseConnection", data);
            }
            catch { }
        }
        public void SetIsMySQLEnabled(bool data)
        {
            try
            {
                SecureStorage.SetAsync("IsMySQLEnabled", data.ToString());
            }
            catch { }
        }
    }
}