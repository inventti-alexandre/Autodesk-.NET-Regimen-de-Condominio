using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RegimenCondominio.C
{
    public class Inicio
    {
        public static string FormateaString(string OracionMayuscula)
        {            
            StringBuilder UnirPalabra = new StringBuilder();

            string[] OracionSeparada = OracionMayuscula.Split(' ');

            for (int i = 0; i < OracionSeparada.Count(); i++)
            {
                string Palabra = OracionSeparada[i];


                if (Palabra != string.Empty)
                {
                    //Envío a minusculas
                    Palabra = Palabra.ToLower();

                    //Obtengo la inicial 
                    if (Array.IndexOf(M.Inicio.PalabrasOmitiadas, Palabra.ToUpper()) == -1//Que no se encuentre dentro del array
                        || i == 0)//O que sea la primera palabra
                    {
                        string Inicial = Palabra[0].ToString();

                        Inicial = Inicial.ToUpper();

                        Palabra = Palabra.Remove(0, 1);

                        Palabra = Inicial + Palabra;

                    }

                    if (i < OracionSeparada.Count() - 1)
                        Palabra = Palabra + " ";

                    UnirPalabra.Append(Palabra);
                }
            }

            return UnirPalabra.ToString();
        }
    }
}
