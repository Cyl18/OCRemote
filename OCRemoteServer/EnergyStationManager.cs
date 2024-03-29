﻿using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Reflection.Emit;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Numerics;
using System.Text.RegularExpressions;

namespace OCRemoteServer
{
    public class EnergyStationManager
    {
        public static ConcurrentQueue<ESReport> latestValue = new();
        public static BigInteger lastMainStorage;
        public static void Init()
        {
            if (!File.Exists(Config.DatabasePath))
            {
                using var context = new MyDbContext();
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

            }

            /*
            using var ctx = new MyDbContext();
            ctx.ESReports.RemoveRange(ctx.ESReports.Where(x => (DateTime.Now.AddMinutes(-40) < x.Date)));
            ctx.SaveChanges();
            Console.WriteLine("done");
            return;*/
            Task.Run(async () =>
            {
                var start = DateTime.Now;
                start:
                var lastDate = start - TimeSpan.FromHours(1);
                var lastUsed = new BigInteger();
                var lastWireless = new BigInteger();
                string?[][]? lastRoot = null;
                while (true)
                {
                    try
                    {
                        if (DateTime.Now.Second != start.Second)
                        {
                            await TSTDysonManager.DoTick();

                            using var ctx = new MyDbContext();
                            var storeR = RemoteManager.Request("return getStorageEnergyStatus()");
                            var dateNow = DateTime.Now;

                            var root = JsonDocument.Parse(await storeR).RootElement.EnumerateArray().ToArray()
                                .Select(x => x.EnumerateArray().Select(x => x.GetString()).ToArray()).ToArray();

                            var store = root[0];
                            lastMainStorage = Normalize(store[1]!);
                            var total = Normalize(store[2]!); // store 的

                            var used = root.Select(x => Normalize(x[1]!)).Aggregate((a, b) => a + b);

                            // ** 你可能需要修改下面的内容 **
                            var in1 = root[1]; // naq
                            var in2 = root[2]; // dyson
                            var naqIn = Normalize(in1[6]!);
                            var dysonIn = Normalize(in2[6]!);
                            var avgin = naqIn + dysonIn + (BigInteger)TSTDysonManager.eut; // eu in
                            // ** 你可能需要修改上面的内容 **

                            var wireless = root.Select(x => Normalize(x[14]!)).Last(); // 无线存储

                            if ((DateTime.Now - lastDate).TotalSeconds > 5)
                            {
                                goto end;
                            }


                            if (latestValue.Count >= 50)
                            {
                                latestValue.TryDequeue(out _);
                            }

                            var totalEUPrevious = lastUsed + lastWireless;
                            var totalEUCurrent = used + wireless;
                            var timeDelta = (dateNow - lastDate).TotalSeconds;
                            var deltaInTick = ((double)(totalEUCurrent - totalEUPrevious) / (timeDelta * 20));
                            var nReport = new ESReport((double)avgin, (double)avgin - deltaInTick, (double)used, (double)total, (double)wireless, DateTime.Now);

                            var inUv = ((double) avgin) / 524288;
                            var outUv = (deltaInTick) / 524288;
                            if (Math.Abs(outUv) > 70000000)
                            {
                                //Console.Clear();
                                //Console.WriteLine($"\nin {inUv:N0} delta {outUv:N0} lastwireless {lastWireless/524288/3600/20} wireless {wireless/524288/3600/20} delta {(wireless-lastWireless) / 524288 / 3600 / 20} ");
                                for (int i = 0; i < root.Length; i++)
                                {
                                    var s = root[i][1];
                                    var s1 = lastRoot[i][1];
                                    var nn = Normalize(s) / 524288 / 3600 / 20;
                                    var nn2 = Normalize(s1) / 524288 / 3600 / 20;
                                    //Console.WriteLine($"i{i} {nn} {nn2} delta{nn2-nn}");
                                }

                                //Thread.Sleep(5000);
                                //goto start;
                            }

                            latestValue.Enqueue(nReport);
                            if (latestValue.Count > 10)
                            {
                                ctx.ESReports.Add(nReport);
                            }


                            ctx.SaveChanges();
                            lastRoot = root;
                            end:
                            lastWireless = wireless;
                            lastUsed = used;
                            lastDate = dateNow;
                            start = DateTime.Now;
                        }
                        else
                        {
                            Thread.Sleep(10);
                        }

                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
            });
        }

        static BigInteger Normalize(string s)
        {
            var fi = s.SkipWhile(c => !char.IsNumber(c));
            var l = new List<char>();
            foreach (var c in fi)
            {
                if (char.IsNumber(c))
                {
                    l.Add(c);
                }
                else if (c != ',')
                {
                    break;
                }
            }
            var s1 = new string(l.ToArray());
            return BigInteger.Parse(s1);
        }

    }

    public class MyDbContext : DbContext
    {

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite($@"DataSource={Config.DatabasePath};").LogTo(x =>
            {
                //Console.WriteLine(Regex.Match(x, "(\\d+ms)", RegexOptions.Multiline).Groups[1].Value);
            }, LogLevel.Information);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
        }

        public DbSet<ESReport> ESReports { get; set; }
    }

    [Table("ESReport")]
    public record ESReport(double AvgIn, double AvgOut, double Used, double Total, double Wireless,[property: Key] DateTime Date)
    {
    }
}
