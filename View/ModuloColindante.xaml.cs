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
using RegimenCondominio.C;
using MahApps.Metro.Controls.Dialogs;
using System.ComponentModel;
using Autodesk.AutoCAD.Geometry;
using System.Windows.Controls.Primitives;
using System.Diagnostics;

namespace RegimenCondominio.V
{
    /// <summary>
    /// Interaction logic for ModuloColindante.xaml
    /// </summary>
    public partial class ModuloColindante : MetroWindow
    {
        
        public ModuloColindante()
        {
            InitializeComponent();
        }

        private void btnAtras_Click(object sender, RoutedEventArgs e)
        {
            ModuloManzana mM = new ModuloManzana();
            Autodesk.AutoCAD.ApplicationServices.Core.Application.ShowModelessWindow(mM);
            this.Close();
        }

        private void btnAvanzar_Click(object sender, RoutedEventArgs e)
        {
            ModuloInfoTabla mI = new ModuloInfoTabla();
            Autodesk.AutoCAD.ApplicationServices.Core.Application.ShowModelessWindow(mI);
            this.Close();
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {           
            //Comienzo transacción minimizando
            this.WindowState = WindowState.Minimized;

            //Crea Layers en dado caso que no exista
            string.Format("Creó {0} Layers \n", Met_Colindante.CreateAdjacencyLayers()).ToEditor();

            //Encuentro segmentos 
            ValidateEntityNumbers();                                  

            #region Si es Macrolote
            //Solamente cuando sea Macrolote leo los Macrolotes--------------------------------------
            if (M.Manzana.EsMacrolote)                                                         
                AdaptView();
            //--------------------------------------------------------------------          
            #endregion

            BindingOperations.EnableCollectionSynchronization(M.Colindante.ListadoErrores, M.Colindante.Lock);

            this.WindowState = WindowState.Normal;

            FillControlsData();            
        }

        private void ValidateEntityNumbers()
        {
            //Inicializo Variables--------------------------------------

            //Deshabilito los pasos nuevamente
            tb1Paso0.IsEnabled = false;
            tb1Paso1.IsEnabled = false;
            tb1Paso2.IsEnabled = false;
            tb1Paso3.IsEnabled = false;

            //Envío a NULL itemsource
            tb2GridLotes.ItemsSource = null;

            //Limpio lista para reasignarla
            M.Colindante.ValorLotes.Clear();

            //Listado de segmentos
            List<ObjectId> listSegments = new List<ObjectId>();

            //Obtengo los lotes dentro de los segmentos
            ObjectIdCollection idsLotes = new ObjectIdCollection();

            string LoteOMacroLote = "";//Complementa lbl de Lotes                    

            int countLote = 0, //Reviso cuantos Número de Lotes encontré
                countNumOficial = 0; //Reviso cuantos Números Oficiales encontré
            //----------------------------------------------------------   

            //Leo los segmentos seleccionados
            listSegments = M.Manzana.ColindanciaManzana.Select(x => x.HndPlColindancia.toObjectId()).ToList();

            //Obtengo los Lotes / Macrolotes creando Polilínea
            idsLotes = Met_Autodesk.GetPolylinesInSegments(listSegments, M.Constant.LayerLote);

            //Por cada lote encontrado en los segmentos
            foreach (ObjectId idLote in idsLotes)
            {
                //Punto minimo y máximo de la polílinea
                Point3d ptmin = new Point3d(),
                        ptmax = new Point3d();

                DBText idTextLote = new DBText(),
                       idTextNumOficial = new DBText();

                string valorLote = "",
                        valorNumOficial = "";

                //Abro entidad de PL                
                Autodesk.AutoCAD.DatabaseServices.Polyline lote
                    = (idLote.OpenEntity() as Autodesk.AutoCAD.DatabaseServices.Polyline);

                //Calculo punto mínimo y máximo
                ptmin = lote.GeometricExtents.MinPoint;
                ptmax = lote.GeometricExtents.MaxPoint;

                //Enfoco
                lote.Focus(20, 40);

                //Detecto Texto con LayerLote
                ObjectIdCollection idsTextoLotes = Met_Autodesk.ObjectsInside(ptmin, ptmax,
                                                                    typeof(DBText).Filter(M.Constant.LayerLote));

                //Detecto Texto con Número Oficial
                ObjectIdCollection idsNoOficial = Met_Autodesk.ObjectsInside(ptmin, ptmax,
                                                                    typeof(DBText).Filter(M.Constant.LayerNoOficial));

                //Reviso que sólo haya 1 para asignarlo
                //***NÚMERO LOTE---------------------------------------------------------------------------
                if (idsTextoLotes.Count == 1)
                {
                    idTextLote = idsTextoLotes.OfType<ObjectId>().FirstOrDefault().OpenEntity() as DBText;

                    valorLote = idTextLote.TextString.GetAfterSpace();

                    countLote++;
                }
                else if (idsTextoLotes.Count > 1) //Si encontro más de uno imprimo cuantos encontró
                {
                    valorLote = idsTextoLotes.Count + " Encontrados";
                }
                //***-----------------------------------------------------------------------------------------

                //***NÚMERO OFICIAL---------------------------------------------------------------------------
                if (idsNoOficial.Count == 1)
                {
                    idTextNumOficial = idsNoOficial.OfType<ObjectId>().FirstOrDefault().OpenEntity() as DBText;

                    valorNumOficial = idTextNumOficial.TextString.GetAfterSpace();

                    countNumOficial++;
                }
                else if (idsNoOficial.Count > 1)
                {
                    valorNumOficial = idsTextoLotes.Count + " Encontrados";
                }
                //***-----------------------------------------------------------------------------------------

                //Agrego Valor de Lotes
                M.Colindante.ValorLotes.Add(new M.InLotes()
                {
                    handleEntity = idLote.Handle.Value,
                    numOficial = valorNumOficial,
                    numLote = valorLote
                });
            }

            //Valido si tiene todos los elementos necesarios
            if (countLote == idsLotes.Count && countNumOficial == idsLotes.Count)
            {
                if (M.Manzana.EsMacrolote)
                {
                    //Activo Paso 0
                    tb1Paso0.IsEnabled = true;
                }
                else
                {
                    //Activo Paso 1
                    tb1Paso1.IsEnabled = true;
                }
            }
            else
            {
                this.ShowMessageAsync("Error de Detección en Lotes",
                           "Hay elementos sin o con más de un Lote y/o Número Oficial \n Ir a Detalles");
            }

            //Dependiendo de cuantos lotes encontró
            if (idsLotes.Count == 1)
                LoteOMacroLote = M.Manzana.EsMacrolote ? "Macrolote" : "Lote";
            else
                LoteOMacroLote = M.Manzana.EsMacrolote ? "Macrolotes" : "Lotes";

            lblGnLotes.Text = idsLotes.Count.ToString() + " " + LoteOMacroLote;

            tb2GridLotes.ItemsSource = M.Colindante.ValorLotes;
        }

        private void FillControlsData()
        {
            tb1cmbDecimales.Items.Add("1");
            tb1cmbDecimales.Items.Add("2");
            tb1cmbDecimales.Items.Add("3");

            //Datos Iniciales
            tb2txtFracc.Text = M.Inicio.Fraccionamiento;
            tb2txtEstado.Text = M.Inicio.Estado;
            tb2txtMunicipio.Text = M.Inicio.Municipio;
            tb2txtSector.Text = M.Inicio.Sector;
            tb2txtRegion.Text = M.Inicio.Region;
            tb2txtTipoVivienda.Text = M.Inicio.TipoViv;

            ListPrincipal.ItemsSource = M.Manzana.ColindanciaManzana;
            tb2GridErrores.ItemsSource = M.Colindante.ListadoErrores;                      
        }

        private void AdaptView()
        {
            tab1.Header = "Macrolote";

            //PASOS
            txtHeaderPaso1.Text = "1. Selecciona Macrolote";
            txtHeaderPaso2.Text = "2. Sel. Primer Edificio Tipo";
            txtHeaderPaso3.Text = "3. Sel. Edificios Irregulares";
            txtHeaderPaso4.Text = "4. Calcular Puntos";

            //Cambio el color
            txtHeaderPaso1.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#00897B"));            

            //Posición de Datos Generales                
            lblGnLotes.VerticalAlignment = VerticalAlignment.Bottom;
            lblGnLotes.FontSize = 13;
            //lblLoteEdificio.Margin = new Thickness(3, 23, 3, 0);                

            //Macrolotes
            lblGnEdificios.Visibility = System.Windows.Visibility.Visible;
        }


        #region Pasos

        //PASO 0
        private void tb1SelMacrolote_Click(object sender, RoutedEventArgs e)
        {
            ObjectId idPl = new ObjectId();

            this.WindowState = WindowState.Minimized;

            //Valido que se haya obtenido de manera correcta una Polilínea
            if (Met_Autodesk.Entity("Selecciona Macrolote: ", out idPl,
                typeof(Autodesk.AutoCAD.DatabaseServices.Polyline)))
            {
                //Reviso que la polilínea este dentro del mismo layer
                if (idPl.OpenEntity().Layer == M.Constant.LayerLote)
                {
                    //Reviso que este dentro de los Lotes Leídos al Inicio
                    if (M.Colindante.ValorLotes.OfType<M.InLotes>()
                        .Select(x => new Handle(x.handleEntity).toObjectId()).Contains(idPl))
                    {
                        //Busco Edificios y Apartamentos dentro de Macrolote
                        if (!Met_Colindante.noEstaEnEdificios(idPl))
                        {
                            //Dependiendo de cuantos edificios encuentre los asigno en lbl
                            if (M.Colindante.ValorEdificio.Count > 0)
                                lblGnEdificios.Text = string.Format("{0} Edificios", M.Colindante.ValorEdificio.Count);
                            else
                                lblGnEdificios.Text = "0 Edificios";

                            //Asigno Número de Lote
                            lblMacrolote.Text = M.Colindante.ValorLotes.Where(x => new Handle(x.handleEntity).toObjectId() == idPl)
                                .FirstOrDefault().numLote;

                            Met_Colindante.GetCommonArea(idPl);

                            //Muestro Número de Lote en el paso
                            lblMacrolote.Visibility = System.Windows.Visibility.Visible;

                            //Asigno ID Macrolote encontrado
                            M.Colindante.IdMacrolote = idPl;

                            //Muestro siguiente paso
                            tb1Paso1.IsEnabled = true;
                        }
                        else
                        {
                            this.ShowMessageAsync("Selección de Edificios",
                            "Hay un problema dentro del Macrolote, ir a Detalle...");

                            tb2GridErrores.Items.Refresh();
                        }

                    }
                    else
                        this.ShowMessageAsync("Error de selección",
                            "El lote seleccionado esta fuera de la Manzana");
                }
                else
                    this.ShowMessageAsync("Error de selección",
                           string.Format("El lote no tiene el layer {0}", M.Constant.LayerLote));
                
            }
            else
            {
                M.Colindante.ListadoErrores.Add(new M.DescribeError()
                {
                    Error = "Id no Valida",
                    Description = "No se seleccionó Polilínea o se canceló",
                    timeError = DateTime.Now,
                    tipoError = M.TipoError.Warning,
                    longObject = 0,
                    Metodo = "tb1SelMacrolote_Click"
                });

                tb2GridErrores.ItemsSource = null;
                tb2GridErrores.ItemsSource = M.Colindante.ListadoErrores;
            }

            this.WindowState = WindowState.Normal;
        }

        //PASO 1
        private void tb1SelLoteTipo_Click(object sender, RoutedEventArgs e)
        {
            //Nuevo ID
            ObjectId idPl = new ObjectId();

            //Mensaje a Mostrar
            string msg = M.Manzana.EsMacrolote ? "Selecciona Edificio Tipo" : "Selecciona Lote Tipo";            

            this.WindowState = WindowState.Minimized;

            //Seleccionar Entity con Polilínea Válida
            if (Met_Autodesk.Entity(msg, out idPl, typeof(Autodesk.AutoCAD.DatabaseServices.Polyline)))
            {
                //Reviso que este dentro del layer correcto, depende de si es Macrolote o lote
                if (idPl.OpenEntity().Layer == M.Colindante.LayerTipo)
                {
                    //Lista dependiendo de si es Edificio o Lote
                    List<long> listaTipo = M.Manzana.EsMacrolote ? M.Colindante.ValorEdificio.Select(x=> x.longEntity).ToList() : 
                                                                   M.Colindante.ValorLotes.Select(x => x.handleEntity).ToList();

                    //Reviso que este dentro de los edificios leídos en el plano
                    if (listaTipo.OfType<long>().Select(x=> new Handle(x).toObjectId())
                        .Contains(idPl))                        
                    {
                        M.Colindante.IdTipo = idPl;

                        long l = listaTipo.Where(x => new Handle(x).toObjectId() == idPl)
                            .FirstOrDefault();

                        string VivTipo = "";

                        if (M.Manzana.EsMacrolote)
                            VivTipo = M.Colindante.ValorEdificio.Where(x => x.longEntity == l)
                                .Select(x => x.numEdificio).FirstOrDefault();
                        else
                            VivTipo = M.Colindante.ValorLotes.Where(x => x.handleEntity == l)
                                .Select(x => x.numLote).FirstOrDefault();

                        lblLoteTipo.Text = VivTipo;

                        //lblLoteTipo.Text = 
                        lblLoteTipo.Visibility = System.Windows.Visibility.Visible;
                        tb1Paso2.IsEnabled = true;
                        tb1Paso3.IsEnabled = true;                       
                    }
                    //Cuando el layer de Polilínea no esta dentro del mismo layer
                    else
                    {                        
                        this.ShowMessageAsync("Error de selección",
                            "El lote seleccionado esta fuera de la Manzana y/o tiene distinto layer");
                    }
                }
                else
                {
                    this.ShowMessageAsync("Error de selección",
                           string.Format("El lote no esta dentró del layer {0}", M.Colindante.LayerTipo));

                    M.Colindante.ListadoErrores.Add(new M.DescribeError()
                    {
                        Error = "Layer Inválida",
                        Description = string.Format("El objeto debe de tener Layer {0}", M.Colindante.LayerTipo),
                        timeError = DateTime.Now
                    });

                    tb2GridErrores.Items.Refresh();
                }
            }
            else
            {
                M.Colindante.ListadoErrores.Add(new M.DescribeError()
                {
                    Error = "Id no Valida",
                    Description = "Id de Polilínea no Válido",
                    timeError = DateTime.Now,
                    tipoError = M.TipoError.Warning,
                    longObject = 0,
                    Metodo = "tb1Sel_Tipo"
                });

            }
            this.WindowState = WindowState.Normal;
        }

        //Paso 2
        private void tb1SelMultiple_Click(object sender, RoutedEventArgs e)
        {
            //Ids seleccionados
            ObjectIdCollection idsSelected = new ObjectIdCollection();

            //Mensaje a Mostrar
            string msg = M.Manzana.EsMacrolote ? "Selecciona los Edificios irregulares" : "Selecciona los lotes irregulares";

            this.WindowState = WindowState.Minimized;

            //Selecciona Polilíneas
            if (Met_Autodesk.SelectPolylines(out idsSelected, msg, M.Colindante.LayerTipo))
            {               
                List<long> listTipo = M.Manzana.EsMacrolote ?  M.Colindante.ValorEdificio.Select(x=> x.longEntity).ToList() : 
                                                                    M.Colindante.ValorLotes.Select(x => x.handleEntity).ToList();

                //Obtengo todos los ids en el Lote
                List<ObjectId> allIdsInLote = listTipo.Select(x => new Handle(x).toObjectId()).ToList();

                //Todos los ids fuera del lote
                List<ObjectId> idsAreOut = idsSelected.OfType<ObjectId>().Where(x => !allIdsInLote.Contains(x)).ToList();

                //Si ninguno esta afuera de la manzana
                if (idsAreOut.Count == 0)
                {
                    M.Colindante.IdsIrregulares = idsSelected;
                    tb1Paso3.IsEnabled = true;
                    tb1CheckLoteIrregular.IsChecked = true;
                }
                else
                {
                    string _in = M.Manzana.EsMacrolote ? "del Macrolote" : "de la Manzana"; 
                    string msgLote = idsAreOut.Count == 1 ? "Lote seleccionado esta fuera" + _in:
                                                            "Lotes seleccionados están fuera" + _in;

                    for(int i = 0; i < idsAreOut.Count;i++)
                    {
                        ObjectId id = idsAreOut[i];
                        M.Colindante.ListadoErrores.Add(new M.DescribeError()
                        {
                            Error = "Id Inválido",
                            Description = "El id seleccionado esta afuera " + _in,
                            timeError = DateTime.Now,
                            longObject = id.Handle.Value,
                            Metodo = "tb1SelMultiple_Click",
                            tipoError = M.TipoError.Error
                        });

                    }

                    this.ShowMessageAsync("Error de selección", msgLote);
                }
            }
            else
            {
                M.Colindante.ListadoErrores.Add(new M.DescribeError()
                {
                    Error = "Ids Invalidos",
                    Description = "Error al seleccionar Polilíneas en Paso 3",
                    timeError = DateTime.Now
                });
            }

            this.WindowState = WindowState.Normal;
        }

        //Paso 3
        private void tb1CalPuntos_Click(object sender, RoutedEventArgs e)
        {
            int cont = 0;

            tb1CalPuntos.IsEnabled = false;
            tb1GridColindancia.ItemsSource = null;
            M.Colindante.MainData.Clear();

            this.WindowState = WindowState.Minimized;

            var s1 = Stopwatch.StartNew();

            #region Si Es Macrolote                                                
            if (M.Manzana.EsMacrolote)
            {
                M.Colindante.LastPoint = 0;

                if(Met_Colindante.CreatePointsMacroset(M.Colindante.IdTipo, "Edificio Tipo"))
                {
                    cont++;
                    bool siGeneroTodo = true;

                    foreach (ObjectId idIrregular in M.Colindante.IdsIrregulares)
                    {
                       
                        cont++;
                        if (!Met_Colindante.CreatePointsMacroset(idIrregular, "Edificio Irregular"))
                        {
                            RollBackPoints();
                            siGeneroTodo = false;
                            this.ShowMessageAsync("Error al Crear Puntos",
                                    "No se crearon los puntos de manera correcta");
                            break;
                        }
                    }

                    if(siGeneroTodo)
                    {
                        if(Met_Colindante.GenerateCornerPoints())
                        {
                            //Genero Descripción de Área Común
                            if(Met_Colindante.GenerateMacroCommonArea())
                            {
                                //Busco en Lotes que NO son Regulares e Irregulares

                                if (tb2GridErrores.Items.NeedsRefresh)
                                    tb2GridErrores.Items.Refresh();

                                tb1GridColindancia.ItemsSource = M.Colindante.MainData;
                                CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(this.tb1GridColindancia.ItemsSource);
                                view.GroupDescriptions.Clear();
                                view.GroupDescriptions.Add(new PropertyGroupDescription("Vivienda"));
                                view.GroupDescriptions.Add(new PropertyGroupDescription("Apartamento"));
                            }
                            else
                            {
                                RollBackPoints();

                                this.ShowMessageAsync("Error al Crear Puntos",
                                        "No se crearon los puntos de manera correcta");
                            }                                                      
                        }
                        else
                        {
                            RollBackPoints();

                            this.ShowMessageAsync("Error al Crear Puntos",
                                    "No se crearon los puntos de manera correcta");
                        }
                    }

                }
                
                
                else
                {
                    RollBackPoints();

                    this.ShowMessageAsync("Error al Crear Puntos",
                            "No se crearon los puntos de manera correcta");
                }
            }
            #endregion
            #region LOTE
            else //Si es LOTE
            {
                if(Met_Colindante.CreatePointsSet(M.Colindante.IdTipo, "Lote Tipo"))
                {
                    cont++;
                    bool siGeneroTodo = true;
                    foreach (ObjectId idIrregular in M.Colindante.IdsIrregulares)
                    {
                        cont++;
                        if (!Met_Colindante.CreatePointsSet(idIrregular, "Lote Irregular"))
                        {
                            RollBackPoints();
                            siGeneroTodo = false;
                            this.ShowMessageAsync("Error al Crear Puntos",
                                    "No se crearon los puntos de manera correcta");
                            break;
                        }

                    }

                    if (siGeneroTodo)
                    {
                        if (tb2GridErrores.Items.NeedsRefresh)
                            tb2GridErrores.Items.Refresh();

                        tb1GridColindancia.ItemsSource = M.Colindante.MainData;
                        CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(this.tb1GridColindancia.ItemsSource);
                        view.GroupDescriptions.Clear();
                        view.GroupDescriptions.Add(new PropertyGroupDescription("Vivienda"));
                        view.GroupDescriptions.Add(new PropertyGroupDescription("Apartamento"));
                    }
                }
                else
                {
                    RollBackPoints();

                    this.ShowMessageAsync("Error al Crear Puntos",
                            "No se crearon los puntos de manera correcta");
                }
            }
            #endregion

           
            tb1CalPuntos.IsEnabled = true;

            s1.Stop();

            this.WindowState = WindowState.Normal;

            this.ShowMessageAsync("Tiempo tomado", "Minutos: " + s1.Elapsed.TotalMinutes.Trunc(3) 
                + "\nSegundos: " + s1.Elapsed.TotalSeconds.Trunc(3) +string.Format("con  {0} Viviendas", cont));    

        }

        private void RollBackPoints()
        {
            
        }

        #endregion

        /// <summary>
        /// Le cambio el formato al TAB Activo
        /// </summary>
        private void tabCsControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FormatTab();
        }
        
