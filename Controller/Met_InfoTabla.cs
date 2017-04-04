using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.ComponentModel;
using System.Data;
using System.Windows.Data;
using RegimenCondominio.M;
using System.Text.RegularExpressions;

namespace RegimenCondominio.C
{
    public static class Met_InfoTabla
    {

        internal static M.Checked<M.LoteItem> SearchByLong(this ObservableCollection<M.Checked<M.LoteItem>> list, long longToLook)
        {
            M.Checked<M.LoteItem> itemActual = new M.Checked<M.LoteItem>();            

            for (int i = 0; i < list.Count; i++)
            {
                itemActual = list[i];

                long longFound = itemActual.Item.Long;

                if (longFound == longToLook)
                    return itemActual;
            }

            return new M.Checked<M.LoteItem>();
        }

        internal static M.Medidas SearchByLong(this ObservableCollection<M.Medidas> list, long longToLook)
        {
            M.Medidas itemActual = new M.Medidas();

            for (int i = 0; i < list.Count; i++)
            {
                itemActual = list[i];

                long longFound = itemActual.LongLote;

                if (longFound == longToLook)
                    return itemActual;
            }

            return new M.Medidas();
        }

        internal static List<M.Medidas> SearchAll(this ObservableCollection<M.Medidas> list, long longToLook)
        {
            List<M.Medidas> listItem = new List<M.Medidas>();

            for (int i = 0; i < list.Count; i++)
            {
                M.Medidas item = list[i];

                long longFound = item.LongLote;

                if (longFound == longToLook)
                    listItem.Add(item);
            }

            return listItem;
        }

        internal static bool GetInverseAddress()
        {
            string RumboInverso = "";

            bool foundData = false;

            if (M.InfoTabla.DiccionarioRumboInverso.TryGetValue(M.Manzana.RumboFrente, out RumboInverso))
            {
                foreach (M.ManzanaData datosColindancia in M.Manzana.ColindanciaManzana)
                {
                    if (datosColindancia.rumboActual == RumboInverso)
                    {
                        M.InfoTabla.RumboInverso = datosColindancia;
                        foundData = true;
                        break;
                    }
                }
            }

            return foundData;
        }

        internal static void GetHeaderTable(bool esMacrolote)
        {                           
            //Inicializo Variables---------------------------------------------- 
            List<M.LoteDetail> ItemsLote = new List<M.LoteDetail>();

            M.InfoTabla.MedidasGlobales = new ObservableCollection<M.Medidas>();

            ObservableCollection<M.Medidas> medidasTemporales = new ObservableCollection<M.Medidas>(); 
            //------------------------------------------------------------------            

            if (esMacrolote)
            {
                if (M.Colindante.IdMacrolote.IsValid)//
                {
                    //Obtengo Id Macrolote
                    long longLote = M.Colindante.IdMacrolote.Handle.Value;

                    M.Lote loteMacro = M.Colindante.Lotes.Search(longLote);

                    foreach (M.ColindanciaData mColindancia in M.Colindante.MainData)
                    {
                        if (mColindancia.LayerSeccion != M.Constant.LayerAreaComun)
                        {
                            ItemsLote.Add(new M.LoteDetail()
                            {
                                LongLote = longLote,
                                NumLote = loteMacro.numLote,
                                NumEdificio = mColindancia.Edificio_Lote,
                                NumApartamento = mColindancia.Apartamento,
                                NumOficial = mColindancia.NoOficial
                            });
                        }
                    }
                }
            }
            else
            {
                foreach (M.ColindanciaData mColindancia in M.Colindante.MainData)
                {
                    if (mColindancia.LayerSeccion != M.Constant.LayerAreaComun)
                    {
                        ItemsLote.Add(new M.LoteDetail()
                        {
                            LongLote = mColindancia.idVivienda,
                            NumLote = mColindancia.Edificio_Lote,
                            NumEdificio = 0,
                            NumApartamento = mColindancia.Apartamento,
                            NumOficial = mColindancia.NoOficial
                        });
                    }
                }
            }

            //Descarto los Items repetidos
            ItemsLote.GetUniqueItems();

            foreach (M.LoteDetail mLoteDetail in ItemsLote)
            {
                medidasTemporales.Add(new M.Medidas()
                {
                    LongLote = mLoteDetail.LongLote,
                    NumLote = mLoteDetail.NumLote,
                    NumEdificio = mLoteDetail.NumEdificio,
                    Apartamento = mLoteDetail.NumApartamento.GetAfterSpace(),
                    NoOficial = mLoteDetail.NumOficial,
                    Calle = M.InfoTabla.CalleFrente,
                    Manzana = M.Manzana.NoManzana.ToString(),
                    ExpedienteCatastral = string.Format("{0}-{1}-", M.Inicio.Region, M.Manzana.NoManzana.ToString())
                });
            }

            //Ordenarlas por diferentes criterios
            if (esMacrolote)
                M.InfoTabla.MedidasGlobales = medidasTemporales.OrderBy(x => x.NumLote)
                                                               .ThenBy(x => x.NumEdificio)
                                                               .ThenBy(x => x.Apartamento).ToObservable();
            else
                M.InfoTabla.MedidasGlobales = medidasTemporales.OrderBy(x => x.NumLote)
                                                               .ThenBy(x => x.Apartamento).ToObservable();
        }

