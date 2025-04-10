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

        public MainWindow()
        {
            InitializeComponent();
            MyCanvas.Loaded += (sender, e) => InitializeGame();
            gameStopwatch.Start(); // Rozpocznij pomiar czasu gry
        }

        private void InitializeGame()
        {
            gameLevel = new Level_Gry(MyCanvas, null);
            player = new Hero(MyCanvas, 200, 20, gameLevel);
            gameLevel.SetPlayer(player);
            gameLevel.LoadLevel(3);

            gameTimer = new DispatcherTimer();
            gameTimer.Interval = TimeSpan.FromMilliseconds(16);
            gameTimer.Tick += GameLoop;
            gameTimer.Start();

            player.LivesChanged += (sender, e) => UpdateLifeDisplay();
            current_level.Text = $"Level {gameLevel.CurrentLevel}";
            UpdateLifeDisplay();
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

            SaveGameResult(isWin); // Zapisz wynik przed pokazaniem komunikatu

            MessageBox.Show(isWin ? "Gratulacje! Wygrałeś!" : "Przegrałeś!");

            // Możesz dodać tutaj przejście do ekranu wyników lub menu głównego
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

            if (!isUpgradeScreenOpen) // Tylko jeśli okno ulepszeń nie jest otwarte
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
                        gameLevel.NextLevel();
                        current_level.Text = $"Level {gameLevel.CurrentLevel}";
                        ShowUpgradeScreen(); // Wywołanie metody pokazującej okno ulepszeń
                        UpdateLifeDisplay();
                    }
                }
            }

            for (int i = activeBoosts.Count - 1; i >= 0; i--)
            {
                if (activeBoosts[i].UpdatePosition(player)) // boost został zebrany lub spadł
                {
                    activeBoosts.RemoveAt(i);
                }
            }

        }

        private bool isUpgradeScreenOpen = false;

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