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
using System.ComponentModel;
using System.Data;
using MahApps.Metro.Controls.Dialogs;
using RegimenCondominio.C;

namespace RegimenCondominio.V
{
    /// <summary>
    /// Interaction logic for ModuloInicial.xaml
    /// </summary>
    public partial class ModuloInicial : MetroWindow
    {
        public ModuloInicial()
        {
            InitializeComponent();
        }       

        bool SiEstado = false,
             SiMunicipio = false,
             SiSector = false,
             SiRegion = false;

        #region Animaciones


        private void RegionBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            
            if (RegionBox.Text.Length == 1 && !SiRegion)
            {
                (this.Resources["expandRegion"] as
                    System.Windows.Media.Animation.Storyboard).Begin();                
            }
            else if (RegionBox.Text.Length == 0)
            {
                SiRegion = false;
                (this.Resources["hideRegion"] as
                System.Windows.Media.Animation.Storyboard).Begin();

            }
        }

        private void tipoVivCombo_SelectChanged(object sender, SelectionChangedEventArgs e)
        {
            if (tipoVivCombo.SelectedItem != null
                && tipoVivCombo.SelectedItem.ToString() != string.Empty)
            {
                (this.Resources["expandTipoViv"] as
                    System.Windows.Media.Animation.Storyboard).Begin();
            }
            else
                (this.Resources["hideTipoViv"] as
                System.Windows.Media.Animation.Storyboard).Begin();
        }

