using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RegimenCondominio.C
{
    public class Met_Manzana
    {
        /// <summary>
        /// Obtengo la lista de acuerdo a Rumbo de Frente
        /// </summary>
        /// <param name="RumboFrente">Rumbo de Frente de Manzana</param>
        /// <returns>Lista de Rumbo</returns>
        public static List<string> OrientacionFrente(string RumboFrente)
        {            
            List<string> OrientacionesFinales = new List<string>();

            int PrimerDimension = ObtengoPosicion(RumboFrente, 0);

            //Obtengo tipo de orientación (0 o 1)
            int TipoOrientacion = (M.ConstantValues.Orientaciones[PrimerDimension, 1]).Count();

            List<string> l = Obtengolista(TipoOrientacion);

            return CalculaRutaOrientacion(l, RumboFrente);
        }

        /// <summary>
        /// Itero hasta que encuentro la posición dentro del valor de Arreglo de 2 dimensiones
        /// </summary>
        /// <param name="ValorComparar">Valor a encontrar dentro del array</param>
        /// <param name="DimensionEstatica">Dimension del arreglo no cambiante</param>
        /// <returns></returns>
        public static int ObtengoPosicion(string ValorComparar, int DimensionEstatica)
        {
            string ValorActual = "";
            int PrimerDimension = 0;

            do
            {
                ValorActual = M.ConstantValues.Orientaciones[PrimerDimension, DimensionEstatica];

                if (PrimerDimension + 1 <= M.ConstantValues.Orientaciones.GetLength(DimensionEstatica))
                    PrimerDimension++;
            }
            while (ValorComparar != ValorActual);

            return PrimerDimension - 1;
        }

        public static int ObtengoPosicion(string ValorComparar, List<string> listColindancia)
        {            
            int PrimerDimension = -1;

            foreach(string item in listColindancia)
            {
                if (string.Equals(item, ValorComparar))
                    PrimerDimension = listColindancia.LastIndexOf(item);
            }

            return PrimerDimension;
        }

        private static List<string> CalculaRutaOrientacion(List<string> l, string rumboFrente)
        {
            //Reviso la posición dentro de la variable
            int index = l.IndexOf(rumboFrente);

            List<string> listaOrdenada = new List<string>();

            //Realizo array hacia adelante
            for(int i = index; i < l.Count; i++)
            {
                listaOrdenada.Add(l[i]);
            }

            //Realizo array hacia atras
            for (int j = 0; j < index; j++)
            {
                listaOrdenada.Add(l[j]);
            }

            return listaOrdenada;
        }

        private static List<string> Obtengolista(int tipoOrientacion)
        {
            List<string> ListaOrienta = new List<string>();
                        
            for(int i=0; i < M.ConstantValues.Orientaciones.GetLength(0); i++)
            {
                if (M.ConstantValues.Orientaciones[i, 1].Count() == tipoOrientacion)
                {
                    ListaOrienta.Add(M.ConstantValues.Orientaciones[i, 0]);
                }
            }

            return ListaOrienta;
        }

        internal static List<string> DespliegoOrientaciones()
        {
            List<string> todasOrientaciones = new List<string>();

            for(int i=0; i< M.ConstantValues.Orientaciones.GetLength(0); i++)
            {
                todasOrientaciones.Add(M.ConstantValues.Orientaciones[i, 0]);
            }

            return todasOrientaciones;
        }
        
    }
}
