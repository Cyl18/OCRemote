using System.Numerics;
using System.Text.Json;
using GammaLibrary.Extensions;

namespace OCRemoteServer
{
    public class TSTDysonManager
    {
        const double Coefficient = 2.6 * 2.98 * 4;

        public static async Task DoTick()
        {
            try
            {
                var req = JsonDocument.Parse(await RemoteManager.Request("return getTSTDysonStatus()")).RootElement
                    .EnumerateArray().ToArray()
                    .Select(x => x.GetString()).ToArray();

                var rawData = req[11];
                remainingTime = req[12].Split('r').Last(); // haha
                amountDSPSolarSail = rawData.GetLastPart("amountDSPSolarSail:").GetFirstPart(" ").ToDouble();
                amountDSPNode = rawData.GetLastPart("amountDSPNode:").GetFirstPart(" ").ToDouble();
                maxDSPPowerPoint = rawData.GetLastPart("maxDSPPowerPoint:").GetFirstPart(" ").ToDouble();
                usedDSPPowerPoint = rawData.GetLastPart("usedDSPPowerPoint:").GetFirstPart(" ").ToDouble();
                eut = (double) usedDSPPowerPoint * Coefficient;
                maxeut = (double) maxDSPPowerPoint * Coefficient;

            }
            catch (Exception e)
            {
                //Console.WriteLine(e);
            }
        }

        public static string remainingTime { get; set; }

        public static double eut { get; set; }
        public static double maxeut { get; set; }

        public static double amountDSPSolarSail { get; set; }

        public static double amountDSPNode { get; set; }

        public static double maxDSPPowerPoint { get; set; }

        public static double usedDSPPowerPoint { get; set; }
    }
}
