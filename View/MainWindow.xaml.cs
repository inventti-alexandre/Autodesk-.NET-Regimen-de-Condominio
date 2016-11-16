using MahApps.Metro.Controls;
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

namespace RegimenCondominio
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        bool Si = false;

        private void CsTextbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Sector.Text.Length == 1 && !Si)
            {
                HidText.Visibility = Visibility.Visible;
                (this.Resources["expandlabel"] as
                    System.Windows.Media.Animation.Storyboard).Begin();                
                Si = true;
            }
            else if(Sector.Text.Length == 0)
            {
                Si = false;
                (this.Resources["hidelabel"] as
                System.Windows.Media.Animation.Storyboard).Begin(); 
                                    
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            HidText.Visibility = Visibility.Visible;
            (this.Resources["hidelabel"] as
                System.Windows.Media.Animation.Storyboard).Begin();
        }
    }
}
