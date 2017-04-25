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
        private static List<Fraccionamiento> resultFraccs;

        //Resultado de Tipo de Viviendas de consulta SQL
        private static List<EncabezadoMachote> resultTipoVivs;        

        #region DatosIniciales

        //Fraccionamiento seleccionado en Módulo Inicio
        private static M.Fraccionamiento fraccionamiento;

        //estado seleccionado en Módulo Inicio
        private static string estado;

        private static string municipio;

        private static string region;

        private static string sector;

        private static M.EncabezadoMachote encMachote;

        #endregion

        private static bool isOpen = new bool();

        public static List<Fraccionamiento> ResultFraccs
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

        public static List<EncabezadoMachote> ResultTipoVivs
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


        public static M.Fraccionamiento Fraccionamiento
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

        public static M.EncabezadoMachote EncMachote
        {
            get
            {
                return encMachote;
            }

            set
            {
                encMachote = value;
            }
        }

        public static bool IsOpen
        {
            get
            {
                return isOpen;
            }

            set
            {
                isOpen = value;
            }
        }
    }
}
