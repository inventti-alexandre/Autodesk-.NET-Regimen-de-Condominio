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
        private static List<DatosColindancia> colindanciaManzana = new List<DatosColindancia>();

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

        public static List<DatosColindancia> ColindanciaManzana
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
    }
}
