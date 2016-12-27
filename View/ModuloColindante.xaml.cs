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
using System.Windows.Shapes;

namespace RegimenCondominio.V
{
    /// <summary>
    /// Interaction logic for ModuloColindante.xaml
    /// </summary>
    public partial class ModuloColindante : MetroWindow
    {
        public ModuloColindante()
        {
            InitializeComponent();
        }

        private void btnAtras_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnAvanzar_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            GridColindancia.Items.Add(1);
        }
    }
}
