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
    public partial class CrystalloWindow : Window
    {
        public static int GRIDSIZE = 50;
        private static string[] _orbs = { "phoenix", "firefox", "icewolf", "seahorse", "faerie", "unicorn" };
        private static string[] _crystals = { "red1", "red2", "red3", "orange1", "orange2", "orange3", "purple1", "purple2", "purple3" };
        private bool IsVert { get => cardAngle % 180 == 0; }
        public int CardHeight { get => GRIDSIZE * 3; }
        public int CardWidth { get => GRIDSIZE * 2; }
        public int NumCardsLeft { get => deck.Count(); }

        private List<Card> deck;
        private List<Card> discard;
        private List<Card> dragonDeck;
        private Card _nextCard;
        private Card _previewCard;
        private Card PreviewCard { get => _previewCard ?? new Card(_nextCard); }
        private int cardAngle;
        private readonly Random random;
        private GameboardCell[][] GameboardModel;

        private struct GameboardCell
        {
            string _rawcontent;
            public int _cardid;
            public string _type => _orbs.Contains(_rawcontent) ? "orb" : _crystals.Contains(_rawcontent) ? "crystal" : "blank";
            public string _color => _type == "crystal" ? _rawcontent.Substring(0, _rawcontent.Length - 1) : _rawcontent;
            public int _crystalnum => _type == "crystal" ? _rawcontent.Last()-'0' : 0;

            public GameboardCell(string content, int id)
            {
                _rawcontent = content;
                _cardid = id;
            }
        }

        public CrystalloWindow()
        {
            random = new Random();
            cardAngle = 0;

            DataContext = this;
            InitializeComponent();

            Gameboard.ShowGridLines = true;

            BuildDecks();
            BuildGrid();
            SelectNewCard();
            Gameboard.Children.Add(_nextCard);
        }

        /// <summary>
        /// Read in the deck of cards from a text file and create a <see cref="Card"/> for each.
        /// Add all cards to <see cref="deck"/>, then shuffle, and remove 9 and place them
        /// in <see cref="dragonDeck"/>.
        /// </summary>
        private void BuildDecks()
        {
            deck = new List<Card>();
            int id = 0;

            using (StreamReader r = File.OpenText("/Shal/source/repos/Crystallo/assets/cards.txt"))
            {
                while (!r.EndOfStream)
                {
                    string[] cardString = r.ReadLine().Split(',');
                    deck.Add(new Card(id, cardString[0], cardString[1], cardString[2], cardString[3], cardString[4]));
                    id++;
                }
            }

            Shuffle(deck);

            discard = new List<Card>();
            dragonDeck = deck.GetRange(0, 9);
            deck.RemoveRange(0, 9);
        }

        /// <summary>
        /// Shuffles the given deck in place using Durstenfeld Shuffle.
        /// </summary>
        /// <param name="deck">The deck to shuffle.</param>
        private void Shuffle(List<Card> deck)
        {
            for (int i = 0; i < deck.Count() - 1; i++)
            {
                int j = random.Next(i, deck.Count());
                (deck[i], deck[j]) = (deck[j], deck[i]);
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
            // define rows and columns within the grid panel
            for(int r=0; r<numRows; r++)
            {
                Gameboard.RowDefinitions.Add(new RowDefinition { Height = new GridLength(GRIDSIZE) });
            }
            for(int c = 0; c < numCols; c++)
            {
                Gameboard.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(GRIDSIZE) });
            }
            // define board model
            GameboardModel = new GameboardCell[numRows][];
            for(int r=0; r< numRows; r++)
            {
                GameboardModel[r] = new GameboardCell[numCols];
            }
        }

        /// <summary>
        /// Gets a new card and assigns it to <see cref="_nextCard"/>.
        /// </summary>
        private void SelectNewCard()
        {
            if (deck.Count() == 0)
            {
                MessageBox.Show("All out of cards!");
                return;
            }
            _nextCard = deck[random.Next(deck.Count())];
            cardAngle = 0;
            deck.Remove(_nextCard);
            discard.Add(_nextCard);
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

            Gameboard.Children.Remove(_nextCard);
            InsertIntoModel(_nextCard);
            CheckForMatch(_nextCard);
            UpdateCardPosition(_nextCard, R, C);
            Gameboard.Children.Add(_nextCard);

            (int oldR, int oldC) = (Grid.GetRow(_nextCard), Grid.GetColumn(_nextCard));
            SelectNewCard();
            UpdateCardPosition(_nextCard, oldR, oldC);

            PreviewPane.Children.Remove(PreviewCard);
            _previewCard = null;
            PreviewPane.Children.Add(PreviewCard);
            Gameboard.Children.Add(_nextCard);
        }

        private void CheckForMatch(Card crd)
        {
            int r = Grid.GetRow(crd);
            int c = Grid.GetColumn(crd);
            int r_limit = GameboardModel.Count();
            int c_limit = GameboardModel[0].Count();

            (int[] rows, int[] columns) = IsVert
                ? (new int[] { r - 1, r + 2 }, new int[] { c - 1, c, c + 1 })
                : (new int[] { r - 1, r, r + 1 }, new int[] { c - 1, c + 2 });

            bool matchFound;
            string orbFound;
            foreach(int row in rows)
            {
                if (row < 0 || row > r_limit - 1) continue;
                foreach (int col in columns)
                {
                    if (col < 0 || col > c_limit - 1) continue;
                    (matchFound, orbFound) = CheckMatch(row, col);
                    if (matchFound)
                    {
                        Console.WriteLine("Found a match at row " + row + " column " + col + ": " + orbFound);
                    }
                }
                
            }
        }

        private (bool, string) CheckMatch(int r, int c)
        {
            GameboardCell[] cells = new GameboardCell[]{ GameboardModel[r][c], GameboardModel[r + 1][c], GameboardModel[r+1][c+1], GameboardModel[r][c+1] };
            int num_orbs = 0;
            int num_crys = 0;
            string orb_name = "";
            HashSet<string> colors = new HashSet<string>();
            HashSet<int> numbers = new HashSet<int>();
            foreach(GameboardCell cell in cells)
            {
                if (cell._type == "orb")
                {
                    num_orbs++;
                    orb_name = cell._color;
                }
                if (cell._type == "crystal")
                {
                    num_crys++;
                    colors.Add(cell._color);
                    numbers.Add(cell._crystalnum);
                }
            }

            if(num_orbs == 1 && num_crys == 3
                && (colors.Count() == 1 || colors.Count() == 3)
                && (numbers.Count() == 1 || numbers.Count() == 3))
            {
                return (true, orb_name);
            }
            else
            {
                return (false, "blank");
            }
        }

        private void InsertIntoModel(Card crd)
        {
            int r = Grid.GetRow(crd);
            int c = Grid.GetColumn(crd);

            string[] contents = crd.GetContents();
            GameboardCell[] cells = new GameboardCell[4];
            for(int i=0; i<4; i++)
            {
                cells[i] = new GameboardCell(contents[i], crd.CardID);
            }

            int arrayOffset = 0;
            switch (cardAngle)
            {
                case 90:
                    arrayOffset = 3;
                    break;
                case 180:
                    arrayOffset = 2;
                    break;
                case 270:
                    arrayOffset = 1;
                    break;
            }


            (int rSpan, int cSpan) = IsVert ? (3, 2) : (2, 3);
            
            for(int ri = 0; ri < rSpan; ri++)
            {
                for(int ci = 0; ci < cSpan; ci++)
                {
                    if((ri == 0 || ri == rSpan - 1)
                        && (ci == 0 || ci == cSpan - 1))
                    {
                        int index;
                        if (ri == 0 && ci == cSpan - 1)
                            index = 1;  // top right
                        else if (ri == rSpan - 1 && ci == cSpan - 1)
                            index = 2; // bottom right
                        else if (ri == rSpan - 1 && ci == 0)
                            index = 3; // bottom left
                        else
                            index = 0; // top left

                        GameboardModel[r + ri][c + ci] = new GameboardCell(contents[(index+arrayOffset) % 4], crd.CardID);
                    }
                    else
                    {
                        GameboardModel[r + ri][c + ci] = new GameboardCell("blank", crd.CardID);
                    }
                    
                }
            }
            /*
            GameboardModel[r][c] = cells[(0 + arrayOffset) % 4];
            GameboardModel[r][c + cSpan] = cells[(1 + arrayOffset) % 4];
            GameboardModel[r + rSpan][c + cSpan] = cells[(2 + arrayOffset) % 4];
            GameboardModel[r + rSpan][c] = cells[(3 + arrayOffset) % 4];
            if (IsVert)
            {
                GameboardModel[r + 1][c] = new GameboardCell("blank", crd.CardID);
                GameboardModel[r + 1][c + 1] = new GameboardCell("blank", crd.CardID);
            }
            else
            {
                GameboardModel[r][c + 1] = new GameboardCell("blank", crd.CardID);
                GameboardModel[r + 1][c + 1] = new GameboardCell("blank", crd.CardID);
            }
            */
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
                _nextCard.Visibility = Visibility.Hidden;
                return;
            }

            UpdateCardPosition(_nextCard, R, C);

            _nextCard.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Function called when the mouse leaves <see cref="Gameboard"/> to hide the preview from appearing.
        /// </summary>
        /// <param name="sender">Object that sent this action.</param>
        /// <param name="e">Information about the action.</param>
        private void HidePreview(object sender, MouseEventArgs e)
        {
            _nextCard.Visibility = Visibility.Hidden;
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
                    (transX, transY) = (CardHeight, 0);
                    break;
                case 180:
                    (transX, transY) = (CardWidth, CardHeight);
                    break;
                case 270:
                    (transX, transY) = (0, CardWidth);
                    break;
                default:
                    (transX, transY) = (0, 0);
                    break;
            }
            TranslateTransform translate = new TranslateTransform(transX, transY);
            
            TransformGroup transform = new TransformGroup();
            transform.Children.Add(rotate);
            transform.Children.Add(translate);


            _nextCard.RenderTransform = transform;
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
            if (IsVert) // if vertical orientation
            {
                C = (int)(inpX - (CardWidth / 2)) / GRIDSIZE;
                R = (int)(inpY - (CardHeight / 2)) / GRIDSIZE;
            }
            else // if horizontal orientation
            {
                C = (int)(inpX - (CardHeight / 2)) / GRIDSIZE;
                R = (int)(inpY - (CardWidth / 2)) / GRIDSIZE;
            }
            if (R < 0 || (R > Gameboard.RowDefinitions.Count - 3 && IsVert) || (R > Gameboard.RowDefinitions.Count - 2 && !IsVert)
                || C < 0 || (C > Gameboard.ColumnDefinitions.Count - 2 && IsVert) || (C > Gameboard.ColumnDefinitions.Count - 3 && !IsVert))
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
