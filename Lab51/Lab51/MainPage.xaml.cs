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

        public MainPage()
        {
            InitializeComponent();

            BindingContext = raceManager;

            raceManager.UpdateHorseCount(horseCount);
            raceManager.OnRenderRequested += () =>
                Device.BeginInvokeOnMainThread(() => RaceCanvas.InvalidateSurface());

            HorsePicker.ItemsSource = raceManager.Horses.Select(h => h.Name).ToList();

            UpdateLabels();
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
            }
            UpdateLabels();
        }

        private void OnRightClicked(object sender, EventArgs e)
        {
            if (horseCount < 10)
            {
                horseCount++;
                raceManager.UpdateHorseCount(horseCount);
                HorsePicker.ItemsSource = raceManager.Horses.Select(h => h.Name).ToList();
            }
            UpdateLabels();
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
    }
}