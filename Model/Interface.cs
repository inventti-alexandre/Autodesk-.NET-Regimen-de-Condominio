using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RegimenCondominio.M
{

    public class Fraccionamiento
    {
        public string Id_Fracc { get; set; }
        public string fraccionamiento { get; set; }
        public string Estado { get; set; }
        public string Municipio { get; set; }
    }    

    public class AreaComun
    {
        public long LongLote { get; set; }

        public long LongAreaComun { get; set; }

        public string NombreAreaComun { get; set; }

        public double AreaPl { get; set; }
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

    public class Bloques
    {
        public int IdBloque { get; set; }

        public string Descripcion { get; set; }

        public int IdTipoBloque { get; set; }

        public string NomTipoBLoque { get; set; }

        public int Orden { get; set; }
    }

    public class Variables
    {
        public string NombreCorto { get; set; }

        public string NombreVariable { get; set; }

        public string Valor { get; set; }

        public bool EsCalculado { get; set; }

        public bool ConvLetra { get; set; }

        public string NomTipoBloque { get; set; }

        public string NomCortoUnidad { get; set; }

        public string RepUnidad { get; set; }

        public string DescUnidad { get; set; }
    }

    public class DataColumns
    {        

        public Type TipoEnumerador { get; set; }

        public string PropertyName { get; set; }

        public string Descripcion { get; set; }        

        public string Layerseccion { get; set; }

        public bool esVisible { get; set; }       
    }

    public enum DetailColumns
    {
        CPlantaBaja,
        CPlantaAlta,
        CLavanderia,
        CEstacionamiento,
        CPasillo,
        CPatio,
        AreaTotalCubierta,
        DLavanderia,
        DEstacionamiento,
        DPasillo,
        DPatio,
        AreaTotalDescubierta,
        AreaCubiertaDescubierta,
        NombreAreaComun,
        AreaExclusiva,
        Proindiviso,
        PredioFrente,
        PredioFondo,
        PredioArea,
        AreaConstruccion,
        ExpedienteCatastral
    } 

    public enum HeaderColumns
    {
        Calle,
        NoOficial,
        NumEdificio,
        Apartamento
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
        Advertencia,
        Error,
        Informativo
    }

    public class SegmentInfo
    {
        public double Distance { get; set; }

        public Point3d MiddlePoint { get; set; }

        public Point3d StartPoint { get; set; }

        public Point3d EndPoint { get; set; }

        public bool isArc { get; set; }
    }   

    public class LoteItem
    {
        public string Name { get; set; }

        public long Long { get; set; }

        public string TipoLote { get; set; }

        public bool EsLoteBase { get; set; }
    }

    public class LoteDetail
    {
        //int, long, int, string, string           

        public int NumLote { get; set; }

        public long LongLote { get; set; }

        public int NumEdificio { get; set; }

        public string NumApartamento { get; set; }

        public string NumOficial { get; set; }
    }

    public class Medidas : INotifyPropertyChanged
    {

        private long _longLote;

        private int _numLote,
                    _numEdificio;

        private string  _manzana,                        
                        _calle,
                        _noOficial,
                        _numApartamento,
                        _cPlantaBaja,
                        _cPlantaAlta,
                        _cLavanderia,
                        _cEstacionamiento,
                        _cPasillo,
                        _cPatio,
                        _AreaTotalCubierta,
                        _dLavanderia,
                        _dEstacionamiento,
                        _dPasillo,
                        _dPatio,
                        _areaTotalDescubierta,
                        _areaCubiertaDescubierta,
                        _nombreAreaComun,
                        _areaExclusiva,                        
                        _proindiviso,
                        _predioFrente,
                        _predioFondo,
                        _predioArea,
                        _areaConstruccion,
                        _expedienteCatastral;

        public Medidas() { }

        //public Medidas(string manzana, int numLote, string calle,
        //    int numEdificio, string numOficial, string apartamento, long longLote)
        //{
        //    this._manzana = manzana;
        //    this._numLote = numLote;
        //    this._calle = calle;
        //    this._numEdificio = numEdificio;
        //    this._noOficial = numOficial;
        //    this._numApartamento = apartamento;
        //    this._longLote = longLote;
        //}
        public string Manzana
        {
            get
            {
                return _manzana;
            }

            set
            {
                _manzana = value; NotifyPropertyChanged("Manzana");
            }
        }

        public int NumLote
        {
            get
            {
                return _numLote;
            }

            set
            {
                _numLote = value; NotifyPropertyChanged("NumLote");
            }
        }

        public string Calle
        {
            get
            {
                return _calle;
            }

            set
            {
                _calle = value;
                NotifyPropertyChanged("Calle");
            }
        }

        public int NumEdificio
        {
            get
            {
                return _numEdificio;
            }

            set
            {
                _numEdificio = value;
                NotifyPropertyChanged("NumEdificio");
            }
        }

        public string NoOficial
        {
            get
            {
                return _noOficial;
            }

            set
            {
                _noOficial = value;
                NotifyPropertyChanged("NoOficial");
            }
        }

        public string Apartamento
        {
            get
            {
                return _numApartamento;
            }

            set
            {
                _numApartamento = value;
                NotifyPropertyChanged("Apartamento");
            }
        }

        public string CPlantaBaja
        {
            get
            {
                return _cPlantaBaja;
            }

            set
            {
                _cPlantaBaja = value;
                NotifyPropertyChanged("CPlantaBaja");
            }
        }

        public string CPlantaAlta
        {
            get
            {
                return _cPlantaAlta;
            }

            set
            {
                _cPlantaAlta = value;
                NotifyPropertyChanged("CPlantaAlta");
            }
        }

        public string CEstacionamiento
        {
            get
            {
                return _cEstacionamiento;
            }

            set
            {
                _cEstacionamiento = value;
                NotifyPropertyChanged("CEstacionamiento");
            }
        }

        public string CPasillo
        {
            get
            {
                return _cPasillo;
            }

            set
            {
                _cPasillo = value;
                NotifyPropertyChanged("CPasillo");
            }
        }

        public string CLavanderia
        {
            get
            {
                return _cLavanderia;
            }

            set
            {
                _cLavanderia = value;
                NotifyPropertyChanged("CLavanderia");
            }
        }

        public string CPatio
        {
            get
            {
                return _cPatio;
            }

            set
            {
                _cPatio = value;
                NotifyPropertyChanged("CPatio");
            }
        }

        public string AreaTotalCubierta
        {
            get
            {
                return _AreaTotalCubierta;
            }

            set
            {
                _AreaTotalCubierta = value;
                NotifyPropertyChanged("AreaTotalCubierta");
            }
        }

        public string DEstacionamiento
        {
            get
            {
                return _dEstacionamiento;
            }

            set
            {
                _dEstacionamiento = value;
                NotifyPropertyChanged("DEstacionamiento");
            }
        }

        public string DPasillo
        {
            get
            {
                return _dPasillo;
            }

            set
            {
                _dPasillo = value;
                NotifyPropertyChanged("DPasillo");
            }
        }

        public string DLavanderia
        {
            get
            {
                return _dLavanderia;
            }

            set
            {
                _dLavanderia = value;
                NotifyPropertyChanged("DLavanderia");
            }
        }

        public string DPatio
        {
            get
            {
                return _dPatio;
            }

            set
            {
                _dPatio = value;
                NotifyPropertyChanged("DPatio");
            }
        }

        public string AreaTotalDescubierta
        {
            get
            {
                return _areaTotalDescubierta;
            }

            set
            {
                _areaTotalDescubierta = value;
                NotifyPropertyChanged("AreaTotalDescubierta");
            }
        }

        public string AreaCubiertaDescubierta
        {
            get
            {
                return _areaCubiertaDescubierta;
            }

            set
            {
                _areaCubiertaDescubierta = value; NotifyPropertyChanged("AreaCubiertaDescubierta");
            }
        }

        public string NombreAreaComun
        {
            get
            {
                return _nombreAreaComun;
            }

            set
            {
                _nombreAreaComun = value;
                NotifyPropertyChanged("NombreAreaComun");
            }
        }

        public string AreaExclusiva
        {
            get
            {
                return _areaExclusiva;
            }

            set
            {
                _areaExclusiva = value; NotifyPropertyChanged("AreaExclusiva");
            }
        }

        public string Proindiviso
        {
            get
            {
                return _proindiviso;
            }

            set
            {
                _proindiviso = value;
                NotifyPropertyChanged("Proindiviso");
            }
        }

        public string PredioFrente
        {
            get
            {
                return _predioFrente;
            }

            set
            {
                _predioFrente = value;
                NotifyPropertyChanged("PredioFrente");
            }
        }

        public string PredioFondo
        {
            get
            {
                return _predioFondo;
            }

            set
            {
                _predioFondo = value;
                NotifyPropertyChanged("PredioFondo");
            }
        }

        public string PredioArea
        {
            get
            {
                return _predioArea;
            }

            set
            {
                _predioArea = value;
                NotifyPropertyChanged("PredioArea");
            }
        }

        public string AreaConstruccion
        {
            get
            {
                return _areaConstruccion;
            }

            set
            {
                _areaConstruccion = value; NotifyPropertyChanged("AreaConstruccion");
            }
        }

        public string ExpedienteCatastral
        {
            get
            {
                return _expedienteCatastral;
            }

            set
            {
                _expedienteCatastral = value;
                NotifyPropertyChanged("ExpedienteCatastral");
            }
        }

        public long LongLote
        {
            get
            {
                return _longLote;
            }

            set
            {
                _longLote = value;
                NotifyPropertyChanged("LongLote");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// Notifies the property changed.
        /// </summary>
        /// <param name="property">The info.</param>
        public void NotifyPropertyChanged(string property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }
    }

    public class Totales : INotifyPropertyChanged
    {       
        private decimal total;
        private DetailColumns columna;

        public decimal Total
        {
            get
            {
                return total;
            }
            set
            {
                if (value != total)
                {
                    total = value;
                    NotifyPropertyChanged("Total");
                }
            }
        }

        public M.DetailColumns Columna
        {
            get
            {
                return columna;
            }
            set
            {
                if (value != columna)
                {
                    columna = value;
                    NotifyPropertyChanged("Columna");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }
    }

    public class TotalesMedidas
    {

        private Totales totalCubPB = new Totales() { Columna = DetailColumns.CPlantaBaja },
                        totalCubPA = new Totales() { Columna = DetailColumns.CPlantaAlta },
                        totalCubEstac = new Totales() { Columna = DetailColumns.CEstacionamiento },
                        totalCubPasillo = new Totales() { Columna = DetailColumns.CPasillo },
                        totalCubLavanderia = new Totales() { Columna = DetailColumns.CLavanderia },
                        totalCubPatio = new Totales() { Columna = DetailColumns.CPatio },
                        totalAreaCub = new Totales() { Columna = DetailColumns.AreaTotalCubierta },
                        totalDescEstac = new Totales() { Columna = DetailColumns.DEstacionamiento },
                        totalDescPasillo = new Totales() { Columna = DetailColumns.DPasillo },
                        totalDescLavanderia = new Totales() { Columna = DetailColumns.DLavanderia },
                        totalDescPatio = new Totales() { Columna = DetailColumns.DPatio },
                        totalAreaDesc = new Totales() { Columna = DetailColumns.AreaTotalDescubierta },
                        totalAreaCubDesc = new Totales() { Columna = DetailColumns.AreaCubiertaDescubierta },
                        totalAreaComun = new Totales() { Columna = DetailColumns.NombreAreaComun },
                        totalAreaExclusiva = new Totales() { Columna = DetailColumns.AreaExclusiva },
                        totalProindiviso = new Totales() { Columna = DetailColumns.Proindiviso },
                        totalAreaConst = new Totales() { Columna = DetailColumns.AreaConstruccion };


        public TotalesMedidas() { }        
       
        public Totales TotalCubPB {

            get{
                return totalCubPB;
            }
            set {
                if(value.Total != totalCubPB.Total)                
                    totalCubPB = value;                
            }
        }

        public Totales TotalCubPA {

            get
            {
                return totalCubPA;
            }
            set
            {
                if (value.Total != totalCubPA.Total)
                    totalCubPA = value;
            }
        }

        public Totales TotalCubEstac {

            get
            {
                return totalCubEstac;
            }
            set
            {
                if (value.Total != totalCubEstac.Total)
                    totalCubEstac = value;
            }
        }

        public Totales TotalCubPasillo {

            get
            {
                return totalCubPasillo;
            }
            set
            {
                if (value.Total != totalCubPasillo.Total)
                    totalCubPasillo = value;
            }
        }

        public Totales TotalCubLavanderia {

            get
            {
                return totalCubLavanderia;
            }
            set
            {
                if (value.Total != totalCubLavanderia.Total)
                    totalCubLavanderia = value;
            }
        }

        public Totales TotalCubPatio {

            get
            {
                return totalCubPatio;
            }
            set
            {
                if (value.Total != totalCubPatio.Total)
                    totalCubPatio = value;
            }
        }

        public Totales TotalAreaCub {

            get
            {
                return totalAreaCub;
            }
            set
            {
                if (value.Total != totalAreaCub.Total)
                    totalAreaCub = value;
            }
        }

        public Totales TotalDescEstac {

            get
            {
                return totalDescEstac;
            }
            set
            {
                if (value.Total != totalDescEstac.Total)
                    totalDescEstac = value;
            }
        }

        public Totales TotalDescPasillo
        {
            get
            {
                return totalDescPasillo;
            }
            set
            {
                if (value.Total != totalDescPasillo.Total)
                    totalDescPasillo = value;
            }
        }

        public Totales TotalDescLavanderia
        {
            get
            {
                return totalDescLavanderia;
            }
            set
            {
                if (value.Total != totalDescLavanderia.Total)
                    totalDescLavanderia = value;
            }

        }

        public Totales TotalDescPatio
        {
            get
            {
                return totalDescPatio;
            }
            set
            {
                if (value.Total != totalDescPatio.Total)
                    totalDescPatio = value;
            }
        }

        public Totales TotalAreaDesc
        {
            get
            {
                return totalAreaDesc;
            }
            set
            {
                if (value.Total != totalAreaDesc.Total)
                    totalAreaDesc = value;
            }

        }

        public Totales TotalAreaCubDesc
        {
            get
            {
                return totalAreaCubDesc;
            }
            set
            {
                if (value.Total != totalAreaCubDesc.Total)
                    totalAreaCubDesc = value;
            }

        }

        public Totales TotalAreaComun
        {
            get
            {
                return totalAreaComun;
            }
            set
            {
                if (value.Total != totalAreaComun.Total)
                    totalAreaComun = value;
            }
        }

        public Totales TotalAreaExclusiva
        {
            get
            {
                return totalAreaExclusiva;
            }
            set
            {
                if (value.Total != totalAreaExclusiva.Total)
                    totalAreaExclusiva = value;
            }

        }

        public Totales TotalProindiviso
        {
            get
            {
                return totalProindiviso;
            }
            set
            {
                if (value.Total != totalProindiviso.Total)
                    totalProindiviso = value;
            }
        }

        public Totales TotalAreaConst
        {
            get
            {
                return totalAreaConst;
            }
            set
            {
                if (value.Total != totalAreaConst.Total)
                    totalAreaConst = value;
            }
        }
    }

    public class Checked<T> : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private bool isChecked;
        private T item;

        public Checked()
        { }

        public Checked(T item, bool isChecked = false)
        {
            this.item = item;
            this.isChecked = isChecked;
        }

        public T Item
        {
            get { return item; }
            set
            {
                item = value;
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("Item"));
            }
        }


        public bool IsChecked
        {
            get { return isChecked; }
            set
            {
                isChecked = value;
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("IsChecked"));
            }
        }
    }
}
