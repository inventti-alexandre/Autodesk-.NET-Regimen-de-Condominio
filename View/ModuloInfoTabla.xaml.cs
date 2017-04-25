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
using CADDB = Autodesk.AutoCAD.DatabaseServices;
using System.Collections.Specialized;
using System.ComponentModel;
using FormWindow = System.Windows.Forms;
using System.Data;
using System.Reflection;
using System.Windows.Controls.Primitives;
using System.Data.SqlClient;

namespace RegimenCondominio.V
{
    /// <summary>
    /// Lógica de interacción para ModuloInfoTabla.xaml
    /// </summary>
    public partial class ModuloInfoTabla : MetroWindow
    {
        private long LongActual = -1;

        private int ColumnOver = -1;

        private BackgroundWorker bkWorker;

        private List<long> lotesDocs;

        private string filePath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

        private bool exported = false;

        public ModuloInfoTabla()
        {
            InitializeComponent();
            
            M.InfoTabla.LotesSelected.CollectionChanged += new NotifyCollectionChangedEventHandler
                    (LotesChangedMethod);                     
        }

        private void Factories()
        {
            for (int i = 0; i < 30; i++)
                M.Colindante.Lotes.Add(new M.Lote() { numLote = (i + 1) , _long = (i)});

            for (int i = 0; i < 30; i++)
                M.InfoTabla.MedidasGlobales.Add(new M.Medidas() { NumLote = (i + 1) , LongLote = i});

            //for (int i = 0; i < 3; i++)
            //    M.Colindante.IdsIrregulares.Add(new Handle(M.Colindante.Lotes[i]._long).toObjectId());            

            #region Manzana
            M.Manzana.RumboFrente = new M.ManzanaData();
            M.Manzana.RumboFrente.rumboActual = "Norte";
            M.Manzana.NoManzana = 280;

            M.Manzana.ColindanciaManzana.Add(new M.ManzanaData()
            {
                inicialRumbo = "S",
                rumboActual = "Sur",
                textColindancia = "Calle al Sur"
            });

            M.Manzana.ColindanciaManzana.Add(new M.ManzanaData()
            {
                inicialRumbo = "N",
                rumboActual = "Norte",
                textColindancia = "Calle al Norte"
            });
            #endregion

            M.Inicio.Fraccionamiento.fraccionamiento = "Rinconada de Lago de Guadalupe";

            #region MAIN_dATA
            //Factory para Macrolotes
            if (M.Manzana.EsMacrolote)
            {
                for (int j = 0; j < 2; j++)
                {
                    for (int i = 0; i < 20; i++)
                    {
                        M.Colindante.MainData.Add(new M.ColindanciaData()
                        {
                            Edificio_Lote = j + 1,
                            Apartamento = (i + 1).ToEnumerate(),
                            NoOficial = "10" + i
                        });

                        M.Colindante.MainData.Add(new M.ColindanciaData()
                        {
                            Edificio_Lote = j + 1,
                            Apartamento = (i + 1).ToEnumerate(),
                            NoOficial = "10" + i
                        });
                    }
                }
            }
            else
            {
                for (int i = 0; i < 20; i++)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        M.Colindante.MainData.Add(new M.ColindanciaData()
                        {
                            Edificio_Lote = i,//Lote
                            Apartamento = (j + 1).ToEnumerate(),//Apartamento
                            NoOficial = "10" + i,//Número Oficial
                            idVivienda = i + 1
                        });

                        M.Colindante.MainData.Add(new M.ColindanciaData()
                        {
                            Edificio_Lote = i,//Lote
                            Apartamento = (j + 1).ToEnumerate(),//Apartamento
                            NoOficial = "10" + i,//Número Oficial
                            idVivienda = i + 1
                        });
                    }
                }
            }
            #endregion

        }

        #region Cada Carga de la Ventana
        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            List<long> lotesBase, lotesTipo;            
            //Factories();            
            
            #region Determinar Columnas a Ocultar
            
            //Obtengo todas las Propiedades Ligadas a su columna
            M.InfoTabla.AllProperties = Met_InfoTabla.DescribeColumns();

            //Oculto las columnas
            HideDetailColumns();
            
            #endregion                      

            //Obtengo todos los lotes del checklist
            C.Met_InfoTabla.GetChecklistItems(out lotesBase, out lotesTipo);

            //Limpiar listado
            M.InfoTabla.LotesBase.Clear();

            //Lo vuelvo a asignar
            foreach (long lote in lotesBase)
                M.InfoTabla.LotesBase.Add(lote, false);

            //Obtengo la información de Header (Tabla Estática)
            C.Met_InfoTabla.GetHeaderTable(M.Manzana.EsMacrolote);            

            GeneralSource();

            if (M.Manzana.EsMacrolote)
            {                
                AdaptMacroView();
            }
            else
            {
                LoteItems();                

                //Obtengo la calle inversa
                if (C.Met_InfoTabla.GetInverseAddress())
                {
                    txtCalleInversa.Text = M.InfoTabla.RumboInverso.textColindancia;
                    txtRumboInverso.Text = M.InfoTabla.RumboInverso.rumboActual;
                }
            }

            //#region Asigno ProgresssBar

            ////Si ya se había ingresado anteriormente
            //if (M.InfoTabla.LotesBase.Count > 0)
            //{
            //    //Si todos los lotes que se habían capturado son los mismos que los que se obtuvieron     
            //    if (lotesBase.All(M.InfoTabla.LotesBase.Keys.Contains) && lotesBase.Count == M.InfoTabla.LotesBase.Count)
            //    {
            //        //Si la cantidad de Lotes es la Misma que se había obtenido                    
            //        int cont = 0;

            //        foreach (KeyValuePair<long, bool> keyItem in M.InfoTabla.LotesBase)
            //        {
            //            if (keyItem.Value)
            //                cont++;
            //        }

            //        //Lo asigno a la ProgressBar
            //        lblProgreso.Text = string.Format("{0}/{1} Completado", cont, lotesBase.Count);

            //        ChangeProgressBar(cont, lotesBase.Count);
            //    }
            //    else //Si algo cambio
            //    {
            //        int cont = 0;
            //        string msgOut;

            //        M.InfoTabla.LotesBase.Clear();

            //        foreach (long lote in lotesBase)
            //        {
            //            if (Met_InfoTabla.HasEmptyFields(lote, out msgOut))
            //            {
            //                M.InfoTabla.LotesBase.Add(lote, false);
            //            }
            //            else
            //            {
            //                M.InfoTabla.LotesBase.Add(lote, true);
            //                cont++;
            //            }
            //        }
            //        //Lo asigno a la ProgressBar
            //        lblProgreso.Text = string.Format("{0}/{1} Completado", cont, lotesBase.Count);

            //        ChangeProgressBar(cont, lotesBase.Count);
            //    }
            //}
            //else //Si es la primera vez lo asigno de manera directa
            //{
            //    foreach (long lote in lotesBase)
            //        M.InfoTabla.LotesBase.Add(lote, false);

            //    //Lo asigno a la ProgressBar
            //    lblProgreso.Text = string.Format("0/{0} Completado", lotesBase.Count);
            //}

            //#endregion
        }
        

        private void HideDetailColumns()
        {
            //Obtengo las Columnas a Ocultar
            List<M.DataColumns> columnsToHide = Met_InfoTabla.DetailColumnsToHide();

            IEnumerable<string> propertiesToHide = columnsToHide.Select(x => x.PropertyName);

            //Cambio las propiedades que se deben de ocultar a falso
            foreach(M.DataColumns data in M.InfoTabla.AllProperties)
            {
                //Si es del tipo de Dato Columna
                if(data.TipoEnumerador == typeof(M.DetailColumns) && propertiesToHide.Contains(data.PropertyName))                
                    data.esVisible = false;                
            }            

            foreach (M.DataColumns colActual in columnsToHide)
            {
                int columnaMaxCubierta = (int)M.DetailColumns.AreaTotalCubierta,
                    columnaMaxDescubierta = (int)M.DetailColumns.AreaTotalDescubierta,
                    colIndex = -1;

                //Index de la columna a ocultar
                M.DetailColumns ColEnumerator = (M.DetailColumns)Enum.Parse(typeof(M.DetailColumns), colActual.PropertyName);

                //Coniverto el enumerador al index de la columna correspondiente
                colIndex = (int)ColEnumerator;

                //Oculto la columna
                dtDetalle.Columns[colIndex].Visibility = Visibility.Collapsed;

                //Oculto el total asignado a la columna
                stackTotales.Children[colIndex].Visibility = Visibility.Collapsed;

                //Obtengo la columna
                DataGridColumn dtColumnActual = dtDetalle.Columns[colIndex];

                //Si es descubierto voy a ocultar de la sección Cubierto y si no debe de quitar del descubierto
                if (colIndex < columnaMaxCubierta)
                    bordCubierto.Width = bordCubierto.Width - dtColumnActual.Width.Value;
                else if (colIndex < columnaMaxDescubierta)
                    bordDescubierto.Width = bordDescubierto.Width - dtColumnActual.Width.Value;
            }
            //Siempre oculto el Long            
        }

        private void LotesChangedMethod(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (M.Checked<M.LoteItem> item in e.OldItems)
                {
                    //Removed items
                    item.PropertyChanged -= LoteModelPropertyChanged;
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (M.Checked<M.LoteItem> item in e.NewItems)
                {
                    //Added items
                    item.PropertyChanged += LoteModelPropertyChanged;
                }
            }        
        }

        private void LoteModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsChecked")
            {
                //Obtengo el item ligado al ListBox
                M.Checked<M.LoteItem> itemCheck = sender as M.Checked<M.LoteItem>;

                //Obtengo el long del Lote
                long longItem = itemCheck.Item.Long;

                //obtengo todos los Items de Medidas Globales
                List<M.Medidas> listItemsMedida = M.InfoTabla.MedidasGlobales.SearchAll(longItem);

                //Comienzo a cambiar la calle de acuerdo a lo seleccionado
                foreach (M.Medidas itemMedida in listItemsMedida)
                {
                    if (itemCheck.IsChecked)
                        itemMedida.Calle = M.InfoTabla.RumboInverso.textColindancia;
                    else
                        itemMedida.Calle = M.Manzana.RumboFrente.textColindancia;
                }
            }
        }

        private void AdaptMacroView()
        {
            //Genero los Source Items
            MacroItems();

            //Muestro Columna de Edificio
            ColEdificio.Visibility = System.Windows.Visibility.Visible;
            ColApartamento.Width = 50;
            ColCalle.Width = 140;

            //Habilito la Columna de Área Exclusiva con su Total
            colAreaExclusiva.Visibility = Visibility.Visible;
            txtTotalAreaExclusiva.Visibility = Visibility.Visible;
            bordAreaExcl.Visibility = Visibility.Visible;

            //El paso 0 no será necesario
            bPaso0.IsEnabled = false;
            txtPaso0.Text = "Exclusivo para Lotes";
            txtPaso0.Foreground = Brushes.LightGray;

            //Habilito Paso 1
            bPaso1.IsEnabled = true;
            txtPaso1.Text = "1. Llenado de la tabla";

            //Habilito colores
            lblLoteActual.Foreground = Brushes.Black;
            //lblProgreso.Foreground = Brushes.Black;
            txtTipoLoteActual.Foreground = Brushes.Red;
            txtLotesBase.Foreground = Brushes.Black;
            txtLotesTipo.Foreground = Brushes.Black;

            //Desactivo los combobox
            cmbLotesBase.IsEnabled = false;
            cmbLotesTipo.IsEnabled = false;

            //Oculto textos Inversos
            txtRumboInverso.Visibility = System.Windows.Visibility.Hidden;
            txtCalleInversa.Visibility = System.Windows.Visibility.Hidden;

        }

        private void MacroItems()
        {

            long longMacrolote = M.Colindante.IdMacrolote.Handle.Value;

            M.Lote foundLote = M.Colindante.Lotes.Search(longMacrolote);

            if (foundLote != new M.Lote())
            {
                M.Checked<M.LoteItem> mCheckedItem = new M.Checked<M.LoteItem>();
                M.LoteItem mLoteItem = new M.LoteItem();
                mCheckedItem.IsChecked = false;
                mLoteItem.Long = foundLote._long;
                mLoteItem.Name = "Lote " + foundLote.numLote;
                mLoteItem.TipoLote = "Macrolote";
                mLoteItem.EsLoteBase = true;

                mCheckedItem.Item = mLoteItem;
                M.InfoTabla.LotesSelected = new ObservableCollection<M.Checked<M.LoteItem>>() { mCheckedItem };
                cmbLotesBase.ItemsSource = M.InfoTabla.LotesSelected;
                cmbLotesBase.SelectedIndex = 0;
            }


        }

        private void LoteItems()
        {
            //Agrego items al asignado por
            List<string> lFitros = new List<string>() { "Todos", "Pares", "Impares" };
            cmbAsignadoPor.ItemsSource = lFitros;
            cmbAsignadoPor.SelectedIndex = 0;

            //Asigno itemsource de Checklist a listado de calle            
            lBoxLoteCalle.ItemsSource = M.InfoTabla.LotesSelected;

            //Asigno itemsource a combobox
            cmbLotesBase.ItemsSource = M.InfoTabla.LotesSelected.Where(x => x.Item.EsLoteBase);            

            //Lotes Restantes
            cmbLotesTipo.ItemsSource = M.InfoTabla.LotesSelected.Where(x => !x.Item.EsLoteBase);            

        }

        private void GeneralSource()
        {
            //Datos Generales
            txtFraccionamiento.Text = M.Inicio.Fraccionamiento.fraccionamiento;
            txtManzana.Text = M.Manzana.NoManzana.ToString();
            txtRumboFrente.Text = M.Manzana.RumboFrente.rumboActual;

            //Datos Generales Conteo
            txtNoLotes.Text = M.Colindante.Lotes.Count.ToString();
            txtNoEdificios.Text = M.Colindante.Edificios.Count.ToString();
            txtNoApartamentos.Text = M.Colindante.OrderedApartments.Count.ToString();

            //Asigno el path de los combobox
            cmbLotesBase.SelectedValuePath = "Item.Long";
            cmbLotesBase.DisplayMemberPath = "Item.Name";            
            cmbLotesTipo.SelectedValuePath = "Item.Long";
            cmbLotesTipo.DisplayMemberPath = "Item.Name";

            //Asigno el nombre del Área Común en la tabla
            colAreaComun.Header = new TextBlock() { Text = M.Colindante.NomAreaComun, FontSize = 10 };

        }
        #endregion    

        #region Paso 0

        #region CheckBox Controls
        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (!bPaso1.IsEnabled)
            {
                if (M.InfoTabla.LotesSelected.Any(x => x.IsChecked))
                {
                    bPaso1.IsEnabled = true;

                    //Habilito colores
                    lblLoteActual.Foreground = Brushes.Black;
                    //lblProgreso.Foreground = Brushes.Black;
                    txtTipoLoteActual.Foreground = Brushes.Red;
                    txtLotesBase.Foreground = Brushes.Black;
                    txtLotesTipo.Foreground = Brushes.Black;
                }
            }

        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (bPaso1.IsEnabled)
            {
                //Si se quitaron todos, no doy la opción de avanzar
                if (M.InfoTabla.LotesSelected.All(x => !x.IsChecked))
                {
                    bPaso1.IsEnabled = false;
                    lblLoteActual.Foreground = Brushes.LightGray;
                    //lblProgreso.Foreground = Brushes.LightGray;
                    txtTipoLoteActual.Foreground = Brushes.LightGray;
                    txtLotesBase.Foreground = Brushes.LightGray;
                    txtLotesTipo.Foreground = Brushes.LightGray;
                }
            }
        }

        private void checkAll_Checked(object sender, RoutedEventArgs e)
        {
            if (lBoxLoteCalle.ItemsSource != null)
            {
                CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(this.lBoxLoteCalle.ItemsSource);

                foreach (M.Checked<M.LoteItem> item in view)
                    item.IsChecked = true;
                    
                if (!bPaso1.IsEnabled && M.InfoTabla.LotesSelected.Any(x => x.IsChecked))
                {
                    bPaso1.IsEnabled = false;
                    lblLoteActual.Foreground = Brushes.LightGray;
                    //lblProgreso.Foreground = Brushes.LightGray;
                    txtTipoLoteActual.Foreground = Brushes.LightGray;
                    txtLotesBase.Foreground = Brushes.Black;
                    txtLotesTipo.Foreground = Brushes.Black;
                }
            }
        }

        private void checkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            foreach (M.Checked<M.LoteItem> item in M.InfoTabla.LotesSelected)
                item.IsChecked = false;

            //Si se quitaron todos, no doy la opción de avanzar
            if (bPaso1.IsEnabled && M.InfoTabla.LotesSelected.All(x => !x.IsChecked))
            {
                bPaso1.IsEnabled = false;
                lblLoteActual.Foreground = Brushes.LightGray;
                //lblProgreso.Foreground = Brushes.LightGray;
                txtTipoLoteActual.Foreground = Brushes.LightGray;
                txtLotesBase.Foreground = Brushes.LightGray;
                txtLotesTipo.Foreground = Brushes.LightGray;
            }
        }

        #endregion

        private void cmbAsignadoPor_SelectChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lBoxLoteCalle.ItemsSource != null)
            {
                CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(this.lBoxLoteCalle.ItemsSource);
                view.Filter = null;

                if (cmbAsignadoPor.SelectedIndex == 1)
                    view.Filter = new Predicate<object>(IsEven);
                else if (cmbAsignadoPor.SelectedIndex == 2)
                    view.Filter = new Predicate<object>(IsOdd);

            }
        }

        /// <summary>
        /// Determina si es Impar
        /// </summary>
        /// <param name="item"></param>
        /// <returns>Verdadero si es impar</returns>
        public static bool IsOdd(object item)
        {
            M.Checked<M.LoteItem> itemCheck = item as M.Checked<M.LoteItem>;

            string sNum = itemCheck.Item.Name.GetAfterSpace();

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
            M.Checked<M.LoteItem> itemCheck = item as M.Checked<M.LoteItem>;

            string sNum = itemCheck.Item.Name.GetAfterSpace();

            int result = 0;
            if (int.TryParse(sNum, out result))
            {
                return result % 2 == 0;
            }
            else
                return false;

        }


        #endregion

        #region Paso 1      

        private void cmbLotesBase_SelectChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbLotesBase.SelectedIndex != -1)
            {
                //Envío el otro item a Nulo                
                cmbLotesTipo.SelectedIndex = -1;

                //Asigno el Long Nuevamente con el valor correcto              
                LongActual = (long)cmbLotesBase.SelectedValue;                

                if (dtDetalle.ItemsSource == null && dtHeader.ItemsSource == null)
                {
                    dtDetalle.ItemsSource = M.InfoTabla.MedidasGlobales;
                    dtHeader.ItemsSource = M.InfoTabla.MedidasGlobales;   
                }

                FilterDataGrids();

                ChangeToReadOnly(false);
                btnRevisarLoteActual.IsEnabled = true;
            }
        }

        private void cmbLotesTipo_SelectChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbLotesTipo.SelectedIndex != -1)
            {
                //Envío el otro item a Nulo
                cmbLotesBase.SelectedIndex = -1;

                //Asigno el Long Nuevamente con el valor correcto              
                LongActual = (long)(cmbLotesTipo.SelectedValue ?? -1);

                //Deshabilito algunas columnas
                //dtDetalle.IsEnabled = false;

                if (dtDetalle.ItemsSource == null && dtHeader.ItemsSource == null)
                {
                    dtDetalle.ItemsSource = M.InfoTabla.MedidasGlobales;
                    dtHeader.ItemsSource = M.InfoTabla.MedidasGlobales;
                }

                FilterDataGrids();

                ChangeToReadOnly(true);
                btnRevisarLoteActual.IsEnabled = false;
            }
        }

        private void FilterDataGrids()
        {
            M.Checked<M.LoteItem> itemLote = M.InfoTabla.LotesSelected.SearchByLong(LongActual);

            if (itemLote != null)
                if (itemLote.Item != null)
                    txtTipoLoteActual.Text = itemLote.Item.TipoLote ?? "Sin Asignar";
                else
                    txtTipoLoteActual.Text = "Sin Asignar";
            else
                txtTipoLoteActual.Text = "Sin Asignar";

            CollectionView viewDetail = (CollectionView)CollectionViewSource.GetDefaultView(this.dtDetalle.ItemsSource);
            CollectionView viewHeader = (CollectionView)CollectionViewSource.GetDefaultView(this.dtHeader.ItemsSource);

            if (viewDetail != null )
            {
                viewDetail.Filter = null;
                viewDetail.Filter = new Predicate<object>(FilterByLote);
            }

            if (viewHeader != null)
            {
                viewHeader.Filter = null;
                viewHeader.Filter = new Predicate<object>(FilterByLote);
            }

            CalculateTotals(viewDetail);
        }

        private void CalculateTotals(CollectionView viewDetail)
        {
            //Inicializo los totales nuevamente            
            Dictionary<string, decimal> propertyTotals = Met_InfoTabla.CalculatePropertyTotals(viewDetail);

            PropertyInfo[] propertiesTotales = typeof(M.TotalesMedidas).GetProperties();

            //Por cada Propiedad con la suma de totales
            foreach (KeyValuePair<string, decimal> propTotal in propertyTotals)
            {
                M.DetailColumns dtCol;
                //Reviso si es de las columnas de detalle
                if (Enum.TryParse<M.DetailColumns>(propTotal.Key, out dtCol))
                {
                    int indexCol = (int)dtCol;

                    //Por cada Propiedad de los totales
                    foreach (PropertyInfo propActual in propertiesTotales)
                    {
                        M.Totales valorPropiedad = (M.Totales)(propActual.GetValue(M.InfoTabla.TotalesTabla) ?? DBNull.Value);

                        if (valorPropiedad != null && valorPropiedad.Columna == dtCol)
                        {
                            //Modificado el valor
                            propActual.SetValue(M.InfoTabla.TotalesTabla, new M.Totales()
                            {
                                Total = propTotal.Value,
                                Columna = valorPropiedad.Columna
                            });

                            //Modifico el textbox correspondiente 
                            ((TextBlock)(stackTotales.Children[indexCol] as Border).Child).Text = propTotal.Value.ToString();
                        }
                    }
                                     
                }
            }            

        }

        public bool FilterByLote(object item)
        {
            M.Medidas mMedias = (M.Medidas)item;

            return mMedias.LongLote == LongActual;
            
        }

        private void CanPaste(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = dtDetalle.Items.Count > 0 && !dtDetalle.Columns[ColumnOver].IsReadOnly;
            e.Handled = true;            
        }

        private void Paste(object sender, ExecutedRoutedEventArgs e)
        {
            decimal result, 
                    sum = 0;

            //Obtengo la columna máximo del Enumerador M.DetailColumns
            M.DetailColumns dcMax = Enum.GetValues(typeof(M.DetailColumns)).Cast<M.DetailColumns>().Max();

            //Obtengo el texto que este copiado 
            string clipText = Clipboard.GetText();

            if (ColumnOver != -1 && ColumnOver <= ((int)dcMax))
            {
                if(ColumnOver == (int)M.DetailColumns.ExpedienteCatastral)
                {
                    if(clipText.Contains("-"))
                    {
                        int idxLastChar = clipText.LastIndexOf('-');

                        string  strAfterChar = clipText.Substring(idxLastChar+1),
                                strBeforeChar = clipText.Substring(0, idxLastChar);

                        int numberAfterChar;

                        if(int.TryParse(strAfterChar, out numberAfterChar))                          
                            Met_InfoTabla.ModificarExpCatastral(LongActual, null, numberAfterChar);                                                    
                        else                        
                            Met_InfoTabla.ModificarExpCatastral(LongActual, clipText);                        
                    }                        
                    else
                    {
                        int resultInt;

                        if (int.TryParse(clipText, out resultInt))                        
                            Met_InfoTabla.ModificarExpCatastral(LongActual, null , resultInt);                        
                    }
                }
                else
                {
                    //Reviso que sea un valor númerico
                    if (decimal.TryParse(clipText, out result))
                    {                        
                        //Parseo el indíce de la columna sobre la cual esta arriba
                        M.DetailColumns dtCol = (M.DetailColumns)Enum.ToObject(typeof(M.DetailColumns), ColumnOver);

                        //Obtengo la propiedad de esa columna a la que esta ligada
                        PropertyInfo editProp = typeof(M.Medidas).GetProperty(dtCol.ToString());

                        foreach (M.Medidas mItem in M.InfoTabla.MedidasGlobales)
                        {
                            if (mItem.LongLote == LongActual)
                            {
                                editProp.SetValue(mItem, result.ToString());
                                sum += result;
                                UpdateHorizontalTotals(mItem, dtCol, result);

                                if (!M.Manzana.EsMacrolote && LongActual == M.Colindante.IdTipo.Handle.Value)
                                    Met_InfoTabla.DuplicateEditedValues(mItem, result, editProp);
                            }
                        }

                        //Lo asigno al total seleccionado
                        foreach (PropertyInfo prop in typeof(M.TotalesMedidas).GetProperties())
                        {
                            //Obtengo el valor cada total
                            M.Totales itemProp = (M.Totales)prop.GetValue(M.InfoTabla.TotalesTabla);

                            //Reviso que sea la misma del enumerador
                            if (itemProp.Columna == dtCol)
                            {
                                //Le asigno el valor de la suma
                                prop.SetValue(M.InfoTabla.TotalesTabla, new M.Totales()
                                {
                                    Columna = itemProp.Columna,
                                    Total = sum
                                });

                                //Modifico el textblock de totales
                                ((TextBlock)(stackTotales.Children[ColumnOver] as Border).Child).Text = sum.ToString();

                                break;
                            }
                        }
                        //}
                    }
                }
            }            
           
        }

        private void ChangeToReadOnly(bool isReadOnly)
        {
            colCubPB.IsReadOnly = isReadOnly;
            colCubPA.IsReadOnly = isReadOnly;
            colCubLavanderia.IsReadOnly = isReadOnly;
            colCubEstac.IsReadOnly = isReadOnly;
            colCubPasillo.IsReadOnly = isReadOnly;
            colCubPatio.IsReadOnly = isReadOnly;
            colDescLav.IsReadOnly = isReadOnly;
            colDescEstac.IsReadOnly = isReadOnly;
            colDescPasillo.IsReadOnly = isReadOnly;
            colDescPatio.IsReadOnly = isReadOnly;
            colAreaComun.IsReadOnly = isReadOnly;
            colAreaExclusiva.IsReadOnly = isReadOnly;
            colProindiviso.IsReadOnly = isReadOnly;
            colPredioFrente.IsReadOnly = isReadOnly;
            colPredioFondo.IsReadOnly = isReadOnly;
            colPredioArea.IsReadOnly = isReadOnly;
            colAreaCons.IsReadOnly = isReadOnly;            
        }

        //Voy modificando los totales
        private void dtDetalle_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                //Obtengo el Index de las columnas
                int columnIndex = e.Column.DisplayIndex;

                //Obtengo el datagrid de donde fue in
                DataGridRow row = e.Row;

                //Obtengo el item
                M.Medidas mItemEditado = row.DataContext as M.Medidas;

                //Obtengo la columna que debe de ser modificada
                M.DetailColumns coldt = (M.DetailColumns)Enum.ToObject(typeof(M.DetailColumns), columnIndex);               
                
                if (coldt != M.DetailColumns.ExpedienteCatastral)
                {
                    //Valor introducido y suma de la Columna
                    decimal numIntroducido = 0,
                            suma = 0;

                    //Obtengo la propiedad ligada a la columna
                    PropertyInfo prop = typeof(M.Medidas).GetProperty(coldt.ToString());

                    //Obtengo el Textbox de donde fue editado
                    TextBox txt = e.EditingElement as TextBox;

                    string input = string.IsNullOrWhiteSpace(txt.Text) ? "0" : txt.Text;

                    //Reviso que sea realmente un valor númerico
                    if (decimal.TryParse(input, out numIntroducido))
                    {                        
                        //Agrego a la suma lo que edito
                        suma += numIntroducido;

                        //Obtengo los demás valores de la misma columna menos el de si mismo.
                        foreach (M.Medidas mItemMedidasG in M.InfoTabla.MedidasGlobales)
                        {
                            if (mItemMedidasG.LongLote == LongActual && mItemMedidasG.Apartamento != mItemEditado.Apartamento)
                            {
                                object valueMedidas = prop.GetValue(mItemMedidasG);

                                if (valueMedidas != null)
                                {
                                    decimal doubMedidas = decimal.Parse(valueMedidas.ToString());
                                    //Tomo la propiedad de todos los lotes menos el que edité
                                    suma += doubMedidas;
                                }
                            }
                        }

                        //Actualizo los totales que se modifican de manera automática
                        UpdateHorizontalTotals(mItemEditado, coldt, numIntroducido);

                        //Obtengo la propiedad que se modifico
                        PropertyDescriptorCollection propCollection = TypeDescriptor.GetProperties(typeof(M.TotalesMedidas));

                        //Modifico el Total que se requiere
                        foreach (PropertyDescriptor propActual in propCollection)
                        {
                            M.Totales itemTotalActual = (M.Totales)propActual.GetValue(M.InfoTabla.TotalesTabla);

                            if (itemTotalActual != null && itemTotalActual.Columna == coldt)
                            {
                                //Modifico el valor al total correcto
                                propActual.SetValue(M.InfoTabla.TotalesTabla, new M.Totales()
                                {
                                    Columna = itemTotalActual.Columna,
                                    Total = suma
                                });

                                //Modifico el textbox correspondiente 
                                ((TextBlock)(stackTotales.Children[columnIndex] as Border).Child).Text = suma.ToString();

                                break;
                            }
                        }

                        //Si no es Macrolote (Envío los valores a todos los Lotes Regulares)
                        if (!M.Manzana.EsMacrolote && LongActual == M.Colindante.IdTipo.Handle.Value)
                            Met_InfoTabla.DuplicateEditedValues(mItemEditado, numIntroducido, prop);

                    }
                    else
                    {
                        if (txt.Text != "")
                            (e.EditingElement as TextBox).Text = "";
                    }

                }
            }

        }

        private void UpdateHorizontalTotals(M.Medidas mItemEditado, M.DetailColumns colEditada, decimal numIntroducido)
        {
            int enumTotalAreaCub = (int)M.DetailColumns.AreaTotalCubierta,
                enumTotalAreaDesc = (int)M.DetailColumns.AreaTotalDescubierta,
                enumEditado = (int)colEditada;

            decimal  sumaHorizontalAreaCub = 0,
                    sumaHorizontalAreaDesc = 0,
                    sumaHorizontalAmbos = 0;

            decimal  sumaVerticalAreaCub = 0,
                    sumaVerticalAreaDesc = 0,
                    sumaVerticalAmbos = 0,
                    sumaProindiviso = 0;

            //Obtengo la propiedad ligada a la columna
            PropertyInfo propertyEdited = typeof(M.Medidas).GetProperty(colEditada.ToString());

            //Obtengo todas las propiedades
            PropertyInfo[] propertiesCollection = typeof(M.Medidas).GetProperties();

            Dictionary<PropertyInfo, M.DetailColumns> propertiesFiltered = new Dictionary<PropertyInfo, M.DetailColumns>();

            PropertyInfo propertyToModify;

            //Solamente se actualiza si la columna fue modificada
            if (enumEditado < enumTotalAreaDesc)
            {
                foreach (PropertyInfo prop in propertiesCollection)
                {
                    M.DetailColumns colParser;
                    if (Enum.TryParse<M.DetailColumns>(prop.Name, out colParser))
                        propertiesFiltered.Add(prop, colParser);
                }

                //Si la columna modificada es menor al total de Área Cubierta Sólo modifico lo de Área Cubierta
                if (enumEditado < enumTotalAreaCub)
                {
                    foreach (KeyValuePair<PropertyInfo, M.DetailColumns> dicValue in propertiesFiltered)
                    {
                        int enumDCActual = (int)dicValue.Value;

                        //Si la columna es menor a la del Área Cubierta y no es la misma que se editó
                        if (enumDCActual < enumTotalAreaCub && enumDCActual != enumEditado)
                        {
                            object objProp = dicValue.Key.GetValue(mItemEditado);

                            if (objProp != null)
                            {
                                decimal numValue = 0;

                                if (decimal.TryParse(objProp.ToString(), out numValue))
                                    sumaHorizontalAreaCub += numValue;
                            }
                        }
                        else if (enumDCActual < enumTotalAreaCub && enumDCActual == enumEditado)
                        {
                            sumaHorizontalAreaCub += numIntroducido;
                        }
                    }

                    //Cambio el valor de el Total de Área Cubierta                    
                    propertyToModify = typeof(M.Medidas).GetProperty(M.DetailColumns.AreaTotalCubierta.ToString());

                    if (propertyToModify != null)
                        propertyToModify.SetValue(mItemEditado, sumaHorizontalAreaCub.ToString());


                }
                else //Si la columna modificada es mayor significa que hay que editar el Área Descubierta
                {
                    foreach (KeyValuePair<PropertyInfo, M.DetailColumns> dicItem in propertiesFiltered)
                    {
                        int enumDCActual = (int)dicItem.Value;

                        //Si la columna es menor a la del Área Cubierta y no es la misma que se editó
                        if (enumDCActual > enumTotalAreaCub && enumDCActual < enumTotalAreaDesc && enumDCActual != enumEditado)
                        {
                            object objProp = dicItem.Key.GetValue(mItemEditado);

                            if (objProp != null)
                            {
                                decimal numValue = 0;

                                if (decimal.TryParse(objProp.ToString(), out numValue))
                                    sumaHorizontalAreaDesc += numValue;
                            }
                        }
                        else if (enumDCActual > enumTotalAreaCub &&  enumDCActual < enumTotalAreaDesc && enumDCActual == enumEditado) {
                            sumaHorizontalAreaDesc += numIntroducido;
                        }
                    }

                    //Cambio el valor de el Total de Área Descubierta                    
                    propertyToModify = typeof(M.Medidas).GetProperty(M.DetailColumns.AreaTotalDescubierta.ToString());

                    if (propertyToModify != null)
                    {
                        propertyToModify.SetValue(mItemEditado, sumaHorizontalAreaDesc.ToString());
                    }

                }



                //Si la modificación no se encontró en el Área Cubierta la obtengo desde el Item
                if (sumaHorizontalAreaCub == 0)
                    sumaHorizontalAreaCub = mItemEditado.AreaTotalCubierta != null ? decimal.Parse(mItemEditado.AreaTotalCubierta) : 0;

                //Si la modificación no se encontró en el Área Descubierta la obtengo desde el Item
                if (sumaHorizontalAreaDesc == 0)
                    sumaHorizontalAreaDesc = mItemEditado.AreaTotalDescubierta != null ? decimal.Parse(mItemEditado.AreaTotalDescubierta) : 0;

                //Sumo ambos y cambio la propiedad Área Total Descubierta + Total Cubierta       
                propertyToModify = typeof(M.Medidas).GetProperty(M.DetailColumns.AreaCubiertaDescubierta.ToString());

                sumaHorizontalAmbos = sumaHorizontalAreaCub + sumaHorizontalAreaDesc;

                if (propertyToModify != null)
                    propertyToModify.SetValue(mItemEditado, sumaHorizontalAmbos.ToString());

                foreach (M.Medidas mMedidasItem in M.InfoTabla.MedidasGlobales)
                {
                    if (mMedidasItem.LongLote == mItemEditado.LongLote && mMedidasItem.Apartamento != mItemEditado.Apartamento)
                    {
                        sumaVerticalAreaCub += decimal.Parse((mMedidasItem.AreaTotalCubierta ?? "0"));
                        sumaVerticalAreaDesc += decimal.Parse((mMedidasItem.AreaTotalDescubierta ?? "0"));
                        sumaVerticalAmbos += decimal.Parse((mMedidasItem.AreaCubiertaDescubierta ?? "0"));
                    }
                }

                sumaVerticalAreaCub += sumaHorizontalAreaCub;
                sumaVerticalAreaDesc += sumaHorizontalAreaDesc;
                sumaVerticalAmbos += sumaHorizontalAmbos;                                

                foreach (PropertyInfo propTotal in typeof(M.TotalesMedidas).GetProperties())
                {
                    M.Totales totalActual = (M.Totales) propTotal.GetValue(M.InfoTabla.TotalesTabla);

                    if(totalActual != null)
                    {
                        if (totalActual.Columna == M.DetailColumns.AreaTotalCubierta)
                            propTotal.SetValue(M.InfoTabla.TotalesTabla, new M.Totales() {
                                Total = sumaVerticalAreaCub,
                                Columna = totalActual.Columna
                            } );
                        else if(totalActual.Columna == M.DetailColumns.AreaTotalDescubierta)
                            propTotal.SetValue(M.InfoTabla.TotalesTabla, new M.Totales()
                            {
                                Total = sumaVerticalAreaDesc,
                                Columna = totalActual.Columna
                            });
                        else if(totalActual.Columna == M.DetailColumns.AreaCubiertaDescubierta)
                            propTotal.SetValue(M.InfoTabla.TotalesTabla, new M.Totales()
                            {
                                Total = sumaVerticalAmbos,
                                Columna = totalActual.Columna
                            });
                    }
                }

                foreach (M.Medidas mMedidasItem in M.InfoTabla.MedidasGlobales)
                {
                    if (mMedidasItem.LongLote == mItemEditado.LongLote)
                    {
                        decimal sumaCubDesc = decimal.Parse(mMedidasItem.AreaCubiertaDescubierta ?? "0"),
                                proindiviso = 0;

                        int decimalsCubDesc = BitConverter.GetBytes(decimal.GetBits(sumaCubDesc)[3])[2];

                        if (sumaVerticalAmbos > 0)
                        {
                            proindiviso = ((sumaCubDesc * 100) / sumaVerticalAmbos);

                            proindiviso = decimal.Parse((double.Parse(proindiviso.ToString()).Trunc(decimalsCubDesc)).ToString());

                            sumaProindiviso += proindiviso;

                            mMedidasItem.Proindiviso = proindiviso.ToString();
                        }
                    }

                }

                //Modifico el textbox correspondiente de Área Cubierta
                ((TextBlock)(stackTotales.Children[enumTotalAreaCub] as Border).Child).Text = sumaVerticalAreaCub.ToString();
                ((TextBlock)(stackTotales.Children[enumTotalAreaDesc] as Border).Child).Text = sumaVerticalAreaDesc.ToString();
                ((TextBlock)(stackTotales.Children[(int)M.DetailColumns.AreaCubiertaDescubierta] as Border).Child).Text = sumaVerticalAmbos.ToString();
                txtTotalProindiviso.Text = sumaProindiviso.ToString();
            }
        }

        #endregion

        #region ToolBar

        private void btnExportarAutocad_Click(object sender, RoutedEventArgs e)
        {
            if (dtDetalle.ItemsSource != null && dtDetalle.Items.Count > 0)
            {
                //Convierto la vista a DataTable
                string[,] dtMedidas = M.InfoTabla.MedidasGlobales.ToStringArray(LongActual);                

                this.WindowState = WindowState.Minimized;

                if (Met_Autodesk.ToAutodeskTable(dtMedidas))
                {
                    //Si creo la table lo despliego en pantalla
                    MessageBox.Show("Se creo la tabla de manera correcta");
                }
                else {
                    MessageBox.Show("No se creo la tabla");
                }

                this.WindowState = WindowState.Normal;
            }

        }

        private void btnExportarExcel_Click(object sender, RoutedEventArgs e)
        {
            if (dtDetalle.ItemsSource != null)
            {
                if (LongActual != -1)
                {
                    FormWindow.FolderBrowserDialog fd = new FormWindow.FolderBrowserDialog();

                    if (fd.ShowDialog() == FormWindow.DialogResult.OK)
                    {
                        string[,] multarray = M.InfoTabla.MedidasGlobales.ToStringArray(LongActual);

                        System.Data.DataTable dTable = multarray.ToDataTable();

                        dTable.ToExcel(fd.SelectedPath);
                    }
                }
                else
                    MessageBox.Show("No se ha seleccionado un lote ");
            }
        }


        private void btnExportarDocActual_Click(object sender, RoutedEventArgs e)
        {
            if (IsAllCaptured(new List<long>() { LongActual }))
            {
                FormWindow.FolderBrowserDialog fd = new FormWindow.FolderBrowserDialog();

                if (fd.ShowDialog() == FormWindow.DialogResult.OK)
                {
                    lotesDocs = new List<long>() { LongActual };

                    filePath = fd.SelectedPath;

                    toolBProgress.Visibility = Visibility.Visible;

                    CreateDocuments();
                }
            }
        }

        private void btnExportarTodosDoc_Click(object sender, RoutedEventArgs e)
        {
            List<long> todosLotes = new List<long>();

            if (M.Manzana.EsMacrolote)
                todosLotes = new List<long>() { M.Colindante.IdMacrolote.Handle.Value };
            else
                todosLotes = M.Colindante.Lotes.Select(x => x._long).ToList();

            if (IsAllCaptured(todosLotes))
            {
                FormWindow.FolderBrowserDialog fd = new FormWindow.FolderBrowserDialog();

                if (fd.ShowDialog() == FormWindow.DialogResult.OK)
                {
                    lotesDocs = todosLotes;

                    filePath = fd.SelectedPath;

                    toolBProgress.Visibility = Visibility.Visible;

                    btnExportarDocActual.IsEnabled = false;
                    btnExportarTodosDoc.IsEnabled = false;

                    CreateDocuments();
                }
            }
        }

        private void CreateDocuments()
        {
            bkWorker = new BackgroundWorker();

            bkWorker.DoWork += new DoWorkEventHandler(BGWorker_DoWork);

            if (!bkWorker.IsBusy)
            {
                bkWorker.RunWorkerAsync();
            }

            bkWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw_RunWorkerCompleted);
            bkWorker.ProgressChanged += new ProgressChangedEventHandler(bw_ProgressChanged);
        }

        private void bw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            toolBProgress.Visibility = Visibility.Hidden;

            btnExportarDocActual.IsEnabled = true;
            btnExportarTodosDoc.IsEnabled = true;

            if (e.Result != null)
            {
                bool isCreated = (bool)e.Result;

                if(isCreated)
                {
                    MessageBox.Show("Se crearon los documentos de manera correcta");
                }
            }
        }

        private void BGWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            if (Met_InfoTabla.ObtenerInfoMachotes(lotesDocs, filePath))
            {
                e.Result = Met_InfoTabla.CrearMachotes(lotesDocs, M.InfoTabla.ResultadoBloques, M.InfoTabla.ResultadoVariables,
                                        filePath);
            }
        }

        private bool IsAllCaptured(List<long> Lotes)
        {
            int cont = 0;
            string msj, titulo;

            foreach (long mLote in Lotes)
            {
                if (Met_InfoTabla.HasEmptyFields(mLote, out msj, out titulo))
                {                    
                    this.ShowMessageAsync(titulo, msj);
                    break;
                }
                else
                    cont++;
            }            

            return cont == Lotes.Count;
        }

        #endregion

        private void dtDetalle_Selected(object sender, RoutedEventArgs e)
        {
            //MouseEventArgs me = (MouseEventArgs)e;

            // Lookup for the source to be DataGridCell
            if (e.OriginalSource.GetType() == typeof(DataGridCell))
            {
                // Starts the Edit on the row;
                DataGrid grd = (DataGrid)sender;
                grd.BeginEdit(e);
            }
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
                {
                    C.Met_General.ClearData();
                    M.Inicio.IsOpen = false;
                }
            }
            else
                M.Constant.IsAutoClose = false;
        }

        private void dtDetalle_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dtHeader.Items.Count > 0)
            {
                DataGrid dt = sender as DataGrid;

                dtHeader.SelectedIndex = dt.SelectedIndex;
            }
        }

        private void btnAtras_Click(object sender, RoutedEventArgs e)
        {
            ModuloColindante mM = new ModuloColindante();
            Autodesk.AutoCAD.ApplicationServices.Core.Application.ShowModelessWindow(mM);
            M.Constant.IsAutoClose = true;
            this.Close();
        }        

        private void btnRevisarLoteActual_Click(object sender, RoutedEventArgs e)
        {
            List<long> lotes = M.Manzana.EsMacrolote ? new List<long>() { M.Colindante.IdMacrolote.Handle.Value } :
                                                       M.Colindante.Lotes.Select(x => x._long).ToList();

            if (IsAllCaptured(lotes))
                MessageBox.Show("Lote Correcto");

        }

        private void dtDetalle_MouseMove(object sender, MouseEventArgs e)
        {            
            ColumnOver = GetColumn(dtDetalle, e.GetPosition(dtDetalle));

            if (ColumnOver >= 0 && ColumnOver <= (int)M.DetailColumns.ExpedienteCatastral)
                MenuItemCol.Header = "Pegar en Columna " + ((TextBlock)dtDetalle.Columns[ColumnOver].Header).Text;
            
        }
        private int GetColumn(DataGrid dt, Point position)
        {
            int columnIndex = 0;
            double total = 0;
            
            foreach (DataGridColumn clm in dt.Columns)
            {
                double widthCol = clm.ActualWidth;

                //Lo sumo
                if (clm.Visibility == Visibility.Visible)
                    total += widthCol;

                if (total > position.X)
                {
                    //Si esta visible si esta correcto
                    if (clm.Visibility == Visibility.Visible)
                        break;
                    else //Si no es visible busco el siguiente que sea visible
                    {                                       
                        for(int colIndexCalc = (columnIndex+1); colIndexCalc < dt.Columns.Count; colIndexCalc++)
                        {
                            if (dt.Columns[colIndexCalc].Visibility == Visibility.Visible)
                                return colIndexCalc;                    
                        }

                        return (dt.Columns.Count - 1);
                    }

                }
                   
                columnIndex++;
            }

            return columnIndex;
            //row = -1; total = 0; foreach (RowDefinition rowDef in @this.RowDefinitions) { if (position.Y < total) { break; } row++; total += rowDef.ActualHeight; }
        }

        private async void btnEnviar_Click(object sender, RoutedEventArgs e)
        {

            List<long> lotes = M.Manzana.EsMacrolote ? new List<long>() { M.Colindante.IdMacrolote.Handle.Value } :
                                                       M.Colindante.Lotes.Select(x => x._long).ToList();

            if (IsAllCaptured(lotes))
            {
                if (M.InfoTabla.BloquesCalculados.Count > 0)
                {
                    MessageDialogResult dg = await this.ShowMessageAsync("Enviar Datos",
                        "¿Desea enviar los Datos Calculados? \n Se reescribirá reporte de " + M.Inicio.EncMachote.Encabezado
                        , MessageDialogStyle.AffirmativeAndNegative, M.Constant.DialogMetroSettings);

                    if (dg == MessageDialogResult.Affirmative)
                    {
                        btnEnviarBD.IsEnabled = false;
                        toolBProgress.Visibility = Visibility.Visible;
                        //Obtengo Fraccionamientos ejecutando consulta a BD
                        new C.SqlTransaction(null, EnviarBD, DatosEnviadosDB).Run();
                    }
                }
                else
                {
                    await this.ShowMessageAsync("Exportar a Word",
                        "Favor de exportar documento(s) de Régimen de Condominio primero");
                }
            }
                        
        }

        private void DatosEnviadosDB(object input)
        {
            btnEnviarBD.IsEnabled = true;
            toolBProgress.Visibility = Visibility.Hidden;

            if (input != null)
            {
                bool isCreated = (bool)input;

                if(isCreated)
                {
                    MessageBox.Show("Se guardaron los datos de manera correcta");
                    exported = true;
                }else
                {
                    MessageBox.Show("Error al Enviar a la Base de Datos,\n Revisar el Editor Autodesk");
                }
            }
            else
            {
                MessageBox.Show("Error al Enviar a la Base de Datos,\n Revisar el Editor Autodesk");
            }
        }

        private object EnviarBD(SQL_Connector conn, object input, BackgroundWorker bg)
        {
            bool    isDeleted = false,
                    isCreated = false;

            string queryDelete = "";

            Dictionary<string, object> paramsInsert = new Dictionary<string, object>();

            try
            {

                queryDelete = string.Format(Config.DB.QueryLimpiaRegistros, M.Inicio.EncMachote.IdMachote);

                isDeleted = conn.Run(queryDelete);

                if (isDeleted)
                {                    
                    isCreated = conn.Run(Config.DB.QueryCargaMachote, M.InfoTabla.BloquesCalculados.OrderBy(x=> x.Orden).ToList());
                }
                else
                    isCreated = false;

            }
            catch (Exception ex)
            {
                ex.Message.ToEditor();
                isCreated = false;
            }

            return isCreated;
        }

        private async void irInicio_Click(object sender, RoutedEventArgs e)
        {
            MessageDialogResult dg = await this.ShowMessageAsync("Ir a Inicio"
                                , "Se perderán los datos, ¿Desea ir a Inicio?"
                                , MessageDialogStyle.AffirmativeAndNegative, M.Constant.DialogMetroSettings);

            if (dg == MessageDialogResult.Affirmative)
            {
                C.Met_General.ClearData(false);
                ModuloInicial mi = new ModuloInicial();
                mi.Show();
                M.Constant.IsAutoClose = true;
                this.Close();
            }
        }

        private void btnLimpiar_Click(object sender, RoutedEventArgs e)
        {
            if (dtDetalle.Items.Count > 0 && M.InfoTabla.MedidasGlobales.Count > 0)
            {
                foreach (M.Medidas mMedida in M.InfoTabla.MedidasGlobales)
                {
                    if (mMedida.LongLote == LongActual)
                    {
                        string cero = "0";
                        //Planta Baja Cubierta
                        mMedida.CPlantaBaja = cero;                                                
                        mMedida.CPlantaAlta = cero;
                        mMedida.CLavanderia = cero;
                        mMedida.CEstacionamiento = cero;
                        mMedida.CPasillo = cero;
                        mMedida.CPatio = cero;
                        mMedida.AreaTotalCubierta = cero;
                        mMedida.CLavanderia = cero;
                        mMedida.DLavanderia = cero;
                        mMedida.DEstacionamiento = cero;
                        mMedida.DPasillo = cero;
                        mMedida.DPatio = cero;
                        mMedida.AreaTotalDescubierta = cero;
                        mMedida.AreaCubiertaDescubierta = cero;
                        mMedida.NombreAreaComun = cero;
                        mMedida.AreaExclusiva = cero;
                        mMedida.Proindiviso = cero;
                        mMedida.PredioFrente = cero;
                        mMedida.PredioFondo = cero;
                        mMedida.PredioArea = cero;
                        mMedida.AreaConstruccion = cero;
                        mMedida.ExpedienteCatastral = M.Inicio.Region + "-" + M.Manzana.NoManzana + "-";


                        
                    }
                }
              
                foreach (PropertyInfo prop in typeof(M.TotalesMedidas).GetProperties())
                {
                    M.Totales totalActual = (M.Totales)prop.GetValue(M.InfoTabla.TotalesTabla);

                    prop.SetValue(M.InfoTabla.TotalesTabla, new M.Totales()
                    {
                        Columna = totalActual.Columna,
                        Total = 0
                    });
                }
            }            
        }

        private async void btnFinalizar_Click(object sender, RoutedEventArgs e)
        {
            MetroDialogSettings mDialogSet = new MetroDialogSettings();

            //mDialogSet.ColorScheme = MetroDialogColorScheme.Accented;
            mDialogSet.DefaultButtonFocus = MessageDialogResult.Negative;            
            mDialogSet.AffirmativeButtonText = "Sí, estoy de acuerdo";
            mDialogSet.NegativeButtonText = "Cancelar";

            if (M.InfoTabla.BloquesCalculados.Count > 0)
            {
                if(exported)
                {
                    MessageDialogResult dg = await this.ShowMessageAsync("Eliminar Datos",
                        "Se eliminarán los datos del programa, plano y se cerrará el programa\n ¿Esta de acuerdo?"
                        , MessageDialogStyle.AffirmativeAndNegative, mDialogSet);

                    if (dg == MessageDialogResult.Affirmative)
                    {
                        M.Constant.IsAutoClose = true;
                        this.Close();                        
                        C.Met_General.ClearData();                        
                    }
                }
                else
                {
                    await this.ShowMessageAsync("Enviar a BD",
                    "Favor presionar \"Enviar\" para cargarlo a la Base de datos");
                }
            }
            else
            {
                await this.ShowMessageAsync("Exportar a Word",
                    "Favor de exportar documento(s) de Régimen de Condominio primero");
            }
        }
    }
}
