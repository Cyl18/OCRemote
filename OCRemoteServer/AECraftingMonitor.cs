using System.Globalization;
using GammaLibrary.Extensions;
using System.Text.Json;
using Humanizer;
using Humanizer.Localisation;
using System;

namespace OCRemoteServer
{
    record CraftingPlan(DateTime StartTime, int CpuID, AEItem OutputItem);
    public class AECraftingMonitor
    {
        static List<CraftingPlan> set = new();
        static List<CraftingPlan> paperList = new List<CraftingPlan>();
        public static async Task Refresh()
        {
            var request = (await RemoteManager.Request("return GetAllCpuInfo()").ConfigureAwait(false)).JsonDeserialize<CpuInfo[]>();
            var i = 0;
            foreach (var cpuInfo in request)
            {
                cpuInfo.ID = i++;
            }
            foreach (var cpuInfo in request)
            {
                if (cpuInfo.Busy)
                {
                    var info = JsonDocument.Parse(await RemoteManager.Request($"return GetCpuInfo({cpuInfo.ID})").ConfigureAwait(false)).RootElement.GetString();
                    var item = info.Split("^#^#")[3].JsonDeserialize<AEItem>();
                    cpuInfo.CraftingItem = item;
                }
            }

            var set1 = new List<CraftingPlan>();
            foreach (var cpuInfo in request.Where(x => x.Busy && x.Name.Length > 1))
            {
                set1.Add(new CraftingPlan(DateTime.Now, cpuInfo.ID, cpuInfo.CraftingItem));
            }
            // 将新合成的物品加入列表
            foreach (var plan in set1)
            {
                var r = set.FirstOrDefault(x => x.OutputItem == plan.OutputItem && x.CpuID == plan.CpuID);
                if (r == null)
                {
                    set.Add(plan);
                }
            }

            // 处理合成完的物品
            var list = new List<CraftingPlan>();
            foreach (var plan in set)
            {
                var r = set1.FirstOrDefault(x => x.OutputItem == plan.OutputItem && x.CpuID == plan.CpuID);
                if (r == null)
                {
                    if (paperList.Contains(plan))
                    {
                        list.Add(plan);
                    }
                    else
                    {
                        list.Add(plan);
                        if ((DateTime.Now - plan.StartTime).TotalMinutes > 1)
                        {
                            await QQBotModule.Push(
                                $"物品: [{plan.OutputItem.SizeM}x{plan.OutputItem.Chinese}/{plan.OutputItem.Name}] 合成完成，用时{(DateTime.Now - plan.StartTime).Humanize(precision: 2, minUnit: TimeUnit.Second, maxUnit: TimeUnit.Hour, culture: new CultureInfo("zh-Hans"))}。");
                        }
                    }
                }
                
                if (!paperList.Contains(plan) && await CheckPaper(r))
                {
                    paperList.Add(plan);
                    await QQBotModule.Push(
                        $"集合物品: [{plan.OutputItem.SizeM.ToUpper()}x{plan.OutputItem.Chinese}/{plan.OutputItem.Label}] 合成完成，用时{(DateTime.Now - plan.StartTime).Humanize(precision: 2, minUnit: TimeUnit.Second, maxUnit: TimeUnit.Hour, culture: new CultureInfo("zh-Hans"))}。");
                }
            }

            foreach (var craftingPlan in list)
            {
                set.Remove(craftingPlan);
            }

        }

        static async Task<bool> CheckPaper(CraftingPlan plan)
        {
            if (plan == null) return false;
            
            if (plan.OutputItem?.Chinese == "纸")
            {
                var info = JsonDocument.Parse(await RemoteManager.Request($"return GetCpuInfo({plan.CpuID})")).RootElement.GetString();
                var pending1 = info.Split("^#^#")[2].JsonDeserialize<AEItem[]>();
                var crafting1 = info.Split("^#^#")[0].JsonDeserialize<AEItem[]>();
                return pending1.Length == 0 && crafting1.Length == 1;
            }

            return false;
        }
    }
}
