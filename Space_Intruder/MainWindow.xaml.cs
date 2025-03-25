using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Space_Intruder.Class;

namespace Space_Intruder
{
    public partial class MainWindow : Window
    {
        private double pozycjaX = 200;
        private bool isGameOver = false;
        private Storyboard moveStoryboard;
        private DoubleAnimation moveAnimation;
        public double krok = 20;
        private Level_Gry gameLevel;
        private DispatcherTimer gameTimer;

        // Statustyki Bohatera

        private int life = 3;

        public MainWindow()
        {
            InitializeComponent();
            InitializeGame();
        }

        private void InitializeGame()
        {
            Canvas.SetLeft(Klocek, pozycjaX);
            Canvas.SetBottom(Klocek, 20);

            // Inicjalizacja animacji ruchu
            moveStoryboard = new Storyboard();
            moveAnimation = new DoubleAnimation
            {
                Duration = TimeSpan.FromMilliseconds(200),
                EasingFunction = new QuadraticEase()
            };
            Storyboard.SetTarget(moveAnimation, Klocek);
            Storyboard.SetTargetProperty(moveAnimation, new PropertyPath("(Canvas.Left)"));
            moveStoryboard.Children.Add(moveAnimation);

            // Inicjalizacja poziomów gry
            gameLevel = new Level_Gry(MyCanvas, Klocek);
            gameLevel.LoadLevel(1);

            // Uruchomienie głównej pętli gry
            gameTimer = new DispatcherTimer();
            gameTimer.Interval = TimeSpan.FromMilliseconds(16);
            gameTimer.Tick += GameLoop;
            gameTimer.Start();

            this.Loaded += (sender, e) => gameLevel.LoadLevel(1);
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (isGameOver) return;

            // Obsługa ruchu gracza
            double newX = pozycjaX;

            if (e.Key == Key.Left && pozycjaX > 0)
            {
                newX = pozycjaX - krok;
            }
            else if (e.Key == Key.Right && pozycjaX < Width - Klocek.Width - 16)
            {
                newX = pozycjaX + krok;
            }

            if (newX != pozycjaX)
            {
                moveStoryboard.Stop();
                moveAnimation.To = newX;
                moveStoryboard.Begin();
                pozycjaX = newX;
            }

            // Obsługa strzału
            if (e.Key == Key.Space)
            {
                ShootPlayerBullet();
            }
        }

        private void ShootPlayerBullet()
        {
            double klocekX = Canvas.GetLeft(Klocek);
            double klocekY = Canvas.GetBottom(Klocek);
            var bullet = new Pocisk(
                "bohater",
                5,
                klocekX,
                klocekY,
                MyCanvas,
                gameLevel.GetCurrentEnemies(),
                Klocek
            );
        }

        private void GameLoop(object sender, EventArgs e)
        {
            if (isGameOver) return;

            // Sprawdzenie czy poziom został ukończony
            if (gameLevel.AreAllEnemiesDefeated())
            {
                if (gameLevel.IsGameCompleted)
                {
                    EndGame(true); // Wygrana
                }
                else
                {
                    gameLevel.NextLevel();
                    current_level.Text =$"Level {gameLevel.CurrentLevel.ToString()}";
                }
            }

            // Aktualizacja przeciwników
            foreach (var enemy in gameLevel.GetCurrentEnemies())
            {
                enemy.Move();

                // Sprawdzenie kolizji z krawędziami
                double enemyX = Canvas.GetLeft(enemy.Visual);
                if (enemyX <= 0 || enemyX + enemy.Visual.Width >= MyCanvas.ActualWidth)
                {
                    enemy.Direction *= -1;
                    double currentY = Canvas.GetBottom(enemy.Visual);
                    Canvas.SetBottom(enemy.Visual, currentY - 10);
                }
            }
        }

        private void EndGame(bool isWin)
        {
            isGameOver = true;
            gameTimer.Stop();

            foreach (var enemy in gameLevel.GetCurrentEnemies())
            {
                if (enemy is MageEnemy mage) mage.StopShooting();
                if (enemy is SpiderEnemy spider) spider.StopShooting();
            }

            MessageBox.Show(isWin ? "Gratulacje! Wygrałeś grę!" : "Przegrałeś! Koniec gry.");
        }

        public void SlowDownPlayer()
        {
            double slowSpeed = 0;
            double normalSpeed = 20;

            krok = slowSpeed;

            var restoreSpeedTimer = new DispatcherTimer();
            restoreSpeedTimer.Interval = TimeSpan.FromSeconds(2);
            restoreSpeedTimer.Tick += (s, e) =>
            {
                krok = normalSpeed;
                restoreSpeedTimer.Stop();
            };
            restoreSpeedTimer.Start();
        }

        public void PlayerHit()
        {
            life--;
            UpdateLifeDisplay();

            if (life <= 0)
            {
                EndGame(false); // Game over
            }
        }

        private void UpdateLifeDisplay()
        {
            if(life >= 0) { current_life.Text = new string('❤', life); }
        }
    }
}