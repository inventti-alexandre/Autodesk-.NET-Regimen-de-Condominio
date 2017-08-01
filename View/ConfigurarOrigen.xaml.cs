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
    /// Interaction logic for ConfigurarOrigen.xaml
    /// </summary>
    public partial class ConfigurarOrigen : MetroWindow
    {
        public ConfigurarOrigen()
        {
            InitializeComponent();
        }

        private void btnGuardar_Click(object sender, RoutedEventArgs e)
        {
            //Reviso que haya tecleado la contraseña
            if (!string.IsNullOrEmpty(txtPassword.Password))
            {
                //Reviso que la contraseña sea la correcta
                if (txtPassword.Password == Config.DB.PassAmbientes)
                {
                    //Reviso que se haya seleccionado un ambiente
                    if (cmbOrigenes.SelectedIndex != -1)
                    {
                        //Obtengo el identificador del ambiente seleccionado
                        string nombreCortoSel = cmbOrigenes.SelectedValue.ToString();                                

                        bool cambioAmbiente = false;

                        //Comienzo a buscar el ambiente
                        foreach (M.Ambientes ambiente in Config.DB.Ambientes)
                        {
                            //Si es igual al ambiente seleccionado
                            if (ambiente.NombreCorto == nombreCortoSel)
                            {
                                //Cambio el ambiente durante la sesión
                                Config.DB.AmbienteActual = ambiente;
                                cambioAmbiente = true;
                                break;
                            }
                        }

                        //Despliego al usuario
                        if (cambioAmbiente)
                        {
                            MessageBox.Show("Se cambió de manera éxitosa al ambiente " + (cmbOrigenes.SelectedItem as M.Ambientes).Nombre, "Cambio exitoso");
                            this.Close();
                        }
                    }
                    else
                        MessageBox.Show("Favor de seleccionar un ambiente", "Sin Ambiente");
                }
                else
                    MessageBox.Show("La contraseña tecleada esta incorrecta", "Contraseña Incorrecta");
            }
            else
                MessageBox.Show("Favor de escribir la contraseña", "Sin Contraseña");


        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {            
            cmbOrigenes.SelectedValuePath = "NombreCorto";
            cmbOrigenes.DisplayMemberPath = "Nombre";
            
            cmbOrigenes.ItemsSource = Config.DB.Ambientes;

            if (Config.DB.AmbienteActual != null &&
                !string.IsNullOrWhiteSpace(Config.DB.AmbienteActual.NombreCorto))
            {
                cmbOrigenes.SelectedValue = Config.DB.AmbienteActual.NombreCorto;
            }
        }     

        private void cmbOrigenes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
        }
    }
}
