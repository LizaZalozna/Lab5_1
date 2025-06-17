using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Lab51
{
    public class Horse
    {
        private static readonly Random random = new Random();
        public string name { get; }
        public Color color { get; }
        public float speed { get; } 
        public float acceleration { get; private set; } 
        public float t { get; private set; } 
        public TimeSpan raceTime { get; private set; }

        private Stopwatch stopwatch;

        public Horse(string name, Color color)
        {
            this.name = name;
            this.color = color;
            speed = random.Next(5, 11); 
            t = 0;
            stopwatch = new Stopwatch();
        }

        public void ChangeAcceleration()
        {
            double k = random.NextDouble() * 0.3 + 0.7;
            acceleration = (float)(speed * k / 5000);
        }

        public async Task RunAsync(Barrier barrier, CancellationToken token)
        {
            stopwatch.Start();

            while (!token.IsCancellationRequested && t < 1.0f)
            {
                ChangeAcceleration(); 
                t = Math.Min(1.0f, t + acceleration); 
                raceTime = stopwatch.Elapsed;
                barrier.SignalAndWait();
                await Task.Delay(10);
            }
            stopwatch.Stop();
        }
    }
}