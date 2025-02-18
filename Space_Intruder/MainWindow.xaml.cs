using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Space_Intruder
{
    public partial class MainWindow : Window
    {
        private double pozycjaX = 200;

        public MainWindow()
        {
            InitializeComponent();
            Canvas.SetLeft(Klocek, pozycjaX);
            Canvas.SetTop(Klocek, 150);
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            const double krok = 10;

            if (e.Key == Key.Left && pozycjaX > 0)
            {
                pozycjaX -= krok;
            }
            else if (e.Key == Key.Right && pozycjaX < Width - Klocek.Width - 16)
            {
                pozycjaX += krok;
            }

            Canvas.SetLeft(Klocek, pozycjaX);
        }
    }
}