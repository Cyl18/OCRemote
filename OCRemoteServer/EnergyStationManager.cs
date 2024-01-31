using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Reflection.Emit;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Numerics;

namespace OCRemoteServer
{
    public class EnergyStationManager
    {
        public static ConcurrentQueue<ESReport> latestValue = new();
        public static void Init()
        {
            if (!File.Exists("../es.db"))
            {
                using var context = new MyDbContext();
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

            }
            Task.Run(async () =>
            {
                var start = DateTime.Now; 
                while (true)
                {
                    try
                    {
                        if (DateTime.Now.Second != start.Second)
                        {
                            using var ctx = new MyDbContext();
                            var request = JsonDocument.Parse(await RemoteManager.Request("return getEnergyStatus()")).RootElement.EnumerateArray().Select(x => x.GetString()).ToArray();

                            
                            var used = Normalize(request[1]);
                            var total = Normalize(request[2]);
                            var avgin = Normalize(request[6]);
                            var avgout = Normalize(request[7]);
                            var report = new ESReport((double)avgin,(double)avgout,(double)used,(double)total,0,DateTime.Now);
                            if (latestValue.Count >= 5)
                            {
                                latestValue.TryDequeue(out _);
                            }

                            latestValue.Enqueue(report);

                            ctx.ESReports.Add(report);

                            ctx.SaveChanges();
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
            optionsBuilder.UseSqlite(@"DataSource=../es.db;");
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
