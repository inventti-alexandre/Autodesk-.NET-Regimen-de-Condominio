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
        private static List<string> calcOrientaciones = new List<string>();

        public static List<string> CalcOrientaciones
        {
            get
            {
                return calcOrientaciones;
            }

            set
            {
                calcOrientaciones = value;
            }
        }
    }
}
