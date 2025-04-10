using Space_Intruder.GameObjects;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
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

    // Bazowa klasa Boost (nieznacznie zmodyfikowana)
    public abstract class Boost
    {
        public Image Visual { get; protected set; }
        public BoostType Type { get; protected set; }
        public double X { get; protected set; }
        public double Y { get; protected set; }
        protected Canvas canvas;
        protected DispatcherTimer fallTimer;
        protected Hero hero;
        protected Level_Gry level;

        public Boost(Canvas canvas, double x, double y, BoostType type, Hero hero, Level_Gry level)
        {
            this.canvas = canvas;
            this.X = x;
            this.Y = y;
            this.Type = type;
            this.hero = hero;
            this.level = level;

            Visual = new Image
            {
                Width = 40,
                Height = 40,
                Stretch = Stretch.Uniform
            };

            Visual.Source = new BitmapImage(new Uri(GetImagePathForType(type), UriKind.Relative));
            Canvas.SetLeft(Visual, x);
            Canvas.SetBottom(Visual, y);
            canvas.Children.Add(Visual);

            fallTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(10) };
            fallTimer.Tick += (s, e) => UpdatePosition();
            fallTimer.Start();
        }

        protected virtual string GetImagePathForType(BoostType type)
        {
            return type switch
            {
                BoostType.Shield => "Images/tarcza.png",
                BoostType.Freeze => "Images/kostka.png",
                BoostType.TripleShot => "Images/triple.png",
                _ => "Images/default_boost.png"
            };
        }

        public abstract void Activate();

        public bool UpdatePosition()
        {
            Y -= 2;
            Canvas.SetBottom(Visual, Y);

            Rect boostRect = new Rect(
                Canvas.GetLeft(Visual),
                Canvas.GetBottom(Visual),
                Visual.Width,
                Visual.Height);

            Rect heroRect = new Rect(
                hero.PositionX,
                hero.PositionY,
                hero.Width,
                hero.Height);

            if (boostRect.IntersectsWith(heroRect))
            {
                Activate();
                return true;
            }

            if (Y <= 0)
            {
                RemoveFromCanvas();
                return true;
            }

            return false;
        }

        protected void RemoveFromCanvas()
        {
            fallTimer?.Stop();
            canvas.Children.Remove(Visual);
        }

    }

    // Klasa dla Shield Boost
    public class ShieldBoost : Boost
    {
        private DispatcherTimer effectTimer;

        public ShieldBoost(Canvas canvas, double x, double y, Hero hero, Level_Gry level)
            : base(canvas, x, y, BoostType.Shield, hero, level) { }

        public override void Activate()
        {
            RemoveFromCanvas();
            hero.IsShielded = true;

            // Nadajemy efekt wizualny - żółty filtr na bohaterze
            hero.Visual.OpacityMask = new SolidColorBrush(Color.FromArgb(200, 255, 255, 255));

            effectTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
            effectTimer.Tick += (s, e) =>
            {
                hero.IsShielded = false;
                hero.Visual.OpacityMask = null; // Usuwamy filtr
                effectTimer.Stop();
            };
            effectTimer.Start();
        }

    }

    // Klasa dla Freeze Boost
    public class FreezeBoost : Boost
    {
        private DispatcherTimer effectTimer;

        private Rectangle freezeOverlay;

        public FreezeBoost(Canvas canvas, double x, double y, Hero hero, Level_Gry level)
            : base(canvas, x, y, BoostType.Freeze, hero, level) { }

        public override void Activate()
        {
            RemoveFromCanvas();
            level.StopAllEnemies();

            // Tworzymy efekt zamrożenia (półprzezroczysty niebieski overlay)
            freezeOverlay = new Rectangle
            {
                Width = canvas.ActualWidth,
                Height = canvas.ActualHeight,
                Fill = new SolidColorBrush(Color.FromArgb(100, 100, 150, 255)), // Lekki niebieski
                IsHitTestVisible = false
            };
            Canvas.SetLeft(freezeOverlay, 0);
            Canvas.SetTop(freezeOverlay, 0);
            canvas.Children.Add(freezeOverlay);

            effectTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };
            effectTimer.Tick += (s, e) =>
            {
                level.ResumeAllEnemies();
                effectTimer.Stop();

                // Usuwamy efekt zamrożenia
                canvas.Children.Remove(freezeOverlay);
            };
            effectTimer.Start();
        }

    }

    // Klasa dla TripleShot Boost
    public class TripleShotBoost : Boost
    {
        private DispatcherTimer effectTimer;

        public TripleShotBoost(Canvas canvas, double x, double y, Hero hero, Level_Gry level)
            : base(canvas, x, y, BoostType.TripleShot, hero, level) { }

        public override void Activate()
        {
            RemoveFromCanvas();
            hero.IsTripleShot = true;

            effectTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(7) };
            effectTimer.Tick += (s, e) =>
            {
                hero.IsTripleShot = false;
                effectTimer.Stop();
            };
            effectTimer.Start();

        }
    }
}