        /// <summary>
        /// Formateo tab
        /// </summary>
        private void FormatTab()
        {
            Thickness myActiveThickness = new Thickness()
            {
                Left = 1,
                Right = 1,
                Bottom = 0,
                Top = 5
            };

            Thickness mySleepThickness = new Thickness()
            {
                Left = 0,
                Bottom = 0,
                Right = 0,
                Top = 5
            };

            TabItem tbItemActivo = tabCsControl.SelectedIndex == 0 ?
                                    (tabCsControl.Items[0] as TabItem) :
                                    (tabCsControl.Items[1] as TabItem);

            TabItem tbItemOculto = tabCsControl.SelectedIndex == 0 ?
                                    (tabCsControl.Items[1] as TabItem) :
                                    (tabCsControl.Items[0] as TabItem);            

            tbItemActivo.BorderBrush = Brushes.LightGray;
            tbItemOculto.BorderBrush = Brushes.Transparent;

            tbItemActivo.BorderThickness = myActiveThickness;
            tbItemOculto.BorderThickness = mySleepThickness;
        }               


        private async  void tb1Clear_Click(object sender, RoutedEventArgs e)
        {
            if (tb1GridColindancia.Items.Count > 0)
            {
                MessageDialogResult dg = await this.ShowMessageAsync("Eliminar Datos",
                            "¿Desea Eliminar Puntos Calculados?", MessageDialogStyle.AffirmativeAndNegative);

                if (dg == MessageDialogResult.Affirmative)
                {
                    this.WindowState = WindowState.Minimized;                   

                    Met_Colindante.DeleteAdjacencyObjects();

                    tb1GridColindancia.ItemsSource = null;

                    M.Colindante.MainData.Clear();                   

                    this.tb1GridColindancia.Items.Refresh();

                    this.WindowState = WindowState.Maximized;
                }
            }
        }
        

