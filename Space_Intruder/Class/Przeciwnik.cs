using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Space_Intruder.Class
{
    internal class Przeciwnik
    {
        public enum EnemyType
        {
            Basic,   // Podstawowy typ
            Mage,    // Mag
            Tank,    // Typ o dużym zdrowiu
            Spider   // Nowy typ przeciwnika - Spider
        }

        public class Enemy
        {
            // Właściwości przeciwnika
            public double X { get; set; }
            public double Y { get; set; }
            public double Speed { get; set; }
            public int Health { get; set; }
            public Ellipse Visual { get; set; }
            public EnemyType Type { get; set; }
            public bool MovingRight { get; set; }  // Flaga do kontrolowania kierunku ruchu
            public bool IsShooting { get; set; }  // Flaga informująca, czy przeciwnik strzela
            public List<Rectangle> Bullets { get; set; }  // Lista pocisków przeciwnika

            private Random _random;

            // Konstruktor przeciwnika
            public Enemy(double x, double y, EnemyType type)
            {
                X = x;
                Y = y;
                Type = type;
                MovingRight = true;  // Początkowy ruch w prawo
                Bullets = new List<Rectangle>();
                _random = new Random();

                // Ustawienia w zależności od typu przeciwnika
                switch (Type)
                {
                    case EnemyType.Basic:
                        Speed = 2;
                        Health = 1;
                        Visual = new Ellipse
                        {
                            Width = 40,
                            Height = 40,
                            Fill = Brushes.Red
                        };
                        break;

                    case EnemyType.Mage:
                        Speed = 2;
                        Health = 1;
                        Visual = new Ellipse
                        {
                            Width = 30,
                            Height = 30,
                            Fill = Brushes.Orange
                        };
                        break;

                    case EnemyType.Tank:
                        Speed = 1;
                        Health = 3;
                        Visual = new Ellipse
                        {
                            Width = 50,
                            Height = 50,
                            Fill = Brushes.Green
                        };
                        break;

                    case EnemyType.Spider:  // Nowy typ - Spider
                        Speed = 1.5;
                        Health = 2;
                        Visual = new Ellipse
                        {
                            Width = 35,
                            Height = 35,
                            Fill = Brushes.Purple
                        };
                        break;
                }

                // Ustawiamy początkową pozycję przeciwnika
                Canvas.SetLeft(Visual, X);
                Canvas.SetTop(Visual, Y);
            }

            // Metoda do poruszania przeciwnika
            public void Move()
            {
                if (Type == EnemyType.Spider)
                {
                    // Spider porusza się w losowy sposób (prawo, lewo)
                    if (_random.Next(2) == 0)
                    {
                        X += Speed;
                    }
                    else
                    {
                        X -= Speed;
                    }
                }
                else
                {
                    // Standardowy ruch w prawo/lewo dla innych typów
                    if (MovingRight)
                    {
                        X += Speed;
                        if (X > 800 - Visual.Width)  // Gdy przeciwnik osiągnie prawą krawędź ekranu
                        {
                            MovingRight = false;
                            Y += 20;  // Zmniejszamy wysokość, aby przemieszczać go w dół
                        }
                    }
                    else
                    {
                        X -= Speed;
                        if (X < 0)  // Gdy przeciwnik osiągnie lewą krawędź ekranu
                        {
                            MovingRight = true;
                            Y += 20;  // Zmniejszamy wysokość, aby przemieszczać go w dół
                        }
                    }
                }

                Canvas.SetLeft(Visual, X);
                Canvas.SetTop(Visual, Y);
            }

            // Metoda do zranienia przeciwnika
            public void TakeDamage(int damage)
            {
                Health -= damage;
                if (Health <= 0)
                {
                    // Przeciwnik zginął, można dodać logikę usuwania go z planszy
                    // Możesz dodać metodę, która usunie go z canvas
                }
            }

            // Opcjonalnie: Metoda do sprawdzenia kolizji z pociskiem gracza
            public bool CheckCollision(Rect playerBullet)
            {
                Rect enemyBounds = new Rect(X, Y, Visual.Width, Visual.Height);
                return playerBullet.IntersectsWith(enemyBounds);
            }

            // Funkcja do strzelania przez Spidera i Mage
            public void Shoot(Canvas gameCanvas)
            {
                if (Type == EnemyType.Spider || Type == EnemyType.Mage)
                {
                    // Przeciwnik zaczyna strzelać co pewien czas (np. co 100ms)
                    if (_random.Next(100) < 10)  // Procent prawdopodobieństwa, że strzeli
                    {
                        var bullet = new Rectangle
                        {
                            Width = 5,
                            Height = 15,
                            Fill = Brushes.Yellow,
                            RadiusX = 5,
                            RadiusY = 5
                        };

                        // Ustawienie pocisku na aktualnej pozycji przeciwnika
                        Canvas.SetLeft(bullet, X + (Visual.Width / 2) - (bullet.Width / 2));
                        Canvas.SetTop(bullet, Y + Visual.Height);  // Pocisk wystrzelony z dołu przeciwnika

                        // Dodanie pocisku do listy
                        Bullets.Add(bullet);

                        // Dodanie pocisku do Canvas
                        gameCanvas.Children.Add(bullet);
                    }
                }
            }

            // Aktualizacja pozycji pocisków
            public void UpdateBullets()
            {
                var bulletsToRemove = new List<Rectangle>();

                foreach (var bullet in Bullets)
                {
                    // Ruch pocisku w dół
                    double top = Canvas.GetTop(bullet);
                    Canvas.SetTop(bullet, top + 5);

                    // Sprawdzenie, czy pocisk opuścił ekran (usuwa go, jeśli tak)
                    if (Canvas.GetTop(bullet) > 600)  // Załóżmy, że wysokość ekranu to 600
                    {
                        bulletsToRemove.Add(bullet);
                    }
                }

                // Usunięcie pocisków, które opuściły ekran
                foreach (var bullet in bulletsToRemove)
                {
                    Bullets.Remove(bullet);
                }
            }
        }
    }
}
