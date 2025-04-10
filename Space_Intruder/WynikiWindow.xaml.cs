using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;

namespace Space_Intruder
{
    public partial class WynikiWindow : Window
    {
        public class GameResult
        {
            public string PlayerName { get; set; }
            public string Level { get; set; }
            public string Time { get; set; }
            public string Result { get; set; }
        }

        // Ścieżka do pliku z wynikami
        private readonly string resultsFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Wyniki_Graczy.txt");

        public WynikiWindow()
        {
            InitializeComponent();
            LoadResults();
        }

        private void LoadResults()
        {
            try
            {
                var results = new List<GameResult>();

                // Tworzy plik, jeśli nie istnieje
                if (!File.Exists(resultsFilePath))
                {
                    File.Create(resultsFilePath).Close();
                }

                string[] lines = File.ReadAllLines(resultsFilePath);

                foreach (string line in lines)
                {
                    string[] parts = line.Split('|');
                    if (parts.Length == 4)
                    {
                        results.Add(new GameResult
                        {
                            PlayerName = parts[0].Trim(),
                            Level = parts[1].Trim(),
                            Time = parts[2].Trim(),
                            Result = parts[3].Trim()
                        });
                    }
                }

                ResultsList.ItemsSource = results;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd podczas wczytywania wyników: {ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            StartWindow startWindow = new StartWindow();
            startWindow.Show();
            this.Close();
        }

        private void VideoBackground_MediaEnded(object sende, RoutedEventArgs e)
        {
            MessageBox.Show("uj");
        }

        private void ExitBtn_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}