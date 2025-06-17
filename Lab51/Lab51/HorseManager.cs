using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Lab51
{
    public class RaceManager
    {
        public ObservableCollection<Horse> Horses { get; private set; } = new ObservableCollection<Horse>();
        private Barrier barrier;
        private CancellationTokenSource cts;
        private int frameDelay = 50;

        public event Action OnRenderRequested;

        private readonly Random rand = new Random();
        public event Action<int> OnBalanceChanged;

        public void UpdateHorseCount(int desiredCount)
        {
            if (desiredCount < 0) desiredCount = 0;

            while (Horses.Count < desiredCount)
            {
                var color = Color.FromRgb(rand.Next(256), rand.Next(256), rand.Next(256));
                Horses.Add(new Horse($"Horse {Horses.Count + 1}", color));
            }

            while (Horses.Count > desiredCount)
            {
                Horses.RemoveAt(Horses.Count - 1);
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
                while (!token.IsCancellationRequested && Horses.Any(h => h.T < 1.0f))
                {
                    barrier.SignalAndWait();
                    OnRenderRequested?.Invoke();
                    await Task.Delay(frameDelay);
                }
                OnRenderRequested?.Invoke();
                FinishRace();
            });
        }

        public void FinishRace()
        {
            var finished = Horses
                .OrderBy(h => h.RaceTime)
                .ToList();

            for (int i = 0; i < finished.Count; i++)
            {
                finished[i].Position = i + 1;

                if (finished[i].Position == 1 && finished[i].BetAmount > 0)
                {
                    int winnings = (int)(finished[i].BetAmount * finished[i].Coefficient);
                    OnBalanceChanged?.Invoke(winnings);
                }
            }

            for (int i = 0; i < finished.Count; i++)
            {
                finished[i].Coefficient += (Horses.Count - finished[i].Position) * 0.05;
                finished[i].BetAmount = 0;
            }

            OnRenderRequested?.Invoke();
        }

        public void StopRace() => cts?.Cancel();
    }
}