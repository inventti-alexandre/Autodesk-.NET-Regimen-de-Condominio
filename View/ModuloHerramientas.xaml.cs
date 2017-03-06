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
    /// Interaction logic for ModuloHerramientas.xaml
    /// </summary>
    public partial class ModuloHerramientas : MetroWindow
    {
        public ModuloHerramientas()
        {
            InitializeComponent();
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            gridLayers.ItemsSource = M.Colindante.TodosLayers;

            txtMargen.Text = M.Colindante.ToleranceError.ToString();
            txtTamanio.Text = M.Colindante.DbTextSize.ToString();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string sNumMargen = txtMargen.Text,
                    sNumText = txtTamanio.Text; 
                    
            double  dNuevoMargen = new double(), 
                    dNuevoTamanio = new double();

            if(double.TryParse(sNumMargen, out dNuevoMargen))
            {
                if(dNuevoMargen > 0)
                {
                    if(double.TryParse(sNumText, out dNuevoTamanio))
                    {
                        if(dNuevoTamanio > 0)
                        {
                            M.Colindante.ToleranceError = dNuevoMargen;
                            M.Colindante.DbTextSize = dNuevoTamanio;
                        }
                        else
                        {
                            this.ShowMessageAsync("Error al Modificar",
                                        "El tamaño del Texto debe de ser mayor a 0");
                        }
                        
                    }
                    else
                    {
                        this.ShowMessageAsync("Error al Modificar",
                                            "El tamaño del Texto debe de tener valor númerico");
                    }
                    
                }
                else
                {
                    this.ShowMessageAsync("Error al Modificar",
                                        "El margen de distancia debe de ser mayor a 0");
                }

            }
            else
            {
                this.ShowMessageAsync("Error al Modificar",
                                    "El margen de distancia debe de tener valor númerico");
            }
        }
    }
}
