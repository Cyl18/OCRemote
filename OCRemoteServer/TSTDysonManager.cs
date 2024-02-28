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
            var req = JsonDocument.Parse(await RemoteManager.Request("return getTSTDysonStatus()")).RootElement.EnumerateArray().ToArray()
                .Select(x => x.GetString()).ToArray();

            var rawData = req[11];
            remainingTime = req[12].Split('r').Last(); // haha
            amountDSPSolarSail = rawData.GetLastPart("amountDSPSolarSail:").GetFirstPart(" ").ToBigInteger();
            amountDSPNode = rawData.GetLastPart("amountDSPNode:").GetFirstPart(" ").ToBigInteger();
            maxDSPPowerPoint = rawData.GetLastPart("maxDSPPowerPoint:").GetFirstPart(" ").ToBigInteger();
            usedDSPPowerPoint = rawData.GetLastPart("usedDSPPowerPoint:").GetFirstPart(" ").ToBigInteger();
            eut = (double)usedDSPPowerPoint * Coefficient;

        }

        public static string remainingTime { get; set; }

        public static double eut { get; set; }

        public static BigInteger amountDSPSolarSail { get; set; }

        public static BigInteger amountDSPNode { get; set; }

        public static BigInteger maxDSPPowerPoint { get; set; }

        public static BigInteger usedDSPPowerPoint { get; set; }
    }
}
