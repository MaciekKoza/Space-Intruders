using System.Windows.Controls;
using System.Windows;
using System.Windows.Shapes;
using System.Windows.Threading;
using static Space_Intruder.Class.Enemy;

namespace Space_Intruder.Class
{
    public class Pocisk
    {
        private Rectangle visual; // Wizualna reprezentacja pocisku
        private double speed; // Prędkość pocisku
        private Canvas canvas; // Canvas, na którym znajduje się pocisk
        private List<Enemy> enemies; // Lista przeciwników
        private DispatcherTimer timer; // Timer do aktualizacji pozycji pocisku

        public Pocisk(double speed, double startX, double startY, Canvas canvas, List<Enemy> enemies)
        {
            this.speed = speed;
            this.canvas = canvas;
            this.enemies = enemies;

            // Tworzymy wizualną reprezentację pocisku
            visual = new Rectangle
            {
                Width = 5,
                Height = 15,
                Fill = System.Windows.Media.Brushes.Yellow
            };

            // Ustawiamy pozycję początkową pocisku
            Canvas.SetLeft(visual, startX + 20); // 20 to offset, aby pocisk był na środku gracza
            Canvas.SetBottom(visual, startY);

            // Dodajemy pocisk do canvas
            canvas.Children.Add(visual);

            // Uruchamiamy timer do aktualizacji pozycji pocisku
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(16); // ~60 FPS
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            // Przesuwamy pocisk do góry
            double currentY = Canvas.GetBottom(visual);
            Canvas.SetBottom(visual, currentY + speed);

            // Sprawdzamy kolizję z przeciwnikami
            CheckCollision();

            // Jeśli pocisk wyjdzie poza canvas, usuwamy go
            if (currentY > canvas.ActualHeight)
            {
                timer.Stop();
                canvas.Children.Remove(visual);
            }
        }

        private void CheckCollision()
        {
            // Pobieramy pozycję i rozmiar pocisku
            Rect pociskRect = new Rect(Canvas.GetLeft(visual), Canvas.GetBottom(visual), visual.Width, visual.Height);

            // Sprawdzamy kolizję z każdym przeciwnikiem
            foreach (var enemy in enemies.ToList()) // Używamy ToList(), aby uniknąć modyfikacji kolekcji podczas iteracji
            {
                Rect enemyRect = new Rect(Canvas.GetLeft(enemy.Visual), Canvas.GetBottom(enemy.Visual), enemy.Visual.Width, enemy.Visual.Height);

                if (pociskRect.IntersectsWith(enemyRect))
                {
                    // Kolizja! Usuwamy przeciwnika i pocisk
                    canvas.Children.Remove(enemy.Visual);
                    enemies.Remove(enemy);
                    canvas.Children.Remove(visual);
                    timer.Stop();
                    break; // Przerywamy pętlę, ponieważ pocisk został zniszczony
                }
            }
        }
    }
}