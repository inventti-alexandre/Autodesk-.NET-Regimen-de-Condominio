using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Autodesk.AutoCAD.Colors;

namespace RegimenCondominio.C
{
    public static class Met_Colindante
    {
        public static bool CreatePointsSet(ObjectId idLote, string provieneDe)
        {
            //Inicializo variables globales----------------------------------
            Point3d minLote = new Point3d(), //minManzana = new Point3d(),
                    maxLote = new Point3d();//, maxManzana = new Point3d();

            //Polilínea a crear puntos
            Polyline plLote = new Polyline();

            //Revisa que no haya problema
            bool isCorrect = true;

            //Número de Vivienda y Número Oficial
            string  strNoOficial = "";

            int numLote = 0;

            long longLote = new long();

            ObjectIdCollection idsApartments = new ObjectIdCollection();

            Polyline plManzana = new Polyline();
            //-----------------------------------------------------------------                                         

            //Obtengo NÚMERO OFICIAL siempre desde el LOTE----------------------------------------------------
            //Busco en Lote
            longLote = idLote.Handle.Value;

            //Busco el que concuerde con el Lote principal actual
            M.Lote itemLote = M.Colindante.Lotes.Where(x => x._long == longLote)
                                    .FirstOrDefault();

            //Obtengo número de Lote y Oficial
            numLote = itemLote.numLote;
            strNoOficial = itemLote.numOficial;
            //------------------------------------------------------------------------------------------------

            //Abro Polilínea como lectura
            plLote = idLote.OpenEntity() as Polyline;

            //Enfoco Polilínea
            //plLote.Focus(50, 10);
            if (M.Colindante.IdPolManzana.IsValid)
                plManzana = (M.Colindante.IdPolManzana.OpenEntity() as Polyline);
            else
            {
                //Leo los segmentos seleccionados
                List<ObjectId> listSegments = M.Manzana.ColindanciaManzana
                                                .Select(x => x.hndPlColindancia.toObjectId()).ToList();

                Met_Autodesk.SegmentsToPolyline(listSegments, out plManzana);
            }
                

            //Enfoco Manzana
            plManzana.Focus(10, 30);

            //Obtengo Punto mínimo y máximo
            minLote = plLote.GeometricExtents.MinPoint;
            maxLote = plLote.GeometricExtents.MaxPoint;

            //Obtengo Punto mínimo y máximo
            //minManzana = plManzana.GeometricExtents.MinPoint;
            //maxManzana = plManzana.GeometricExtents.MaxPoint;

            //Obtengo Apartamentos dentro del Lote
            idsApartments = Met_Autodesk.ObjectsInside( minLote, maxLote,
                                                        typeof(Polyline).Filter(M.Constant.LayerApartamento));            

            //Reviso que haya más de 1 apartamento y que los apartamentos del edificio correspondan a la cantidad introducida
            if (idsApartments.Count == M.Inicio.EncMachote.Cant_Viviendas)
            {
                //Reviso que tengan las letras correspondientes dependiendo de la cantidad de apartamentos
                if (CheckAndOrderApartment(idsApartments))
                {
                    try
                    {
                        //Inicia contador de consecutivos para puntos              
                        int ConsecutivePoint = 1;

                        #region Por cada Apartamento Ordenado
                        //Por cada Apartamento Ordenado
                        for (int i = 0; i < M.Colindante.OrderedApartments.Count; i++)
                        {
                            //Obtengo Pl Long
                            long plLong = M.Colindante.OrderedApartments[i].longPl;

                            //Apartamento Actual de A -> Z
                            Polyline plApartmento = (new Handle(plLong)).toObjectId().OpenEntity() as Polyline;

                            //Texto del Apartamento
                            string TextAp = M.Colindante.OrderedApartments[i].TextAp;

                            //Punto mínimo y máximo del apartamento
                            Point3d ptmin = plApartmento.GeometricExtents.MinPoint;
                            Point3d ptMax = plApartmento.GeometricExtents.MaxPoint;

                            #region Por cada Sección

                            //Comienzo busqueda por cada Sección
                            foreach (string layerSeccion in M.Colindante.Secciones)
                            {
                                ObjectIdCollection idsSecciones = new ObjectIdCollection();

                                SelectionFilter sf = typeof(Polyline).Filter(layerSeccion);

                                //Obtengo cada sección de acuerdo al layer                               
                                idsSecciones = Met_Autodesk.ObjectsInside(ptmin, ptMax, sf);

                                //Si encontró sólo una sección
                                if (idsSecciones.Count == 1)
                                {
                                    ObjectId idSeccion = idsSecciones[0];

                                    //Polilinea de la sección
                                    Polyline plSeccion = idSeccion.OpenEntity() as Polyline;

                                    //Distancia, Punto Medio, Punto Inicial y Punto Final
                                    List<M.SegmentInfo> listSegments = new List<M.SegmentInfo>();

                                    //Obtengo todos los arcos y sus puntos medios
                                    listSegments = GetInfoSegments(plSeccion);

                                    if (listSegments.Count > 0)
                                    {
                                        //Genera el orden correcto de los puntos
                                        List<Point3d> listPoints = plSeccion.ClockwisePoints();

                                        //Guardo Punto donde Inició la Búsqueda
                                        int startPoint = ConsecutivePoint;

                                        //Puntos A y B
                                        int numPointA = new int(),
                                            numPointB = new int();

                                        //Por cada vertice de la sección
                                        for (int j = 0; j < listPoints.Count; j++)
                                        {
                                            //Inicializo variables de cada VERTICE
                                            //Colindancia
                                            string strColindancia = "",
                                                    strRumbo = "";

                                            Point3d ptA = new Point3d(),//Punto Actual 
                                                    ptB = new Point3d(),//Punto siguiente
                                                    ptMedio = new Point3d();//Punto Medio

                                            List<string> layersColindancia = new List<string>();

                                            //Inicializo distancia
                                            double distance = new double();

                                            bool isArc = new bool();
                                            //-----------------------------------

                                            ////Punto A
                                            ptA = listPoints[j];

                                            //Asigno Punto B
                                            //Si no es el último punto le asigno 1 más, en dado caso que si, asigno Index 0.
                                            if (j + 1 < listPoints.Count)
                                                ptB = listPoints[j + 1];
                                            else
                                                ptB = listPoints[0];

                                            #region Calculo Distancia y Punto Medio                                                                                

                                            //Obtengo el index real
                                            //int idxPtActual = Met_Autodesk.GetPointIndex(plSeccion, ptA);

                                            M.SegmentInfo data = FindSegmentData(listSegments, ptA, ptB);

                                            //Obtengo distancia total del arco
                                            distance = data.Distance;

                                            //Obtengo el punto medio
                                            ptMedio = data.MiddlePoint;

                                            //Reviso si es Arco
                                            isArc = data.isArc;

                                            #endregion

                                            //Encuentro colindancia de acuerdo al punto medio
                                            //Si no es planta alta se busca la adyacencia
                                            if (layerSeccion != M.Constant.LayerAPAlta)
                                                strColindancia = FindAdjacency(ptMedio, out strRumbo, out layersColindancia, idSeccion, plApartmento.Id, idLote);
                                            else
                                                strColindancia = "Vacío";

                                            //Encuentro Rumbo si no estaba en la colindancia
                                            if (strRumbo == "")
                                                strRumbo = FindDirection(ptMedio, new Point3dCollection(listPoints.ToArray()), idSeccion);

                                            //Lo trunco a las decimales establecidas
                                            distance = distance.Trunc(M.Colindante.Decimals);

                                            //Enfoco de acuerdo a la polilínea guardada
                                            //(M.Colindante.IdPolManzana.OpenEntity() as Polyline).Focus(10, 30);

                                            #region Creo DB Points y DB Text

                                            //Si es el primer punto debo de analizar PuntoA y PuntoB
                                            if (j == 0)
                                            {
                                                if (layerSeccion != M.Constant.LayerAPAlta)
                                                {
                                                    if (ValidatePoint(ptA, ConsecutivePoint, out numPointA))
                                                        ConsecutivePoint++;
                                                    else
                                                        //Valido si realmente el punto de Inicio ya estaba insertado                                            
                                                        startPoint = numPointA;

                                                    if (ValidatePoint(ptB, ConsecutivePoint, out numPointB))
                                                        ConsecutivePoint++;
                                                }
                                                else
                                                {
                                                    if (HasTopFloorPoint(ptA, ConsecutivePoint, out numPointA))
                                                        ConsecutivePoint++;
                                                    else
                                                        startPoint = numPointA;

                                                    if (HasTopFloorPoint(ptB, ConsecutivePoint, out numPointB))
                                                        ConsecutivePoint++;
                                                }
                                            }
                                            else//Si no es el primer punto sólo asignó el Punto A como el último B, calculo el Punto B
                                            {
                                                numPointA = numPointB;

                                                if (j + 1 < listPoints.Count)//Si no es el último Punto lo valido
                                                {
                                                    if (layerSeccion != M.Constant.LayerAPAlta)
                                                    {
                                                        if (ValidatePoint(ptB, ConsecutivePoint, out numPointB))
                                                            ConsecutivePoint++;
                                                    }
                                                    else
                                                    {
                                                        if (HasTopFloorPoint(ptB, ConsecutivePoint, out numPointB))
                                                            ConsecutivePoint++;
                                                    }
                                                }
                                                else//En dado caso que si es el último punto cierro la polilínea con el punto Inicial
                                                    numPointB = startPoint;
                                            }

                                            #endregion

                                            M.Colindante.MainData.Add(new M.ColindanciaData()
                                            {
                                                idVivienda = idLote.Handle.Value,
                                                Seccion = Translate(layerSeccion),
                                                LayerSeccion = layerSeccion,
                                                Rumbo = strRumbo,
                                                Edificio_Lote = numLote,
                                                NoOficial = strNoOficial,
                                                Apartamento = "Apartamento " + TextAp,
                                                PuntoA = numPointA,
                                                PuntoB = numPointB,
                                                CoordenadaA = ptA,
                                                CoordenadaB = ptB,
                                                Colindancia = strColindancia,
                                                LayersColindancia = layersColindancia,
                                                Distancia = distance,
                                                esArco = isArc,
                                                esEsquinaA = false,
                                                esEsquinaB = false
                                            });
                                        }
                                    }
                                    else
                                    {
                                        M.Colindante.ListadoErrores.Add(new M.Error()
                                        {
                                            error = "Segmentos de Sección",
                                            description = "Error al detectar los segmentos de Sección " + Translate(layerSeccion),                                            
                                            timeError = DateTime.Now.ToString(),
                                            longObject = idSeccion.Handle.Value,
                                            metodo = "Met_Colindante - CreatePointsSet",
                                            tipoError = M.TipoError.Error
                                        });

                                        isCorrect = false;

                                        break;
                                    }
                                }
                                else if(idsSecciones.Count > 1)
                                {
                                    foreach (ObjectId idSecc in idsSecciones)
                                    {
                                        M.Colindante.ListadoErrores.Add(new M.Error()
                                        {
                                            error = "Secciones en Ap. " + TextAp,
                                            description = string.Format("Se detecto más de una sección {0} en el apartamento {1}"
                                            , Translate(layerSeccion), TextAp),
                                            timeError = DateTime.Now.ToString(),
                                            longObject = idSecc.Handle.Value,
                                            metodo = "Met_Colindante - CreatePointsMacroset",
                                            tipoError = M.TipoError.Error
                                        });
                                    }

                                    isCorrect = false;
                                    break;
                                }
                                else
                                {
                                    M.Colindante.ListadoErrores.Add(new M.Error()
                                    {
                                        error = "Secciones en Ap. " + TextAp,
                                        description = "No se detectó sección " + Translate(layerSeccion),
                                        timeError = DateTime.Now.ToString(),
                                        longObject = plApartmento.Id.Handle.Value,
                                        metodo = "Met_Colindante - CreatePointsMacroset",
                                        tipoError = M.TipoError.Informativo
                                    });
                                }

                            }
                            #endregion

                            //Si en algún punto tiene error detiene la iteración de Apartamentos
                            if (!isCorrect)
                                break;
                        }

                        #endregion

                        //Si no ha detectado algún error
                        if (isCorrect)
                        {
                            //Ordeno Área Común
                            M.Colindante.ListCommonArea.Sort((s1, s2) => s1.nombreAreaComun.CompareTo(s2.nombreAreaComun));

                            #region Por cada Área Común
                            //Nombre de Área Común
                            for (int i = 0; i < M.Colindante.ListCommonArea.Count; i++)
                            {
                                M.AreaComun areaActual = M.Colindante.ListCommonArea[i];

                                if (areaActual._longLote == idLote.Handle.Value)
                                {
                                    ObjectId idArea = new Handle(areaActual._longAreaComun).toObjectId();

                                    if (idArea.IsValid)
                                    {
                                        Polyline plAreaComun = idArea.OpenEntity() as Polyline;

                                        //Distancia, Punto Medio, Punto Inicial y Punto Final
                                        List<M.SegmentInfo> listSegments = new List<M.SegmentInfo>();

                                        //Obtengo todos los arcos y sus puntos medios
                                        listSegments = GetInfoSegments(plAreaComun);

                                        if (listSegments.Count > 0)
                                        {
                                            //Genera el orden correcto de los puntos
                                            List<Point3d> listPoints = plAreaComun.ClockwisePoints();

                                            //Guardo Punto donde Inició la Búsqueda
                                            int startPoint = ConsecutivePoint;

                                            //Puntos A y B
                                            int numPointA = new int(),
                                                numPointB = new int();

                                            //Por cada vertice de la sección
                                            for (int j = 0; j < listPoints.Count; j++)
                                            {
                                                //Inicializo variables de cada VERTICE
                                                //Colindancia
                                                string strColindancia = "",
                                                        strRumbo = "";

                                                Point3d ptA = new Point3d(),//Punto Actual 
                                                        ptB = new Point3d(),//Punto siguiente
                                                        ptMedio = new Point3d();//Punto Medio

                                                List<string> layersColindancia = new List<string>();

                                                //Inicializo distancia
                                                double distance = new double();

                                                bool isArc = new bool();
                                                //-----------------------------------

                                                ////Punto A
                                                ptA = listPoints[j];

                                                //Asigno Punto B
                                                //Si no es el último punto le asigno 1 más, en dado caso que si, asigno Index 0.
                                                if (j + 1 < listPoints.Count)
                                                    ptB = listPoints[j + 1];
                                                else
                                                    ptB = listPoints[0];

                                                #region Calculo Distancia y Punto Medio                                                                                

                                                //Obtengo el index real
                                                //int idxPtActual = Met_Autodesk.GetPointIndex(plSeccion, ptA);

                                                M.SegmentInfo data = FindSegmentData(listSegments, ptA, ptB);

                                                //Obtengo distancia total del arco
                                                distance = data.Distance;

                                                //Obtengo el punto medio
                                                ptMedio = data.MiddlePoint;

                                                //Reviso si es Arco
                                                isArc = data.isArc;

                                                #endregion

                                                //Encuentro colindancia de acuerdo al punto medio
                                                //Si no es planta alta se busca la adyacencia
                                                strColindancia = FindAdjacency(ptMedio, out strRumbo, out layersColindancia,
                                                                               new Handle(areaActual._longAreaComun).toObjectId());

                                                //Encuentro Rumbo si no estaba en la colindancia
                                                if (strRumbo == "")
                                                    strRumbo = FindDirection(ptMedio, new Point3dCollection(listPoints.ToArray()), idArea);

                                                //Lo trunco a las decimales establecidas
                                                distance = distance.Trunc(M.Colindante.Decimals);

                                                //Enfoco de acuerdo a la polilínea guardada
                                                //(M.Colindante.IdPolManzana.OpenEntity() as Polyline).Focus(10, 30);

                                                #region Creo DB Points y DB Text

                                                //Si es el primer punto debo de analizar PuntoA y PuntoB
                                                if (j == 0)
                                                {
                                                    if (ValidatePoint(ptA, ConsecutivePoint, out numPointA))
                                                        ConsecutivePoint++;
                                                    else
                                                        //Valido si realmente el punto de Inicio ya estaba insertado                                            
                                                        startPoint = numPointA;

                                                    if (ValidatePoint(ptB, ConsecutivePoint, out numPointB))
                                                        ConsecutivePoint++;
                                                }
                                                else//Si no es el primer punto sólo asignó el Punto A como el último B, calculo el Punto B
                                                {
                                                    numPointA = numPointB;

                                                    if (j + 1 < listPoints.Count)//Si no es el último Punto lo valido
                                                    {
                                                        if (ValidatePoint(ptB, ConsecutivePoint, out numPointB))
                                                            ConsecutivePoint++;
                                                    }
                                                    else//En dado caso que si es el último punto cierro la polilínea con el punto Inicial
                                                        numPointB = startPoint;
                                                }

                                                #endregion

                                                M.Colindante.MainData.Add(new M.ColindanciaData()
                                                {
                                                    idVivienda = idLote.Handle.Value,
                                                    Seccion = areaActual.nombreAreaComun.FormatString(),
                                                    LayerSeccion = M.Constant.LayerAreaComun,
                                                    Rumbo = strRumbo,
                                                    Edificio_Lote = numLote,
                                                    NoOficial = strNoOficial,
                                                    Apartamento = Translate(M.Constant.LayerAreaComun),
                                                    PuntoA = numPointA,
                                                    PuntoB = numPointB,
                                                    CoordenadaA = ptA,
                                                    CoordenadaB = ptB,
                                                    Colindancia = strColindancia,
                                                    LayersColindancia = layersColindancia,
                                                    Distancia = distance,
                                                    esArco = isArc,
                                                    esEsquinaA = false,
                                                    esEsquinaB = false
                                                });
                                            }
                                        }
                                        else
                                        {
                                            M.Colindante.ListadoErrores.Add(new M.Error()
                                            {
                                                error = "Segmentos de Sección",
                                                description = "Error al detectar los segmentos de Sección " + Translate(M.Constant.LayerAreaComun),
                                                timeError = DateTime.Now.ToString(),
                                                longObject = idArea.Handle.Value,
                                                metodo = "Met_Colindante - CreatePointsSet",
                                                tipoError = M.TipoError.Error
                                            });

                                            isCorrect = false;

                                            break;
                                        }
                                    }
                                    else
                                    {
                                        M.Colindante.ListadoErrores.Add(new M.Error()
                                        {
                                            error = "Id Inválida",
                                            description = "El id del Área Común no esta disponible o fue eliminada",
                                            longObject = M.Colindante.IdMacrolote.Handle.Value,
                                            metodo = "Met_Colindante - CreatePointsSet",
                                            timeError = DateTime.Now.ToString(),
                                            tipoError = M.TipoError.Error
                                        });

                                        isCorrect = false;

                                        break;
                                    }
                                }
                                #endregion
                            }
                        }
                    }
                    catch (Exception exc)
                    {
                        //Se deshace lo que sale mal en la transacción
                        //El abort lo destruye
                        exc.Message.ToEditor();
                        isCorrect = false;
                    }
                }
                else
                {
                    M.Colindante.ListadoErrores.Add(new M.Error()
                    {
                        error = "Apartamentos en Lote",
                        description = "Hubo un error al ordenar los apartamentos dentro del Lote",
                        timeError = DateTime.Now.ToString(),
                        longObject = idLote.Handle.Value,
                        metodo = "Met_Colindante - CreatePointsSet",
                        tipoError = M.TipoError.Error
                    });

                    isCorrect = false;
                }                            
            }
            else
            {
                M.Colindante.ListadoErrores.Add(new M.Error()
                {
                    error = "Cantidad de Apartamentos",
                    description = string.Format("El Lote tiene {0} apartamentos y deben de ser {1}", 
                                    idsApartments.Count, M.Inicio.EncMachote.Cant_Viviendas),
                    timeError = DateTime.Now.ToString(),
                    longObject = idLote.Handle.Value,
                    metodo = "Met_Colindante-CreatePointsSet",
                    tipoError = M.TipoError.Error
                });

                isCorrect = false;
            }
            return isCorrect;
        }

