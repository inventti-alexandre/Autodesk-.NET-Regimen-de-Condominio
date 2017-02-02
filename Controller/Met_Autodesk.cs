using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RegimenCondominio.C
{
    public static class Met_Autodesk
    {

        public static void SetImpliedSelection(this ObjectId idEntity)
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;            

            ObjectId[] ids = new ObjectId[] { idEntity };

            ed.SetImpliedSelection(ids);
        }

        /// <summary>
        /// Obtengo Entidad de cierto tipo del plano
        /// </summary>
        /// <param name="msg">Mensaje para mostrar en el Editor</param>
        /// <param name="id">Id de la Entidad seleccionada </param>
        /// <param name="tps">Tipos de Entidades aceptadas</param>
        /// <returns>Verdadero si se seleccionó una Entidad Valida</returns>
        public static bool Entity(string msg, out ObjectId id, params Type[] tps)
        {
            id = new ObjectId();
            
            //Opciones
            PromptEntityOptions opt = new PromptEntityOptions(msg);
            if (tps.Length > 0)
            {
                opt.SetRejectMessage("\nNo es del tipo solicitado");
                foreach (Type tp in tps)
                    opt.AddAllowedClass(tp, true);
            }
            //Resultado
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            PromptEntityResult res = ed.GetEntity(opt);

            if (res.Status == PromptStatus.OK)
                id = res.ObjectId;            

            return id.IsValid;
        }

        /// <summary>
        /// Obtengo Entidad del plano
        /// </summary>
        /// <param name="msg">Mensaje para mostrar en el Editor</param>
        /// <param name="id">Id de Entidad</param>
        /// <returns>Verdadero si se seleccionó una Entidad Valida</returns>
        public static bool Entity(string msg, out ObjectId id)
        {
            id = new ObjectId();
            bool flag = false;
            //Opciones
            PromptEntityOptions opt = new PromptEntityOptions(msg);
            
            //Resultado
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            PromptEntityResult res = ed.GetEntity(opt);
            if (res.Status == PromptStatus.OK)
            {
                flag = true;
                id = res.ObjectId;
            }

            return flag;
        }

        /// <summary>
        /// Extraigo vertices de la polilinea
        /// </summary>
        /// <param name="plId">Id de la polilinea</param>
        /// <returns>Vertices de Puntos3d</returns>
        public static Point3dCollection ExtractVertex(this ObjectId plId)
        {
            Point3dCollection pts = new Point3dCollection();

            Document doc = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;
            //Abrimos la BD y el editor

            Database dwg = doc.Database;
            Editor ed = doc.Editor;
            //En la BD se encuentra el transaction manager que se encarga de 
            //controlar todas las transacciones
            using (Transaction tr = dwg.TransactionManager.StartTransaction())
            {
                try
                {
                    Polyline pl = plId.GetObject(OpenMode.ForRead) as Polyline;
                    for (int i = 0; i < pl.NumberOfVertices; i++)
                    {
                        pts.Add(pl.GetPoint3dAt(i));

                    }
                    tr.Commit();
                }
                catch (Autodesk.AutoCAD.Runtime.Exception exc)
                {
                    //Se deshace lo que sale mla en la transacción
                    //El abort lo destruye
                    ed.WriteMessage(exc.Message);
                    tr.Abort();
                }
            }

            return pts;
        }

        internal static bool SelectPolylines(out ObjectIdCollection ids, string msg, string layer)
        {
            ids = new ObjectIdCollection();


            TypedValue[] FilType = new TypedValue[2];
            FilType.SetValue(new TypedValue((int)DxfCode.Start, RXObject.GetClass(typeof(Polyline)).DxfName), 0);
            FilType.SetValue(new TypedValue((int)DxfCode.LayerName, layer), 1);

            SelectionFilter SelFilter = new SelectionFilter(FilType);

            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            PromptSelectionOptions pt = new PromptSelectionOptions();

            pt.MessageForAdding = msg;

            pt.AllowDuplicates = false;

            //Resultado
            PromptSelectionResult res = ed.GetSelection(pt, SelFilter);

            if (res.Status == PromptStatus.OK)
            {
                ids = new ObjectIdCollection(res.Value.GetObjectIds());

            }

            return ids.Count > 0;
        }

        /// <summary>
        /// Obtengo Polilineas dentro de listado segmentos
        /// </summary>
        /// <param name="listSegments">Listado de segmentos</param>
        /// <param name="layerName">Layer de polilineas a buscar</param>
        /// <returns>Polilineas dentro de segmentos</returns>
        internal static ObjectIdCollection GetPolylinesInSegments(List<ObjectId> listSegments, string layerName = "0")
        {
            //Inicializo variables-------
            //Ids de Polilineas dentro de Segmentos
            ObjectIdCollection idsInside = new ObjectIdCollection();

            //Documento activo
            Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;

            //Base de datos de documentos activos
            Database db = doc.Database;
            
            //Polilinea temporal creada
            Polyline plSegments = new Polyline();

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                using (DocumentLock myLock = doc.LockDocument())
                {
                    try
                    {
                        if (SegmentsToPolyline(listSegments, out plSegments))
                        {
                            M.Colindante.IdPolManzana = plSegments.Id;

                            plSegments.Focus(50, 10);

                            Point3dCollection pt3d = plSegments.Id.ExtractVertex();

                            TypedValue[] tvs = new TypedValue[2]
                            {
                                new TypedValue((int)DxfCode.LayerName, layerName),
                                new TypedValue( (int)DxfCode.Start, RXObject.GetClass(typeof(Polyline)).DxfName)
                            };

                            SelectionFilter sf = new SelectionFilter(tvs);

                            Point3d min = new Point3d(), max = new Point3d();

                            //Obtengo punto mínimo de la polílinea
                            min = plSegments.GeometricExtents.MinPoint;
                            max = plSegments.GeometricExtents.MaxPoint;

                            idsInside = ObjectsInside(min, max, sf);

                            //if (!plSegments.IsWriteEnabled)
                            //    plSegments.UpgradeOpen();

                            //plSegments.Erase(true);                            

                            tr.Commit();
                        }
                    }
                    catch (Autodesk.AutoCAD.Runtime.Exception ex)
                    {
                        ex.Message.ToEditor();
                        tr.Abort();
                    }
                    catch (System.Exception ex)
                    {
                        ex.Message.ToEditor();
                        tr.Abort();
                    }
                }
            }

            return idsInside;
        }        

        internal static void GetAndRemove(this ObjectId id)
        {
            //Documento activo
            Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;

            //Nueva Entidad
            Entity ent;

            //Base de datos de documentos activos
            Database db = doc.Database;            

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                using (DocumentLock myLock = doc.LockDocument())
                {
                    try
                    {
                        ent = id.GetObject(OpenMode.ForWrite) as Entity;

                        ent.Erase(true);

                        tr.Commit();

                    }
                    catch (Autodesk.AutoCAD.Runtime.Exception ex)
                    {
                        ex.Message.ToEditor();
                        tr.Abort();
                    }
                    catch (System.Exception ex)
                    {
                        ex.Message.ToEditor();
                        tr.Abort();
                    }
                }
            }
        }

        /// <summary>
        /// Selection Window de Punto Mínimo y Máximo
        /// </summary>
        /// <param name="min">Punto 3d Mínimo</param>
        /// <param name="max">Punto 3d Máximo</param>
        /// <param name="sf">Filtro de selección</param>
        /// <returns>Ids dentro de Puntos</returns>
        internal static ObjectIdCollection ObjectsInside(Point3d min, Point3d max, SelectionFilter sf)
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            ObjectIdCollection IdsGenerales = new ObjectIdCollection(),
                                IdsFinal = new ObjectIdCollection(); ;

            PromptSelectionResult psr;

            if (sf != null)
            {
                psr = ed.SelectWindow(min, max, sf);
            }
            else
                psr = ed.SelectWindow(min, max);

            //Valida que el filtro funcione con palabra exacta
            if (psr.Status == PromptStatus.OK)
            {
                IdsFinal = new ObjectIdCollection(psr.Value.GetObjectIds());
            }

            return IdsFinal;
        }

        /// <summary>
        /// Convierte Segmentos en Polilinea
        /// </summary>
        /// <param name="listSegments">Listado de segmentos</param>
        /// <param name="outPl">Polilinea Creada</param>
        /// <returns>Verdadero si Id es Válido/returns>
        private static bool SegmentsToPolyline(List<ObjectId> listSegments, out Polyline outPl)
        {
            outPl = new Polyline();           
            
            ObjectIdCollection idsInside = new ObjectIdCollection();
            Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;
            Polyline plSegments = new Polyline();
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                using (DocumentLock myLock = doc.LockDocument())
                {

                    try
                    {
                        BlockTableRecord btr = 
                            (BlockTableRecord)tr.GetObject(db.CurrentSpaceId, OpenMode.ForWrite);
                        PolylineSegmentCollection psc = new PolylineSegmentCollection();
                        Plane plane = new Plane(Point3d.Origin, Vector3d.ZAxis);

                        foreach (ObjectId id in listSegments)
                        {
                            Entity ent = (Entity)tr.GetObject(id, OpenMode.ForRead);
                            switch (ent.GetType().Name)
                            {
                                case "Arc":
                                    Arc arc = (Arc)ent;
                                    psc.Add(new PolylineSegment(
                                        new CircularArc2d(
                                            arc.Center.Convert2d(plane),
                                            arc.Radius,
                                            arc.StartAngle,
                                            arc.EndAngle,
                                            Vector2d.XAxis,
                                            false)));
                                    break;
                                case "Ellipse":
                                    Ellipse el = (Ellipse)ent;
                                    psc.AddRange(new PolylineSegmentCollection(el));
                                    break;
                                case "Line":
                                    Line l = (Line)ent;
                                    psc.Add(new PolylineSegment(
                                        new LineSegment2d(
                                            l.StartPoint.Convert2d(plane),
                                            l.EndPoint.Convert2d(plane))));
                                    break;
                                case "Polyline":
                                    Polyline pl = (Polyline)ent;
                                    psc.AddRange(new PolylineSegmentCollection(pl));
                                    break;
                                case "Spline":
                                    try
                                    {
                                        Spline spl = (Spline)ent;
                                        psc.AddRange(new PolylineSegmentCollection((Polyline)spl.ToPolyline()));
                                    }
                                    catch (Autodesk.AutoCAD.Runtime.Exception ex) { ex.Message.ToEditor(); }
                                    break;
                                default:
                                    break;
                            }
                        }

                        PolylineSegmentCollection segs = psc.Join()[0];


                        outPl = segs.ToPolyline();
                        outPl.Layer = M.Constant.LayerExcRegimen;
                        btr.AppendEntity(outPl);
                        tr.AddNewlyCreatedDBObject(outPl, true);
                        tr.Commit();                        
                    }
                    catch (Autodesk.AutoCAD.Runtime.Exception ex)
                    {
                        ex.Message.ToEditor();
                        tr.Abort();
                    }
                    catch (System.Exception ex)
                    {
                        ex.Message.ToEditor();
                        tr.Abort();
                    }
                }
            }

            return outPl.Id.IsValid;
        }

        internal static void DeleteObjects(string layername)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            using (Transaction tr = doc.Database.TransactionManager.StartTransaction())
            {
                using (doc.LockDocument())
                {
                    try
                    {
                        BlockTable blkTab = doc.Database.BlockTableId.GetObject(OpenMode.ForWrite) as BlockTable;
                        BlockTableRecord model = blkTab[BlockTableRecord.ModelSpace].GetObject(OpenMode.ForWrite) as BlockTableRecord;
                        Entity ent;
                        foreach (ObjectId id in model)
                        {
                            ent = id.GetObject(OpenMode.ForRead) as Entity;

                            if(ent != null)
                            {
                                if(ent.Layer == layername)
                                {
                                    ent.UpgradeOpen();
                                    ent.Erase(true);
                                }
                            }
                                                            
                        }
                        tr.Commit();
                    }
                    catch (System.Exception exc)
                    {
                        tr.Abort();
                        doc.Editor.WriteMessage(exc.Message);
                    }
                }
            }
        }

        /// <summary>
        /// Obtengo los DBTEXT de un layer en especifico del modelo
        /// </summary>
        /// <param name="layerName">Layer a filtrar</param>
        /// <param name="pts">Puntos de la Entidad</param>
        /// <param name="msjerror">Mensaje en dado caso de error</param>
        /// <returns>Colección de Ids dentro de Puntos3d</returns>
        public static ObjectIdCollection DBTextByLayer(this Point3dCollection pts, string layerName,  
            out string msjerror)
        {

            msjerror = "";
            string entLayer = "";
            Editor ed = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument.Editor;
            ObjectIdCollection IdsGenerales = new ObjectIdCollection();
            ObjectIdCollection IdsFinal = new ObjectIdCollection();
            PromptSelectionResult psr;
            Entity ent;


            TypedValue[] tvs = new TypedValue[2]
                            {
                                new TypedValue((int)DxfCode.LayerName, layerName),
                                new TypedValue( (int)DxfCode.Start, RXObject.GetClass(typeof(DBText)).DxfName)
                            };
            SelectionFilter sf = new SelectionFilter(tvs);

            psr = ed.SelectWindowPolygon(pts, sf);
            //Valida que el filtro funcione con palabra exacta
            if (psr.Status == PromptStatus.OK)
            {
                IdsFinal = new ObjectIdCollection(psr.Value.GetObjectIds());
                return IdsFinal;
            }
            //Valida que no sea error por mayusculas o espacios
            else if (psr.Status == PromptStatus.Error)
            {
                psr = ed.SelectWindowPolygon(pts);
                if (psr != null)
                    if (psr.Value != null)
                        IdsGenerales = new ObjectIdCollection(psr.Value.GetObjectIds());
                    else
                        IdsGenerales = new ObjectIdCollection();

                foreach (ObjectId objid in IdsGenerales)
                {
                    ent = objid.OpenEntity() as Entity;
                    entLayer = (ent.Layer.ToUpper().Replace(" ", string.Empty));
                    if (entLayer == (layerName.ToUpper().Replace(" ", string.Empty)) && ent.GetType().Name == "DBText")
                    {
                        IdsFinal.Add(objid);
                    }
                }
                if (IdsFinal.Count == 0)
                {
                    msjerror = "Falta configurar la Capa: " + layerName;
                }

                return IdsFinal;
            }
            else
            {
                msjerror = "Falta configurar la Capa: " + layerName;
                return IdsFinal;
            }
        }

        /// <summary>
        /// Abrir Entidad a Modo Lectura
        /// </summary>
        /// <param name="entId">Id de Entidad</param>
        /// <returns>Entidad Abierta</returns>
        public static Entity OpenEntity(this ObjectId entId)
        {
            Entity ent = null;
            Document doc = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;
            //Abrimos la BD y el editor

            Database dwg = doc.Database;
            Editor ed = doc.Editor;
            //En la BD se encuentra el transaction manager que se encarga de 
            //controlar todas las transacciones
            using (Transaction tr = dwg.TransactionManager.StartTransaction())
            {
                try
                {
                    ent = entId.GetObject(OpenMode.ForRead) as Entity;
                    tr.Commit();
                }
                catch (Autodesk.AutoCAD.Runtime.Exception exc)
                {
                    //Se deshace lo que sale mla en la transacción
                    //El abort lo destruye
                    ed.WriteMessage(exc.Message);
                    tr.Abort();
                }
            }

            return ent;
        }

        /// <summary>
        /// Abrir Entidad a Modo Escritura
        /// </summary>
        /// <param name="entId">Id de Entidad</param>
        /// <returns>Entidad Abierta</returns>
        public static Entity OpenForWrite(this ObjectId entId)
        {
            Entity ent = null;
            Document doc = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;
            //Abrimos la BD y el editor

            Database dwg = doc.Database;
            Editor ed = doc.Editor;
            //En la BD se encuentra el transaction manager que se encarga de 
            //controlar todas las transacciones
            using (Transaction tr = dwg.TransactionManager.StartTransaction())
            {
                using (DocumentLock myLock = doc.LockDocument())
                {
                    try
                    {
                        ent = entId.GetObject(OpenMode.ForWrite) as Entity;
                        tr.Commit();
                    }
                    catch (Autodesk.AutoCAD.Runtime.Exception exc)
                    {
                        //Se deshace lo que sale mla en la transacción
                        //El abort lo destruye
                        ed.WriteMessage(exc.Message);
                        tr.Abort();
                    }
                }
            }

            return ent;
        }

        public static ObjectId toObjectId(this Handle h)
        {
            ObjectId id = new ObjectId();
            Document doc = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;
            //Abrimos la BD y el editor

            Database db = doc.Database;
            Editor ed = doc.Editor;
            //En la BD se encuentra el transaction manager que se encarga de 
            //controlar todas las transacciones
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                try
                {
                    id = db.GetObjectId(false, h, 0);
                    tr.Commit();
                }
                catch (Autodesk.AutoCAD.Runtime.Exception exc)
                {
                    //Se deshace lo que sale mla en la transacción
                    //El abort lo destruye
                    ed.WriteMessage(exc.Message);
                    tr.Abort();
                }
            }

            return id;
        }

        /// <summary>
        /// Crea e Inserta Diccionario de Datos a Entidad
        /// </summary>
        /// <param name="entId">Id de Entidad</param>
        /// <param name="recordName">Nombre del Record</param>
        /// <param name="recordData">Datos del Record</param>
        /// <returns>Verdadero si inserto los datos</returns>
        public static bool InsertDictionary(ObjectId entId, string recordName, params string[] recordData)
        {
            bool flag = false;
            Entity ent;
            Document doc = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;            
            //Abrimos la BD y el editor
            Database dwg = doc.Database;            

            Editor ed = doc.Editor;
            //En la BD se encuentra el transaction manager que se encarga de 
            //controlar todas las transacciones
            using (Transaction tr = dwg.TransactionManager.StartTransaction())
            {
                using (DocumentLock myLock = doc.LockDocument())
                {
                    try
                    {
                        ent = entId.GetObject(OpenMode.ForWrite) as Entity;

                        if (!ent.IsWriteEnabled)
                            ent.UpgradeOpen();

                        if (!ent.ExtensionDictionary.IsValid)
                        {
                            ent.CreateExtensionDictionary();
                        }

                        ObjectId xRecordId = DManager.AddXRecord(ent.ExtensionDictionary,
                                                                    recordName);

                        DManager.AddData(xRecordId, recordData);

                        tr.Commit();
                        flag = true;
                    }
                    catch (Autodesk.AutoCAD.Runtime.Exception exc)
                    {
                        //Se deshace lo que sale mla en la transacción
                        //El abort lo destruye
                        ed.WriteMessage(exc.Message);
                        tr.Abort();
                    }
                }
            }

            return flag;
        }

        /// <summary>
        /// Obtengo todas las polilineas dentro de un plano especificando 
        /// </summary>
        /// <param name="Layername">Layer de polilineas</param>
        /// <returns>Listado de polilineas</returns>
        public static List<Polyline> ModelPolylines(string Layername)
        {
            List<Polyline> TodasPolilineas = new List<Polyline>();
            Document doc = Application.DocumentManager.MdiActiveDocument;
            using (Transaction tr = doc.Database.TransactionManager.StartTransaction())
            {
                try
                {
                    BlockTable blkTab = doc.Database.BlockTableId.GetObject(OpenMode.ForRead) as BlockTable;
                    BlockTableRecord model = blkTab[BlockTableRecord.ModelSpace].GetObject(OpenMode.ForRead) as BlockTableRecord;
                    DBObject obj;
                    foreach (ObjectId id in model)
                    {
                        obj = id.GetObject(OpenMode.ForRead);
                        if (obj is Polyline && (obj as Polyline).Layer == Layername)
                            TodasPolilineas.Add(obj as Polyline);
                    }
                    tr.Commit();
                }
                catch (System.Exception exc)
                {
                    tr.Abort();
                    doc.Editor.WriteMessage(exc.Message);
                }
            }

            return TodasPolilineas;
        }

        /// <summary>
        /// Obtengo todos los DBText de algún layer en específico
        /// </summary>
        /// <param name="Layername">Layer a Filtrar</param>
        /// <returns>Lista de DBTexts encontrados</returns>
        public static List<DBText> ModelDBText(string Layername)
        {
            List<DBText> TodoDBText = new List<DBText>();
            Document doc = Application.DocumentManager.MdiActiveDocument;
            using (Transaction tr = doc.Database.TransactionManager.StartTransaction())
            {
                try
                {
                    BlockTable blkTab = doc.Database.BlockTableId.GetObject(OpenMode.ForRead) as BlockTable;
                    BlockTableRecord model = blkTab[BlockTableRecord.ModelSpace].GetObject(OpenMode.ForRead) as BlockTableRecord;
                    DBObject obj;
                    foreach (ObjectId id in model)
                    {
                        obj = id.GetObject(OpenMode.ForRead);
                        if (obj is DBText && (obj as DBText).Layer == Layername)
                            TodoDBText.Add(obj as DBText);
                    }
                    tr.Commit();
                }
                catch (System.Exception exc)
                {
                    tr.Abort();
                    doc.Editor.WriteMessage(exc.Message);
                }
            }

            return TodoDBText;
        }

        /// <summary>
        /// Obtengo objetos dentro de un poligono
        /// </summary>
        /// <param name="pts">Puntos3d de Poligono</param>
        /// <param name="layerName">Layer a Filtrar</param>
        /// <returns>Colección de Ids (Objetos)</returns>
        internal static ObjectIdCollection ObjectsInside(Point3dCollection pts, 
             SelectionFilter sf)
        {                        
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            ObjectIdCollection  IdsGenerales = new ObjectIdCollection(),
                                IdsFinal = new ObjectIdCollection(); ;

            PromptSelectionResult psr;

            if (sf != null)
            {
                psr = ed.SelectWindowPolygon(pts, sf);
            }
            else
                psr = ed.SelectWindowPolygon(pts);

            //Valida que el filtro funcione con palabra exacta
            if (psr.Status == PromptStatus.OK)
            {
                IdsFinal = new ObjectIdCollection(psr.Value.GetObjectIds());                
            }           
                        
            return IdsFinal;
        }

        /// <summary>
        /// Forza la visibilidad de una polilinea dentro del modelo
        /// </summary>
        /// <param name="pl">Polilinea a mostrar</param>
        internal static void Focus(this Polyline pl, int plusHeight, int plusWidth)
        {            
            ViewTableRecord rec = new ViewTableRecord();
            rec.CenterPoint = MiddlePoint(pl);
            rec.Height = (pl.GeometricExtents.MaxPoint.Y - pl.GeometricExtents.MinPoint.Y) + plusHeight;
            rec.Width = (pl.GeometricExtents.MaxPoint.X - pl.GeometricExtents.MinPoint.X) + plusWidth;
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            ed.SetCurrentView(rec);
            ed.Regen();
        }

        /// <summary>
        /// Calcula el punto medio de una polilinea
        /// </summary>
        /// <param name="pl">Polilinea</param>
        /// <returns>Punto Medio 2d</returns>
        internal static Point2d MiddlePoint(Polyline pl)
        {
            return new Point2d((pl.GeometricExtents.MinPoint.X + pl.GeometricExtents.MaxPoint.X) / 2,
                                             (pl.GeometricExtents.MinPoint.Y + pl.GeometricExtents.MaxPoint.Y) / 2);
        }

        /// <summary>
        /// Determina si la polilinea fue dibujada en sentido horario
        /// Mediante el calculo del algortimo del área con signo
        /// http://www.mathopenref.com/coordpolygonarea2.html
        /// </summary>
        /// <param name="pl">La polilínea a revisar</param>
        /// <returns>Verdadero si es sentido horario</returns>
        internal static bool IsClockwise(this Polyline pl)
        {
            Point2d middle = new Point2d((pl.GeometricExtents.MinPoint.X + pl.GeometricExtents.MaxPoint.X) / 2,
                                        (pl.GeometricExtents.MinPoint.Y + pl.GeometricExtents.MaxPoint.Y) / 2);
            Double dx = middle.X, dy = middle.Y, sum = 0;
            Point2d prev, current;
            for (int i = 1; i < pl.NumberOfVertices; i++)
            {
                prev = pl.GetPoint2dAt(i - 1);
                current = pl.GetPoint2dAt(i);
                sum += (prev.X - dx) * (current.Y - dy) - (prev.Y - dy) * (current.X - dx);
            }
            return (sum / 2) < 0;
        }

        internal static void ToEditor(this string msg)
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            ed.WriteMessage(msg);
        }
        /*
        public static bool CreatePoints(this Polyline pl)
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            
            //Documento activo
            Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;

            //Base de datos de documentos activos
            Database db = doc.Database;

            //Revisa que no haya problema
            bool flag = false;

            //Enfoco Polilínea
            pl.Focus();

            Point3d min = new Point3d(),
                    max = new Point3d();

            string strNoLote = "", strNoOficial = "";            

            min = pl.GeometricExtents.MinPoint;

            max = pl.GeometricExtents.MaxPoint;

            strNoLote = TextInWindow(min, max, M.Constant.LayerLote);

            strNoOficial = TextInWindow(min, max, M.Constant.LayerNoOficial);

            //idsOrdenadas
            ObjectIdCollection orderIdsApartments;                        

            //Obtengo Apartamentos dentro del Lote
            ObjectIdCollection idsApartments = ObjectsInside(   pl.GeometricExtents.MinPoint,
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
                        ObjectId idTextAp = ObjectsInside(  ptmin, ptMax, 
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
                            idSeccion = ObjectsInside(ptmin, ptMax, typeof(Polyline).Filter(layerSeccion))
                                                .OfType<ObjectId>().FirstOrDefault();

                            //Si encontro una polilínea válida dentro del layer de la sección
                            if (idSeccion.IsValid)
                            {
                                //Polilinea de la sección
                                Polyline plSeccion = idSeccion.OpenEntity() as Polyline;

                                //Genera el orden correcto de los puntos
                                List<Point3d> listPoints = plSeccion.ClockwisePoints();

                                //Guardo Punto donde Inició la Búsqueda
                                int startPoint = NumPoint;

                                //Puntos A y B
                                int numPointA = new int(), 
                                    numPointB = new int();

                                for (int j = 0; j < listPoints.Count; j++)
                                {
                                    //Colindancia
                                    string  strColindancia = string.Empty, 
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
                                    int idxPtActual = GetPointIndex(plSeccion, ptA);

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
                                        strColindancia = ptMedio.FindAdjacency(idSeccion, plApartmento.Id, TextAp, out strRumbo);

                                        //Encuentro Rumbo
                                        if (strRumbo == string.Empty)
                                            strRumbo = FindDirection();
                                    }




                                    #region Número de PuntoA y B
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
                    ed.WriteMessage(exc.Message);
                }
                catch (System.Exception exc)
                {
                    //Se deshace lo que sale mal en la transacción
                    //El abort lo destruye
                    ed.WriteMessage(exc.Message);
                }
            }

            return flag;
        }
        */
        internal static ObjectId DrawInModel(this Entity ent)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            ObjectId idEntity = new ObjectId();
            using (Transaction tr = doc.Database.TransactionManager.StartTransaction())
            {
                using (DocumentLock dc = doc.LockDocument())
                {
                    try
                    {                        
                        BlockTableRecord currentSpace = (BlockTableRecord)doc.Database.CurrentSpaceId.GetObject(OpenMode.ForWrite);
                        idEntity = currentSpace.AppendEntity(ent);
                        tr.AddNewlyCreatedDBObject(ent, true);
                        tr.Commit();
                    }
                    catch (Autodesk.AutoCAD.Runtime.Exception exc)
                    {
                        tr.Abort();
                        doc.Editor.WriteMessage(exc.Message);
                    }
                }
            }

            return idEntity;
        }        

        internal static Point3dCollection CreateGeometry(int numOfVertex, double apo, Point3d center)
        {
            Point3dCollection ptsGenerados = new Point3dCollection();

            double delta = new double();                    

            delta = (2 * Math.PI) / numOfVertex;

            for (int i = 0; i < numOfVertex; i++)
            {
                double  x = new double(),
                        y = new double();

                x = center.X + apo * Math.Cos(delta * i);
                y = center.Y + apo * Math.Sin(delta * i);
                ptsGenerados.Add(new Point3d(x, y, 0));
            }

            return ptsGenerados;
        }

        internal static ObjectIdCollection SelectByFence(this Point3dCollection pts, SelectionFilter filter, params ObjectId[] idsToIgnore)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            ObjectIdCollection result = new ObjectIdCollection();
            PromptSelectionResult res;

            using (DocumentLock _docLock = doc.LockDocument())
            {
                try
                {
                    if (filter != null)
                        res = ed.SelectFence(pts, filter);
                    else
                        res = ed.SelectFence(pts);
                    if (res.Status == PromptStatus.OK)
                    {
                        List<ObjectId> qR = res.Value.GetObjectIds().Where(x => !idsToIgnore.Contains(x)).ToList();
                        if (qR.Count() > 0)
                            result = new ObjectIdCollection(qR.ToArray());
                    }
                }
                catch (Autodesk.AutoCAD.Runtime.Exception ex)
                {
                    ex.Message.ToEditor();
                }
                catch (System.Exception ex)
                {
                    ex.Message.ToEditor();
                }
            }

            return result;
        }

        internal static bool ChangeLength(this Line line, Point3d newEndPoint)
        {
            
            bool flag = false;

            Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                using (DocumentLock dc = doc.LockDocument())
                {
                    try
                    {
                        Line lineFW = line.Id.GetObject(OpenMode.ForWrite) as Line;

                        lineFW.EndPoint = newEndPoint;

                        tr.Commit();
                    }
                    catch (Autodesk.AutoCAD.Runtime.Exception CADEx)
                    {
                        ed.WriteMessage(CADEx.Message);
                        tr.Abort();
                    }
                    catch (System.Exception ex)
                    {
                        ed.WriteMessage(ex.Message);
                        ex.Message.ToEditor();
                        tr.Abort();
                    }
                }
            }                      

            return flag;
        }

        internal static ObjectIdCollection SelectCrossing(this Point3dCollection pts, SelectionFilter filter, params Object[] idstoIgnore)
        {
            ObjectIdCollection ids = new ObjectIdCollection();

            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            ObjectIdCollection result = new ObjectIdCollection();
            PromptSelectionResult res;

            res = ed.SelectCrossingPolygon(pts, filter);

            if (res.Status == PromptStatus.OK)
            {
                if (res.Value.Count > 0)
                    ids = new ObjectIdCollection(res.Value.GetObjectIds());
            }

            return ids;
        }

        internal static ObjectIdCollection SelectFence(this Point3dCollection Vertex, SelectionFilter filter, params Object[] idstoIgnore)
        {
            ObjectIdCollection ids = new ObjectIdCollection();

            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            ObjectIdCollection result = new ObjectIdCollection();
            PromptSelectionResult res;

            res = ed.SelectFence(Vertex, filter);

            if (res.Status == PromptStatus.OK)
            {
                if (res.Value.Count > 0)
                    ids = new ObjectIdCollection(res.Value.GetObjectIds());
            }

            return ids;
        }

        internal static string TextInWindow(Point3d min, Point3d max, string layer)
        {
            string text = "";

            //Obtengo DBTEXT
            ObjectId idDBText = ObjectsInside(min, max, typeof(DBText).Filter(layer))
                                .OfType<ObjectId>().FirstOrDefault();

            //Obtengo Número de Lote
            if (idDBText.IsValid)
                text = (idDBText.OpenEntity() as DBText).TextString;

            return text;
        }

        internal static Point3d MiddlePoint(this Arc _arc, Point3d ptA, Point3d ptB)
        {
            double middleAngle = (_arc.StartAngle + _arc.EndAngle) / 2;

            Point3d Center = _arc.Center;

            double x = new double(),
                    y = new double();

            //x = c.X + r * Math.Cos(ang);
            //y = c.Y + r*Math.Sin(ang);
            x = Center.X + (_arc.Radius * Math.Cos(middleAngle));
            y = Center.Y + (_arc.Radius * Math.Sin(middleAngle));

            return new Point3d(x, y, 0);
        }

        internal static Point3d MiddlePoint(this Point3d ptA, Point3d ptB)
        {
            return new Point3d(((ptA.X + ptB.X) / 2), ((ptA.Y + ptB.Y) / 2), 0);
        }

        internal static bool ToDBPoint(this Point3d ptToAdd, int numPoint)
        {
            Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            bool siInserta = false;           

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                using (DocumentLock dc = doc.LockDocument())
                {
                    try
                    {
                        BlockTableRecord btr =
                           (BlockTableRecord)tr.GetObject(db.CurrentSpaceId, OpenMode.ForWrite);

                        DBPoint newPoint = new DBPoint(ptToAdd);

                        newPoint.Layer = M.Constant.LayerExcDBPoints;                        

                        ObjectId idPoint = btr.AppendEntity(newPoint);

                        if (idPoint.IsValid)
                            InsertDictionary(idPoint, M.Constant.XRecordPoints, numPoint.ToString());

                        tr.AddNewlyCreatedDBObject(newPoint, true);

                        DBText txt = new DBText()
                        {
                            Position = ptToAdd,
                            TextString = numPoint.ToString(),
                            Height = 0.05d,
                            Layer = M.Constant.LayerExcDBText
                        };

                        btr.AppendEntity(txt);
                        tr.AddNewlyCreatedDBObject(txt, true);

                        db.Pdmode = 0;
                        db.Pdsize = 0;

                        tr.Commit();

                        siInserta = true;
                    }
                    catch(Autodesk.AutoCAD.Runtime.Exception CADEx)
                    {
                        ed.WriteMessage(CADEx.Message);
                        tr.Abort();
                    }
                    catch (System.Exception ex)
                    {
                        ed.WriteMessage(ex.Message);
                        ex.Message.ToEditor();
                        tr.Abort();
                    }

                    return siInserta;
                }
            }
        }
        public static int GetPointIndex(Polyline plSeccion, Point3d ptActual)
        {            
            for (int i = 0; i < plSeccion.NumberOfVertices; i++)
            {
                if (plSeccion.GetPoint3dAt(i) == ptActual)
                    return i;
            }

            return new int();
        }

        internal static bool OrderByLetters(this ObjectIdCollection ids, out ObjectIdCollection orderIds)
        {
            orderIds = new ObjectIdCollection();

            for(int i=0; i < ids.Count; i ++)
            {
                //Letra del Apartamento Inicial
                char Letter = M.Constant.Alphabet[i];

                ObjectId id = new ObjectId();

                id = ids.FindApartment(Letter);

                if (id.IsValid)
                    orderIds.Add(id);
            }                                           

            return orderIds.Count == ids.Count;
        }

        internal static SelectionFilter Filter(this Type typeClass, string layerName)
        {

            TypedValue[] tvs = new TypedValue[2]
                            {
                                new TypedValue((int)DxfCode.LayerName, layerName),
                                new TypedValue( (int)DxfCode.Start, RXObject.GetClass(typeClass).DxfName)
                            };

            return new SelectionFilter(tvs);
        }

        internal static SelectionFilter Filter(string layerName)
        {

            TypedValue[] tvs = new TypedValue[1]
                            {
                                new TypedValue((int)DxfCode.LayerName, layerName),                                
                            };

            return new SelectionFilter(tvs);
        }

        internal static SelectionFilter Filter(this Type typeClass)
        {
            TypedValue[] tvs = new TypedValue[1]
                            {
                                new TypedValue( (int)DxfCode.Start, RXObject.GetClass(typeClass).DxfName)
                            };

            return new SelectionFilter(tvs);
        }

        internal static SelectionFilter Filter(this Type[] typeClass)
        {

            TypedValue[] tvs = new TypedValue[typeClass.Count()];

            for (int i = 0; i < typeClass.Count(); i++)
            {
                tvs[i] = new TypedValue((int)DxfCode.Start, RXObject.GetClass(typeClass[i]).DxfName);
            }
                    
            return new SelectionFilter(tvs);
        }

        internal static SelectionFilter Filter(this Type[] typeClass, string layerName)
        {

            TypedValue[] tvs = new TypedValue[typeClass.Count() + 1];

            for (int i = 0; i < typeClass.Count(); i++)
            {
                tvs[i] = new TypedValue((int)DxfCode.Start, RXObject.GetClass(typeClass[i]).DxfName);
            }

            tvs[typeClass.Count()] = new TypedValue((int)DxfCode.LayerName, layerName);

            return new SelectionFilter(tvs);
        }

        private static ObjectId FindApartment(this ObjectIdCollection ids, char letter)
        {
            SelectionFilter sf = typeof(DBText).Filter(M.Constant.LayerApartamento);
            
            ObjectIdCollection AppLetters = new ObjectIdCollection();

            foreach(ObjectId idApp in ids)
            {
                Polyline pl = idApp.OpenEntity() as Polyline;

                ObjectId idText = ObjectsInside(pl.GeometricExtents.MinPoint, pl.GeometricExtents.MaxPoint, sf)
                                    .OfType<ObjectId>()
                                    .Where(x => (x.OpenEntity() as DBText).TextString == letter.ToString())
                                    .FirstOrDefault();                

                if(idText.IsValid)
                    return idApp;
            }

            return new ObjectId();
        }

        public static List<Point3d> ClockwisePoints(this Polyline pline)
        {
            Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            List<Point3d> points = new List<Point3d>();

            for (int i = 0; i < pline.NumberOfVertices; i++)
            {
                Point3d ptActual = pline.GetPoint3dAt(i);

                if (!ptActual.IsPointInList(points))
                    points.Add(ptActual);
            }

            if (GetArea(pline) > 0)
                points.Reverse();

            SpecialOrder(points);

            return points;
        }

        internal static bool IsPointInList(this Point3d pt, List<Point3d> list)
        {
            double dist = 0.005;

            int count = list.Where(x => x.DistanceTo(pt) <= dist).Count();

            return count >= 1;
        }

        internal static bool IsFirstPoint(this Polyline pl, Point3d pt)
        {
            double dist = 0.005;

            bool isFirstPoint = pl.GetPoint3dAt(0).DistanceTo(pt) <= dist;

            return isFirstPoint;
        }

        internal static bool ExistsPoint(this Point3d point3d, out ObjectId idPointFound)
        {
            Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            ObjectIdCollection idsAllPoints= new ObjectIdCollection();

            idPointFound = new ObjectId();

            PromptSelectionResult psr = ed.SelectAll(typeof(DBPoint).Filter());


            if(psr.Status == PromptStatus.OK)
            {
                idsAllPoints = new ObjectIdCollection(psr.Value.GetObjectIds());

                idPointFound = idsAllPoints.OfType<ObjectId>()
                    .Where(x => (x.OpenEntity() as DBPoint).Position.DistanceTo(point3d) <= 0.005).FirstOrDefault();                
            }

            return idPointFound.IsValid;

        }

        public static void SpecialOrder(List<Point3d> points)
        {
            //Asigno el primer valor de los puntos
            var pt = points[0];
            //Index de inicio
            int index = 0;

            for (int i = 1; i < points.Count; i++)
            {
                double pointActualX = double.Parse(points[i].X.ToString("N3")),
                        pointActualY = double.Parse(points[i].Y.ToString("N3"));

                double pointX = double.Parse(pt.X.ToString("N3")),
                        pointY = double.Parse(pt.Y.ToString("N3"));

                if (pointActualX > pointX
                    || (pointActualX == pointX && pointActualY < pointY))
                {
                    pt = points[i];
                    index = i;
                }
            }
            for (int i = 0; i < index; i++)
            {
                points.Add(points[0]);
                points.RemoveAt(0);
            }
        }

        public static double GetArea(Point2d pt1, Point2d pt2, Point2d pt3)
        {
            return (((pt2.X - pt1.X) * (pt3.Y - pt1.Y)) -
                        ((pt3.X - pt1.X) * (pt2.Y - pt1.Y))) / 2.0;
        }

        public static double GetArea(CircularArc2d arc)
        {
            double rad = arc.Radius;
            double ang = arc.IsClockWise ?
                arc.StartAngle - arc.EndAngle :
                arc.EndAngle - arc.StartAngle;
            return rad * rad * (ang - Math.Sin(ang)) / 2.0;
        }

        public static double GetArea(Polyline pline)
        {
            CircularArc2d arc = new CircularArc2d();
            double area = 0.0;
            int last = pline.NumberOfVertices - 1;
            Point2d p0 = pline.GetPoint2dAt(0);

            if (pline.GetBulgeAt(0) != 0.0)
            {
                area += GetArea(pline.GetArcSegment2dAt(0));
            }
            for (int i = 1; i < last; i++)
            {
                area += GetArea(p0, pline.GetPoint2dAt(i), pline.GetPoint2dAt(i + 1));
                if (pline.GetBulgeAt(i) != 0.0)
                {
                    area += GetArea(pline.GetArcSegment2dAt(i)); ;
                }
            }
            if ((pline.GetBulgeAt(last) != 0.0) && pline.Closed)
            {
                area += GetArea(pline.GetArcSegment2dAt(last));
            }
            return area;
        }

        internal static bool CreateLayer(string layerName)
        {
            // Get the current document and database
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database dbDoc = doc.Database;

            //Id de Layer
            ObjectId idLayerCreated = new ObjectId();

            using (Transaction acTrans = dbDoc.TransactionManager.StartTransaction())
            {
                using (doc.LockDocument())
                {
                    try
                    {
                        // Abrir el layerTable de Lectura
                        LayerTable layerTable;
                        layerTable = acTrans.GetObject(dbDoc.LayerTableId,
                                                        OpenMode.ForRead) as LayerTable;

                        if (!layerTable.Has(layerName))
                        {
                            using (LayerTableRecord acLyrTblRec = new LayerTableRecord())
                            {
                                // Assign the layer the ACI color 3 and a name
                                acLyrTblRec.Color = Color.FromColorIndex(ColorMethod.ByAci, 3);
                                acLyrTblRec.Name = layerName;

                                // Upgrade the Layer table for write
                                layerTable.UpgradeOpen();

                                // Append the new layer to the Layer table and the transaction
                                idLayerCreated = layerTable.Add(acLyrTblRec);
                                acTrans.AddNewlyCreatedDBObject(acLyrTblRec, true);
                            }
                        }
                        acTrans.Commit();
                    }
                    catch (Autodesk.AutoCAD.Runtime.Exception ex)
                    {
                        ex.Message.ToEditor();
                    }
                    catch (System.Exception ex)
                    {
                        ex.Message.ToEditor();
                    }
                }                
            }

            return idLayerCreated.IsValid;
        }

        public static Point2d MiddlePoint(this CircularArc2d _arc)
        {
            double middleAngle = (_arc.StartAngle + _arc.EndAngle) / 2;

            Point2d center = _arc.Center;

            double x = center.X + (_arc.Radius * Math.Cos(middleAngle));
            double y = center.Y + (_arc.Radius * Math.Sin(middleAngle));

            return new Point2d(x, y);
        }

        public static Point3d To3d(this Point2d pt)
        {
           return new Point3d(pt.X, pt.Y, 0);
        }

    }
}
