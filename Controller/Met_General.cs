using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.ComponentModel;
using Autodesk.AutoCAD.Geometry;
using cadDB = Autodesk.AutoCAD.DatabaseServices;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace RegimenCondominio.C
{
    public static class Met_General
    {
              
        internal static double ToRadians(this double angleDegrees)
        {
            return ((angleDegrees / 180) * Math.PI);
        }
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

        public static DataTable ConvertToDataTable<T>(this IList<T> data)

        {

            PropertyDescriptorCollection properties =

            TypeDescriptor.GetProperties(typeof(T));

            DataTable table = new DataTable();

            foreach (PropertyDescriptor prop in properties)
            {                
                if(prop.PropertyType.Name != typeof(List<T>).Name && prop.PropertyType.Name != typeof(Point3d).Name)
                    table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
            }

            foreach (T item in data)

            {
                DataRow row = table.NewRow();

                foreach (PropertyDescriptor prop in properties)
                {
                    if (prop.PropertyType.Name != typeof(List<T>).Name && prop.PropertyType.Name != typeof(Point3d).Name)
                        row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                }

                table.Rows.Add(row);

            }

            return table;

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

        internal static void ClearData()
        {
            //Datos almacenados en Módulo Inicial
            M.Inicio.ResultFraccs = null;
            M.Inicio.ResultTipoVivs = null;

            M.Inicio.Fraccionamiento = new M.Fraccionamiento();
            M.Inicio.Estado = "";
            M.Inicio.Municipio = "";
            M.Inicio.Sector = "";
            M.Inicio.Region = "";
            M.Inicio.EncMachote = new M.EncabezadoMachote();           


            //Datos almacenados en Módulo Manzana
            M.Manzana.OrientacionCalculada = new List<string>();
            M.Manzana.ColindanciaManzana = new List<M.ManzanaData>();
            M.Manzana.NoManzana = 0;
            M.Manzana.RumboFrente = "";
            M.Manzana.EsMacrolote = false;

            //Elimino los Xrecord guardados
            C.Met_Manzana.EliminaColindancias();

            //Datos almacenados en Módulo Colindancia            
            M.Colindante.IdTipo = new cadDB.ObjectId();
            M.Colindante.IdsIrregulares = new cadDB.ObjectIdCollection();
            M.Colindante.IdMacrolote = new cadDB.ObjectId();
            M.Colindante.Edificios = new List<M.InEdificios>();
            M.Colindante.Lotes = new List<M.Lote>();
            M.Colindante.ListadoErrores = new ObservableCollection<M.Error>();
            M.Colindante.MainData = new List<M.ColindanciaData>();
            M.Colindante.OrderedApartments = new List<M.Apartments>();
            M.Colindante.PtsVertex = new Point3dCollection();
            M.Colindante.ListCommonArea = new List<M.AreaComun>();
            M.Colindante.LastPoint = 0;

            //Debo de eliminar la polilínea creada al cargar Módulo Colindante       
            if (M.Colindante.IdPolManzana.IsValid)
                M.Colindante.IdPolManzana.GetAndRemove();

            //Eliminar todos los puntos creados
            Met_Colindante.DeleteAdjacencyObjects();

            //Datos de Módulo de Tabla
            M.InfoTabla.MedidasGlobales = new ObservableCollection<M.Medidas>();
            M.InfoTabla.LotesItem = new ObservableCollection<M.Checked<M.LoteItem>>();            
            M.InfoTabla.AllProperties = new List<M.DataColumns>();
            M.InfoTabla.CalleFrente = "";
            M.InfoTabla.RumboInverso = new M.ManzanaData();            
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

        internal static M.Lote Search(this List<M.Lote> listLote, long objToLook)
        {
            M.Lote lote = new M.Lote();

            for(int i = 0; i < listLote.Count; i++)
            {
                lote = listLote[i];

                if (lote._long == objToLook)
                    return lote;
            }

            return new M.Lote();
        }

        internal static M.InEdificios Search(this List<M.InEdificios> listLote, long objToLook)
        {
            M.InEdificios edificio = new M.InEdificios();

            for (int i = 0; i < listLote.Count; i++)
            {
                edificio = listLote[i];

                if (edificio._long == objToLook)
                    return edificio;
            }

            return new M.InEdificios();
        }

        internal static cadDB.ObjectId Search(this List<cadDB.ObjectId> list, long objToLook)
        {
            cadDB.ObjectId id = new cadDB.ObjectId();

            for (int i = 0; i < list.Count; i++)
            {
                id = list[i];

                if (id.Handle.Value == objToLook )
                    return id;
            }

            return new cadDB.ObjectId();
        }

        internal static M.AreaComun Search(this List<M.AreaComun> list, long objToLook)
        {
            M.AreaComun areaComun = new M.AreaComun();

            for (int i = 0; i < list.Count; i++)
            {
                areaComun = list[i];

                if (areaComun._longAreaComun == objToLook)
                    return areaComun;
            }

            return new M.AreaComun();
        }

        internal static M.Apartments Search(this List<M.Apartments> list, long objToLook)
        {
            M.Apartments colindante = new M.Apartments();

            for (int i = 0; i < list.Count; i++)
            {
                colindante = list[i];

                if (colindante.longPl == objToLook)
                    return colindante;
            }

            return new M.Apartments();
        }
        internal static M.DescribeLayer Search(this List<M.DescribeLayer> list, string layerToLook)
        {
            M.DescribeLayer describe = new M.DescribeLayer();

            for (int i = 0; i < list.Count; i++)
            {
                describe = list[i];

                if (describe.Layername == layerToLook)
                    return describe;
            }

            return new M.DescribeLayer();
        }

        internal static long Search(this List<long> list, long longToLook)
        {
            long longFound = new long();

            for (int i = 0; i < list.Count; i++)
            {
                longFound = list[i];

                if (longFound == longToLook)
                    return longFound;
            }

            return new long();
        }

        public static ObservableCollection<T> ToObservable<T>(this IEnumerable<T> original)
        {
            return new ObservableCollection<T>(original);
        }

        internal static DataTable ToDataTable(this string[,] multArray)
        {
            DataTable _myDataTable = new DataTable();

            int rowLenght = multArray.GetLength(0),
                colLenght = multArray.GetLength(1);

            // create columns
            for (int i = 0; i < colLenght; i++)
            {
                _myDataTable.Columns.Add(multArray[0, i]);
            }

            //Creo renglones
            for (int j = 1; j < rowLenght; j++)
            {
                // create a DataRow using .NewRow()
                DataRow row = _myDataTable.NewRow();

                // iterate over all columns to fill the row
                for (int i = 0; i < colLenght; i++)
                {
                    row[i] = multArray[j, i];
                }

                // add the current row to the DataTable
                _myDataTable.Rows.Add(row);
            }

            return _myDataTable;
        }
    }
    
    //public class CustomComparer : IComparer<M.ColindanciaData>
    //{
    //    public int Compare(M.ColindanciaData x, M.ColindanciaData y)
    //    {
    //        var regex = new Regex("^(d+)");

    //        // run the regex on both strings
    //        var xRegexResult = regex.Match(x.Edificio_Lote.ToString());
    //        var yRegexResult = regex.Match(y.Edificio_Lote.ToString());

    //        // check if they are both numbers
    //        if (xRegexResult.Success && yRegexResult.Success)
    //        {
    //            return int.Parse(xRegexResult.Groups[1].Value).CompareTo(int.Parse(yRegexResult.Groups[1].Value));
    //        }

    //        // otherwise return as string comparison
    //        return x.Edificio_Lote.CompareTo(y.Edificio_Lote);
    //    }
    //}
}
