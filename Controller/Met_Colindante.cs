using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RegimenCondominio.C
{
    public static class Met_Colindante
    {
        public static bool CreatePoints(ObjectId idPl)
        {            
            //Revisa que no haya problema
            bool flag = false;

            Polyline pl = idPl.OpenEntity() as Polyline;

            //Enfoco Polilínea
            pl.Focus(50, 10);

            Point3d min = new Point3d(),
                    max = new Point3d();

            string strNoLote = "", strNoOficial = "";

            min = pl.GeometricExtents.MinPoint;

            max = pl.GeometricExtents.MaxPoint;

            strNoLote = Met_Autodesk.TextInWindow(min, max, M.Constant.LayerLote);

            strNoOficial = Met_Autodesk.TextInWindow(min, max, M.Constant.LayerNoOficial);

            //idsOrdenadas
            ObjectIdCollection orderIdsApartments;

            //Obtengo Apartamentos dentro del Lote
            ObjectIdCollection idsApartments 
                = Met_Autodesk.ObjectsInside(pl.GeometricExtents.MinPoint,
                                             pl.GeometricExtents.MaxPoint,
                                             typeof(Polyline).Filter(M.Constant.LayerApartamento));

            if (idsApartments.OrderByLetters(out orderIdsApartments))
            {
                try
                {
                    int NumPoint = 1;

                    //Por cada Apartamento Ordenado
                    for (int i = 0; i < orderIdsApartments.Count; i++)
                    {
                        //Apartamento Actual de A -> Z
                        Polyline plApartmento = orderIdsApartments[i].OpenEntity() as Polyline;                       

                        //Texto del Apartamento
                        string TextAp = "";

                        //Punto mínimo del apartamento
                        Point3d ptmin = plApartmento.GeometricExtents.MinPoint;

                        //Punto máximo del apartamento
                        Point3d ptMax = plApartmento.GeometricExtents.MaxPoint;

                        //Obtengo el Nombre del Apartamento dentro del layer DBTEXT
                        ObjectId idTextAp = Met_Autodesk.ObjectsInside(ptmin, ptMax,
                                            typeof(DBText).Filter(M.Constant.LayerApartamento))
                                            .OfType<ObjectId>().FirstOrDefault();

                        //Obtengo String del Nombre del Apartamento
                        if (idTextAp.IsValid)
                            TextAp = (idTextAp.OpenEntity() as DBText).TextString;

                        //Comienzo busqueda por cada Sección
                        foreach (string layerSeccion in M.Colindante.Secciones)
                        {
                            ObjectId idSeccion = new ObjectId();
                            //Obtengo cada sección de acuerdo al layer                               
                            idSeccion = Met_Autodesk.ObjectsInside(ptmin, ptMax, 
                                        typeof(Polyline).Filter(layerSeccion))
                                        .OfType<ObjectId>().FirstOrDefault();

                            //Si encontro una polilínea válida dentro del layer de la sección
                            if (idSeccion.IsValid)
                            {
                                //Polilinea de la sección
                                Polyline plSeccion = idSeccion.OpenEntity() as Polyline;

                                //Enfoco Polilinea
                                plSeccion.Focus(20,10);

                                //Genera el orden correcto de los puntos
                                List<Point3d> listPoints = plSeccion.ClockwisePoints();

                                //Guardo Punto donde Inició la Búsqueda
                                int startPoint = NumPoint;

                                //Puntos A y B
                                int numPointA = new int(),
                                    numPointB = new int();

                                //Por cada vertice de la sección
                                for (int j = 0; j < listPoints.Count; j++)
                                {
                                    //Colindancia
                                    string strColindancia = string.Empty,
                                            strRumbo = string.Empty;

                                    ////Punto A
                                    Point3d ptA = listPoints[j];

                                    //Punto B
                                    Point3d ptB = new Point3d();

                                    //Asigno Punto B
                                    if (j + 1 < listPoints.Count)
                                        ptB = listPoints[j + 1];
                                    else
                                        ptB = listPoints[0];

                                    #region Calculo Distancia y Punto Medio

                                    //Inicializo distancia
                                    double distance = new double();

                                    //Obtengo el index real
                                    int idxPtActual = Met_Autodesk.GetPointIndex(plSeccion, ptA);

                                    //Punto Medio
                                    Point3d ptMedio = new Point3d();

                                    //Dependiendo del tipo de segmento
                                    if (plSeccion.GetSegmentType(idxPtActual) == SegmentType.Arc)
                                    {
                                        CircularArc2d cd2d = plSeccion.GetArcSegment2dAt(idxPtActual);

                                        Arc arc = new Arc(cd2d.MiddlePoint().To3d(), cd2d.Radius,
                                            cd2d.StartAngle, cd2d.EndAngle);

                                        distance = arc.Length.Trunc(3);

                                        ptMedio = arc.MiddlePoint(ptA, ptB);
                                    }
                                    else
                                    {
                                        distance = ptA.DistanceTo(ptB).Trunc(3);

                                        ptMedio = ptA.MiddlePoint(ptB);
                                    }
                                    #endregion

                                    //Encuentro colindancia de acuerdo al punto medio
                                    if (idSeccion.IsValid)
                                    {
                                        (M.Colindante.IdPolManzana.OpenEntity() as Polyline).Focus(10,30);

                                        strColindancia = FindAdjacency(ptMedio, idSeccion, plApartmento.Id, TextAp, out strRumbo);

                                        //Encuentro Rumbo si no estaba en la colindancia
                                        if (strRumbo == "")
                                           strRumbo = FindDirection(ptMedio, new Point3dCollection(listPoints.ToArray()));
                                    }

                                    #region Creo DB Points y DB Text
                                    //Si es el primer punto debo de analizar PuntoA y PuntoB
                                    if (j == 0)
                                    {
                                        if (ValidatePoint(ptA, NumPoint, out numPointA))
                                            NumPoint++;
                                        else
                                            //Valido si realmente el punto de Inicio ya estaba insertado                                            
                                            startPoint = numPointA;

                                        if (ValidatePoint(ptB, NumPoint, out numPointB))
                                            NumPoint++;
                                    }
                                    else
                                    {
                                        numPointA = numPointB;

                                        if (j + 1 < listPoints.Count)//Si no es el último Punto lo valido
                                        {
                                            if (ValidatePoint(ptB, NumPoint, out numPointB))
                                                NumPoint++;
                                        }
                                        else//En dado caso que si es el último punto cierro la polilínea con el punto Inicial
                                            numPointB = startPoint;
                                    }

                                    #endregion

                                    M.Colindante.MainData.Add(new M.DatosColindancia()
                                    {
                                        Seccion = layerSeccion,
                                        Rumbo = strRumbo,
                                        Lote = strNoLote,
                                        NoOficial = strNoOficial,
                                        Apartamento = TextAp,
                                        PuntoA = numPointA,
                                        PuntoB = numPointB,
                                        Colindancia = strColindancia,
                                        Distancia = distance
                                    });
                                }

                            }
                        }
                    }

                    flag = true;
                }
                catch (Autodesk.AutoCAD.Runtime.Exception exc)
                {
                    //Se deshace lo que sale mal en la transacción
                    //El abort lo destruye
                    exc.Message.ToEditor();
                }
                catch (Exception exc)
                {
                    //Se deshace lo que sale mal en la transacción
                    //El abort lo destruye
                    exc.Message.ToEditor();
                }
            }

            return flag;
        }

        internal static string FindAdjacency(Point3d ptMedio, ObjectId idSeccion, ObjectId idApartment, string letter, out string Rumbo)
        {
            Point3dCollection ptsGeometry = new Point3dCollection();

            ObjectIdCollection idsSelected = new ObjectIdCollection();

            ptsGeometry = Met_Autodesk.CreateGeometry(7, 0.05, ptMedio);

            //ObjectId idFigure = ptsGeometry.Draw(7);

            //Obtengo todos los Ids que colindan
            idsSelected = ptsGeometry.SelectByFence(typeof(Polyline).Filter(), idSeccion);

            Rumbo = "";

            if (idsSelected.Count > 0)
            {
                ObjectId idAdjacent = new ObjectId();

                idAdjacent = idsSelected.OfType<ObjectId>()
                                .Where(x => x.OpenEntity().Layer != M.Constant.LayerAPAlta && M.Colindante.Secciones.Contains(x.OpenEntity().Layer))
                                .FirstOrDefault();

                if (idAdjacent.IsValid)
                {
                    List<ObjectId> idsAp = idsSelected.OfType<ObjectId>()
                        .Where(x => x.OpenEntity().Layer == M.Constant.LayerApartamento && x != idApartment)
                        .ToList();

                    string Ap = "", layerColindancia = "";

                    layerColindancia = idAdjacent.OpenEntity().Layer;

                    if (idsAp.Count() > 0)
                    {
                        Polyline plApartment = idsAp.FirstOrDefault().OpenEntity() as Polyline;

                        //plApartment.Focus();

                        ObjectId idText = Met_Autodesk.ObjectsInside
                                        (plApartment.GeometricExtents.MinPoint,
                                         plApartment.GeometricExtents.MaxPoint,
                                         typeof(DBText).Filter(M.Constant.LayerApartamento))
                                         .OfType<ObjectId>().FirstOrDefault();

                        if (idText.IsValid)
                            Ap = (idText.OpenEntity() as DBText).TextString;

                        return layerColindancia + " " + "AP-" + Ap;
                    }
                    else
                    {
                        return layerColindancia;
                    }
                }
                else
                {
                    //Busco si la colindancia se encuentra con la Manzana
                    idAdjacent = idsSelected.OfType<ObjectId>()
                                .Where(x => x.OpenEntity().Layer == M.Constant.LayerManzana)
                                .FirstOrDefault();

                    if(idAdjacent.IsValid)
                    {
                        Entity ent = idAdjacent.OpenEntity();

                        if(ent.ExtensionDictionary.IsValid)
                        {
                            ObjectId idXrecord = DManager.GetXRecord(ent.ExtensionDictionary, M.Constant.XRecordColindancia);

                            string[] data = DManager.GetData(idXrecord);

                            if(data.Count() > 0)
                                Rumbo = data[0];
                        }

                        return ent.Layer;                        
                    }
                }
            }

            return string.Empty;
        }

        internal static string FindDirection(Point3d ptMedio, Point3dCollection pts)
        {
            string Rumbo = "";

            try
            {
                ObjectIdCollection idsSelected = new ObjectIdCollection(),
                                    idsLines = new ObjectIdCollection(),
                                    idsOut = new ObjectIdCollection();

                List<Line> linesInQuadrant = CreateQuadrants(ptMedio);

                //Dibujo las líneas
                foreach (Line _line in linesInQuadrant)
                    idsLines.Add(_line.DrawInModel());

                //Busco las lineas que se crucen o contengan la sección
                idsSelected = pts.SelectFence(typeof(Line).Filter());

                //De las líneas que cruzan las comparo con las líneas creadas
                List<ObjectId> idsFound = idsLines.OfType<ObjectId>().Where(x => !idsSelected.Contains(x)).ToList();

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

                //Elimino todas las líneas
                foreach (ObjectId idDel in idsLines)
                    idDel.GetAndRemove();

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
                pt.ToDBPoint(numPoint);
                finalNum = numPoint;
                siInserta = true;
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