        internal static bool ReadApartmentPoints()
        {
            bool isCorrect = false;

            try
            {
                foreach (M.InEdificios edificio in M.Colindante.Edificios)
                {
                    foreach (long longAp in edificio.Apartments)
                    {
                        M.Apartments ap = M.Colindante.OrderedApartments.Search(longAp);

                        if (ap != null)
                        {
                            string letraAp = ap.TextAp;

                            ObjectId idAp = new Handle(longAp).toObjectId();

                            if (idAp.IsValid)
                            {
                                //Inicializa variables-----------------------------------
                                double distance = 0;

                                Point3d ptMedio = new Point3d();

                                bool isArc = false;

                                string strColindancia = "",
                                        strRumbo = "",
                                        noOficial = "";

                                List<M.SegmentInfo> listSegments = new List<M.SegmentInfo>();
                                List<string> layersColindancia = new List<string>();
                                //----------------------------------------------------------

                                Polyline plAp = idAp.OpenEntity() as Polyline;

                                long lMacrolote = M.Colindante.IdMacrolote.Handle.Value;

                                M.Lote Macrolote = M.Colindante.Lotes.Search(lMacrolote);

                                if (Macrolote != null)
                                    noOficial = Macrolote.numOficial;

                                //Obtengo todos los arcos y sus puntos medios
                                listSegments = GetInfoSegments(plAp);

                                List<Point3d> ptsApartamento = plAp.ClockwisePoints();

                                int startPoint = 0,
                                    numPtA = 0,
                                    numPtB = 0;

                                //Por cada vertice del Edificio
                                for (int k = 0; k < ptsApartamento.Count; k++)
                                {
                                    Point3d ptA = new Point3d(),
                                            ptB = new Point3d();

                                    ObjectId idPtFoundA = new ObjectId(),
                                             idPtFoundB = new ObjectId();

                                    ////Punto A
                                    ptA = ptsApartamento[k];

                                    //Asigno Punto B
                                    //Si no es el último punto le asigno 1 más, en dado caso que si, asigno Index 0.
                                    if (k + 1 < ptsApartamento.Count)
                                        ptB = ptsApartamento[k + 1];
                                    else
                                        ptB = ptsApartamento[0];

                                    //Reviso a que coordenada pertenece
                                    if (ptA.ExistsPoint(M.Constant.LayerExcDBPoints, out idPtFoundA))
                                    {
                                        DBPoint dbPointA = idPtFoundA.OpenEntity() as DBPoint;

                                        ObjectId idXRecord = new ObjectId();

                                        string[] dataA = new string[1];

                                        if (dbPointA.ExtensionDictionary.IsValid)
                                            idXRecord = DManager.GetXRecord(dbPointA.ExtensionDictionary, M.Constant.XRecordPoints);

                                        if (idXRecord.IsValid)
                                            dataA = DManager.GetData(idXRecord);

                                        numPtA = int.Parse(dataA[0]);

                                        if (k == 0)
                                            startPoint = numPtA;
                                    }

                                    if (ptB.ExistsPoint(M.Constant.LayerExcDBPoints, out idPtFoundB))
                                    {
                                        DBPoint dbPointB = idPtFoundB.OpenEntity() as DBPoint;

                                        ObjectId idXRecord = new ObjectId();

                                        string[] dataB = new string[1];

                                        if (dbPointB.ExtensionDictionary.IsValid)
                                            idXRecord = DManager.GetXRecord(dbPointB.ExtensionDictionary, M.Constant.XRecordPoints);

                                        if (idXRecord.IsValid)
                                            dataB = DManager.GetData(idXRecord);

                                        numPtB = int.Parse(dataB[0]);
                                    }

                                    //Dependiendo del tipo de segmento
                                    M.SegmentInfo data = FindSegmentData(listSegments, ptA, ptB);

                                    //Obtengo distancia total del arco
                                    distance = data.Distance.Trunc(M.Colindante.Decimals);

                                    //Obtengo el punto medio
                                    ptMedio = data.MiddlePoint;

                                    isArc = data.isArc;

                                    strColindancia = FindAdjacency(ptMedio, out strRumbo, out layersColindancia, new Handle(edificio._long).toObjectId(), idAp);

                                    if (strRumbo == "")
                                        strRumbo = FindDirection(ptMedio, new Point3dCollection(ptsApartamento.ToArray()), idAp);

                                    M.Colindante.MainData.Add(new M.ColindanciaData()
                                    {
                                        Edificio_Lote = edificio.numEdificio,
                                        Apartamento = "Apartamento " + letraAp,
                                        Colindancia = strColindancia,
                                        CoordenadaA = ptA,
                                        CoordenadaB = ptB,
                                        idVivienda = edificio._long,
                                        Seccion = "Área Exclusiva",
                                        LayerSeccion = M.Constant.LayerApartamento,
                                        PuntoA = numPtA,
                                        PuntoB = numPtB,
                                        NoOficial = noOficial,
                                        Distancia = distance,
                                        esArco = isArc,
                                        esEsquinaA = false,
                                        esEsquinaB = false,
                                        LayersColindancia = layersColindancia,
                                        Rumbo = strRumbo
                                    });
                                }
                            }
                        }
                    }
                }

                isCorrect = true;

            } catch (Exception ex) {
                ex.Message.ToEditor();
                isCorrect = false;
                M.Colindante.ListadoErrores.Add(new M.Error()
                {
                    error = "Error en Área Exclusiva",
                    description = "Se generó un Error al Obtener Área Exclusiva",
                    longObject = 0,
                    metodo = "ReadApartmentPoints - Met_Colindante",
                    timeError = DateTime.Now.ToString(),
                    tipoError = M.TipoError.Error
                });
            }

            return isCorrect;
        }

        internal static bool FoundEmptyItem(M.ColindanciaData mdata, out string emptyValue)
        {
            emptyValue = "";

            if (string.IsNullOrWhiteSpace(mdata.Apartamento))
            {
                emptyValue = "Apartamento en blanco";
                return true;
            }
            else if (string.IsNullOrWhiteSpace(mdata.Seccion))
            {
                emptyValue = "Sección en blanco";
                return true;
            }
            else if (string.IsNullOrWhiteSpace(mdata.PuntoA.ToString()))
            {
                emptyValue = "Punto A en blanco";
                return true;
            }
            else if (string.IsNullOrWhiteSpace(mdata.PuntoB.ToString()))
            {
                emptyValue = "Punto B en blanco";
                return true;
            }
            else if (mdata.Distancia <= 0)
            {
                emptyValue = "Distancia menor o igual a 0";
                return true;
            }
            else if (string.IsNullOrWhiteSpace(mdata.Colindancia))
            {
                emptyValue = "Colindancia en blanco";
                return true;
            }
            else if (string.IsNullOrWhiteSpace(mdata.NoOficial))
            {
                emptyValue = "Número Oficial en blanco";
                return true;
            }
            else if (string.IsNullOrWhiteSpace(mdata.Rumbo))
            {
                emptyValue = "Rumbo en blanco";
                return true;
            }
            else
                return false;

        }

        internal static void GetCommonArea(ObjectId idLote)
        {            
            //Inicializo variables globales----------------------------------
            Point3d min = new Point3d(),
                    max = new Point3d();

            Polyline plLote = new Polyline();

            ObjectIdCollection idsCommonArea = new ObjectIdCollection();            
            //----------------------------------------------------------------

            plLote = idLote.OpenEntity() as Polyline;

            min = plLote.GeometricExtents.MinPoint;
            max = plLote.GeometricExtents.MaxPoint;

            idsCommonArea = Met_Autodesk.ObjectsInside(min, max, typeof(Polyline).Filter(M.Constant.LayerAreaComun));

            foreach (ObjectId id in idsCommonArea)
            {
                Polyline plCA = id.OpenEntity() as Polyline;

                Point3d minAreaComun = new Point3d(),
                        maxAreaComun = new Point3d();

                minAreaComun = plCA.GeometricExtents.MinPoint;
                maxAreaComun = plCA.GeometricExtents.MaxPoint;

                ObjectId idText = Met_Autodesk.ObjectsInside(minAreaComun, maxAreaComun, typeof(DBText).Filter(M.Constant.LayerAreaComun))
                    .OfType<ObjectId>().FirstOrDefault();

                string nomArea = idText.IsValid ? (idText.OpenEntity() as DBText).TextString : Translate(M.Constant.LayerAreaComun);
                
                //Si ya existía este elemento ligado a un área común lo elimino    
                M.Colindante.ListCommonArea.RemoveAll(x => x._longAreaComun == id.Handle.Value);
                
                //Agrego el elemento a Listado
                M.Colindante.ListCommonArea.Add(new M.AreaComun()
                {
                    _longLote = idLote.Handle.Value,
                    _longAreaComun = id.Handle.Value,
                    nombreAreaComun = nomArea
                });
            }
        }

