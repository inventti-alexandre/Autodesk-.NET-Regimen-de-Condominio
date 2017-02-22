using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RegimenCondominio.M
{
    public class Colindante
    {
        private static ObjectId idPolManzana = new ObjectId();

        /// <summary>
        /// Id de selección de Lote Tipo
        /// </summary>
        private static ObjectId idTipo = new ObjectId();

        /// <summary>
        /// Colección de Selección de Lotes irregulares
        /// </summary>
        private static ObjectIdCollection idsIrregulares = new ObjectIdCollection();        

        /// <summary>
        /// Id de selección de Macrolote
        /// </summary>
        private static ObjectId idMacrolote = new ObjectId();

        /// <summary>
        /// Colección de Lotes dentro de Manzana.
        /// </summary>
        private static List<InEdificios> edificios = new List<InEdificios>();

        private static List<InLotes> lotes = new List<InLotes>();                      

        private static ObservableCollection<DescribeError> listadoErrores = new ObservableCollection<DescribeError>();

        private static object _lock = new object();

        private static List<DatosColindancia> mainData = new List<DatosColindancia>();        

        private static List<Apartments> orderedApartments = new List<Apartments>();

        private static Point3dCollection ptsVertex = new Point3dCollection();

        private static int decimals = new int();

        private static int lastPoint = new int();

        private static List<M.AreaComun> listCommonArea = new List<M.AreaComun>();


        public static List<string> sourceComboError
        {
            get
            {
                DescribeError db = new DescribeError();
                List<string> properties = new List<string>();

                foreach (PropertyInfo prop in db.GetType().GetProperties())
                    properties.Add(prop.Name);

                return properties;
            }
        }

        /// <summary>
        /// Id de selección de Lote Tipo
        /// </summary>
        public static ObjectId IdTipo
        {
            get
            {
                return idTipo;
            }

            set
            {
                idTipo = value;
            }
        }

        /// <summary>
        /// (Tab Lote) Colección de Selección de Lotes irregulares
        /// </summary>
        public static ObjectIdCollection IdsIrregulares
        {
            get
            {
                return idsIrregulares;
            }

            set
            {
                idsIrregulares = value;
            }
        }

        /// <summary>
        /// Colección de Lotes/Edificios dentro de Manzana.
        /// </summary>        
        public static List<InEdificios> Edificios
        {
            get
            {
                return edificios;
            }

            set
            {
                edificios = value;
            }
        }

        /// <summary>
        /// Id de selección de Macrolote
        /// </summary>
        public static ObjectId IdMacrolote
        {
            get
            {
                return idMacrolote;
            }

            set
            {
                idMacrolote = value;
            }
        }

        public static List<string> Secciones
        {
            get
            {
                return new List<string>(new string[]
                    {
                        Constant.LayerAPBaja,
                        Constant.LayerAPAlta,
                        Constant.LayerLavanderia,
                        Constant.LayerEstacionamiento,
                        Constant.LayerPasillo,
                        Constant.LayerPatio                       
                    });
            }
        }

        /// <summary>
        /// Información Principal en Cálculo de Puntos
        /// </summary>
        public static List<DatosColindancia> MainData
        {
            get
            {
                return mainData;
            }

            set
            {
                mainData = value;
            }
        }

        public static List<InLotes> Lotes
        {
            get
            {
                return lotes;
            }

            set
            {
                lotes = value;
            }
        }

        public static ObservableCollection<DescribeError> ListadoErrores
        {
            get
            {
                return listadoErrores;
            }

            set
            {
                listadoErrores = value;
            }
        }

        public static ObjectId IdPolManzana
        {
            get
            {
                return idPolManzana;
            }

            set
            {
                idPolManzana = value;
            }
        }

        public static string LayerTipo
        {
            get
            {
                return Manzana.EsMacrolote ? Constant.LayerEdificio : Constant.LayerLote;
            }
        }

        public static List<DescribeLayer> TodosLayers
        {
            get
            {
                return C.Met_General.GetAllLayers();
            }
        }

        public static SelectionFilter LineFilter
        {
            get
            {
                //RXClass nos sirve para obtener el nombre del DXF en AutoCAD
                //El Start nos sirve para definir el tipo de entidad a filtrar
                return new SelectionFilter(
                    new TypedValue[]
                                    {
                                        new TypedValue((int)DxfCode.Operator,"<and"),
                                        new TypedValue((int)DxfCode.Operator,"<or"),
                                        new TypedValue((int)DxfCode.Start, RXClass.GetClass(typeof(Polyline)).DxfName),
                                        new TypedValue((int)DxfCode.Operator,"or>"),
                                        new TypedValue((int)DxfCode.Start, RXClass.GetClass(typeof(Line)).DxfName),
                                        new TypedValue((int)DxfCode.Operator,"and>")
                                    });
            }
        }

        public static object Lock
        {
            get
            {
                return _lock;
            }

            set
            {
                _lock = value;
            }
        }       

        public static int Decimals
        {
            get
            {
                return decimals;
            }

            set
            {
                decimals = value;
            }
        }

        public static Point3dCollection PtsVertex
        {
            get
            {
                return ptsVertex;
            }

            set
            {
                ptsVertex = value;
            }
        }

        public static List<Apartments> OrderedApartments
        {
            get
            {
                return orderedApartments;
            }

            set
            {
                orderedApartments = value;
            }
        }

        public static int LastPoint
        {
            get
            {
                return lastPoint;
            }

            set
            {
                lastPoint = value;
            }
        }

        public static List<M.AreaComun> ListCommonArea
        {
            get
            {
                return listCommonArea;
            }

            set
            {
                listCommonArea = value;
            }
        }
    }
}
