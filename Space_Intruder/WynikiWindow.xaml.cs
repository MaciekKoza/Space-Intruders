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
        private readonly string resultsFilePath = @"C:\Users\admin\Desktop\Space-Intruders\Space_Intruder\bin\Debug\net8.0-windows\Wyniki_Graczy.txt";

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

                // Sprawdzenie czy plik istnieje
                if (File.Exists(resultsFilePath))
                {
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
                }
                else
                {
                    // Jeśli plik nie istnieje, wyświetl komunikat
                    MessageBox.Show("Brak zapisanych wyników.", "Informacja", MessageBoxButton.OK, MessageBoxImage.Information);
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

        private void ExitBtn_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}