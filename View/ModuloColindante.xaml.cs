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
            M.DatosColindancia dc = new M.DatosColindancia()
            {
                Lote = "1",
                Seccion = "LAVANDERÍA",
                PuntoA = 0,
                PuntoB = 1,
                Distancia = 2.25,
                Rumbo = "Noreste",
                Colindancia =  "Lote 1"
            };

            M.DatosColindancia dc2 = new M.DatosColindancia()
            {
                Lote = "2",
                Seccion = "PATIO DESCUBIERTO",
                PuntoA = 0,
                PuntoB = 1,
                Distancia = 2.25,
                Rumbo = "Noreste",
                Colindancia = "Lote 1"
            };

            List<M.DatosColindancia> l = new List<M.DatosColindancia>();

            l.Add(dc);
            l.Add(dc);
            l.Add(dc2);
            l.Add(dc2);

            tb1GridColindancia.ItemsSource = l;

            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(this.tb1GridColindancia.ItemsSource);
            PropertyGroupDescription groupDesc = new PropertyGroupDescription("Lote");            
            view.GroupDescriptions.Add(groupDesc);                    
        }
    }
}
