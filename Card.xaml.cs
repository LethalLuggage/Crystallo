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
    /// Interaction logic for Card.xaml
    /// </summary>
    public partial class Card : UserControl
    {
        public int CardID { get; }

        //private string[] crystals = { "red1", "red2", "red3", "orange1", "orange2", "orange3", "purple1", "purple2", "purple3" };
        //private string[] orbs = { "firefox", "phoenix", "seahorse", "icewolf", "faerie", "unicorn"};
        //private string[] bonus = { "book", "coins", "crown", "drinks", "helmet", "necklace", "shield", "sword", "wand" };

        private readonly string CrystalOneString;
        public ImageSource CrystalOneImage
        {
            get
            {
                if (CrystalOneString == "blank")
                {
                    return null;
                }
                return new BitmapImage(new Uri("pack://application:,,,/assets/crystal-" + CrystalOneString + ".png"));
            }
        }

        private readonly string CrystalTwoString;
        public ImageSource CrystalTwoImage
        {
            get
            {
                if (CrystalTwoString == "blank")
                {
                    return null;
                }
                return new BitmapImage(new Uri("pack://application:,,,/assets/crystal-" + CrystalTwoString + ".png"));
            }
        }

        private readonly string OrbOneString;
        public ImageSource OrbOneImage
        {
            get
            {
                if (OrbOneString == "blank")
                {
                    return null;
                }
                return new BitmapImage(new Uri("pack://application:,,,/assets/orb-" + OrbOneString + ".png"));
            }
        }

        private readonly string OrbTwoString;
        public ImageSource OrbTwoImage
        {
            get
            {
                if (OrbTwoString == "blank")
                {
                    return null;
                }
                return new BitmapImage(new Uri("pack://application:,,,/assets/orb-" + OrbTwoString + ".png"));
            }
        }

        private readonly string BonusString;
        public ImageSource BonusImage
        {
            get
            {
                if(BonusString == "blank")
                {
                    return null;
                }
                return new BitmapImage(new Uri("pack://application:,,,/assets/bonus-" + BonusString + ".png"));
            }
        }

        public Card(int id, string crystal1, string orb1, string crystal2, string orb2, string bonus)
        {
            CardID = id;
            CrystalOneString = crystal1;
            CrystalTwoString = crystal2;
            OrbOneString = orb1;
            OrbTwoString = orb2;
            BonusString = bonus;
            Grid.SetColumnSpan(this, 2);
            Grid.SetRowSpan(this, 3);
            DataContext = this;
            InitializeComponent();
        }

        /// <summary>
        /// Create a copy of a <see cref="Card"/>, using -1 as the <see cref="CardID"/>
        /// </summary>
        /// <param name="crd">Original card to copy.</param>
        public Card(Card crd)
        {
            CardID = -1;
            CrystalOneString = crd.CrystalOneString;
            CrystalTwoString = crd.CrystalTwoString;
            OrbOneString = crd.OrbOneString;
            OrbTwoString = crd.OrbTwoString;
            BonusString = crd.BonusString;
            DataContext = this;
            InitializeComponent();
        }

        public string[] GetContents()
        {
            string[] contents = { CrystalOneString, OrbOneString, CrystalTwoString, OrbTwoString, BonusString };
            return contents;
        }
    }
}
