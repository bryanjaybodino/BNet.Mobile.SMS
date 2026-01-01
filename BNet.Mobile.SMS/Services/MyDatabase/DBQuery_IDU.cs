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

namespace BNet.Mobile.SMS.Services.MyDatabase
{
    public class DBQuery_IDU
    {
        public bool isSuccess = false;
        public string HtmlEncode(string value)
        {
            return value.Replace("&NBSP;", "").Replace("&AMP;AMP;", "&").Replace("&AMP;", "&").Replace("&LT;", "<").Replace("&GT;", ">").Replace("&QUOT;", "ˈ").Replace("&APOS;", "ˈ").Replace(";", "");
        }
        public void Command(string Query, string YourMessage)
        {
            isSuccess = false;
            MySQLService dBConnection = new MySQLService();
            try
            {
                dBConnection.CheckConnection();

                if (dBConnection.DBSetCommand(HtmlEncode(Query)))
                {
                    dBConnection.DBReader = dBConnection.DBCommand.ExecuteReader();
                    dBConnection.MyConnection.Close();
                    isSuccess = true; ;
                }
            }
            catch
            {
                isSuccess = false;
                if (dBConnection.MyConnection.State == System.Data.ConnectionState.Open)
                {
                    dBConnection.MyConnection.Close();
                    dBConnection.MyConnection.Dispose();
                }
            }
        }
    }
}