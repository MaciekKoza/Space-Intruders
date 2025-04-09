using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        public MainWindow()
        {
            InitializeComponent();
            MyCanvas.Loaded += (sender, e) => InitializeGame();
        }

        public void SetPlayer(Hero player)
        {
            this.player = player;
        }


        private void InitializeGame()
        {
            // 1. Stwórz Level_Gry bez gracza (tymczasowo null)
            gameLevel = new Level_Gry(MyCanvas, null);

            // 2. Stwórz gracza z referencją do gameLevel
            player = new Hero(MyCanvas, 200, 20, gameLevel);

            // 3. Ustaw gracza w Level_Gry
            gameLevel.SetPlayer(player);

            // 4. Wczytaj poziom
            gameLevel.LoadLevel(1);

            // Reszta bez zmian
            gameTimer = new DispatcherTimer();
            gameTimer.Interval = TimeSpan.FromMilliseconds(16);
            gameTimer.Tick += GameLoop;
            gameTimer.Start();

            player.LivesChanged += (sender, e) => UpdateLifeDisplay();
            current_level.Text = $"Level {gameLevel.CurrentLevel}";
            UpdateLifeDisplay();

        }

        private void UpdateLifeDisplay()
        {
            Dispatcher.Invoke(() =>
            {
                LifeContainer.Children.Clear();

                for (int i = 0; i < player.Lives; i++)
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

        // Wywołuj tę metodę zawsze gdy zmienia się liczba żyć:
        private void Player_OnLifeChanged(object sender, EventArgs e)
        {
            UpdateLifeDisplay();
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

            var upgradeScreen = new UpgradeScreen(player);
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

        private void EndGame(bool isWin)
        {
            isGameOver = true;
            gameTimer?.Stop();
            gameLevel?.StopAllEnemies();
            MessageBox.Show(isWin ? "Congratulations! You won!" : "Game Over!");
        }

        public void StopGame()
        {
            gameTimer?.Stop();
            gameLevel?.StopAllEnemies();
        }

        public void SlowDownPlayer()
        {
            player.ApplySlowEffect(0.5, 2.0);
        }

        public void PlayerHit()
        {
            player.TakeDamage();
            UpdateLifeDisplay();
            if (!player.IsAlive) EndGame(false);
        }
    }
}