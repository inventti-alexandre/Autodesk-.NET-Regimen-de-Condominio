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
            //Inicializo Variables--------------------------------------
            List<ObjectId> listSegments = new List<ObjectId>();

            ObjectIdCollection idsLotes = new ObjectIdCollection();                                        
            //----------------------------------------------------------            

            //Crea Layers en dado caso que no exista
            string.Format("Creo {0} Layers \n", Met_Colindante.CreateAdjacencyLayers()).ToEditor();

            //Leo los segmentos seleccionados
            listSegments = M.Manzana.ColindanciaManzana
            .Select(x => x.HndPlColindancia.toObjectId()).ToList();

            this.WindowState = WindowState.Minimized;

            #region Busco Lote o Edificios según aplique
                        
            //Obtengo los Lotes / Macrolotes
            idsLotes = Met_Autodesk.GetPolylinesInSegments(listSegments, M.Constant.LayerLote);

            foreach (ObjectId idLote in idsLotes)
            {
                M.Colindante.ValorLotes.Add(new M.EntityValue()
                {
                    idEntity = idLote,
                    value = ""
                });
            }

            string LoteOMacroLote = "";

            if (idsLotes.Count == 1)
                LoteOMacroLote = M.Manzana.EsMacrolote ? "Macrolote" : "Lote";
            else
                LoteOMacroLote = M.Manzana.EsMacrolote ? "Macrolotes" : "Lotes";


            lblGnLotes.Text = idsLotes.Count.ToString() + " " + LoteOMacroLote;            
            #endregion

            #region Si es Macrolote
            //Solamente cuando sea Macrolote leo los Macrolotes--------------------------------------
            if (M.Manzana.EsMacrolote)                                                         
                AdaptView();            
            //--------------------------------------------------------------------          
            #endregion

            this.WindowState = WindowState.Normal;

            FillControlsData();
                                              
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
            txtHeaderPaso2.Text = "2. Sel. Último Edificio Tipo";
            txtHeaderPaso3.Text = "3. Sel. Edificios Irregulares";
            txtHeaderPaso4.Text = "4. Calcular Puntos";

            //Cambio el color
            txtHeaderPaso1.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#00897B"));

            //Activo Paso 1 y desactivo paso 2
            tb1BorderPaso1.IsEnabled = true;
            tb1SelLoteTipo.IsEnabled = false;

            //Posición de Datos Generales                
            lblGnLotes.VerticalAlignment = VerticalAlignment.Bottom;
            lblGnLotes.FontSize = 13;
            //lblLoteEdificio.Margin = new Thickness(3, 23, 3, 0);                

            //Macrolotes
            lblGnEdificios.Visibility = System.Windows.Visibility.Visible;
        }


        #region Pasos

        private void tb1SelMacrolote_Click(object sender, RoutedEventArgs e)
        {
            ObjectId idPl = new ObjectId();

            this.WindowState = WindowState.Minimized;

            //Valido que se haya obtenido de manera correcta
            if (Met_Autodesk.Entity("Selecciona Macrolote: ", out idPl,
                typeof(Autodesk.AutoCAD.DatabaseServices.Polyline)))
            {
                //Reviso que la polilínea este dentro del mismo layer
                if (idPl.OpenEntity().Layer == M.Constant.LayerLote)
                {
                    if (M.Colindante.ValorLotes.OfType<M.EntityValue>()
                        .Where(x => x.idEntity == idPl).Count() > 0)
                    {
                        M.Colindante.IdMacrolote = idPl;
                        //tb1CheckMacrolote.IsChecked = true;
                        tb1SelLoteTipo.IsEnabled = true;
                    }
                    else
                        this.ShowMessageAsync("Error de selección",
                            "El lote seleccionado esta fuera de la Manzana y/o tiene distinto layer");
                }
                else
                {
                    this.ShowMessageAsync("Error de selección",
                           string.Format("El lote no esta dentró del layer {0}", M.Constant.LayerLote));

                    M.Colindante.ListadoErrores.Add(new M.DescribeError()
                    {
                        Error = "Layer Inválida",
                        Description = string.Format("El objeto debe de tener Layer {0}", M.Constant.LayerLote),
                        timeError = M.Constant.TimeNow
                    });
                }
            }
            else
            {
                M.Colindante.ListadoErrores.Add(new M.DescribeError()
                {
                    Error = "Id no Valida",
                    Description = "Id de Polilínea no Válido",
                    timeError = M.Constant.TimeNow
                });
            }

            this.WindowState = WindowState.Normal;
        }


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
                    //Reviso que este dentro de los edificios leídos en el plano
                    if (M.Colindante.ValorEdificio.OfType<M.EntityValue>()
                        .Where(x => x.idEntity == idPl).Count() > 0)
                    {
                        M.Colindante.IdLoteTipo = idPl;
                        tb1SelMultiple.IsEnabled = true;
                        //tb1CheckLoteTipo.IsChecked = true;
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
                        timeError = M.Constant.TimeNow
                    });
                }
            }
            else
            {
                M.Colindante.ListadoErrores.Add(new M.DescribeError()
                {
                    Error = "Id no Valida",
                    Description = "Id de Polilínea no Válido",
                    timeError = M.Constant.TimeNow
                });
            }
            this.WindowState = WindowState.Normal;
        }

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
                //Variable para revisar si esta afuera del macrolote
                bool areInside = false;

                //Obtengo los ids
                List<ObjectId> idsEdificiosLote = M.Colindante.ValorEdificio.Select(x => x.idEntity).ToList();

                //Si todos los Ids seleccionados están dentro de los edificios/lotes detectados
                areInside = idsSelected.OfType<ObjectId>().
                    Where(x => idsEdificiosLote.Contains(x)).Count() == idsSelected.Count ? true: false ;

                //Si ninguno esta afuera de la manzana
                if (areInside)
                {
                    M.Colindante.IdsLotesIrregulares = idsSelected;
                    tb1CalPuntos.IsEnabled = true;
                    tb1CheckLoteIrregular.IsChecked = true;
                }
                else
                    this.ShowMessageAsync("Error de selección",
                        "Algún lote seleccionado esta fuera de la Manzana y/o tiene distinto layer");
            }
            else
            {
                M.Colindante.ListadoErrores.Add(new M.DescribeError()
                {
                    Error = "Ids Invalidos",
                    Description = "Error al seleccionar Polilíneas en Paso 3",
                    timeError = M.Constant.TimeNow
                });
            }

            this.WindowState = WindowState.Normal;
        }




        #endregion
        private void tabCsControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FormatTab();
        }

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

        private void tb1CalPuntos_Click(object sender, RoutedEventArgs e)
        {
            tb1CalPuntos.IsEnabled = false;

            this.WindowState = WindowState.Minimized;

            if(Met_Colindante.CreatePoints(M.Colindante.IdLoteTipo))
            {
                
                foreach(ObjectId idIrregular in M.Colindante.IdsLotesIrregulares)
                {
                    Met_Colindante.CreatePoints(idIrregular);
                }

                tb1GridColindancia.ItemsSource = M.Colindante.MainData;
                CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(this.tb1GridColindancia.ItemsSource);                
                view.GroupDescriptions.Clear();
                view.GroupDescriptions.Add(new PropertyGroupDescription("Lote"));
                view.GroupDescriptions.Add(new PropertyGroupDescription("Apartamento"));
            }
            else
                this.ShowMessageAsync("Error al Crear Puntos",
                        "No se crearon los puntos de manera correcta");


            tb1CalPuntos.IsEnabled = true;

            this.WindowState = WindowState.Maximized;                                 
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

                    M.Colindante.MainData.Clear();

                    tb1GridColindancia.Items.Clear();

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
    }
}