        public static bool CreatePointsMacroset(ObjectId idEdificio, string provieneDe)
        {
            //Inicializo variables globales----------------------------------
            Point3d min = new Point3d(),
                    max = new Point3d();

            //Polilínea a crear puntos
            Polyline plEdificio = new Polyline();

            //Revisa que no haya problema
            bool isCorrect = true;

            //Número de Vivienda y Número Oficial
            string strNoOficial = "";

            int numEdificio = 0;

            long longLote = new long();

            M.Colindante.PtsVertex = new Point3dCollection();

            ObjectIdCollection idsApartaments = new ObjectIdCollection();
            //-----------------------------------------------------------------                                         

            //Obtengo NÚMERO OFICIAL siempre desde el LOTE----------------------------------------------------
            //Busco en Macrolote
            longLote = M.Colindante.IdMacrolote.Handle.Value;

            //Busco el que concuerde con la polilínea principal actual o macrolote
            M.Lote itemLote = M.Colindante.Lotes.Where(x => x._long == longLote)
                                .FirstOrDefault();

            //Obtengo número Oficial
            strNoOficial = itemLote.numOficial;
            //------------------------------------------------------------------------------------------------


            //Busco el edificio
            M.InEdificios itemEdificio = M.Colindante.Edificios.Where(x => x._long == idEdificio.Handle.Value)
                .FirstOrDefault();

            //Obtengo el número de edificio
            numEdificio = itemEdificio.numEdificio;

            //Obtengo todos los vertices            
            M.Colindante.PtsVertex = idEdificio.ExtractVertex();           

            //Abro Polilínea como lectura
            plEdificio = idEdificio.OpenEntity() as Polyline;

            Polyline plManzana = new Polyline();

            //Enfoco Polilínea
            if (M.Colindante.IdPolManzana.IsValid)
            {
                plManzana = M.Colindante.IdPolManzana.OpenEntity() as Polyline;

                plManzana.Focus(50, 10);
            }                
            else
            {
                //Leo los segmentos seleccionados
                List<ObjectId> listSegments = M.Manzana.ColindanciaManzana
                                                .Select(x => x.hndPlColindancia.toObjectId()).ToList();

                Met_Autodesk.SegmentsToPolyline(listSegments, out plManzana);

                plManzana.Focus(50, 10);
            }

            //Obtengo Punto mínimo y máximo
            min = plEdificio.GeometricExtents.MinPoint;
            max = plEdificio.GeometricExtents.MaxPoint;

            //Obtengo Apartamentos dentro del Lote
            idsApartaments = Met_Autodesk.ObjectsInside(min, max,
                            typeof(Polyline).Filter(M.Constant.LayerApartamento));            

            //Reviso que haya más de 1 apartamento y que los apartamentos del edificio correspondan a la cantidad introducida
            if (idsApartaments.Count > 0 && idsApartaments.Count == M.Inicio.EncMachote.Cant_Viviendas)
            {
                //Reviso que tengan las letras correspondientes dependiendo de la cantidad de apartamentos
                try
                {
                    //Inicia contador de consecutivos para puntos              
                    int ConsecutivePoint = 1;

                    List<M.Apartments> listApartments = OrderApartments(idsApartaments);

                    if (listApartments.Count > 0)
                    {
                        //Por cada Apartamento Ordenado
                        for (int i = 0; i < listApartments.Count; i++)
                        {
                            //Obtengo Pl Long
                            long plLong = listApartments[i].longPl;

                            //Apartamento Actual de A -> Z
                            Polyline plApartmento = (new Handle(plLong)).toObjectId().OpenEntity() as Polyline;

                            //Texto del Apartamento
                            string TextAp = listApartments[i].TextAp;

                            //Punto mínimo del apartamento
                            Point3d ptmin = plApartmento.GeometricExtents.MinPoint;

                            //Punto máximo del apartamento
                            Point3d ptMax = plApartmento.GeometricExtents.MaxPoint;

                            #region Por cada Sección
                            //Comienzo busqueda por cada Sección
                            foreach (string layerSeccion in M.Colindante.Secciones)
                            {
                                ObjectIdCollection idsSecciones = new ObjectIdCollection();

                                //Obtengo cada sección de acuerdo al layer (En dado caso que no sea Área Común)
                                idsSecciones = Met_Autodesk.ObjectsInside(ptmin, ptMax, 
                                                    typeof(Polyline).Filter(layerSeccion));

                                if (idsSecciones.Count == 1)
                                {
                                    ObjectId idSeccion = idsSecciones[0];

                                    //Polilinea de la sección
                                    Polyline plSeccion = idSeccion.OpenEntity() as Polyline;

                                    //Distancia, Punto Medio, Punto Inicial y Punto Final
                                    List<M.SegmentInfo> listSegments = new List<M.SegmentInfo>();

                                    //Obtengo todos los arcos y sus puntos medios
                                    listSegments = GetInfoSegments(plSeccion);

                                    if (listSegments.Count > 0)
                                    {
                                        //Genera el orden correcto de los puntos
                                        List<Point3d> listPoints = plSeccion.ClockwisePoints();

                                        //Guardo Punto donde Inició la Búsqueda
                                        int startPoint = ConsecutivePoint;

                                        //Puntos A y B
                                        int numPointA = new int(),
                                            numPointB = new int();

                                        //Por cada vertice de la sección
                                        for (int j = 0; j < listPoints.Count; j++)
                                        {
                                            //Inicializo variables de cada VERTICE------------
                                            //Colindancia
                                            string strColindancia = "",
                                                    strRumbo = "";

                                            Point3d ptA = new Point3d(),//Punto Actual 
                                                    ptB = new Point3d(),//Punto siguiente
                                                    ptMedio = new Point3d();//Punto Medio

                                            List<string> layersColindancia = new List<string>();

                                            //Inicializo distancia
                                            double distance = new double();

                                            bool    isArc = new bool(),
                                                    isCornerA = new bool(),
                                                    isCornerB = new bool(); ;
                                            //--------------------------------------------

                                            ////Punto A
                                            ptA = listPoints[j];

                                            //Asigno Punto B
                                            //Si no es el último punto le asigno 1 más, en dado caso que si, asigno Index 0.
                                            if (j + 1 < listPoints.Count)
                                                ptB = listPoints[j + 1];
                                            else
                                                ptB = listPoints[0];

                                            #region Calculo Distancia y Punto Medio

                                            //Dependiendo del tipo de segmento
                                            M.SegmentInfo data = FindSegmentData(listSegments, ptA, ptB);

                                            //Obtengo distancia total del arco
                                            distance = data.Distance.Trunc(M.Colindante.Decimals);

                                            //Obtengo el punto medio
                                            ptMedio = data.MiddlePoint;

                                            isArc = data.isArc;
                                            #endregion

                                            //Encuentro colindancia de acuerdo al punto medio
                                            //Si no es planta alta se busca la adyacencia
                                            if (layerSeccion != M.Constant.LayerAPAlta)
                                                strColindancia = FindAdjacency(ptMedio, out strRumbo, out layersColindancia, idSeccion, plApartmento.Id, plEdificio.Id);
                                            else
                                                strColindancia = "Vacío";

                                            //Encuentro Rumbo si no estaba en la colindancia
                                            if (strRumbo == "")
                                                strRumbo = FindDirection(ptMedio, new Point3dCollection(listPoints.ToArray()), idSeccion);

                                            #region Creo DB Points y DB Text

                                            //Si es el primer punto debo de analizar PuntoA y PuntoB
                                            if (j == 0)
                                            {
                                                if (layerSeccion != M.Constant.LayerAPAlta)
                                                {
                                                    if (ValidatePoint(ptA, ConsecutivePoint, out numPointA))
                                                        ConsecutivePoint++;
                                                    else
                                                        //Valido si realmente el punto de Inicio ya estaba insertado                                            
                                                        startPoint = numPointA;

                                                    if (ValidatePoint(ptB, ConsecutivePoint, out numPointB))
                                                        ConsecutivePoint++;
                                                }
                                                else
                                                {
                                                    if (HasTopFloorPoint(ptA, ConsecutivePoint, out numPointA))
                                                        ConsecutivePoint++;
                                                    else
                                                        startPoint = numPointA;

                                                    if (HasTopFloorPoint(ptB, ConsecutivePoint, out numPointB))
                                                        ConsecutivePoint++;                                                                                                       
                                                }
                                            }
                                            else//Si no es el primer punto sólo asignó el Punto A como el último B, calculo el Punto B
                                            {
                                                numPointA = numPointB;

                                                if (j + 1 < listPoints.Count)//Si no es el último Punto lo valido
                                                {
                                                    if (layerSeccion != M.Constant.LayerAPAlta)
                                                    {
                                                        if (ValidatePoint(ptB, ConsecutivePoint, out numPointB))
                                                            ConsecutivePoint++;
                                                    }
                                                    else
                                                    {
                                                        if (HasTopFloorPoint(ptB, ConsecutivePoint, out numPointB))
                                                            ConsecutivePoint++;
                                                    }
                                                }
                                                else//En dado caso que si es el último punto cierro la polilínea con el punto Inicial
                                                    numPointB = startPoint;
                                            }
                                            #endregion

                                            if (numPointA == -1)
                                                isCornerA = true;

                                            if (numPointB == -1)
                                                isCornerB = true;

                                            M.Colindante.MainData.Add(new M.ColindanciaData()
                                            {
                                                idVivienda = idEdificio.Handle.Value,
                                                Seccion = Translate(layerSeccion),
                                                LayerSeccion = layerSeccion,
                                                Rumbo = strRumbo,
                                                Edificio_Lote = numEdificio,
                                                NoOficial = strNoOficial,
                                                Apartamento = "Apartamento " + TextAp,
                                                PuntoA = numPointA,
                                                PuntoB = numPointB,
                                                CoordenadaA = ptA,
                                                CoordenadaB = ptB,
                                                Colindancia = strColindancia,
                                                LayersColindancia = layersColindancia,
                                                Distancia = distance,
                                                esArco = isArc,
                                                esEsquinaA = isCornerA,
                                                esEsquinaB = isCornerB
                                            });
                                        }
                                    }
                                    else
                                    {
                                        M.Colindante.ListadoErrores.Add(new M.Error()
                                        {
                                            error = "Segmentos de Sección",
                                            description = "Error al detectar los segmentos de " + Translate(layerSeccion),
                                            timeError = DateTime.Now.ToString(),
                                            longObject = idSeccion.Handle.Value,
                                            metodo = "Met_Colindante - CreatePointsMacroset",
                                            tipoError = M.TipoError.Error
                                        });

                                        isCorrect = false;
                                    }

                                }
                                else if(idsSecciones.Count > 1){

                                    foreach (ObjectId idSecc in idsSecciones)
                                    {
                                        M.Colindante.ListadoErrores.Add(new M.Error()
                                        {
                                            error = "Secciones en Ap. " + TextAp,
                                            description = string.Format("Se detecto más de una sección {0} en el apartamento {1}" 
                                            ,layerSeccion,TextAp),
                                            timeError = DateTime.Now.ToString(),
                                            longObject = idSecc.Handle.Value,
                                            metodo = "Met_Colindante - CreatePointsMacroset",
                                            tipoError = M.TipoError.Error
                                        });
                                    }

                                    isCorrect = false;
                                    break;
                                }
                                else
                                {
                                    M.Colindante.ListadoErrores.Add(new M.Error()
                                    {
                                        error = "Secciones en Ap. " + TextAp,
                                        description = "No se detectó sección " + Translate(layerSeccion),
                                        timeError = DateTime.Now.ToString(),
                                        longObject = plApartmento.Id.Handle.Value,
                                        metodo = "Met_Colindante - CreatePointsMacroset",
                                        tipoError = M.TipoError.Informativo
                                    });
                                }

                                if (!isCorrect)
                                    break;
                            }//Por cada  layer sección FIN
                            #endregion
                        }

                        if (isCorrect)
                        {
                            if (ConsecutivePoint > M.Colindante.LastPoint)
                                M.Colindante.LastPoint = ConsecutivePoint;
                        }
                    }
                    else
                    {
                        M.Colindante.ListadoErrores.Add(new M.Error()
                        {
                            error = "Apartamentos en Edificio",
                            description = "Hubo un error al ordenar los apartamentos dentro del Edificio",
                            timeError = DateTime.Now.ToString(),
                            longObject = idEdificio.Handle.Value,
                            metodo = "Met_Colindante - CreatePointsMacroset",
                            tipoError = M.TipoError.Error
                        });

                        isCorrect = false;
                    }
                }
                catch (Exception exc)
                {
                    //Se deshace lo que sale mal en la transacción
                    //El abort lo destruye
                    exc.Message.ToEditor();
                    isCorrect = false;
                }              
            }
            else
            {
                M.Colindante.ListadoErrores.Add(new M.Error()
                {
                    error = "Cantidad de Apartamentos",
                    description = string.Format("El edificio tiene {0} apartamentos y deben de ser {1}"
                                                , idsApartaments.Count, M.Inicio.EncMachote.Cant_Viviendas),
                    timeError = DateTime.Now.ToString(),
                    longObject = idEdificio.Handle.Value,
                    metodo = "Met_Colindante - CreatePointsMacroset",
                    tipoError = M.TipoError.Error
                });

                isCorrect = false;
            }

            return isCorrect;
        }

