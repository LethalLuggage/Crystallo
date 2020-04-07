using System;
using System.Collections.Generic;
using System.IO;
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
        public static int GRIDSIZE = 50;
        public int cardHeight { get; set; }
        public int cardWidth { get; set; }
        private List<Card> Deck;
        private Card nextCard;
        private int cardAngle;
        private bool isVert
        {
            get
            {
                return cardAngle % 180 == 0;
            }
        }

        private Random rando;
        public MainWindow()
        {
            rando = new Random();
            cardHeight = GRIDSIZE * 3;
            cardWidth = GRIDSIZE * 2;
            cardAngle = 0;

            DataContext = this;
            InitializeComponent();

            Gameboard.ShowGridLines = true;

            BuildDeck();
            BuildGrid();
            SelectNewCard();
            Gameboard.Children.Add(nextCard);
        }

        /// <summary>
        /// Read in the deck of cards from a text file and create a <see cref="Card"/> for each.
        /// </summary>
        private void BuildDeck()
        {
            Deck = new List<Card>();

            using (StreamReader r = File.OpenText("/Shal/source/repos/Crystallo/assets/cards.txt"))
            {
                while (!r.EndOfStream)
                {
                    string[] cardString = r.ReadLine().Split(',');
                    Deck.Add(new Card(cardHeight, cardWidth,
                        cardString[0], cardString[1], cardString[2], cardString[3], cardString[4]));
                }
            }
        }

        /// <summary>
        /// Add row and column definitions to <see cref="Gameboard"/> 
        /// 5according to Gridsize until the grid is filled.
        /// </summary>
        private void BuildGrid()
        {
            int numCols = (int)Gameboard.Width / GRIDSIZE;
            int numRows = (int)Gameboard.Height / GRIDSIZE;
            for(int r=0; r<numRows; r++)
            {
                Gameboard.RowDefinitions.Add(new RowDefinition { Height = new GridLength(GRIDSIZE) });
            }
            for(int c = 0; c < numCols; c++)
            {
                Gameboard.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(GRIDSIZE) });
            }
        }

        /// <summary>
        /// Gets a new card and assigns it to <see cref="nextCard"/>.
        /// </summary>
        private void SelectNewCard()
        {
            if (Deck.Count() == 0)
            {
                MessageBox.Show("All out of cards!");
                return;
            }
            nextCard = Deck[rando.Next(Deck.Count())];
            Deck.Remove(nextCard);
        }

        /// <summary>
        /// Update the position of the given <see cref="Card"/> on <see cref="Gameboard"/>.
        /// </summary>
        /// <param name="crd">the card to update</param>
        /// <param name="r">new row of the card</param>
        /// <param name="c">new column of the card</param>
        private void UpdateCardPosition(Card crd, int r, int c)
        {
            Grid.SetRow(crd, r);
            Grid.SetColumn(crd, c);
        }

        /// <summary>
        /// Function called when there is a left-click event on <see cref="Gameboard"/>.
        /// Place the card on the grid where clicked, then draw a new card for the preview.
        /// </summary>
        /// <param name="sender">Object that sent this action.</param>
        /// <param name="e">Information about the action.</param>
        private void PlaceCard(object sender, MouseButtonEventArgs e)
        {
            (int R, int C) = GetCardCornerRC(e.GetPosition(Gameboard).X, e.GetPosition(Gameboard).Y);

            if (R == -1 || C == -1)
            {
                return;
            }

            Gameboard.Children.Remove(nextCard);
            UpdateCardPosition(nextCard, R, C);
            Gameboard.Children.Add(nextCard);

            (int oldR, int oldC) = (Grid.GetRow(nextCard), Grid.GetColumn(nextCard));
            SelectNewCard();
            UpdateCardPosition(nextCard, oldR, oldC);
            Gameboard.Children.Add(nextCard);
        }

        /// <summary>
        /// Function called by <see cref="Gameboard"/> when the mouse moves over it.
        /// Move the preview to follow the mouse while it's within <see cref="Gameboard"/>.
        /// Preview will snap to the gridlines.
        /// </summary>
        /// <param name="sender">Object that sent this action.</param>
        /// <param name="e">Information about the action.</param>
        private void MovePreview(object sender, MouseEventArgs e)
        {
            (int R, int C) = GetCardCornerRC(e.GetPosition(Gameboard).X, e.GetPosition(Gameboard).Y);

            if (R == -1 || C == -1)
            {
                nextCard.Visibility = Visibility.Hidden;
                return;
            }

            UpdateCardPosition(nextCard, R, C);

            nextCard.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Function called when the mouse leaves <see cref="Gameboard"/> to hide the preview from appearing.
        /// </summary>
        /// <param name="sender">Object that sent this action.</param>
        /// <param name="e">Information about the action.</param>
        private void HidePreview(object sender, MouseEventArgs e)
        {
            nextCard.Visibility = Visibility.Hidden;
        }

        /// <summary>
        /// Function called by a mouse wheel action while hovering over <see cref="Gameboard"/>
        /// </summary>
        /// <param name="sender">Object that sent this action.</param>
        /// <param name="e">Information about the action.</param>
        private void RotateByMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                RotateLeft();
            }
            else if (e.Delta < 0)
            {
                RotateRight();
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
        /// <param name="angle">How much to rotate the card by. 0, 90, 180 or 270.</param>
        private void RotateAndShift(int angle)
        {
            cardAngle = (cardAngle + 360 + angle) % 360;
            if (cardAngle % 90 != 0 || cardAngle < 0 || cardAngle > 360)
                throw new ArgumentException(cardAngle + " is outside the range of RotateAndShift's functionality.");

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
            if (isVert) // if vertical orientation
            {
                C = (int)(inpX - (cardWidth / 2)) / GRIDSIZE;
                R = (int)(inpY - (cardHeight / 3)) / GRIDSIZE;
            }
            else // if horizontal orientation
            {
                C = (int)(inpX - (cardHeight / 3)) / GRIDSIZE;
                R = (int)(inpY - (cardWidth / 2)) / GRIDSIZE;
            }
            if (R < 0 || (R > Gameboard.RowDefinitions.Count - 3 && isVert) || (R > Gameboard.RowDefinitions.Count - 2 && !isVert)
                || C < 0 || (C > Gameboard.ColumnDefinitions.Count - 2 && isVert) || (C > Gameboard.ColumnDefinitions.Count - 3 && !isVert))
            {
                return (-1, -1);
            }
            else
            {
                return (R, C);
            }
        }
    }
}
