using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace BNet.Mobile.SMS.Services.TempData
{
    internal class SaveQueue
    {
        private static async Task<Queue<string>> GetQueueAsync()
        {
            var messagesList = new Queue<string>();
            try
            {
                var jsonString = await SecureStorage.GetAsync("SAVING_SMS_QUEUE");
                if (!string.IsNullOrEmpty(jsonString))
                {
                    messagesList = JsonConvert.DeserializeObject<Queue<string>>(jsonString) ?? new Queue<string>();
                }
            }
            catch (Exception ex)
            {
            }
            return messagesList;
        }

        private static async Task SaveQueueAsync(Queue<string> messagesList)
        {
            try
            {
                var jsonString = JsonConvert.SerializeObject(messagesList);
                await SecureStorage.SetAsync("SAVING_SMS_QUEUE", jsonString);
            }
            catch (Exception ex)
            {
            }
        }

        public static async Task<string> PeekAsync()
        {
            var queue = await GetQueueAsync();
            return queue.Count > 0 ? queue.Peek() : null;
        }

        public static async Task<int> CountAsync()
        {
            var queue = await GetQueueAsync();
            return queue.Count;
        }

        public static async Task SetQueueAsync(string message)
        {
            var queue = await GetQueueAsync();
            queue.Enqueue(message);
            await SaveQueueAsync(queue);
        }

        public static async Task DequeueAsync()
        {
            var queue = await GetQueueAsync();
            if (queue.Count > 0)
            {
                queue.Dequeue();
                await SaveQueueAsync(queue);
            }
        }
    }
}
