using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Space_Intruder.Class;
using static Space_Intruder.Class.Przeciwnik;

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

            // Tworzymy czterech przeciwników
            CreateEnemies();
        }

        private void CreateEnemies()
        {
            // Dodanie czterech przeciwników na canvas
            enemies.Add(new Enemy(100, 50, EnemyType.Basic));
            enemies.Add(new Enemy(200, 50, EnemyType.Mage));
            enemies.Add(new Enemy(300, 50, EnemyType.Tank));
            enemies.Add(new Enemy(400, 50, EnemyType.Spider));

            // Dodajemy ich na canvas
            foreach (var enemy in enemies)
            {
                MyCanvas.Children.Add(enemy.Visual);
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


        // Możesz dodać logikę do poruszania przeciwników i ich strzelania
        private void GameLoop(object sender, EventArgs e)
        {
            foreach (var enemy in enemies)
            {
                // Ruch przeciwników
                enemy.Move();

                // Strzelanie przeciwników
                enemy.Shoot(MyCanvas);

                // Aktualizacja pocisków przeciwników
                enemy.UpdateBullets();
            }
        }
    }
}
