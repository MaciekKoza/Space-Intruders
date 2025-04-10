using Space_Intruder.Class;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Windows;
using System.Windows.Shapes;
using System.Windows.Media;
using Space_Intruder;
using System;
using System.Linq;
using Space_Intruder.GameObjects;
using System.Windows.Media.Imaging;

public class Pocisk
{
    public Image Visual { get; private set; }
    private double speed;
    private Canvas canvas;
    private List<Enemy> enemies;
    private DispatcherTimer timer;
    private int direction;
    private Hero hero;
    private string postac;
    private int damage;
    private Random random;
    private Level_Gry level;
    public bool IsFrozen { get; set; }

    public event EventHandler BulletDestroyed;

    public Pocisk(string postac, int damage, double speed, double startX, double startY,
                 Canvas canvas, List<Enemy> enemies, Hero hero, Level_Gry level, int direction = 1,
                 double width = 20, double height = 30)
    {
        this.speed = speed;
        this.canvas = canvas;
        this.enemies = enemies;
        this.direction = direction;
        this.hero = hero;
        this.postac = postac;
        this.damage = damage;
        this.level = level;
        this.random = new Random();

        Visual = new Image
        {
            Width = width,
            Height = height,
            Stretch = Stretch.Fill
        };

        // Ładowanie odpowiedniej grafiki w zależności od typu pocisku
        LoadBulletImage();

        // Obrót pocisku jeśli leci w dół (od wrogów)
        if (direction == -1)
        {
            Visual.RenderTransform = new RotateTransform(180);
            Visual.RenderTransformOrigin = new Point(0.5, 0.5);
        }

        Canvas.SetLeft(Visual, startX - width / 2);
        Canvas.SetBottom(Visual, startY);
        canvas.Children.Add(Visual);

        timer = new DispatcherTimer();
        timer.Interval = TimeSpan.FromMilliseconds(16);
        timer.Tick += Timer_Tick;
        timer.Start();
    }

    private void LoadBulletImage()
    {
        string imagePath = postac switch
        {
            "bohater" => "pack://application:,,,/Images/pocisk_bohatera.png",
            "mag" => "pack://application:,,,/Images/pocisk_wroga.png",
            "spider" => "pack://application:,,,/Images/pocisk_pajaka.png",
            "boss" => "pack://application:,,,/Images/pocisk_bossa.png",
            _ => "pack://application:,,,/Images/default_bullet.png"
        };

        try
        {
            BitmapImage bitmap = new BitmapImage(new Uri(imagePath, UriKind.Absolute));
            Visual.Source = bitmap;
        }
        catch
        {
            // Fallback na prosty kształt jeśli obrazek się nie załaduje
            Visual.Source = CreateFallbackBullet();
        }
    }

    private BitmapSource CreateFallbackBullet()
    {
        DrawingVisual drawingVisual = new DrawingVisual();
        using (DrawingContext drawingContext = drawingVisual.RenderOpen())
        {
            Brush brush = postac switch
            {
                "bohater" => Brushes.Cyan,
                "mag" => Brushes.DarkViolet,
                "spider" => Brushes.White,
                "boss" => Brushes.Red,
                _ => Brushes.White
            };

            drawingContext.DrawRectangle(brush, null, new Rect(0, 0, Visual.Width, Visual.Height));

            // Dodajemy efekt świetlny dla fallbackowych pocisków
            if (postac == "boss")
            {
                drawingContext.DrawRectangle(Brushes.Orange, null,
                    new Rect(Visual.Width / 4, Visual.Height / 4, Visual.Width / 2, Visual.Height / 2));
            }
        }

        RenderTargetBitmap rtb = new RenderTargetBitmap(
            (int)Visual.Width, (int)Visual.Height, 96, 96, PixelFormats.Pbgra32);
        rtb.Render(drawingVisual);
        return rtb;
    }

    private void Timer_Tick(object sender, EventArgs e)
    {
        if (IsFrozen) return;

        double currentY = Canvas.GetBottom(Visual);
        double newY = currentY + speed * direction;
        Canvas.SetBottom(Visual, newY);

        if (direction == 1) // Pocisk gracza
        {
            CheckCollisionWithEnemies();
        }
        else // Pocisk przeciwnika
        {
            CheckCollisionWithPlayer();
        }

        if (newY > canvas.ActualHeight || newY < 0)
        {
            Destroy();
        }
    }

    private void CheckCollisionWithEnemies()
    {
        Rect pociskRect = new Rect(Canvas.GetLeft(Visual), Canvas.GetBottom(Visual), Visual.Width, Visual.Height);

        foreach (var enemy in enemies.ToList())
        {
            if (enemy == null) continue;

            Rect enemyRect = new Rect(Canvas.GetLeft(enemy.Visual), Canvas.GetBottom(enemy.Visual),
                            enemy.Visual.Width, enemy.Visual.Height);

            if (pociskRect.IntersectsWith(enemyRect))
            {
                enemy.TakeDamage(damage);

                if (enemy.Health <= 0)
                {
                    HandleEnemyDeath(enemy);
                }

                Destroy();
                return;
            }
        }
    }

    private void HandleEnemyDeath(Enemy enemy)
    {
        if (enemy is MageEnemy mageEnemy)
            mageEnemy.StopShooting();
        if (enemy is SpiderEnemy spiderEnemy)
            spiderEnemy.StopShooting();

        canvas.Children.Remove(enemy.Visual);
        enemies.Remove(enemy);

        if (random.Next(0, 10) < 3)
        {
            BoostType type = (BoostType)random.Next(0, 3);
            Boost boost = CreateBoost(type, Canvas.GetLeft(enemy.Visual), Canvas.GetBottom(enemy.Visual));
        }
    }

    private Boost CreateBoost(BoostType type, double x, double y)
    {
        switch (type)
        {
            case BoostType.Shield:
                return new ShieldBoost(canvas, x, y, hero, level);
            case BoostType.Freeze:
                return new FreezeBoost(canvas, x, y, hero, level);
            case BoostType.TripleShot:
                return new TripleShotBoost(canvas, x, y, hero, level);
            default:
                throw new ArgumentOutOfRangeException(nameof(type), $"Unknown boost type: {type}");
        }
    }

    private void CheckCollisionWithPlayer()
    {
        Rect pociskRect = new Rect(
            Canvas.GetLeft(Visual),
            Canvas.GetBottom(Visual),
            Visual.Width,
            Visual.Height);

        Rect playerRect = new Rect(
            Canvas.GetLeft(hero.Visual),
            Canvas.GetBottom(hero.Visual),
            hero.Visual.Width,
            hero.Visual.Height);

        if (pociskRect.IntersectsWith(playerRect))
        {
            if (postac == "spider")
            {
                (Application.Current.MainWindow as MainWindow)?.SlowDownPlayer();
            }

            if (postac == "mag" || postac == "boss")
            {
                level.PlayerHit();
            }

            Destroy();
        }
    }

    public void Destroy()
    {
        timer?.Stop();
        if (canvas != null && Visual != null && canvas.Children.Contains(Visual))
        {
            canvas.Children.Remove(Visual);
        }
        BulletDestroyed?.Invoke(this, EventArgs.Empty);
    }
}