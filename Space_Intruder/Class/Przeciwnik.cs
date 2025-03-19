using System.Windows.Controls;
using System.Windows.Shapes;

namespace Space_Intruder.Class
{
    public class Enemy
    {
        public Rectangle Visual { get; private set; }
        public EnemyType Type { get; private set; }
        public double Speed { get; private set; } = 2; // Prędkość przeciwnika
        public int Direction { get; set; } = 1; // 1 = prawo, -1 = lewo

        public Enemy(double x, double y, EnemyType type)
        {
            Type = type;
            Visual = new Rectangle
            {
                Width = 50,
                Height = 50,
                Fill = System.Windows.Media.Brushes.Red // Przykładowy kolor
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
            : base(x, y, EnemyType.Tank)
        {
            // Ustawiamy kolor dla TankEnemy
            Visual.Fill = System.Windows.Media.Brushes.Green;
        }
    }

    public class BasicEnemy : Enemy
    {
        public BasicEnemy(double x, double y)
            : base(x, y, EnemyType.Basic)
        {
            // Ustawiamy kolor dla BasicEnemy
            Visual.Fill = System.Windows.Media.Brushes.Red;
        }
    }

    public class MageEnemy : Enemy
    {
        public MageEnemy(double x, double y)
            : base(x, y, EnemyType.Mage)
        {
            // Ustawiamy kolor dla MageEnemy
            Visual.Fill = System.Windows.Media.Brushes.Blue;
        }
    }

    public class SpiderEnemy : Enemy
    {
        public SpiderEnemy(double x, double y)
            : base(x, y, EnemyType.Spider)
        {
            // Ustawiamy kolor dla SpiderEnemy
            Visual.Fill = System.Windows.Media.Brushes.Purple;
        }
    }
}