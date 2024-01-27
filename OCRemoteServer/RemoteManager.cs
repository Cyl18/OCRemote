using System.Collections.Concurrent;

namespace OCRemoteServer
{
    public class RemoteManager
    {
        internal static ConcurrentQueue<string> commandQueue = new();
        static Dictionary<string, TaskCompletionSource<string>> handlers = new();
        static Dictionary<string, DateTime> times = new();
        static int commandId = 0;
        static readonly object locker = new();


        static RemoteManager()
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    lock (locker)
                    {
                        foreach (var (i, time) in times.ToList())
                        {
                            if (DateTime.Now - time > TimeSpan.FromSeconds(60))
                            {
                                handlers[i].SetCanceled();
                                handlers.Remove(i);
                            }
                        }
                    }

                    await Task.Delay(TimeSpan.FromSeconds(1));
                }
            });
        }


        public static Task<string> Request(string lua)
        {
            lock (locker)
            {
                commandId = (commandId + 1) % 1000;
                var cId = commandId.ToString("D3");
                commandQueue.Enqueue(cId + lua);
                var tcs = new TaskCompletionSource<string>();
                handlers[cId] = tcs;
                return tcs.Task;
            }
        }

        public static void Callback(string response)
        {
            lock (locker)
            {
                var cId = response.Substring(0, 3);
                var reply = response.Substring(3);
                var handler = handlers[cId];
                handlers.Remove(cId);
                if (reply.StartsWith("e$"))
                {
                    var error = response.Substring(2);
                    handler.SetException(new Exception(error));

                }
                else
                {
                    handler.SetResult(reply);

                }
            }
        }

    }

}
