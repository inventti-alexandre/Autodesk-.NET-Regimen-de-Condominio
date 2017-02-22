using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

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

        internal static List<M.DescribeLayer> GetAllLayers()
        {
            List<M.DescribeLayer> mainList = new List<M.DescribeLayer>();
           
            //Agrego las secciones
            foreach(string seccion in M.Colindante.Secciones)
            {
                string  layerName = seccion, 
                        description = "";

                //Planta Alta
                if (seccion == M.Constant.LayerAPAlta)
                    description = "Planta Alta";
                //Planta Baja
                else if (seccion == M.Constant.LayerAPBaja)
                    description = "Planta Baja";
                //Lavandería
                else if (seccion == M.Constant.LayerLavanderia)
                    description = "Lavandería";
                //Estacionamiento
                else if (seccion == M.Constant.LayerEstacionamiento)
                    description = "Estacionamiento";
                //Pasillo
                else if (seccion == M.Constant.LayerPasillo)
                    description = "Pasillo Descubierto";
                //Patio
                else if (seccion == M.Constant.LayerPatio)
                    description = "Patio";                
                //En cualquier otro caso asigno nombre de sección
                else
                    description = seccion;

                mainList.Add(new M.DescribeLayer()
                {
                    Layername = seccion,
                    Description = description
                });

            }

            //Lotes
            mainList.Add(new M.DescribeLayer()
            {
                Layername = M.Constant.LayerLote,
                Description = "Lote"
            });

            //Área Común
            mainList.Add(new M.DescribeLayer()
            {
                Layername = M.Constant.LayerAreaComun,
                Description = "Área Común"
            });

            //Lotes
            mainList.Add(new M.DescribeLayer()
            {
                Layername = M.Constant.LayerEdificio,
                Description = "Edificio"
            });

            //Manzana
            mainList.Add(new M.DescribeLayer()
            {
                Layername = M.Constant.LayerManzana,
                Description = "Manzana"
            });

            //Apartamento
            mainList.Add(new M.DescribeLayer()
            {
                Layername = M.Constant.LayerApartamento,
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
            int ColumnBase = M.Constant.Alphabet.Count();
            const int DigitMax = 7; // ceil(log26(Int32.Max))
            string Digits = M.Constant.Alphabet;

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

        public static string NumberToWord(this string num)
        {
            string res, dec = "";
            Int64 entero;
            int decimales;
            double nro;
            try
            {
                nro = Convert.ToDouble(num);
            }
            catch
            {
                return "";
            }
            entero = Convert.ToInt64(Math.Truncate(nro));
            decimales = Convert.ToInt32(Math.Round((nro - entero) * 100, 2));
            if (decimales > 0)
            {
                dec = " CON " + decimales.ToString() + "/100";
            }
            res = toText(Convert.ToDouble(entero)) + dec;
            return res;
        }

        private static string toText(double value)
        {
            string Num2Text = "";
            value = Math.Truncate(value);
            if (value == 0) Num2Text = "CERO";
            else if (value == 1) Num2Text = "UNO";
            else if (value == 2) Num2Text = "DOS";
            else if (value == 3) Num2Text = "TRES";
            else if (value == 4) Num2Text = "CUATRO";
            else if (value == 5) Num2Text = "CINCO";
            else if (value == 6) Num2Text = "SEIS";
            else if (value == 7) Num2Text = "SIETE";
            else if (value == 8) Num2Text = "OCHO";
            else if (value == 9) Num2Text = "NUEVE";
            else if (value == 10) Num2Text = "DIEZ";
            else if (value == 11) Num2Text = "ONCE";
            else if (value == 12) Num2Text = "DOCE";
            else if (value == 13) Num2Text = "TRECE";
            else if (value == 14) Num2Text = "CATORCE";
            else if (value == 15) Num2Text = "QUINCE";
            else if (value < 20) Num2Text = "DIECI" + toText(value - 10);
            else if (value == 20) Num2Text = "VEINTE";
            else if (value < 30) Num2Text = "VEINTI" + toText(value - 20);
            else if (value == 30) Num2Text = "TREINTA";
            else if (value == 40) Num2Text = "CUARENTA";
            else if (value == 50) Num2Text = "CINCUENTA";
            else if (value == 60) Num2Text = "SESENTA";
            else if (value == 70) Num2Text = "SETENTA";
            else if (value == 80) Num2Text = "OCHENTA";
            else if (value == 90) Num2Text = "NOVENTA";
            else if (value < 100) Num2Text = toText(Math.Truncate(value / 10) * 10) + " Y " + toText(value % 10);
            else if (value == 100) Num2Text = "CIEN";
            else if (value < 200) Num2Text = "CIENTO " + toText(value - 100);
            else if ((value == 200) || (value == 300) || (value == 400) || (value == 600) || (value == 800)) Num2Text = toText(Math.Truncate(value / 100)) + "CIENTOS";
            else if (value == 500) Num2Text = "QUINIENTOS";
            else if (value == 700) Num2Text = "SETECIENTOS";
            else if (value == 900) Num2Text = "NOVECIENTOS";
            else if (value < 1000) Num2Text = toText(Math.Truncate(value / 100) * 100) + " " + toText(value % 100);
            else if (value == 1000) Num2Text = "MIL";
            else if (value < 2000) Num2Text = "MIL " + toText(value % 1000);
            else if (value < 1000000)
            {
                Num2Text = toText(Math.Truncate(value / 1000)) + " MIL";
                if ((value % 1000) > 0) Num2Text = Num2Text + " " + toText(value % 1000);
            }
            else if (value == 1000000) Num2Text = "UN MILLON";
            else if (value < 2000000) Num2Text = "UN MILLON " + toText(value % 1000000);
            else if (value < 1000000000000)
            {
                Num2Text = toText(Math.Truncate(value / 1000000)) + " MILLONES ";
                if ((value - Math.Truncate(value / 1000000) * 1000000) > 0) Num2Text = Num2Text + " " + toText(value - Math.Truncate(value / 1000000) * 1000000);
            }
            else if (value == 1000000000000) Num2Text = "UN BILLON";
            else if (value < 2000000000000) Num2Text = "UN BILLON " + toText(value - Math.Truncate(value / 1000000000000) * 1000000000000);
            else
            {
                Num2Text = toText(Math.Truncate(value / 1000000000000)) + " BILLONES";
                if ((value - Math.Truncate(value / 1000000000000) * 1000000000000) > 0) Num2Text = Num2Text + " " + toText(value - Math.Truncate(value / 1000000000000) * 1000000000000);
            }
            return Num2Text;
        }

        public static string FindInDimensions(this string[,] target, string searchTerm, 
                                                    int columnToLook = -1, int resultColumn = -1)
        {
            string result = "";

            int RowLimit = target.GetLength(0),
                columnLimit = target.GetLength(1);

            if (columnToLook == -1 && resultColumn == -1)
            {
                for (int row = 0; row < RowLimit; row++)
                {
                    for (int col = 0; col < columnLimit; col++)
                    {
                        string ActualObject = target[row, col];

                        if (ActualObject == searchTerm)
                            result = ActualObject;
                    }
                }
            }
            else
            {
                for (int row = 0; row < RowLimit; row++)
                {
                    string ActualObject = target[row, columnToLook];

                    if (ActualObject == searchTerm)
                        result = target[row, resultColumn];
                }
            }
            
            return result;
        }        
    }
    
    public class CustomComparer : IComparer<M.DatosColindancia>
    {
        public int Compare(M.DatosColindancia x, M.DatosColindancia y)
        {
            var regex = new Regex("^(d+)");

            // run the regex on both strings
            var xRegexResult = regex.Match(x.numVivienda.ToString());
            var yRegexResult = regex.Match(y.numVivienda.ToString());

            // check if they are both numbers
            if (xRegexResult.Success && yRegexResult.Success)
            {
                return int.Parse(xRegexResult.Groups[1].Value).CompareTo(int.Parse(yRegexResult.Groups[1].Value));
            }

            // otherwise return as string comparison
            return x.numVivienda.CompareTo(y.numVivienda);
        }
    }
}