        private void FraccionamientoCombo_SelectChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FraccionamientoCombo.SelectedItem != null)
            {
                (this.Resources["expandFracc"] as
                    System.Windows.Media.Animation.Storyboard).Begin();

                //Obtengo el Fraccionamiento seleccionado
                M.Fraccionamiento mItemSelected = (M.Fraccionamiento) FraccionamientoCombo.SelectedItem;

                EstadoBox.Text = mItemSelected.Estado;

                municipioBox.Text = mItemSelected.Municipio;
            }
            else
                (this.Resources["hideFracc"] as
                System.Windows.Media.Animation.Storyboard).Begin();
        }

        private void EstadoBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            
            if (EstadoBox.Text.Length > 0 && !SiEstado)
            {
                (this.Resources["expandlabel"] as
                    System.Windows.Media.Animation.Storyboard).Begin();
                //Comienzo animación para reducir textbox
                (this.Resources["kExpEstadoBox"] as
                System.Windows.Media.Animation.Storyboard).Begin();
                SiEstado = true;
            }
            else if (EstadoBox.Text.Length == 0)
            {
                SiEstado = false;
                (this.Resources["hidelabel"] as
                System.Windows.Media.Animation.Storyboard).Begin();

            }
        }


        private void municipioBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (municipioBox.Text.Length > 0 && !SiMunicipio)
            {
                (this.Resources["expandMunicipio"] as
                    System.Windows.Media.Animation.Storyboard).Begin();
                //Comienzo animación para reducir textbox
                (this.Resources["kExpMunicipioBox"] as
                System.Windows.Media.Animation.Storyboard).Begin();
                SiMunicipio = true;                
            }
            else if (municipioBox.Text.Length == 0)
            {
                SiMunicipio = false;
                (this.Resources["hideMunicipio"] as
                System.Windows.Media.Animation.Storyboard).Begin();

            }
        }
        private void sectorBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sectorBox.Text.Length == 1 && !SiSector)
            {
                (this.Resources["expandSector"] as
                    System.Windows.Media.Animation.Storyboard).Begin();
                SiSector = true;
            }
            else if (sectorBox.Text.Length == 0)
            {
                SiSector = false;
                (this.Resources["hideSector"] as
                System.Windows.Media.Animation.Storyboard).Begin();

            }
        }

        #endregion

        private void btnSiguiente_Click(object sender, RoutedEventArgs e)
        {
            //Reviso que no haya valores nulos, espacios en blanco o 
            if (FraccionamientoCombo.SelectedItem != null //Fraccionamiento
                && !string.IsNullOrWhiteSpace((EstadoBox.Text ?? "").ToString())//Estado
                && !string.IsNullOrWhiteSpace((municipioBox.Text ?? "").ToString())//Municipio
                && !string.IsNullOrWhiteSpace((RegionBox.Text ?? "").ToString()) //Region
                && !string.IsNullOrWhiteSpace((sectorBox.Text ?? "").ToString())//Sector
                && tipoVivCombo.SelectedItem != null) //FraccionamientoCombo              
            {
                //Asigno Valores seleccionados
                M.Inicio.Fraccionamiento = (M.Fraccionamiento) FraccionamientoCombo.SelectedItem;
                M.Inicio.Estado = EstadoBox.Text;
                M.Inicio.Municipio = municipioBox.Text;
                M.Inicio.Region = RegionBox.Text;
                M.Inicio.Sector = sectorBox.Text;

                //Calculo Tipo de Vivienda
                M.EncabezadoMachote mItemMachote = 

                M.Inicio.EncMachote = (M.EncabezadoMachote)(M.EncabezadoMachote)tipoVivCombo.SelectedItem;
                              
                ModuloManzana M_Manzana = new ModuloManzana();
                M_Manzana.Show();
                M.Constant.IsAutoClose = true;
                this.Close();

            }
            else
            {
                this.ShowMessageAsync("Valores en Blanco", "Favor de llenar todos los campos");
            }
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {

            //Usuario de sesión de Windows.
            Usuariotxt.Text = M.Constant.Usuario;

            //Si ya se había ejecutado anteriormente, sólo carga la información
            if (M.Inicio.ResultFraccs != null && M.Inicio.ResultTipoVivs != null
                && M.Inicio.ResultFraccs.Count > 0 && M.Inicio.ResultTipoVivs.Count > 0)
            {
                //Si la lista contiene fraccionamientos                
                FraccionamientoCombo.ItemsSource = M.Inicio.ResultFraccs;

                //Si la lista contiene Tipo de Viviendas                
                tipoVivCombo.ItemsSource = M.Inicio.ResultTipoVivs;

                //Habilito los controles
                ShowControls();

                //Si ya cuenta con datos los selecciono en la Pantalla--------------------------------
                FraccionamientoCombo.SelectedItem = M.Inicio.Fraccionamiento != null ? M.Inicio.Fraccionamiento: null;

                //Si ya cuenta con datos los selecciono en la Pantalla--------------------------------
                RegionBox.Text = !string.IsNullOrWhiteSpace(M.Inicio.Region)
                                    ? M.Inicio.Region : "";

                sectorBox.Text = !string.IsNullOrWhiteSpace(M.Inicio.Sector)
                                    ? M.Inicio.Sector : "";

                tipoVivCombo.SelectedItem = M.Inicio.EncMachote != null ? M.Inicio.EncMachote : null;
                //------------------------------------------------------------------------------------

            }
            else
            {
                //Obtengo Fraccionamientos ejecutando consulta a BD
                new C.SqlTransaction(null, LoadDataTask, DataLoaded).Run();
            }

        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            //Envío itemsource a nulls
            tipoVivCombo.ItemsSource = null;
            FraccionamientoCombo.ItemsSource = null;

            //Envío a vacío 
            EstadoBox.Text = "";
            municipioBox.Text = "";

            //Obtengo Fraccionamientos ejecutando consulta a BD
            new C.SqlTransaction(null, LoadDataTask, DataLoaded).Run();

        }

        private void MetroWindow_Closing(object sender, CancelEventArgs e)
        {
            //MessageDialogResult dg = this.ShowMessageAsync("Cerrando",
            //                "¿Desea Cerrar la ventana? \n Se perderá el avance completado", MessageDialogStyle.AffirmativeAndNegative);

            if (!M.Constant.IsAutoClose)
            {
                MessageBoxResult dg = MessageBox.Show("¿Desea Cerrar la ventana? \n Se perderá el avance completado", "Cerrando",
                                            MessageBoxButton.YesNo, MessageBoxImage.Exclamation);

                if (dg == MessageBoxResult.No)
                    e.Cancel = true;
                else
                {
                    C.Met_General.ClearData();
                    M.Inicio.IsOpen = false;
                }
            }
            else
                M.Constant.IsAutoClose = false;
        }

        private void ShowControls()
        {
            //Envío a visible Nombre de Usuario, Logo
            Usuariotxt.Visibility = Visibility.Visible;
            Logo.Visibility = Visibility.Visible;
            btnSiguiente.Visibility = Visibility.Visible;

            //Oculto el progress Ring
            Progress.Visibility = Visibility.Collapsed;
            txtCargando.Visibility = Visibility.Collapsed;

            //Habilito Grid
            GridPrincipal.IsEnabled = true;
        }
        #region CargaDatos        
        /// <summary>
        /// Datos obtenidos de la consulta realizada
        /// </summary>
        /// <param name="result">Resultado de consulta, en este caso es un DataSet</param>
        private void DataLoaded(object result)
        {
            //Convierto a DtSet lo obtenido desde la BD
            DataSet dtSet = result as DataSet;

            //Inicializo listas
            M.Inicio.ResultFraccs = new List<M.Fraccionamiento>();
            M.Inicio.ResultTipoVivs = new List<M.EncabezadoMachote>();

            char separator = '|';

            for(int i=0; i< dtSet.Tables.Count; i++)
            {
                //Asigno tabla
                DataTable Table = dtSet.Tables[i];

                //Reviso que tabla es la que esta asignada
                //Si es la tabla de Fraccionamientos, asigno rows
                if (i == int.Parse(Config.DB.OrdenQueryInicio[0,0]))
                {
                    //Por cada renglon ingreso 
                    foreach(DataRow dtrow in Table.Rows)
                    {
                        string row = dtrow.RowToString(separator);

                        string[] cell = row.Split(separator);

                        M.Inicio.ResultFraccs.Add(new M.Fraccionamiento
                        {
                            Id_Fracc = cell[0],
                            fraccionamiento = cell[1],//NOMBRE_FRACC
                            Estado = cell[2],//ESTADO
                            Municipio = cell[3]//MUNICIPIO

                        });                      
                    }                   
                }
                //Si es la tabla de Tipo de Viviendas, asigno rows.
                else if(i == int.Parse(Config.DB.OrdenQueryInicio[1, 0]))
                {
                    foreach (DataRow dtrow in Table.Rows)
                    {
                        string row = dtrow.RowToString(separator);

                        //Divido los renglones con celda
                        string[] cell = row.Split(separator);

                        //Agrego 
                        M.Inicio.ResultTipoVivs.Add(new M.EncabezadoMachote
                        {
                            IdMachote = int.Parse(cell[0]),
                            Encabezado = cell[1],
                            Cant_Viviendas = int.Parse(cell[2])

                        });
                    }
                }
            }

            //Si la lista contiene fraccionamientos
            if (M.Inicio.ResultFraccs.Count > 0)
                FraccionamientoCombo.ItemsSource = M.Inicio.ResultFraccs;

            //Si la lista contiene Tipo de Viviendas
            if (M.Inicio.ResultTipoVivs.Count > 0)
                tipoVivCombo.ItemsSource = M.Inicio.ResultTipoVivs;

            //Desbloqueo controles
            ShowControls();              
        }

        /// <summary>
        /// Realiza la consulta a la BD mediante BGWorker
        /// </summary>
        /// <param name="conn">conexión</param>
        /// <param name="input">input.</param>
        /// <param name="bg">bg.</param>
        /// <returns></returns>
        private object LoadDataTask(C.SQL_Connector conn, object input, BackgroundWorker bg)
        {            
            return conn.SelectTables(string.Format(Config.DB.QuerysInicio, Environment.UserName.ToUpper()));                             
        }

        #endregion

    }
}
