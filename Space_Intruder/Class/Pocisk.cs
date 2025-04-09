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
    private Hero hero; // 🔥 zmiana z Image na Hero
    private string postac;
    private Brush color = Brushes.White;
    private bool isSpiderShot;
    private int damage;
    private Random random;
    private Level_Gry level;

    public Pocisk(string postac, int damage, double speed, double startX, double startY,
             Canvas canvas, List<Enemy> enemies, Hero hero, Level_Gry level, int direction = 1,
             double width = 5, double height = 15)
    {
        this.speed = speed;
        this.canvas = canvas;
        this.enemies = enemies;
        this.direction = direction;
        this.hero = hero; // 🔥
        this.postac = postac;
        this.damage = damage;
        this.isSpiderShot = postac == "spider";
        this.level = level;

        this.random = new Random();

        switch (postac)
        {
            case "bohater":
                color = Brushes.Cyan;
                break;
            case "mag":
                color = Brushes.DarkViolet;
                break;
            case "spider":
                color = Brushes.White;
                break;
            case "boss":
                color = Brushes.Red;
                break;
        }

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
        double currentY = Canvas.GetBottom(visual);
        Canvas.SetBottom(visual, currentY + speed * direction);

        if (direction == 1)
        {
            CheckCollision();
        }
        else if (direction == -1)
        {
            CheckPlayerCollision();
        }

        if (currentY > canvas.ActualHeight || currentY < 0)
        {
            Destroy();
        }
    }

    private void CheckPlayerCollision()
    {
        Rect pociskRect = new Rect(Canvas.GetLeft(visual), Canvas.GetBottom(visual), visual.Width, visual.Height);
        Rect playerRect = new Rect(Canvas.GetLeft(hero.Visual), Canvas.GetBottom(hero.Visual), hero.Visual.Width, hero.Visual.Height);

        if (pociskRect.IntersectsWith(playerRect))
        {
            Destroy();

            if (isSpiderShot)
            {
                (Application.Current.MainWindow as MainWindow)?.SlowDownPlayer();
            }

            (Application.Current.MainWindow as MainWindow)?.PlayerHit();
        }
    }

    private void CheckCollision()
    {
        Rect pociskRect = new Rect(Canvas.GetLeft(visual), Canvas.GetBottom(visual), visual.Width, visual.Height);

        foreach (var enemy in enemies.ToList())
        {
            Rect enemyRect = new Rect(Canvas.GetLeft(enemy.Visual), Canvas.GetBottom(enemy.Visual),
                            enemy.Visual.Width, enemy.Visual.Height);

            if (pociskRect.IntersectsWith(enemyRect))
            {
                enemy.TakeDamage(damage);

                if (enemy.Health <= 0)
                {
                    if (enemy is MageEnemy mageEnemy)
                        mageEnemy.StopShooting();
                    if (enemy is SpiderEnemy spiderEnemy)
                        spiderEnemy.StopShooting();

                    canvas.Children.Remove(enemy.Visual);
                    enemies.Remove(enemy);

                    if (random.Next(0, 10) < 3) // 30% szansy
                    {
                        BoostType type = (BoostType)random.Next(0, 3);
                        var boost = new Boost(canvas, Canvas.GetLeft(enemy.Visual), Canvas.GetBottom(enemy.Visual), type, hero, level); // 🔥 hero zamiast player
                        canvas.Children.Add(boost.Visual);
                    }
                }

                Destroy();
                break;
            }
        }
    }

    private void Destroy()
    {
        timer?.Stop();
        if (canvas.Children.Contains(visual))
        {
            canvas.Children.Remove(visual);
        }
    }
}
