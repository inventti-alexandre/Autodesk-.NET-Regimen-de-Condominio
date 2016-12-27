using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RegimenCondominio.M
{
    public class DatosManzanas
    {
        public Handle HndPlManzana { get; set; }

        public Handle HndTxtManzana { get; set; }

        public string Colindancia { get; set; }
    }
}
