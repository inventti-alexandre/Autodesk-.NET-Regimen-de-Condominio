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
using RegimenCondominio.C;

namespace RegimenCondominio.View
{
    /// <summary>
    /// Interaction logic for ModuloInicial.xaml
    /// </summary>
    public partial class ModuloInicial : MetroWindow
    {
        List<string> Nombre = new List<string>()
        {
            "Vivienda 1",
            "Vivienda 2"
        };

        List<string> Calle = new List<string>()
        {
            "Calle 1",
            "Calle 2"
        };

        public ModuloInicial()
        {
            InitializeComponent();
        }       

        bool SiEstado = false,
             SiMunicipio = false,
             SiSector = false,
             SiRegion = false;

        #region Animaciones
        private void EstadoBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var source = e.Source;
            if (EstadoBox.Text.Length > 0 && !SiEstado)
            {                
                (this.Resources["expandlabel"] as
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

                EstadoBox.Text = C.Inicio.FormateaString(
                    M.Inicio.ResultFraccs.
                    Where(i => i.Fraccionamiento == FraccionamientoCombo.SelectedItem.ToString().ToUpper()).
                    Select(j => j.Estado).FirstOrDefault().ToString());

                municipioBox.Text = Inicio.FormateaString(
                    M.Inicio.ResultFraccs.
                    Where(i => i.Fraccionamiento == FraccionamientoCombo.SelectedItem.ToString().ToUpper()).
                    Select(j => j.Municipio).FirstOrDefault().ToString());
            }
            else
                (this.Resources["hideFracc"] as
                System.Windows.Media.Animation.Storyboard).Begin();
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

        private void municipioBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (municipioBox.Text.Length > 0 && !SiMunicipio)
            {
                (this.Resources["expandMunicipio"] as
                    System.Windows.Media.Animation.Storyboard).Begin();
                SiMunicipio = true;
                Controls.CsTextbox.SiControl = true;
            }
            else if (municipioBox.Text.Length == 0)
            {
                SiMunicipio = false;
                (this.Resources["hideMunicipio"] as
                System.Windows.Media.Animation.Storyboard).Begin();

            }
        }

        #endregion

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {            

            Usuariotxt.Text =  Environment.UserName.ToUpper();            

            //Cargo Fraccionamientos
            new C.SqlTransaction(null, LoadDataTaskFracc, DataLoadedFracc).Run();

        }

        private void DataLoadedFracc(object result)
        {
            List<string> Fraccionamientos = new List<string>();

            List<string> Flist = ((result as List<M.DatosFracc>).
                                    Select(x => Inicio.FormateaString (x.Fraccionamiento)).ToList());

            FraccionamientoCombo.ItemsSource = Flist;

            new C.SqlTransaction(null, LoadDataTaskTipoViv, DataLoadedTipoViv).Run();

        }

        private void DataLoadedTipoViv(object result)
        {
            List<string> listTipoViv = new List<string>();

            foreach (var item in result as List<M.DatosTipoViv>)
            {                
                listTipoViv.Add(item.NombreTipoViv);
            }

            //Envío todos los resultados al combobox
            tipoVivCombo.ItemsSource = listTipoViv;

            //Envío a visible cuando termina de cargar
            Usuariotxt.Visibility = Visibility.Visible;
            Logo.Visibility = Visibility.Visible;
            btnSiguiente.Visibility = Visibility.Visible;

            //Oculto progress Ring
            Progress.Visibility = Visibility.Hidden;
        }

        private object LoadDataTaskTipoViv(SQL_Connector conn, object input, BackgroundWorker bg)
        {
            List<String> result;

            M.Inicio.ResultTipoVivs.Clear();

            if (conn.Select("select ID_TIPOVIV,NOMBRE_TIPOVIV from TIPO_VIVS", out result, '|'))
            {
                string[] cell;
                foreach (String row in result)
                {
                    cell = row.Split('|');

                    M.Inicio.ResultTipoVivs.Add(new M.DatosTipoViv()
                    {
                        IdTipoViv = int.Parse(cell[0]),
                        NombreTipoViv = cell[1]
                    });
                }
            }

            return M.Inicio.ResultTipoVivs;
        }

        private object LoadDataTaskFracc(C.SQL_Connector conn, object input, BackgroundWorker bg)
        {
            List<String> result;

            //Limpio la lista
            M.Inicio.ResultFraccs.Clear();

            if (conn.Select(string.Format("select NOMBRE,ESTADO,MUNICIPIO from viFraccsUsuario where usuario = '{0}'",Environment.UserName.ToUpper()),
                out result, '|'))
            {
                String[] cell;
                foreach (String row in result)
                {
                    cell = row.Split('|');

                    M.Inicio.ResultFraccs.Add(new M.DatosFracc()
                    {
                        Fraccionamiento = cell[0],//NOMBRE_FRACC
                        Estado = cell[1],//ESTADO
                        Municipio = cell[2]//MUNICIPIO
                    });
                }
            }

            return M.Inicio.ResultFraccs;
        }


    }
}
