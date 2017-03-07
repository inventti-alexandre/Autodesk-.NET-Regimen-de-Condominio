using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

    public class Medidas : INotifyPropertyChanged
    {

        private string _manzana = "",
                        _lote,
                        _calle,
                        _noOficial,
                        _apartamento,
                        _cPlantaBaja,
                        _cPlantaAlta,
                        _cLavanderia,
                        _cEstacionamiento,
                        _cPasillo,
                        _cPatio,
                        _AreaTotalCubierta,
                        _dPlantaBaja,
                        _dPlantaAlta,
                        _dLavanderia,
                        _dEstacionamiento,
                        _dPasillo,
                        _dPatio,
                        _areaTotalDescubierta,
                        _areaCubiertaDescubierta,
                        _areaExclusiva,
                        _nombreAreaComun,
                        _proindiviso,
                        _predioFrente,
                        _predioFondo,
                        _predioArea,
                        _areaConstruccion,
                        _expedienteCatastral;

        public string Apartamento
        {
            get
            {
                return _apartamento;
            }

            set
            {
                _apartamento = value;
                NotifyPropertyChanged("Apartamento");
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

        public string DPlantaAlta
        {
            get
            {
                return _dPlantaAlta;
            }

            set
            {
                _dPlantaAlta = value;
                NotifyPropertyChanged("DPlantaAlta");
            }
        }

        public string DPlantaBaja
        {
            get
            {
                return _dPlantaBaja;
            }

            set
            {
                _dPlantaBaja = value;
                NotifyPropertyChanged("DPlantaBaja");
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

        public string Lote
        {
            get
            {
                return _lote;
            }

            set
            {
                _lote = value; NotifyPropertyChanged("Lote");
            }
        }

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
}
