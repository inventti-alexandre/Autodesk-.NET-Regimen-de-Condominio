using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RegimenCondominio.M
{

    public class DatosFracc
    {
        public string Fraccionamiento { get; set; }
        public string Estado { get; set; }
        public string Municipio { get; set; }
    }

    public class DatosManzana
    {

        public Handle HndPlColindancia { get; set; }

        public Handle HndTxtColindancia { get; set; }

        public string TextColindancia { get; set; }

        public string InicialRumbo { get; set; }

        public string RumboActual { get; set; }
    }

    public class DatosTipoViv
    {
        public string NombreTipoViv { get; set; }

        public int IdTipoViv { get; set; }
    }

    public class DatosColindancia
    {
        public string Lote { get; set; }

        public string Apartamento { get; set; }

        public string Seccion{ get; set; }

        public int PuntoA { get; set; }

        public int PuntoB { get; set; }

        public double Distancia { get; set; }

        public string Rumbo { get; set; }

        public string Colindancia { get; set; }    
        
        public string NoOficial { get; set; }
    }

    public class DGVColindancia
    {
        public string Fraccionamiento { get; set; }
        public string TipoViv { get; set; }
        public int Manzana { get; set; }
        public int NoOficial { get; set; }
    }

    public class EntityValue
    {
        public ObjectId idEntity { get; set; }
        public string value { get; set; }
    }

    public class DescribeError
    {
        public string Error { get; set; }

        public string Description { get; set; }

        public ObjectId idObject { get; set; }

        public DateTime timeError { get; set; }
    }
}
