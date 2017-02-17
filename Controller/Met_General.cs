using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RegimenCondominio.M;

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

        internal static string JoinToWord(params string[] s)
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < s.Count(); i++)
            {
                string sActual = s[i];
                if (sActual != "")
                {
                    if (i + 1 < s.Count())
                        sb.Append(sActual).Append(" ");
                    else
                        sb.Append(sActual);
                }
            }

            return sb.ToString();
        }


        internal static List<DescribeLayer> GetAllLayers()
        {
            List<M.DescribeLayer> mainList = new List<DescribeLayer>();
           
            //Agrego las secciones
            foreach(string seccion in Colindante.Secciones)
            {
                string  layerName = seccion, 
                        description = "";

                //Planta Alta
                if (seccion == Constant.LayerAPAlta)
                    description = "Planta Alta";
                //Planta Baja
                else if (seccion == Constant.LayerAPBaja)
                    description = "Planta Baja";
                //Lavandería
                else if (seccion == Constant.LayerLavanderia)
                    description = "Lavandería";
                //Estacionamiento
                else if (seccion == Constant.LayerEstacionamiento)
                    description = "Estacionamiento";
                //Pasillo
                else if (seccion == Constant.LayerPasillo)
                    description = "Pasillo Descubierto";
                //Patio
                else if (seccion == Constant.LayerPatio)
                    description = "Patio";
                else if (seccion == Constant.LayerAreaComun)
                    description = "Área Común";
                //En cualquier otro caso asigno nombre de sección
                else
                    description = seccion;

                mainList.Add(new DescribeLayer()
                {
                    Layername = seccion,
                    Description = description
                });

            }

            //Lotes
            mainList.Add(new DescribeLayer()
            {
                Layername = Constant.LayerLote,
                Description = "Lote"
            });

            //Lotes
            mainList.Add(new DescribeLayer()
            {
                Layername = Constant.LayerEdificio,
                Description = "Edificio"
            });

            //Manzana
            mainList.Add(new DescribeLayer()
            {
                Layername = Constant.LayerManzana,
                Description = "Manzana"
            });

            //Apartamento
            mainList.Add(new DescribeLayer()
            {
                Layername = Constant.LayerApartamento,
                Description = "Apartamento"
            });

            return mainList;
        }

        internal static string GetAfterSpace(this string sentence)
        {
            if (sentence.Contains(" "))
            {
                string[] s = sentence.Split(' ');

                return s.LastOrDefault();
            }
            else
                return sentence;
        }

        public static string ToEnumerate(this int index)
        {
            int ColumnBase = Constant.Alphabet.Count();
            const int DigitMax = 7; // ceil(log26(Int32.Max))
            string Digits = Constant.Alphabet;

            if (index <= 0)
                throw new IndexOutOfRangeException("index must be a positive number");

            if (index <= ColumnBase)
                return Digits[index - 1].ToString();

            var sb = new StringBuilder().Append(' ', DigitMax);
            int current = index;
            int offset = DigitMax;
            while (current > 0)
            {
                sb[--offset] = Digits[--current % ColumnBase];
                current /= ColumnBase;
            }
            return sb.ToString(offset, DigitMax - offset);
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

        public static double Trunc(this double num, int decimals)
        {
            return Math.Floor(num * Math.Pow(10, decimals)) / Math.Pow(10, decimals);
        }
        

    }
}