        internal static bool GenerateAllSets(bool EsMacrolote)
        {

            List<M.ColindanciaData> lRepeatedData,
                                     lToCalculate;
            //lNew = new List<M.DatosColindancia>();

            bool isCorrect = true;
            try
            {
                SeparateData(out lRepeatedData, out lToCalculate);

                #region Macrolote
                if (EsMacrolote)
                {

                    M.InEdificios edificioRegular = M.Colindante.Edificios.Search(M.Colindante.IdTipo.Handle.Value);                        

                    List<M.Apartments> lApsEdificioRegular = GetApartments(edificioRegular);

                    string[,] translatorAps = new string[lApsEdificioRegular.Count, 2];

                    for (int i = 0; i < lApsEdificioRegular.Count; i++)
                        translatorAps[i, 0] = lApsEdificioRegular[i].TextAp;

                    for (int i = 0; i < M.Colindante.Edificios.Count; i++)
                    {
                        M.InEdificios mEdificio = M.Colindante.Edificios[i];

                        //Si el Edificio no es el Edificio Tipo y No esta dentro de los Edificios Irregulares seleccionados
                        if (mEdificio._long != M.Colindante.IdTipo.Handle.Value
                            && !M.Colindante.IdsIrregulares.Contains(new Handle(mEdificio._long).toObjectId()))
                        {

                            //Inicializo variables globales de Edificios-------------
                            int numEdificio = 0;

                            List<M.SegmentInfo> listSegments = new List<M.SegmentInfo>();

                            ObjectId idEdificio = new ObjectId();

                            Polyline plEdificio = new Polyline();

                            List<Point3d> listPoints = new List<Point3d>();

                            List<string> rumbosCalculados = new List<string>();

                            List<M.ColindanciaData> lSegmentData = new List<M.ColindanciaData>();

                            List<M.Apartments> lApsEdificioActual = new List<M.Apartments>();

                            int numPointA = 0,
                                numPointB = 0,
                                ConsecutivePoint = 0,
                                startPoint = 0;
                            //-------------------------------------------------------
                            
                            //---------------------------------------------------------------
                            //Obtengo los Apartametnos del Edificio
                            lApsEdificioActual = GetApartments(mEdificio);                            

                            //Obtengo el número de Edificio
                            numEdificio = mEdificio.numEdificio;

                            //Lo convierto en ID
                            idEdificio = new Handle(mEdificio._long).toObjectId();

                            if (idEdificio.IsValid)
                            {

                                ObjectId[] idsToIgnore = new ObjectId[lApsEdificioActual.Count+1];

                                //Asigno la traducción del apartamento
                                for (int l = 0; l < lApsEdificioRegular.Count; l++)
                                {
                                    M.Apartments mApartment = lApsEdificioActual[l];

                                    translatorAps[l, 1] = mApartment.TextAp;

                                    //Obtengo ids de Apartamento y los asigno
                                    idsToIgnore[l] = new Handle(mApartment.longPl).toObjectId();
                                }

                                //Asigno el ID de Edificio
                                idsToIgnore[(idsToIgnore.Count() - 1)] = idEdificio;

                                //Obtengo Polilínea
                                plEdificio = idEdificio.OpenEntity() as Polyline;

                                //Obtengo los puntos mediante las manecillas del reloj
                                listPoints = plEdificio.ClockwisePoints();

                                //Obtengo todos los arcos y sus puntos medios
                                listSegments = GetInfoSegments(plEdificio);

                                if (listSegments.Count > 0)
                                {
                                    #region Segmentos Repetidos por Edificio Regular
                                    foreach (M.ColindanciaData mColindancia in lRepeatedData)
                                    {
                                        string  ApartamentoTraducido = "",
                                                Colindancia = "";

                                        ApartamentoTraducido = CorrectApartment(mColindancia.Apartamento, translatorAps);
                                        Colindancia = CorrectApartment(mColindancia.Colindancia, translatorAps);                                        

                                        M.Colindante.MainData.Add(new M.ColindanciaData()
                                        {
                                            Edificio_Lote = numEdificio,
                                            idVivienda = mEdificio._long,
                                            Apartamento = ApartamentoTraducido,
                                            Colindancia = Colindancia,
                                            CoordenadaA = mColindancia.CoordenadaA,
                                            CoordenadaB = mColindancia.CoordenadaB,
                                            Distancia = mColindancia.Distancia,
                                            esArco = mColindancia.esArco,
                                            LayersColindancia = mColindancia.LayersColindancia,
                                            LayerSeccion = mColindancia.LayerSeccion,
                                            NoOficial = mColindancia.NoOficial,
                                            PuntoA = mColindancia.PuntoA,
                                            PuntoB = mColindancia.PuntoB,
                                            Rumbo = mColindancia.Rumbo,
                                            Seccion = mColindancia.Seccion
                                        });
                                    }
                                    #endregion

                                    //Debo de guardar numPointA, numPointB, Rumbo y Colindancia
                                    for (int j = 0; j < listPoints.Count; j++)
                                    {
                                        //Inicializo variables
                                        Point3d ptA = new Point3d(),
                                                ptB = new Point3d(),
                                                ptMedio = new Point3d();//Punto Medio

                                        string strColindancia = "",
                                                strRumbo = "";

                                        List<string> layersColindancia = new List<string>();
                                        //-----------------------------------

                                        ////Punto A
                                        ptA = listPoints[j];

                                        //Asigno Punto B
                                        //Si no es el último punto le asigno 1 más, en dado caso que si, asigno Index 0.
                                        if (j + 1 < listPoints.Count)
                                            ptB = listPoints[j + 1];
                                        else
                                            ptB = listPoints[0];

                                        //Dependiendo del tipo de segmento
                                        M.SegmentInfo data = FindSegmentData(listSegments, ptA, ptB);

                                        //Obtengo el punto medio
                                        ptMedio = data.MiddlePoint;                                       

                                        //Encuentro colindancia de acuerdo al punto medio                            
                                        strColindancia = FindAdjacency(ptMedio, out strRumbo, out layersColindancia, idsToIgnore);

                                        //Encuentro Rumbo si no estaba en la colindancia
                                        if (strRumbo == "")
                                            strRumbo = FindDirection(ptMedio, new Point3dCollection(listPoints.ToArray()), idEdificio);

                                        #region Creo DB Points y DB Text

                                        //Si es el primer punto debo de analizar PuntoA y PuntoB
                                        if (j == 0)
                                        {
                                            if (ValidatePoint(ptA, ConsecutivePoint, out numPointA))
                                                ConsecutivePoint++;
                                            else
                                                //Valido si realmente el punto de Inicio ya estaba insertado                                            
                                                startPoint = numPointA;

                                            if (ValidatePoint(ptB, ConsecutivePoint, out numPointB))
                                                ConsecutivePoint++;
                                        }
                                        else//Si no es el primer punto sólo asignó el Punto A como el último B, calculo el Punto B
                                        {
                                            numPointA = numPointB;

                                            if (j + 1 < listPoints.Count)//Si no es el último Punto lo valido
                                            {
                                                if (ValidatePoint(ptB, ConsecutivePoint, out numPointB))
                                                    ConsecutivePoint++;
                                            }
                                            else//En dado caso que si es el último punto cierro la polilínea con el punto Inicial
                                                numPointB = startPoint;
                                        }
                                        #endregion

                                        if (strRumbo != "" && !rumbosCalculados.Contains(strRumbo))
                                        {
                                            rumbosCalculados.Add(strRumbo);
                                            lSegmentData.Add(new M.ColindanciaData()
                                            {
                                                PuntoA = numPointA,
                                                CoordenadaA = ptA,
                                                PuntoB = numPointB,
                                                CoordenadaB = ptB,
                                                Rumbo = strRumbo,
                                                Colindancia = strColindancia
                                            });
                                        }
                                    }

                                    foreach (M.ColindanciaData itemToCalculate in lToCalculate)
                                    {
                                        string ApartamentoTraducido = "";                                                

                                        ApartamentoTraducido = CorrectApartment(itemToCalculate.Apartamento, translatorAps);                                        

                                        M.ColindanciaData currentItem = new M.ColindanciaData();

                                        for (int k = 0; k < lSegmentData.Count; k++)
                                        {
                                            M.ColindanciaData itemSegment = lSegmentData[k];

                                            if (itemSegment.Rumbo == itemToCalculate.Rumbo)
                                            {
                                                currentItem = itemSegment;
                                                break;
                                            }
                                        }

                                        if (currentItem != null && currentItem != new M.ColindanciaData())
                                        {

                                            if (itemToCalculate.esEsquinaA)
                                            {
                                                itemToCalculate.CoordenadaA = currentItem.CoordenadaA;
                                                itemToCalculate.PuntoA = currentItem.PuntoA;
                                            }

                                            if (itemToCalculate.esEsquinaB)
                                            {
                                                itemToCalculate.CoordenadaB = currentItem.CoordenadaB;
                                                itemToCalculate.PuntoB = currentItem.PuntoB;
                                            }                                            

                                            M.Colindante.MainData.Add(new M.ColindanciaData()
                                            {
                                                //Globales
                                                Edificio_Lote = mEdificio.numEdificio,
                                                idVivienda = mEdificio._long,
                                                //Repetibles
                                                NoOficial = itemToCalculate.NoOficial,
                                                esEsquinaA = itemToCalculate.esEsquinaA,
                                                esEsquinaB = itemToCalculate.esEsquinaB,
                                                Seccion = itemToCalculate.Seccion,
                                                LayerSeccion = itemToCalculate.LayerSeccion,
                                                Distancia = itemToCalculate.Distancia,
                                                LayersColindancia = itemToCalculate.LayersColindancia,
                                                esArco = itemToCalculate.esArco,
                                                //Calculados
                                                PuntoA = itemToCalculate.PuntoA,
                                                PuntoB = itemToCalculate.PuntoB,
                                                Apartamento = ApartamentoTraducido,
                                                CoordenadaA = itemToCalculate.CoordenadaA,
                                                CoordenadaB = itemToCalculate.CoordenadaB,
                                                Colindancia = currentItem.Colindancia,
                                                Rumbo = currentItem.Rumbo
                                            });
                                        }
                                    }
                                }
                                else
                                {
                                    M.Colindante.ListadoErrores.Add(new M.Error()
                                    {
                                        error = "Segmentos de Sección",
                                        description = "Error al detectar los segmentos del Edificio",
                                        longObject = idEdificio.Handle.Value,
                                        metodo = "Met_Colindante - GenerateAllSets",
                                        timeError = DateTime.Now.ToString(),
                                        tipoError = M.TipoError.Error
                                    });

                                    return false;
                                }

                            }
                            else
                            {
                                M.Colindante.ListadoErrores.Add(new M.Error()
                                {
                                    error = "Id Inválida",
                                    description = "El id del Edificio no esta disponible o fue eliminado",
                                    longObject = M.Colindante.IdMacrolote.Handle.Value,
                                    metodo = "Met_Colindante - GenerateAllSets",
                                    timeError = DateTime.Now.ToString(),
                                    tipoError = M.TipoError.Error
                                });

                                return false;
                            }
                        }
                    }
                }
                #endregion
                #region LOTE
                else
                {
                    for (int i = 0; i < M.Colindante.Lotes.Count; i++)
                    {
                        M.Lote mLote = M.Colindante.Lotes[i];

                        //Si no es el Lote Tipo y No es de los Lotes Irregulares
                        if (mLote._long != M.Colindante.IdTipo.Handle.Value &&
                            !M.Colindante.IdsIrregulares.Contains(new Handle(mLote._long).toObjectId()))
                        {

                            //Inicializo variables globales de Edificios-------------
                            List<M.SegmentInfo> listSegments = new List<M.SegmentInfo>();

                            ObjectId idLote = new ObjectId();

                            Polyline plLote = new Polyline();

                            List<Point3d> listPoints = new List<Point3d>();

                            List<string> rumbosCalculados = new List<string>();

                            List<M.ColindanciaData> lSegmentData = new List<M.ColindanciaData>();
                            //-------------------------------------------------------

                            //Lo convierto en ID
                            idLote = new Handle(mLote._long).toObjectId();

                            if (idLote.IsValid)
                            {
                                //Obtengo Polilínea
                                plLote = idLote.OpenEntity() as Polyline;

                                //Obtengo los puntos mediante las manecillas del reloj
                                listPoints = plLote.ClockwisePoints();

                                //Obtengo todos los arcos y sus puntos medios
                                listSegments = GetInfoSegments(plLote);

                                if (listSegments.Count > 0)
                                {
                                    #region Segmentos Repetidos por Lote Regular
                                    foreach (M.ColindanciaData mColindancia in lRepeatedData)
                                    {
                                        M.Colindante.MainData.Add(new M.ColindanciaData()
                                        {
                                            Edificio_Lote = mLote.numLote,
                                            idVivienda = mLote._long,
                                            NoOficial = mLote.numOficial,
                                            Apartamento = mColindancia.Apartamento,
                                            Colindancia = mColindancia.Colindancia,
                                            CoordenadaA = mColindancia.CoordenadaA,
                                            CoordenadaB = mColindancia.CoordenadaB,
                                            Distancia = mColindancia.Distancia,
                                            esArco = mColindancia.esArco,
                                            LayersColindancia = mColindancia.LayersColindancia,
                                            LayerSeccion = mColindancia.LayerSeccion,
                                            PuntoA = mColindancia.PuntoA,
                                            PuntoB = mColindancia.PuntoB,
                                            Rumbo = mColindancia.Rumbo,
                                            Seccion = mColindancia.Seccion
                                        });
                                    }
                                    #endregion

                                    #region Segmentos a Calcular
                                    for (int j = 0; j < listPoints.Count; j++)
                                    {
                                        //Inicializo variables
                                        Point3d ptA = new Point3d(),
                                                ptB = new Point3d(),
                                                ptMedio = new Point3d();//Punto Medio

                                        string strColindancia = "",
                                                strRumbo = "";

                                        List<string> layersColindancia = new List<string>();
                                        //-----------------------------------

                                        ////Punto A
                                        ptA = listPoints[j];

                                        //Asigno Punto B
                                        //Si no es el último punto le asigno 1 más, en dado caso que si, asigno Index 0.
                                        if (j + 1 < listPoints.Count)
                                            ptB = listPoints[j + 1];
                                        else
                                            ptB = listPoints[0];

                                        //Dependiendo del tipo de segmento
                                        M.SegmentInfo data = FindSegmentData(listSegments, ptA, ptB);

                                        //Obtengo el punto medio
                                        ptMedio = data.MiddlePoint;

                                        //Encuentro colindancia de acuerdo al punto medio                            
                                        strColindancia = FindAdjacency(ptMedio, out strRumbo, out layersColindancia, idLote);

                                        //Encuentro Rumbo si no estaba en la colindancia
                                        if (strRumbo == "")
                                            strRumbo = FindDirection(ptMedio, new Point3dCollection(listPoints.ToArray()), idLote);

                                        if (strRumbo != "" && !rumbosCalculados.Contains(strRumbo))
                                        {
                                            rumbosCalculados.Add(strRumbo);

                                            lSegmentData.Add(new M.ColindanciaData()
                                            {
                                                CoordenadaA = ptA,
                                                CoordenadaB = ptB,
                                                Rumbo = strRumbo,
                                                Colindancia = strColindancia
                                            });
                                        }
                                    }

                                    foreach (M.ColindanciaData itemToCalculate in lToCalculate)
                                    {
                                        M.ColindanciaData newData = new M.ColindanciaData();

                                        for (int k = 0; k < lSegmentData.Count; k++)
                                        {
                                            M.ColindanciaData itemSegment = lSegmentData[k];

                                            if (itemSegment.Rumbo == itemToCalculate.Rumbo)
                                            {
                                                newData = itemSegment;
                                                break;
                                            }
                                        }

                                        if (newData != null && newData != new M.ColindanciaData())
                                        {
                                            M.Colindante.MainData.Add(new M.ColindanciaData()
                                            {
                                                //Globales
                                                Edificio_Lote = mLote.numLote,
                                                idVivienda = mLote._long,
                                                NoOficial = mLote.numOficial,
                                                //Repetibles
                                                esEsquinaA = itemToCalculate.esEsquinaA,
                                                esEsquinaB = itemToCalculate.esEsquinaB,
                                                Seccion = itemToCalculate.Seccion,
                                                LayerSeccion = itemToCalculate.LayerSeccion,
                                                Distancia = itemToCalculate.Distancia,
                                                LayersColindancia = itemToCalculate.LayersColindancia,
                                                esArco = itemToCalculate.esArco,
                                                PuntoA = itemToCalculate.PuntoA,
                                                PuntoB = itemToCalculate.PuntoB,
                                                Apartamento = itemToCalculate.Apartamento,
                                                CoordenadaA = itemToCalculate.CoordenadaA,
                                                CoordenadaB = itemToCalculate.CoordenadaB,
                                                //Calculados
                                                Colindancia = newData.Colindancia,
                                                Rumbo = newData.Rumbo
                                            });
                                        }
                                    }
                                    #endregion
                                }
                                else
                                {
                                    M.Colindante.ListadoErrores.Add(new M.Error()
                                    {
                                        error = "Segmentos de Sección",
                                        description = "Error al detectar los segmentos del Lote",
                                        longObject = idLote.Handle.Value,
                                        metodo = "Met_Colindante - GenerateAllSets",
                                        timeError = DateTime.Now.ToString(),
                                        tipoError = M.TipoError.Error
                                    });

                                    return false;
                                }
                            }
                            else
                            {
                                M.Colindante.ListadoErrores.Add(new M.Error()
                                {
                                    error = "Id Inválida",
                                    description = "El id del Lote no esta disponible o fue eliminado",
                                    longObject = 0,
                                    metodo = "Met_Colindante - GenerateAllSets",
                                    timeError = DateTime.Now.ToString(),
                                    tipoError = M.TipoError.Error
                                });

                                return false;
                            }
                        }
                    }
                }
                #endregion
            }
            catch(Exception ex)
            {
                ex.Message.ToEditor();
                "\n GenerateAllSets_Error".ToEditor();
                isCorrect = false;
            }

            return isCorrect;
        }