        internal static List<M.DataColumns> DescribeColumns()
        {
            List<M.DataColumns> dictionary = new List<M.DataColumns>();

            #region Columnas Header

            Type typeHeader = typeof(M.HeaderColumns);

            dictionary.Add(new M.DataColumns()
            {
                TipoEnumerador = typeHeader,               
                PropertyName = Enum.GetName(typeHeader, M.HeaderColumns.NoOficial),
                Descripcion = "No. Oficial",
                esVisible = true
            });


            dictionary.Add(new M.DataColumns()
            {
                TipoEnumerador = typeHeader,
                PropertyName = Enum.GetName(typeHeader, M.HeaderColumns.NumEdificio),
                Descripcion = "Edificio",
                esVisible = M.Manzana.EsMacrolote ? true : false
            });

            dictionary.Add(new M.DataColumns()
            {
                TipoEnumerador = typeHeader,
                PropertyName = Enum.GetName(typeHeader, M.HeaderColumns.Apartamento),
                Descripcion = "Apartamento",
                esVisible = true
            });

            dictionary.Add(new M.DataColumns()
            {
                TipoEnumerador = typeHeader,
                PropertyName = Enum.GetName(typeHeader, M.HeaderColumns.Calle),
                Descripcion = "Calle",
                esVisible = true
            });
            #endregion

            #region Columnas Detalle

            Type typeDetail = typeof(M.DetailColumns);

            #region Con Layer


            dictionary.Add(new M.DataColumns()
            {
                TipoEnumerador = typeDetail, Layerseccion = M.Constant.LayerAPBaja,
                PropertyName = Enum.GetName(typeDetail, M.DetailColumns.CPlantaBaja),
                Descripcion = "Planta Baja Cubierta",
                esVisible = true
            });

            dictionary.Add(new M.DataColumns()
            {
                TipoEnumerador = typeDetail, Layerseccion = M.Constant.LayerAPAlta,
                PropertyName = Enum.GetName(typeDetail, M.DetailColumns.CPlantaAlta),
                Descripcion = "Planta Alta Cubierta",
                esVisible = true
            });

            dictionary.Add(new M.DataColumns()
            {
                TipoEnumerador = typeDetail, Layerseccion = M.Constant.LayerLavanderia,
                PropertyName = Enum.GetName(typeDetail, M.DetailColumns.CLavanderia),
                Descripcion = "Lavandería Cubierta",
                esVisible = true
            });

            dictionary.Add(new M.DataColumns()
            {
                TipoEnumerador = typeDetail, Layerseccion = M.Constant.LayerEstacionamiento,
                PropertyName = Enum.GetName(typeDetail, M.DetailColumns.CEstacionamiento),
                Descripcion = "Estacionamiento Cubierto",
                esVisible = true
            });

            dictionary.Add(new M.DataColumns()
            {
                TipoEnumerador = typeDetail, Layerseccion = M.Constant.LayerPasillo,
                PropertyName = Enum.GetName(typeDetail, M.DetailColumns.CPasillo),
                Descripcion = "Pasillo Cubierto",
                esVisible = true
            });

            dictionary.Add(new M.DataColumns()
            {
                TipoEnumerador = typeDetail, Layerseccion = M.Constant.LayerPatio,
                PropertyName = Enum.GetName(typeDetail, M.DetailColumns.CPatio),
                Descripcion = "Patio Cubierto",
                esVisible = true
            });

            dictionary.Add(new M.DataColumns()
            {
                TipoEnumerador = typeDetail, Layerseccion = M.Constant.LayerLavanderia,
                PropertyName = Enum.GetName(typeDetail, M.DetailColumns.DLavanderia),
                Descripcion = "Lavandería Descubierta",
                esVisible = true
            });

            dictionary.Add(new M.DataColumns()
            {
                TipoEnumerador = typeDetail, Layerseccion = M.Constant.LayerEstacionamiento,
                PropertyName = Enum.GetName(typeDetail, M.DetailColumns.DEstacionamiento),
                Descripcion = "Estacionamiento Descubierto",
                esVisible = true
            });
            
            dictionary.Add(new M.DataColumns()
            {
                TipoEnumerador = typeDetail, Layerseccion = M.Constant.LayerPasillo,
                PropertyName = Enum.GetName(typeDetail, M.DetailColumns.DPasillo),
                Descripcion = "Pasillo Descubierto",
                esVisible = true
            });
            
            dictionary.Add(new M.DataColumns()
            {
                TipoEnumerador = typeDetail, Layerseccion = M.Constant.LayerPatio,
                PropertyName = Enum.GetName(typeDetail, M.DetailColumns.DPatio),
                Descripcion = "Patio Descubierto",
                esVisible = true
            });

            #endregion

            #region Sin Layer

            dictionary.Add(new M.DataColumns()
            {
                TipoEnumerador = typeDetail,                
                PropertyName = Enum.GetName(typeDetail, M.DetailColumns.AreaTotalCubierta),
                Descripcion = "Área Total Cubierta",
                esVisible = true
            });

            dictionary.Add(new M.DataColumns()
            {
                TipoEnumerador = typeDetail,
                PropertyName = Enum.GetName(typeDetail, M.DetailColumns.AreaTotalDescubierta),
                Descripcion = "Área Total Descubierta",
                esVisible = true
            });

            dictionary.Add(new M.DataColumns()
            {
                TipoEnumerador = typeDetail,
                PropertyName = Enum.GetName(typeDetail, M.DetailColumns.AreaCubiertaDescubierta),
                Descripcion = "Total de Área Cubierta y Descubierta",
                esVisible = true
            });

            dictionary.Add(new M.DataColumns()
            {
                TipoEnumerador = typeDetail,
                PropertyName = Enum.GetName(typeDetail, M.DetailColumns.NombreAreaComun),
                Descripcion = "Área Común",
                esVisible = true
            });

            dictionary.Add(new M.DataColumns()
            {
                TipoEnumerador = typeDetail,
                PropertyName = Enum.GetName(typeDetail, M.DetailColumns.AreaExclusiva),
                Descripcion = "Área Exclusiva de Terreno",
                esVisible = M.Manzana.EsMacrolote ? true : false
            });

            dictionary.Add(new M.DataColumns()
            {
                TipoEnumerador = typeDetail,
                PropertyName = Enum.GetName(typeDetail, M.DetailColumns.Proindiviso),
                Descripcion = "Proindiviso",
                esVisible = true
            });

            dictionary.Add(new M.DataColumns()
            {
                TipoEnumerador = typeDetail,
                PropertyName = Enum.GetName(typeDetail, M.DetailColumns.PredioFrente),
                Descripcion = "Frente de Predio",
                esVisible = true
            });

            dictionary.Add(new M.DataColumns()
            {
                TipoEnumerador = typeDetail,
                PropertyName = Enum.GetName(typeDetail, M.DetailColumns.PredioFondo),
                Descripcion = "Fondo de Predio",
                esVisible = true
            });

            dictionary.Add(new M.DataColumns()
            {
                TipoEnumerador = typeDetail,
                PropertyName = Enum.GetName(typeDetail, M.DetailColumns.PredioArea),
                Descripcion = "Área de Predio",
                esVisible = true
            });

            dictionary.Add(new M.DataColumns()
            {
                TipoEnumerador = typeDetail,
                PropertyName = Enum.GetName(typeDetail, M.DetailColumns.AreaConstruccion),
                Descripcion = "Área de Construcción",
                esVisible = true
            });
            
            dictionary.Add(new M.DataColumns()
            {
                TipoEnumerador = typeDetail,
                PropertyName = Enum.GetName(typeDetail, M.DetailColumns.ExpedienteCatastral),
                Descripcion = "Expediente Catastral",
                esVisible = true
            });

            #endregion

            #endregion

            #region Propiedades sin Columna

            //Id del Lote
            dictionary.Add(new M.DataColumns()
            {
                TipoEnumerador = typeof(M.DataColumns),
                PropertyName = "LongLote",
                Descripcion = "Long Lote",
                esVisible = false,
                Layerseccion = ""
            });

            //Lote
            dictionary.Add(new M.DataColumns()
            {
                TipoEnumerador = typeof(M.DataColumns),
                PropertyName = "NumLote",
                Descripcion = "Lote",
                esVisible = true,
                Layerseccion = ""
            });

            //Manzana
            dictionary.Add(new M.DataColumns()
            {
                TipoEnumerador = typeof(M.DataColumns),
                PropertyName = "Manzana",
                Descripcion = "Manzana",
                esVisible = true, Layerseccion = ""
            });

            #endregion

            return dictionary;
        }

