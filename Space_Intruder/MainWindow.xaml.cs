using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Space_Intruder.Class;

namespace Space_Intruder
{
    public partial class MainWindow : Window
    {
        private double pozycjaX = 200;
        private List<Enemy> enemies = new List<Enemy>(); // Lista przeciwników

        public MainWindow()
        {
            InitializeComponent();
            Canvas.SetLeft(Klocek, pozycjaX);
            Canvas.SetBottom(Klocek, 20); // Umieszczenie 20px od dolnej krawędzi

            // Uruchamiamy główną pętlę gry
            DispatcherTimer gameTimer = new DispatcherTimer();
            gameTimer.Interval = TimeSpan.FromMilliseconds(16); // ~60 FPS
            gameTimer.Tick += GameLoop;
            gameTimer.Start();

            // Tworzymy przeciwników po załadowaniu okna
            this.Loaded += Window_Loaded;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CreateEnemies();
        }

        private void CreateEnemies()
        {
            int enemyRows = 2; // Liczba rzędów przeciwników
            int enemyColumns = 6; // Liczba przeciwników w rzędzie
            double enemySpacingX = 80; // Odstęp między przeciwnikami w poziomie
            double enemySpacingY = 60; // Odstęp między rzędami przeciwników
            double startX = 50; // Początkowa pozycja X pierwszego przeciwnika
            double startY = MyCanvas.ActualHeight - 100; // Początkowa pozycja Y przeciwników (na górze ekranu)

            for (int row = 0; row < enemyRows; row++)
            {
                for (int col = 0; col < enemyColumns; col++)
                {
                    double x = startX + col * enemySpacingX;
                    double y = startY - row * enemySpacingY; // Przeciwnicy są ustawieni od góry

                    // Tworzymy przeciwnika
                    Enemy enemy = new Enemy(x, y, EnemyType.Basic); // Możesz zmienić typ przeciwnika
                    enemies.Add(enemy);

                    // Dodajemy przeciwnika do canvas
                    MyCanvas.Children.Add(enemy.Visual);
                }
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            const double krok = 10;

            // Obsługa ruchu
            if (e.Key == Key.Left && pozycjaX > 0)
            {
                pozycjaX -= krok;
            }
            else if (e.Key == Key.Right && pozycjaX < Width - Klocek.Width - 16)
            {
                pozycjaX += krok;
            }

            // Strzał
            if (e.Key == Key.Space)
            {
                double klocekX = Canvas.GetLeft(Klocek);
                double klocekY = Canvas.GetBottom(Klocek);

                // Tworzymy pocisk i przekazujemy listę przeciwników
                Pocisk pocisk = new Pocisk(2, klocekX, klocekY, MyCanvas, enemies);
            }

            // Aktualizowanie pozycji gracza
            Canvas.SetLeft(Klocek, pozycjaX);
        }

        private void GameLoop(object sender, EventArgs e)
        {
            // Poruszanie przeciwników
            foreach (var enemy in enemies)
            {
                enemy.Move();

                // Sprawdzamy, czy przeciwnik dotarł do krawędzi ekranu
                double enemyX = Canvas.GetLeft(enemy.Visual);
                if (enemyX <= 0 || enemyX + enemy.Visual.Width >= MyCanvas.ActualWidth)
                {
                    // Zmieniamy kierunek ruchu
                    enemy.Direction *= -1;

                    // Przesuwamy przeciwnika w dół (opcjonalnie)
                    double currentY = Canvas.GetBottom(enemy.Visual);
                    Canvas.SetBottom(enemy.Visual, currentY - 10); // 10 to wartość przesunięcia w dół
                }
            }

            // Strzelanie przeciwników (opcjonalnie)
            foreach (var enemy in enemies)
            {
                enemy.Shoot(MyCanvas);
                enemy.UpdateBullets();
            }
        }
    }
}