        private static string CorrectApartment(string seccion, string[,] translatorAps)
        {
            string ApartamentoCorregido = "";

            if (seccion.Contains("Apartamento"))
            {
                //Separo por Espacios la palabra
                string[] arrayPalabras = seccion.Split(' ');

                //Obtengo la posición de la letra que debe de reemplazar
                int indexAp = Array.IndexOf(arrayPalabras, "Apartamento");

                //Valido que al incrementar uno, todavía tenga un valor
                if ((indexAp + 1) < arrayPalabras.Count())
                {
                    //Obtengo la palabra que siga después del apartamento que debe de ser la letra
                    string letraValidar = arrayPalabras[indexAp + 1],
                            letraCambiar = "";

                    //Reviso si esa letra existe en el arreglo de traducción
                    letraCambiar = translatorAps.FindInDimensions(letraValidar, 0, 1);

                    //Reviso que haya encontrado alguna
                    if (letraCambiar != "")
                    {
                        //Reemplazo la letra
                        arrayPalabras[indexAp + 1] = letraCambiar;

                        //Vuelvo a unir todo
                        ApartamentoCorregido = string.Join(" ", arrayPalabras);
                    }
                }

            }
            else
                ApartamentoCorregido = seccion;


            return ApartamentoCorregido;
        }

        private static List<M.Apartments> GetApartments(M.InEdificios edificio)
        {
            List<M.Apartments> listApartments = new List<M.Apartments>();

            List<long> longAps = new List<long>();

            edificio = M.Colindante.Edificios.Where(x => x._long == edificio._long).FirstOrDefault();

            longAps = edificio.Apartments;            

            //Obtengo los Apartamentos Actuales del 
            foreach (M.Apartments apActual in M.Colindante.OrderedApartments)
            {
                foreach (long lApEdificio in longAps)
                {
                    if (lApEdificio == apActual.longPl)
                    {
                        listApartments.Add(apActual);
                        break;
                    }
                }

                if (listApartments.Count == M.Inicio.EncMachote.Cant_Viviendas)
                    break;
            }

            return listApartments;
        }

        private static void SeparateData(out List<M.ColindanciaData> lRepeatedData, out List<M.ColindanciaData> lDataCalculated)
        {
            lRepeatedData = new List<M.ColindanciaData>();
            lDataCalculated = new List<M.ColindanciaData>();

            for(int i = 0; i < M.Colindante.MainData.Count; i++)
            {
                M.ColindanciaData itemDC = M.Colindante.MainData[i];

                if (itemDC.idVivienda == M.Colindante.IdTipo.Handle.Value)
                {
                    if (itemDC.LayersColindancia.Contains(M.Constant.LayerEdificio) ||
                        itemDC.LayersColindancia.Contains(M.Constant.LayerLote) ||
                        itemDC.LayersColindancia.Contains(M.Constant.LayerManzana))
                    {
                        lDataCalculated.Add(itemDC);
                    }
                    else
                    {
                        lRepeatedData.Add(itemDC);
                    }
                }
            }
        }

        internal static bool GenerateMacroCommonArea()
        {
            bool isCorrect = true;

            int ConsecutivePoint = M.Colindante.LastPoint;
            
            try
            {
                string strNoOficial = M.Colindante.Lotes.Where(x => x._long == M.Colindante.IdMacrolote.Handle.Value).
                                        FirstOrDefault().numOficial;

                for (int i = 0; i < M.Colindante.ListCommonArea.Count; i++)
                {
                    List<M.SegmentInfo> listSegments = new List<M.SegmentInfo>();

                    int startPoint = ConsecutivePoint, 
                        numPointA = 0, 
                        numPointB = 0;

                    long lActual = M.Colindante.ListCommonArea[i]._longAreaComun;

                    ObjectId idCommonArea = new Handle(lActual).toObjectId();

                    if (idCommonArea.IsValid)
                    {
                        Polyline pl = idCommonArea.OpenEntity() as Polyline;

                        List<Point3d> listPoints = pl.ClockwisePoints();

                        //Obtengo todos los arcos y sus puntos medios
                        listSegments = GetInfoSegments(pl);

                        if (listSegments.Count > 0)
                        {

                            //Por cada vértice de la polílinea
                            for (int j = 0; j < listPoints.Count; j++)
                            {
                                //Inicializo variables
                                Point3d ptA = new Point3d(),
                                        ptB = new Point3d(),
                                        ptMedio = new Point3d();//Punto Medio

                                //Inicializo distancia
                                double distance = new double();

                                string strColindancia = "", strRumbo = "";

                                List<string> layersColindancia = new List<string>();
                                //-----------------------------------

                                ////Punto A
                                ptA = listPoints[j];

                                //Asigno Punto B
                                //Si no es el último punto le asigno 1 más, en dado caso que si, asigno Index 0.
                                if (j + 1 < listPoints.Count)
                                    ptB = listPoints[j + 1];
                                else
                                    ptB = listPoints[0];

                                //Dependiendo del tipo de segmento
                                M.SegmentInfo data = FindSegmentData(listSegments, ptA, ptB);

                                //Obtengo distancia total del arco
                                distance = data.Distance.Trunc(M.Colindante.Decimals);

                                //Obtengo el punto medio
                                ptMedio = data.MiddlePoint;

                                //Encuentro colindancia de acuerdo al punto medio
                                //Si no es planta alta se busca la adyacencia
                                strColindancia = FindAdjacency(ptMedio, out strRumbo, out layersColindancia, idCommonArea);

                                //Encuentro Rumbo si no estaba en la colindancia
                                if (strRumbo == "")
                                    strRumbo = FindDirection(ptMedio, new Point3dCollection(listPoints.ToArray()), idCommonArea);

                                #region Creo DB Points y DB Text

                                //Si es el primer punto debo de analizar PuntoA y PuntoB
                                if (j == 0)
                                {
                                    if (ValidatePoint(ptA, ConsecutivePoint, out numPointA))
                                        ConsecutivePoint++;
                                    else
                                        //Valido si realmente el punto de Inicio ya estaba insertado                                            
                                        startPoint = numPointA;

                                    if (ValidatePoint(ptB, ConsecutivePoint, out numPointB))
                                        ConsecutivePoint++;
                                }
                                else//Si no es el primer punto sólo asignó el Punto A como el último B, calculo el Punto B
                                {
                                    numPointA = numPointB;

                                    if (j + 1 < listPoints.Count)//Si no es el último Punto lo valido
                                    {

                                        if (ValidatePoint(ptB, ConsecutivePoint, out numPointB))
                                            ConsecutivePoint++;
                                    }
                                    else//En dado caso que si es el último punto cierro la polilínea con el punto Inicial
                                        numPointB = startPoint;
                                }
                                #endregion

                                M.Colindante.MainData.Add(new M.ColindanciaData()
                                {
                                    idVivienda = lActual,
                                    Seccion = Translate(pl.Layer),
                                    LayerSeccion = pl.Layer,
                                    Rumbo = strRumbo,
                                    Edificio_Lote = 0,
                                    NoOficial = strNoOficial,
                                    Apartamento = "Area Común",
                                    PuntoA = numPointA,
                                    PuntoB = numPointB,
                                    CoordenadaA = ptA,
                                    CoordenadaB = ptB,
                                    Colindancia = strColindancia,
                                    LayersColindancia = layersColindancia,
                                    Distancia = distance
                                });

                            }
                        }
                        else
                        {
                            M.Colindante.ListadoErrores.Add(new M.Error()
                            {
                                error = "Segmentos de Sección",
                                description = "Error al detectar los segmentos de " + M.Constant.LayerAreaComun,
                                longObject = M.Colindante.IdMacrolote.Handle.Value,
                                metodo = "Met_Colindante - GenerateMacroCommonArea",
                                timeError = DateTime.Now.ToString(),
                                tipoError = M.TipoError.Error
                            });
                        }
                    }
                    else
                    {
                        M.Colindante.ListadoErrores.Add(new M.Error()
                        {
                            error = "Id Inválida",
                            description = "El id del Área Común no esta disponible o fue eliminada",
                            longObject = M.Colindante.IdMacrolote.Handle.Value,
                            metodo = "Met_Colindante - GenerateMacroCommonArea",
                            timeError = DateTime.Now.ToString(),
                            tipoError = M.TipoError.Error
                        });

                        isCorrect = false;

                        break;                        
                    }
                    
                }
            }
            catch(Exception ex)
            {
                ex.Message.ToEditor();
                isCorrect = false;
            }

            return isCorrect;
        }

        internal static bool GenerateBuildingCornerPoints()
        {
            //Los edificios que ya había realizado
            List<long> edificiosEvaluados = new List<long>();

            //Empiezo a partir del último punto
            int index = M.Colindante.LastPoint;
            try
            {
                for (int i = 0; i < M.Colindante.OrderedApartments.Count; i++)
                {
                    M.Apartments apartamentoActual = M.Colindante.OrderedApartments[i];

                    M.InEdificios edificioEncontrado = new M.InEdificios();

                    for (int j = 0; j < M.Colindante.Edificios.Count; j++)
                    {
                        M.InEdificios edificioActual = M.Colindante.Edificios[j];

                        if (edificioActual.Apartments.Contains(apartamentoActual.longPl))
                        {
                            edificioEncontrado = edificioActual;
                            break;
                        }
                    }

                    //Si no tuvo problema 
                    if (edificioEncontrado != new M.InEdificios())
                    {
                        //Si ese edificio no ha generado ya las esquinas anteriormente
                        if (!edificiosEvaluados.Contains(edificioEncontrado._long))
                        {
                            ObjectId idEdificio = new Handle(edificioEncontrado._long).toObjectId();

                            Polyline plEdificio = idEdificio.OpenEntity() as Polyline;

                            List<Point3d> ptsEdificio = plEdificio.ClockwisePoints();

                            //Por cada vertice del Edificio
                            for (int k = 0; k < ptsEdificio.Count; k++)
                            {
                                Point3d ptActual = ptsEdificio[k];
                                ObjectId idPtFound = new ObjectId();

                                //Reviso a que coordenada pertenece
                                if (!ptActual.ExistsPoint(M.Constant.LayerExcDBPoints,out idPtFound))
                                {
                                    ////Punto A
                                    ptActual.ToDBPoint(index, M.Constant.LayerExcDBPoints);

                                    //Actualizo información de número de esquinas de los Edificios Insertados
                                    ReplaceCorners(ptActual, index);

                                    index++;
                                }
                                else
                                {
                                    DBPoint pointFound = idPtFound.OpenEntity() as DBPoint;

                                    ObjectId idXRecord = new ObjectId();

                                    string[] data = new string[1];

                                    if (pointFound.ExtensionDictionary.IsValid)
                                        idXRecord = DManager.GetXRecord(pointFound.ExtensionDictionary, M.Constant.XRecordPoints);

                                    if (idXRecord.IsValid)
                                        data = DManager.GetData(idXRecord);
                                    
                                    //Actualizo información de número de esquinas de los Edificios Insertados
                                    ReplaceCorners(pointFound.Position, int.Parse(data[0]));
                                }

                            }

                            //Lo agrego a los ya evaluados
                            edificiosEvaluados.Add(edificioEncontrado._long);
                        }
                    }
                    else
                    {
                        M.Colindante.ListadoErrores.Add(new M.Error()
                        {
                            error = "Sin Edificios",
                            description = "No se encontó el Edificio del Apartamento " + apartamentoActual.TextAp,
                            longObject = apartamentoActual.longPl,
                            metodo = "Met_Colindante - GenerateCornerPoints",
                            timeError = DateTime.Now.ToString(),
                            tipoError = M.TipoError.Error
                        });

                        return false;                        
                    }                    
                }

                M.Colindante.LastPoint = index;
            }
            catch(Exception ex)
            {
                ex.Message.ToEditor();                
                return false;
            }

            return true;
        }

        private static void ReplaceCorners(Point3d pointFound, int numPoint)
        {
            for(int i = 0; i< M.Colindante.MainData.Count; i ++)
            {
                M.ColindanciaData mDatos = M.Colindante.MainData[i];

                if (mDatos.CoordenadaA.isEqualPosition(pointFound) && mDatos.PuntoA == -1)
                    mDatos.PuntoA = numPoint;

                if (mDatos.CoordenadaB.isEqualPosition(pointFound) && mDatos.PuntoB == -1)
                    mDatos.PuntoB = numPoint;
            }
        }

