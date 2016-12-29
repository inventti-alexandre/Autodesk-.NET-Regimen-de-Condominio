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
        public string Lote { get; set; }

        public string Edificio { get; set; }

        public string Apartamento { get; set; }

        public string Seccion{ get; set; }

        public int PuntoA { get; set; }

        public int PuntoB { get; set; }

        public double Distancia { get; set; }

        public string Rumbo { get; set; }

        public string Colindancia { get; set; }        
    }
}
