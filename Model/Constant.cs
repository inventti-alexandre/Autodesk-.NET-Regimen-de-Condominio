using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RegimenCondominio.M
{
    public static class Constant
    {
        //Tipos de lineas aceptadas en la selección de la línea de colindancia.
        private static Type[] tiposLineas = new Type[]
        {
            typeof(Polyline),
            typeof(Line),
            typeof(Arc)        
        };

        private static bool isAutoClose = false;

        //Orden de búsqueda de los layers dentro de los Apartamentos
        private static List<string> busquedaApartamento = new List<string>(new string[] {            
            layerAPBaja,
            layerAPAlta,
            layerLavanderia,
            layerEstacionamiento,
            layerPasillo,
            layerPatio        
        });

       // private static List<DescribeLayer> todosLayers = 

        //Nombre del Record del diccionario de Datos de Colindancia
        private static string xRecordColindancia = "JaverColindancia";

        //Nombre del Record del diccionario de Número de Puntos (DBPoints)
        private static string xRecordPoints = "JaverPoints";

        //Palabras que no se envían a mayuscula en Método C.Met_Inicio.FormateString
        private static string[] palabrasOmitidas = new string[]{
                "DE",
                "DEL",
                "LA",
                "LAS",
                "LOS",
                "EL"
            };

        //Arreglo del Alfabeto
        private static string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        //Cantidad de Rumbos maximos
        private static int rumboMaximo = 4;

        //Usuario a Mostrar obtenido de sesión de Windows
        private static string usuario = Environment.UserName.ToUpper();

        //Orientaciones disponibles en Módulo Manzana
        private static string[,] orientaciones = new string[,]
        {
           { "Norte",   "N"},//0,0 - 0,1 -
           { "Este",   "E"},//1,0 - 1,1
           { "Sur",  "S"},//2,0 - 2,1
           { "Oeste",  "O"},//3,0 - 3,1
           { "Noreste", "NE" },//4,0 - 4,1
           { "Sureste", "SE" },//5,0 - 5,1
           { "Suroeste", "SO"},//6,0 - 6,1
           { "Noroeste", "NO" }//7,0 - 7,1
        };

        private static List<string> listError = new List<string>()
        {
            "Id",
            "Hora",
            "Error",
            "Descripción",
            "Tipo de Error",
            "Método"
        };

        

        private static List<string> tipoColindancias = new List<string>()
        {
            "Calle",
            "Otro..."
        };

        #region Layers

        private static string layerNoOficial = "NUM_OFICIAL";

        //Manzana
        private static string layerManzana = "MANZANA";

        //Número de Manzana
        private static string layerNumManzana = "NUM_MANZANA";        

        //Lote
        private static string layerLote = "LOTE";

        //Edificio
        private static string layerEdificio = "EDIFICIO";

        //Apartamento
        private static string layerApartamento = "APARTAMENTO";
       
        private static string layerAPBaja = "AP_PB";

        private static string layerAPAlta = "AP_PA";

        //Lavandería cubierta
        private static string layerLavanderia = "LAVANDERIA";

        //Estacionamiento cubierto
        private static string layerEstacionamiento = "ESTACIONAMIENTO";

        //Pasillo
        private static string layerPasillo = "PASILLO";

        //Patio descubierto
        private static string layerPatio = "PATIO";

        private static string layerExcRegimen = "EXCLUSIVO_REGIMEN";

        private static string layerExcDBPoints = "EXCL_POINTS_REGIMEN";

        private static string layerExcDBText = "EXCL_NUMS_REGIMEN";

        private static string layerExcRumbos = "EXCL_RUMBOS_REGIMEN";

        private static string layerAreaComun = "AREA_COMUN";

        #endregion
        
        /// <summary>
        /// Filtro de la Manzana
        /// </summary>
        public static SelectionFilter ManzanaFilter
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
                                        new TypedValue((int)DxfCode.Start, RXClass.GetClass(typeof(DBText)).DxfName),
                                        new TypedValue((int)DxfCode.Operator,"or>"),
                                        new TypedValue((int)DxfCode.LayerName, LayerManzana),
                                        new TypedValue((int)DxfCode.Operator,"and>")
                                    });
            }
        }

        /// <summary>
        /// Palabras no tomadas en Cuenta en FormatString()
        /// </summary>
        public static string[] PalabrasOmitidas
        {
            get
            {
                return palabrasOmitidas;
            }
        }

        /// <summary>
        /// Usuario de Windows
        /// </summary>
        public static string Usuario
        {
            get { return usuario; }
        }

        /// <summary>
        /// Orientaciones Disponibles
        /// </summary>
        public static string[,] Orientaciones
        {
            get
            {
                return orientaciones;
            }
        }        

        /// <summary>
        /// Listado de Tipos de Colindancias
        /// </summary>
        public static List<string> TipoColindancias
        {
            get
            {
                return tipoColindancias;
            }
        }

        /// <summary>
        /// Tipo de Clases Líneas aceptadas
        /// </summary>
        public static Type[] TiposLineas
        {
            get
            {
                return tiposLineas;
            }
        }

        /// <summary>
        /// Nombre de Record de Colindancia
        /// </summary>
        public static string XRecordColindancia
        {
            get
            {
                return xRecordColindancia;
            }
        }

        /// <summary>
        /// Cantidad de Rumbos Máximos a Asignar
        /// </summary>
        public static int RumboMaximo
        {
            get
            {
                return rumboMaximo;
            }
        }

        /// <summary>
        /// Layer Manzana
        /// </summary>
        public static string LayerManzana
        {
            get
            {
                return layerManzana;
            }
        }

        /// <summary>
        /// Layer Lote
        /// </summary>
        public static string LayerLote
        {
            get
            {
                return layerLote;
            }
        }

        /// <summary>
        /// Layer Número de Manzana
        /// </summary>
        public static string LayerNumManzana
        {
            get
            {
                return layerNumManzana;
            }
        }

        /// <summary>
        /// Layer Edificio
        /// </summary>
        public static string LayerEdificio
        {
            get
            {
                return layerEdificio;
            }
        }

        /// <summary>
        /// Layer del Apartamento
        /// </summary>
        public static string LayerApartamento
        {
            get
            {
                return layerApartamento;
            }
        }

        /// <summary>
        /// Layer Lavandería
        /// </summary>
        public static string LayerLavanderia
        {
            get
            {
                return layerLavanderia;
            }
        }

        public static string LayerEstacionamiento
        {
            get
            {
                return layerEstacionamiento;
            }

            set
            {
                layerEstacionamiento = value;
            }
        }

        public static string LayerPasillo
        {
            get
            {
                return layerPasillo;
            }

            set
            {
                layerPasillo = value;
            }
        }

        public static string LayerPatio
        {
            get
            {
                return layerPatio;
            }

            set
            {
                layerPatio = value;
            }
        }

        /// <summary>
        /// Layer Apartamento Planta Baja
        /// </summary>
        public static string LayerAPBaja
        {
            get
            {
                return layerAPBaja;
            }

        }

        /// <summary>
        /// Layer Apartamento Planta Alta
        /// </summary>
        public static string LayerAPAlta
        {
            get
            {
                return layerAPAlta;
            }

        }

        /// <summary>
        /// Layer de Número Oficial
        /// </summary>
        public static string LayerNoOficial
        {
            get
            {
                return layerNoOficial;
            }
        }

        /// <summary>
        /// Arreglo de letras de abecedario
        /// </summary>
        public static string Alphabet
        {
            get
            {
                return alphabet;
            }
        }

        /// <summary>
        /// Orden asignado para busqueda de secciones
        /// </summary>
        public static List<string> BusquedaApartamento
        {
            get
            {
                return busquedaApartamento;
            }
        }

        /// <summary>
        /// XRecord de Número de DBPoint asignado
        /// </summary>
        public static string XRecordPoints
        {
            get
            {
                return xRecordPoints;
            }
        }

        public static string LayerExcRegimen
        {
            get
            {
                return layerExcRegimen;
            }
        }

        public static string LayerExcDBPoints
        {
            get
            {
                return layerExcDBPoints;
            }            
        }

        public static string LayerExcDBText
        {
            get
            {
                return layerExcDBText;
            }          
        }

        public static string LayerAreaComun
        {
            get
            {
                return layerAreaComun;
            }
        }       

        public static string LayerExcRumbos
        {
            get
            {
                return layerExcRumbos;
            }

            set
            {
                layerExcRumbos = value;
            }
        }

        public static List<string> ListError
        {
            get
            {
                return listError;
            }
        }

        public static bool IsAutoClose
        {
            get
            {
                return isAutoClose;
            }

            set
            {
                isAutoClose = value;
            }
        }
    }
}
