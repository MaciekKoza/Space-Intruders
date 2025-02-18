using System;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Space_Intruder.Class
{
    class Pocisk
    {
        private int sila;
        private DispatcherTimer timer;
        public Rectangle Kształt { get; private set; }

        public Pocisk(int sila, double startX, double startY, Canvas canvas)
        {
            this.sila = sila;
            Kształt = new Rectangle
            {
                Width = 5,
                Height = 20,
                Fill = Brushes.Red
            };

            // Ustawianie pocisku w odpowiednich współrzędnych
            Canvas.SetLeft(Kształt, startX + 22.5);
            Canvas.SetBottom(Kształt, startY + 50);

            // Dodaj pocisk do właściwego Canvas
            canvas.Children.Add(Kształt);

            timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(0.1)
            };
            timer.Tick += (s, e) => MoveUp(canvas);  // Obsługuje przesuwanie w górę
            timer.Start();
        }


        private void MoveUp(Canvas canvas)
        {
            double currentY = Canvas.GetBottom(Kształt);
            if (currentY >= canvas.ActualHeight - 20)
            {
                timer.Stop();
                canvas.Children.Remove(Kształt);
            }
            else
            {
                Canvas.SetBottom(Kształt, currentY + 5);
            }
        }
    }
}
