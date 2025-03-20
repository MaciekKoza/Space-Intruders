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
        public Image Visual { get; private set; } // Zmieniamy Rectangle na Image
        public EnemyType Type { get; private set; }
        public double Speed { get; private set; } = 2; // Prędkość przeciwnika
        public int Direction { get; set; } = 1; // 1 = prawo, -1 = lewo
        public int Health { get; set; } // Liczba żyć przeciwnika

        protected string imagePath;

        public Enemy(double x, double y, EnemyType type, int health)
        {
            Type = type;
            Health = health;

            // Ustalamy ścieżkę do obrazu
            imagePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images", $"{type.ToString()}.png");

            // Tworzymy obrazek
            Visual = new Image
            {
                Width = 50,
                Height = 50,
                Source = new BitmapImage(new Uri(imagePath)) // Ładujemy obraz z pliku
            };

            Canvas.SetLeft(Visual, x);
            Canvas.SetBottom(Visual, y);
        }

        public void Move()
        {
            // Przesuwamy przeciwnika w poziomie
            double newX = Canvas.GetLeft(Visual) + Speed * Direction;
            Canvas.SetLeft(Visual, newX);
        }

        public void Shoot(Canvas canvas)
        {
            // Logika strzelania przeciwnika
        }

        public void UpdateBullets()
        {
            // Logika aktualizacji pocisków przeciwnika
        }
    }

    public enum EnemyType
    {
        Basic,
        Mage,
        Tank,
        Spider
    }

    public class TankEnemy : Enemy
    {
        public TankEnemy(double x, double y)
            : base(x, y, EnemyType.Tank, 3) // TankEnemy ma 3 życia
        {
            // W przypadku klas dziedziczących nie musimy zmieniać obrazu, ponieważ jest on ładowany przez klasę bazową
        }
    }

    public class BasicEnemy : Enemy
    {
        public BasicEnemy(double x, double y)
            : base(x, y, EnemyType.Basic, 1) // BasicEnemy ma 1 życie
        {
            // Podobnie jak w przypadku TankEnemy, obraz jest ustalany w klasie bazowej
        }
    }

    public class MageEnemy : Enemy
    {
        private Canvas canvas; // Canvas, na którym znajduje się przeciwnik
        private DispatcherTimer shootTimer; // Timer do strzelania
        private Rectangle player; // Gracz (Klocek)

        public MageEnemy(double x, double y, Canvas canvas, Rectangle player)
            : base(x, y, EnemyType.Mage, 1) // MageEnemy ma 1 życie
        {
            this.canvas = canvas;
            this.player = player; // Przekazujemy gracza

            // Inicjalizacja timera do strzelania
            shootTimer = new DispatcherTimer();
            shootTimer.Interval = TimeSpan.FromSeconds(1); // Strzelaj co sekundę
            shootTimer.Tick += ShootTimer_Tick;
            shootTimer.Start();
        }

        private void ShootTimer_Tick(object sender, EventArgs e)
        {
            Shoot();
        }

        public void Shoot()
        {
            // Tworzymy pocisk, który leci w dół
            double startX = Canvas.GetLeft(this.Visual) + this.Visual.Width / 2; // Pocisk startuje na środku przeciwnika
            double startY = Canvas.GetBottom(this.Visual);

            // Tworzymy pocisk i przekazujemy listę przeciwników oraz gracza (Klocek)
            Pocisk pocisk = new Pocisk("mag", 5, startX, startY, canvas, new List<Enemy>(), player, -1); // -1 oznacza kierunek w dół
        }

        public void StopShooting()
        {
            shootTimer.Stop(); // Zatrzymujemy timer strzelania
        }
    }

    public class SpiderEnemy : Enemy
    {
        private Canvas canvas; // Canvas, na którym znajduje się przeciwnik
        private DispatcherTimer shootTimer; // Timer do strzelania
        private Rectangle player; // Gracz (Klocek)

        public SpiderEnemy(double x, double y, Canvas canvas, Rectangle player)
            : base(x, y, EnemyType.Spider, 1) // SpiderEnemy ma 1 życie
        {
            this.canvas = canvas;
            this.player = player; // Przekazujemy gracza

            // Inicjalizacja timera do strzelania
            shootTimer = new DispatcherTimer();
            shootTimer.Interval = TimeSpan.FromSeconds(2); // Strzelaj co sekundę
            shootTimer.Tick += ShootTimer_Tick;
            shootTimer.Start();
        }

        private void ShootTimer_Tick(object sender, EventArgs e)
        {
            Shoot();
        }

        public void Shoot()
        {
            // Tworzymy pocisk, który leci w dół
            double startX = Canvas.GetLeft(this.Visual) + this.Visual.Width / 2; // Pocisk startuje na środku przeciwnika
            double startY = Canvas.GetBottom(this.Visual);

            // Tworzymy pocisk i przekazujemy listę przeciwników oraz gracza (Klocek)
            Pocisk pocisk = new Pocisk("spider", 5, startX, startY, canvas, new List<Enemy>(), player, -1); // -1 oznacza kierunek w dół
        }

        public void StopShooting()
        {
            shootTimer.Stop(); // Zatrzymujemy timer strzelania
        }
    }
}
