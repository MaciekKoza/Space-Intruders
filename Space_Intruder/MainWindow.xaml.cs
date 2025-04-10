using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Space_Intruder.Class;
using Space_Intruder.GameObjects;

namespace Space_Intruder
{
    public partial class MainWindow : Window
    {

        private bool isGameOver = false;
        private Level_Gry gameLevel;
        private DispatcherTimer gameTimer;
        private Hero player;
        private List<Boost> activeBoosts = new List<Boost>();
        private Stopwatch gameStopwatch = new Stopwatch();
        private string playerName = "Gracz"; // Możesz dodać dialog wprowadzania nazwy
        private bool isUpgradeScreenOpen = false;
        private DispatcherTimer statUpdateTimer;


        public MainWindow()
        {
            InitializeComponent();
            MyCanvas.Loaded += (sender, e) => InitializeGame();
            gameStopwatch.Start(); // Rozpocznij pomiar czasu gry
            statUpdateTimer = new DispatcherTimer();
            statUpdateTimer.Interval = TimeSpan.FromMilliseconds(500); // co 0.5 sekundy
            statUpdateTimer.Tick += UpdatePlayerStats;
            statUpdateTimer.Start();

        }

        private void InitializeGame()
        {
            gameLevel = new Level_Gry(MyCanvas, null);
            player = new Hero(MyCanvas, 200, 20, gameLevel, this, MyCanvas);
            gameLevel.SetPlayer(player);
            gameLevel.LoadLevel(1);

            gameTimer = new DispatcherTimer();
            gameTimer.Interval = TimeSpan.FromMilliseconds(16);
            gameTimer.Tick += GameLoop;
            gameTimer.Start();

            player.LivesChanged += (sender, e) => UpdateLifeDisplay();
            current_level.Text = $"Level {gameLevel.CurrentLevel}";
            UpdateLifeDisplay();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // Możesz dodać dodatkową logikę dostosowania UI jeśli potrzebna
            // np. skalowanie czcionek dla bardzo małych okien
        }

        private void UpdatePlayerStats(object sender, EventArgs e)
        {
            if (isGameOver || player == null) return;

            // Czas gry
            StatTimeText.Text = $"Czas gry: {gameStopwatch.Elapsed.ToString(@"hh\:mm\:ss")}";

            // Statystyki z klasy Hero
            StatDamageText.Text = $"Obrażenia: {player.GetDamage()}";
            StatAttackSpeedText.Text = $"Szybkość strzałów: {player.GetAttackSpeed():0.00}/s";
            StatMoveSpeedText.Text = $"Szybkość ruchu: {player.GetMovementSpeed():0.0} px";

            // Statusy
            StatShieldText.Text = $"Tarcza: { (player.IsShielded ? "Aktywna" : "Brak") }";
            StatTripleShotText.Text = $"Potrójny strzał: {(player.IsTripleShot ? "Aktywny" : "Brak")}";
        }


        private void SaveGameResult(bool isWin)
        {
            try
            {
                string resultLine = $"{playerName} | " +
                                   $"{gameLevel.CurrentLevel} | " +
                                   $"{gameStopwatch.Elapsed.ToString(@"hh\:mm\:ss")} | " +
                                   $"{(isWin ? "Wygrał" : "Przegrał")}";

                string filePath = "Wyniki_Graczy.txt";
                File.AppendAllText(filePath, resultLine + Environment.NewLine);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Błąd zapisu wyniku: {ex.Message}");
            }
        }

        private void EndGame(bool isWin)
        {
            isGameOver = true;
            gameTimer?.Stop();
            gameLevel?.StopAllEnemies();
            gameStopwatch.Stop();
            MyCanvas.Children.Clear();

            // Utwórz okno dialogowe do wprowadzenia nazwy gracza
            var inputDialog = new Window()
            {
                Title = "Wprowadź swoją nazwę",
                Width = 300,
                Height = 150,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };

            var stackPanel = new StackPanel { Margin = new Thickness(10) };
            var textBox = new TextBox { Margin = new Thickness(0, 0, 0, 10), Text = playerName };
            var button = new Button
            {
                Content = "Zapisz wynik",
                HorizontalAlignment = HorizontalAlignment.Center,
                Padding = new Thickness(10, 5, 10, 5)
            };

            button.Click += (s, e) =>
            {
                if (!string.IsNullOrWhiteSpace(textBox.Text))
                {
                    playerName = textBox.Text.Trim();
                    inputDialog.Close();
                    SaveGameResult(isWin); // Zapisz wynik po zamknięciu dialogu
                    ShowEndGameMessage(isWin);
                }
            };

            stackPanel.Children.Add(new Label { Content = "Twoja nazwa:" });
            stackPanel.Children.Add(textBox);
            stackPanel.Children.Add(button);
            inputDialog.Content = stackPanel;

            // Ustaw focus na TextBox i przycisk jako domyślny
            textBox.Focus();
            inputDialog.ShowDialog();

            inputDialog = null;
        }

