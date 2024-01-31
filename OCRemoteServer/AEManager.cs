using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;
using GammaLibrary.Extensions;
using Humanizer;
using J = System.Text.Json.Serialization.JsonPropertyNameAttribute;

namespace OCRemoteServer
{
    public record Item(string RegistryName, int ID, int Meta, string DisplayName)
    {

    }

    public static class AEManager
    {
        public static Dictionary<(string, int), Item> items = new();
        public static List<AEItem> LastAEItem;
        static AEManager()
        {
            LoadItems();
        }


        internal static void LoadItems()
        {
            foreach (var s in File.ReadAllLines("../itempanel.csv").Skip(1))
            {
                var strings = s.Split(',');
                var item = new Item(strings[0], strings[1].ToInt(), strings[2].ToInt(),
                    strings[4..].Connect(",").Trim('"').Replace("/", "_"));
                items[(item.RegistryName, item.Meta)] = item;
            }
        }

        
    }

    // {"label":"Stone","hasTag":false,"damage":0,"isCraftable":true,"size":141,"maxSize":64,"name":"minecraft:stone","maxDamage":0}
    public class AEItem
    {
        [J("label")] public string Label { get; set; }
        [J("hasTag")] public bool HasTag { get; set; }
        [J("damage")] public int Damage { get; set; }
        [J("isCraftable")] public bool IsCraftable { get; set; }
        [J("size")] public long Size { get; set; }
        [J("maxSize")] public long MaxSize { get; set; }
        [J("name")] public string Name { get; set; }
        [J("maxDamage")] public long MaxDamage { get; set; }

        [JsonIgnore]
        public string Chinese
        {
            get
            {
                if (AEManager.items.TryGetValue((Name, Damage), out var x))
                {
                    if (x.DisplayName.Contains("液滴"))
                    {
                        return Label;
                    }
                    return x.DisplayName;
                }
                else
                {
                    return Label;
                }
            }
        }

        [JsonIgnore]
        public string ImagePath
        {
            get
            {
                if (AEManager.items.TryGetValue((Name, Damage), out var x))
                {
                    return x.DisplayName;
                }
                else
                {
                    return "MissingNo";
                }
            }
        }
        [JsonIgnore]
        public bool ShowText { get; set; }

        [JsonIgnore]
        public Guid Guid { get; } = Guid.NewGuid();
        [JsonIgnore]
        public string SizeM => ((double)Size).ToMetric(decimals: 1);
    }

    public partial class CpuInfo
    {
        [J("storage")] public long Storage { get; set; }
        [J("name")] public string Name { get; set; }
        [J("busy")] public bool Busy { get; set; }
        [J("coprocessors")] public long Coprocessors { get; set; }
        [JsonIgnore]public int ID { get; set; }
        [JsonIgnore]public AEItem CraftingItem { get; set; }
    }
}
