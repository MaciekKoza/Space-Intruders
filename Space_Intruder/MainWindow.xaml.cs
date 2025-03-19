using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Space_Intruder.Class;

namespace Space_Intruder
{
    public partial class MainWindow : Window
    {
        private double pozycjaX = 200;
        private List<Enemy> enemies = new List<Enemy>(); // Lista przeciwników

        // Animacja ruchu gracza
        private Storyboard moveStoryboard;
        private DoubleAnimation moveAnimation;

        public MainWindow()
        {
            InitializeComponent();
            Canvas.SetLeft(Klocek, pozycjaX);
            Canvas.SetBottom(Klocek, 20); // Umieszczenie 20px od dolnej krawędzi

            // Inicjalizacja animacji
            moveStoryboard = new Storyboard();
            moveAnimation = new DoubleAnimation
            {
                Duration = TimeSpan.FromMilliseconds(200), // Czas trwania animacji
                EasingFunction = new QuadraticEase() // Funkcja płynności
            };
            Storyboard.SetTarget(moveAnimation, Klocek);
            Storyboard.SetTargetProperty(moveAnimation, new PropertyPath("(Canvas.Left)"));
            moveStoryboard.Children.Add(moveAnimation);

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
            int enemyRows = 4; // Liczba rzędów przeciwników (każdy rząd to inna klasa)
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

                    Enemy enemy;

                    // Tworzymy przeciwnika w zależności od rzędu
                    switch (row)
                    {
                        case 0:
                            enemy = new BasicEnemy(x, y); // Pierwszy rząd: BasicEnemy
                            break;
                        case 1:
                            enemy = new MageEnemy(x, y); // Drugi rząd: MageEnemy
                            break;
                        case 2:
                            enemy = new TankEnemy(x, y); // Trzeci rząd: TankEnemy
                            break;
                        case 3:
                            enemy = new SpiderEnemy(x, y); // Czwarty rząd: SpiderEnemy
                            break;
                        default:
                            throw new InvalidOperationException("Nieznany typ przeciwnika");
                    }

                    enemies.Add(enemy);

                    // Dodajemy przeciwnika do canvas
                    MyCanvas.Children.Add(enemy.Visual);
                }
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            const double krok = 10;

            // Oblicz nową pozycję X
            double newX = pozycjaX;

            if (e.Key == Key.Left && pozycjaX > 0)
            {
                newX = pozycjaX - krok;
            }
            else if (e.Key == Key.Right && pozycjaX < Width - Klocek.Width - 16)
            {
                newX = pozycjaX + krok;
            }

            // Uruchom animację tylko jeśli pozycja się zmienia
            if (newX != pozycjaX)
            {
                // Zatrzymaj poprzednią animację
                moveStoryboard.Stop();

                // Ustaw nową pozycję docelową
                moveAnimation.To = newX;

                // Uruchom animację
                moveStoryboard.Begin();

                // Zaktualizuj pozycję X
                pozycjaX = newX;
            }

            // Strzał
            if (e.Key == Key.Space)
            {
                double klocekX = Canvas.GetLeft(Klocek);
                double klocekY = Canvas.GetBottom(Klocek);

                // Tworzymy pocisk i przekazujemy listę przeciwników
                Pocisk pocisk = new Pocisk(5, klocekX, klocekY, MyCanvas, enemies);
            }
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
        }
    }
}