using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RegimenCondominio.C
{
    public static class Met_General
    {
        internal static string FormatString(this string OracionMayuscula)
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
                    if (Array.IndexOf(M.Constant.PalabrasOmitidas, Palabra.ToUpper()) == -1//Que no se encuentre dentro del array
                        || i == 0)//O que sea la primera palabra
                    {
                        //Obtengo primer caracter
                        string Inicial = Palabra[0].ToString();

                        //Envio a mayuscula
                        Inicial = Inicial.ToUpper();

                        //Remuevo primer caracter
                        Palabra = Palabra.Remove(0, 1);

                        //Uno nuevamente el caracter con mayuscula
                        Palabra = Inicial + Palabra;

                    }

                    if (i < OracionSeparada.Count() - 1)
                        Palabra = Palabra + " ";

                    UnirPalabra.Append(Palabra);
                }
            }

            return UnirPalabra.ToString();
        }

        public static string RowToString(this DataRow dtRow, char separator)
        {
            string row = "";
            for (int j = 0; j < dtRow.ItemArray.Length; j++)
            {
                row += dtRow.ItemArray[j].ToString();
                if (j != dtRow.ItemArray.Length - 1)
                    row += separator;
            }

            return row;
        }

    }
}
