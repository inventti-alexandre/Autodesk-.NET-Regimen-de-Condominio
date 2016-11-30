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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RegimenCondominio.UserControls
{
    /// <summary>
    /// Interaction logic for CsTextbox.xaml
    /// </summary>
    public partial class CsTextbox : UserControl
    {
        //Evento para cambiar el texto
        public event TextChangedEventHandler TextChanged;

        public TextboxInputScope InputType
        {
            get
            {
                return (TextboxInputScope)GetValue(InputTypeProperty);
            }
            set
            {
                SetValue(InputTypeProperty, value);
            }
        }

        public String Text
        {
            get
            {
                return this.textValue.Text;
            }
            set
            {
                this.textValue.Text = value;
            }
        }
        /// <summary>
        /// El mensaje que aparece antes de ingresar texto
        /// </summary>
        /// <value>
        /// El contenido del mensaje
        /// </value>
        public String Message
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

        //Propiedades que pueden aparecer en
        //la edición de estilos
        public new Double FontSize
        {
            get { return (Double)GetValue(FontSizeProperty); }
            set { SetValue(FontSizeProperty, value); }
        }

        public static Double GetFontSize(CsTextbox target)
        {
            return target.FontSize;
        }
        public static void SetFontSize(CsTextbox target, Double value)
        {
            target.FontSize = value;
        }

        public static Brush GetBorderColor(CsTextbox target)
        {
            return target.BorderColor;
        }
        public static void SetBorderColor(CsTextbox target, Brush value)
        {
            target.BorderColor = value;
        }

        public Brush BorderColor
        {
            get { return (Brush)GetValue(BorderColorProperty); }
            set { SetValue(BorderColorProperty, value); }
        }

        //Las variables de dependencia
        static DependencyProperty MessageProperty;
        static DependencyProperty InputTypeProperty;
        //Propiedades de estilo
        static new DependencyProperty FontSizeProperty;
        static DependencyProperty BorderColorProperty;

        static CsTextbox()
        {
            MessageProperty = DependencyProperty.Register("Message", typeof(String), typeof(CsTextbox),
                new FrameworkPropertyMetadata(String.Empty, FrameworkPropertyMetadataOptions.AffectsRender, OnMessage_Changed));
            InputTypeProperty = DependencyProperty.Register("InputType", typeof(TextboxInputScope), typeof(CsTextbox),
                new FrameworkPropertyMetadata(TextboxInputScope.None, FrameworkPropertyMetadataOptions.AffectsRender, OnInputType_Changed));
            FontSizeProperty = DependencyProperty.RegisterAttached("FontSize", typeof(Double), typeof(CsTextbox),
                new FrameworkPropertyMetadata(8d, FrameworkPropertyMetadataOptions.AffectsRender, OnFontSize_Changed));
            BorderColorProperty = DependencyProperty.RegisterAttached("BorderColor", typeof(Brush), typeof(CsTextbox),
               new FrameworkPropertyMetadata(default(Brush), FrameworkPropertyMetadataOptions.AffectsRender, BorderColor_Changed));
        }
        static void BorderColor_Changed(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            (sender as CsTextbox).border.BorderBrush = (Brush)e.NewValue;
        }

        static void OnFontSize_Changed(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            (sender as CsTextbox).textValue.FontSize = (Double)e.NewValue;
            (sender as CsTextbox).msgTmp.FontSize = (Double)e.NewValue;
        }        
        static void OnMessage_Changed(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            (sender as CsTextbox).msgTmp.Text = e.NewValue as String;
        }
        static void OnInputType_Changed(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (((TextboxInputScope)e.NewValue) == TextboxInputScope.Number)
            {
                string value = (sender as CsTextbox).Text;
                Double num;
                if (!Double.TryParse(value, out num))
                    (sender as CsTextbox).Text = String.Empty;
            }
        }

        public CsTextbox()
        {
            InitializeComponent();
        }
        
        private void textValue_Changed(object sender, TextChangedEventArgs e)
        {
            //Oculto Textblock cuando ya introdujo un caracter
            this.msgTmp.Visibility = this.textValue.Text.Length > 0 ? Visibility.Hidden : Visibility.Visible;

            //Si oculte el Textblock, muestro el botón
            if (this.msgTmp.Visibility == Visibility.Hidden)
            {
                if (textValue.Text.Length == 1)
                {
                    //Comienzo animación para reducir textbox
                    (this.Resources["kRedBox"] as
                    System.Windows.Media.Animation.Storyboard).Begin();
                    this.btnClear.Visibility = Visibility.Visible;

                   
                }
            }
            ///En dado caso que no, debe de permanecer oculto
            else
            {
                this.btnClear.Visibility = Visibility.Hidden;
                (this.Resources["kExpBox"] as
                System.Windows.Media.Animation.Storyboard).Begin();
            }
            if (TextChanged != null)
                TextChanged(sender, e);
        }

        private void input_validate(object sender, TextCompositionEventArgs e)
        {
            Double num;
            if (InputType == TextboxInputScope.Number && !Double.TryParse(e.Text, out num))
                e.Handled = true;
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            this.Text = String.Empty;
        }
    }
}
