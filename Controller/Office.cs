using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using SpreadsheetLight;
using DocumentFormat.OpenXml;
using Novacode;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Drawing;

namespace RegimenCondominio.C
{
    public static class Office
    {

        internal static void ToExcel(this DataTable ds, string Path)

        {
            SLDocument slDoc = new SLDocument();

            for (int i = 0; i < ds.Columns.Count; i++)
            {
                slDoc.SetCellValue(1, i + 1, ds.Columns[i].ColumnName);
            }

            for (int i = 0; i < ds.Columns.Count; i++)
            {
                for (int j = 0; j < ds.Rows.Count; j++)
                {

                    object obj = new object();

                    obj = ds.Rows[j][i];

                    if (obj is int)
                    {
                        int intobj = (int)obj;
                        slDoc.SetCellValue(j + 2, i + 1, intobj);                       
                    }
                    else if (obj is string)
                    {
                        string cellValue = ds.Rows[j][i].ToString();
                        slDoc.SetCellValue(j + 2, i + 1, cellValue);
                    }
                    else if (obj is double)
                    {
                        SLStyle style = slDoc.CreateStyle();
                        style.FormatCode = "#,##0.000";
                        double doubleObj = (double)obj;
                        slDoc.SetCellValue(j + 2, i + 1, doubleObj);
                        slDoc.SetCellStyle(j + 2, i + 1, style);
                    }
                    else if (obj is bool)
                    {
                        bool bObj = (bool)obj;
                        slDoc.SetCellValue(j + 2, i + 1, bObj);
                    }
                    else
                    {
                        string cellValue = ds.Rows[j][i].ToString();
                        slDoc.SetCellValue(j + 2, i + 1, cellValue);
                    }


                }
            }


            string  dateTime = DateTime.Now.ToShortTimeString().Replace(":", "-").Replace(".",""),
                    desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    nameDoc = "\\" + M.Inicio.EncMachote.Encabezado + " " + dateTime + ".xlsx";

            if (Path != "")
                slDoc.SaveAs(Path + nameDoc);
            else
                slDoc.SaveAs(desktopPath + nameDoc);
        }

        internal static bool ToWord(this List<M.Bloques> Bloques, string fileName)
        {
            try
            {                
                // Create a document in memory:
                DocX doc = DocX.Create(fileName);

                //Formato que debe de tener la hoja del Word
                doc.PageWidth = 595;
                doc.MarginLeft = 72;
                doc.MarginRight = 72;

                //Caracteres promedio por Párrafo
                //int promCaracteres = 97;
                //double promLinea = 1.4; //Cantidad de líneas por cada Carácter

                bool insertoSubtitutlo = false;

                Formatting redFormat = new Formatting() { FontColor = Color.Red,
                                                          FontFamily = new FontFamily("Arial"),Size = 10 },
                           normalFormat = new Formatting()
                           {
                               FontColor = Color.Black,
                               FontFamily = new FontFamily("Arial"),
                               Size = 10
                           },
                           boldFormat = new Formatting()
                           {
                               FontColor = Color.Black,
                               FontFamily = new FontFamily("Arial"),
                               Bold = true,
                               Size = 10
                           },
                           boldRedFormat = new Formatting() { FontFamily = new FontFamily("Arial"), Size = 10, Bold = true, FontColor = Color.Red }; ;

                Func<string, string> funcReplace = RegexHandler;

                for (int j = 0; j < Bloques.Count; j++)
                {
                    M.Bloques block = Bloques[j];                    

                    string blockDescription = block.Descripcion;

                    Paragraph pg = doc.InsertParagraph();
                    //pg.FontSize(10);
                    //pg.Font(new FontFamily("Arial"));

                    if (j == 0)
                    {                                                
                        //pg.Bold();

                        pg.InsertText(blockDescription, false, boldFormat);
                        pg.InsertText("\n" + M.Constant.PropiedadPrivada, false, normalFormat);

                        pg.Alignment = Alignment.left;

                        pg.ReplaceText(M.Constant.RegexBrackets, funcReplace, false,
                            RegexOptions.None, boldRedFormat, null, MatchFormattingOptions.SubsetMatch);
                    }
                    else
                    {
                        
                        if(block.NomTipoBLoque != "APARTAMENTO" && !insertoSubtitutlo)
                        {
                            pg.InsertText(M.Constant.PropiedadComun + "\n", false, normalFormat);
                            insertoSubtitutlo = true;
                        }
                                                
                        pg.Alignment = Alignment.both;

                        string[] paragraphs = blockDescription.Split(new string[] { "[" + M.Constant.LineasXParrafo+ "]", Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

                        foreach (string paragraph in paragraphs)
                        {
                            //int longitud = paragraph.Replace("[","").Replace("]","").Length, //Obtengo la cantidad de caracteres
                            //residuo = longitud % promCaracteres, //Obtengo el residuo de acuerdo al promedio de caracteres por párrafo
                            //carRestantes = promCaracteres - residuo;//Obtengo la cantidad de carácteres que se deben de agregar como líneas

                            ////Obtengo el equivalente en líneas para saber cuantas deben de insertarse
                            //double promLineas = Math.Round(carRestantes * promLinea);

                            //string lineas = "";

                            //for (int i = 0; i < promLineas; i++)
                            //    lineas += "-";

                            string formatedParagraph = paragraph + "\n" + M.Constant.LineasXParrafo;

                            pg.InsertText(formatedParagraph, false, normalFormat);

                            pg.ReplaceText(M.Constant.RegexBrackets, funcReplace, false,
                                    RegexOptions.None, redFormat, null, MatchFormattingOptions.SubsetMatch);
                        }
                                                                                          
                    }

                }                

                doc.Save();
            }
            catch(IOException ex)
            {
                ex.Message.ToEditor();
                MessageBox.Show("No se pudo guardar el archivo ya que se encuentra activo, favor de cerrar.", "Archivo word abierto", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            catch(Exception ex)
            {
                ex.Message.ToEditor();
                return false;
            }
            
            //Process.Start("WINWORD.EXE", fileName);

            return true;
        }

        private static string RegexHandler(string input)
        {
            return input;
        }

    }


}
