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

public class Pocisk
{
    private Rectangle visual;
    private double speed;
    private Canvas canvas;
    private List<Enemy> enemies;
    private DispatcherTimer timer;
    private int direction;
    private Hero hero;
    private string postac;
    private Brush color = Brushes.White;
    private bool isSpiderShot;
    private int damage;
    private Random random;
    private Level_Gry level;
    public bool IsFrozen { get; set; }

    public Pocisk(string postac, int damage, double speed, double startX, double startY,
             Canvas canvas, List<Enemy> enemies, Hero hero, Level_Gry level, int direction = 1,
             double width = 5, double height = 15)
    {
        this.speed = speed;
        this.canvas = canvas;
        this.enemies = enemies;
        this.direction = direction;
        this.hero = hero;
        this.postac = postac;
        this.damage = damage;
        this.isSpiderShot = postac == "spider";
        this.level = level;
        this.random = new Random();

        // Ustawienie koloru pocisku
        this.color = postac switch
        {
            "bohater" => Brushes.Cyan,
            "mag" => Brushes.DarkViolet,
            "spider" => Brushes.White,
            "boss" => Brushes.Red,
            _ => Brushes.White
        };

        visual = new Rectangle
        {
            Width = width,
            Height = height,
            Fill = color
        };

        Canvas.SetLeft(visual, startX - width / 2);
        Canvas.SetBottom(visual, startY);
        canvas.Children.Add(visual);

        timer = new DispatcherTimer();
        timer.Interval = TimeSpan.FromMilliseconds(16);
        timer.Tick += Timer_Tick;
        timer.Start();
    }

    private void Timer_Tick(object sender, EventArgs e)
    {
        if (IsFrozen) return;

        double currentY = Canvas.GetBottom(visual);
        double newY = currentY + speed * direction;
        Canvas.SetBottom(visual, newY);

        if (direction == 1) // Pocisk gracza (leci w górę)
        {
            CheckCollisionWithEnemies();
        }
        else // Pocisk przeciwnika (leci w dół)
        {
            CheckCollisionWithPlayer();
        }

        // Sprawdź czy pocisk wyszedł poza ekran
        if (newY > canvas.ActualHeight || newY < 0)
        {
            Destroy();
        }
    }

    private void CheckCollisionWithEnemies()
    {
        Rect pociskRect = new Rect(Canvas.GetLeft(visual), Canvas.GetBottom(visual), visual.Width, visual.Height);

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
            var boost = new Boost(canvas,
                               Canvas.GetLeft(enemy.Visual),
                               Canvas.GetBottom(enemy.Visual),
                               type,
                               hero,
                               level);
        }
    }

    private void CheckCollisionWithPlayer()
    {
        if (hero.IsShielded) return; // Jeśli bohater ma tarczę, nie otrzymuje obrażeń

        Rect pociskRect = new Rect(
            Canvas.GetLeft(visual),
            Canvas.GetBottom(visual),
            visual.Width,
            visual.Height);

        Rect playerRect = new Rect(
            Canvas.GetLeft(hero.Visual),
            Canvas.GetBottom(hero.Visual),
            hero.Visual.Width,
            hero.Visual.Height);

        if (pociskRect.IntersectsWith(playerRect))
        {

            if (isSpiderShot)
            {
                (Application.Current.MainWindow as MainWindow)?.SlowDownPlayer();
            }

            if (postac == "mag" || postac == "boss" || postac == "spider")
            {
                MessageBox.Show("Dostał");
                level.PlayerHit();
            }

            Destroy();
        }
    }

    private void Destroy()
    {
        timer?.Stop();
        if (canvas != null && visual != null && canvas.Children.Contains(visual))
        {
            canvas.Children.Remove(visual);
        }
    }
}