        private async void tb1IrHome_Click(object sender, RoutedEventArgs e)
        {
            MessageDialogResult dg = await this.ShowMessageAsync("Ir a Inicio",
                            "¿Desea ir a Módulo Inicial?", MessageDialogStyle.AffirmativeAndNegative);

            if(dg == MessageDialogResult.Affirmative)
            {
                ModuloInicial mi = new ModuloInicial();
                mi.Show();
                this.Close();
            }
        }

        private void DataGridTextColumn_MouseUp(object sender, MouseButtonEventArgs e)
        {
            TextBlock txt = sender as TextBlock;

            long result = 0;

            if (long.TryParse(txt.Text, out result))
            {
                if(result  != 0)
                {
                    Handle handle = new Handle(result);

                    ObjectId id = handle.toObjectId();

                    Met_Autodesk.SetImpliedSelection(handle.toObjectId());

                    (id.OpenEntity() as Autodesk.AutoCAD.DatabaseServices.Polyline).Focus(10, 10);

                    this.WindowState = WindowState.Minimized;
                }
                
            }
            
        }

        private void tb1btnReload_Click(object sender, RoutedEventArgs e)
        {
            //Comienzo transacción minimizando
            this.WindowState = WindowState.Minimized;

            ValidateEntityNumbers();

            //Termino transacción maximizando
            this.WindowState = WindowState.Normal;
        }

        private void tb1cmbDecimales_SelectChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox combo = sender as ComboBox;             

            if(combo.SelectedValue != null && combo.SelectedValue.ToString() != "")
            {
                int decimals = int.Parse(combo.SelectedValue.ToString());

                M.Colindante.Decimals = decimals;
            }               
        }
    }
}
