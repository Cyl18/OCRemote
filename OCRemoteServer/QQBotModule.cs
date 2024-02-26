using Sisters.WudiLib;
using Sisters.WudiLib.WebSocket;

namespace OCRemoteServer
{
    public class QQBotModule
    {
        static HttpApiClient c;
        public static async Task Init()
        {
            var httpApi = new HttpApiClient();
            httpApi.ApiAddress = "http://192.168.1.5:5700/";
            c = httpApi;
        }

        static List<DateTime> sentTimes = new List<DateTime>();
        public static async Task Push(string text, DateTime planStartTime)
        {
            if (sentTimes.Contains(planStartTime))
            {
                return;
            }

            sentTimes.Add(planStartTime);
            if (Directory.Exists("C:\\Users\\cyl18"))
                await c.SendGroupMessageAsync(903711172, new SendingMessage(text));
        }
    }
}
