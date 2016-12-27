using Autodesk.AutoCAD.ApplicationServices;
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
            bool flag = false;
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
            {
                flag = true;
                id = res.ObjectId;
            }

            return flag;
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

        /// <summary>
        /// Obtengo las Entidades de un layer en especifico
        /// </summary>
        /// <param name="layerName">Layer a filtrar</param>
        /// <param name="pts">Puntos de la Entidad</param>
        /// <param name="msjerror">Mensaje en dado caso de error</param>
        /// <returns>Colección de Ids dentro de Puntos3d</returns>
        public static ObjectIdCollection EntitiesByLayer(this Point3dCollection pts, string layerName,  
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
        public static List<Polyline> ModelPolyline(string Layername)
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
        /// Otengo objetos dentro de un poligono con un filtro
        /// </summary>
        /// <param name="pts">Puntos3d de Poligono</param>
        /// <param name="sf">Filtro de Selección</param>
        /// <param name="layerName">Layer a Filtrar</param>
        /// <returns>Colección de Ids (Objetos)</returns>
        internal static ObjectIdCollection ObjectsInside(Point3dCollection pts, SelectionFilter sf, 
            string layerName = "0")
        {            
            string entLayer = "";

            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            ObjectIdCollection  IdsGenerales = new ObjectIdCollection(),
                                IdsFinal = new ObjectIdCollection(); ;
            
            PromptSelectionResult psr;
            Entity ent;            

            psr = ed.SelectWindowPolygon(pts, sf);
            //Valida que el filtro funcione con palabra exacta
            if (psr.Status == PromptStatus.OK)
            {
                IdsFinal = new ObjectIdCollection(psr.Value.GetObjectIds());                
            }
            //Valida que no sea error por mayusculas o espacios
            else if (psr.Status == PromptStatus.Error)
            {
                psr = ed.SelectCrossingPolygon(pts);
                if (psr != null)
                    IdsGenerales = new ObjectIdCollection(psr.Value.GetObjectIds());
                else
                    IdsGenerales = new ObjectIdCollection();

                foreach (ObjectId objid in IdsGenerales)
                {
                    ent = objid.OpenEntity() as Entity;
                    entLayer = (ent.Layer.ToUpper().Replace(" ", string.Empty));
                    if (entLayer == layerName && ent.GetType().Name == "DBText")
                    {
                        IdsFinal.Add(objid);
                    }
                }                
               
            }
            return IdsFinal;
        }

        /// <summary>
        /// Forza la visibilidad de una polilinea dentro del modelo
        /// </summary>
        /// <param name="pl">Polilinea a mostrar</param>
        internal static void Zoom(this Polyline pl)
        {
            ViewTableRecord rec = new ViewTableRecord();
            rec.CenterPoint = MiddlePoint(pl);
            rec.Height = (pl.GeometricExtents.MaxPoint.Y - pl.GeometricExtents.MinPoint.Y) + 50;
            rec.Width = (pl.GeometricExtents.MaxPoint.X - pl.GeometricExtents.MinPoint.X) + 10;
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

        internal static void ToMessageEditor(this string msg)
        {
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            ed.WriteMessage(msg);
        }
    }
}
