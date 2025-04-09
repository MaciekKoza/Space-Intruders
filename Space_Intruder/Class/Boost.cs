using Space_Intruder.GameObjects;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Space_Intruder.Class
{
    public enum BoostType
    {
        Shield,
        Freeze,
        TripleShot
    }

    public class Boost
    {
        public Ellipse Visual { get; private set; }
        public BoostType Type { get; private set; }
        public double X { get; private set; }
        public double Y { get; private set; }
        private Canvas canvas;
        private DispatcherTimer fallTimer;
        private DispatcherTimer effectTimer;
        private Hero hero;
        private Level_Gry level;

        // Konstruktor Boost
        public Boost(Canvas canvas, double x, double y, BoostType type, Hero hero, Level_Gry level)
        {
            this.canvas = canvas;
            this.X = x;
            this.Y = y;
            this.Type = type;
            this.hero = hero;
            this.level = level;

            Visual = new Ellipse
            {
                Width = 30,
                Height = 30,
                Fill = GetColorForType(type)
            };

            Canvas.SetLeft(Visual, x);
            Canvas.SetBottom(Visual, y);
            canvas.Children.Add(Visual);

            // Dodaj timer opadania
            fallTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(20)  // Standardowa wartość dla płynności animacji
            };
            fallTimer.Tick += (s, e) => UpdatePosition(hero);
            fallTimer.Start();
        }


        private Brush GetColorForType(BoostType type)
        {
            return type switch
            {
                BoostType.Shield => Brushes.Cyan,
                BoostType.Freeze => Brushes.LightBlue,
                BoostType.TripleShot => Brushes.Orange,
                _ => Brushes.White
            };
        }

        private bool CheckCollisionWithHero()
        {
            double boostLeft = Canvas.GetLeft(Visual);
            double boostRight = boostLeft + Visual.Width;
            double heroLeft = Canvas.GetLeft(hero.Visual);
            double heroRight = heroLeft + hero.Visual.Width;

            return Y <= Canvas.GetBottom(hero.Visual) + hero.Visual.Height &&
                   boostRight >= heroLeft && boostLeft <= heroRight;
        }

        private void Activate()
        {
            fallTimer.Stop();

            switch (Type)
            {
                case BoostType.Shield:
                    hero.IsShielded = true;
                    effectTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
                    effectTimer.Tick += (s, e) =>
                    {
                        hero.IsShielded = false;
                        effectTimer.Stop();
                    };
                    effectTimer.Start();
                    break;

                case BoostType.Freeze:
                    level.StopAllEnemies();
                    effectTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };
                    effectTimer.Tick += (s, e) =>
                    {
                        level.ResumeAllEnemies();
                        effectTimer.Stop();
                    };
                    effectTimer.Start();
                    break;

                case BoostType.TripleShot:
                    hero.IsTripleShot = true;
                    effectTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(7) };
                    effectTimer.Tick += (s, e) =>
                    {
                        hero.IsTripleShot = false;
                        effectTimer.Stop();
                    };
                    effectTimer.Start();
                    break;
            }

            RemoveFromCanvas();
        }

        public bool UpdatePosition(Hero hero)
        {
            Y -= 2;  // Move boost down
            Canvas.SetBottom(Visual, Y);  // Update the position on screen

            // Get boost coordinates
            double boostLeft = Canvas.GetLeft(Visual);
            double boostRight = boostLeft + Visual.Width;
            double boostBottom = Y;
            double boostTop = boostBottom + Visual.Height;  // Since Y is the bottom position

            // Get hero coordinates
            double heroLeft = Canvas.GetLeft(hero.Visual);
            double heroRight = heroLeft + hero.Visual.Width;
            double heroBottom = Canvas.GetBottom(hero.Visual);
            double heroTop = heroBottom + hero.Visual.Height;

            // Check for collision
            bool isCollected = boostRight >= heroLeft &&
                              boostLeft <= heroRight &&
                              boostBottom <= heroTop &&
                              boostTop >= heroBottom;

            if (isCollected)
            {
                Activate();
                MessageBox.Show($"Boost {Type} collected!");
                return true;
            }

            // Check if boost reached bottom of screen
            if (Y <= 0)
            {
                RemoveFromCanvas();
                return true;
            }

            return false;
        }

        private void RemoveFromCanvas()
        {
            fallTimer?.Stop();
            effectTimer?.Stop();
            canvas.Children.Remove(Visual);
        }
    }
}