        internal static List<M.DataColumns> DetailColumnsToHide()
        {
            //Declaro variables--------------------------
            List<string> seccionesPlano = new List<string>();

            List<M.DataColumns> seccionesAOcultar = new List<M.DataColumns>();

            HashSet <string> seccionesUnicasPlano;
            //-------------------------------------------

            //Obtengo todas las secciones
            seccionesPlano = M.Colindante.MainData.Select(x => x.LayerSeccion).ToList();

            //Las asigno en hash para sólo tener secciones únicas
            seccionesUnicasPlano = new HashSet<string>(seccionesPlano);

            //Encuentro todas las columnas disponibles sólo en las Columnas del detalle
            seccionesAOcultar = DescribeColumns().Where(x=> x.TipoEnumerador == typeof(M.DetailColumns)
                                                        && !string.IsNullOrWhiteSpace(x.Layerseccion)).ToList();
            
            //Empiezo a eliminar las secciones que no se encuentren
            foreach(string seccionActual in seccionesUnicasPlano)                            
                seccionesAOcultar.RemoveAll(x => x.Layerseccion == seccionActual);
           
            return seccionesAOcultar;                                  
        }

        internal static string[,] ToStringArray(this ObservableCollection<M.Medidas> list, long LoteActual)
        {
            
            List<M.Medidas> listView = new List<M.Medidas>();            

            foreach (M.Medidas mActual in list)
                if (mActual.LongLote == LoteActual)
                    listView.Add(mActual);

            //Aquí tengo todas las propiedades           
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(typeof(M.Medidas));
            PropertyDescriptorCollection propertiesTotals = TypeDescriptor.GetProperties(typeof(M.TotalesMedidas));

            //Obtengo y Agrego las Columnas           
            Dictionary<string, bool> dColumnas = new Dictionary<string, bool>();

            Dictionary<string, M.DetailColumns> dIndexColumns = new Dictionary<string, M.DetailColumns>();           

            //Agrego el nombre de las columnas
            foreach (PropertyDescriptor prop in properties)
            {
                bool esVisible = false;
                M.DetailColumns dt;

                if (M.InfoTabla.VisibilidadPropiedades.TryGetValue(prop.Name, out esVisible))
                {
                    if (esVisible)
                    {
                        bool isEnumValid = Enum.TryParse<M.DetailColumns>(prop.Name, out dt);

                        dColumnas.Add(prop.Name, isEnumValid);

                        if (isEnumValid)
                            dIndexColumns.Add(prop.Name, dt);
                    }
                }
            }

            int rowsToArray = listView.Count + 2,
                colCount = 0;

            string[,] Table = new string[rowsToArray, dColumnas.Count];


            //Cargo las columnas y los Totales
            foreach(KeyValuePair<string, bool> propActual in dColumnas)
            {
                //Agrego la columna a la tabla------------------------------------------
                Table[0, colCount] = M.InfoTabla.DescripcionPropiedades[propActual.Key];

                //Si tiene total lo agrego a la tabla----------------------------------                
                if (propActual.Value)
                {
                    //Obtengo la columna a localizar
                    M.DetailColumns dtCol = dIndexColumns[propActual.Key];

                    foreach(PropertyDescriptor propTotal in propertiesTotals)
                    {
                        M.Totales mTotalActual = (M.Totales)propTotal.GetValue(M.InfoTabla.TotalesTabla);

                        if(mTotalActual  != null && mTotalActual.Columna == dtCol)
                        {
                            Table[rowsToArray - 1, colCount] = mTotalActual.Total.ToString();
                            break;
                        }
                    }
                }

                //Si no tiene total lo pongo como vacío
                if (Table[rowsToArray - 1, colCount] == null)
                    Table[rowsToArray - 1, colCount] = "";              
                //----------------------------------------------------------------------
                colCount++;
            }

            //Agrego los renglones
            for (int i = 0; i < listView.Count; i++)
            {
                M.Medidas item = listView[i];

                int col = 0;

                foreach (PropertyDescriptor prop in properties)
                {
                    bool esVisible = false;

                    if (M.InfoTabla.VisibilidadPropiedades.TryGetValue(prop.Name, out esVisible))
                    {
                        if (esVisible)
                        {
                            Table[i + 1, col]  = (prop.GetValue(item) ?? DBNull.Value).ToString();
                            col++;
                        }
                    }
                    
                }
            }

            return Table;

        }

