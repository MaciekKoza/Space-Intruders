using Space_Intruder.Class;
using Space_Intruder.GameObjects;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Space_Intruder
{
    public partial class UpgradeScreen : Window
    {
        private readonly Hero _player;
        private int _remainingUpgrades = 3; // Maksymalnie 3 ulepszenia
        private readonly Brush _availableColor = Brushes.Gold;
        private readonly Brush _usedColor = Brushes.Gray;
        private Level_Gry level;
        public UpgradeScreen(Hero player, Level_Gry level_)
        {
            InitializeComponent();
            _player = player;
            level = level_;

            // Zatrzymaj przeciwników gdy okno jest otwarte
            level.SetEnemiesMovement(false);

            UpdateUpgradePointsDisplay();

            // Sprawdź maksymalną liczbę żyć na starcie
            if (_player.Lives >= _player.MaxLives)
            {
                HealthUpgradeBtn.IsEnabled = false;
                HealthUpgradeBtn.Content = "MAX ŻYĆ OSIĄGNIĘTY";
                HealthUpgradeBtn.Background = Brushes.LightGray;
            }

            // Animacja pojawiania się
            this.Opacity = 0;
            var fadeIn = new DoubleAnimation(1, TimeSpan.FromSeconds(0.3));
            this.BeginAnimation(Window.OpacityProperty, fadeIn);

            // Animacja powiększania
            this.RenderTransform = new ScaleTransform();
            var scaleAnim = new DoubleAnimation(0.9, 1, TimeSpan.FromSeconds(0.3))
            {
                EasingFunction = new ElasticEase { Oscillations = 1, Springiness = 5 }
            };
            this.RenderTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnim);
            this.RenderTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnim);
        }

        private void UpgradeStat(string stat, double value)
        {
            if (_remainingUpgrades > 0)
            {
                _player.UpgradeStat(stat, value);
                _remainingUpgrades--;
                UpdateUpgradePointsDisplay();

                if (_remainingUpgrades == 0)
                {
                    DisableAllUpgradeButtons();
                }
            }
        }

        private void UpdateUpgradePointsDisplay()
        {
            PointsPanel.Children.Clear();

            for (int i = 0; i < 3; i++)
            {
                PointsPanel.Children.Add(new Ellipse
                {
                    Width = 15,
                    Height = 15,
                    Fill = i < _remainingUpgrades ? _availableColor : _usedColor,
                    Margin = new Thickness(5)
                });
            }

            PointsText.Text = $"Pozostało ulepszeń: {_remainingUpgrades}/3";
        }

        private void DisableAllUpgradeButtons()
        {
            DamageUpgradeBtn.IsEnabled = false;
            AttackSpeedUpgradeBtn.IsEnabled = false;
            MovementUpgradeBtn.IsEnabled = false;
            HealthUpgradeBtn.IsEnabled = false;

            // Wizualne podkreślenie, że wybrano maksymalną liczbę ulepszeń
            ContinueBtn.Background = Brushes.LimeGreen;
            ContinueBtn.Content = "KONTYNUUJ (MAX)";
        }

        private void DamageUpgrade_Click(object sender, RoutedEventArgs e)
        {
            UpgradeStat("damage", 2);
            AnimateButton(DamageUpgradeBtn);
        }

        private void AttackSpeedUpgrade_Click(object sender, RoutedEventArgs e)
        {
            UpgradeStat("attackspeed", 0.5);
            AnimateButton(AttackSpeedUpgradeBtn);
        }

        private void MovementUpgrade_Click(object sender, RoutedEventArgs e)
        {
            UpgradeStat("movementspeed", 5);
            AnimateButton(MovementUpgradeBtn);
        }

        private void HealthUpgrade_Click(object sender, RoutedEventArgs e)
        {
            // Sprawdź czy gracz ma już maksymalną liczbę żyć
            if (_player.Lives >= _player.MaxLives)
            {
                // Wyłącz przycisk i pokaż komunikat
                HealthUpgradeBtn.IsEnabled = false;
                HealthUpgradeBtn.Content = "MAX ŻYĆ OSIĄGNIĘTY";
                HealthUpgradeBtn.Background = Brushes.LightGray;

                // Możesz dodać animację informującą o maksymalnej liczbie żyć
                var flashAnimation = new ColorAnimation(Colors.Red, Colors.LightGray,
                    TimeSpan.FromMilliseconds(300));
                var brush = new SolidColorBrush(Colors.LightGray);
                HealthUpgradeBtn.Background = brush;
                brush.BeginAnimation(SolidColorBrush.ColorProperty, flashAnimation);

                return;
            }

            // Normalna procedura ulepszania jeśli nie ma maksa żyć
            if (_remainingUpgrades > 0)
            {
                UpgradeStat("health", 1);
                AnimateButton(HealthUpgradeBtn);

                // Dodatkowe sprawdzenie po ulepszeniu
                if (_player.Lives >= _player.MaxLives)
                {
                    HealthUpgradeBtn.IsEnabled = false;
                    HealthUpgradeBtn.Content = "MAX ŻYĆ OSIĄGNIĘTY";
                    HealthUpgradeBtn.Background = Brushes.LightGray;
                }
            }
        }

        private void Continue_Click(object sender, RoutedEventArgs e)
        {
            // Wznów ruch przeciwników przed zamknięciem
            level.SetEnemiesMovement(true);

            // Animacja znikania
            var fadeOut = new DoubleAnimation(0, TimeSpan.FromSeconds(0.2));
            fadeOut.Completed += (s, _) => this.Close();
            this.BeginAnimation(Window.OpacityProperty, fadeOut);

            // Animacja zmniejszania
            var scaleAnim = new DoubleAnimation(1, 0.9, TimeSpan.FromSeconds(0.2));
            this.RenderTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnim);
            this.RenderTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnim);
        }

        private void AnimateButton(Button button)
        {
            var transform = new ScaleTransform();
            button.RenderTransform = transform;
            button.RenderTransformOrigin = new Point(0.5, 0.5);

            var animation = new DoubleAnimation(1, 1.1, TimeSpan.FromMilliseconds(100))
            {
                AutoReverse = true,
                EasingFunction = new ElasticEase { Oscillations = 1 }
            };
            transform.BeginAnimation(ScaleTransform.ScaleXProperty, animation);
            transform.BeginAnimation(ScaleTransform.ScaleYProperty, animation);

            // Efekt błysku
            var colorAnimation = new ColorAnimation(Colors.Gold, Colors.Transparent,
                TimeSpan.FromMilliseconds(300));
            var brush = new SolidColorBrush(Colors.Transparent);
            button.Background = brush;
            brush.BeginAnimation(SolidColorBrush.ColorProperty, colorAnimation);
        }


    }
}