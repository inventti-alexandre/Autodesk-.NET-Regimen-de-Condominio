using System;
using System.Collections;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RegimenCondominio.UserControls
{
    /// <summary>
    /// Lógica de interacción para CsNormalCombo.xaml
    /// </summary>
    public partial class CsNormalCombo : UserControl
    {        

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
        public String MessageCombo2
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

        static CsNormalCombo()
        {
            MessageProperty = DependencyProperty.Register("MessageCombo2", typeof(string), typeof(CsNormalCombo),
                new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.AffectsRender,
                OnMessage_Changed));
        }

        //Expongo Propiedad
        public IEnumerable ItemsSource
        {
            get { return this.ComboPrinc.ItemsSource; }
            set { this.ComboPrinc.ItemsSource = value; }
        }

        public ItemCollection Items
        {
            get { return this.ComboPrinc.Items; }            
        }
        public int SelectedIndex
        {
            get { return this.ComboPrinc.SelectedIndex; }
            set { this.ComboPrinc.SelectedIndex = value; }
        }

        static void OnMessage_Changed(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            (sender as CsNormalCombo).msgTmp.Text = e.NewValue as String;
        }
        public CsNormalCombo()
        {
            InitializeComponent();
        }

        public event SelectionChangedEventHandler SelectChanged;

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Oculto Textblock cuando ya introdujo un caracter
            this.msgTmp.Visibility = ComboPrinc.SelectedIndex > -1 ? Visibility.Hidden : Visibility.Visible;

            if (SelectChanged != null)
                SelectChanged(sender, e);
        }
    }
}
