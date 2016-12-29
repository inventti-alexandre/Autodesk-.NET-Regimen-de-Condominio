using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RegimenCondominio.M
{
    public class Manzana
    {
        //Orientaciones asignadas al seleccionar Rumbo de Frente
        private static List<string> orientacionCalculada = new List<string>();

        //Manzanas obtenidas del plano con cierto criterio
        private static List<DatosManzana> colindanciaManzana = new List<DatosManzana>();

        //Número de Manzana
        private static int noManzana = new int();

        //Rumbo de Frente
        private static string rumboFrente = string.Empty;

        public static List<string> OrientacionCalculada
        {
            get
            {
                return orientacionCalculada;
            }

            set
            {
                orientacionCalculada = value;
            }
        }

        public static List<DatosManzana> ColindanciaManzana
        {
            get
            {
                return colindanciaManzana;
            }

            set
            {
                colindanciaManzana = value;
            }
        }

        public static int NoManzana
        {
            get
            {
                return noManzana;
            }

            set
            {
                noManzana = value;
            }
        }

        public static string RumboFrente
        {
            get
            {
                return rumboFrente;
            }

            set
            {
                rumboFrente = value;
            }
        }
    }
}
