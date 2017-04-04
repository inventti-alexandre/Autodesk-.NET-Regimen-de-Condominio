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
    public static class OfficeUtility
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
                    nameDoc = "\\" + M.Inicio.EncMachote + " " + dateTime + ".xlsx";

            if (Path != "")
                slDoc.SaveAs(Path + nameDoc);
            else
                slDoc.SaveAs(desktopPath + nameDoc);
        }

        internal static void ToWord(List<M.Bloques> Paragraphs, string fileName = @"C:\Users\mnieto\Desktop\MachotePrueba.docx")
        {
            try
            {                
                // Create a document in memory:
                DocX doc = DocX.Create(fileName);


                Formatting redFormat = new Formatting() { FontColor = System.Drawing.Color.Red,
                                                          FontFamily = new FontFamily("Arial"),Size = 10 },
                           normalFormat = new Formatting() { FontFamily = new FontFamily("Arial") , Size = 10};

                foreach (M.Bloques block in Paragraphs)
                {
                    Paragraph pg = doc.InsertParagraph();

                    pg.Alignment = Alignment.both;

                    string paragraph = block.Descripcion;

                    string[] words = paragraph.Split(' ');

                    bool keepFormat = false;

                    for (int i = 0; i < words.Count(); i++)
                    {
                        string word = words[i];

                        if (i + 1 < words.Count())
                            word = word + " ";

                        if (Regex.IsMatch(word, M.Constant.RegexBrackets))
                        {
                            word = word.Replace("[", "").Replace("]", "");
                            pg.InsertText(word, false, redFormat);
                        }
                        else if(word.Contains("["))
                        {
                            keepFormat = true;
                            word = word.Replace("[", "");
                            pg.InsertText(word, false, redFormat);
                        }
                        else if(keepFormat || word.Contains("]"))
                        {
                            if (word.Contains("]"))
                            {
                                keepFormat = false;
                                word = word.Replace("]", "");
                            }

                            pg.InsertText(word, false, redFormat);
                        }                        
                        else
                            pg.InsertText(word, false, normalFormat);


                    }
                   
                }


                doc.Save();
            }
            catch(IOException ex)
            {
                ex.Message.ToEditor();
            }
            catch(Exception ex)
            {
                ex.Message.ToEditor();
            }
            //Process.Start("WINWORD.EXE", fileName);
        }

    }


}