        private void ShowEndGameMessage(bool isWin)
        {
            MessageBox.Show(
                isWin ? "Gratulacje! Wygrałeś!" : "Przegrałeś!",
                "Koniec gry",
                MessageBoxButton.OK,
                isWin ? MessageBoxImage.Information : MessageBoxImage.Exclamation);

            StartWindow start = new StartWindow();
            start.Show();
            this.Close();
        }

        public void PlayerHit()
        {
            player.TakeDamage();
            UpdateLifeDisplay();
            if (!player.IsAlive)
            {
                EndGame(false);
            }
        }

        private void UpdateLifeDisplay()
        {
            Dispatcher.Invoke(() =>
            {
                LifeContainer.Children.Clear();

                for (int i = 0; i < player._lives; i++)
                {
                    var heart = new Image
                    {
                        Source = new BitmapImage(new Uri("pack://application:,,,/Images/heart.jpg")),
                        Width = 30,
                        Height = 30,
                        Margin = new Thickness(5, 0, 5, 0)
                    };
                    LifeContainer.Children.Add(heart);
                }
            });
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (player.IsFrozen) return;
            if (isGameOver) return;

            if (e.Key == Key.Left) player.MoveLeft(0);
            else if (e.Key == Key.Right) player.MoveRight(MyCanvas.ActualWidth);
            else if (e.Key == Key.Space) player.Shoot(MyCanvas, gameLevel.GetCurrentEnemies());
        }

        private void GameLoop(object sender, EventArgs e)
        {
            if (isGameOver) return;

            if (!isUpgradeScreenOpen)
            {
                gameLevel.UpdateEnemies();

                if (gameLevel.AreAllEnemiesDefeated())
                {
                    if (gameLevel.IsGameCompleted)
                    {
                        EndGame(true);
                    }
                    else
                    {
                        // Usuń wszystkie pociski z planszy przed przejściem do następnego poziomu
                        ClearAllBullets();

                        gameLevel.NextLevel();
                        current_level.Text = $"Level {gameLevel.CurrentLevel}";
                        ShowUpgradeScreen();
                        UpdateLifeDisplay();
                    }
                }
            }

            for (int i = activeBoosts.Count - 1; i >= 0; i--)
            {
                if (activeBoosts[i].UpdatePosition())
                {
                    activeBoosts.RemoveAt(i);
                }
            }
        }

        private void ClearAllBullets()
        {
            // Usuń pociski gracza
            player.ClearAllBullets();

            // Usuń pociski przeciwników
            foreach (var enemy in gameLevel.GetCurrentEnemies())
            {
                enemy.ClearAllBullets();
            }

            // Możesz też dodać dodatkowe czyszczenie jeśli masz inne źródła pocisków
        }

        private void ShowUpgradeScreen()
        {
            isUpgradeScreenOpen = true;
            gameTimer.Stop();
            gameLevel.StopAllEnemies();
            player.IsFrozen = true;
            foreach (var enanmy in gameLevel.GetCurrentEnemies())
            {
                enanmy.IsFrozen = true;
            }


            var upgradeScreen = new UpgradeScreen(player, gameLevel);
            upgradeScreen.Closed += (s, args) =>
            {
                isUpgradeScreenOpen = false;
                player.IsFrozen = false;
                gameLevel.ResumeAllEnemies();
                gameTimer.Start();

                // Dodaj tę linię, aby wymusić odświeżenie wyświetlania żyć
                UpdateLifeDisplay();
            };
            upgradeScreen.Show();
        }

        public void SlowDownPlayer()
        {
            player.ApplySlowEffect(0.5, 2.0);
        }
    }
}