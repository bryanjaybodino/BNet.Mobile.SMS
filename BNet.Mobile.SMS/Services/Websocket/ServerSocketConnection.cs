using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using BNet.Mobile.SMS.Services.MyNetwork;
using BNet.Mobile.SMS.Services.Websocket.Models;
using BNet.WebSocket.Server;
using Java.Net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BNet.Mobile.SMS.Services.Websocket
{
    internal class ServerSocketConnection
    {
        public class ServerSocket
        {
            public static Connection connection { get; set; }
        }
        NetworkChecker INetworkChecker = new NetworkChecker();
        private async Task KeepAlive()
        {
            try
            {
                ServerSocket.connection = new Connection(INetworkChecker.PortNumber());
                ServerSocket.connection.OnReceived += Connection_OnReceived;
                await ServerSocket.connection.StartAsync();
            }
            catch (Exception ex)
            {
                await ServerSocket.connection.StopAsync();
                // Log the exception
                System.Diagnostics.Debug.WriteLine($"Exception in KeepAlive: {ex.Message}");
            }
        }

        private async void Connection_OnReceived(object sender, EventHandlers.ReceivedEventArgs e)
        {
            try
            {
                string message = e.Message;
                if (message.Replace(" ", "") != "")
                {
                    SendPayload sms = JsonConvert.DeserializeObject<SendPayload>(message);
                    await ISendQueue.SetQueueAsync(message);
                }
            }
            catch (Exception ex)
            {
                // Log the exception
                System.Diagnostics.Debug.WriteLine($"Exception in Connection_OnReceived: {ex.Message}");
            }
        }
    }
}