        private static List<M.LoteDetail> GetUniqueItems(this List<M.LoteDetail> initialList)
        {          
            for(int i = 0; i < initialList.Count ; i++)
            {
                M.LoteDetail currenItem = initialList[i];

                for (int j = initialList.Count - 1; j>=0; j--)
                {
                    if(j != i)
                    {
                        M.LoteDetail mEvaluatedItem = initialList[j];

                        if (currenItem.LongLote == mEvaluatedItem.LongLote &&
                            currenItem.NumApartamento == mEvaluatedItem.NumApartamento &&
                            currenItem.NumEdificio == mEvaluatedItem.NumEdificio &&
                            currenItem.NumLote == mEvaluatedItem.NumLote &&
                            currenItem.NumOficial == mEvaluatedItem.NumOficial)
                            initialList.RemoveAt(j);                      
                    }
                }
            }

            return initialList;
        }

        internal static void GetChecklistItems(out List<long> BaseItems, out List<long> NotBaseItems)
        {
            BaseItems = new List<long>();
            NotBaseItems = new List<long>();

            if (M.Manzana.EsMacrolote)
            {
                if (M.Colindante.IdMacrolote.IsValid)
                    BaseItems.Add(M.Colindante.IdMacrolote.Handle.Value);
            }
            else {

                foreach (M.Lote mLote in M.Colindante.Lotes.OrderBy(x => x.numLote))
                {
                    bool esIrregular = false,
                         esLoteBase = false;

                    string TipoLote = "";

                    long loteTipo = new long();

                    //Si esta dentro de los Irregulares
                    esIrregular = M.Colindante.IdsIrregulares.Contains(new Handle(mLote._long).toObjectId());

                    //Asigno el Tipo de Lote
                    TipoLote = esIrregular ? "Irregular" : "Regular";

                    //Busco el Lote Tipo
                    loteTipo = M.Colindante.IdTipo.Handle.Value;

                    //Si es Irregular o es Regular es tomado como Lote Tipo
                    if (esIrregular || loteTipo == mLote._long)
                    {
                        esLoteBase = true;
                        BaseItems.Add(mLote._long);
                    }

                    M.InfoTabla.LotesItem.Add(new M.Checked<M.LoteItem>()
                    {
                        IsChecked = false,

                        Item = new M.LoteItem()
                        {
                            Name = "Lote " + mLote.numLote,
                            TipoLote = TipoLote,
                            EsLoteBase = esLoteBase,
                            Long = mLote._long
                        }
                    });

                }
            }        
        }

        internal static Dictionary<string, double> CalculatePropertyTotals(CollectionView view)
        {
            Dictionary<string, double> propertyTotals = new Dictionary<string, double>();

            PropertyDescriptorCollection propCollection = TypeDescriptor.GetProperties(typeof(M.Medidas));                        

            //Por cada item dentro de la vista
            foreach (M.Medidas medidaActual in view)
            {
                //Por cada Propiedad del item Medidas
                foreach (PropertyDescriptor prop in propCollection)
                {
                    bool esVisible = false;
                    double numeroCelda = 0;

                    //Reviso que sea una propiedad Visible y que deba de ser tomada en cuenta
                    if (M.InfoTabla.VisibilidadPropiedades.TryGetValue(prop.Name, out esVisible))
                    {
                        //Si es visible
                        if (esVisible)
                        {
                            //Si no se ha agregado la propiedad la agrego
                            if (!propertyTotals.ContainsKey(prop.Name))
                                propertyTotals.Add(prop.Name, 0);

                            //Obtengo el valor de la Celda actual
                            string valorCelda = (prop.GetValue(medidaActual) ?? "").ToString();

                            //Reviso que el valor sea númerico
                            if (double.TryParse(valorCelda, out numeroCelda))                            
                                propertyTotals[prop.Name] += numeroCelda;                            
                        }
                    }
                }
            }

            return propertyTotals;
        }

        internal static void DuplicateEditedValues(M.Medidas itemEdited, double valueEdited, PropertyInfo propToUpdate)
        {
            //Revisar los Lote que no son Base
            List<long> listNotBase = M.InfoTabla.LotesItem.Where(x => !x.Item.EsLoteBase).Select(x => x.Item.Long).ToList();

            //Itero hacia atrás para realizar modificaciones al Item si es necesario
            for (int j = M.InfoTabla.MedidasGlobales.Count - 1; j >= 0; j--)
            {
                M.Medidas itemMedida = M.InfoTabla.MedidasGlobales[j];

                //Si esta dentro de los Lotes que no son Base y su apartamento es el mismo de la edición
                if (listNotBase.Contains(itemMedida.LongLote) && itemMedida.Apartamento == itemEdited.Apartamento)
                {
                    //Modifico lo que pudiera llegar a cambiar
                    propToUpdate.SetValue(itemMedida, valueEdited.ToString());

                    M.DetailColumns dtCol;
                    List<M.Totales> totalesVertical;

                    if (Enum.TryParse<M.DetailColumns>(propToUpdate.Name, out dtCol))
                        UpdateHorizontalTotals(itemMedida, dtCol, valueEdited, out totalesVertical);
                }
            }
        }


