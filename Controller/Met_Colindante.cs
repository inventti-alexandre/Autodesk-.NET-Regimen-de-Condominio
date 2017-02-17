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
            bool flag = false;

            //Número de Vivienda y Número Oficial
            string  strNoLote = "",
                    strNoOficial = "";

            long longLote = new long();

            ObjectIdCollection idsApartments = new ObjectIdCollection();
            //-----------------------------------------------------------------                                         

            //Obtengo NÚMERO OFICIAL siempre desde el LOTE----------------------------------------------------
            //Busco en Lote
            longLote = idLote.Handle.Value;

            //Busco el que concuerde con el Lote principal actual
            M.InLotes itemLote = M.Colindante.ValorLotes.Where(x => x.handleEntity == longLote)
                                 .FirstOrDefault();

            //Obtengo número de Lote y Oficial
            strNoLote = itemLote.numLote;
            strNoOficial = itemLote.numOficial;
            //------------------------------------------------------------------------------------------------

            //Abro Polilínea como lectura
            plLote = idLote.OpenEntity() as Polyline;

            //Enfoco Polilínea
            //plLote.Focus(50, 10);
            Polyline plManzana = (M.Colindante.IdPolManzana.OpenEntity() as Polyline);

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

            ObjectIdCollection idsAreaComun = new ObjectIdCollection();

            idsAreaComun = Met_Autodesk.ObjectsInside(  minLote, maxLote, 
                                                        typeof(Polyline).Filter(M.Constant.LayerAreaComun));

            //Reviso que haya más de 1 apartamento y que los apartamentos del edificio correspondan a la cantidad introducida
            if (idsApartments.Count == M.Inicio.ApartamentosXVivienda)
            {
                //Reviso que tengan las letras correspondientes dependiendo de la cantidad de apartamentos
                if (CheckAndOrderApartment(idsApartments))
                {
                    try
                    {
                        //Inicia contador de consecutivos para puntos              
                        int ConsecutivePoint = 1;

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
                                List<ObjectId> idsSecciones = new List<ObjectId>();

                                SelectionFilter sf = typeof(Polyline).Filter(layerSeccion);

                                //Obtengo cada sección de acuerdo al layer                               
                                idsSecciones = Met_Autodesk.ObjectsInside(ptmin, ptMax, sf)
                                            .OfType<ObjectId>().ToList();

                                foreach (ObjectId idSeccion in idsSecciones)
                                {
                                    //Si encontro una polilínea válida dentro del layer de la sección
                                    if (idSeccion.IsValid)
                                    {
                                        //Polilinea de la sección
                                        Polyline plSeccion = idSeccion.OpenEntity() as Polyline;

                                        //Distancia, Punto Medio, Punto Inicial y Punto Final
                                        List<M.SegmentInfo> listSegments = new List<M.SegmentInfo>();

                                        //Obtengo todos los arcos y sus puntos medios
                                        listSegments = GetInfoSegments(plSeccion);

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

                                            M.SegmentInfo data = FindMidData(listSegments, ptA, ptB);

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
                                                strRumbo = FindDirection(ptMedio, new Point3dCollection(listPoints.ToArray()));

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
                                                    numPointA = ConsecutivePoint;

                                                    ConsecutivePoint++;

                                                    numPointB = ConsecutivePoint;

                                                    ConsecutivePoint++;

                                                    StackFrame callStack = new StackFrame(1, true);

                                                    M.Colindante.ListadoErrores.Add(new M.DescribeError()
                                                    {
                                                        Error = "Puntos Planta Alta",
                                                        Description = string.Format("Se agregaron los puntos A:{0} y B:{1} al Lote {2}, Apartamento {3}",
                                                           numPointA, numPointB, strNoLote, TextAp),
                                                        longObject = idSeccion.Handle.Value,
                                                        timeError = DateTime.Now,
                                                        tipoError = M.TipoError.Warning,
                                                        Metodo = (callStack.GetMethod().ToString()) + " " + (callStack.GetFileLineNumber().ToString())
                                                    });
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
                                                        numPointB = ConsecutivePoint;
                                                        ConsecutivePoint++;
                                                    }
                                                }
                                                else//En dado caso que si es el último punto cierro la polilínea con el punto Inicial
                                                    numPointB = startPoint;
                                            }

                                            #endregion

                                            M.Colindante.MainData.Add(new M.DatosColindancia()
                                            {
                                                Seccion = Translate(layerSeccion),
                                                LayerSeccion = layerSeccion,
                                                Rumbo = strRumbo,
                                                Vivienda = "Lote " + strNoLote,
                                                NoOficial = strNoOficial,
                                                Apartamento = "Apartamento " + TextAp,
                                                PuntoA = numPointA,
                                                PuntoB = numPointB,
                                                CoordenadaA = ptA,
                                                CoordenadaB = ptB,
                                                Colindancia = strColindancia,
                                                LayersColindancia = layersColindancia,
                                                Distancia = distance,
                                                IsArc = isArc                                                
                                            });
                                        }

                                    }//Si es una sección valida

                                }//Por cada sección FIN                                
                            }
                            #endregion
                        }

                        flag = true;
                    }
                    catch (Exception exc)
                    {
                        //Se deshace lo que sale mal en la transacción
                        //El abort lo destruye
                        exc.Message.ToEditor();
                    }
                }                               
            }
            else
            {
                M.Colindante.ListadoErrores.Add(new M.DescribeError()
                {
                    Error = "Sin Apartamentos",
                    Description = "No se encontraron apartamentos en el lote seleccionado",
                    timeError = DateTime.Now,
                    longObject = idLote.Handle.Value,
                    Metodo = "Met_Colindante-CreatePoints Ln-309",
                    tipoError = M.TipoError.Error
                });
            }
            return flag;
        }

        internal static void GetCommonArea(ObjectId idMacrolote)
        {            
            //Inicializo variables globales----------------------------------
            Point3d min = new Point3d(),
                    max = new Point3d();

            Polyline plMacrolote = new Polyline();

            ObjectIdCollection idsCommonArea = new ObjectIdCollection();

            M.Colindante.ListCommonArea = new List<long>();
            //----------------------------------------------------------------

            plMacrolote = idMacrolote.OpenEntity() as Polyline;

            min = plMacrolote.GeometricExtents.MinPoint;
            max = plMacrolote.GeometricExtents.MaxPoint;

            idsCommonArea = Met_Autodesk.ObjectsInside(min, max, typeof(Polyline).Filter(M.Constant.LayerAreaComun));

            foreach (ObjectId id in idsCommonArea)
                M.Colindante.ListCommonArea.Add(id.Handle.Value);
        }

        public static bool CreatePointsMacroset(ObjectId idEdificio, string provieneDe)
        {
            //Inicializo variables globales----------------------------------
            Point3d min = new Point3d(),
                    max = new Point3d();

            //Polilínea a crear puntos
            Polyline plEdificio = new Polyline();

            //Revisa que no haya problema
            bool flag = false;

            //Número de Vivienda y Número Oficial
            string strNoViv = "",
                    strNoOficial = "";

            long longLote = new long();

            M.Colindante.PtsVertex = new Point3dCollection();

            ObjectIdCollection idsApartaments = new ObjectIdCollection();
            //-----------------------------------------------------------------                                         

            //Obtengo NÚMERO OFICIAL siempre desde el LOTE----------------------------------------------------
            //Busco en Macrolote
            longLote = M.Colindante.IdMacrolote.Handle.Value;

            //Busco el que concuerde con la polilínea principal actual o macrolote
            M.InLotes itemLote = M.Colindante.ValorLotes.Where(x => x.handleEntity == longLote)
                .FirstOrDefault();

            //Obtengo número Oficial
            strNoOficial = itemLote.numOficial;
            //------------------------------------------------------------------------------------------------


            //Busco el edificio
            M.InEdificios itemEdificio = M.Colindante.ValorEdificio.Where(x => x.longEntity == idEdificio.Handle.Value)
                .FirstOrDefault();

            //Obtengo el número de edificio
            strNoViv = itemEdificio.numEdificio;

            //Obtengo todos los vertices            
            M.Colindante.PtsVertex = idEdificio.ExtractVertex();           

            //Abro Polilínea como lectura
            plEdificio = idEdificio.OpenEntity() as Polyline;

            //Enfoco Polilínea
            (M.Colindante.IdPolManzana.OpenEntity() as Polyline).Focus(50, 10);

            //Obtengo Punto mínimo y máximo
            min = plEdificio.GeometricExtents.MinPoint;
            max = plEdificio.GeometricExtents.MaxPoint;

            //Obtengo Apartamentos dentro del Lote
            idsApartaments = Met_Autodesk.ObjectsInside(min, max,
                            typeof(Polyline).Filter(M.Constant.LayerApartamento));            

            //Reviso que haya más de 1 apartamento y que los apartamentos del edificio correspondan a la cantidad introducida
            if (idsApartaments.Count > 0 && idsApartaments.Count == M.Inicio.ApartamentosXVivienda)
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
                                List<ObjectId> idsSecciones = new List<ObjectId>();
                                
                                //Obtengo cada sección de acuerdo al layer (En dado caso que no sea Área Común)
                                if (layerSeccion != M.Constant.LayerAreaComun)
                                    idsSecciones = Met_Autodesk.ObjectsInside(ptmin, ptMax, typeof(Polyline).Filter(layerSeccion))
                                                   .OfType<ObjectId>().ToList();

                                foreach (ObjectId idSeccion in idsSecciones)
                                {
                                    //Si encontro una polilínea válida dentro del layer de la sección
                                    if (idSeccion.IsValid)
                                    {
                                        //Polilinea de la sección
                                        Polyline plSeccion = idSeccion.OpenEntity() as Polyline;

                                        //Distancia, Punto Medio, Punto Inicial y Punto Final
                                        List<M.SegmentInfo> listSegments = new List<M.SegmentInfo>();

                                        //Obtengo todos los arcos y sus puntos medios
                                        listSegments = GetInfoSegments(plSeccion);

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

                                            //Dependiendo del tipo de segmento
                                            M.SegmentInfo data = FindMidData(listSegments, ptA, ptB);

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
                                                strRumbo = FindDirection(ptMedio, new Point3dCollection(listPoints.ToArray()));                                           

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
                                                    numPointA = ConsecutivePoint;

                                                    ConsecutivePoint++;

                                                    numPointB = ConsecutivePoint;

                                                    ConsecutivePoint++;

                                                    M.Colindante.ListadoErrores.Add(new M.DescribeError()
                                                    {
                                                        Error = "Puntos Planta Alta",
                                                        Description = string.Format("Se agregaron los puntos A:{0} y B:{1} al Edificio {2}, Apartamento {3}",
                                                           numPointA, numPointB, strNoViv, TextAp),
                                                        longObject = idSeccion.Handle.Value,
                                                        timeError = DateTime.Now,
                                                        tipoError = M.TipoError.Info,
                                                        Metodo = "CreatePointsMacroSet"
                                                    });
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
                                                        numPointB = ConsecutivePoint;
                                                        ConsecutivePoint++;

                                                        M.Colindante.ListadoErrores.Add(new M.DescribeError()
                                                        {
                                                            Error = "Puntos Planta Alta",
                                                            Description = string.Format("Se agregaron los puntos A:{0} y B:{1} al Edificio {2}, Apartamento {3}",
                                                            numPointA, numPointB, strNoViv, TextAp),
                                                            longObject = idSeccion.Handle.Value,
                                                            timeError = DateTime.Now,
                                                            tipoError = M.TipoError.Info,
                                                            Metodo = "CreatePointsMacroSet"
                                                        });
                                                    }
                                                }
                                                else//En dado caso que si es el último punto cierro la polilínea con el punto Inicial
                                                    numPointB = startPoint;
                                            }

                                            #endregion                                            

                                            M.Colindante.MainData.Add(new M.DatosColindancia()
                                            {
                                                Seccion = Translate(layerSeccion),
                                                LayerSeccion = layerSeccion,
                                                Rumbo = strRumbo,
                                                Vivienda = "Edificio " + strNoViv,
                                                NoOficial = strNoOficial,
                                                Apartamento = "Apartamento " + TextAp,
                                                PuntoA = numPointA,
                                                PuntoB = numPointB,
                                                CoordenadaA = ptA,
                                                CoordenadaB = ptB,
                                                Colindancia = strColindancia,
                                                LayersColindancia = layersColindancia,
                                                Distancia = distance,
                                                IsArc = isArc
                                            });
                                        }

                                    }//Si es una sección valida
                                }
                            }//Por cada  layer sección FIN
                            #endregion
                        }

                        if (ConsecutivePoint > M.Colindante.LastPoint)
                            M.Colindante.LastPoint = ConsecutivePoint;

                        flag = true;
                    }
                }
                catch (Exception exc)
                {
                    //Se deshace lo que sale mal en la transacción
                    //El abort lo destruye
                    exc.Message.ToEditor();
                }              
            }
            else
            {
                M.Colindante.ListadoErrores.Add(new M.DescribeError()
                {
                    Error = "Número Incorrecto de Apartamentos",
                    Description = string.Format("Los Apartamentos detectados son {0} y deben de ser {1}"
                                                , idsApartaments.Count, M.Inicio.ApartamentosXVivienda),
                    timeError = DateTime.Now,
                    longObject = idEdificio.Handle.Value,
                    Metodo = "CreatePoints",
                    tipoError = M.TipoError.Error
                });
            }
            return flag;
        }

        internal static bool GenerateMacroCommonArea()
        {
            bool isCorrect = true;

            int ConsecutivePoint = M.Colindante.LastPoint;
            
            try
            {
                string strNoOficial = M.Colindante.ValorLotes.Where(x => x.handleEntity == M.Colindante.IdMacrolote.Handle.Value).
                                        FirstOrDefault().numOficial;

                for (int i = 0; i < M.Colindante.ListCommonArea.Count; i++)
                {
                    List<M.SegmentInfo> listSegments = new List<M.SegmentInfo>();

                    int startPoint = ConsecutivePoint, numPointA = 0, numPointB = 0;

                    long lActual = M.Colindante.ListCommonArea[i];

                    ObjectId idCommonArea = new Handle(lActual).toObjectId();

                    Polyline pl = idCommonArea.OpenEntity() as Polyline;

                    List<Point3d> listPoints = pl.ClockwisePoints();

                    //Obtengo todos los arcos y sus puntos medios
                    listSegments = GetInfoSegments(pl);

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
                        M.SegmentInfo data = FindMidData(listSegments, ptA, ptB);

                        //Obtengo distancia total del arco
                        distance = data.Distance.Trunc(M.Colindante.Decimals);

                        //Obtengo el punto medio
                        ptMedio = data.MiddlePoint;

                        //Encuentro colindancia de acuerdo al punto medio
                        //Si no es planta alta se busca la adyacencia
                        strColindancia = FindAdjacency(ptMedio, out strRumbo, out layersColindancia, idCommonArea);

                        //Encuentro Rumbo si no estaba en la colindancia
                        if (strRumbo == "")
                            strRumbo = FindDirection(ptMedio, new Point3dCollection(listPoints.ToArray()));

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

                        M.Colindante.MainData.Add(new M.DatosColindancia()
                        {
                            Seccion = Translate(pl.Layer),
                            LayerSeccion = pl.Layer,
                            Rumbo = strRumbo,
                            Vivienda = "Area Común",
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
            }
            catch(Exception ex)
            {
                ex.Message.ToEditor();
                isCorrect = false;
            }

            return isCorrect;
        }

        internal static bool GenerateCornerPoints()
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

                    for (int j = 0; j < M.Colindante.ValorEdificio.Count; j++)
                    {
                        M.InEdificios edificioActual = M.Colindante.ValorEdificio[j];

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
                        if (!edificiosEvaluados.Contains(edificioEncontrado.longEntity))
                        {
                            ObjectId idEdificio = new Handle(edificioEncontrado.longEntity).toObjectId();

                            Polyline plEdificio = idEdificio.OpenEntity() as Polyline;

                            List<Point3d> ptsEdificio = plEdificio.ClockwisePoints();

                            //Por cada vertice del Edificio
                            for (int k = 0; k < ptsEdificio.Count; k++)
                            {
                                Point3d ptActual = ptsEdificio[k];
                                ObjectId idPtFound = new ObjectId();

                                //Reviso a que coordenada pertenece
                                if (!ptActual.ExistsPoint(out idPtFound))
                                {
                                    ////Punto A
                                    ptActual.ToDBPoint(index);

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
                            edificiosEvaluados.Add(edificioEncontrado.longEntity);
                        }
                    }
                    else
                    {
                        throw new Exception("No se encontró Edificio");
                    }
                }

                M.Colindante.LastPoint = index;
            }
            catch(Exception ex)
            {
                ex.Message.ToEditor();
                ex.StackTrace.ToEditor();
                return false;
            }

            return true;
        }

        private static void ReplaceCorners(Point3d pointFound, int numPoint)
        {
            for(int i = 0; i< M.Colindante.MainData.Count; i ++)
            {
                M.DatosColindancia mDatos = M.Colindante.MainData[i];

                if (mDatos.CoordenadaA.isEqualPosition(pointFound) && mDatos.PuntoA == -1)
                    mDatos.PuntoA = numPoint;

                if (mDatos.CoordenadaB.isEqualPosition(pointFound) && mDatos.PuntoB == -1)
                    mDatos.PuntoB = numPoint;
            }
        }

        private static M.SegmentInfo FindMidData(List<M.SegmentInfo> listSegments, 
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

                List<M.Apartments> listIn = M.Colindante.OrderedApartments.Where(x => x.longPl == lActual).ToList();

                bool isIn = listIn.Count() > 0;

                if (isIn)
                    listado.Add(listIn.FirstOrDefault());
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

                //Letra del Apartamento Inicial
                string letter = numeroActualAp.ToEnumerate();

                bool esFaltante = new bool();

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
                    //Si es más de 1 lo imprimo en el log de errores                    
                    if (i + 1 < idsApartments.Count)
                        letrasFaltantes = letrasFaltantes + letter + ", ";
                    else
                        letrasFaltantes = letrasFaltantes + letter;
                }
            }

            if (letrasFaltantes != "")
                M.Colindante.ListadoErrores.Add(new M.DescribeError()
                {
                    Error = "Letras Faltantes",
                    Description = "Faltan los apartamentos " + letrasFaltantes,
                    longObject = 0,
                    timeError = DateTime.Now,
                    tipoError = M.TipoError.Error,
                    Metodo = "OrderByLetters"
                });

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
                    M.Colindante.ListadoErrores.Add(new M.DescribeError()
                    {
                        Error = "Más de 1 Texto",
                        Description = string.Format("Se encontraron {0} Textos en Layer {1}"
                                                    , idsTextMatch.Count, M.Constant.LayerApartamento),
                        longObject = idApp.Handle.Value,
                        timeError = DateTime.Now,
                        tipoError = M.TipoError.Error,
                        Metodo = "FindApartment"
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
                M.Colindante.ValorEdificio.Clear();                
                
                if (idsEdificios.Count > 0)
                {                

                    //Reviso que corresponda la cantidad de Apartamentos con la de 
                    ObjectIdCollection cantAps = Met_Autodesk.ObjectsInside(min, max,
                                                                typeof(Polyline).Filter(M.Constant.LayerApartamento));

                    int cantCorrectaAps = idsEdificios.Count * M.Inicio.ApartamentosXVivienda;

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

                                if (apsInEdificio.Count != M.Inicio.ApartamentosXVivienda)
                                {                                                                     
                                    M.Colindante.ListadoErrores.Add(new M.DescribeError()
                                    {
                                        Error = "Menor Cant. de Apartamentos",
                                        Description = string.Format("En el Edificio {0} se detectaron {1} y deben ser {2} Apartamentos (Polilíneas)",
                                                                    idEdificio, apsInEdificio.Count, M.Inicio.ApartamentosXVivienda),
                                        longObject = idEdificio.Handle.Value,
                                        timeError = DateTime.Now,
                                        tipoError = M.TipoError.Error,
                                        Metodo = "Asigna_Edificios"
                                    });

                                    siIncorrecto = true;
                                }                    
                            }
                        }
                        else
                            //**********************************************SI INCORRECTO
                            siIncorrecto = true;
                    }
                    else
                    {                           
                        siIncorrecto = true;

                        M.Colindante.ListadoErrores.Add(new M.DescribeError()
                        {
                            Error = "Menor Cant. de Apartamentos",
                            Description =
                                string.Format("En el Macrolote se detectaron {1} y deben ser {2} Apartamentos",
                                                 cantAps.Count, cantCorrectaAps),
                            longObject = idMacrolote.Handle.Value,
                            timeError = DateTime.Now,
                            tipoError = M.TipoError.Error,
                            Metodo = "Asigna_Edificios"
                        });                       

                    }
                }
                else
                {
                    //**********************************************SI INCORRECTO
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
            string numEdificio = "";

            if (textoEnEdificio.Count == 1)
            {
                DBText text = textoEnEdificio.OfType<ObjectId>().FirstOrDefault().OpenEntity() as DBText;

                numEdificio = text.TextString.GetAfterSpace();

                M.Colindante.ValorEdificio.Add(new M.InEdificios()
                {
                    longEntity = idActual.Handle.Value,
                    numEdificio = numEdificio,
                    Apartments = Aps         
                });
            }
            else if (textoEnEdificio.Count > 1)
            {
                M.Colindante.ListadoErrores.Add(new M.DescribeError()
                {
                    longObject = idActual.Handle.Value,
                    Error = "Más de 1 Texto",
                    Description = "Se encontró más de 1 Texto con Layer " + M.Constant.LayerEdificio,
                    timeError = DateTime.Now,
                    tipoError = M.TipoError.Error,
                    Metodo = "AsignaEdificios"
                });
            }
            else
            {
                M.Colindante.ListadoErrores.Add(new M.DescribeError()
                {
                    longObject = idActual.Handle.Value,
                    Error = "Sin Texto",
                    Description = "No se encontró Texto con Layer " + M.Constant.LayerEdificio,
                    timeError = DateTime.Now,
                    tipoError = M.TipoError.Error,
                    Metodo = "AsignaEdificios"
                });
            }

            return numEdificio;
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

            idsLines = ptsGeometry.SelectByFence(typeof(Line).Filter(), idsToIgnore);

            foreach(ObjectId idLine in idsLines)
                idsSelected.Add(idLine);

            if (idsSelected.Count > 0)
            {
                ObjectId idManzana = new ObjectId(),
                            idLote = new ObjectId(),
                            idEdificio = new ObjectId(),
                            idApartamento = new ObjectId(), 
                            idSeccion = new ObjectId();

                List<string> allLayers = M.Colindante.TodosLayers.Select(x => x.Layername).ToList();

                //Todos los ids que tengan colindancia (adyacencia)
                List<ObjectId> idsAdjacent = idsSelected.OfType<ObjectId>()
                                .Where(x => x.OpenEntity().Layer != M.Constant.LayerAPAlta
                                && allLayers.Contains(x.OpenEntity().Layer)).ToList();

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

                        string numLote = M.Colindante.ValorLotes.Where(X => X.handleEntity == longLote).FirstOrDefault().numLote;

                        seccionColindancia = Met_General.JoinToWord(Translate(M.Constant.LayerLote), numLote);
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

                                string numEdificio = "",
                                       seccion = "";

                                M.Apartments aps = new M.Apartments();
                                //-----------------------------------------                               

                                //Obtengo Número de Edificio
                                numEdificio = M.Colindante.ValorEdificio.Where(X => X.longEntity == longEdificio).FirstOrDefault().numEdificio;

                                //Agrego Edificios al listado de colindancia
                                layersColindancia.Add(M.Constant.LayerEdificio);

                                if (idApartamento.IsValid)
                                {
                                    aps = M.Colindante.OrderedApartments.Where(x => x.longPl == idApartamento.Handle.Value).FirstOrDefault();
                                    layersColindancia.Add(M.Constant.LayerApartamento);
                                }

                                if (idSeccion.IsValid)
                                {                                    
                                    seccion = Translate(idSeccion.OpenEntity().Layer);
                                    layersColindancia.Add(idSeccion.OpenEntity().Layer);
                                }

                                seccionColindancia = Met_General.JoinToWord(Translate(M.Constant.LayerEdificio), numEdificio, Translate(M.Constant.LayerApartamento), aps.TextAp, seccion);
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

                                    numAp = M.Colindante.OrderedApartments.Where(x => x.longPl == lApartamento)
                                            .FirstOrDefault().TextAp;

                                    layersColindancia.Add(M.Constant.LayerApartamento);


                                    if (idSeccion.IsValid)
                                    {
                                        seccion = Translate(idSeccion.OpenEntity().Layer);
                                        layersColindancia.Add(idSeccion.OpenEntity().Layer);
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

                                numAp = M.Colindante.OrderedApartments.Where(x => x.longPl == lApartamento)
                                        .FirstOrDefault().TextAp;

                                layersColindancia.Add(M.Constant.LayerApartamento);

                                if (idSeccion.IsValid)
                                {
                                    seccion = Translate(idSeccion.OpenEntity().Layer);
                                    layersColindancia.Add(idSeccion.OpenEntity().Layer);
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
                M.Colindante.TodosLayers.Where(x => x.Layername == layerColindancia).ToList().FirstOrDefault();

            if (itemFound != null && itemFound != new M.DescribeLayer())
            {
                return itemFound.Description;
            }
            else
                return layerColindancia;
        }

        internal static string FindDirection(Point3d ptMedio, Point3dCollection pts)
        {
            string Rumbo = "";

            try
            {
                ObjectIdCollection idsSelected = new ObjectIdCollection(),
                                    idsLines = new ObjectIdCollection();
                                    //idsOut = new ObjectIdCollection();

                List<Line> linesInQuadrant = CreateQuadrants(ptMedio);

                //Dibujo las líneas
                foreach (Line _line in linesInQuadrant)
                    idsLines.Add(_line.DrawInModel());

                //Busco las lineas que se crucen o contengan la sección
                idsSelected = pts.SelectCrossing(typeof(Line).Filter());

                //De las líneas que cruzan las comparo con las líneas creadas
                List<ObjectId> idsFound = idsLines.OfType<ObjectId>().Where(x => !idsSelected.Contains(x)).ToList();

                //Las líneas que están afuera
                List<ObjectId> idsOut = idsLines.OfType<ObjectId>().Where(x => idsSelected.Contains(x)).ToList();

                if (idsFound.Count == 1)
                {
                    ObjectId idLine = idsFound.FirstOrDefault();

                    Line line = idLine.OpenEntity() as Line;

                    Point3d startPoint = new Point3d(),
                            endPoint = new Point3d();

                    startPoint = line.StartPoint;
                    endPoint = line.EndPoint;

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
                }
                else
                {
                    Rumbo = string.Format("{0} Líneas", idsFound.Count);
                }

                //if (idsFound.Count != 1)
                //{
                //    //Elimino todas las líneas
                //    foreach (ObjectId idDel in idsOut)
                //        idDel.GetAndRemove();
                //}
                //else
                //{
                    //Elimino todas las líneas
                    foreach (ObjectId idDel in idsLines)
                        idDel.GetAndRemove();
                //}


            }
            catch (Exception ex)
            {
                ex.Message.ToEditor();
            }

            return Rumbo;
        }

        internal static void DeleteAdjacencyObjects()
        {
            Met_Autodesk.DeleteObjects(M.Constant.LayerExcDBText);

            Met_Autodesk.DeleteObjects(M.Constant.LayerExcDBPoints);
        }

        private static ObjectId SearchRumbo(Point3d startPoint, Point3d endPoint, double angle)
        {
            bool detected = false;

            ObjectId idManzana = new ObjectId();

            int value = 100;

            while (!detected)
            {
                Point3dCollection ptsLine = new Point3dCollection(new Point3d[2]
                {
                    startPoint,
                    NewEndPoint(angle, endPoint, value)
                });

                ObjectIdCollection idsManzana
                        = ptsLine.SelectByFence(null);

                List<ObjectId> listManzana =
                    idsManzana.OfType<ObjectId>().Where(x => x.OpenEntity().Layer == M.Constant.LayerManzana).ToList();

                if (listManzana.Count > 0)
                {
                    idManzana = listManzana.FirstOrDefault();

                    detected = true;
                }
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

        private static List<Line> CreateQuadrants(Point3d ptMedio)
        {
            List<Line> listLines = new List<Line>();

            //Líneas de referencia
            //Linea 1 a 0°
            Line line0G = new Line(new Point3d(ptMedio.X + .01, ptMedio.Y, 0), //Inicia en este punto
                                new Point3d(ptMedio.X + .2, ptMedio.Y, 0));

            //Asigno a layer Especifica
            line0G.Layer = M.Constant.LayerExcRegimen;

            //Linea 2 a 90°
            Line line90G = new Line(new Point3d(ptMedio.X, ptMedio.Y + .01, 0), //Inicia en este punto
                                new Point3d(ptMedio.X, ptMedio.Y + .2, 0));            

            line90G.Layer = M.Constant.LayerExcRegimen;

            //Linea 3 a 180°
            Line line180G = new Line(new Point3d(ptMedio.X - .01, ptMedio.Y, 0), //Inicia en este punto
                                new Point3d(ptMedio.X - .2, ptMedio.Y, 0));

            line180G.Layer = M.Constant.LayerExcRegimen;

            //Linea 4 a 270°
            Line line270G = new Line(new Point3d(ptMedio.X, ptMedio.Y - .01, 0), //Inicia en este punto
                                new Point3d(ptMedio.X, ptMedio.Y - .2, 0));            

            line270G.Layer = M.Constant.LayerExcRegimen;

            listLines.Add(line0G);
            listLines.Add(line90G);
            listLines.Add(line180G);
            listLines.Add(line270G);

            return listLines;

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
            if (!pt.ExistsPoint(out idptFound))
            {
                //Si es Macrolote
                if(M.Manzana.EsMacrolote)
                {
                    //Reviso que no este dentro de las esquinas/vertices de Polilínea actual 
                    if (M.Colindante.PtsVertex.OfType<Point3d>().Where(x => x.isEqualPosition(pt)).Count() == 0)
                    {
                        pt.ToDBPoint(numPoint);
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
                    pt.ToDBPoint(numPoint);
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

        internal static bool ExistsPoint(this Point3d point3d, out ObjectId idPointFound)
        {
            Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            ObjectIdCollection idsAllPoints = new ObjectIdCollection();

            idPointFound = new ObjectId();

            PromptSelectionResult psr = ed.SelectAll(typeof(DBPoint).Filter(M.Constant.LayerExcDBPoints));

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

            foreach (string layer in layerstoCreate)
            {
                if(Met_Autodesk.CreateLayer(layer))                
                    count++;                
            }

            return count;
        }
    }
}

