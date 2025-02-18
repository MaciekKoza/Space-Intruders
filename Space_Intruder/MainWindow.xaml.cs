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
using Space_Intruder.Class;

namespace Space_Intruder
{
    public partial class MainWindow : Window
    {
        private double pozycjaX = 200;

        public MainWindow()
        {
            InitializeComponent();
            Canvas.SetLeft(Klocek, pozycjaX);
            Canvas.SetBottom(Klocek, 20); // Umieszczenie 20px od dolnej krawędzi
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            const double krok = 10;

            // Obsługuje ruch w lewo
            if (e.Key == Key.Left && pozycjaX > 0)
            {
                pozycjaX -= krok;
            }
            // Obsługuje ruch w prawo
            else if (e.Key == Key.Right && pozycjaX < Width - Klocek.Width - 16)
            {
                pozycjaX += krok;
            }

            // Strzał
            if (e.Key == Key.Space)
            {
                double klocekX = Canvas.GetLeft(Klocek);
                double klocekY = Canvas.GetBottom(Klocek);

                // Sprawdzenie, czy MyCanvas nie jest null
                if (MyCanvas == null)
                {
                    MessageBox.Show("MyCanvas jest null.");
                    return;
                }

                // Tworzymy pocisk
                Pocisk pocisk = new Pocisk(10, klocekX, klocekY, MyCanvas);
            }

            // Aktualizowanie pozycji klocek po każdej zmianie
            Canvas.SetLeft(Klocek, pozycjaX);
        }
    }

}