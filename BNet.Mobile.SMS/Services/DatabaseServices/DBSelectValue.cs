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

namespace BNet.Mobile.SMS.Services.DatabaseServices
{
    public class DBSelectValue
    {
        public string getValue { get; set; } = "0";
        public void Command(string Query)
        {
            MySQLService dBConnection = new MySQLService();
            try
            {
                dBConnection.CheckConnection();
                if (dBConnection.DBSetCommand(Query + " LIMIT 1"))
                {
                    if (Query.ToUpper().Contains("COUNT"))
                    {
                        getValue = Convert.ToDouble(dBConnection.DBCommand.ExecuteScalar()).ToString("N0");
                    }
                    else
                    {
                        getValue = Convert.ToDouble(dBConnection.DBCommand.ExecuteScalar()).ToString("N2").Replace(",", "");
                    }
                    dBConnection.MyConnection.Close();
                }


            }
            catch
            {
                if (dBConnection.MyConnection.State == System.Data.ConnectionState.Open)
                {
                    dBConnection.MyConnection.Close();
                    dBConnection.MyConnection.Dispose();
                }
            }
        }
    }
}