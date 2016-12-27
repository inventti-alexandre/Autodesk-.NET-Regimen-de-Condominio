using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RegimenCondominio.M
{
    public class DatosColindancia
    {                

        public Handle HndPlColindancia { get; set; }

        public Handle HndTxtColindancia { get; set; }

        public string TextColindancia { get; set; }

        public string InicialRumbo { get; set; }

        public string RumboActual { get; set; }
    }
}
