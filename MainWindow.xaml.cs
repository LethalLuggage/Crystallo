using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Crystallo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static int GRIDSIZE = 150;
        public int cardHeight { get; set; }
        public int cardWidth { get; set; }
        private Canvas nextCard;
        private int cardAngle;

        private Random rando;
        public MainWindow()
        {
            rando = new Random();
            cardHeight = GRIDSIZE * 3;
            cardWidth = GRIDSIZE * 2;
            cardAngle = 0;

            DataContext = this;
            InitializeComponent();

            CanvasBag.ShowGridLines = true;

            BuildGrid();
            DrawCard();
            CanvasBag.Children.Add(nextCard);
        }

        /// <summary>
        /// Add row and column definitions to <see cref="CanvasBag"/> according to Gridsize until the grid is filled.
        /// </summary>
        private void BuildGrid()
        {
            int numCols = (int)CanvasBag.Width / GRIDSIZE;
            int numRows = (int)CanvasBag.Height / GRIDSIZE;
            for(int r=0; r<numRows; r++)
            {
                CanvasBag.RowDefinitions.Add(new RowDefinition { Height = new GridLength(GRIDSIZE) });
            }
            for(int c = 0; c < numCols; c++)
            {
                CanvasBag.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(GRIDSIZE) });
            }
        }

        /// <summary>
        /// Gets a new card and assigns it to <see cref="nextCard"/>.
        /// </summary>
        private void DrawCard()
        {
            nextCard = new Canvas
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Height = GRIDSIZE * 3,
                Width = GRIDSIZE * 2,
                Background = new SolidColorBrush(Colors.Black)
            };
            
            Rectangle bl = new Rectangle
            {
                Fill = new SolidColorBrush(Color.FromRgb((byte)rando.Next(1,255), (byte)rando.Next(1,255), (byte)rando.Next(1,255))),
                Height = 30,
                Width = 30
            };


            Rectangle br = new Rectangle
            {
                Fill = new SolidColorBrush(Color.FromRgb((byte)rando.Next(1, 255), (byte)rando.Next(1, 255), (byte)rando.Next(1, 255))),
                Height = 30,
                Width = 30
            };

            Rectangle tl = new Rectangle
            {
                Fill = new SolidColorBrush(Color.FromRgb((byte)rando.Next(1, 255), (byte)rando.Next(1, 255), (byte)rando.Next(1, 255))),
                Height = 30,
                Width = 30
            };

            Rectangle tr = new Rectangle
            {
                Fill = new SolidColorBrush(Color.FromRgb((byte)rando.Next(1, 255), (byte)rando.Next(1, 255), (byte)rando.Next(1, 255))),
                Height = 30,
                Width = 30
            };
            Canvas.SetBottom(bl, (GRIDSIZE - bl.Height) / 2);
            Canvas.SetBottom(br, (GRIDSIZE - br.Height) / 2);
            Canvas.SetTop(tl, (GRIDSIZE - tl.Height) / 2);
            Canvas.SetTop(tr, (GRIDSIZE - tr.Height) / 2);

            Canvas.SetLeft(bl, (GRIDSIZE - bl.Height) / 2);
            Canvas.SetRight(br, (GRIDSIZE - br.Height) / 2);
            Canvas.SetLeft(tl, (GRIDSIZE - tl.Height) / 2);
            Canvas.SetRight(tr, (GRIDSIZE - tr.Height) / 2);

            nextCard.Children.Add(bl);
            nextCard.Children.Add(br);
            nextCard.Children.Add(tl);
            nextCard.Children.Add(tr);

            nextCard.Opacity = 50;
        }

        /// <summary>
        /// Function called when there is a left-click event on <see cref="CanvasBag"/>.
        /// Place the card on the grid where clicked, then draw a new card for the preview.
        /// </summary>
        /// <param name="sender">Object that sent this action.</param>
        /// <param name="e">Information about the action.</param>
        private void PlaceCard(object sender, MouseButtonEventArgs e)
        {
            (int R, int C) = GetCardCornerRC(e.GetPosition(CanvasBag).X, e.GetPosition(CanvasBag).Y);

            if (R == -1 || C == -1)
            {
                return;
            }

            CanvasBag.Children.Remove(nextCard);
            Grid.SetRow(nextCard, R);
            Grid.SetColumn(nextCard, C);
            CanvasBag.Children.Add(nextCard);

            DrawCard();
            CanvasBag.Children.Add(nextCard);
        }

        /// <summary>
        /// Function called by <see cref="CanvasBag"/> when the mouse moves over it.
        /// Move the preview to follow the mouse while it's within <see cref="CanvasBag"/>.
        /// Preview will snap to the gridlines.
        /// </summary>
        /// <param name="sender">Object that sent this action.</param>
        /// <param name="e">Information about the action.</param>
        private void MovePreview(object sender, MouseEventArgs e)
        {
            (int R, int C) = GetCardCornerRC(e.GetPosition(CanvasBag).X, e.GetPosition(CanvasBag).Y);

            if (R == -1 || C == -1)
            {
                nextCard.Visibility = Visibility.Hidden;
                return;
            }

            Grid.SetRow(nextCard, R);
            Grid.SetColumn(nextCard, C);
            nextCard.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Function called when the mouse leaves <see cref="CanvasBag"/> to hide the preview from appearing.
        /// </summary>
        /// <param name="sender">Object that sent this action.</param>
        /// <param name="e">Information about the action.</param>
        private void HidePreview(object sender, MouseEventArgs e)
        {
            nextCard.Visibility = Visibility.Hidden;
        }

        /// <summary>
        /// Function called by a mouse wheel action while hovering over <see cref="CanvasBag"/>
        /// </summary>
        /// <param name="sender">Object that sent this action.</param>
        /// <param name="e">Information about the action.</param>
        private void RotateByMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                RotateRight();
            }
            else if (e.Delta < 0)
            {
                RotateLeft();
            }
        }

        /// <summary>
        /// Use <see cref="RotateAndShift(int)"/> to rotate the card left.
        /// </summary>
        private void RotateLeft() => RotateAndShift(-90);

        /// <summary>
        /// Use <see cref="RotateAndShift(int)"/> to rotate the card right.
        /// </summary>
        private void RotateRight() => RotateAndShift(90);

        /// <summary>
        /// Rotate the card preview by a specified angle, and translate it so that the
        /// top left corner is in the same location as before.
        /// </summary>
        /// <param name="angle">How much to rotate the card by. 0, 90, 180, 270.</param>
        private void RotateAndShift(int angle)
        {
            if (angle % 90 != 0 || angle < 0 || angle > 360)
                throw new ArgumentException(angle + " is outside the range of RotateAndShift's functionality.");

            cardAngle = (cardAngle + 360 + angle) % 360;
            RotateTransform rotate = new RotateTransform(cardAngle);

            int transX, transY;
            switch (cardAngle)
            {
                case 90:
                    (transX, transY) = (cardHeight, 0);
                    break;
                case 180:
                    (transX, transY) = (cardWidth, cardHeight);
                    break;
                case 270:
                    (transX, transY) = (0, cardWidth);
                    break;
                default:
                    (transX, transY) = (0, 0);
                    break;
            }
            TranslateTransform translate = new TranslateTransform(transX, transY);
            
            TransformGroup transform = new TransformGroup();
            transform.Children.Add(rotate);
            transform.Children.Add(translate);

            nextCard.RenderTransform = transform;
        }

        /// <summary>
        /// Returns the Row and Column of the top left corner of the card.
        /// Card is dimensions 3*GRIDSIZE by 2*GRIDSIZE.
        /// </summary>
        /// <param name="inpX">X-coord of the centerpoint of the card</param>
        /// <param name="inpY">Y-coord of the centerpoint of the card</param>
        /// <returns></returns>
        private (int, int) GetCardCornerRC(double inpX, double inpY)
        {
            int C, R;
            if (IsVert()) // if vertical orientation
            {
                C = (int)(inpX - (cardWidth / 2)) / GRIDSIZE;
                R = (int)(inpY - (cardHeight / 3)) / GRIDSIZE;
            }
            else // if horizontal orientation
            {
                C = (int)(inpX - (cardHeight / 3)) / GRIDSIZE;
                R = (int)(inpY - (cardWidth / 2)) / GRIDSIZE;
            }
            if (R < 0 || (R > CanvasBag.RowDefinitions.Count - 3 && IsVert()) || (R > CanvasBag.RowDefinitions.Count - 2 && !IsVert())
                || C < 0 || (C > CanvasBag.ColumnDefinitions.Count - 2 && IsVert()) || (C > CanvasBag.ColumnDefinitions.Count - 3 && !IsVert()))
            {
                return (-1, -1);
            }
            else
            {
                return (R, C);
            }
        }

        /// <summary>
        /// Function to determine the orientation of the card by its angle.
        /// </summary>
        /// <returns>true if orientation is vertical, false if orientation is horizontal</returns>
        private bool IsVert()
        {
            return cardAngle % 180 == 0;
        }
    }
}
