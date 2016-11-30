using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RegimenCondominio.M
{
    public class Inicio
    {
        //Resultado de Fraccionamientos de consulta SQL
        private static List<DatosFracc> resultFraccs;

        //Resultado de Tipo de Viviendas de consulta SQL
        private static List<DatosTipoViv> resultTipoVivs;


        #region DatosIniciales

        //Fraccionamiento seleccionado en Módulo Inicio
        private static string fraccionamiento;

        //estado seleccionado en Módulo Inicio
        private static string estado;

        private static string municipio;

        private static string region;

        private static string sector;

        private static string tipoViv;

        #endregion

        public static List<DatosFracc> ResultFraccs
        {
            get
            {
                return resultFraccs;
            }

            set
            {
                resultFraccs = value;
            }
        }

        public static List<DatosTipoViv> ResultTipoVivs
        {
            get
            {
                return resultTipoVivs;
            }

            set
            {
                resultTipoVivs = value;
            }
        }


        public static string Fraccionamiento
        {
            get
            {
                return fraccionamiento;
            }

            set
            {
                fraccionamiento = value;
            }
        }

        public static string Estado
        {
            get
            {
                return estado;
            }

            set
            {
                estado = value;
            }
        }

        public static string Municipio
        {
            get
            {
                return municipio;
            }

            set
            {
                municipio = value;
            }
        }

        public static string Region
        {
            get
            {
                return region;
            }

            set
            {
                region = value;
            }
        }

        public static string Sector
        {
            get
            {
                return sector;
            }

            set
            {
                sector = value;
            }
        }

        public static string TipoViv
        {
            get
            {
                return tipoViv;
            }

            set
            {
                tipoViv = value;
            }
        }

        
    }
}
