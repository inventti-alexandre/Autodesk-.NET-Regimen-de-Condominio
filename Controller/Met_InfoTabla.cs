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
using System.Text.RegularExpressions;
using RegimenCondominio.M;

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

            if (M.InfoTabla.DiccionarioRumboInverso.TryGetValue(M.Manzana.RumboFrente.rumboActual, out RumboInverso))
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
                    Calle = M.Manzana.RumboFrente.textColindancia,
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
                Descripcion = "Planta Baja Cubierta (m²)",
                esVisible = true
            });

            dictionary.Add(new M.DataColumns()
            {
                TipoEnumerador = typeDetail, Layerseccion = M.Constant.LayerAPAlta,
                PropertyName = Enum.GetName(typeDetail, M.DetailColumns.CPlantaAlta),
                Descripcion = "Planta Alta Cubierta (m²)",
                esVisible = true
            });

            dictionary.Add(new M.DataColumns()
            {
                TipoEnumerador = typeDetail, Layerseccion = M.Constant.LayerLavanderia,
                PropertyName = Enum.GetName(typeDetail, M.DetailColumns.CLavanderia),
                Descripcion = "Lavandería Cubierta (m²)",
                esVisible = true
            });

            dictionary.Add(new M.DataColumns()
            {
                TipoEnumerador = typeDetail, Layerseccion = M.Constant.LayerEstacionamiento,
                PropertyName = Enum.GetName(typeDetail, M.DetailColumns.CEstacionamiento),
                Descripcion = "Estacionamiento Cubierto (m²)",
                esVisible = true
            });

            dictionary.Add(new M.DataColumns()
            {
                TipoEnumerador = typeDetail, Layerseccion = M.Constant.LayerPasillo,
                PropertyName = Enum.GetName(typeDetail, M.DetailColumns.CPasillo),
                Descripcion = "Pasillo Cubierto (m²)",
                esVisible = true
            });

            dictionary.Add(new M.DataColumns()
            {
                TipoEnumerador = typeDetail, Layerseccion = M.Constant.LayerPatio,
                PropertyName = Enum.GetName(typeDetail, M.DetailColumns.CPatio),
                Descripcion = "Patio Cubierto (m²)",
                esVisible = true
            });

            dictionary.Add(new M.DataColumns()
            {
                TipoEnumerador = typeDetail, Layerseccion = M.Constant.LayerLavanderia,
                PropertyName = Enum.GetName(typeDetail, M.DetailColumns.DLavanderia),
                Descripcion = "Lavandería Descubierta (m²)",
                esVisible = true
            });

            dictionary.Add(new M.DataColumns()
            {
                TipoEnumerador = typeDetail, Layerseccion = M.Constant.LayerEstacionamiento,
                PropertyName = Enum.GetName(typeDetail, M.DetailColumns.DEstacionamiento),
                Descripcion = "Estacionamiento Descubierto (m²)",
                esVisible = true
            });

            dictionary.Add(new M.DataColumns()
            {
                TipoEnumerador = typeDetail, Layerseccion = M.Constant.LayerPasillo,
                PropertyName = Enum.GetName(typeDetail, M.DetailColumns.DPasillo),
                Descripcion = "Pasillo Descubierto (m²)",
                esVisible = true
            });

            dictionary.Add(new M.DataColumns()
            {
                TipoEnumerador = typeDetail, Layerseccion = M.Constant.LayerPatio,
                PropertyName = Enum.GetName(typeDetail, M.DetailColumns.DPatio),
                Descripcion = "Patio Descubierto (m²)",
                esVisible = true
            });

            dictionary.Add(new M.DataColumns()
            {
                TipoEnumerador = typeDetail,
                PropertyName = Enum.GetName(typeDetail, M.DetailColumns.NombreAreaComun),
                Layerseccion = M.Constant.LayerAreaComun,
                Descripcion = M.Colindante.NomAreaComun,
                esVisible = true
            });

            #endregion

            #region Sin Layer

            dictionary.Add(new M.DataColumns()
            {
                TipoEnumerador = typeDetail,
                PropertyName = Enum.GetName(typeDetail, M.DetailColumns.AreaTotalCubierta),
                Descripcion = "Área Total Cubierta (m²)",
                esVisible = true
            });

            dictionary.Add(new M.DataColumns()
            {
                TipoEnumerador = typeDetail,
                PropertyName = Enum.GetName(typeDetail, M.DetailColumns.AreaTotalDescubierta),
                Descripcion = "Área Total Descubierta (m²)",
                esVisible = true
            });

            dictionary.Add(new M.DataColumns()
            {
                TipoEnumerador = typeDetail,
                PropertyName = Enum.GetName(typeDetail, M.DetailColumns.AreaCubiertaDescubierta),
                Descripcion = "Total de Área Cubierta y Descubierta (m²)",
                esVisible = true
            });

            dictionary.Add(new M.DataColumns()
            {
                TipoEnumerador = typeDetail,
                PropertyName = Enum.GetName(typeDetail, M.DetailColumns.AreaExclusiva),
                Descripcion = "Área Exclusiva de Terreno (m²)",
                esVisible = M.Manzana.EsMacrolote ? true : false
            });

            dictionary.Add(new M.DataColumns()
            {
                TipoEnumerador = typeDetail,
                PropertyName = Enum.GetName(typeDetail, M.DetailColumns.Proindiviso),
                Descripcion = "Proindiviso (%)",
                esVisible = true
            });

            dictionary.Add(new M.DataColumns()
            {
                TipoEnumerador = typeDetail,
                PropertyName = Enum.GetName(typeDetail, M.DetailColumns.PredioFrente),
                Descripcion = "Frente de Predio (m)",
                esVisible = true
            });

            dictionary.Add(new M.DataColumns()
            {
                TipoEnumerador = typeDetail,
                PropertyName = Enum.GetName(typeDetail, M.DetailColumns.PredioFondo),
                Descripcion = "Fondo de Predio (m)",
                esVisible = true
            });

            dictionary.Add(new M.DataColumns()
            {
                TipoEnumerador = typeDetail,
                PropertyName = Enum.GetName(typeDetail, M.DetailColumns.PredioArea),
                Descripcion = "Área de Predio (m)",
                esVisible = true
            });

            dictionary.Add(new M.DataColumns()
            {
                TipoEnumerador = typeDetail,
                PropertyName = Enum.GetName(typeDetail, M.DetailColumns.AreaConstruccion),
                Descripcion = "Área de Construcción (m²)",
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

            HashSet<string> seccionesUnicasPlano;
            //-------------------------------------------

            //Obtengo todas las secciones
            seccionesPlano = M.Colindante.MainData.Select(x => x.LayerSeccion).ToList();

            //Las asigno en hash para sólo tener secciones únicas
            seccionesUnicasPlano = new HashSet<string>(seccionesPlano);

            //Encuentro todas las columnas disponibles sólo en las Columnas del detalle
            seccionesAOcultar = DescribeColumns().Where(x => x.TipoEnumerador == typeof(M.DetailColumns)
                                                        && !string.IsNullOrWhiteSpace(x.Layerseccion)).ToList();

            //Empiezo a eliminar las secciones que no se encuentren
            foreach (string seccionActual in seccionesUnicasPlano)
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
            foreach (KeyValuePair<string, bool> propActual in dColumnas)
            {
                //Agrego la columna a la tabla------------------------------------------
                Table[0, colCount] = M.InfoTabla.DescripcionPropiedades[propActual.Key];

                //Si tiene total lo agrego a la tabla----------------------------------                
                if (propActual.Value)
                {
                    //Obtengo la columna a localizar
                    M.DetailColumns dtCol = dIndexColumns[propActual.Key];

                    foreach (PropertyDescriptor propTotal in propertiesTotals)
                    {
                        M.Totales mTotalActual = (M.Totales)propTotal.GetValue(M.InfoTabla.TotalesTabla);

                        if (mTotalActual != null && mTotalActual.Columna == dtCol)
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
                            Table[i + 1, col] = (prop.GetValue(item) ?? DBNull.Value).ToString();
                            col++;
                        }
                    }

                }
            }

            return Table;

        }

        private static List<M.LoteDetail> GetUniqueItems(this List<M.LoteDetail> initialList)
        {
            for (int i = 0; i < initialList.Count; i++)
            {
                M.LoteDetail currenItem = initialList[i];

                for (int j = initialList.Count - 1; j >= 0; j--)
                {
                    if (j != i)
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
            M.InfoTabla.LotesSelected.Clear();

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

                    M.InfoTabla.LotesSelected.Add(new M.Checked<M.LoteItem>()
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

        internal static Dictionary<string, decimal> CalculatePropertyTotals(CollectionView view)
        {
            Dictionary<string, decimal> propertyTotals = new Dictionary<string, decimal>();

            PropertyDescriptorCollection propCollection = TypeDescriptor.GetProperties(typeof(M.Medidas));

            //Por cada item dentro de la vista
            foreach (M.Medidas medidaActual in view)
            {
                //Por cada Propiedad del item Medidas
                foreach (PropertyDescriptor prop in propCollection)
                {
                    bool esVisible = false;
                    decimal numeroCelda = 0;

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
                            if (decimal.TryParse(valorCelda, out numeroCelda))
                                propertyTotals[prop.Name] += numeroCelda;
                        }
                    }
                }
            }

            return propertyTotals;
        }

        private static void GetTotals(this long lote)
        {
            decimal totalCubPB = 0.0M,
                    totalCubPA = 0.0M,
                    totalCubEstac = 0.0M,
                    totalCubPasillo = 0.0M,
                    totalCubLavanderia = 0.0M,
                    totalCubPatio = 0.0M,
                    totalAreaCub = 0.0M,
                    totalDescEstac = 0.0M,
                    totalDescPasillo = 0.0M,
                    totalDescLavanderia = 0.0M,
                    totalDescPatio = 0.0M,
                    totalAreaDesc = 0.0M,
                    totalAreaCubDesc = 0.0M,
                    totalAreaComun = 0.0M,
                    totalAreaExclusiva = 0.0M,
                    totalProindiviso = 0.0M,
                    totalAreaConst = 0.0M;

            foreach (M.Medidas mMedidas in M.InfoTabla.MedidasGlobales)
            {
                if (mMedidas.LongLote == lote)
                {
                    totalCubPB += mMedidas.CPlantaBaja.ToDecimal();//Planta Baja Cubierta
                    totalCubPA += mMedidas.CPlantaAlta.ToDecimal();//Planta Alta Cubierta
                    totalCubEstac += mMedidas.CEstacionamiento.ToDecimal();//Cajón de Estacionamiento Cubierto
                    totalCubPasillo += mMedidas.CPasillo.ToDecimal();//Pasillo Cubierto
                    totalCubLavanderia += mMedidas.CLavanderia.ToDecimal(); //Lavandería Cubierta
                    totalCubPatio += mMedidas.CPatio.ToDecimal(); //Patio Cubierto
                    totalAreaCub += mMedidas.AreaTotalCubierta.ToDecimal(); //Suma de todas las Áreas Cubiertas
                    totalDescEstac += mMedidas.DEstacionamiento.ToDecimal(); //Estacionamiento Descubierto
                    totalDescPasillo += mMedidas.DPasillo.ToDecimal(); //Pasillo Descubierto
                    totalDescLavanderia += mMedidas.DLavanderia.ToDecimal(); // Lavandería Descubierta
                    totalDescPatio += mMedidas.DPatio.ToDecimal(); //Patio Descubierto
                    totalAreaDesc += mMedidas.AreaTotalDescubierta.ToDecimal(); //Suma de todas las Áreas Cubiertas
                    totalAreaCubDesc += mMedidas.AreaCubiertaDescubierta.ToDecimal(); //Suma de Área Cubierta y Descubierta
                    totalAreaComun += mMedidas.NombreAreaComun.ToDecimal(); // Área Común
                    totalAreaExclusiva += mMedidas.AreaExclusiva.ToDecimal(); // Área Exclusiva
                    totalProindiviso += mMedidas.Proindiviso.ToDecimal(); //Proindiviso
                    totalAreaConst += mMedidas.AreaConstruccion.ToDecimal(); //Área de Construcción
                }
            }

            M.InfoTabla.TotalesTabla = new TotalesMedidas()
            {
                TotalCubPB = new Totales() { Columna = DetailColumns.CPlantaBaja, Total = totalCubPB },
                TotalCubPA = new Totales() { Columna = DetailColumns.CPlantaAlta, Total = totalCubPA },
                TotalCubEstac = new Totales() { Columna = DetailColumns.CEstacionamiento, Total = totalCubEstac },
                TotalCubPasillo = new Totales() { Columna = DetailColumns.CPasillo, Total = totalCubPasillo },
                TotalCubLavanderia = new Totales() { Columna = DetailColumns.CLavanderia, Total = totalCubLavanderia },
                TotalCubPatio = new Totales() { Columna = DetailColumns.CPatio, Total = totalCubPatio },
                TotalAreaCub = new Totales() { Columna = DetailColumns.AreaTotalCubierta, Total = totalAreaCub },
                TotalDescEstac = new Totales() { Columna = DetailColumns.DEstacionamiento, Total = totalDescEstac },
                TotalDescPasillo = new Totales() { Columna = DetailColumns.DPasillo, Total = totalDescPasillo },
                TotalDescLavanderia = new Totales() { Columna = DetailColumns.DLavanderia, Total = totalDescLavanderia },
                TotalDescPatio = new Totales() { Columna = DetailColumns.DPatio, Total = totalDescPatio },
                TotalAreaDesc = new Totales() { Columna = DetailColumns.AreaTotalDescubierta, Total = totalAreaDesc },
                TotalAreaCubDesc = new Totales() { Columna = DetailColumns.AreaCubiertaDescubierta, Total = totalAreaCubDesc },
                TotalAreaComun = new Totales() { Columna = DetailColumns.NombreAreaComun, Total = totalAreaComun },
                TotalAreaExclusiva = new Totales() { Columna = DetailColumns.AreaExclusiva, Total = totalAreaExclusiva },
                TotalProindiviso = new Totales() { Columna = DetailColumns.Proindiviso, Total = totalProindiviso },
                TotalAreaConst = new Totales() { Columna = DetailColumns.AreaConstruccion, Total = totalAreaConst }
            };
        }

        internal static void ModificarExpCatastral(long lote, string expCatastral = null, int id=-1)
        {
            for (int i = 0; i < M.Colindante.OrderedApartments.Count; i++)
            {
                string apartment = (i + 1).ToEnumerate();

                foreach (M.Medidas Medidas in M.InfoTabla.MedidasGlobales.Where(x => x.LongLote == lote && x.Apartamento == apartment))
                {
                    if (id != -1)
                    {
                        Medidas.ExpedienteCatastral = string.Format("{0}-{1}-{2}", M.Inicio.Region, M.Manzana.NoManzana.ToString(), id);
                        id++;
                    }
                    else
                        Medidas.ExpedienteCatastral = (expCatastral ?? "");
                }
            }
            
        }

        internal static decimal ToDecimal(this string _string)
        {
            decimal _decimal = 0.0M;

            if (!string.IsNullOrWhiteSpace(_string) && _string.Replace(".", "").All(c => char.IsDigit(c)))
                Convert.ToDecimal(_string);

            return _decimal;    
        }

        internal static void DuplicateEditedValues(M.Medidas itemEdited, decimal valueEdited, PropertyInfo propToUpdate)
        {
            //Revisar los Lote que no son Base
            List<long> listNotBase = M.InfoTabla.LotesSelected.Where(x => !x.Item.EsLoteBase).Select(x => x.Item.Long).ToList();

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


        internal static bool UpdateHorizontalTotals (M.Medidas mItemEditado, M.DetailColumns colEditada, decimal numIntroducido,
                                                        out List<M.Totales> listVerticalTotals)
        {
            int enumTotalAreaCub = (int)M.DetailColumns.AreaTotalCubierta,
                enumTotalAreaDesc = (int)M.DetailColumns.AreaTotalDescubierta,
                enumEditado = (int)colEditada;

            decimal sumaHorizontalAreaCub = 0,
                    sumaHorizontalAreaDesc = 0,
                    sumaHorizontalAmbos = 0;

            decimal sumaVerticalAreaCub = 0,
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
                                decimal numValue = 0;

                                if (decimal.TryParse(objProp.ToString(), out numValue))
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
                                decimal numValue = 0;

                                if (decimal.TryParse(objProp.ToString(), out numValue))
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
                    sumaHorizontalAreaCub = mItemEditado.AreaTotalCubierta != null ? decimal.Parse(mItemEditado.AreaTotalCubierta) : 0;

                //Si la modificación no se encontró en el Área Descubierta la obtengo desde el Item
                if (sumaHorizontalAreaDesc == 0)
                    sumaHorizontalAreaDesc = mItemEditado.AreaTotalDescubierta != null ? decimal.Parse(mItemEditado.AreaTotalCubierta) : 0;

                //Sumo ambos y cambio la propiedad Área Total Descubierta + Total Cubierta       
                propertyToModify = typeof(M.Medidas).GetProperty(M.DetailColumns.AreaCubiertaDescubierta.ToString());

                sumaHorizontalAmbos = sumaHorizontalAreaCub + sumaHorizontalAreaDesc;

                if (propertyToModify != null)
                    propertyToModify.SetValue(mItemEditado, sumaHorizontalAmbos.ToString());

                foreach (M.Medidas mMedidasItem in M.InfoTabla.MedidasGlobales)
                {
                    if (mMedidasItem.LongLote == mItemEditado.LongLote && mMedidasItem.Apartamento != mItemEditado.Apartamento)
                    {
                        sumaVerticalAreaCub += decimal.Parse((mMedidasItem.AreaTotalCubierta ?? "0"));
                        sumaVerticalAreaDesc += decimal.Parse((mMedidasItem.AreaTotalDescubierta ?? "0"));
                        SumaVerticalAmbos += decimal.Parse((mMedidasItem.AreaCubiertaDescubierta ?? "0"));
                    }
                }

                sumaVerticalAreaCub += sumaHorizontalAreaCub;
                sumaVerticalAreaDesc += sumaHorizontalAreaDesc;
                SumaVerticalAmbos += sumaHorizontalAmbos;

                //Cálculo Proindiviso
                foreach(M.Medidas mMedidasItem in M.InfoTabla.MedidasGlobales)
                {
                    if (mMedidasItem.LongLote == mItemEditado.LongLote)
                    {
                        if(mMedidasItem.Apartamento != mItemEditado.Apartamento)
                        {
                            decimal sumaCubDesc = decimal.Parse(mMedidasItem.AreaCubiertaDescubierta ?? "0");

                            if (SumaVerticalAmbos > 0)
                                mMedidasItem.Proindiviso = ((sumaCubDesc * 100) / SumaVerticalAmbos).ToString();
                            else
                                mMedidasItem.Proindiviso = "0";
                        }
                        else
                        {
                            decimal sumaCubDesc = decimal.Parse(mItemEditado.AreaCubiertaDescubierta ?? "0");

                            if (SumaVerticalAmbos > 0)
                                mMedidasItem.Proindiviso = ((sumaCubDesc * 100) / SumaVerticalAmbos).ToString();
                            else
                                mMedidasItem.Proindiviso = "0";
                        }
                    }

                }

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

        internal static bool HasEmptyFields(long longActual, out string msgs, out string titulo)
        {
            msgs = "";
            titulo = "";
            
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
                                else if (objProp is string)
                                {
                                    string sValue = (string)objProp;

                                    if (string.IsNullOrWhiteSpace(sValue))
                                    {
                                        propApartments.Add(new Tuple<string, string>(dtC.Descripcion, mItemActual.Apartamento));
                                        //msgs.Add(string.Format("Falta valor en {0} Apartamento {1}", dtC.Descripcion,mItemActual.Apartamento));
                                        hasEmptyFields = true;
                                    }
                                    else if(prop.Name.ToUpper() == "ExpedienteCatastral".ToUpper())
                                    {
                                        if(sValue[sValue.Length - 1] == '-')
                                        {
                                            propApartments.Add(new Tuple<string, string>(dtC.Descripcion, mItemActual.Apartamento));
                                            hasEmptyFields = true;
                                        }
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
                int numLote = M.Colindante.Lotes.Search(longActual).numLote;

                titulo = "Faltan datos del Lote: " + numLote;
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

        internal static bool CrearMachotes(List<long> lotes, List<M.Bloques> bloques, List<M.Variables> variables,
                                               string filePath)
        {
            try
            {
                foreach (long loteActual in lotes)
                {
                    //Inicializar variables-----------------------------------------------------------
                    List<M.Variables> varsGlobales = new List<M.Variables>();
                    List<Tuple<M.Variables, string>> varsAps = new List<Tuple<M.Variables, string>>();
                    HashSet<string> ApsLote = new HashSet<string>();

                    M.InfoTabla.BloquesCalculados = new List<M.Bloques>();

                    string fileName = loteActual.ToString();
                    //--------------------------------------------------------------------------------

                    if (lotes.Count > 1)
                        loteActual.GetTotals();//Obtener los totales por si se deben de utilizar en las variables

                    //Obtengo todas las variables Globales
                    foreach (M.Variables varActual in variables)
                    {
                        if (varActual.NomTipoBloque.ToUpper() == "GLOBAL")
                        {
                            if (varActual.EsCalculado)
                            {
                                string valorVar = GetGlobalVariable(varActual, loteActual);

                                varsGlobales.Add(new M.Variables()
                                {
                                    ConvLetra = varActual.ConvLetra,
                                    EsCalculado = varActual.EsCalculado,
                                    NombreCorto = varActual.NombreCorto,
                                    NombreVariable = varActual.NombreVariable,
                                    NomTipoBloque = varActual.NomTipoBloque,
                                    DescUnidad  = varActual.DescUnidad,
                                    NomCortoUnidad = varActual.NomCortoUnidad,
                                    RepUnidad = varActual.RepUnidad,
                                    Valor = valorVar
                                });

                            }
                            else
                                varsGlobales.Add(varActual);
                        }
                        else if (varActual.NomTipoBloque.ToUpper() == "APARTAMENTO")
                        {
                            foreach (M.Medidas mMedidas in M.InfoTabla.MedidasGlobales)
                            {
                                if (mMedidas.LongLote == loteActual)
                                {
                                    string valorAp = GetApartmentVariable(varActual, loteActual, mMedidas);

                                    M.Variables varAPActual = new M.Variables() {
                                        ConvLetra = varActual.ConvLetra,
                                        EsCalculado = varActual.EsCalculado,
                                        NombreCorto = varActual.NombreCorto,                                        
                                        NombreVariable = varActual.NombreVariable,
                                        NomTipoBloque = varActual.NomTipoBloque,
                                        DescUnidad = varActual.DescUnidad,
                                        NomCortoUnidad = varActual.NomCortoUnidad,
                                        RepUnidad = varActual.RepUnidad,
                                        Valor = valorAp
                                    };

                                    varsAps.Add(new Tuple<M.Variables, string>(varAPActual, mMedidas.Apartamento));

                                    ApsLote.Add(mMedidas.Apartamento);
                                }
                            }
                        }
                    }

                    int nuevoOrden = 1;

                    foreach (M.Bloques bloque in bloques.OrderBy(x => x.Orden))
                    {
                        string descripcion = bloque.Descripcion;

                        if (bloque.NomTipoBLoque.ToUpper() == "GLOBAL")
                        {
                            foreach (M.Variables varGlobal in varsGlobales)
                            {
                                string valor = varGlobal.Valor == Environment.NewLine ? varGlobal.Valor : "[" + varGlobal.Valor + "]";
                                descripcion = descripcion.Replace("[" + varGlobal.NombreCorto + "]", valor);
                            }

                            M.InfoTabla.BloquesCalculados.Add(new M.Bloques()
                            {
                                Descripcion = descripcion,
                                IdBloque = bloque.IdBloque,
                                IdTipoBloque = bloque.IdTipoBloque,
                                NomTipoBLoque = bloque.NomTipoBLoque,
                                Orden = nuevoOrden
                            });

                            nuevoOrden++;
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
                                    string valor = "[" + varGlobal.Valor + "]";

                                    descApartment = descApartment.Replace("[" + varGlobal.NombreCorto + "]", valor);
                                }

                                M.InfoTabla.BloquesCalculados.Add(new M.Bloques()
                                {
                                    Descripcion = descApartment,
                                    IdBloque = bloque.IdBloque,
                                    IdTipoBloque = bloque.IdTipoBloque,
                                    NomTipoBLoque = bloque.NomTipoBLoque,
                                    Orden = nuevoOrden
                                });

                                nuevoOrden++;
                            }
                        }
                    }

                    M.Lote lote = M.Colindante.Lotes.Search(loteActual);

                    if (lote != null)
                        fileName = filePath + "\\" + string.Format("Manzana {0} Lote {1}", M.Manzana.NoManzana, lote.numLote) + ".docx";
                    else
                        fileName = filePath + "\\" + loteActual.ToString() + ".docx";

                    if (!M.InfoTabla.BloquesCalculados.ToWord(fileName))
                        return false;
                }
            }
            catch(Exception ex)
            {
                ex.Message.ToEditor();
                return false;
            }

            return true;
        }

        public static List<long> ListLongs;


        #region Catalogo de Apartamentos y Globales

        private static string GetGlobalVariable(M.Variables variable, long lote)
        {
            string valor = "";

            //case "":
            //  break;

            switch (variable.NombreCorto)
            {
                case "NumLote": //Número del Lote
                    M.Lote loteActual = M.Colindante.Lotes.Search(lote);

                    if (loteActual != null)
                        valor = loteActual.numLote.ToString();
                    break;
                case "Fraccionamiento": //Fraccionamiento
                    valor = M.Inicio.Fraccionamiento.fraccionamiento.FormatString();
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
                case "NumManzana":
                    valor = M.Manzana.NoManzana.ToString();
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
                case "SeccionAreaComun":
                    valor = ObtenerDescAreaComun(variable, lote, M.Constant.LayerAreaComun);
                    break;
                case "SaltoLinea":
                    valor = M.Constant.LineasXParrafo;
                    break;
                case "SaltoParrafo":
                    valor = Environment.NewLine;
                    break;
                case "TotalCubPB":
                    valor = ObtenerMetros(variable.ConvLetra, M.InfoTabla.TotalesTabla.TotalCubPB.Total.ToString(), 
                                                    variable.RepUnidad, variable.NomCortoUnidad);
                    break;
                case "TotalCubPA":
                    valor = ObtenerMetros(variable.ConvLetra, M.InfoTabla.TotalesTabla.TotalCubPA.Total.ToString(),
                                                    variable.RepUnidad, variable.NomCortoUnidad);
                    break;
                case "TotalCubEstac":
                    valor = ObtenerMetros(variable.ConvLetra, M.InfoTabla.TotalesTabla.TotalCubEstac.Total.ToString(),
                                                    variable.RepUnidad, variable.NomCortoUnidad);
                    break;
                case "TotalCubPasillo":
                    valor = ObtenerMetros(variable.ConvLetra, M.InfoTabla.TotalesTabla.TotalCubPasillo.Total.ToString(),
                                                    variable.RepUnidad, variable.NomCortoUnidad);
                    break;
                case "TotalCubLavanderia":
                    valor = ObtenerMetros(variable.ConvLetra, M.InfoTabla.TotalesTabla.TotalCubLavanderia.Total.ToString(),
                                                    variable.RepUnidad, variable.NomCortoUnidad);
                    break;
                case "TotalCubPatio":
                    valor = ObtenerMetros(variable.ConvLetra, M.InfoTabla.TotalesTabla.TotalCubPatio.Total.ToString(),
                                                    variable.RepUnidad, variable.NomCortoUnidad);
                    break;
                case "TotalAreaCub":
                    valor = ObtenerMetros(variable.ConvLetra, M.InfoTabla.TotalesTabla.TotalAreaCub.Total.ToString(),
                                                    variable.RepUnidad, variable.NomCortoUnidad);
                    break;
                case "TotalDescEstac":
                    valor = ObtenerMetros(variable.ConvLetra, M.InfoTabla.TotalesTabla.TotalDescEstac.Total.ToString(),
                                                    variable.RepUnidad, variable.NomCortoUnidad);
                    break;
                case "TotalDescPasillo":
                    valor = ObtenerMetros(variable.ConvLetra, M.InfoTabla.TotalesTabla.TotalDescPasillo.Total.ToString(),
                                                    variable.RepUnidad, variable.NomCortoUnidad);
                    break;
                case "TotalDescLavanderia":
                    valor = ObtenerMetros(variable.ConvLetra, M.InfoTabla.TotalesTabla.TotalDescLavanderia.Total.ToString(),
                                                    variable.RepUnidad, variable.NomCortoUnidad);
                    break;
                case "TotalDescPatio":
                    valor = ObtenerMetros(variable.ConvLetra, M.InfoTabla.TotalesTabla.TotalDescPatio.Total.ToString(),
                                                    variable.RepUnidad, variable.NomCortoUnidad);
                    break;
                case "TotalAreaDesc":
                    valor = ObtenerMetros(variable.ConvLetra, M.InfoTabla.TotalesTabla.TotalAreaDesc.Total.ToString(),
                                                    variable.RepUnidad, variable.NomCortoUnidad);
                    break;
                case "TotalCubDesc":
                    valor = ObtenerMetros(variable.ConvLetra, M.InfoTabla.TotalesTabla.TotalAreaCubDesc.Total.ToString(),
                                                    variable.RepUnidad, variable.NomCortoUnidad);
                    break;
                case "TotalAreaComun":
                    valor = ObtenerMetros(variable.ConvLetra, M.InfoTabla.TotalesTabla.TotalAreaComun.Total.ToString(),
                                                    variable.RepUnidad, variable.NomCortoUnidad);
                    break;
                case "TotalAreaExclusiva":
                    valor = ObtenerMetros(variable.ConvLetra, M.InfoTabla.TotalesTabla.TotalAreaExclusiva.Total.ToString(),
                                                    variable.RepUnidad, variable.NomCortoUnidad);
                    break;
                case "TotalProindiviso":
                    valor = ObtenerMetros(variable.ConvLetra, M.InfoTabla.TotalesTabla.TotalProindiviso.Total.ToString(),
                                                    variable.RepUnidad, variable.NomCortoUnidad);
                    break;
                case "TotalAreaConst":
                    valor = ObtenerMetros(variable.ConvLetra, M.InfoTabla.TotalesTabla.TotalAreaConst.Total.ToString(),
                                                    variable.RepUnidad, variable.NomCortoUnidad);
                    break;                
                default:
                    valor = "";
                    break;                   
            }
            return valor;
        }

        internal static bool EnviarInfoBD()
        {
            throw new NotImplementedException();
        }

        private static string GetApartmentVariable(M.Variables variable, long lote, M.Medidas mMedidas)
        {
            string  valor = "",
                    strEntero, 
                    strDecimales;

            decimal valorA = 0M,
                    valorB = 0M,
                    sumValores = 0M;

            switch (variable.NombreCorto)
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

                    int numOficial;

                    if (int.TryParse(mMedidas.NoOficial, out numOficial))
                    {
                        decimal.Parse(numOficial.ToString()).EnLetra(out strEntero, out strDecimales);

                        valor = mMedidas.NoOficial + string.Format(" ({0})", strEntero);
                    }
                    else {
                        valor = mMedidas.NoOficial;
                    }
                    break;
                case "NumOficialLetra":

                    if (int.TryParse(mMedidas.NoOficial, out numOficial))
                    {
                        decimal.Parse(numOficial.ToString()).EnLetra(out strEntero, out strDecimales);

                        valor = string.Format("{0}-{1} ({2} guion letra {3})", mMedidas.NoOficial, mMedidas.Apartamento, 
                                                                        strEntero, mMedidas.Apartamento);
                    }
                    else {
                        valor = mMedidas.NoOficial + "-" + mMedidas.Apartamento;
                    }
                    
                    break;
                case "AreaCubPB_AP":
                    valor =  ObtenerMetros(variable.ConvLetra, mMedidas.CPlantaBaja, variable.RepUnidad, variable.NomCortoUnidad);                                       
                    break;
                case "AreaCubPA_AP":
                    valor = ObtenerMetros(variable.ConvLetra, mMedidas.CPlantaAlta, variable.RepUnidad, variable.NomCortoUnidad);                    
                    break;                
                case "AreaCubEstac_AP":
                    valor = ObtenerMetros(variable.ConvLetra, mMedidas.CEstacionamiento, variable.RepUnidad, variable.NomCortoUnidad);
                    break;
                case "AreaCubPasillo_AP":
                    valor = ObtenerMetros(variable.ConvLetra, mMedidas.CPasillo, variable.RepUnidad, variable.NomCortoUnidad);
                    break;
                case "AreaCubLav_AP":
                    valor = ObtenerMetros(variable.ConvLetra, mMedidas.CLavanderia, variable.RepUnidad, variable.NomCortoUnidad); 
                    break;
                case "AreaCubPatio_AP":
                    valor = ObtenerMetros(variable.ConvLetra, mMedidas.CPatio, variable.RepUnidad, variable.NomCortoUnidad); 
                    break;
                case "AreaCubTotal_AP":
                    valor = ObtenerMetros(variable.ConvLetra, mMedidas.AreaTotalCubierta, variable.RepUnidad, variable.NomCortoUnidad);  
                    break;
                case "AreaDescEstac_AP":
                    valor = ObtenerMetros(variable.ConvLetra, mMedidas.DEstacionamiento, variable.RepUnidad, variable.NomCortoUnidad); 
                    break;
                case "AreaDescPasillo_AP":
                    valor = ObtenerMetros(variable.ConvLetra, mMedidas.DPasillo, variable.RepUnidad, variable.NomCortoUnidad);  
                    break;
                case "AreaDescLav_AP":
                    valor = ObtenerMetros(variable.ConvLetra, mMedidas.DLavanderia, variable.RepUnidad, variable.NomCortoUnidad); 
                    break;
                case "AreaDescPatio_AP":
                    valor = ObtenerMetros(variable.ConvLetra, mMedidas.DPatio, variable.RepUnidad, variable.NomCortoUnidad); 
                    break;
                case "AreaDescTotal_AP":
                    valor = ObtenerMetros(variable.ConvLetra, mMedidas.AreaTotalDescubierta, variable.RepUnidad, variable.NomCortoUnidad) ;
                    break;
                case "AreaDescCub_AP":
                    valor = ObtenerMetros(variable.ConvLetra, mMedidas.AreaCubiertaDescubierta, variable.RepUnidad, variable.NomCortoUnidad);
                    break;
                case "SumaPA_PB":

                    if (decimal.TryParse(mMedidas.CPlantaBaja, out valorA) && decimal.TryParse(mMedidas.CPlantaAlta, out valorB))
                    {
                        sumValores = (valorA + valorB);

                        valor = ObtenerMetros(variable.ConvLetra, sumValores.ToString(), variable.RepUnidad, variable.NomCortoUnidad);
                    }

                    break;
                case "AreaDCEstac_AP":                                      

                    if (decimal.TryParse(mMedidas.CEstacionamiento, out valorA) && decimal.TryParse(mMedidas.DEstacionamiento, out valorB))
                    {
                        sumValores = (valorA + valorB);

                        valor = ObtenerMetros(variable.ConvLetra, sumValores.ToString(), variable.RepUnidad, variable.NomCortoUnidad);
                    }                       
                    break;
                case "AreaDCPasillo_AP":                                      

                    if (decimal.TryParse(mMedidas.CPasillo, out valorA) 
                        && decimal.TryParse(mMedidas.DPasillo, out valorB))
                    {
                        sumValores = (valorA + valorB);

                        valor = ObtenerMetros(variable.ConvLetra, sumValores.ToString(), variable.RepUnidad, variable.NomCortoUnidad);
                    }
                                            
                    break;
                case "AreaDCLavanderia_AP":                    

                    if (decimal.TryParse(mMedidas.CLavanderia, out valorA)
                        && decimal.TryParse(mMedidas.DLavanderia, out valorB))
                    {
                        sumValores = (valorA + valorB);

                        valor = ObtenerMetros(variable.ConvLetra, sumValores.ToString(), variable.RepUnidad, variable.NomCortoUnidad);
                    }

                        break;
                case "AreaDCPatio_AP":                                       

                    if (decimal.TryParse(mMedidas.CPatio, out valorA)
                        && decimal.TryParse(mMedidas.DPatio, out valorB))
                    {
                        sumValores = (valorA + valorB);

                        valor = ObtenerMetros(variable.ConvLetra, sumValores.ToString(), variable.RepUnidad, variable.NomCortoUnidad);
                    }

                    break;                
                case "M2AreaComun":
                    valor = ObtenerMetros(variable.ConvLetra, mMedidas.NombreAreaComun, variable.RepUnidad, variable.NomCortoUnidad); 
                    break;
                case "AreaExclusiva":
                    valor = ObtenerMetros(variable.ConvLetra, mMedidas.AreaExclusiva, variable.RepUnidad, variable.NomCortoUnidad);
                    break;
                case "Proinvidiviso":
                    valor = ObtenerMetros(variable.ConvLetra, mMedidas.Proindiviso, variable.RepUnidad, variable.NomCortoUnidad);
                    break;
                case "PredioFrente":
                    valor = ObtenerMetros(variable.ConvLetra, mMedidas.PredioFrente, variable.RepUnidad, variable.NomCortoUnidad) ;
                    break;
                case "PredioFondo":
                    valor = ObtenerMetros(variable.ConvLetra, mMedidas.PredioFondo, variable.RepUnidad, variable.NomCortoUnidad);
                    break;
                case "PredioArea":
                    valor = ObtenerMetros(variable.ConvLetra, mMedidas.PredioArea, variable.RepUnidad, variable.NomCortoUnidad);
                    break;
                case "AreaConst":
                    valor = ObtenerMetros(variable.ConvLetra, mMedidas.AreaConstruccion, variable.RepUnidad, variable.NomCortoUnidad);
                    break;
                case "ExpCatastral":
                    valor = mMedidas.ExpedienteCatastral;
                    break;
                case "SeccionAreaExc":
                    valor = ObtenerDescPuntos(variable, lote, mMedidas.Apartamento, M.Constant.LayerApartamento);
                    break;                
                case "SeccionPB":
                    valor = ObtenerDescPuntos(variable, lote, mMedidas.Apartamento, M.Constant.LayerAPBaja);
                    break;
                case "SeccionPA":
                    valor = ObtenerDescPuntos(variable, lote, mMedidas.Apartamento, M.Constant.LayerAPAlta);
                    break;
                case "SeccionLav":
                    valor = ObtenerDescPuntos(variable, lote, mMedidas.Apartamento, M.Constant.LayerLavanderia);
                    break;
                case "SeccionEstac":
                    valor = ObtenerDescPuntos(variable, lote, mMedidas.Apartamento, M.Constant.LayerEstacionamiento);
                    break;
                case "SeccionPasillo":
                    valor = ObtenerDescPuntos(variable, lote, mMedidas.Apartamento, M.Constant.LayerPasillo);
                    break;
                case "SeccionPatio":
                    valor = ObtenerDescPuntos(variable, lote, mMedidas.Apartamento, M.Constant.LayerPatio);
                    break;
                default:
                    break;

            }
            return valor;
        }

        private static string ObtenerDescPuntos(Variables variable, long lote, string apartamento, string layerSeccion)
        {
            string valorDescriptivo = "";

            List<M.ColindanciaData> colindanteInfo = new List<ColindanciaData>();

            List<string> orientaciones = new List<string>(),
                         rumbosDescritos = new List<string>();

            Dictionary<string, int> cantOrientaciones = new Dictionary<string, int>();

            foreach(M.ColindanciaData mDataCol in M.Colindante.MainData)
            {
                if(M.Manzana.EsMacrolote)
                {
                    if (mDataCol.Apartamento.GetAfterSpace() == apartamento
                        && mDataCol.LayerSeccion == layerSeccion)
                        colindanteInfo.Add(mDataCol);
                }
                else
                {
                    if (mDataCol.idVivienda == lote
                        && mDataCol.Apartamento.GetAfterSpace() == apartamento
                        && mDataCol.LayerSeccion == layerSeccion)
                        colindanteInfo.Add(mDataCol);
                }                
            }

            orientaciones = Met_Manzana.OrientacionFrente(M.Manzana.RumboFrente.rumboActual);

            foreach(string orientacion in orientaciones)
            {
                int cantOrientacion = 0;

                List<string> tramos = new List<string>();

                M.ColindanciaData mSingleData = new ColindanciaData();

                string describeDistancia = "",
                        describeOrientacion = "";

                foreach (M.ColindanciaData mDataCol in colindanteInfo)
                {
                    if (mDataCol.Rumbo == orientacion)
                    {
                        string  tramo = "",
                                ordinal = "";                       

                        describeDistancia = "";

                        cantOrientacion++;

                        mSingleData = mDataCol;

                        ordinal = cantOrientacion.ToOrdinalNumber();

                        describeDistancia = ObtenerMetros(variable.ConvLetra, mDataCol.Distancia.ToString(),
                                                    variable.RepUnidad, variable.NomCortoUnidad);                        

                        if (mDataCol.esArco)
                            tramo = string.Format("el {0} del punto {1} al punto {2} es una línea curva que mide {3} a colindar con {4}",
                                        ordinal, mDataCol.PuntoA, mDataCol.PuntoB, describeDistancia, DescribeSeccion(mDataCol.Colindancia));
                        else
                            tramo = string.Format("el {0} del punto {1} al punto {2} que mide {3} a colindar con {4}",
                                        ordinal, mDataCol.PuntoA, mDataCol.PuntoB, describeDistancia, DescribeSeccion(mDataCol.Colindancia));


                        tramos.Add(tramo);
                    }
                }

                if(cantOrientacion == 1)
                {
                    
                    if (mSingleData.esArco)
                        describeOrientacion = string.Format("Al {0} del punto {1} al punto {2} es una línea curva que mide {3} a colindar con {4}",
                                    orientacion.ToUpper(), mSingleData.PuntoA, mSingleData.PuntoB, describeDistancia, DescribeSeccion(mSingleData.Colindancia));
                    else
                        describeOrientacion = string.Format("Al {0} del punto {1} al punto {2} que mide {3} a colindar con {4}",
                                    orientacion.ToUpper(), mSingleData.PuntoA, mSingleData.PuntoB, describeDistancia, DescribeSeccion(mSingleData.Colindancia));
                }
                else
                {
                    string  entero = "",
                            _decimal = "",
                            tramosDescritos = "";

                    decimal dCanOrientacion = Convert.ToDecimal(cantOrientacion);

                    dCanOrientacion.EnLetra(out entero, out _decimal);

                    tramosDescritos = string.Join(", ", tramos);


                    describeOrientacion = string.Format("Al {0} en {1} tramos, {2}",
                                                        orientacion.ToUpper(), entero.ToLower(), tramosDescritos);
                }

                rumbosDescritos.Add(describeOrientacion);
                
            }

            valorDescriptivo = string.Join("; ", rumbosDescritos);

            return valorDescriptivo;
        }

        internal static string ObtenerDescAreaComun(Variables variable, long lote, string layerSeccion)
        {
            //Inicializo variables globales-------------------------------------
            string valorDescriptivo = "";

            List<M.ColindanciaData> colindanteInfo = new List<ColindanciaData>();

            List<string> orientaciones = new List<string>(),
                         rumbosDescritos = new List<string>(),
                         descAreasComunes = new List<string>();                 
            //------------------------------------------------------------------       

            //Obtengo solamente los datos que representen a las Áreas Comúnes
            foreach (M.ColindanciaData mDataCol in M.Colindante.MainData)
            {
                if (M.Manzana.EsMacrolote)
                {
                    if (mDataCol.LayerSeccion == layerSeccion && mDataCol.Edificio_Lote == 0)                    
                        colindanteInfo.Add(mDataCol);                                            
                }
                else
                {
                    if (mDataCol.idVivienda == lote && mDataCol.LayerSeccion == layerSeccion)
                        colindanteInfo.Add(mDataCol);                     
                }
            }
            //------------------------------------------------------------------

            //Obtengo el orden de ls orientaciones de acuerdo al Rumbo de Frente
            orientaciones = Met_Manzana.OrientacionFrente(M.Manzana.RumboFrente.rumboActual);

            //Por cada Área Común
            foreach(M.AreaComun mAreaComun in M.Colindante.ListCommonArea)
            {
                if (mAreaComun.LongLote == lote)
                {
                    string  superficieAC = "",                            
                            inicioDescAC = "",
                            descripcionPuntosAC = "",
                            bloqueAreaComun;                    

                    inicioDescAC = string.Format("\n Medidas y colindancias del {0} ", DescribeSeccion(mAreaComun.NombreAreaComun));

                    //Obtengo el primer renglón que es la superficie
                    //Ejemplo Pasillo Común Descubierto 2 (dos) tiene una superficie de 15.180m2 (quince metros ciento ochenta milímetros cuadrados).
                    superficieAC = string.Format("{0} tiene una superficie de {1}.",
                                        DescribeSeccion(mAreaComun.NombreAreaComun),
                                        ObtenerMetros(variable.ConvLetra, mAreaComun.AreaPl.ToString(), "m²", "METRO_CUAD"));

                    #region Descripción de Puntos de Área Común
                    foreach (string orientacion in orientaciones)
                    {
                        //Cantidad de tramos encontrados en esa orientación
                        int cantTramos = 0;

                        //Listado para guardar los tramos por orientación
                        List<string> tramos = new List<string>();

                        //Cuando solamente toma en cuenta un tramo lo asigno
                        M.ColindanciaData colindanteActual = new ColindanciaData();

                        string describeDistancia = "", //Describo la cantidad de metros que hay en el tramo actual
                                describeOrientacion = ""; //Describo la orientación que se encuentra actualmente

                        foreach (M.ColindanciaData mDataCol in colindanteInfo)
                        {
                            //Reviso que sólo tome en cuenta el rumbo actual y el Área Común Actual
                            if (mDataCol.Seccion.ToUpper() == mAreaComun.NombreAreaComun.ToUpper()
                                && mDataCol.Rumbo.ToUpper() == orientacion.ToUpper())
                            {
                                //Inicializo-----------
                                string tramo = "",
                                        ordinal = "";

                                describeDistancia = "";
                                //---------------------                            

                                //Asigno el valor por si sólo encontró un tramo
                                colindanteActual = mDataCol;

                                //Incremento la cantidad de tramos
                                cantTramos++;

                                //Obtengo el número ordinal del tramo
                                ordinal = cantTramos.ToOrdinalNumber();

                                //Obtengo la distancia de ese tramo
                                describeDistancia = ObtenerMetros(variable.ConvLetra, mDataCol.Distancia.ToString(), "m", "METRO");

                                //Describo el tramo
                                if (mDataCol.esArco)
                                    tramo = string.Format("el {0} del punto {1} al punto {2} es una línea curva que mide {3} a colindar con {4}",
                                                ordinal, mDataCol.PuntoA, mDataCol.PuntoB, describeDistancia, DescribeSeccion(mDataCol.Colindancia));
                                else
                                    tramo = string.Format("el {0} del punto {1} al punto {2} que mide {3} a colindar con {4}",
                                                ordinal, mDataCol.PuntoA, mDataCol.PuntoB, describeDistancia, DescribeSeccion(mDataCol.Colindancia));

                                //Lo agrego a la lista
                                tramos.Add(tramo);

                            }
                        }


                        //Si sólo encontró un tramo
                        if (cantTramos == 1)
                        {

                            if (colindanteActual.esArco)
                                describeOrientacion = string.Format("Al {0} del punto {1} al punto {2} es una línea curva que mide {3} a colindar con {4}",
                                            orientacion.ToUpper(), colindanteActual.PuntoA, colindanteActual.PuntoB, describeDistancia, DescribeSeccion(colindanteActual.Colindancia));
                            else
                                describeOrientacion = string.Format("Al {0} del punto {1} al punto {2} que mide {3} a colindar con {4}",
                                            orientacion.ToUpper(), colindanteActual.PuntoA, colindanteActual.PuntoB, describeDistancia, DescribeSeccion(colindanteActual.Colindancia));
                        }
                        else //Si encontró más de uno
                        {
                            string entero = "",
                                    _decimal = "",
                                    tramosDescritos = "";

                            //Obtengo cuantos tramos son
                            Convert.ToDecimal(cantTramos).EnLetra(out entero, out _decimal);

                            //Uno los tramos mediante comas
                            tramosDescritos = string.Join(", ", tramos);

                            //Hago la descripción del rumbo
                            describeOrientacion = string.Format("Al {0} en {1} tramos, {2}",
                                                                orientacion.ToUpper(), entero.ToLower(), tramosDescritos);
                        }

                        rumbosDescritos.Add(describeOrientacion);

                    }

                    descripcionPuntosAC = string.Join("; ", rumbosDescritos);

                    bloqueAreaComun = superficieAC + inicioDescAC + descripcionPuntosAC;

                    descAreasComunes.Add(bloqueAreaComun);

                    #endregion
                }
            }

            valorDescriptivo = string.Join("\n", descAreasComunes) + ".";

            return valorDescriptivo;
        }

        internal static string DescribeSeccion(string seccion)
        {
            List<string> wsStrings = new List<string>();

            foreach (string strSeccion in seccion.Split(' '))
            {
                int outNumber;

                if (int.TryParse(strSeccion, out outNumber))
                {
                    string strEntero, strDecimales;

                    Convert.ToDecimal(outNumber).EnLetra(out strEntero, out strDecimales);

                    wsStrings.Add((string.Format("{0} ({1})", outNumber, strEntero)));
                }
                else
                    wsStrings.Add((strSeccion.FormatString()));
            }

            return string.Join(" ", wsStrings);
        }

        internal static string ObtenerMetros(bool ConvLetra, string medidaIn, string RepUnidad, string NomCortoUnidad)
        {
            string valor = "";

            decimal medida;

            if (ConvLetra)
            {
                if (decimal.TryParse(medidaIn, out medida))
                {
                    valor = string.Format("{0}{1} ({2})", medida.ToString("G29"), RepUnidad,
                                                    FormatMeasure(medida, NomCortoUnidad));
                }
                else
                    valor = medidaIn;

            }
            else {
                valor = medidaIn;
            }

            return valor;
        }

        internal static string FormatMeasure(decimal measure, string NomCorto)
        {
            string  valor = "",
                    strLetraEntero = "",
                    strLetraDecimal = "",
                    subUnidad = "";

            int decimals = measure.GetDecimals();

            decimals = decimals > 3 ? 3 : decimals;

            measure = measure.Trunc(decimals);

            if (decimals == 1)
                subUnidad = "decímetros";
            else if (decimals == 2)
                subUnidad = "centímetros";
            else if (decimals == 3)
                subUnidad = "milímetros";

            switch (NomCorto)
            {
                case "METRO_CUAD":

                    measure.EnLetra(out strLetraEntero, out strLetraDecimal);                   

                    //Si no tiene decimales
                    if (string.IsNullOrWhiteSpace(strLetraDecimal))
                    {
                        if (strLetraEntero.ToUpper() != "UNO")
                        {
                            valor = strLetraEntero + "metros cuadrados";
                        }
                        else
                            valor = "un metro cuadrado";                        
                    }
                    else //Si cuenta con decimales
                    {
                        if (strLetraEntero.ToUpper() != "UNO")
                        {
                            valor = string.Format("{0} metros, {1} {2} cuadrados", strLetraEntero, strLetraDecimal, subUnidad);
                        }
                        else
                        {
                            if (strLetraDecimal.ToUpper() != "UNO")
                                valor = string.Format("un metro, {1} {2} cuadrados", strLetraEntero, strLetraDecimal, subUnidad);
                            else
                                valor = string.Format("un metro, un decímetro cuadrado", strLetraEntero, strLetraDecimal, subUnidad);
                        }
                    }

                    break;

                case "METRO":

                    measure.EnLetra(out strLetraEntero, out strLetraDecimal);

                    //Si no tiene decimales
                    if (string.IsNullOrWhiteSpace(strLetraDecimal))
                    {
                        if (strLetraEntero.ToUpper() != "UNO")
                        {
                            valor = strLetraEntero + " metros";
                        }
                        else
                            valor = "un metro";
                    }
                    else //Si cuenta con decimales
                    {
                        if (strLetraEntero.ToUpper() != "UNO")
                        {
                            valor = string.Format("{0} metros, {1} {2}", strLetraEntero, strLetraDecimal, subUnidad);
                        }
                        else
                        {
                            if (strLetraDecimal != "UNO")
                                valor = string.Format("un metro, {1} {2}", strLetraEntero, strLetraDecimal, subUnidad);
                            else
                                valor = string.Format("un metro, un decímetro", strLetraEntero, strLetraDecimal, subUnidad);
                        }
                    }

                    break;

                case "PORCENTAJE":

                    measure.EnLetra(out strLetraEntero, out strLetraDecimal);

                    //Si no tiene decimales
                    if (string.IsNullOrWhiteSpace(strLetraDecimal))
                    {
                        //noventa y nueve por ciento
                        valor = strLetraEntero + " por ciento";

                    }
                    else //Si cuenta con decimales
                    {
                        //(noventa y nueve) punto (cuarenta y dos) por ciento
                        valor = strLetraEntero + " punto " + strLetraDecimal + " por ciento";
                                        
                    }

                    break;

                default:
                    if (string.IsNullOrWhiteSpace(strLetraDecimal))
                        valor = strLetraDecimal;
                    else
                        valor = strLetraDecimal + " punto " + strLetraEntero;
                        break;
            }

            return valor;
        }

        #endregion


        internal static bool ObtenerInfoMachotes(List<long> lotesDocs, string filePath)
        {
            List<string> resultadoMachote = new List<string>(),
                         resultadoVariables = new List<string>(),
                         varsFormated = new List<string>();

            HashSet<string> variables;
            try
            {
                char separator = '|';

                using (SQL_Connector conn = new SQL_Connector(Config.DB.ConnectionString))
                {                    
                    string queryMachote = string.Format(Config.DB.QueryDescMachote, M.Inicio.EncMachote.IdMachote);

                    //Obtengo el Machote de la BD
                    if (conn.Select(queryMachote, out resultadoMachote, separator))
                    {
                        M.InfoTabla.ResultadoBloques = new List<M.Bloques>();

                        foreach (string row in resultadoMachote)
                        {
                            string[] cells = row.Split(separator);

                            if (cells != null && cells.Count() > 0)
                            {
                                M.InfoTabla.ResultadoBloques.Add(new M.Bloques()
                                {
                                    Descripcion = cells[0], //DESCRIPCION
                                    IdBloque = int.Parse(cells[1]), //ID_BLOQUE
                                    IdTipoBloque = int.Parse(cells[2]), //ID_TIPO_BLOQUE
                                    NomTipoBLoque = cells[3],
                                    Orden = int.Parse(cells[4])
                                });
                            }
                        }

                        variables = C.Met_InfoTabla.GetVariables(M.InfoTabla.ResultadoBloques);

                        foreach (string var in variables)
                            varsFormated.Add("'" + var.Replace("[", "").Replace("]", "") + "'");

                        string inClause = string.Join(",", varsFormated),
                               query = string.Format(Config.DB.QueryVariables, inClause);

                        conn.Select(query, out resultadoVariables, separator);
                    }
                }

                M.InfoTabla.ResultadoVariables = new List<M.Variables>();

                foreach (string row in resultadoVariables)
                {
                    string[] cell = row.Split(separator);

                    M.InfoTabla.ResultadoVariables.Add(new M.Variables()
                    {
                        NombreCorto = cell[0],//NOM_CORTO
                        NombreVariable = cell[1],//NOM_VAR
                        Valor = cell[2],//VALOR
                        EsCalculado = Convert.ToBoolean(cell[3]), //ES_CALCULADO
                        ConvLetra = Convert.ToBoolean(cell[4]),//CONV_LETRA
                        NomTipoBloque = cell[5], //NOM_TIPO_BLOQUE
                        NomCortoUnidad = cell[6],//NOM_CORTO_UNIDAD
                        RepUnidad = cell[7],//REP_UNIDAD
                        DescUnidad = cell[8]//DESC_UNIDAD
                    });
                }
            }
            catch (Exception ex)
            {
                ex.Message.ToEditor();
            }

            return M.InfoTabla.ResultadoBloques.Count > 0 && M.InfoTabla.ResultadoVariables.Count > 0;
        }        
    }
}
