using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using BNet.Mobile.SMS.Services.TempData;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

namespace BNet.Mobile.SMS.Services.MyDatabase
{
    internal class MySQLService
    {
        //server=localhost;user id=root;database=bnet_sms;Connect Timeout=2147483
        public MySqlConnection MyConnection = new MySqlConnection();
        public MySqlCommand DBCommand = new MySqlCommand();
        public MySqlDataReader DBReader;
        public MySqlDataAdapter DBAdapter = new MySqlDataAdapter();

        public static bool IsIPAddress(string host)
        {
            return IPAddress.TryParse(host, out _);
        }
        public static bool PingIpAddress(string ipAddress)
        {
            try
            {
                using (var ping = new Ping())
                {
                    var pingReply = ping.Send(ipAddress, 10); // Wait for 1 second (1000 milliseconds)
                    return pingReply.Status == IPStatus.Success;
                }
            }
            catch
            {
                return false;
            }
        }
        public static bool IsValidHost(string host)
        {
            if (IPAddress.TryParse(host, out _))
            {
                return true;
            }
            string domainNamePattern = @"^(?!-)[A-Za-z0-9-]{1,63}(?<!-)$";
            string domainNameRegex = @"^([a-zA-Z0-9-]{1,63}\.)+[a-zA-Z]{2,63}$";
            return Regex.IsMatch(host, domainNameRegex);
        }
        public static bool IsHostActive(string host, string port = "3306")
        {
            try
            {
                IPHostEntry hostEntry = Dns.GetHostEntry(host);
                using (var tcpClient = new TcpClient())
                {
                    tcpClient.Connect(host, Convert.ToInt32(port));
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
        public bool isConnected()
        {
            try
            {
                string server = ServerName();
                string port = PortNumber();

                if (IsIPAddress(server))
                {

                    if (PingIpAddress(server))
                    {
                        using (var connection = new MySqlConnection(Properties.DatabaseConnection()))
                        {
                            connection.Open(); // Attempt to open the connection
                        }
                        return true;
                    }
                    else
                    {
                        return false;
                    }


                }
                else if (IsValidHost(server))
                {

                    if (IsHostActive(server, port))
                    {
                        using (var connection = new MySqlConnection(Properties.DatabaseConnection()))
                        {
                            connection.Open(); // Attempt to open the connection
                        }
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }
        public string DatabaseName()
        {
            MySqlConnectionStringBuilder builder = new MySqlConnectionStringBuilder(Properties.DatabaseConnection());
            return builder.Database;
        }
        public string ServerName()
        {
            MySqlConnectionStringBuilder builder = new MySqlConnectionStringBuilder(Properties.DatabaseConnection());
            return builder.Server;
        }
        public string PortNumber()
        {
            MySqlConnectionStringBuilder builder = new MySqlConnectionStringBuilder(Properties.DatabaseConnection());
            return builder.Port.ToString();
        }
        public bool DBSetCommand(string MyCommand)
        {
            if (MyConnection.State == System.Data.ConnectionState.Open)
            {
                DBCommand = new MySqlCommand(MyCommand, MyConnection);
                DBAdapter = new MySqlDataAdapter(DBCommand);
                return true;
            }
            else
            {
                return false;
            }
        }
        public void CheckConnection()//Helps to avoid terminating the system/
        {
            if (MyConnection.State == System.Data.ConnectionState.Open)
            {
                MyConnection.Close();
            }
            else
            {
                string server = ServerName();
                string port = PortNumber();

                if (IsIPAddress(server))
                {
                    if (PingIpAddress(server))
                    {
                        MyConnection = new MySqlConnection(Properties.DatabaseConnection());
                        MyConnection.Open();
                    }
                }
                else if (IsValidHost(server))
                {
                    if (IsHostActive(server, port))
                    {
                        MyConnection = new MySqlConnection(Properties.DatabaseConnection());
                        MyConnection.Open();
                    }
                }

            }
        }
    }
}