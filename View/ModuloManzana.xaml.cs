using Autodesk.AutoCAD.DatabaseServices;
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
    /// Interaction logic for ModuloManzana.xaml
    /// </summary>
    public partial class ModuloManzana : MetroWindow
    {
        public ModuloManzana()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ModuloInicial M_Manzana = new ModuloInicial();
            M_Manzana.Show();
            this.Close();
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            CmbRumboFrente.ItemsSource = M.ConstantValues.Orientaciones;
        }

        private void btnSelManzana_Click(object sender, RoutedEventArgs e)
        {
            ObjectId id = new ObjectId();
            if(C.Met_Autodesk.Entity("Favor de seleccionar la Manzana: ",out id, 
                typeof(Autodesk.AutoCAD.DatabaseServices.Polyline)))
            {
                
            }
        }
    }
}
