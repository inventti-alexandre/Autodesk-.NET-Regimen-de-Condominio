using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
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
        public string Vivienda { get; set; }

        public string Apartamento { get; set; }

        public string Seccion{ get; set; }

        public string LayerSeccion { get; set; }

        public int PuntoA { get; set; }

        public int PuntoB { get; set; }

        public double Distancia { get; set; }

        public string Rumbo { get; set; }

        public string Colindancia { get; set; }  
        
        public List<string> LayersColindancia { get; set; }  
        
        public string NoOficial { get; set; }

        public Point3d CoordenadaA { get; set; }

        public Point3d CoordenadaB { get; set; }

        public bool IsArc { get; set; }
    }

    public class DGVColindancia
    {
        public string Fraccionamiento { get; set; }
        public string TipoViv { get; set; }
        public int Manzana { get; set; }
        public int NoOficial { get; set; }
    }

    public class InLotes
    {
        /// <summary>
        /// ObjectId de Entidad
        /// </summary>
        public long handleEntity { get; set; }

        /// <summary>
        /// Valor de la Entidad
        /// </summary>
        public string numLote { get; set; }

        /// <summary>
        /// Llave que identifica que valor se introdujo
        /// </summary>
        public string numOficial { get; set; }
    }

    public class InEdificios
    {
        public long longEntity { get; set; }

        public string numEdificio { get; set; }
        
        public List<long> Apartments { get; set; }
    }

    public class Apartments
    {
        public long longPl { get; set; }

        public long longText { get; set; }

        public string TextAp { get; set; }
    }

    public class DescribeError
    {
        public string Error { get; set; }

        public string Description { get; set; }

        public long longObject { get; set; }

        public DateTime timeError { get; set; }

        public TipoError tipoError { get; set; }

        public string Metodo { get; set; }
    }

    public class DescribeLayer
    {
        public string Layername { get; set; }

        public string Description { get; set; }
    }

    public enum TipoError
    {
        Warning,
        Error,
        Info
    }

    public class SegmentInfo
    {
        public double Distance { get; set; }

        public Point3d MiddlePoint { get; set; }

        public Point3d StartPoint { get; set; }

        public Point3d EndPoint { get; set; }

        public bool isArc { get; set; }
    }

    public enum TipoLinea
    {
        Line,
        Arc
    }
}
