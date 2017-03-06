using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RegimenCondominio.M
{

    public class Fraccionamiento
    {
        public string fraccionamiento { get; set; }
        public string estado { get; set; }
        public string municipio { get; set; }
    }


    public class AreaComun
    {
        public long _longLote { get; set; }

        public long _longAreaComun { get; set; }

        public string nombreAreaComun { get; set; }
    }
    public class ManzanaData
    {

        public Handle hndPlColindancia { get; set; }

        public Handle hndTxtColindancia { get; set; }

        public string textColindancia { get; set; }

        public string inicialRumbo { get; set; }

        public string rumboActual { get; set; }
    }

    public class EncabezadoMachote
    {
        public int IdMachote { get; set; }

        public string Encabezado { get; set; }

        public int Cant_Viviendas { get; set; }


    }



    public class ColindanciaData
    {
        public int Edificio_Lote { get; set; }

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

        public bool esEsquinaA { get; set; }

        public bool esEsquinaB { get; set; }

        public bool esArco { get; set; }

        public long idVivienda { get; set; }
    }

    public class Lote
    {
        /// <summary>
        /// ObjectId de Entidad
        /// </summary>
        public long _long { get; set; }

        /// <summary>
        /// Valor de la Entidad
        /// </summary>
        public int numLote { get; set; }

        /// <summary>
        /// Llave que identifica que valor se introdujo
        /// </summary>
        public string numOficial { get; set; }
    }

    public class InEdificios
    {
        public long _long { get; set; }

        public int numEdificio { get; set; }
        
        public List<long> Apartments { get; set; }
    }

    public class Apartments
    {
        public long longPl { get; set; }

        public long longText { get; set; }

        public string TextAp { get; set; }
    }

    public class Error
    {
        public string error { get; set; }

        public string description { get; set; }

        public long longObject { get; set; }

        public string timeError { get; set; }

        public TipoError tipoError { get; set; }

        public string metodo { get; set; }
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

    public enum Grades
    {
        G0,
        G90,
        G180,
        G270
    }

    public enum TableInfo
    {
        Line,
        Arc
    }
}