        private static M.SegmentInfo FindSegmentData(List<M.SegmentInfo> listSegments, 
                                                     Point3d startPoint, Point3d endPoint)
        {

            M.SegmentInfo data = new M.SegmentInfo();

            for (int j = 0; j < listSegments.Count; j++)
            {
                M.SegmentInfo itemActual = listSegments[j];

                Point3d actualstartPoint = itemActual.StartPoint, 
                        actualEndPoint = itemActual.EndPoint;


                //Si el inicio es igual al inicio del arco
                if (startPoint.isEqualPosition(actualstartPoint))
                {
                    //Comparo que el final del circular_Arc sea igual al final del arco
                    if (endPoint.isEqualPosition(actualEndPoint))
                    {
                        data = itemActual;
                        break;
                    }
                }
                //Si el iniciio es igual al final
                else if (startPoint.isEqualPosition(actualEndPoint))
                {
                    //Comparo que el final del circular_Arc sea igual al inicio del arco
                    if (endPoint.isEqualPosition(actualstartPoint))
                    {
                        data = itemActual;
                        break;
                    }
                }
            }

            return data;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="plSeccion">Polilínea de donde se obtendrán los arcos</param>
        /// <returns>De cada arco la distancia, Punto Medio, Inicial y Final</returns>
        internal static List<M.SegmentInfo> GetInfoSegments(Polyline plSeccion)
        {
            List<M.SegmentInfo> listSegments = new List<M.SegmentInfo>();

            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            //Documento activo
            Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;

            //Base de datos de documentos activos
            Database db = doc.Database;


            using (Transaction tr = db.TransactionManager.StartTransaction())
            {

                using (doc.LockDocument())
                {
                    try
                    {
                        using (Polyline plCopy = plSeccion.Clone() as Polyline)
                        {

                            if (!plCopy.IsWriteEnabled)
                                plCopy.UpgradeOpen();

                            DBObjectCollection dbc = new DBObjectCollection();

                            plCopy.Explode(dbc);

                            for (int i = 0; i < dbc.Count; i++)
                            {
                                DBObject dbObject = dbc[i];

                                if (dbObject is Arc)
                                {
                                    Arc _arc = (Arc)dbObject;

                                    Curve3d curve3d = _arc.GetGeCurve();

                                    Curve cv = Curve.CreateFromGeCurve(curve3d);

                                    Point3d ptMid = cv.GetMidpointCurve();

                                    //Point3d ptMedio = _arc.MiddlePoint();//

                                    Point3d midbyParam = _arc.GetPointAtParameter(_arc.StartAngle + (_arc.TotalAngle / 2));

                                    Point3d midbyDist = _arc.GetPointAtDist(_arc.Length / 2);

                                    //new DBPoint(ptMid).DrawDBPoint();

                                    //ed.Regen();

                                    //new DBPoint(ptMedio).DrawDBPoint();

                                    //ed.Regen();

                                    //new DBPoint(midbyParam).DrawDBPoint();

                                    //ed.Regen();

                                    //new DBPoint(midbyDist).DrawDBPoint();                                                                                                           

                                    //ed.Regen();

                                    listSegments.Add(new M.SegmentInfo()
                                    {
                                       Distance = _arc.Length,
                                       MiddlePoint = ptMid,
                                       StartPoint = _arc.StartPoint,
                                       EndPoint = _arc.EndPoint,
                                       isArc = true                                       
                                    });

                                    //listArcs.Add(new Tuple<double, Point3d, Point3d, Point3d>
                                    //                (_arc.Length, ptMid, _arc.StartPoint, _arc.EndPoint));
                                }
                                else
                                {
                                    Line l = dbObject as Line;

                                    Point3d ptmedio = l.StartPoint.MiddlePoint(l.EndPoint);

                                    listSegments.Add(new M.SegmentInfo()
                                    {
                                        Distance = l.Length,
                                        MiddlePoint = ptmedio,
                                        StartPoint = l.StartPoint,
                                        EndPoint = l.EndPoint,
                                        isArc = false
                                    });
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        tr.Abort();
                        ex.Message.ToEditor();
                    }
                    tr.Commit();
                }

            }


            return listSegments;
        }

        

        private static List<M.Apartments> OrderApartments(ObjectIdCollection idsApartments)
        {
            List<M.Apartments> listado = new List<M.Apartments>();

            for (int i = 0; i < idsApartments.Count; i++)
            {
                long lActual = idsApartments[i].Handle.Value;
                
                foreach (M.Apartments ApActual in M.Colindante.OrderedApartments)
                {
                    if (ApActual.longPl == lActual)
                    {
                        listado.Add(ApActual);
                        break;
                    }
                }                                                    
            }

            return listado.OrderBy(x => x.TextAp).ToList();
        }

        internal static bool CheckAndOrderApartment(ObjectIdCollection idsApartments)
        {
            M.Colindante.OrderedApartments = new List<M.Apartments>();

            string letrasFaltantes = "";            

            //Validar que los apartamentos
            for (int i = 0; i < idsApartments.Count; i++)
            {
                int numeroActualAp = i + 1;
                
                ObjectId idTextApartment = new ObjectId(), 
                         idPlApartment = new ObjectId();

                bool esFaltante = new bool();

                //Letra del Apartamento Inicial
                string letter = numeroActualAp.ToEnumerate();               

                //Encuentro cantidad de Apartamentos con esta letra
                idTextApartment = FindApartment(idsApartments, letter, out idPlApartment, out esFaltante);

                //Si sólo es uno se lo asigno
                if (idTextApartment.IsValid)
                {                    
                    M.Colindante.OrderedApartments.Add(new M.Apartments()
                    {
                        longPl = idPlApartment.Handle.Value,
                        longText = idPlApartment.Handle.Value,
                        TextAp = letter
                    });
                }
                else if (esFaltante) //Sólo en dado caso que no encontró letra
                {                                      
                    letrasFaltantes = letrasFaltantes + letter + ", ";      
                }
            }

            if (letrasFaltantes != "")
            {
                //Si solamente encontró un apartamento faltante elimina la última coma                
                letrasFaltantes = letrasFaltantes.Remove(letrasFaltantes.LastIndexOf(','), 1);

                M.Colindante.ListadoErrores.Add(new M.Error()
                {
                    error = "Letras Faltantes",
                    description = "Faltan los apartamentos " + letrasFaltantes,
                    longObject = 0,
                    timeError = DateTime.Now.ToString(),
                    tipoError = M.TipoError.Error,
                    metodo = "Met_Colindante - CheckAndOrderApartment"
                });
            }

            return M.Colindante.OrderedApartments.Count == idsApartments.Count;
        }

        private static ObjectId FindApartment(ObjectIdCollection ids, string letter, out ObjectId idoutAp, out bool esFaltante)
        {
            SelectionFilter sf = typeof(DBText).Filter(M.Constant.LayerApartamento);

            ObjectIdCollection idsTextMatch = new ObjectIdCollection();

            idoutAp = new ObjectId();
            esFaltante = false;

            //Obtengo todos los Apartamentos
            foreach (ObjectId idApp in ids)
            {
                //Texto dentro de Apartamento y Textos que son iguales a la letra
                ObjectIdCollection idstextInside = new ObjectIdCollection();

                //Polilínea de Apartamento
                Polyline pl = idApp.OpenEntity() as Polyline;

                //Texto dentro de la polilínea con el texto de Apartamento
                idstextInside = Met_Autodesk.ObjectsInside(pl.GeometricExtents.MinPoint,
                                                           pl.GeometricExtents.MaxPoint, sf);                               

                List<ObjectId> idsFound = idstextInside.OfType<ObjectId>()
                                            .Where(x => (x.OpenEntity() as DBText).TextString.GetAfterSpace() == letter).ToList();

                if (idsFound.Count > 0)
                {
                    idoutAp = idApp;
                    foreach (ObjectId id in idsFound)
                        idsTextMatch.Add(id);
                }
            }

            if (idsTextMatch.Count == 1)
            {
                return idsTextMatch[0];
            }
            else if (idsTextMatch.Count > 1)
            {
                foreach (ObjectId idApp in idsTextMatch)
                {
                    M.Colindante.ListadoErrores.Add(new M.Error()
                    {
                        error = "Más de 1 Texto",
                        description = string.Format("Se encontraron {0} Textos en Layer {1}"
                                                    , idsTextMatch.Count, M.Constant.LayerApartamento),
                        longObject = idApp.Handle.Value,
                        timeError = DateTime.Now.ToString(),
                        tipoError = M.TipoError.Error,
                        metodo = "FindApartment"
                    });
                }
            }
            else
                esFaltante = true;            

            return new ObjectId();
        }

        internal static string InLote(ObjectId idPl, string layer)
        {
            Polyline pl = idPl.OpenEntity() as Polyline;

            Point3d min = new Point3d(), max = new Point3d();

            min = pl.GeometricExtents.MinPoint;
            max = pl.GeometricExtents.MaxPoint;

            string Text = Met_Autodesk.TextInWindow(min, max, layer);

            return Text;

        }

        internal static bool noEstaEnEdificios(ObjectId idMacrolote)
        {
            bool siIncorrecto = false;

            try
            {
                Polyline plMacrolote = idMacrolote.OpenEntity() as Polyline;

                Point3d min = new Point3d(),
                        max = new Point3d();

                min = plMacrolote.GeometricExtents.MinPoint;
                max = plMacrolote.GeometricExtents.MaxPoint;

                plMacrolote.Focus(30, 20);

                ObjectIdCollection idsEdificios = Met_Autodesk.ObjectsInside(min, max,
                                                  typeof(Polyline).Filter(M.Constant.LayerEdificio));

                //Edificios
                M.Colindante.Edificios.Clear();                
                
                if (idsEdificios.Count > 0)
                {                

                    //Reviso que corresponda la cantidad de Apartamentos con la de 
                    ObjectIdCollection cantAps = Met_Autodesk.ObjectsInside(min, max,
                                                                typeof(Polyline).Filter(M.Constant.LayerApartamento));

                    int cantCorrectaAps = idsEdificios.Count * M.Inicio.EncMachote.Cant_Viviendas;

                    //Reviso que las polilíneas del apartamento sean las correctas
                    if (cantAps.Count == cantCorrectaAps)
                    {
                        //Valido que la cantidad de letras sea la correspondiente a los apartamentos
                        if (CheckAndOrderApartment(cantAps))
                        {
                            //Por cada Edificio encontrado
                            foreach (ObjectId idEdificio in idsEdificios)
                            {
                                //Inicializo variables de foreach
                                Point3d minEdificio = new Point3d(),
                                        maxEdificio = new Point3d();

                                string numEdificio = "";

                                Polyline plEdificio = new Polyline();
                                //---------------------------------------

                                //Abro entidad de Edificio
                                plEdificio = idEdificio.OpenEntity() as Polyline;

                                //Obtengo Punto mínimo y máximo
                                minEdificio = plEdificio.GeometricExtents.MinPoint;
                                maxEdificio = plEdificio.GeometricExtents.MaxPoint;

                                //Obtengo el texto dentro del Edificio
                                ObjectIdCollection textosEnEdificio = Met_Autodesk.ObjectsInside(minEdificio, maxEdificio,
                                                                        typeof(DBText).Filter(M.Constant.LayerEdificio));

                                //Obtengo Apartamentos de cada Edificio
                                ObjectIdCollection apsInEdificio = Met_Autodesk.ObjectsInside(minEdificio, maxEdificio,
                                                                  typeof(Polyline).Filter(M.Constant.LayerApartamento));

                                //Cambio a tipo de dato LONG
                                List<long> apartments = apsInEdificio.OfType<ObjectId>().Select(x => x.Handle.Value).ToList();

                                //Reviso que sólo sea un texto Edificio
                                numEdificio = isOneText(textosEnEdificio, idEdificio, apartments);

                                if (apsInEdificio.Count != M.Inicio.EncMachote.Cant_Viviendas)
                                {
                                    M.Colindante.ListadoErrores.Add(new M.Error()
                                    {
                                        error = "Menor Cant. de Apartamentos",
                                        description = string.Format("En el Edificio {0} se detectaron {1} y deben ser {2} Apartamentos (Polilíneas)",
                                                                    idEdificio, apsInEdificio.Count, M.Inicio.EncMachote.Cant_Viviendas),
                                        longObject = idEdificio.Handle.Value,
                                        timeError = DateTime.Now.ToString(),
                                        tipoError = M.TipoError.Error,
                                        metodo = "Asigna_Edificios"
                                    });

                                    siIncorrecto = true;
                                }
                            }
                        }
                        else
                        {
                            //Reviso que los apartamentos contengan las letras que le corresponden. 
                            //Ejemplo si son 26 =>  A hasta la Z
                            siIncorrecto = true;
                        }
                    }
                    else
                    {                           
                        siIncorrecto = true;

                        M.Colindante.ListadoErrores.Add(new M.Error()
                        {
                            error = "Cantidad de Apartamentos",
                            description =
                                string.Format("En el Macrolote se detectaron {0} y deben ser {1} Apartamentos",
                                                 cantAps.Count, cantCorrectaAps),
                            longObject = idMacrolote.Handle.Value,
                            timeError = DateTime.Now.ToString(),
                            tipoError = M.TipoError.Error,
                            metodo = "Met_Colindante - noEstaEnEdificios"
                        });                       

                    }
                }
                else
                {
                    //Cuando no se encuentran edificios dentro del Lote
                    M.Colindante.ListadoErrores.Add(new M.Error()
                    {
                        error = "Sin Edificios",
                        description ="No se encontraron Edificios en el lote seleccionado",
                        longObject = idMacrolote.Handle.Value,
                        timeError = DateTime.Now.ToString(),
                        tipoError = M.TipoError.Error,
                        metodo = "Met_Colindante - noEstaEnEdificios"
                    });

                    siIncorrecto = true;
                }
            }
            catch (Exception ex)
            {
                ex.Message.ToEditor();
            }

            return siIncorrecto;
        }       

        private static string isOneText(ObjectIdCollection textoEnEdificio, ObjectId idActual, List<long> Aps)
        {
            string strEdificio = "";

            int numEdificio = 0;

            if (textoEnEdificio.Count == 1)
            {
                DBText text = textoEnEdificio[0].OpenEntity() as DBText;

                strEdificio = text.TextString.GetAfterSpace();

                if(int.TryParse(strEdificio, out numEdificio))
                {
                    M.Colindante.Edificios.Add(new M.InEdificios()
                    {
                        _long = idActual.Handle.Value,
                        numEdificio = numEdificio,
                        Apartments = Aps
                    });
                }
                else
                {
                    M.Colindante.ListadoErrores.Add(new M.Error()
                    {
                        longObject = idActual.Handle.Value,
                        error = "Error de Conversión",
                        description = string.Format("El edificio {0} debe de ser número Entero", strEdificio),
                        timeError = DateTime.Now.ToString(),
                        tipoError = M.TipoError.Error,
                        metodo = "AsignaEdificios"
                    });
                }
               
            }
            else if (textoEnEdificio.Count > 1)
            {
                foreach(ObjectId idTexto in textoEnEdificio)
                {
                    M.Colindante.ListadoErrores.Add(new M.Error()
                    {
                        longObject = idTexto.Handle.Value,
                        error = "Más de 1 Texto",
                        description = "Se encontró más de 1 Texto con Layer " + M.Constant.LayerEdificio,
                        timeError = DateTime.Now.ToString(),
                        tipoError = M.TipoError.Error,
                        metodo = "AsignaEdificios"
                    });
                }
                
            }
            else
            {
                M.Colindante.ListadoErrores.Add(new M.Error()
                {
                    longObject = idActual.Handle.Value,
                    error = "Sin Texto",
                    description = "No se encontró Texto con Layer " + M.Constant.LayerEdificio,
                    timeError = DateTime.Now.ToString(),
                    tipoError = M.TipoError.Error,
                    metodo = "AsignaEdificios"
                });
            }

            return strEdificio;
        }

        internal static string FindAdjacency(Point3d ptMedio, out string Rumbo, out List<string> layersColindancia, params ObjectId[] idsToIgnore)
        {
            Point3dCollection ptsGeometry = new Point3dCollection();

            ObjectIdCollection  idsSelected = new ObjectIdCollection(),
                                idsLines = new ObjectIdCollection();

            layersColindancia = new List<string>();

            Rumbo = "";

            string seccionColindancia = "";

            ptsGeometry = Met_Autodesk.CreateGeometry(7, 0.05, ptMedio);

            //Polyline pl = new Polyline();
            //for (int i = 0; i < ptsGeometry.Count; i++)
            //    pl.AddVertexAt(i, new Point2d(ptsGeometry[i].X, ptsGeometry[i].Y), 0, 0, 0);
            //pl.Closed = true;
            //ObjectId idFigure = pl.DrawInModel();

            //Obtengo todos los Ids que colindan de Polylinea
            idsSelected = ptsGeometry.SelectByFence(typeof(Polyline).Filter(), idsToIgnore);

            //Obtengo todos los Ids que colindan de Linea
            idsLines = ptsGeometry.SelectByFence(typeof(Line).Filter(), idsToIgnore);

            //Uno Polilineas y Lineas
            foreach(ObjectId idLine in idsLines)
                idsSelected.Add(idLine);

            if (idsSelected.Count > 0)
            {
                ObjectId    idManzana = new ObjectId(),
                            idLote = new ObjectId(),
                            idEdificio = new ObjectId(),
                            idApartamento = new ObjectId(), 
                            idSeccion = new ObjectId();

                //Inicializo
                List<ObjectId> idsAdjacent = new List<ObjectId>();

                List<string> allLayers = M.Colindante.TodosLayers.Select(x => x.Layername).ToList();                

                //Todos los ids que tengan colindancia (adyacencia)
                for (int i = 0; i < idsSelected.Count; i++)
                {
                    Entity entActual = idsSelected[i].OpenEntity();

                    if (entActual.Layer != M.Constant.LayerAPAlta && allLayers.Contains(entActual.Layer))
                        idsAdjacent.Add(entActual.Id);
                }                

                //Busco si tiene colindancia con Manzana
                idManzana = ContainedInLayer(idsAdjacent, M.Constant.LayerManzana);

                #region Colindancia Manzana
                //Si contiene Manzana esa será su colindancia
                if (idManzana.IsValid)
                {
                    //Asigno Layer Manzana
                    layersColindancia.Add(M.Constant.LayerManzana);
                    
                    //Abro Entidad de Manzana
                    Entity ent = idManzana.OpenEntity();

                    //Reviso que tenga un diccionario de datos
                    if (ent.ExtensionDictionary.IsValid)
                    {
                        //X Record
                        ObjectId idXrecord = DManager.GetXRecord(ent.ExtensionDictionary, M.Constant.XRecordColindancia);

                        //Datos del xRecord
                        string[] data = DManager.GetData(idXrecord);

                        //Si los datos son más de 1
                        if (data.Count() > 0)
                        {
                            //Asigno Rumbo de acuerdo a datos dentro de linea
                            Rumbo = data[0];

                            //Asigno Colindancia de acuerdo a datos dentro de linea
                            seccionColindancia = data[1];
                        }
                    }   
                }
                #endregion                
                else
                {
                    #region Colindancia Lote
                    //COLINDANCIA CON EL LOTE
                    idLote = ContainedInLayer(idsAdjacent, M.Constant.LayerLote);

                    //Busco si colinda con un Lote
                    if (idLote.IsValid)
                    {
                        layersColindancia.Add(M.Constant.LayerLote);

                        long longLote = idLote.Handle.Value;

                        int numLote = M.Colindante.Lotes.Search(longLote).numLote;

                        seccionColindancia = Met_General.JoinToWord(Translate(M.Constant.LayerLote), numLote.ToString());
                    }
                    #endregion
                    else
                    {                        
                        //SI NO COLINDA CON NINGÚN LOTE REVISO EDIFICIOS ÚNICAMENTE CON MACROLOTE
                        if(M.Manzana.EsMacrolote)
                        {
                            #region Colindancia Edificios
                            //Reviso Edificios
                            idEdificio = ContainedInLayer(idsAdjacent, M.Constant.LayerEdificio);

                            //En dado caso que no, reviso apartamentos
                            idApartamento = ContainedInLayer(idsAdjacent, M.Constant.LayerApartamento);

                            //Busco también sección
                            idSeccion = ContainedInLayer(idsAdjacent, M.Colindante.Secciones);

                            if (idEdificio.IsValid)
                            {
                                //Inicializo variables
                                long longEdificio = idEdificio.Handle.Value;

                                string seccion = "";

                                int numEdificio = 0;

                                M.Apartments aps = new M.Apartments();
                                //-----------------------------------------                               

                                //Obtengo Número de Edificio
                                numEdificio = M.Colindante.Edificios.Search(longEdificio).numEdificio;                                    

                                //Agrego Edificios al listado de colindancia
                                layersColindancia.Add(M.Constant.LayerEdificio);

                                if (idApartamento.IsValid)
                                {
                                    aps = M.Colindante.OrderedApartments.Search(idApartamento.Handle.Value);
                                                                            
                                    layersColindancia.Add(M.Constant.LayerApartamento);
                                }

                                if (idSeccion.IsValid)
                                {                                    
                                    seccion = Translate(idSeccion.OpenEntity().Layer);
                                    layersColindancia.Add(idSeccion.OpenEntity().Layer);
                                }
                                else
                                {
                                    //Busco también sección
                                    idSeccion = ContainedInLayer(idsAdjacent, M.Constant.LayerAreaComun);

                                    if (idSeccion.IsValid)
                                    {
                                        M.AreaComun areaComun = M.Colindante.ListCommonArea.Search(idSeccion.Handle.Value);                                                        

                                        if (areaComun != null && areaComun != new M.AreaComun())
                                            seccionColindancia = areaComun.nombreAreaComun;
                                        else
                                            seccionColindancia = Translate(idSeccion.OpenEntity().Layer);

                                        layersColindancia.Add(idSeccion.OpenEntity().Layer);

                                    }
                                }

                                seccionColindancia = Met_General.JoinToWord(Translate(M.Constant.LayerEdificio), numEdificio.ToString(), Translate(M.Constant.LayerApartamento), aps.TextAp, seccion);
                            }
                            #endregion
                            else
                            {
                                #region Colindancia Apartamentos Secciones
                                
                                //Si encuentra colindancia con apartamento Uno sección y apartamento
                                if (idApartamento.IsValid)
                                {
                                    long lApartamento = 0;

                                    string  numAp = "", 
                                            seccion = "";

                                    lApartamento = idApartamento.Handle.Value;

                                    numAp = M.Colindante.OrderedApartments.Search(lApartamento).TextAp;                                        

                                    layersColindancia.Add(M.Constant.LayerApartamento);


                                    if (idSeccion.IsValid)
                                    {
                                        seccion = Translate(idSeccion.OpenEntity().Layer);
                                        layersColindancia.Add(idSeccion.OpenEntity().Layer);
                                    }
                                    else
                                    {
                                        //Busco también por Área Común
                                        idSeccion = ContainedInLayer(idsAdjacent, M.Constant.LayerAreaComun);

                                        if (idSeccion.IsValid)
                                        {
                                            M.AreaComun areaComun = M.Colindante.ListCommonArea.Search(idSeccion.Handle.Value);                                                            

                                            if (areaComun != null && areaComun != new M.AreaComun())
                                                seccionColindancia = areaComun.nombreAreaComun;
                                            else
                                                seccionColindancia = Translate(idSeccion.OpenEntity().Layer);

                                            layersColindancia.Add(idSeccion.OpenEntity().Layer);

                                        }
                                    }

                                    seccionColindancia = Met_General.JoinToWord(Translate(M.Constant.LayerApartamento), numAp, seccion);
                                }
                                else
                                {
                                    if (idSeccion.IsValid)
                                    {
                                        seccionColindancia = Translate(idSeccion.OpenEntity().Layer);
                                        layersColindancia.Add(idSeccion.OpenEntity().Layer);
                                    }
                                    else
                                    {
                                        //Busco también por Área Común
                                        idSeccion = ContainedInLayer(idsAdjacent, M.Constant.LayerAreaComun);

                                        if (idSeccion.IsValid)
                                        {
                                            M.AreaComun areaComun = M.Colindante.ListCommonArea.Search(idSeccion.Handle.Value);

                                            if(areaComun != null && areaComun != new M.AreaComun())                                            
                                                seccionColindancia = areaComun.nombreAreaComun;                                            
                                            else
                                                seccionColindancia = Translate(idSeccion.OpenEntity().Layer);

                                            layersColindancia.Add(idSeccion.OpenEntity().Layer);

                                        }
                                    }
                                }
                                #endregion
                            }
                        }
                        else //SI NO ES MACROLOTE
                        {
                            #region Colindancia Apartamentos - Secciones
                            //En dado caso que no, reviso apartamentos
                            idApartamento = ContainedInLayer(idsAdjacent, M.Constant.LayerApartamento);

                            //Busco también sección
                            idSeccion = ContainedInLayer(idsAdjacent, M.Colindante.Secciones);

                            //Si encuentra colindancia con apartamento Uno sección y apartamento
                            if (idApartamento.IsValid)
                            {
                                long lApartamento = 0;

                                string  numAp = "",
                                        seccion = "";

                                lApartamento = idApartamento.Handle.Value;

                                numAp = M.Colindante.OrderedApartments.Search(lApartamento).TextAp;                                    

                                layersColindancia.Add(M.Constant.LayerApartamento);

                                if (idSeccion.IsValid)
                                {
                                    seccion = Translate(idSeccion.OpenEntity().Layer);
                                    layersColindancia.Add(idSeccion.OpenEntity().Layer);
                                }
                                else
                                {
                                    //Busco también por Área Común
                                    idSeccion = ContainedInLayer(idsAdjacent, M.Constant.LayerAreaComun);

                                    if (idSeccion.IsValid)
                                    {
                                        M.AreaComun areaComun = M.Colindante.ListCommonArea.Search(idSeccion.Handle.Value);                                                        

                                        if (areaComun != null && areaComun != new M.AreaComun())
                                            seccionColindancia = (areaComun.nombreAreaComun ?? "").FormatString();
                                        else
                                            seccionColindancia = Translate(idSeccion.OpenEntity().Layer);

                                        layersColindancia.Add(idSeccion.OpenEntity().Layer);

                                    }
                                }

                                seccionColindancia = Met_General.JoinToWord(Translate(M.Constant.LayerApartamento), numAp, seccion);
                            }
                            else
                            {
                                if (idSeccion.IsValid)
                                {
                                    seccionColindancia = Translate(idSeccion.OpenEntity().Layer);
                                    layersColindancia.Add(idSeccion.OpenEntity().Layer);
                                }
                                else
                                {
                                    //Busco también por Área Común
                                    idSeccion = ContainedInLayer(idsAdjacent, M.Constant.LayerAreaComun);

                                    if (idSeccion.IsValid)
                                    {
                                        M.AreaComun areaComun = M.Colindante.ListCommonArea.Search(idSeccion.Handle.Value);                                                        

                                        if (areaComun != null && areaComun != new M.AreaComun())
                                            seccionColindancia = (areaComun.nombreAreaComun ?? "").FormatString();
                                        else
                                            seccionColindancia = Translate(idSeccion.OpenEntity().Layer);

                                        layersColindancia.Add(idSeccion.OpenEntity().Layer);

                                    }
                                }
                            }
                            #endregion
                        }

                    }
                }
            }

            return seccionColindancia;
        }

        internal static ObjectId ContainedInLayer(List<ObjectId> listAllIds, string layerActual)
        {
            for(int i = 0; i < listAllIds.Count; i ++)
            {
                ObjectId idActual = listAllIds[i];

                if (idActual.OpenEntity().Layer == layerActual)
                    return idActual;
            }

            return new ObjectId();
        }

        internal static ObjectId ContainedInLayer(List<ObjectId> listAllIds, List<string> layers)
        {

            for (int i = 0; i < listAllIds.Count; i++)
            {
                ObjectId idActual = listAllIds[i];

                for (int j = 0; j < layers.Count(); j++)
                {
                    string layerActual = layers[j];

                    if (idActual.OpenEntity().Layer == layerActual)
                        return idActual;
                }
            }

            return new ObjectId();
        }

        private static string Translate(string layerColindancia)
        {
            M.DescribeLayer itemFound =
                M.Colindante.TodosLayers.Search(layerColindancia);

            if (itemFound != null && itemFound != new M.DescribeLayer())
            {
                return itemFound.Description;
            }
            else
                return layerColindancia;
        }

        internal static string FindDirection(Point3d ptMedio, Point3dCollection pts, ObjectId idSeccion)
        {
            string Rumbo = "";

            try
            {
                //Inicializo-----------------------------------------------------------
                ObjectIdCollection  idsSelected = new ObjectIdCollection(),
                                    idsLines = new ObjectIdCollection();
                                    //idsOut = new ObjectIdCollection();

                List<Tuple<ObjectId, Point3d, Point3d>> list 
                                                        = new List<Tuple<ObjectId, Point3d, Point3d>>();

                List<Point3dCollection> vertexPolygons = new List<Point3dCollection>();
                //----------------------------------------------------------------------

                //Genero los puntos con los 4 cuadrantes
                List<M.SegmentInfo> PointsInQuadrant = CreateQuadrants(ptMedio);                

                //Creo polilíneas de cada Cuadrante
                foreach (M.SegmentInfo segInfo in PointsInQuadrant)
                {
                    Polyline pl = new Polyline();

                    //Obtengo todas las vertices de la geometria calculada
                    Point3dCollection vertexPoints = Met_Autodesk.CreateGeometry(4, 0.01, segInfo.MiddlePoint);
                    
                    //Asigno vertices a Polilínea
                    for (int j = 0; j < vertexPoints.Count; j++)
                        pl.AddVertexAt(j, new Point2d(vertexPoints[j].X, vertexPoints[j].Y), 0, 0, 0);

                    //Le agrego atributos
                    pl.Closed = true;
                    pl.Layer = M.Constant.LayerExcRumbos;

                    //Obtengo Id dibujada
                    ObjectId idPl = pl.DrawInModel();

                    //Extraigo la información
                    list.Add(new Tuple<ObjectId, Point3d, Point3d>(idPl, segInfo.StartPoint, segInfo.EndPoint));                    
                }                

                //Busco las lineas que se crucen o contengan la sección
                idsSelected = pts.SelectCrossing(typeof(Polyline).Filter(M.Constant.LayerExcRumbos));                

                //De las líneas que cruzan las comparo con las líneas creadas
                List<ObjectId> idsRumbo = list.Select(x=> x.Item1).Where(x => !idsSelected.Contains(x)).ToList();

                //Las líneas que están afuera
                //List<ObjectId> idsOut = idsLines.OfType<ObjectId>().Where(x => idsSelected.Contains(x)).ToList();

                //Si encuentra el Rumbo correcto
                if (idsRumbo.Count == 1)
                {
                    ObjectId idLine = idsRumbo[0];

                    Point3d startPoint = new Point3d(),
                            endPoint = new Point3d();

                    Tuple<ObjectId, Point3d, Point3d> item = new Tuple<ObjectId, Point3d, Point3d>
                                                                (new ObjectId(), new Point3d(), new Point3d());

                    for (int i = 0; i < list.Count; i++)
                    {
                        if (list[i].Item1 == idLine)
                        {
                            item = list[i];
                            break;
                        }
                    }

                    startPoint = item.Item2;
                    endPoint = item.Item3;

                    double angle = GetAngle(endPoint, ptMedio);

                    ObjectId IdManzana = SearchRumbo(startPoint, endPoint, angle);

                    if (IdManzana.IsValid)
                    {
                        Entity ent = IdManzana.OpenEntity();

                        if (ent.ExtensionDictionary.IsValid)
                        {
                            ObjectId idXrecord = DManager.GetXRecord(ent.ExtensionDictionary, M.Constant.XRecordColindancia);

                            string[] data = DManager.GetData(idXrecord);

                            if (data.Count() > 0)
                                Rumbo = data[0];
                        }
                    }
                    else
                    {
                        M.Colindante.ListadoErrores.Add(new M.Error()
                        {
                            error = "Sin Rumbo",
                            description = "No se encontró un rumbo para la sección " + Translate(idSeccion.OpenEntity().Layer),
                            timeError = DateTime.Now.ToString(),
                            longObject = idSeccion.Handle.Value,
                            metodo = "Met_Colindante-FindDirection",
                            tipoError = M.TipoError.Advertencia
                        });
                    }
                }
                else
                {
                    M.Colindante.ListadoErrores.Add(new M.Error()
                    {
                        error = "Cantidad de Rumbos",
                        description = string.Format("La sección {0} tiene {1} y debe de ser sólo 1",
                        Translate(idSeccion.OpenEntity().Layer) , idsRumbo.Count),
                        timeError = DateTime.Now.ToString(),
                        longObject = idSeccion.Handle.Value,
                        metodo = "Met_Colindante-FindDirection",
                        tipoError = M.TipoError.Advertencia
                    });
                }

                //Elimino todas las líneas
                //foreach (Tuple<ObjectId, Point3d, Point3d> item in list)
                //    item.Item1.GetAndRemove();              
                if (idsRumbo.Count == 1)
                    foreach (Tuple<ObjectId, Point3d, Point3d> item in list)
                        item.Item1.GetAndRemove();
                else
                {
                    List<ObjectId> idsOut = idsLines.OfType<ObjectId>().Where(x => idsSelected.Contains(x)).ToList();

                    foreach (ObjectId id in idsOut)
                        id.GetAndRemove();
                }
            }
            catch (Exception ex)
            {
                ex.Message.ToEditor();
            }

            return Rumbo;
        }

        internal static void DeleteAdjacencyObjects()
        {
            Met_Autodesk.DeleteObjects(M.Constant.LayerExcDBText, M.Constant.LayerExcDBPoints,
                M.Constant.LayerExcPlantaAlta);            
        }

        private static ObjectId SearchRumbo(Point3d startPoint, Point3d endPoint, double angle)
        {
            bool detected = false;

            ObjectId idManzana = new ObjectId();

            int value = 100;

            while (!detected)
            {
                Point3d newEnd = NewEndPoint(angle, endPoint, value);

                Point3dCollection ptsLine = new Point3dCollection(new Point3d[2]
                {
                    startPoint,
                    newEnd
                });

                //Line l = new Line(startPoint, newEnd);

                //l.Layer = M.Constant.LayerExcRumbos;

                //l.DrawInModel();

                ObjectIdCollection idsManzana = ptsLine.SelectByFence(null);

                List<ObjectId> idsSegmentos = new List<ObjectId>();


                foreach (M.ManzanaData mManzana in M.Manzana.ColindanciaManzana)
                    idsSegmentos.Add(mManzana.hndPlColindancia.toObjectId());

                List<ObjectId> listManzana = idsManzana.OfType<ObjectId>().Where(x => x.OpenEntity().Layer == M.Constant.LayerManzana 
                    && idsSegmentos.Contains(x)).ToList();

                if (listManzana.Count == 1)
                {
                    idManzana = listManzana.FirstOrDefault();

                    detected = true;
                }
                else if (listManzana.Count > 1)
                    detected = true;
                else
                {
                    value = value + 100;

                    if (value >= 300)
                        detected = true;
                }

            }

            return idManzana;
        }

        private static Point3d NewEndPoint(double angle, Point3d endPoint, int value)
        {
            Point3d calcPoint = new Point3d();

            if(angle >= 0 && angle < 90)
            {
                //Incrementa X
                calcPoint = new Point3d(endPoint.X + value, endPoint.Y, 0);
            }
            else if(angle >= 90 && angle < 180)
            {
                //Incrementa Y
                calcPoint = new Point3d(endPoint.X, endPoint.Y + value, 0);
            }
            else if(angle >= 180 && angle < 270)
            {
                //Resto X
                //Incrementa Y
                calcPoint = new Point3d(endPoint.X - value, endPoint.Y, 0);
            }
            else//270 en adelante
            {
                //Resto Y
                calcPoint = new Point3d(endPoint.X, endPoint.Y - value, 0);
            }

            return calcPoint;
        }

        private static double GetAngle(Point3d endPoint, Point3d ptMedio)
        {          
            Point2d ptMedio2d = new Point2d(ptMedio.X, ptMedio.Y);

            Vector2d v3d = ptMedio2d.GetVectorTo(new Point2d(endPoint.X, endPoint.Y));

            double angle = v3d.Angle;
            
            return (angle * (180 / Math.PI));
        }

        private static void ImpliedSelection(ObjectId[] idsToSelect)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;

            Editor ed = doc.Editor;

            ed.SetImpliedSelection(idsToSelect);
        }

        private static List<M.SegmentInfo> CreateQuadrants(Point3d ptMedio)
        {
            List<M.SegmentInfo> listPolygons = new List<M.SegmentInfo>();

            //Linea 1 a 0°-------------------------------------------
            Point3d ptStart0G = new Point3d(ptMedio.X + .01, ptMedio.Y, 0),
                    ptEnd0G = new Point3d(ptMedio.X + .02, ptMedio.Y, 0),
                    midPoint0G = ptStart0G.MiddlePoint(ptEnd0G);

            listPolygons.Add(new M.SegmentInfo()
            {
                StartPoint = ptStart0G,
                EndPoint = ptEnd0G,
                MiddlePoint = midPoint0G,
                Distance = ptStart0G.DistanceTo(ptEnd0G)
            });
            //Linea 1 a 0° FIN----------------------------------------

            //Linea 2 a 90°------------------------------------------
            Point3d ptStart90G = new Point3d(ptMedio.X, ptMedio.Y + .01, 0),
                    ptEnd90G = new Point3d(ptMedio.X, ptMedio.Y + .02, 0),
                    midPoint90G = ptStart90G.MiddlePoint(ptEnd90G);

            listPolygons.Add(new M.SegmentInfo()
            {
                StartPoint = ptStart90G,
                EndPoint = ptEnd90G,
                MiddlePoint = midPoint90G,
                Distance = ptStart90G.DistanceTo(ptEnd90G)
            });
            //Linea 2 a 90° FIN-----------------------------------------

            //Linea 3 a 180°-------------------------------------------------
            Point3d ptStart180G = new Point3d(ptMedio.X - .01, ptMedio.Y, 0),
                    ptEnd180G = new Point3d(ptMedio.X - .02, ptMedio.Y, 0),
                    midPoint180G = ptStart180G.MiddlePoint(ptEnd180G);

            listPolygons.Add(new M.SegmentInfo()
            {
                StartPoint = ptStart180G,
                EndPoint = ptEnd180G,
                MiddlePoint = midPoint180G,
                Distance = ptStart180G.DistanceTo(ptEnd180G)
            });
            //Linea 3 a 180° FIN---------------------------------------------

            //270°--------------------------
            Point3d ptStart270G = new Point3d(ptMedio.X, ptMedio.Y - .01, 0),
                    ptEnd270G = new Point3d(ptMedio.X, ptMedio.Y - .02, 0),
                    midPoint270G = ptStart270G.MiddlePoint(ptEnd270G);

            listPolygons.Add(new M.SegmentInfo()
            {
                StartPoint = ptStart270G,
                EndPoint = ptEnd270G,
                MiddlePoint = midPoint270G,
                Distance = ptStart270G.DistanceTo(ptEnd270G)
            });
            //----------------------------

            //Líneas de referencia

            //Line line0G = new Line(, //Inicia en este punto);

            //Guardar Punto Inicial y Punto Final de cada Grado
            //Obtener el Punto Medio
            //Crear Geomtería de acuerdo a cada Punto medio de los puntos calculados

            return listPolygons;

        }

        internal static bool HasTopFloorPoint(Point3d pt, int numPoint, out int finalNum)
        {
            ObjectId idptFound = new ObjectId();

            finalNum = new int();

            bool siInserta = false;

            //Si NO existe el Punto
            if (!pt.ExistsPoint(M.Constant.LayerExcPlantaAlta, out idptFound))
            {
                pt.ToDBPoint(numPoint, M.Constant.LayerExcPlantaAlta);
                finalNum = numPoint;
                siInserta = true;
            }
            else//SI YA EXISTE EL PUNTO
            {
                DBPoint pointFound = idptFound.OpenEntity() as DBPoint;

                //VALIDO QUE EL PUNTO REALMENTE ESTE EN PLANTA ALTA                
                ObjectId idXRecord = new ObjectId();

                string[] data = new string[1];

                if (pointFound.ExtensionDictionary.IsValid)
                    idXRecord = DManager.GetXRecord(pointFound.ExtensionDictionary, M.Constant.XRecordPoints);

                if (idXRecord.IsValid)
                    data = DManager.GetData(idXRecord);

                finalNum = int.Parse(data[0]);

            }
                

            return siInserta;
        }
        /// <summary>
        /// Calcula el número del punto Asignado o a Asignar
        /// </summary>
        /// <param name="pt">Coordenada de Vértice</param>
        /// <param name="numPoint">Número Actual</param>
        /// <param name="finalNum">Número Calculado</param>
        /// <returns>Verdadero cuando no inserta el Punto</returns>
        internal static bool ValidatePoint(Point3d pt, int numPoint, out int finalNum)
        {
            ObjectId idptFound = new ObjectId();

            finalNum = new int();

            bool siInserta = false;

            //En dado caso que no exista se inserta el punto
            if (!pt.ExistsPoint(M.Constant.LayerExcDBPoints, out idptFound))
            {
                //Si es Macrolote
                if(M.Manzana.EsMacrolote)
                {
                    //Reviso que no este dentro de las esquinas/vertices de Polilínea actual 
                    if (M.Colindante.PtsVertex.OfType<Point3d>().Where(x => x.isEqualPosition(pt)).Count() == 0)
                    {
                        pt.ToDBPoint(numPoint, M.Constant.LayerExcDBPoints);
                        finalNum = numPoint;
                        siInserta = true;
                    }
                    else
                    {
                        finalNum = -1;                       
                    }
                }
                //Si no es Macrolote inserto diractamente
                else
                {
                    pt.ToDBPoint(numPoint, M.Constant.LayerExcDBPoints);
                    finalNum = numPoint;
                    siInserta = true;
                }                
            }
            //Si ya existe ese punto lo obtengo
            else
            {
                DBPoint pointFound = idptFound.OpenEntity() as DBPoint;

                ObjectId idXRecord = new ObjectId();

                string[] data = new string[1];

                if (pointFound.ExtensionDictionary.IsValid)
                    idXRecord = DManager.GetXRecord(pointFound.ExtensionDictionary, M.Constant.XRecordPoints);

                if (idXRecord.IsValid)
                    data = DManager.GetData(idXRecord);

                finalNum = int.Parse(data[0]);
            }

            return siInserta;
        }

        internal static bool ExistsPoint(this Point3d point3d, string layerFilter, out ObjectId idPointFound)
        {
            Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            ObjectIdCollection idsAllPoints = new ObjectIdCollection();

            idPointFound = new ObjectId();

            PromptSelectionResult psr = ed.SelectAll(typeof(DBPoint).Filter(layerFilter));

            if (psr.Status == PromptStatus.OK)
            {
                //Obtengo todos los puntos del plano
                idsAllPoints = new ObjectIdCollection(psr.Value.GetObjectIds());

                //Busco si hay algún punto actual que tenga la misma coordenada
                for (int i = 0; i < idsAllPoints.Count; i++)
                {
                    DBPoint actualPoint = idsAllPoints[i].OpenEntity() as DBPoint;

                    if (point3d.isEqualPosition(actualPoint.Position))
                    {
                        idPointFound = idsAllPoints[i];
                        break;
                    }
                }
            }

            return idPointFound.IsValid;

        }
        

        internal static int CreateAdjacencyLayers()
        {
            int count = 0;

            List<string> layerstoCreate = new List<string>();

            layerstoCreate.Add(M.Constant.LayerExcRegimen);
            layerstoCreate.Add(M.Constant.LayerExcDBPoints);
            layerstoCreate.Add(M.Constant.LayerExcDBText);
            
            layerstoCreate.Add(M.Constant.LayerExcRumbos);

            Color cGlobal = Color.FromColorIndex(ColorMethod.ByAci, 3);
            foreach (string layer in layerstoCreate)
            {                
                if(Met_Autodesk.CreateLayer(layer, cGlobal))                
                    count++;                
            }

            Met_Autodesk.CreateLayer(M.Constant.LayerExcPlantaAlta, Color.FromRgb(255,0,0));

            return count;
        }
    }
}

