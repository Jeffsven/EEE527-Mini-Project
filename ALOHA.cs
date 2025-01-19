using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using static System.Collections.Specialized.BitVector32;

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
        public static byte[] FlattenImage(Bitmap b)
        {
            int width = b.Width;
            int height = b.Height;

            // Create a 1D array to store grayscale values
            byte[] flattenedArray = new byte[width * height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Get pixel color
                    Color pixel = b.GetPixel(x, y);

                    // Convert to grayscale
                    byte gray = (byte)((pixel.R + pixel.G + pixel.B) / 3);

                    // Store grayscale value in the 1D array
                    flattenedArray[y * width + x] = gray;
                }
            }

            return flattenedArray;
        }

        public static List<byte[]> SplitIntoFrames(byte[] flattenedArray, int frameSizeBits)
        {
            int frameSizeBytes = frameSizeBits / 8; // Convert bits to bytes
            List<byte[]> frames = new List<byte[]>();

            for (int i = 0; i < flattenedArray.Length; i += frameSizeBytes)
            {
                int length = Math.Min(frameSizeBytes, flattenedArray.Length - i);
                byte[] frame = new byte[length];
                Array.Copy(flattenedArray, i, frame, 0, length);
                frames.Add(frame);
            }

            return frames;
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
        static (int successes, int collisions, double throughput) SimulateALOHA(List<(double, double, double)> stations, int[] frames, int frameSize, double bandwidth, double duration, double? slotTime = null)
        {
            int numSlots = (int)(duration / (slotTime ?? 1));
            var rand = new Random();
            //var frames = Enumerable.Range(0, stations.Count).Select(_ => rand.Next(numSlots)).ToArray();

            int successes = 0, collisions = 0;

            for (int slot = 0; slot < numSlots; slot++)
            {
                var transmissions = frames.Select((frame, index) => (frame, index))
                                          .Where(t => t.frame == slot)
                                          .Select(t => t.index)
                                          .ToList();

                if (transmissions.Count == 1)
                {
                    successes++;
                }
                else if (transmissions.Count > 1)
                {
                    collisions++;

                    // Retransmit collided frames in random future slots
                    foreach (var station in transmissions)
                    {
                        frames[station] = rand.Next(slot + 1, numSlots);
                    }
                }
            }

            double throughput = successes * frameSize / (duration * bandwidth);
            return (successes, collisions, throughput);
        }

        public static void Execute(Bitmap bitmap, int numStations, double minDist, double maxDist, double bandwidth, double duration, RichTextBox rtb)
        {
            Rtbx = rtb;
            int frameSize = 256; // bits
            bandwidth *= 1e6;
            double? slotTime = null; // Use null for pure ALOHA, set to 1 for slotted ALOHA

            //// Generate Stations
            //var stations = GenerateStations(numStations, minDist, maxDist);
            //for(int i = 0; i < stations.Count; i++)
            //{
            //    var station = stations[i];
            //    AddLog($"Station {i+1:d2}\t[X:{station.Item1:F6},\tY:{station.Item2:F6},\tDist:{station.Dist:f6}]");
            //}
            //AddLog($"Generated Stations:{stations.Count}");

            List<string> stringRes = new List<string>();

            var flattenImg = FlattenImage(bitmap);
            //var flatternArray = SplitIntoFrames(flattenImg, frameSize);
            for (int n = 5; n <= numStations; n += 5)
            {

                var stations = GenerateStations(n, minDist, maxDist);
                var simulate = SimulateALOHA(stations, flattenImg.Select(x => (int)x).ToArray(), frameSize, bandwidth, duration, slotTime);

                stringRes.Add($"{n:d2}\t{simulate.throughput:f8}\t{simulate.collisions}");
            }

            AddLog("Stations\tThroughput\tCollisions");
            foreach (var result in stringRes)
            {
                AddLog(result);
            }
        }
    }
}