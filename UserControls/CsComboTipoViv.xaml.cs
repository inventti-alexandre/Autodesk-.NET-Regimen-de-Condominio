using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RegimenCondominio.Controls
{
    /// <summary>
    /// Interaction logic for CsComboTipoViv.xaml
    /// </summary>
    public partial class CsComboTipoViv : UserControl
    {
        public event SelectionChangedEventHandler SelectChanged;

        public object SelectedItem
        {
            get
            {
                return this.ComboPrinc.SelectedItem;
            }
            set
            {
                this.ComboPrinc.SelectedItem = value;
            }
        }
        /// <summary>
        /// El mensaje que aparece antes de ingresar texto
        /// </summary>
        /// <value>
        /// El contenido del mensaje
        /// </value>
        public String MessageCombo
        {
            get
            {
                return (String)GetValue(MessageProperty);
            }
            set
            {
                SetValue(MessageProperty, value);
            }
        }

        //Las variables de dependencia
        static DependencyProperty MessageProperty;       
       

        static CsComboTipoViv()
        {
            MessageProperty = DependencyProperty.Register("MessageCombo", typeof(String), typeof(CsComboTipoViv),
                new FrameworkPropertyMetadata(String.Empty, FrameworkPropertyMetadataOptions.AffectsRender,
                OnMessage_Changed));
                        
        }        

        public CsComboTipoViv()
        {
            InitializeComponent();
        }

        static void OnMessage_Changed(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            (sender as CsComboTipoViv).msgTmp.Text = e.NewValue as String;
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Oculto Textblock cuando ya introdujo un caracter
            this.msgTmp.Visibility = ComboPrinc.SelectedIndex > -1 ? Visibility.Hidden : Visibility.Visible;

            //Si oculte el Textblock, muestro el botón
            if (this.msgTmp.Visibility == Visibility.Hidden)
            {
                //Comienzo animación para reducir textbox
                (this.Resources["kRedBox"] as
                System.Windows.Media.Animation.Storyboard).Begin();
            }
            ///En dado caso que no, debe de permanecer oculto
            else
            {
                (this.Resources["kExpBox"] as
                System.Windows.Media.Animation.Storyboard).Begin();
            }
            if (SelectChanged != null)
                SelectChanged(sender, e);
        }

        private void GenTextbox_Loaded(object sender, RoutedEventArgs e)
        {
            //new C.SqlTransaction(null, LoadDataTask, DataLoaded).Run();
        }

        private void DataLoaded(object input)
        {
            foreach (var item in input as List<M.DatosTipoViv>)
            {
                ComboPrinc.Items.Add(item.NombreTipoViv);
            }

            //ProgressComboFracc.Visibility = Visibility.Collapsed;
            msgTmp.Visibility = Visibility.Visible;
        }

        public IEnumerable ItemsSource
        {
            get { return this.ComboPrinc.ItemsSource; }
            set { this.ComboPrinc.ItemsSource = value; }
        }


        private object LoadDataTask(C.SQL_Connector conn, object input, BackgroundWorker bg)
        {            
            List<String> result;

            M.EncInicio.ResultTipoVivs.Clear();

            if (conn.Select("select ID_TIPOVIV,NOMBRE_TIPOVIV from TIPO_VIVS", out result, '|'))
            {
                String[] cell;
                foreach (String row in result)
                {
                    cell = row.Split('|');                   

                    M.EncInicio.ResultTipoVivs.Add(new M.DatosTipoViv()
                    {
                        IdTipoViv = int.Parse(cell[0]),
                        NombreTipoViv = cell[1]                        
                    });
                }
            }

            return M.EncInicio.ResultTipoVivs;
        }
    }
}