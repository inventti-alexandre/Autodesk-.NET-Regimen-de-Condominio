using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Drawing;
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
using System.Windows.Resources;
using System.Windows.Shapes;
using RegimenCondominio.C;

namespace RegimenCondominio.V
{
    /// <summary>
    /// Interaction logic for ModuloManzana.xaml
    /// </summary>
    public partial class ModuloManzana : MetroWindow
    {
        public ModuloManzana()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ModuloInicial M_Inicial = new ModuloInicial();
            M_Inicial.Show();
            M.Constant.IsAutoClose = true;
            this.Close();
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //Todas las orientaciones disponibles
            CmbRumboFrente.ItemsSource = Met_Manzana.DespliegoOrientaciones();

            //Agrego los tipos de Colindancias
            cmbTipo.ItemsSource = M.Constant.TipoColindancias;

            //Obtengo las Manzanas que cumplen con criterio en plano
            cmbManzana.ItemsSource = ObtengoManzanas();            

            //Si solamente leyó una manzana que la asigné de manera automática
            if (cmbManzana.Items.Count == 1)
                cmbManzana.SelectedIndex = 0;

            //La lista debe de estar de acuerdo a lo insertado en listado de Colindancias
            ListPrincipal.ItemsSource = M.Manzana.ColindanciaManzana;

            //Asigno Lote o Macrolote
            if (M.Manzana.EsMacrolote)
                rbMacroLote.IsChecked = true;
            else
                rbLote.IsChecked = true;
        }



        private List<string> ObtengoManzanas()
        {                      
            return Met_Autodesk.ModelDBText(M.Constant.LayerManzana).
                Select(x=> x.TextString).ToList();                       
        }

        private void btnSelManzana_Click(object sender, RoutedEventArgs e)
        {
            //Inicializo Id de Polilínea de Manzana y Puntos
            ObjectId    idManzana = new ObjectId(), 
                        idNoManzana = new ObjectId();

            Point3dCollection pts = new Point3dCollection();

            string strNoManzana = "";

            //Minimizo la ventana
            WindowState = WindowState.Minimized;

            //Solicito al usuario las selección de la manzana
            if(C.Met_Autodesk.Entity("Favor de seleccionar la Manzana: ",out idManzana, 
                typeof(Autodesk.AutoCAD.DatabaseServices.Polyline)))
            {
                CmbRumboFrente.IsEnabled = true;
                
                //Obtengo Puntos 3d de Polilínea Manzana
                pts = idManzana.ExtractVertex();

                string msj = "";

                //Busco Id Manzana
                idNoManzana = pts.DBTextByLayer(M.Constant.LayerManzana, out msj).
                    OfType<ObjectId>().FirstOrDefault();

                strNoManzana = (idNoManzana.OpenEntity() as DBText).TextString;

                //ResManzana.Text = strNoManzana;
            }
            WindowState = WindowState.Normal;
        }

        private bool handleSelection = true;

        private async void CmbRumboFrente_SelectChanged(object sender, SelectionChangedEventArgs e)
        {
            //Si ya se selecciono un rumbo
            if (CmbRumboFrente.SelectedIndex != -1)
            {
                //Activo la segunda sección "Colindancia"
                GridColindancia.IsEnabled = true;
                
                //En el titulo cambio el color
                TituloColindancia.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 137, 123));

                //Si ya había introducido algo en la tabla envío advertencia
                if (ListPrincipal.Items.Count > 0 && handleSelection)
                {
                    MessageDialogResult result
                        = await this.ShowMessageAsync("Modificación de Rumbo Frente",
                        "Se eliminarán los cambios realizados", MessageDialogStyle.AffirmativeAndNegative);

                    //Si presiona OK
                    if (result == MessageDialogResult.Affirmative)
                    {
                        //Limpio la lista
                        M.Manzana.ColindanciaManzana.Clear();

                        //Refresco los items
                        ListPrincipal.Items.Refresh();

                        //Asigno nuevamente
                        AsignaOrientaciones(CmbRumboFrente.SelectedItem.ToString());

                        //Bloqueo el combo de Colindancias nuevamente
                        cmbRumboActual.IsEnabled = false;

                        //Cambio el Fondo
                        btnAdd.Content = FindResource("AddC");

                    }
                    else
                    {
                        if (e.RemovedItems.Count > 0)
                        {
                            handleSelection = false;
                            (sender as ComboBox).SelectedValue = (e.RemovedItems[0] ?? "").ToString();                            
                            return;
                        }
                    }
                }
                //Si no ha introducido nada asigno colindancias a la lista
                else if(ListPrincipal.Items.Count == 0)
                {
                    AsignaOrientaciones(CmbRumboFrente.SelectedItem.ToString());
                }

