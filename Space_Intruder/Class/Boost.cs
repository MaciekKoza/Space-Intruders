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

    public class Boost
    {
        public Image Visual { get; private set; }
        public BoostType Type { get; private set; }
        public double X { get; private set; }
        public double Y { get; private set; }
        private Canvas canvas;
        private DispatcherTimer fallTimer;
        private DispatcherTimer effectTimer;
        private Hero hero;
        private Level_Gry level;

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

            string imagePath = GetImagePathForType(type);
            try
            {
                Visual.Source = new BitmapImage(new Uri(imagePath, UriKind.Relative));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd ładowania obrazka: {imagePath}\n{ex.Message}");
            }

            Canvas.SetLeft(Visual, x);
            Canvas.SetBottom(Visual, y);
            canvas.Children.Add(Visual);

            fallTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(10)
            };
            fallTimer.Tick += (s, e) => UpdatePosition(hero);
            fallTimer.Start();
        }

        private string GetImagePathForType(BoostType type)
        {
            return type switch
            {
                BoostType.Shield => "Images/tarcza.png",
                BoostType.Freeze => "Images/kostka.png",
                BoostType.TripleShot => "Images/triple.png",
                _ => "Images/default_boost.png"
            };
        }

        private void Activate()
        {
            fallTimer.Stop();

            switch (Type)
            {
                case BoostType.Shield:
                    /*hero.IsShielded = true;
                    effectTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
                    effectTimer.Tick += (s, e) =>
                    {
                        hero.IsShielded = false;
                        effectTimer.Stop();
                        ShowEffectEndedMessage("Tarcza wygasła!");
                    };
                    effectTimer.Start();
                    ShowEffectActivatedMessage("Aktywowano tarczę!");*/
                    break;

                case BoostType.Freeze:
                    FreezeEnemies();
                    break;

                case BoostType.TripleShot:
                    hero.IsTripleShot = true;
                    effectTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(7) };
                    effectTimer.Tick += (s, e) =>
                    {
                        hero.IsTripleShot = false;
                        effectTimer.Stop();
                        ShowEffectEndedMessage("Triple shot wygasł!");
                    };
                    effectTimer.Start();
                    ShowEffectActivatedMessage("Aktywowano triple shot!");
                    break;
            }

            RemoveFromCanvas();
        }

        private void FreezeEnemies()
        {
            level.StopAllEnemies();
            ShowEffectActivatedMessage("Zamrożono przeciwników!");

            effectTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };
            effectTimer.Tick += (s, e) => free();
            effectTimer.Start();
        }

        private void free()
        {
            foreach (var ene in level.GetCurrentEnemies())
            {
                ene.IsFrozen = false;
            }
            ShowEffectEndedMessage("Efekt zamrożenia zakończony!");
        }

        private void ShowEffectActivatedMessage(string message)
        {
            MessageBox.Show(message, "Boost aktywowany", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ShowEffectEndedMessage(string message)
        {
            MessageBox.Show(message, "Boost zakończony", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public bool UpdatePosition(Hero hero)
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

        private void RemoveFromCanvas()
        {
            fallTimer?.Stop();
            effectTimer?.Stop();
            canvas.Children.Remove(Visual);
        }
    }
}