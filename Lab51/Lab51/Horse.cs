﻿using System;
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
        public float T { get; set; }

        private int betAmount;
        private double coefficient;
        private int position;
        private TimeSpan raceTime;

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

        public TimeSpan RaceTime
        {
            get => raceTime;
            private set
            {
                if (raceTime != value)
                {
                    raceTime = value;
                    OnPropertyChanged(nameof(RaceTime));
                    OnPropertyChanged(nameof(RaceTimeFormatted));
                }
            }
        }

        private Stopwatch stopwatch;
        public string RaceTimeFormatted => $"{RaceTime.TotalMilliseconds:F5} s";

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
            acceleration = (float)(Speed * k / 100);
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
            stopwatch.Restart();

            bool finished = false;

            while (!token.IsCancellationRequested)
            {
                if (!finished)
                {
                    ChangeAcceleration();
                    T = Math.Min(1.0f, T + acceleration);

                    if (T >= 1.0f)
                    {
                        stopwatch.Stop();
                        RaceTime = stopwatch.Elapsed;
                        finished = true; 
                    }
                }

                barrier.SignalAndWait();
                await Task.Delay(200);
            }

            OnPropertyChanged(nameof(RaceTimeFormatted));
        }

        protected void OnPropertyChanged(string name) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}