        internal static bool UpdateHorizontalTotals (M.Medidas mItemEditado, M.DetailColumns colEditada, double numIntroducido,
                                                        out List<M.Totales> listVerticalTotals)
        {
            int enumTotalAreaCub = (int)M.DetailColumns.AreaTotalCubierta,
                enumTotalAreaDesc = (int)M.DetailColumns.AreaTotalDescubierta,
                enumEditado = (int)colEditada;

            double sumaHorizontalAreaCub = 0,
                    sumaHorizontalAreaDesc = 0,
                    sumaHorizontalAmbos = 0;

            double sumaVerticalAreaCub = 0,
                    sumaVerticalAreaDesc = 0,
                    SumaVerticalAmbos = 0;

            bool siAplica = false;

            listVerticalTotals = new List<M.Totales>();



            //Obtengo la propiedad ligada a la columna
            PropertyInfo propertyEdited = typeof(M.Medidas).GetProperty(colEditada.ToString());

            //Obtengo todas las propiedades
            PropertyInfo[] propertiesCollection = typeof(M.Medidas).GetProperties();

            Dictionary<PropertyInfo, M.DetailColumns> propertiesFiltered = new Dictionary<PropertyInfo, M.DetailColumns>();

            PropertyInfo propertyToModify;

            //Solamente se actualiza si la columna fue modificada
            if (enumEditado < enumTotalAreaDesc)
            {
                siAplica = true;

                foreach (PropertyInfo prop in propertiesCollection)
                {
                    M.DetailColumns colParser;
                    if (Enum.TryParse<M.DetailColumns>(prop.Name, out colParser))
                        propertiesFiltered.Add(prop, colParser);
                }

                //Si la columna modificada es menor al total de Área Cubierta Sólo modifico lo de Área Cubierta
                if (enumEditado < enumTotalAreaCub)
                {
                    foreach (KeyValuePair<PropertyInfo, M.DetailColumns> dicValue in propertiesFiltered)
                    {
                        int enumDCActual = (int)dicValue.Value;

                        //Si la columna es menor a la del Área Cubierta y no es la misma que se editó
                        if (enumDCActual < enumTotalAreaCub && enumDCActual != enumEditado)
                        {
                            object objProp = dicValue.Key.GetValue(mItemEditado);

                            if (objProp != null)
                            {
                                double numValue = 0;

                                if (double.TryParse(objProp.ToString(), out numValue))
                                    sumaHorizontalAreaCub += numValue;
                            }
                        }
                        else if (enumDCActual < enumTotalAreaCub && enumDCActual == enumEditado)
                        {
                            sumaHorizontalAreaCub += numIntroducido;
                        }
                    }

                    //Cambio el valor de el Total de Área Cubierta                    
                    propertyToModify = typeof(M.Medidas).GetProperty(M.DetailColumns.AreaTotalCubierta.ToString());

                    if (propertyToModify != null)
                        propertyToModify.SetValue(mItemEditado, sumaHorizontalAreaCub.ToString());


                }
                else //Si la columna modificada es mayor significa que hay que editar el Área Descubierta
                {
                    foreach (KeyValuePair<PropertyInfo, M.DetailColumns> dicItem in propertiesFiltered)
                    {
                        int enumDCActual = (int)dicItem.Value;

                        //Si la columna es menor a la del Área Cubierta y no es la misma que se editó
                        if (enumDCActual < enumTotalAreaDesc && enumDCActual != enumEditado)
                        {
                            object objProp = dicItem.Key.GetValue(mItemEditado);

                            if (objProp != null)
                            {
                                double numValue = 0;

                                if (double.TryParse(objProp.ToString(), out numValue))
                                    sumaHorizontalAreaDesc += numValue;
                            }
                        }
                        else if (enumDCActual < enumTotalAreaDesc && enumDCActual == enumEditado)
                        {
                            sumaHorizontalAreaDesc += numIntroducido;
                        }
                    }

                    //Cambio el valor de el Total de Área Descubierta                    
                    propertyToModify = typeof(M.Medidas).GetProperty(M.DetailColumns.AreaTotalDescubierta.ToString());

                    if (propertyToModify != null)
                    {
                        propertyToModify.SetValue(mItemEditado, sumaHorizontalAreaDesc.ToString());
                    }

                }



                //Si la modificación no se encontró en el Área Cubierta la obtengo desde el Item
                if (sumaHorizontalAreaCub == 0)
                    sumaHorizontalAreaCub = mItemEditado.AreaTotalCubierta != null ? double.Parse(mItemEditado.AreaTotalCubierta) : 0;

                //Si la modificación no se encontró en el Área Descubierta la obtengo desde el Item
                if (sumaHorizontalAreaDesc == 0)
                    sumaHorizontalAreaDesc = mItemEditado.AreaTotalDescubierta != null ? double.Parse(mItemEditado.AreaTotalCubierta) : 0;

                //Sumo ambos y cambio la propiedad Área Total Descubierta + Total Cubierta       
                propertyToModify = typeof(M.Medidas).GetProperty(M.DetailColumns.AreaCubiertaDescubierta.ToString());

                sumaHorizontalAmbos = sumaHorizontalAreaCub + sumaHorizontalAreaDesc;

                if (propertyToModify != null)
                    propertyToModify.SetValue(mItemEditado, sumaHorizontalAmbos.ToString());

                foreach (M.Medidas mMedidasItem in M.InfoTabla.MedidasGlobales)
                {
                    if (mMedidasItem.LongLote == mItemEditado.LongLote && mMedidasItem.Apartamento != mItemEditado.Apartamento)
                    {
                        sumaVerticalAreaCub += double.Parse((mMedidasItem.AreaTotalCubierta ?? "0"));
                        sumaVerticalAreaDesc += double.Parse((mMedidasItem.AreaTotalDescubierta ?? "0"));
                        SumaVerticalAmbos += double.Parse((mMedidasItem.AreaCubiertaDescubierta ?? "0"));
                    }
                }

                sumaVerticalAreaCub += sumaHorizontalAreaCub;
                sumaVerticalAreaDesc += sumaHorizontalAreaDesc;
                SumaVerticalAmbos += sumaHorizontalAmbos;

                foreach (PropertyInfo propTotal in typeof(M.TotalesMedidas).GetProperties())
                {
                    M.Totales totalActual = (M.Totales)propTotal.GetValue(M.InfoTabla.TotalesTabla);

                    if (totalActual != null)
                    {
                        M.Totales mItemTotal;

                        if (totalActual.Columna == M.DetailColumns.AreaTotalCubierta)
                        {
                            mItemTotal = new M.Totales()
                            {
                                Total = sumaVerticalAreaCub,
                                Columna = totalActual.Columna
                            };

                            propTotal.SetValue(M.InfoTabla.TotalesTabla, mItemTotal);

                            listVerticalTotals.Add(mItemTotal);
                        }
                        else if (totalActual.Columna == M.DetailColumns.AreaTotalDescubierta)
                        {
                            mItemTotal = new M.Totales()
                            {
                                Total = sumaVerticalAreaDesc,
                                Columna = totalActual.Columna
                            };

                            propTotal.SetValue(M.InfoTabla.TotalesTabla, mItemTotal);
                            listVerticalTotals.Add(mItemTotal);
                        }
                        else if (totalActual.Columna == M.DetailColumns.AreaCubiertaDescubierta)
                        {
                            mItemTotal = new M.Totales()
                            {
                                Total = SumaVerticalAmbos,
                                Columna = totalActual.Columna
                            };

                            propTotal.SetValue(M.InfoTabla.TotalesTabla, mItemTotal);

                            listVerticalTotals.Add(mItemTotal);
                        }
                    }
                }
                              
            }

            return siAplica;
        }

