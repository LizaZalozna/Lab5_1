using System;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using Lab51;

namespace Lab51
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPage : ContentPage
    {
        private readonly RaceManager raceManager = new RaceManager();
        private int horseCount = 3;
        private int balance = 1000;
        private int betAmount = 100;
        private int betHorseIndex = -1;

        private SKPoint[] trackPoints;

        private void ResetHorsePositions()
        {
            foreach (var horse in raceManager.Horses)
                horse.T = 0;
        }

        public MainPage()
        {
            InitializeComponent();
            BindingContext = raceManager;
            raceManager.UpdateHorseCount(horseCount);
            raceManager.OnRenderRequested += () =>
                Device.BeginInvokeOnMainThread(() => RaceCanvas.InvalidateSurface());

            raceManager.OnBalanceChanged += (winnings) =>
            {
                balance += winnings;
                Device.BeginInvokeOnMainThread(UpdateLabels);
            };

            HorsePicker.ItemsSource = raceManager.Horses.Select(h => h.Name).ToList();
            UpdateLabels();
            RaceCanvas.PaintSurface += OnRaceCanvasPaintSurface;
        }

        private void UpdateLabels()
        {
            BalanceLabel.Text = $"Balance: ${balance}";
            BetAmountLabel.Text = $"${betAmount}";
            HorseCountLabel.Text = $"{horseCount}";
        }

        private void OnLeftClicked(object sender, EventArgs e)
        {
            if (horseCount > 1)
            {
                horseCount--;
                raceManager.UpdateHorseCount(horseCount);
                HorsePicker.ItemsSource = raceManager.Horses.Select(h => h.Name).ToList();
                ResetHorsePositions();
            }
            UpdateLabels();
            RaceCanvas.InvalidateSurface();
        }

        private void OnRightClicked(object sender, EventArgs e)
        {
            if (horseCount < 7)
            {
                horseCount++;
                raceManager.UpdateHorseCount(horseCount);
                HorsePicker.ItemsSource = raceManager.Horses.Select(h => h.Name).ToList();
                ResetHorsePositions();
            }
            UpdateLabels();
            RaceCanvas.InvalidateSurface();
        }

        private void OnBetLeftClicked(object sender, EventArgs e)
        {
            if (betAmount > 50)
            {
                betAmount -= 50;
                UpdateLabels();
            }
        }

        private void OnBetRightClicked(object sender, EventArgs e)
        {
            if (betAmount + 50 <= balance)
            {
                betAmount += 50;
                UpdateLabels();
            }
        }

        private void OnBetClicked(object sender, EventArgs e)
        {
            if (HorsePicker.SelectedIndex < 0)
            {
                DisplayAlert("Error", "Please select a horse to bet on!", "OK");
                return;
            }

            if (balance < betAmount)
            {
                DisplayAlert("Error", "Not enough balance to place this bet!", "OK");
                return;
            }

            betHorseIndex = HorsePicker.SelectedIndex;
            balance -= betAmount;
            UpdateLabels();

            raceManager.Horses[betHorseIndex].BetAmount += betAmount;
        }

        private void OnStartClicked(object sender, EventArgs e)
        {
            ResetHorsePositions();
            raceManager.StartRace();
        }

        private void OnRaceCanvasPaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;
            canvas.Clear(SKColors.White);

            DrawTrack(canvas, e.Info.Width, e.Info.Height);
            DrawHorses(canvas, e.Info.Width, e.Info.Height);
        }

        private SKPoint[] GeneratePointsOnBezierCurve(SKPoint p0, SKPoint p1, SKPoint p2, SKPoint p3, int steps)
        {
            var points = new SKPoint[steps];
            for (int i = 0; i < steps; i++)
            {
                float t = (float)i / (steps - 1);
                points[i] = InterpolateCubicBezier(p0, p1, p2, p3, t);
            }
            return points;
        }

        private SKPoint InterpolateCubicBezier(SKPoint p0, SKPoint p1, SKPoint p2, SKPoint p3, float t)
        {
            float u = 1 - t;
            float tt = t * t;
            float uu = u * u;
            float uuu = uu * u;
            float ttt = tt * t;

            SKPoint p = new SKPoint();
            p.X = uuu * p0.X + 3 * uu * t * p1.X + 3 * u * tt * p2.X + ttt * p3.X;
            p.Y = uuu * p0.Y + 3 * uu * t * p1.Y + 3 * u * tt * p2.Y + ttt * p3.Y;

            return p;
        }

        private void DrawTrack(SKCanvas canvas, int width, int height)
        {
            var p0 = new SKPoint(10, height / 2 + 50);
            var p1 = new SKPoint(width / 3, height / 2 - 30);
            var p2 = new SKPoint(2 * width / 3, height / 2 + 40);
            var p3 = new SKPoint(width - 10, height / 2);

            trackPoints = GeneratePointsOnBezierCurve(p0, p1, p2, p3, 100);

            float trackWidth = 7 * 40f + 30f;

            var leftPoints = new SKPoint[trackPoints.Length];
            var rightPoints = new SKPoint[trackPoints.Length];

            for (int i = 0; i < trackPoints.Length - 1; i++)
            {
                var current = trackPoints[i];
                var next = trackPoints[i + 1];

                var direction = new SKPoint(next.X - current.X, next.Y - current.Y);
                float length = (float)Math.Sqrt(direction.X * direction.X + direction.Y * direction.Y);

                var normal = new SKPoint(-direction.Y / length, direction.X / length);

                leftPoints[i] = new SKPoint(current.X + normal.X * (trackWidth / 2), current.Y + normal.Y * (trackWidth / 2));
                rightPoints[i] = new SKPoint(current.X - normal.X * (trackWidth / 2), current.Y - normal.Y * (trackWidth / 2));
            }

            leftPoints[leftPoints.Length - 1] = leftPoints[leftPoints.Length - 2];
            rightPoints[rightPoints.Length - 1] = rightPoints[rightPoints.Length - 2];

            using (var paint = new SKPaint
            {
                Style = SKPaintStyle.Fill,
                Color = SKColors.LightGray,
                IsAntialias = true
            })
            {
                using (var path = new SKPath())
                {
                    path.MoveTo(leftPoints[0]);
                    for (int i = 1; i < leftPoints.Length; i++)
                        path.LineTo(leftPoints[i]);

                    for (int i = rightPoints.Length - 1; i >= 0; i--)
                        path.LineTo(rightPoints[i]);

                    path.Close();
                    canvas.DrawPath(path, paint);
                }
            }
        }

        private void DrawHorses(SKCanvas canvas, int width, int height)
        {
            float laneSpacing = 40f;
            int horseCountLocal = raceManager.Horses.Count;

            for (int i = 0; i < horseCountLocal; i++)
            {
                var horse = raceManager.Horses[i];
                float t = horse.T;
                int index = (int)(t * (trackPoints.Length - 1));
                if (index < 0) index = 0;
                if (index >= trackPoints.Length) index = trackPoints.Length - 1;

                var pos = trackPoints[index];
                float offsetY = (i - horseCountLocal / 2f) * laneSpacing;
                var horsePos = new SKPoint(pos.X, pos.Y + offsetY);

                using (var paint = new SKPaint
                {
                    Color = horse.Color.ToSKColor(),
                    IsAntialias = true
                })
                    canvas.DrawCircle(horsePos, 15, paint);

                using (var textPaint = new SKPaint
                {
                    Color = SKColors.Black,
                    TextSize = 14,
                    IsAntialias = true
                })
                    canvas.DrawText(horse.Name, horsePos.X + 20, horsePos.Y + 5, textPaint);
            }
        }
    }
}