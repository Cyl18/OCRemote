using GammaLibrary.Extensions;
using System.Collections.Concurrent;
using System.Text.Json;

namespace OCRemoteServer
{
    public class RemoteManager
    {
        internal static ConcurrentQueue<(int, string)> commandQueue = new();
        static Dictionary<int, TaskCompletionSource<string>> handlers = new();
        static Dictionary<int, DateTime> times = new();
        static int commandId = 0;
        static readonly object locker = new();

        public static async Task SyncCode()
        {
            var dir = "../OC";
            foreach (var file in Directory.GetFiles(dir))
            {
                var text = File.ReadAllText(file);
                await RemoteManager.Request(text);
            }
        }

        static RemoteManager()
        {
            Task.Factory.StartNew(async () =>
            {
                try
                {
                    await Request("return 1").ConfigureAwait(false);
                    OnInitialized();
                }
                catch (Exception e)
                {
                    Console.WriteLine("OnInitialized failed: " + e);
                }

                while (true)
                {
                    lock (locker)
                    {
                        foreach (var (i, time) in times.ToList())
                        {
                            if (DateTime.Now - time > TimeSpan.FromSeconds(30))
                            {
                                handlers[i].SetCanceled();
                                handlers.Remove(i);
                            }
                        }
                    }

                    await Task.Delay(TimeSpan.FromSeconds(1));
                }
            }, TaskCreationOptions.HideScheduler | TaskCreationOptions.LongRunning);

          

        }

        public static async void OnInitialized()
        {
            await SyncCode();
            EnergyStationManager.Init();
            Task.Factory.StartNew(async () =>
            {
                // Chtholly Plus Gaming PRO MAX Ti Ultra Super 2.0

                var baseComponents = (await Get().ConfigureAwait(false)).ToHashSet();
                while (true)
                {
                    try
                    {
                        var setBase = await Get().ConfigureAwait(false);
                        var set = setBase.ToHashSet();
                        set.ExceptWith(baseComponents);
                        foreach (var (address, name) in set)
                        {
                            Console.WriteLine($"Component Added: [{address}]: {name}");
                        }

                        baseComponents = setBase;
                        await Task.Delay(TimeSpan.FromSeconds(10));

                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }

                async Task<HashSet<(string address, string name)>> Get()
                {
                    var componentJson = await RemoteManager.Request("return require('component').list()").ConfigureAwait(false);
                    var components = JsonDocument.Parse(componentJson)
                        .RootElement.EnumerateObject().Select(x => (address: x.Name, name: x.Value.GetString()!)).ToArray();
                    return components.ToHashSet();
                }

            }, TaskCreationOptions.HideScheduler | TaskCreationOptions.LongRunning);
        }

        public static Task<string> Request(string lua)
        {
            lock (locker)
            {
                commandId = (commandId + 1) % 1000;
                //Console.WriteLine($"{commandId}: {lua}");
                var cId = commandId;
                commandQueue.Enqueue((cId, lua));
                var tcs = new TaskCompletionSource<string>();
                handlers[cId] = tcs;
                return tcs.Task;
            }
        }

        public static void Callback(string response)
        {
            var list = JsonSerializer.Deserialize<Dictionary<int, string>>(response);
            lock (locker)
            {
                foreach (var (k, v) in list)
                {
                    var cId = k;
                    var reply = v;
                    var handler = handlers[cId];
                    handlers[cId] = null;
                    if (handler == null) continue;
                    
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

}
