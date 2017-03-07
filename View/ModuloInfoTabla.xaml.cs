using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using SoftwareArchitects.Windows.Controls;

namespace RegimenCondominio.V
{
    /// <summary>
    /// Lógica de interacción para ModuloInfoTabla.xaml
    /// </summary>
    public partial class ModuloInfoTabla : MetroWindow
    {
        private ObservableCollection<M.Medidas> _connectionitems = new ObservableCollection<M.Medidas>();

        public ObservableCollection<M.Medidas> ConnectionItems
        {
            get { return _connectionitems; }
            set { _connectionitems = value; }
        }


        public ModuloInfoTabla()
        {
            InitializeComponent();

            for (int i = 0; i < 50; i++)
            {
                ConnectionItems.Add(new M.Medidas
                {
                    Apartamento = "A",
                    Lote = "5",
                    Calle = "BOREAL",
                    AreaTotalCubierta = "1900.77",
                    CEstacionamiento = "111.11",
                    CLavanderia = "111.11",
                    CPasillo = "111.11",
                    CPatio = "111.11",
                    CPlantaAlta = "111.11",
                    CPlantaBaja = "111.11",
                    DEstacionamiento = "111.11",
                    DLavanderia = "111.11",
                    DPasillo = "111.11",
                    DPatio = "111.11",
                    DPlantaAlta = "111.11",
                    DPlantaBaja = "111.11",
                    Manzana = "500",
                    NoOficial = "125-A"
                });
            }
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            dtDetalle.ItemsSource = ConnectionItems;
            dtHeader.ItemsSource = ConnectionItems;
        }

        private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!M.Constant.IsAutoClose)
            {
                MessageBoxResult dg = MessageBox.Show("¿Desea Cerrar la ventana? \n Se perderá el avance completado", "Cerrando",
                                            MessageBoxButton.YesNo, MessageBoxImage.Exclamation);

                if (dg == MessageBoxResult.No)
                    e.Cancel = true;
                else
                    C.Met_General.ClearData();
            }
            else
                M.Constant.IsAutoClose = false;
        }       

    }
}
