using System;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Space_Intruder.Class
{
    public class Enemy
    {
        public Image Visual { get; private set; }
        public EnemyType Type { get; private set; }
        public double Speed { get; protected set; } = 2;
        public int Direction { get; set; } = 1;
        public int Health { get; set; }
        public int BaseHealth { get; protected set; } = 1;
        public double BaseSpeed { get; protected set; } = 2;
        public double BaseAttackRate { get; protected set; } = 1.0;

        protected string imagePath;
        protected int currentLevel = 1;

        public Enemy(double x, double y, EnemyType type, int health, int level)
        {
            Type = type;
            currentLevel = level;
            BaseHealth = health;
            Health = CalculateScaledHealth(health, level);
            Speed = CalculateScaledSpeed(BaseSpeed, level);

            imagePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images", $"{type.ToString()}.png");

            Visual = new Image
            {
                Width = 50,
                Height = 50,
                Source = new BitmapImage(new Uri(imagePath))
            };

            Canvas.SetLeft(Visual, x);
            Canvas.SetBottom(Visual, y);
        }

        protected int CalculateScaledHealth(int baseHealth, int level)
        {
            // Health increases by 0.5 per level (rounded up)
            return baseHealth + (int)Math.Ceiling(level * 0.5);
        }

        protected double CalculateScaledSpeed(double baseSpeed, int level)
        {
            // Speed increases by 5% per level
            return baseSpeed * (1 + (level * 0.05));
        }

        protected double CalculateScaledAttackRate(double baseRate, int level)
        {
            // Attack rate increases by 10% per level (lower number = faster)
            return Math.Max(0.1, baseRate * Math.Pow(0.9, level));
        }

        public void Move()
        {
            double newX = Canvas.GetLeft(Visual) + Speed * Direction;
            Canvas.SetLeft(Visual, newX);
        }
    }

    public enum EnemyType
    {
        Basic,
        Mage,
        Tank,
        Spider
    }

    public class BasicEnemy : Enemy
    {
        public BasicEnemy(double x, double y, int level)
            : base(x, y, EnemyType.Basic, 1, level)
        {
            BaseHealth = 1;
            BaseSpeed = 2.0;
            Health = CalculateScaledHealth(BaseHealth, level);
            Speed = CalculateScaledSpeed(BaseSpeed, level);
        }
    }

    public class TankEnemy : Enemy
    {
        public TankEnemy(double x, double y, int level)
            : base(x, y, EnemyType.Tank, 3, level)
        {
            BaseHealth = 3;
            BaseSpeed = 1.5;
            Health = CalculateScaledHealth(BaseHealth, level);
            Speed = CalculateScaledSpeed(BaseSpeed, level);
        }
    }

    public class MageEnemy : Enemy
    {
        private Canvas canvas;
        private DispatcherTimer shootTimer;
        private Rectangle player;
        private double attackRate;

        public MageEnemy(double x, double y, Canvas canvas, Rectangle player, int level)
            : base(x, y, EnemyType.Mage, 1, level)
        {
            this.canvas = canvas;
            this.player = player;
            BaseHealth = 1;
            BaseSpeed = 1.8;
            BaseAttackRate = 1.0;

            Health = CalculateScaledHealth(BaseHealth, level);
            Speed = CalculateScaledSpeed(BaseSpeed, level);
            attackRate = CalculateScaledAttackRate(BaseAttackRate, level);

            shootTimer = new DispatcherTimer();
            shootTimer.Interval = TimeSpan.FromSeconds(attackRate);
            shootTimer.Tick += ShootTimer_Tick;
            shootTimer.Start();
        }

        private void ShootTimer_Tick(object sender, EventArgs e)
        {
            Shoot();
        }

        public void Shoot()
        {
            double startX = Canvas.GetLeft(this.Visual) + this.Visual.Width / 2;
            double startY = Canvas.GetBottom(this.Visual);
            Pocisk pocisk = new Pocisk("mag", 5 + (currentLevel * 0.5), startX, startY, canvas, new List<Enemy>(), player, -1);
        }

        public void StopShooting()
        {
            shootTimer?.Stop();
        }
    }

    public class SpiderEnemy : Enemy
    {
        private Canvas canvas;
        private DispatcherTimer shootTimer;
        private Rectangle player;
        private double attackRate;

        public SpiderEnemy(double x, double y, Canvas canvas, Rectangle player, int level)
            : base(x, y, EnemyType.Spider, 1, level)
        {
            this.canvas = canvas;
            this.player = player;
            BaseHealth = 1;
            BaseSpeed = 2.2;
            BaseAttackRate = 2.0;

            Health = CalculateScaledHealth(BaseHealth, level);
            Speed = CalculateScaledSpeed(BaseSpeed, level);
            attackRate = CalculateScaledAttackRate(BaseAttackRate, level);

            shootTimer = new DispatcherTimer();
            shootTimer.Interval = TimeSpan.FromSeconds(attackRate);
            shootTimer.Tick += ShootTimer_Tick;
            shootTimer.Start();
        }

        private void ShootTimer_Tick(object sender, EventArgs e)
        {
            Shoot();
        }

        public void Shoot()
        {
            double startX = Canvas.GetLeft(this.Visual) + this.Visual.Width / 2;
            double startY = Canvas.GetBottom(this.Visual);
            Pocisk pocisk = new Pocisk("spider", 5 + (currentLevel * 0.3), startX, startY, canvas, new List<Enemy>(), player, -1);
        }

        public void StopShooting()
        {
            shootTimer?.Stop();
        }
    }
}