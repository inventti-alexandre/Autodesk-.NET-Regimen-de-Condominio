using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
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

namespace RegimenCondominio.UserControls
{
    /// <summary>
    /// Interaction logic for CsComboBox.xaml
    /// </summary>
    public partial class CsComboBox : UserControl
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

        static CsComboBox()
        {
            MessageProperty = DependencyProperty.Register("MessageCombo", typeof(String), typeof(CsComboBox),
                new FrameworkPropertyMetadata(String.Empty, FrameworkPropertyMetadataOptions.AffectsRender, 
                OnMessage_Changed));                       
        }

        //Expongo Propiedad
        public IEnumerable ItemsSource
        {
            get { return this.ComboPrinc.ItemsSource; }
            set { this.ComboPrinc.ItemsSource = value; }
        }


        public CsComboBox()
        {
            InitializeComponent();
        }

        static void OnMessage_Changed(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            (sender as CsComboBox).msgTmp.Text = e.NewValue as String;
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
    }
}
