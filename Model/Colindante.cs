using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RegimenCondominio.M
{
    public class Colindante
    {
        private static ObjectId idPolManzana = new ObjectId();

        /// <summary>
        /// (Tab Lote) Id de selección de Lote Tipo
        /// </summary>
        private static ObjectId idLoteTipo = new ObjectId();

        /// <summary>
        /// (Tab Lote) Colección de Selección de Lotes irregulares
        /// </summary>
        private static ObjectIdCollection idsLotesIrregulares = new ObjectIdCollection();        

        /// <summary>
        /// Id de selección de Macrolote
        /// </summary>
        private static ObjectId idMacrolote = new ObjectId();

        /// <summary>
        /// Colección de Lotes dentro de Manzana.
        /// </summary>
        private static List<EntityValue> valorEdificio = new List<EntityValue>();

        private static List<EntityValue> valorLotes = new List<EntityValue>();

        private static string layerTipo = Manzana.EsMacrolote ? Constant.LayerEdificio : Constant.LayerLote;

        private static List<string> secciones = new List<string>(new string[]
        {
            Constant.LayerAPBaja,
            Constant.LayerAPAlta,
            Constant.LayerLavanderia,
            Constant.LayerEstacionamiento,
            Constant.LayerPasillo,
            Constant.LayerPatio
        });

        private static List<DescribeError> listadoErrores = new List<DescribeError>();

        private static List<DatosColindancia> mainData = new List<DatosColindancia>();

        /// <summary>
        /// (Tab Lote) Id de selección de Lote Tipo
        /// </summary>
        public static ObjectId IdLoteTipo
        {
            get
            {
                return idLoteTipo;
            }

            set
            {
                idLoteTipo = value;
            }
        }

        /// <summary>
        /// (Tab Lote) Colección de Selección de Lotes irregulares
        /// </summary>
        public static ObjectIdCollection IdsLotesIrregulares
        {
            get
            {
                return idsLotesIrregulares;
            }

            set
            {
                idsLotesIrregulares = value;
            }
        }

        /// <summary>
        /// Colección de Lotes/Edificios dentro de Manzana.
        /// </summary>        
        public static List<EntityValue> ValorEdificio
        {
            get
            {
                return valorEdificio;
            }

            set
            {
                valorEdificio = value;
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
                return secciones;
            }

            set
            {
                secciones = value;
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

        public static List<EntityValue> ValorLotes
        {
            get
            {
                return valorLotes;
            }

            set
            {
                valorLotes = value;
            }
        }

        public static List<DescribeError> ListadoErrores
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
                return layerTipo;
            }

            set
            {
                layerTipo = value;
            }
        }
    }
}
