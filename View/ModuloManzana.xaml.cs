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
            ModuloInicial M_Manzana = new ModuloInicial();
            M_Manzana.Show();
            this.Close();
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            CmbRumboFrente.ItemsSource = C.Met_Manzana.DespliegoOrientaciones();

            cmbTipo.ItemsSource = M.ConstantValues.TipoColindancias;            
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
                pts = C.Met_Autodesk.ExtraerVertices(idManzana);

                string msj = "";

                //Busco Id Manzana
                idNoManzana = C.Met_Autodesk.TomarEntidadLayer(M.ConstantValues.LayerManzana, pts, out msj).
                    OfType<ObjectId>().FirstOrDefault();

                strNoManzana = (C.Met_Autodesk.AbrirEntidad(idNoManzana) as DBText).TextString;

                ResManzana.Text = strNoManzana;
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
                        //Limpio la tabla
                        ListPrincipal.Items.Clear();

                        //Asigno nuevamente
                        AsignaOrientaciones(CmbRumboFrente.SelectedItem.ToString());

                        //Bloqueo el combo de Colindancias nuevamente
                        cmbColindancia.IsEnabled = false;

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
            M.Manzana.CalcOrientaciones = new List<string>();

            //Calculo las Orientaciones de acuerdo a Lista
            M.Manzana.CalcOrientaciones =
                C.Met_Manzana.OrientacionFrente(RumboFrente);

            //Asigno la primera orientación
            cmbColindancia.ItemsSource = M.Manzana.CalcOrientaciones;

            cmbColindancia.SelectedIndex = 0;
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {            

            ObjectId idCol = new ObjectId();

            string ColindanciaActual = (cmbColindancia.SelectedItem ?? "").ToString();

            this.WindowState = WindowState.Minimized;

            //Si ya se selecciono algo en el combobox de tipo
            if (cmbTipo.SelectedIndex != -1)
            {
                //Solicito que me hagan saber el texto que colinda
                if (C.Met_Autodesk.Entity("Selecciona la colindancia al " + ColindanciaActual + "\n",
                    out idCol, typeof(DBText)))
                {
                    //Obtengo el Texto seleccionado en el plano
                    string Colindancia = C.Met_General.FormatString((C.Met_Autodesk.AbrirEntidad(idCol) as DBText).TextString);

                    //Modelo los datos
                    M.DatosColindancia dc = new M.DatosColindancia()
                    {
                        RumboActual = (M.ConstantValues.Orientaciones
                                            [C.Met_Manzana.ObtengoPosicion(ColindanciaActual, 0), 1]),
                        TextPlano = cmbTipo.SelectedIndex > 0 ? Colindancia : "calle " + Colindancia
                    };                    

                    if (ListPrincipal.Items.Count < M.Manzana.CalcOrientaciones.Count)
                    {
                        //Agrego el dato a la lista
                        ListPrincipal.Items.Add(dc);
                        
                        //Obtengo la siguiente orientación
                        int sigPosicion = C.Met_Manzana.ObtengoPosicion(ColindanciaActual,
                            M.Manzana.CalcOrientaciones) + 1;

                        if (sigPosicion < M.Manzana.CalcOrientaciones.Count)
                        {
                            //Obtengo el siguiente rumbo
                            string sigRumbo = M.Manzana.CalcOrientaciones[sigPosicion];

                            //Cambio el rumbo en el combobox
                            cmbColindancia.SelectedItem = sigRumbo;
                        }
                        else
                        {
                            //Cambio el Fondo
                            btnAdd.Content = FindResource("Edit");

                            //Activo el combo de Colindancia
                            cmbColindancia.IsEnabled = true;
                        }
                    }
                    else
                    {
                        
                        string LetrasRumbo = (M.ConstantValues.Orientaciones
                                            [C.Met_Manzana.ObtengoPosicion(ColindanciaActual, 0), 1]);

                        
                        for (int i=0; i < ListPrincipal.Items.Count; i++)
                        {
                            //Modifico el item con la nueva colindancia
                            if ((ListPrincipal.Items[i] as M.DatosColindancia).RumboActual == LetrasRumbo)
                            {
                                ListPrincipal.Items[i] = new M.DatosColindancia()
                                {
                                    RumboActual = LetrasRumbo,
                                    TextPlano = cmbTipo.SelectedIndex > 0 ? Colindancia : "calle " + Colindancia
                                };
                                break;
                            }
                        }
                    }

                }
            }
            else
            {
                this.ShowMessageAsync("Datos no seleccionados",
                        "Favor de seleccionar Tipo de Colindancia");
            }            
            this.WindowState = WindowState.Normal;

        }

        private void TextBlock_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            
        }
    }
}
