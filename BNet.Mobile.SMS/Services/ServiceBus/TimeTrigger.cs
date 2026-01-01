using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using BNet.Mobile.SMS.Services.SmsService;
using BNet.Mobile.SMS.Services.TempData;
using BNet.Mobile.SMS.Services.Websocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BNet.Mobile.SMS.Services.ServiceBus
{
    internal class TimeTrigger
    {
        SendQueue ISendQueue = new SendQueue();
        SaveQueue ISaveQueue = new SaveQueue();
        SendMessage ISendMessage = new SendMessage();
        SaveMessage ISaveMessage = new SaveMessage();


        int Send_TimerTrigger = 0;
        int Save_TimerTrigger = 0;

        bool isRunning = true;
        //static ClientSocketConnection clientSocketConnection = new ClientSocketConnection();
        static ServerSocketConnection serverSocketConnection = new ServerSocketConnection();
        public async void ProcessSendingMessages()
        {
            int countSent = await ISendQueue.CountAsync();
            if (countSent > 0)
            {
                if (Send_TimerTrigger >= 10)
                {
                    Send_TimerTrigger = 0;
                    string Message = await ISendQueue.PeekAsync();
                    ISendMessage.Send(Message);
                }
                else
                {
                    Send_TimerTrigger++;
                }
            }
        }

        public async void ProcessSavingMessages()
        {
            int countSave = await ISaveQueue.CountAsync();
            if (countSave > 0)
            {
                if (Save_TimerTrigger >= 10)
                {
                    Save_TimerTrigger = 0;
                    //ISaveMessage.Saved();
                }
                else
                {
                    Save_TimerTrigger++;
                }
            }
        }

    }
}