        internal static bool HasEmptyFields(long longActual, out string msgs)
        {
            msgs = "";
            
            bool hasEmptyFields = false;

            List<Tuple<string, string>> propApartments = new List<Tuple<string, string>>();

            foreach(M.Medidas mItemActual in M.InfoTabla.MedidasGlobales)
            {
                if(mItemActual.LongLote == longActual)
                {
                    foreach(M.DataColumns dtC in M.InfoTabla.AllProperties)
                    {
                        if (dtC.esVisible)
                        {
                            PropertyInfo prop = typeof(M.Medidas).GetProperty(dtC.PropertyName);

                            if(prop != null)
                            {
                                object objProp = prop.GetValue(mItemActual);

                                if (objProp == null)
                                {
                                    propApartments.Add(new Tuple<string, string>(dtC.Descripcion, mItemActual.Apartamento));
                                    //msgs.Add(string.Format("Falta valor en {0} Apartamento {1}", dtC.Descripcion,mItemActual.Apartamento));
                                    hasEmptyFields = true;
                                }
                                //Solamente voy a revisar los string que son los objetos a los que tienen acceso
                                else if(objProp is string)
                                {
                                    string sValue = (string)objProp;                                    

                                    if(string.IsNullOrWhiteSpace(sValue))
                                    {
                                        propApartments.Add(new Tuple<string, string>(dtC.Descripcion, mItemActual.Apartamento));
                                        //msgs.Add(string.Format("Falta valor en {0} Apartamento {1}", dtC.Descripcion,mItemActual.Apartamento));
                                        hasEmptyFields = true;
                                    }                                    
                                }
                            }
                        }
                    }
                }
            }

            if(hasEmptyFields)
            {
                string actualAp = "";

                msgs =  "Faltan datos de Apartamento(s): ";

                foreach(Tuple<string, string> propApartment in propApartments)
                {
                    //Si es un nuevo apartamento doy salto de línea
                    if (actualAp != propApartment.Item2)
                    {
                        actualAp = propApartment.Item2;
                        msgs = msgs + "\n " + propApartment.Item2 + ": " + propApartment.Item1;
                    }
                    else
                        msgs = msgs + ", " + propApartment.Item1; 
                }
            }

            return hasEmptyFields;
        }

        internal static HashSet<string> GetVariables(List<M.Bloques> resultadoBloques)
        {
            HashSet<string> variables = new HashSet<string>();

            foreach (M.Bloques bloque in M.InfoTabla.ResultadoBloques.OrderBy(x => x.Orden))
            {
                string descripcion = bloque.Descripcion;

                MatchCollection matches = Regex.Matches(descripcion, M.Constant.RegexBrackets);

                foreach (Match match in matches)
                    variables.Add(match.Groups[0].Value);
            }

            return variables;
        }

        internal static object CategorizeVariables(SQL_Connector conn, object input, BackgroundWorker bg)
        {
            HashSet<string> inputVars = new HashSet<string>();
            List<string> resultRows;

            if (input != null)
                inputVars = (HashSet<string>)input;

            List<string> vars = new List<string>();

