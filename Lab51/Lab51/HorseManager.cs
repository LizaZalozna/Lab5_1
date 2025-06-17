using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Lab51
{
    public class RaceManager
    {
        public List<Horse> Horses { get; private set; }
        private Barrier barrier;
        private CancellationTokenSource cts;
        private int frameDelay = 50;

        public event Action OnRenderRequested;

        private readonly Random rand = new Random();

        public void UpdateHorseCount(int desiredCount)
        {
            if (desiredCount < 0) desiredCount = 0;

            if (desiredCount > Horses.Count)
            {
                int currentCount = Horses.Count;
                for (int i = currentCount; i < desiredCount; i++)
                {
                    var color = Color.FromRgb(rand.Next(256), rand.Next(256), rand.Next(256));
                    Horses.Add(new Horse($"Horse {i + 1}", color));
                }
            }
            else if (desiredCount < Horses.Count)
            {
                Horses = Horses.Take(desiredCount).ToList();
            }
        }

        private void ResetHorses()
        {
            foreach (var horse in Horses)
            {
                horse.Reset();
            }
        }

        public void StartRace()
        {
            if (Horses.Count == 0)
                throw new InvalidOperationException("No horses created.");

            ResetHorses();

            cts?.Cancel();
            cts = new CancellationTokenSource();
            var token = cts.Token;

            barrier = new Barrier(Horses.Count + 1);

            foreach (var horse in Horses)
            {
                Task.Run(() => horse.RunAsync(barrier, token));
            }

            Task.Run(async () =>
            {
                while (!token.IsCancellationRequested && Horses.Any(h => h.t < 1.0f))
                {
                    barrier.SignalAndWait();
                    OnRenderRequested?.Invoke();
                    await Task.Delay(frameDelay);
                }
                OnRenderRequested?.Invoke();
            });
        }

        public void StopRace() => cts?.Cancel();
    }
}