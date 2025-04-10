using Space_Intruder.Class;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using WpfAnimatedGif;

namespace Space_Intruder.GameObjects
{
    public class Hero
    {
        public Image Visual { get; private set; }  // Changed from Rectangle to Image

        public UIElement PlayerVisual => Visual;
        public double PositionX { get; private set; }
        public double PositionY { get; private set; }
        public double Width => Visual.Width;
        public double Height => Visual.Height;
        public event EventHandler LivesChanged;

        public int _lives;
        private int _maxLives = 5;
        public int MaxLives
        {
            get => _maxLives;
            set => _maxLives = value;
        }
        public bool IsAlive => _lives > 0;

        public bool IsFrozen { get; set; }

        public bool IsShielded { get; set; } = false;
        public bool IsTripleShot { get; set; } = false;


        // Stats
        public double _movementSpeed = 25;
        private int _damage = 2;
        private double _attackSpeed = 1.5; // attacks per second
        private DispatcherTimer _attackTimer;
        private bool _canAttack = true;

        // Animation
        private readonly Storyboard _moveStoryboard;
        private readonly DoubleAnimation _moveAnimation;

        private Level_Gry _levelGry; // przechowujemy referencję
        private MainWindow _mainWindow;

        public Hero(Canvas gameCanvas, double initialX, double initialY, Level_Gry levelGry, MainWindow mainWindow)
        {
            _mainWindow = mainWindow;
            Visual = new Image
            {
                Width = 50,
                Height = 50,
                Stretch = Stretch.Fill
            };
            LoadAnimatedGif("Video/Hero.gif");  // Assuming you have a hero.gif in your Video folder

            _levelGry = levelGry;
            PositionX = initialX;
            PositionY = initialY;
            _lives = 3;

            gameCanvas.Children.Add(Visual);
            Canvas.SetLeft(Visual, PositionX);
            Canvas.SetBottom(Visual, PositionY);

            // Initialize movement animation
            _moveStoryboard = new Storyboard();
            _moveAnimation = new DoubleAnimation
            {
                Duration = TimeSpan.FromMilliseconds(200),
                EasingFunction = new QuadraticEase()
            };
            Storyboard.SetTarget(_moveAnimation, Visual);
            Storyboard.SetTargetProperty(_moveAnimation, new PropertyPath("(Canvas.Left)"));
            _moveStoryboard.Children.Add(_moveAnimation);

            // Initialize attack timer
            _attackTimer = new DispatcherTimer();
            _attackTimer.Interval = TimeSpan.FromSeconds(1 / _attackSpeed);
            _attackTimer.Tick += (s, e) => _canAttack = true;
        }

        private void LoadAnimatedGif(string path)
        {
            try
            {
                var imageUri = new Uri(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path));
                var imageSource = new BitmapImage(imageUri);
                ImageBehavior.SetAnimatedSource(Visual, imageSource);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading hero GIF: {ex.Message}");
                // Fallback to a solid color if GIF fails to load
                Visual.Source = new BitmapImage(new Uri("pack://application:,,,/Images/boss.png"));
            }
        }

        public void MoveLeft(double gameAreaLeft)
        {
            if (PositionX > gameAreaLeft)
            {
                MoveTo(PositionX - _movementSpeed);
            }
        }

        public void MoveRight(double gameAreaRight)
        {
            if (PositionX < gameAreaRight - Width)
            {
                MoveTo(PositionX + _movementSpeed);
            }
        }

        private void MoveTo(double newX)
        {
            _moveStoryboard.Stop();
            _moveAnimation.To = newX;
            _moveStoryboard.Begin();
            PositionX = newX;
        }

        public void Shoot(Canvas gameCanvas, List<Enemy> enemies)
        {
            if (!_canAttack || !IsAlive) return;

            double bulletX = PositionX + Width / 2;
            double bulletY = PositionY + Height;

            var bullet = new Pocisk(
                "bohater",
                _damage,
                5,
                bulletX,
                bulletY,
                gameCanvas,
                enemies,
                this,  // ✅ przekazanie obiektu Hero
                _levelGry,  // ✅ przekazanie Level_Gry
                1
            );

            _canAttack = false;
            _attackTimer.Start();
        }

        public void Damage()
        {
            _mainWindow.PlayerHit();
        }

        public void TakeDamage()
        {
            _lives--;
        }

        public void ApplySlowEffect(double duration = 2.0, double slowFactor = 0.5)
        {
            double originalSpeed = _movementSpeed;
            _movementSpeed *= slowFactor;

            var restoreSpeedTimer = new DispatcherTimer();
            restoreSpeedTimer.Interval = TimeSpan.FromSeconds(duration);
            restoreSpeedTimer.Tick += (s, e) =>
            {
                _movementSpeed = originalSpeed;
                restoreSpeedTimer.Stop();
            };
            restoreSpeedTimer.Start();
        }

        public void UpgradeStat(string stat, double value)
        {
            switch (stat.ToLower())
            {
                case "damage":
                    _damage += (int)value;
                    break;
                case "attackspeed":
                    _attackSpeed += value;
                    _attackTimer.Interval = TimeSpan.FromSeconds(1 / _attackSpeed);
                    break;
                case "movementspeed":
                    _movementSpeed += value;
                    break;
                case "health":
                    _lives += (int)value;
                    break;
            }
        }
    }
}