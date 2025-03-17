using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using static Space_Intruder.Class.Przeciwnik;

namespace Space_Intruder.Class
{
    public class Pocisk
    {
        private int sila;
        private DispatcherTimer timer;
        public Rectangle Kształt { get; private set; }
        private Canvas gameCanvas;
        private List<Enemy> enemies;

        public Pocisk(int sila, double startX, double startY, Canvas canvas, List<Enemy> enemies)
        {
            this.sila = sila;
            this.gameCanvas = canvas;
            this.enemies = enemies;

            Kształt = new Rectangle
            {
                Width = 5,
                Height = 20,
                Fill = Brushes.Red
            };

            // Ustawienie pozycji pocisku
            Canvas.SetLeft(Kształt, startX + 22.5); // Środek bohatera
            Canvas.SetTop(Kształt, startY - 20); // Pocisk startuje nad bohaterem

            // Dodaj pocisk do canvas
            canvas.Children.Add(Kształt);

            // Timer do ruchu pocisku
            timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(0.02) // Szybszy ruch
            };
            timer.Tick += MoveUp;
            timer.Start();
        }

        private void MoveUp(object sender, EventArgs e)
        {
            double currentY = Canvas.GetTop(Kształt);

            if (currentY <= 0) // Jeśli pocisk dotarł do góry ekranu, usuń go
            {
                Destroy();
            }
            else
            {
                Canvas.SetTop(Kształt, currentY - 10); // Poruszamy pocisk w górę
                CheckCollisionWithEnemies();
            }
        }

        private void CheckCollisionWithEnemies()
        {
            Rect bulletBounds = new Rect(Canvas.GetLeft(Kształt), Canvas.GetTop(Kształt), Kształt.Width, Kształt.Height);

            foreach (var enemy in enemies)
            {
                if (enemy.Visual == null) continue;

                double enemyX = Canvas.GetLeft(enemy.Visual);
                double enemyY = Canvas.GetTop(enemy.Visual);

                Rect enemyBounds = new Rect(enemyX, enemyY, enemy.Visual.Width, enemy.Visual.Height);

                if (bulletBounds.IntersectsWith(enemyBounds))
                {
                    enemy.TakeDamage(sila);
                    Destroy();
                    break;
                }
            }
        }

        private void Destroy()
        {
            timer.Stop();
            gameCanvas.Children.Remove(Kształt);
        }
    }
}
