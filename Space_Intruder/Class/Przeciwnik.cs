using Space_Intruder.GameObjects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using WpfAnimatedGif;

namespace Space_Intruder.Class
{
    public abstract class Enemy
    {
        public Image Visual { get; protected set; }
        public EnemyType Type { get; protected set; }
        public double Speed { get; protected set; } = 2;
        public int Direction { get; set; } = 1;
        public int Health { get; set; }
        public int BaseHealth { get; set; } = 1;
        public double BaseSpeed { get; set; } = 2;
        public double BaseAttackRate { get; set; } = 1.0;
        public event Action<Enemy> OnEnemyDied;

        protected int currentLevel = 1;
        private ScaleTransform _flipTransform;
        protected DispatcherTimer movementTimer;

        public bool ShouldMove { get; protected set; } = true;
        public bool IsFrozen { get; set; }

        public Canvas canvas;
        protected List<Pocisk> activeBullets = new List<Pocisk>(); // Changed from _activeBullets to match usage below

        public Enemy(double x, double y, EnemyType type, int health, int level, Canvas _canvas)
        {
            Type = type;
            currentLevel = level;
            BaseHealth = health;
            Health = CalculateScaledHealth(health, level);
            Speed = CalculateScaledSpeed(BaseSpeed, level);

            canvas = _canvas;

            Visual = new Image
            {
                Width = 50,
                Height = 50,
                Stretch = Stretch.Fill
            };
            LoadAnimatedGif($"Video/{type.ToString()}.gif");

            _flipTransform = new ScaleTransform { ScaleX = 1 };
            Visual.RenderTransformOrigin = new Point(0.5, 0.5);
            Visual.RenderTransform = _flipTransform;

            Canvas.SetLeft(Visual, x);
            Canvas.SetBottom(Visual, y);

            movementTimer = new DispatcherTimer();
            movementTimer.Interval = TimeSpan.FromMilliseconds(16);
            movementTimer.Tick += (s, e) => Move();
            movementTimer.Start();
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
                MessageBox.Show($"Error loading GIF: {ex.Message}");
                Visual.Source = new BitmapImage(new Uri("pack://application:,,,/Images/default_enemy.png"));
            }
        }

        public virtual void Move()
        {
            if (!IsFrozen && ShouldMove)
            {
                double newX = Canvas.GetLeft(Visual) + Speed * Direction;
                Canvas.SetLeft(Visual, newX);

                if ((Direction > 0 && _flipTransform.ScaleX < 0) ||
                    (Direction < 0 && _flipTransform.ScaleX > 0))
                {
                    _flipTransform.ScaleX *= -1;
                }
            }
        }

        public virtual void TakeDamage(int damage)
        {
            Health -= damage;
            FlashDamage();
            if (Health <= 0)
            {
                OnEnemyDied?.Invoke(this);
            }
        }

        protected virtual void FlashDamage()
        {
            var originalOpacity = Visual.Opacity;
            Visual.Opacity = 0.5;

            var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
            timer.Tick += (s, e) =>
            {
                Visual.Opacity = originalOpacity;
                timer.Stop();
            };
            timer.Start();
        }

        protected int CalculateScaledHealth(int baseHealth, int level)
        {
            return baseHealth + (int)Math.Ceiling(level * 0.5);
        }

        protected double CalculateScaledSpeed(double baseSpeed, int level)
        {
            return baseSpeed * (1 + (level * 0.05));
        }

        protected double CalculateScaledAttackRate(double baseRate, int level)
        {
            return Math.Max(0.1, baseRate * Math.Pow(0.9, level));
        }

        public void StopMovement()
        {
            ShouldMove = false;
            movementTimer?.Stop();
        }

        public void ResumeMovement()
        {
            ShouldMove = true;
            if (movementTimer != null && !movementTimer.IsEnabled)
            {
                movementTimer.Start();
            }
        }