            foreach (string var in inputVars)
                vars.Add("'" + var.Replace("[", "").Replace("]", "") + "'");

            string inClause = string.Join(",", vars),
                   query = string.Format(Config.DB.QueryVariables, inClause);

            conn.Select(query,out resultRows, '|');

            return resultRows; //resultRows;            
        }

        internal static bool GenerateTemplate(List<long> lotes, List<Bloques> bloques, List<Variables> variables)
        {
            foreach(long loteActual in lotes)
            {
                List<M.Variables> varsGlobales = new List<Variables>();
                List<Tuple<M.Variables, string>> varsAps = new List<Tuple<Variables, string>>();
                HashSet<string> ApsLote = new HashSet<string>();

                List<M.Bloques> bloquesCalc = new List<Bloques>();

                //Obtengo todas las variables Globales
                foreach(Variables varActual in variables)
                {
                    if (varActual.Nom_Tipo_Bloque.ToUpper() == "GLOBAL")
                    {
                        if (!varActual.EsCalculado)
                        {
                            string valorVar = GetGlobalVariable(varActual.NombreCorto, loteActual);

                            varsGlobales.Add(new M.Variables()
                            {
                                Conv_Letra = varActual.Conv_Letra,
                                EsCalculado = varActual.EsCalculado,
                                NombreCorto = varActual.NombreCorto,
                                NombreVariable = varActual.NombreVariable,
                                Nom_Tipo_Bloque = varActual.Nom_Tipo_Bloque,
                                Valor = valorVar
                            });
                                                        
                        }
                        else
                            varsGlobales.Add(varActual);
                    }
                    else if(varActual.Nom_Tipo_Bloque.ToUpper() == "APARTAMENTO")
                    {
                       foreach(M.Medidas mMedidas in M.InfoTabla.MedidasGlobales)
                        {
                            if(mMedidas.LongLote == loteActual)
                            {
                                string valorAp = GetApartmentVariable(varActual.NombreCorto, loteActual, mMedidas);

                                Variables varAPActual = new Variables() {
                                    Conv_Letra = varActual.Conv_Letra,
                                    EsCalculado = varActual.EsCalculado,
                                    NombreCorto = varActual.NombreCorto,
                                    Valor = valorAp,
                                    NombreVariable = varActual.NombreVariable,
                                    Nom_Tipo_Bloque = varActual.Nom_Tipo_Bloque
                                };

                                varsAps.Add(new Tuple<Variables, string>(varAPActual, mMedidas.Apartamento));

                                ApsLote.Add(mMedidas.Apartamento);
                            }
                        }
                    }
                }


                foreach(M.Bloques bloque in bloques.OrderBy(x => x.Orden))
                {
                    string descripcion = bloque.Descripcion;

                    if (bloque.Nom_Tipo_BLoque.ToUpper() == "GLOBAL")
                    {
                        foreach (M.Variables varGlobal in varsGlobales)
                        {
                            string valor = varGlobal.Valor == Environment.NewLine ? varGlobal.Valor : "[" + varGlobal.Valor + "]";
                            descripcion = descripcion.Replace("[" + varGlobal.NombreCorto + "]", valor);
                        }

                        bloquesCalc.Add(new Bloques()
                        {
                            Descripcion = descripcion,
                            Id_Tipo_Bloque = bloque.Id_Tipo_Bloque,
                            Nom_Tipo_BLoque = bloque.Nom_Tipo_BLoque,
                            Orden = bloque.Orden
                        });
                    }
                    else
                    {
                        foreach (string Apartment in ApsLote)
                        {
                            string descApartment = descripcion;

                            foreach (Tuple<M.Variables, string> itemVars in varsAps)
                            {
                                if (itemVars.Item2 == Apartment)
                                    descApartment = descApartment.Replace("[" + itemVars.Item1.NombreCorto + "]", "[" + itemVars.Item1.Valor + "]");
                            }

                            foreach (M.Variables varGlobal in varsGlobales)
                            {
                                string valor = varGlobal.Valor == Environment.NewLine ? varGlobal.Valor : "[" + varGlobal.Valor + "]";
                                descApartment = descApartment.Replace("[" + varGlobal.NombreCorto + "]", valor);
                            }

                            bloquesCalc.Add(new Bloques()
                            {
                                Descripcion = descApartment,
                                Id_Tipo_Bloque = bloque.Id_Tipo_Bloque,
                                Nom_Tipo_BLoque = bloque.Nom_Tipo_BLoque,
                                Orden = bloque.Orden
                            });
                        }
                    }
                }

                OfficeUtility.ToWord(bloquesCalc, "MachotePrueba");                
            }

            return true;
        }

        public static List<long> ListLongs;

        internal static void FinishedCategorize(object input)
        {
           if(input != null)
            {
                List<string> resultRows = (List<string>)input;

                M.InfoTabla.ResultadoVariables = new List<M.Variables>();

                foreach(string row in resultRows)
                {
                    string[] cell = row.Split('|');

                    M.InfoTabla.ResultadoVariables.Add(new M.Variables()
                    {
                        NombreCorto = cell[0],//NOM_CORTO
                        NombreVariable = cell[1],//NOM_VAR
                        Valor = cell[2],//VALOR
                        EsCalculado = cell[3] == "1" ? true : false, //ES_CALCULADO
                        Conv_Letra = cell[4] == "1" ? true : false,//CONV_LETRA
                        Nom_Tipo_Bloque = cell[5] //NOM_TIPO_BLOQUE
                    });
                }

                GenerateTemplate(ListLongs, M.InfoTabla.ResultadoBloques, M.InfoTabla.ResultadoVariables);
            }
        }


        #region Catalogo de Apartamentos y Globales

