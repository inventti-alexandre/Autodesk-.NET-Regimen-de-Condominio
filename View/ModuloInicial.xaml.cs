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

namespace RegimenCondominio.View
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
            if (FraccionamientoCombo.SelectedItem != null
                && FraccionamientoCombo.SelectedItem.ToString() != string.Empty)
            {
                (this.Resources["expandFracc"] as
                    System.Windows.Media.Animation.Storyboard).Begin();
                
                //Asigno Municipio
                EstadoBox.Text = C.Met_Inicio.FormateaString(
                    M.Inicio.ResultFraccs.
                    Where(i => i.Fraccionamiento == FraccionamientoCombo.SelectedItem.ToString().ToUpper()).
                    Select(j => j.Estado).FirstOrDefault().ToString());


                municipioBox.Text = C.Met_Inicio.FormateaString(
                    M.Inicio.ResultFraccs.
                    Where(i => i.Fraccionamiento == FraccionamientoCombo.SelectedItem.ToString().ToUpper()).
                    Select(j => j.Municipio).FirstOrDefault().ToString());
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

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {

            //Usuario de sesión de Windows.
            Usuariotxt.Text = M.Inicio.Usuario;

            //Si ya se había ejecutado anteriormente, sólo carga la información
            if (M.Inicio.ResultFraccs != null && M.Inicio.ResultTipoVivs != null)
            {
                //Si la lista contiene fraccionamientos
                if (M.Inicio.ResultFraccs.Count > 0)
                    FraccionamientoCombo.ItemsSource = M.Inicio.ResultFraccs.
                                            Select(x => C.Met_Inicio.FormateaString(x.Fraccionamiento)).ToList();

                //Si la lista contiene Tipo de Viviendas
                if (M.Inicio.ResultTipoVivs.Count > 0)
                    tipoVivCombo.ItemsSource = M.Inicio.ResultTipoVivs.
                                            Select(x => C.Met_Inicio.FormateaString(x.NombreTipoViv)).ToList();
            }
            else
            {
                //Cargo Fraccionamientos ejecutando consulta a BD
                new C.SqlTransaction(null, LoadDataTask, DataLoaded).Run();
            }

        }

        #region CargaDatos
        private void DataLoaded(object result)
        {
            //Convierto a DtSet lo obtenido desde la BD
            DataSet dtSet = result as DataSet;

            //Inicializo listas
            M.Inicio.ResultFraccs = new List<M.DatosFracc>();
            M.Inicio.ResultTipoVivs = new List<M.DatosTipoViv>();

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
                        string row = C.Met_Inicio.ReturnRow(dtrow, separator);

                        string[] cell = row.Split(separator);

                        M.Inicio.ResultFraccs.Add(new M.DatosFracc
                        {
                            Fraccionamiento = cell[0],//NOMBRE_FRACC
                            Estado = cell[1],//ESTADO
                            Municipio = cell[2]//MUNICIPIO

                        });                      
                    }                   
                }
                //Si es la tabla de Tipo de Viviendas, asigno rows.
                else if(i == int.Parse(Config.DB.OrdenQueryInicio[1, 0]))
                {
                    foreach (DataRow dtrow in Table.Rows)
                    {
                        string row = C.Met_Inicio.ReturnRow(dtrow, separator);

                        //Divido los renglones con celda
                        string[] cell = row.Split(separator);

                        //Agrego 
                        M.Inicio.ResultTipoVivs.Add(new M.DatosTipoViv
                        {
                            IdTipoViv = int.Parse(cell[0]),
                            NombreTipoViv = cell[1]

                        });
                    }
                }
            }

            //Si la lista contiene fraccionamientos
            if (M.Inicio.ResultFraccs.Count > 0)
                FraccionamientoCombo.ItemsSource = M.Inicio.ResultFraccs.
                                        Select(x => C.Met_Inicio.FormateaString(x.Fraccionamiento)).ToList();

            //Si la lista contiene Tipo de Viviendas
            if (M.Inicio.ResultTipoVivs.Count > 0)
                tipoVivCombo.ItemsSource = M.Inicio.ResultTipoVivs.
                                        Select(x => C.Met_Inicio.FormateaString(x.NombreTipoViv)).ToList();

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

        private object LoadDataTask(C.SQL_Connector conn, object input, BackgroundWorker bg)
        {            
            return conn.SelectTables(string.Format(Config.DB.QueryInicio, Environment.UserName.ToUpper()));                             
        }

        #endregion

    }
}
