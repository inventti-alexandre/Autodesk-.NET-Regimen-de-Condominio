using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
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
    /// Lógica de interacción para ModuloInfoTabla.xaml
    /// </summary>
    public partial class ModuloInfoTabla : MetroWindow
    {
        public ModuloInfoTabla()
        {
            InitializeComponent();
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {

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
