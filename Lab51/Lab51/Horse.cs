using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Lab51
{
    public class Horse : INotifyPropertyChanged
    {
        private static readonly Random random = new Random();
        public event PropertyChangedEventHandler PropertyChanged;

        public string Name { get; }
        public Color Color { get; }
        public float Speed { get; }
        public float acceleration { get; private set; }
        public float T { get; private set; }
        public TimeSpan RaceTime { get; private set; }

        private int betAmount;
        private double coefficient;
        private int position;

        public int BetAmount
        {
            get => betAmount;
            set
            {
                if (betAmount != value)
                {
                    betAmount = value;
                    OnPropertyChanged(nameof(BetAmount));
                }
            }
        }

        public double Coefficient
        {
            get => coefficient;
            set
            {
                if (coefficient != value)
                {
                    coefficient = value;
                    OnPropertyChanged(nameof(Coefficient));
                }
            }
        }

        public int Position
        {
            get => position;
            set
            {
                if (position != value)
                {
                    position = value;
                    OnPropertyChanged(nameof(Position));
                }
            }
        }

        private Stopwatch stopwatch;

        public Horse(string name, Color color)
        {
            Name = name;
            Color = color;
            Speed = random.Next(5, 11); 
            T = 0;
            BetAmount = 0;
            coefficient = 1.0;
            stopwatch = new Stopwatch();
            position = 0;
        }

        public void ChangeAcceleration()
        {
            double k = random.NextDouble() * 0.3 + 0.7;
            acceleration = (float)(Speed * k / 10);
        }

        public void Reset()
        {
            T = 0;
            RaceTime = TimeSpan.Zero;
            acceleration = 0;
            stopwatch.Reset();
        }

        public async Task RunAsync(Barrier barrier, CancellationToken token)
        {
            stopwatch.Start();

            while (!token.IsCancellationRequested && T < 1.0f)
            {
                ChangeAcceleration(); 
                T = Math.Min(1.0f, T + acceleration); 
                RaceTime = stopwatch.Elapsed;
                barrier.SignalAndWait();
                await Task.Delay(10);
            }
            stopwatch.Stop();
        }

        protected void OnPropertyChanged(string name) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}