        public void ClearAllBullets()
        {
            foreach (var bullet in activeBullets)
            {
                if (bullet.Visual != null && canvas.Children.Contains(bullet.Visual))
                {
                    canvas.Children.Remove(bullet.Visual);
                }
            }
            activeBullets.Clear();
        }
    }

    public enum EnemyType
    {
        Basic,
        Mage,
        Tank,
        Spider,
        Boss,
        HeavyMage
    }

    public class BasicEnemy : Enemy
    {
        public BasicEnemy(double x, double y, int level, Canvas canvas)
            : base(x, y, EnemyType.Basic, 1, level, canvas)
        {
            BaseHealth = 1;
            BaseSpeed = 2.0;
            Health = CalculateScaledHealth(BaseHealth, level);
            Speed = CalculateScaledSpeed(BaseSpeed, level);
        }
    }

    public class TankEnemy : Enemy
    {
        public TankEnemy(double x, double y, int level, Canvas canvas)
            : base(x, y, EnemyType.Tank, 3, level, canvas)
        {
            BaseHealth = 3;
            BaseSpeed = 1.5;
            Health = CalculateScaledHealth(BaseHealth, level);
            Speed = CalculateScaledSpeed(BaseSpeed, level);
        }
    }

    public class MageEnemy : Enemy
    {
        protected DispatcherTimer shootTimer;
        protected Hero player;
        protected double attackRate;
        protected Level_Gry level_g;

        public MageEnemy(double x, double y, Canvas canvas, Hero player, int level, Level_Gry level_g)
            : base(x, y, EnemyType.Mage, 1, level, canvas)
        {
            this.level_g = level_g;
            this.player = player;
            BaseHealth = 1;
            BaseSpeed = 1.8;
            BaseAttackRate = 1.0;

            Health = CalculateScaledHealth(BaseHealth, level);
            Speed = CalculateScaledSpeed(BaseSpeed, level);
            attackRate = CalculateScaledAttackRate(BaseAttackRate, level);

            InitializeShooting();
        }

        protected virtual void InitializeShooting()
        {
            shootTimer = new DispatcherTimer();
            shootTimer.Interval = TimeSpan.FromSeconds(attackRate);
            shootTimer.Tick += ShootTimer_Tick;
            shootTimer.Start();
        }

        protected virtual void ShootTimer_Tick(object sender, EventArgs e)
        {
            if (!IsFrozen) Shoot();
        }

        protected virtual void Shoot()
        {
            double startX = Canvas.GetLeft(Visual) + Visual.Width / 2;
            double startY = Canvas.GetBottom(Visual);
            new Pocisk("mag", 1, 5 + (currentLevel * 0.5), startX, startY, canvas, new List<Enemy>(), player, level_g, -1);
        }

        public void StopShooting()
        {
            shootTimer?.Stop();
        }

        public void StartShooting()
        {
            if (shootTimer != null && !shootTimer.IsEnabled)
            {
                shootTimer.Interval = TimeSpan.FromSeconds(attackRate);
                shootTimer.Start();
            }
        }

        public override void TakeDamage(int damage)
        {
            base.TakeDamage(damage);
            if (Health <= 0) StopShooting();
        }
    }

    public class SpiderEnemy : Enemy
    {
        private DispatcherTimer shootTimer;
        private Hero player;
        private double attackRate;
        private Level_Gry level_g;

        public SpiderEnemy(double x, double y, Canvas canvas, Hero player, int level, Level_Gry level_g)
            : base(x, y, EnemyType.Spider, 1, level, canvas)
        {
            this.level_g = level_g;
            this.player = player;
            BaseHealth = 1;
            BaseSpeed = 2.2;
            BaseAttackRate = 2.0;

            Health = CalculateScaledHealth(BaseHealth, level);
            Speed = CalculateScaledSpeed(BaseSpeed, level);
            attackRate = CalculateScaledAttackRate(BaseAttackRate, level);

            InitializeShooting();
        }

        private void InitializeShooting()
        {
            shootTimer = new DispatcherTimer();
            shootTimer.Interval = TimeSpan.FromSeconds(attackRate);
            shootTimer.Tick += ShootTimer_Tick;
            shootTimer.Start();
        }

        private void ShootTimer_Tick(object sender, EventArgs e)
        {
            if (!IsFrozen) Shoot();
        }

        public void Shoot()
        {
            double startX = Canvas.GetLeft(Visual) + Visual.Width / 2;
            double startY = Canvas.GetBottom(Visual);
            new Pocisk("spider", 1, 5 + (currentLevel * 0.3), startX, startY, canvas, new List<Enemy>(), player, level_g, -1);
        }

        public void StopShooting()
        {
            shootTimer?.Stop();
        }

        public void StartShooting()
        {
            if (shootTimer != null && !shootTimer.IsEnabled)
            {
                shootTimer.Interval = TimeSpan.FromSeconds(attackRate);
                shootTimer.Start();
            }
        }

        public override void TakeDamage(int damage)
        {
            base.TakeDamage(damage);
            if (Health <= 0) StopShooting();
        }
    }

    public class BossEnemy : TankEnemy
    {
        private DispatcherTimer shootTimer;
        private Hero player;
        private Level_Gry level_g;

        public BossEnemy(double x, double y, Canvas canvas, Hero player, int level, Level_Gry level_g)
            : base(x, y, level, canvas)
        {
            this.level_g = level_g;
            this.player = player;
            BaseHealth = 30;
            BaseSpeed = 1.2;
            Health = CalculateScaledHealth(BaseHealth, level);
            Speed = CalculateScaledSpeed(BaseSpeed, level);

            Visual.Width = 100;
            Visual.Height = 100;

            InitializeShooting();
        }

        private void InitializeShooting()
        {
            shootTimer = new DispatcherTimer();
            shootTimer.Interval = TimeSpan.FromSeconds(4.0);
            shootTimer.Tick += ShootTimer_Tick;
            shootTimer.Start();
        }

        private void ShootTimer_Tick(object sender, EventArgs e)
        {
            if (!IsFrozen) Shoot();
        }

        private void Shoot()
        {
            double startX = Canvas.GetLeft(Visual) + Visual.Width / 2;
            double startY = Canvas.GetBottom(Visual);
            new Pocisk("boss", 3, 6, startX, startY, canvas, new List<Enemy>(), player, level_g, -1, 20, 50);
        }

        public void StopShooting()
        {
            shootTimer?.Stop();
        }

        public void StartShooting()
        {
            if (shootTimer != null && !shootTimer.IsEnabled)
            {
                shootTimer.Interval = TimeSpan.FromSeconds(1.2);
                shootTimer.Start();
            }
        }

        public override void TakeDamage(int damage)
        {
            base.TakeDamage(damage);
            if (Health <= 0) StopShooting();
        }
    }

    public class HeavyMageEnemy : MageEnemy
    {
        public HeavyMageEnemy(double x, double y, Canvas canvas, Hero player, int level, Level_Gry level_g)
            : base(x, y, canvas, player, level, level_g)
        {
            Type = EnemyType.HeavyMage;
            BaseHealth = 6;
            BaseAttackRate = 2.0;
            Health = CalculateScaledHealth(BaseHealth, level);
            Speed = CalculateScaledSpeed(BaseSpeed, level);

            Visual.Width = 60;
            Visual.Height = 60;
        }

        protected override void Shoot()
        {
            double startX = Canvas.GetLeft(Visual) + Visual.Width / 2;
            double startY = Canvas.GetBottom(Visual);
            new Pocisk("mag", 2, 5 + (currentLevel * 0.5), startX, startY, canvas, new List<Enemy>(), player, level_g, -1, 10, 30);
        }
    }
}