                handleSelection = true;

            }

            
        }

        private void AsignaOrientaciones(string RumboFrente)
        {
            //Inicializo Lista
            M.Manzana.OrientacionCalculada = new List<string>();

            //Calculo las Orientaciones de acuerdo a Lista
            M.Manzana.OrientacionCalculada = Met_Manzana.OrientacionFrente(RumboFrente);

            //Asigno resultado
            cmbRumboActual.ItemsSource = M.Manzana.OrientacionCalculada;

            //Asigno la primera orientación
            cmbRumboActual.SelectedIndex = 0;
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {                        
            ObjectId    idtxtCol = new ObjectId(),//Id de Texto con el que Colinda
                        idLineCol = new ObjectId();//Id de Linea/Polilinea con la que Colinda            

            //Selecciono el Item 
            string rumboSeleccionado = (cmbRumboActual.SelectedItem ?? "").ToString();            

            //Si ya se selecciono algo en el combobox de tipo
            if (cmbTipo.SelectedIndex != -1)
            {
                this.WindowState = WindowState.Minimized;
                //Solicito que me hagan saber el texto que colinda
                if (Met_Autodesk.Entity("Selecciona la línea de colindancia al " + rumboSeleccionado + "\n",
                    out idLineCol, M.Constant.TiposLineas) &&
                    Met_Autodesk.Entity("Selecciona la colindancia al " + rumboSeleccionado + "\n",
                    out idtxtCol, typeof(DBText)))
                {
                    if (idLineCol.OpenEntity().Layer == M.Constant.LayerManzana)
                    {
                        //Obtengo el DBText seleccionado
                        DBText DBTextColindancia = idtxtCol.OpenEntity() as DBText;

                        //Texto del DB Text                                            
                        string txtColindancia = DBTextColindancia.TextString.FormatString();

                        //Modelo los datos
                        M.ManzanaData insertedData = new M.ManzanaData()
                        {
                            hndPlColindancia = idLineCol.Handle,
                            hndTxtColindancia = idtxtCol.Handle,
                            inicialRumbo = (M.Constant.Orientaciones
                                                [Met_Manzana.ObtengoPosicion(rumboSeleccionado, 0), 1]),
                            rumboActual = rumboSeleccionado,
                            textColindancia = cmbTipo.SelectedIndex > 0 ? txtColindancia :
                                                                            "calle " + txtColindancia
                        };

                        bool PolilineaNueva = false,
                                RumboNuevo = false;

                        int sigPosicion = 0;

                        //Si ya se había insertado esa polilinea
                        PolilineaNueva = M.Manzana.ColindanciaManzana.Where
                            (x => x.hndPlColindancia.Value == insertedData.hndPlColindancia.Value).
                            Count() > 0 ? false : true;

                        //Si ya se había insertado ese rumbo en la lista
                        RumboNuevo = M.Manzana.ColindanciaManzana.
                            Where(x => x.rumboActual == insertedData.rumboActual).Count() > 0
                            ? false : true;

                        //Si es Nueva Polilinea y nuevo Rumbo
                        if (PolilineaNueva && RumboNuevo)
                            sigPosicion = insertedData.InsertoColindancia();
                        else
                            sigPosicion = insertedData.ReasignoColindancia(PolilineaNueva, RumboNuevo);

                        //Reviso que rumbo mostrará
                        SigColindancia(sigPosicion);

                        if (ListPrincipal.ItemsSource != null)
                            ListPrincipal.Items.Refresh();
                        else
                            ListPrincipal.ItemsSource = M.Manzana.ColindanciaManzana;
                    }
                    else
                        this.ShowMessageAsync("Layer Incorrecto", "La línea debe de estar en Layer " + M.Constant.LayerManzana);
                }
                this.WindowState = WindowState.Normal;
            }
            else            
                this.ShowMessageAsync("Datos no seleccionados", "Favor de seleccionar Tipo de Colindancia");                       
            

        }       

        private void SigColindancia(int sigPosicion)
        {
            int countOrientaciones = M.Manzana.ColindanciaManzana.Count;

            if (sigPosicion < M.Constant.RumboMaximo &&//Si todavía el siguiente rumbo es menor a 4
                         countOrientaciones < M.Constant.RumboMaximo)//Y no ha llegado a los 4 la lista
            {
                //Obtengo el siguiente rumbo
                string sigRumbo = M.Manzana.OrientacionCalculada[sigPosicion];

                //Cambio el rumbo en el combobox
                cmbRumboActual.SelectedItem = sigRumbo;
            }
            else
            {
                //Activo el combo de Colindancia
                cmbRumboActual.IsEnabled = true;
            }


            if (countOrientaciones >= M.Constant.RumboMaximo)
            {
                //Cambio el Fondo
                btnAdd.Content = FindResource("Edit");
            }
            else
            {
                //Cambio el Fondo
                btnAdd.Content = FindResource("AddC");                
            }

        }
       
        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {            
            //Reasigno elementos posterior a la búsqueda
            cmbManzana.ItemsSource = ObtengoManzanas();
        }

        private void btnAvanzar_Click(object sender, RoutedEventArgs e)
        {
            int outNoManzana = 0;
            //Reviso que no sea nulo los valores seleccionados
            if (!string.IsNullOrWhiteSpace((cmbManzana.SelectedItem ?? "").ToString()) &&
                !string.IsNullOrWhiteSpace((CmbRumboFrente.SelectedItem ?? "").ToString()))
            {
                //Si ya cuenta con los 4 rumbos
                if (M.Manzana.ColindanciaManzana.Count == M.Constant.RumboMaximo)
                {
                    //Si la manzana es un número entero
                    if (int.TryParse(cmbManzana.SelectedItem.ToString(), out outNoManzana))
                    {
                        //Una vez se validaron los datos los encapsulo
                        M.Manzana.NoManzana = outNoManzana;
                        M.Manzana.RumboFrente = CmbRumboFrente.SelectedItem.ToString();
                        M.Constant.IsAutoClose = true;
                        this.Close();
                        ModuloColindante M_Colindante = new ModuloColindante();
                        M_Colindante.Show();
                        
                    }
                    else
                        this.ShowMessageAsync("Error en No. de Manzana", "La manzana debe de ser un número entero");
                }
                else
                    this.ShowMessageAsync("Datos Faltantes", 
                        string.Format("Deben de ser {0} rumbos asignados", M.Constant.RumboMaximo));
            }
            else
                this.ShowMessageAsync("Valores en Blanco",
                    string.Format("Favor de llenar todos los campos y/o los {0} Rumbos", M.Constant.RumboMaximo));
        }

        private void rbLote_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rbsent = sender as RadioButton;

            if (rbsent.Content.ToString() == "Lote")
                M.Manzana.EsMacrolote = false;
            else
                M.Manzana.EsMacrolote = true;
                                    
            //tab1.Header = rbsent.Content.ToString();

                //if (rbsent.Content.ToString() == "Lote")
                //{
                //    txtHeaderPaso2.Text = "2. Seleccionar Lote Tipo";
                //    txtHeaderPaso3.Text = "3. Selecciona Lotes Irregulares";
                //}
                //else
                //{
                //    txtHeaderPaso2.Text = "2. Sel. Último Edificio Tipo";
                //    txtHeaderPaso3.Text = "3. Sel. Edificios Irregulares";
                //}
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