        private static string GetGlobalVariable(string input, long lote)
        {
            string valor = "";

            //case "":
            //  break;

            switch (input)
            {
                case "NumLote": //Número del Lote
                    M.Lote loteActual = M.Colindante.Lotes.Search(lote);
                    if (loteActual != null)
                        valor = loteActual.numLote.ToString();
                    break;
                case "Fraccionamiento": //Fraccionamiento
                    valor = M.Inicio.Fraccionamiento.fraccionamiento;
                    break;
                case "Sector":
                    valor = M.Inicio.Sector;
                    break;
                case "Municipio":
                    valor = M.Inicio.Municipio;
                    break;
                case "Estado":
                    valor = M.Inicio.Fraccionamiento.Estado;
                    break;
                case "ColindanciasManzana":
                    
                    List<string> colindancias = new List<string>();

                    int c = 1;

                    foreach(M.ManzanaData mManzana in M.Manzana.ColindanciaManzana)
                    {

                        if(c == 1)                        
                            colindancias.Add(string.Format("Al {0} con {1}", mManzana.rumboActual, mManzana.textColindancia));                        
                        else                        
                            colindancias.Add(string.Format("al {0} con {1}", mManzana.rumboActual, mManzana.textColindancia));                                                   

                        c++;
                    }

                    valor = string.Join(", ", colindancias);
                    break;
                case "NomAreaComun":
                    break;
                case "SeccionAreaComun":
                    break;
                case "SaltoLinea":
                    valor = Environment.NewLine;
                    break;
                default:
                    valor = "";
                    break;                   
            }
            return valor;
        }

        private static string GetApartmentVariable(string input, long lote, M.Medidas mMedidas)
        {
            string valor = "";

            switch (input)
            {
                case "NumEdificio":
                    valor = mMedidas.NumEdificio.ToString();
                    break;
                case "LetraAp":
                    valor = mMedidas.Apartamento;
                    break;
                case "CalleFrente":
                    valor = mMedidas.Calle;
                    break;
                case "NumOficial":
                    valor = mMedidas.NoOficial;
                    break;
                case "NumOficialLetra":
                    valor = mMedidas.NoOficial + "-" + mMedidas.Apartamento;
                    break;
                case "AreaCubPB":
                    valor = mMedidas.CPlantaBaja;
                    break;
                case "AreaCubPA":
                    valor = mMedidas.CPlantaAlta;
                    break;
                case "AreaApartamento":

                    double AreaPB = 0, AreaPA = 0;

                    double.TryParse(mMedidas.CPlantaBaja, out AreaPB);
                    double.TryParse(mMedidas.CPlantaAlta, out AreaPA);

                    valor = (AreaPB + AreaPA).ToString();
                    break;
                case "AreaCubEstac":
                    valor = mMedidas.CEstacionamiento;
                    break;
                case "AreaCubPasillo":
                    valor = mMedidas.CPasillo;
                    break;
                case "AreaCubLav":
                    valor = mMedidas.CLavanderia;
                    break;
                case "AreaCubPatio":
                    valor = mMedidas.CPatio;
                    break;
                case "AreaCubTotal":
                    valor = mMedidas.AreaTotalCubierta;
                    break;
                case "AreaDescEstac":
                    valor = mMedidas.DEstacionamiento;
                    break;
                case "AreaDescPasillo":
                    valor = mMedidas.DPasillo;
                    break;
                case "AreaDescLav":
                    valor = mMedidas.DLavanderia;
                    break;
                case "AreaDescPatio":
                    valor = mMedidas.DPatio;
                    break;
                case "AreaDescTotal":
                    valor = mMedidas.AreaTotalDescubierta;
                    break;
                case "AreaEstac":
                    double EstCub = 0, EstDesc = 0;

                    double.TryParse(mMedidas.CEstacionamiento, out EstCub);
                    double.TryParse(mMedidas.DEstacionamiento, out EstDesc);

                    valor = (EstCub + EstDesc).ToString();
                    break;
                case "AreaPasillo":
                    double PasilloCub = 0, PasilloDesc = 0;

                    double.TryParse(mMedidas.CPasillo, out PasilloCub);
                    double.TryParse(mMedidas.DPasillo, out PasilloDesc);

                    valor = (PasilloCub + PasilloDesc).ToString();
                    break;
                case "AreaLavanderia":
                    double lavCub = 0, lavDesc = 0;

                    double.TryParse(mMedidas.CLavanderia, out lavCub);
                    double.TryParse(mMedidas.DLavanderia, out lavDesc);

                    valor = (lavCub + lavDesc).ToString();

                    break;
                case "AreaPatio":
                    double patioCub = 0, patioDesc = 0;

                    double.TryParse(mMedidas.CPatio, out patioCub);
                    double.TryParse(mMedidas.DPatio, out patioDesc);

                    valor = (patioCub + patioDesc).ToString();

                    break;
                case "TotalCubDesc":
                    valor = mMedidas.AreaCubiertaDescubierta;
                    break;
                case "M2AreaComun":
                    valor = mMedidas.NombreAreaComun;
                    break;
                case "AreaExclusiva":
                    valor = mMedidas.AreaExclusiva;
                    break;
                case "Proinvidiviso":
                    valor = mMedidas.Proindiviso;
                    break;
                case "PredioFrente":
                    valor = mMedidas.PredioFrente;
                    break;
                case "PredioFondo":
                    valor = mMedidas.PredioFondo;
                    break;
                case "PredioArea":
                    valor = mMedidas.PredioArea;
                    break;
                case "AreaConst":
                    valor = mMedidas.AreaConstruccion;
                    break;
                case "ExpCatastral":
                    valor = mMedidas.ExpedienteCatastral;
                    break;
                default:
                    break;

            }
            return valor;
        }

        #endregion
    }
}
