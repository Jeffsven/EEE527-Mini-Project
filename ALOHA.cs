using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using static System.Collections.Specialized.BitVector32;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Rebar;

namespace EEE527_Mini_Project
{
    public class ALOHA
    {
        static RichTextBox Rtbx = null;

        static void AddLog(string s)
        {
            Rtbx.Invoke(new Action(() =>
            {
                Rtbx.AppendText(s + "\n");
                Rtbx.ScrollToCaret();
            }));
        }
        static byte[] FlattenImage(Bitmap b)
        {
            int width = b.Width;
            int height = b.Height;

            System.IO.MemoryStream stream = new System.IO.MemoryStream();
            b.Save(stream, System.Drawing.Imaging.ImageFormat.Bmp);
            stream.Position = 0;
            byte[] data = new byte[stream.Length];
            stream.Read(data, 0, (int)stream.Length);

            return data;
        }
        // Step 1: Network Setup
        static List<(double X, double Y, double Dist)> GenerateStations(int numStations, double minDist, double maxDist)
        {
            var stations = new List<(double, double, double)>();
            var rand = new Random();

            while (stations.Count < numStations)
            {
                double x = rand.NextDouble() * 100;
                double y = rand.NextDouble() * 100;
                double dist = Math.Sqrt(Math.Pow(x, 2) * Math.Pow(y, 2));
                bool valid = dist >= minDist && dist <= maxDist;
                if (valid) stations.Add((x, y, dist));
            }
            return stations;
        }

        // Step 2: ALOHA Logic
        static (int successes, int collisions, double throughput) SimulateALOHA(List<(double, double, double)> stations, int[] fileSizes, double bandwidth, double duration, double? slotTime = null)
        {
            int numSlots = (int)(duration / (slotTime ?? 1));
            var rand = new Random();
            var frames = Enumerable.Range(0, stations.Count).Select(_ => rand.Next(numSlots)).ToArray();

            int successes = 0, collisions = 0;
            int totalDataTransmitted = 0;

            for (int slot = 0; slot < numSlots; slot++)
            {
                var transmissions = frames.Select((frame, index) => (frame, index))
                                          .Where(t => t.frame == slot)
                                          .Select(t => t.index)
                                          .ToList();

                if (transmissions.Count == 1)
                {
                    int stationIndex = transmissions[0];
                    successes++;
                    totalDataTransmitted += fileSizes[stationIndex];
                }
                else if (transmissions.Count > 1)
                {
                    collisions++;

                    foreach (var station in transmissions)
                    {
                        frames[station] = rand.Next(slot + 1, numSlots);
                    }
                }
            }
            //double throughput = successes * frameSize / (duration * bandwidth);

            double throughput = totalDataTransmitted / (duration * bandwidth);
            return (successes, collisions, throughput);
        }

        public static void Execute(Bitmap bitmap, int numStations, int numIte, double minDist, double maxDist, double bandwidth, double duration, int frameSize_bit, RichTextBox rtb)
        {
            Rtbx = rtb;
            bandwidth *= 1e6;
            double? slotTime = null; // Use null for pure ALOHA, set to 1 for slotted ALOHA

            //// Generate Stations / Sampling for debug
            //var stations = GenerateStations(numStations, minDist, maxDist);
            //for (int i = 0; i < stations.Count; i++)
            //{
            //    var station = stations[i];
            //    AddLog($"Station {i + 1:d2}\t[X:{station.Item1:F6},\tY:{station.Item2:F6},\tDist:{station.Dist:f6}]");
            //}
            //AddLog($"Generated Stations:{stations.Count}");

            List<string> stringRes = new List<string>();

            //int[] fileSizes = Enumerable.Range(0, numStations).Select(_ => rand.Next(1024, 10240) * 8).ToArray();
            int[] fileSizes = FlattenImage(bitmap).Select(x => (int)x* frameSize_bit).ToArray();
            for (int n = numIte; n <= numStations; n += numIte)
            {
                var stations = GenerateStations(n, minDist, maxDist);
                //var simulate = SimulateALOHA(stations, flattenImg.Select(x => (int)x).ToArray(), frameSize, bandwidth, duration, slotTime);
                var simulate = SimulateALOHA(stations, fileSizes, bandwidth, duration, slotTime);

                stringRes.Add($"{n:d2}\t{simulate.throughput:f8}\t{simulate.collisions}");
            }

            AddLog("Stations\tThroughput\tCollisions");
            foreach (var result in stringRes) AddLog(result);
        }
    }
}