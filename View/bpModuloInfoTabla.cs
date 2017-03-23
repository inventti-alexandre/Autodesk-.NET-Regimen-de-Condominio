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
using RegimenCondominio.C;
using Autodesk.AutoCAD.DatabaseServices;

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

            //for (int i = 0; i < 50; i++)
            //{
            //    ConnectionItems.Add(new M.Medidas
            //    {
            //        Apartamento = "A",
            //        Lote = "5",
            //        Calle = "BOREAL",
            //        AreaTotalCubierta = "1900.77",
            //        CEstacionamiento = "111.11",
            //        CLavanderia = "111.11",
            //        CPasillo = "111.11",
            //        CPatio = "111.11",
            //        CPlantaAlta = "111.11",
            //        CPlantaBaja = "111.11",
            //        DEstacionamiento = "111.11",
            //        DLavanderia = "111.11",
            //        DPasillo = "111.11",
            //        DPatio = "111.11",
            //        DPlantaAlta = "111.11",
            //        DPlantaBaja = "111.11",
            //        Manzana = "500",
            //        NoOficial = "125-A"
            //    });
            //}
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            M.InfoTabla.LotesInfoTabla = new ObservableCollection<M.CheckedListItem<M.LoteInfo>>();

            dtDetalle.ItemsSource = ConnectionItems;
            dtHeader.ItemsSource = ConnectionItems;            
            lBoxLoteCalle.ItemsSource = M.InfoTabla.LotesInfoTabla;

            AddSourceItems();

            for (int i = 0; i < 30; i++)
                M.Colindante.Lotes.Add(new M.Lote() { numLote = (i + 1) });

            foreach (M.Lote mLote in M.Colindante.Lotes)
            {
                bool esIrregular = false,
                     esLoteBase = false;

                string TipoLote = "";

                long loteTipo = new long();

                //Si esta dentro de los Irregulares
                esIrregular = M.Colindante.IdsIrregulares.Contains(new Handle(mLote._long).toObjectId());

                //Asigno el Tipo de Lote
                TipoLote = esIrregular ? "Irregular" : "Regular";

                //Busco el Lote Tipo
                loteTipo = M.Colindante.IdTipo.Handle.Value;

                if (esIrregular)
                    esLoteBase = true;
                else if (loteTipo == mLote._long)
                    esLoteBase = true;                

                M.InfoTabla.LotesInfoTabla.Add(new M.CheckedListItem<M.LoteInfo>()
                {
                    IsChecked = false,
                    Item =
                    {
                        NombreLote = "Lote " + mLote.numLote,
                        tipoLote = TipoLote,
                        esLoteBase = esLoteBase,
                        _long = mLote._long
                    }
                });             

            }

            if (!M.Manzana.EsMacrolote)
            {                
                if (C.Met_InfoTabla.GetInverseDirection())
                {
                    txtCalleInversa.Text = M.InfoTabla.RumboInverso.textColindancia;
                    txtRumboInverso.Text = M.InfoTabla.RumboInverso.rumboActual;
                }
            }
                   
        }

        private void AddSourceItems()
        {            

            //Agrego items al asignado por
            List<string> lCmbAsignado = new List<string>() { "Todos", "Pares", "Impares" };
            cmbAsignadoPor.ItemsSource = lCmbAsignado;
            cmbAsignadoPor.SelectedIndex = 0;

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

        private void checkAll_Checked(object sender, RoutedEventArgs e)
        {
            if (lBoxLoteCalle.ItemsSource != null)
            {
                CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(this.lBoxLoteCalle.ItemsSource);
                
                foreach (M.LoteTable<M.LoteInfo> item in view)
                    item.IsChecked = true;
            }
        }

        private void checkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            //foreach (M.LoteTable<M.LoteInfo> item in M.InfoTabla.LotesInfoTabla)
            //    item.IsChecked = false;
        }

        private void dtDetalle_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dtHeader.Items.Count > 0)
            {
                DataGrid dt = sender as DataGrid;

                dtHeader.SelectedIndex = dt.SelectedIndex;
            }
        }

        private void cmbAsignadoPor_SelectChanged(object sender, SelectionChangedEventArgs e)
        {
            if(lBoxLoteCalle.ItemsSource != null)
            {
                CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(this.lBoxLoteCalle.ItemsSource);
                
                if (cmbAsignadoPor.SelectedIndex == 0)
                {
                    view.Filter = null;
                }
                else if(cmbAsignadoPor.SelectedIndex == 1)
                {
                    view.Filter = null;
                    view.Filter = new Predicate<object>(IsEven);
                }
                else if(cmbAsignadoPor.SelectedIndex == 2)
                {
                    view.Filter = null;
                    view.Filter = new Predicate<object>(IsOdd);
                }
                
               
            }
        }

        /// <summary>
        /// Determina si es Impar
        /// </summary>
        /// <param name="item"></param>
        /// <returns>Verdadero si es impar</returns>
        public static bool IsOdd(object item)
        {
            M.LoteTable<M.LoteInfo> itemCheck = item as M.LoteTable<M.LoteInfo>;

            string sNum = itemCheck.Item.NombreLote.GetAfterSpace();

            int result = 0;
            if (int.TryParse(sNum, out result))
            {
                return result % 2 != 0;
            }
            else
                return false;
            
        }

        /// <summary>
        /// Determina si es par
        /// </summary>
        /// <param name="item"></param>
        /// <returns>Verdadero si es par</returns>
        public static bool IsEven(object item)
        {
            M.LoteTable<M.LoteInfo> itemCheck = item as M.LoteTable<M.LoteInfo>;

            string sNum = itemCheck.Item.NombreLote.GetAfterSpace();

            int result = 0;
            if (int.TryParse(sNum, out result))
            {
                return result % 2 == 0;
            }
            else
                return false;

        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            //Sólo evaluo si estaba deshabilitado
            if (!bPaso1.IsEnabled)
                if (M.InfoTabla.LotesInfoTabla.Any(x => x.Item.esRumboInverso))
                    bPaso1.IsEnabled = true;
            
            //Le doy formato
            lblLoteActual.Foreground = Brushes.Black;
            lblProgreso.Foreground = Brushes.Black;
            valueLoteActual.Foreground = Brushes.Red;

        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            //Sólo evaluo si ya estaba habilitado
            if (bPaso1.IsEnabled)
                if (M.InfoTabla.LotesInfoTabla.All(x => !x.Item.esRumboInverso))
                    bPaso1.IsEnabled = false;

            //Le doy formato
            lblLoteActual.Foreground = Brushes.LightGray;
            lblProgreso.Foreground = Brushes.LightGray;
            valueLoteActual.Foreground = Brushes.LightGray;
        }

        private void btnAtras_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
