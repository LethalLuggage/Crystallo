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

namespace CardGame
{
    /// <summary>
    /// Interaction logic for Card.xaml
    /// </summary>
    public partial class Card : UserControl
    {
        private Random random = new Random();
        private Color[] cornerColors = { Colors.DarkBlue, Colors.DarkGreen, Colors.DarkMagenta, Colors.DarkRed, Colors.DarkSalmon, Colors.LightBlue, Colors.LightGreen, Colors.LightYellow, Colors.Coral, Colors.LightSalmon };

        public SolidColorBrush CornerColor
        {
            get
            { 
              return new SolidColorBrush(
                  cornerColors[random.Next(cornerColors.Count())]
                  );
            }
        }
        public Card()
        {
            DataContext = this;
            InitializeComponent();
        }
    }
}
