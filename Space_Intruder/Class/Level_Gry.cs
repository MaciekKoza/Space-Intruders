using Space_Intruder.GameObjects;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace Space_Intruder.Class
{
    public class Level_Gry
    {
        public int CurrentLevel { get; private set; } = 1;
        public int TotalLevels { get; } = 9;
        public bool IsGameCompleted { get; private set; }
        public bool IsPaused { get; private set; }

        private Canvas gameCanvas;
        private Hero player;
        private List<Enemy> enemies = new List<Enemy>();

        public static Level_Gry Instance { get; private set; }

        public Level_Gry(Canvas gameCanvas, Hero player)
        {
            this.gameCanvas = gameCanvas;
            this.player = player;
            Instance = this;
        }

        public void PlayerHit()
        {
            player.Damage();
        }

        public void SetPlayer(Hero player) => this.player = player;

        public void LoadLevel(int level)
        {
            ClearEnemies();
            CurrentLevel = level;

            switch (CurrentLevel)
            {
                case 1: CreateLevel1(); break;
                case 2: CreateLevel2(); break;
                case 3: CreateLevel3(); break;
                case 4: CreateLevel4(); break;
                case 5: CreateLevel5(); break;
                case 6: CreateLevel6(); break;
                case 7: CreateLevel7(); break;
                case 8: CreateLevel8(); break;
                case 9: CreateLevel9(); break;
                default: IsGameCompleted = true; break;
            }
        }

        public void NextLevel()
        {
            if (CurrentLevel < TotalLevels)
                LoadLevel(CurrentLevel + 1);
            else
                IsGameCompleted = true;
        }

        public void ResumeAllEnemies()
        {
            foreach (var enemy in enemies.ToList())
            {
                enemy.IsFrozen = false; ;
                if (enemy is MageEnemy mage) mage.StartShooting();
                if (enemy is SpiderEnemy spider) spider.StartShooting();
            }
        }

        public void StopAllEnemies()
        {
            foreach (var enemy in enemies.ToList())
            {
                enemy.IsFrozen = true;
                if (enemy is MageEnemy mage) mage.StopShooting();
                if (enemy is SpiderEnemy spider) spider.StopShooting();
            }
        }

        public bool AreAllEnemiesDefeated() => enemies.Count == 0;
        public List<Enemy> GetCurrentEnemies() => enemies;

        public void UpdateEnemies()
        {
            if (IsPaused) return;

            foreach (var enemy in enemies.ToList())
            {
                if (!enemy.ShouldMove) continue;

                enemy.Move();

                double enemyX = Canvas.GetLeft(enemy.Visual);
                if (enemyX <= 0 || enemyX + enemy.Visual.Width >= gameCanvas.ActualWidth)
                {
                    enemy.Direction *= -1;
                    double currentY = Canvas.GetBottom(enemy.Visual);
                    Canvas.SetBottom(enemy.Visual, currentY - 20);
                }

                Rect enemyRect = new Rect(
                    Canvas.GetLeft(enemy.Visual),
                    Canvas.GetBottom(enemy.Visual),
                    enemy.Visual.Width,
                    enemy.Visual.Height);

                Rect playerRect = new Rect(
                    player.PositionX,
                    player.PositionY,
                    player.Width,
                    player.Height);

                if (enemyRect.IntersectsWith(playerRect))
                {
                    enemy.TakeDamage(1);
                    player.TakeDamage();
                }
            }
        }

        private void ClearEnemies()
        {
            foreach (var enemy in enemies)
            {
                if (enemy is MageEnemy mage) mage.StopShooting();
                if (enemy is SpiderEnemy spider) spider.StopShooting();
                gameCanvas.Children.Remove(enemy.Visual);
            }
            enemies.Clear();
        }

        private void CreateEnemyPattern(int rows, int cols, double spacingX, double spacingY,
                                      double startX, double startY, Func<int, double, double, Enemy> enemyCreator)
        {
            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    double x = startX + col * spacingX;
                    double y = gameCanvas.ActualHeight - startY - row * spacingY;
                    var enemy = enemyCreator(row, x, y);
                    enemies.Add(enemy);
                    gameCanvas.Children.Add(enemy.Visual);
                }
            }
        }

        private void CreateLevel1()
        {
            double startY = 60; // 50px od góry + miejsce dla 2 rzędów
            CreateEnemyPattern(2, 5, 80, 60, 50, startY,
                (row, x, y) => new BasicEnemy(x, y, CurrentLevel, gameCanvas)
                {
                    BaseSpeed = 2.5,
                    BaseAttackRate = 1.2
                });
        }

        private void CreateLevel2()
        {
            double startY = 60; // 50px od góry + miejsce dla 2 rzędów
            CreateEnemyPattern(3, 4, 90, 70, 60, startY,
                (row, x, y) => new BasicEnemy(x, y, CurrentLevel, gameCanvas)
                {
                    BaseHealth = row + 1,
                    BaseSpeed = 2.5 + (row * 0.2),
                    BaseAttackRate = 1.0 + (row * 0.1)
                });
        }

        private void CreateLevel3()
        {
            double startY = 60; // 50px od góry + miejsce dla 2 rzędów
            CreateEnemyPattern(2, 6, 70, 60, 40, startY,
                (row, x, y) => row < 1 ?
                    new BasicEnemy(x, y, CurrentLevel, gameCanvas)
                    {
                        BaseSpeed = 2.8,
                        BaseAttackRate = 1.3
                    } :
                    new MageEnemy(x, y, gameCanvas, player, CurrentLevel, this)
                    {
                        BaseAttackRate = 1.5
                    });
        }

        private void CreateLevel4()
        {
            double startY = 60; // 50px od góry + miejsce dla 2 rzędów
            CreateEnemyPattern(3, 5, 80, 65, 50, startY,
                (row, x, y) => row == 0 ?
                    new BasicEnemy(x, y, CurrentLevel, gameCanvas)
                    {
                        BaseSpeed = 3.0
                    } :
                    row == 1 ?
                    new MageEnemy(x, y, gameCanvas, player, CurrentLevel, this)
                    {
                        BaseAttackRate = 1.8
                    } :
                    new TankEnemy(x, y, CurrentLevel, gameCanvas)
                    {
                        BaseHealth = 5,
                        BaseSpeed = 2.0
                    });
        }

        private void CreateLevel5()
        {
            double startY = 60; // 50px od góry + miejsce dla 2 rzędów
            CreateEnemyPattern(4, 6, 75, 60, 40, startY,
                (row, x, y) => row < 2 ?
                    new BasicEnemy(x, y, CurrentLevel, gameCanvas)
                    {
                        BaseHealth = row + 2,
                        BaseSpeed = 3.2
                    } :
                    row == 2 ?
                    new MageEnemy(x, y, gameCanvas, player, CurrentLevel , this)
                    {
                        BaseAttackRate = 2.0
                    } :
                    new TankEnemy(x, y, CurrentLevel, gameCanvas)
                    {
                        BaseHealth = 6,
                        BaseSpeed = 2.2
                    });
        }

        private void CreateLevel6()
        {
            double startY = 60; // 50px od góry + miejsce dla 2 rzędów
            CreateEnemyPattern(3, 7, 70, 65, 30, startY,
                (row, x, y) => row == 0 ?
                    new BasicEnemy(x, y, CurrentLevel, gameCanvas)
                    {
                        BaseSpeed = 3.5
                    } :
                    row == 1 ?
                    new MageEnemy(x, y, gameCanvas, player, CurrentLevel, this)
                    {
                        BaseAttackRate = 2.0
                    } :
                    new SpiderEnemy(x, y, gameCanvas, player, CurrentLevel, this)
                    {
                        BaseAttackRate = 0.5,
                        BaseSpeed = 3.0
                    });
        }

        private void CreateLevel7()
        {
            double startY = 60; // 50px od góry + miejsce dla 2 rzędów
            CreateEnemyPattern(4, 6, 80, 70, 40, startY,
                (row, x, y) => row < 1 ?
                    new BasicEnemy(x, y, CurrentLevel, gameCanvas)
                    {
                        BaseHealth = 3,
                        BaseSpeed = 3.7
                    } :
                    row < 3 ?
                    new MageEnemy(x, y, gameCanvas, player, CurrentLevel, this)
                    {
                        BaseAttackRate = 1.5
                    } :
                    new TankEnemy(x, y, CurrentLevel, gameCanvas)
                    {
                        BaseHealth = 8,
                        BaseSpeed = 2.5
                    });
        }

        private void CreateLevel8()
        {
            double startY = 60; // 50px od góry + miejsce dla 2 rzędów
            CreateEnemyPattern(5, 5, 85, 60, 50, startY,
                (row, x, y) => row < 2 ?
                    new BasicEnemy(x, y, CurrentLevel, gameCanvas)
                    {
                        BaseHealth = 4,
                        BaseSpeed = 4.0
                    } :
                    row < 4 ?
                    new MageEnemy(x, y, gameCanvas, player, CurrentLevel, this)
                    {
                        BaseAttackRate = 1.5
                    } :
                    new SpiderEnemy(x, y, gameCanvas, player, CurrentLevel, this)
                    {
                        BaseAttackRate = 3.5,
                        BaseSpeed = 1.5
                    });
        }

        public void SetEnemiesMovement(bool shouldMove)
        {
            foreach (var enemy in enemies.ToList())
            {
                if (shouldMove)
                {
                    enemy.ResumeMovement();
                    if (enemy is MageEnemy mage) mage.StartShooting();
                    if (enemy is SpiderEnemy spider) spider.StartShooting();
                }
                else
                {
                    enemy.StopMovement();
                    if (enemy is MageEnemy mage) mage.StopShooting();
                    if (enemy is SpiderEnemy spider) spider.StopShooting();
                }
            }
        }

        private void CreateLevel9()
        {
            double bossY = gameCanvas.ActualHeight - 200;
            double spacingX = 90;
            double spacingY = 55;
            double startX = 30;

            // Elitarni magowie – 1 rząd pod bossem
            double eliteStartY = 80; // Pod bossem, z odstępem
            CreateEnemyPattern(1, 6, spacingX, spacingY, startX, eliteStartY,
                (row, x, y) => new MageEnemy(x, y, gameCanvas, player, CurrentLevel, this)
                {
                    BaseHealth = 20,
                    BaseAttackRate = 1.0, // 2x szybciej (domyślnie 2.0 lub 3.0)
                    Visual = { Width = 80, Height = 80 } // Więksi
                });

            // Boss
            var boss = new TankEnemy(gameCanvas.ActualWidth / 2 - 60, bossY, CurrentLevel * 3, gameCanvas)
            {
                BaseHealth = 200,
                BaseSpeed = 1.2,
                Visual = { Width = 160, Height = 160 }
            };


            enemies.Add(boss);
            gameCanvas.Children.Add(boss.Visual);
        }

    }
}