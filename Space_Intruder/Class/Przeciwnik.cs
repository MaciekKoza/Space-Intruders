using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Space_Intruder.Class
{
    public class Przeciwnik
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
            public double X { get; set; }
            public double Y { get; set; }
            public double Speed { get; set; }
            public int Health { get; set; }
            public Image Visual { get; set; }
            public EnemyType Type { get; set; }
            public bool MovingRight { get; set; }
            public bool IsShooting { get; set; }
            public List<Rectangle> Bullets { get; set; }
            private Random _random;

            public Enemy(double x, double y, EnemyType type)
            {
                X = x;
                Y = y;
                Type = type;
                MovingRight = true;
                Bullets = new List<Rectangle>();
                _random = new Random();
                Visual = new Image
                {
                    Width = 60,
                    Height = 60
                };

                // Ustawienia przeciwników
                switch (Type)
                {
                    case EnemyType.Basic:
                        Speed = 2;
                        Health = 5;
                        break;
                    case EnemyType.Mage:
                        Speed = 2;
                        Health = 4;
                        break;
                    case EnemyType.Tank:
                        Speed = 1;
                        Health = 10;
                        break;
                    case EnemyType.Spider:
                        Speed = 1.5;
                        Health = 7;
                        break;
                }

                // Ładowanie obrazu przeciwnika
                string imagePath = GetEnemyImagePath(Type);
                if (!string.IsNullOrEmpty(imagePath))
                {
                    Visual.Source = new BitmapImage(new Uri(imagePath, UriKind.RelativeOrAbsolute));
                }

                Canvas.SetLeft(Visual, X);
                Canvas.SetTop(Visual, Y);
            }

            private string GetEnemyImagePath(EnemyType type)
            {
                string basePath = "pack://application:,,,/Images/";
                string fileName = type switch
                {
                    EnemyType.Basic => "nietoperz.png",
                    EnemyType.Mage => "czarodziej.png",
                    EnemyType.Tank => "boss.png",
                    EnemyType.Spider => "pajak.png",
                    _ => null
                };

                return fileName != null ? basePath + fileName : null;
            }

            public void Move()
            {
                if (Type == EnemyType.Spider)
                {
                    X += _random.Next(2) == 0 ? Speed : -Speed;
                }
                else
                {
                    if (MovingRight)
                    {
                        X += Speed;
                        if (X > 800 - Visual.Width)
                        {
                            MovingRight = false;
                            Y += 20;
                        }
                    }
                    else
                    {
                        X -= Speed;
                        if (X < 0)
                        {
                            MovingRight = true;
                            Y += 20;
                        }
                    }
                }

                Canvas.SetLeft(Visual, X);
                Canvas.SetTop(Visual, Y);
            }

            public bool CheckCollision(Rect playerBullet)
            {
                Rect enemyBounds = new Rect(X, Y, Visual.Width, Visual.Height);
                return playerBullet.IntersectsWith(enemyBounds);
            }

            public void Shoot(Canvas gameCanvas)
            {
                if (Type == EnemyType.Spider || Type == EnemyType.Mage)
                {
                    if (_random.Next(100) < 10)
                    {
                        var bullet = new Rectangle
                        {
                            Width = 5,
                            Height = 15,
                            Fill = Brushes.Yellow,
                            RadiusX = 5,
                            RadiusY = 5
                        };

                        Canvas.SetLeft(bullet, X + (Visual.Width / 2) - (bullet.Width / 2));
                        Canvas.SetTop(bullet, Y + Visual.Height);
                        Bullets.Add(bullet);
                        gameCanvas.Children.Add(bullet);
                    }
                }
            }

            public void TakeDamage(int damage)
            {
                Health -= damage;
                if (Health <= 0)
                {
                    Visual.Visibility = Visibility.Hidden;
                }
            }


            public void UpdateBullets()
            {
                var bulletsToRemove = new List<Rectangle>();

                foreach (var bullet in Bullets)
                {
                    double top = Canvas.GetTop(bullet);
                    Canvas.SetTop(bullet, top + 5);
                    if (Canvas.GetTop(bullet) > 600)
                    {
                        bulletsToRemove.Add(bullet);
                    }
                }

                foreach (var bullet in bulletsToRemove)
                {
                    Bullets.Remove(bullet);
                }
            }
        }
    }
}
