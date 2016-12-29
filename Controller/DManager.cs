//AutoCAD References
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using System;
using System.Collections.Generic;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;
using AcadExc = Autodesk.AutoCAD.Runtime.Exception;

namespace RegimenCondominio.C
{
    public class DManager
    {
        public static Editor ed { get { return AcadApp.DocumentManager.MdiActiveDocument.Editor; } }
        /// <summary>
        /// Agrega un diccionario al diccionario de AutoCAD
        /// </summary>
        /// <param name="dictionaryName">El nombre del diccionario</param>
        /// <returns>El object id del diccionario recien creado</returns>
        public static ObjectId AddDictionary(String dictionaryName)
        {
            Database db = AcadApp.DocumentManager.MdiActiveDocument.Database;
            ObjectId id = new ObjectId();
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                try
                {
                    DBDictionary NOD =
                        (DBDictionary)db.NamedObjectsDictionaryId.GetObject(OpenMode.ForWrite);
                    DBDictionary d = new DBDictionary();
                    NOD.SetAt(dictionaryName, d);
                    tr.AddNewlyCreatedDBObject(d, true);
                    id = d.ObjectId;
                    tr.Commit();
                }
                catch (AcadExc exc)
                {
                    ed.WriteMessage(exc.Message);
                    tr.Abort();
                }
                catch (System.Exception exc)
                {
                    ed.WriteMessage(exc.Message);
                    tr.Abort();
                }
            }
            return id;
        }
        /// <summary>
        /// Agrega un diccionario al diccionario de extensión de una entidad
        /// </summary>
        /// <param name="dictionaryName">El nombre del diccionario</param>
        /// <param name="ent">La entidad agregar el diccionario</param>
        /// <returns>El object id del diccionario recien creado</returns>
        public static ObjectId AddDictionary(Entity ent, String dictionaryName)
        {
            Database db = AcadApp.DocumentManager.MdiActiveDocument.Database;
            ObjectId id = new ObjectId();
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                try
                {
                    Entity e = (Entity)ent.ObjectId.GetObject(OpenMode.ForRead);
                    DBDictionary extDic;
                    if (e.ExtensionDictionary.IsValid)
                        extDic = (DBDictionary)e.ExtensionDictionary.GetObject(OpenMode.ForWrite);
                    else
                    {
                        e.UpgradeOpen();
                        e.CreateExtensionDictionary();
                        extDic = (DBDictionary)e.ExtensionDictionary.GetObject(OpenMode.ForWrite);
                    }
                    DBDictionary d = new DBDictionary();
                    extDic.SetAt(dictionaryName, d);
                    tr.AddNewlyCreatedDBObject(d, true);
                    id = d.ObjectId;
                    tr.Commit();
                }
                catch (AcadExc exc)
                {
                    ed.WriteMessage(exc.Message);
                    tr.Abort();
                }
                catch (System.Exception exc)
                {
                    ed.WriteMessage(exc.Message);
                    tr.Abort();
                }
            }
            return id;
        }
        /// <summary>
        /// Agrega un diccionario a otro diccionario
        /// </summary>
        /// <param name="dictionaryName">El nombre del diccionario</param>
        /// <param name="dicId">El id del diccionario.</param>
        /// <returns>El object id del diccionario recien creado</returns>
        public static ObjectId AddDictionary(ObjectId dicId, String dictionaryName)
        {
            Database db = AcadApp.DocumentManager.MdiActiveDocument.Database;
            ObjectId id = new ObjectId();
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                try
                {
                    DBDictionary oldD = (DBDictionary)dicId.GetObject(OpenMode.ForWrite);
                    DBDictionary d = new DBDictionary();
                    oldD.SetAt(dictionaryName, d);
                    tr.AddNewlyCreatedDBObject(d, true);
                    id = d.ObjectId;
                    tr.Commit();
                }
                catch (AcadExc exc)
                {
                    ed.WriteMessage(exc.Message);
                    tr.Abort();
                }
                catch (System.Exception exc)
                {
                    ed.WriteMessage(exc.Message);
                    tr.Abort();
                }
            }
            return id;
        }
        /// <summary>
        /// Agrega un xrecord en un diccionario
        /// </summary>
        /// <param name="dictionaryName">El nombre del diccionario</param>
        /// <param name="dicId">El id del diccionario.</param>
        /// <returns>El object id del xrecord recien creado</returns>
        public static ObjectId AddXRecord(ObjectId dicId, String XrecordName)
        {
            Database db = AcadApp.DocumentManager.MdiActiveDocument.Database;
            ObjectId id = new ObjectId();
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                try
                {
                    DBDictionary oldD = (DBDictionary)dicId.GetObject(OpenMode.ForWrite);
                    Xrecord x = new Xrecord();
                    oldD.SetAt(XrecordName, x);
                    tr.AddNewlyCreatedDBObject(x, true);
                    id = x.ObjectId;
                    tr.Commit();
                }
                catch (AcadExc exc)
                {
                    ed.WriteMessage(exc.Message);
                    tr.Abort();
                }
                catch (System.Exception exc)
                {
                    ed.WriteMessage(exc.Message);
                    tr.Abort();
                }
            }
            return id;
        }
        /// <summary>
        /// Agrega información a un Xrecord
        /// </summary>
        /// <param name="XrecId">El id del Xrecord.</param>
        /// <param name="data">La información que se guardara en el Xrecord</param>
        /// <returns>El object id del xrecord recien creado</returns>
        public static void AddData(ObjectId XrecId, params String[] data)
        {
            Database db = AcadApp.DocumentManager.MdiActiveDocument.Database;
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                try
                {
                    Xrecord oldX = (Xrecord)XrecId.GetObject(OpenMode.ForWrite);
                    List<TypedValue> typedValueData = new List<TypedValue>();
                    for (int i = 0; i < data.Length; i++)
                        typedValueData.Add(new TypedValue((int)DxfCode.Text, data[i]));
                    oldX.Data = new ResultBuffer(typedValueData.ToArray());
                    tr.Commit();
                }
                catch (AcadExc exc)
                {
                    ed.WriteMessage(exc.Message);
                    tr.Abort();
                }
                catch (System.Exception exc)
                {
                    ed.WriteMessage(exc.Message);
                    tr.Abort();
                }
            }
        }
        /// <summary>
        /// Remueve un diccionario en el diccionario de AutoCAD
        /// </summary>
        /// <param name="dictionaryName">El nombre del diccionario</param>
        public static void RemoveDictionary(String dictionaryName)
        {
            Database db = AcadApp.DocumentManager.MdiActiveDocument.Database;
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                try
                {
                    DBDictionary NOD =
                        (DBDictionary)AcadApp.DocumentManager.MdiActiveDocument.Database.NamedObjectsDictionaryId.GetObject(OpenMode.ForWrite);
                    NOD.Remove(dictionaryName);
                    tr.Commit();
                }
                catch (AcadExc exc)
                {
                    ed.WriteMessage(exc.Message);
                    tr.Abort();
                }
                catch (System.Exception exc)
                {
                    ed.WriteMessage(exc.Message);
                    tr.Abort();
                }
            }
        }
        /// <summary>
        /// Remueve un diccionario en el diccionario de extensión de una 
        /// entidad.
        /// </summary>
        /// <param name="dictionaryName">El nombre del diccionario</param>
        /// <param name="ent">La entidad a remover el diccionario</param>
        public static void RemoveDictionary(Entity ent, String dictionaryName)
        {
            Database db = AcadApp.DocumentManager.MdiActiveDocument.Database;
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                try
                {
                    Entity e = (Entity)ent.ObjectId.GetObject(OpenMode.ForRead);
                    DBDictionary extDic;
                    if (e.ExtensionDictionary.IsValid)
                    {
                        extDic = (DBDictionary)e.ExtensionDictionary.GetObject(OpenMode.ForWrite);
                        extDic.Remove(dictionaryName);
                    }
                    tr.Commit();
                }
                catch (AcadExc exc)
                {
                    ed.WriteMessage(exc.Message);
                    tr.Abort();
                }
                catch (System.Exception exc)
                {
                    ed.WriteMessage(exc.Message);
                    tr.Abort();
                }
            }
        }
        /// <summary>
        /// Remueve un diccionario en el diccionario de extensión de una 
        /// entidad.
        /// </summary>
        /// <param name="dictionaryName">El nombre del diccionario</param>
        /// <param name="dicId">El id del diccionario</param>
        public static void RemoveDictionary(ObjectId dicId, String dictionaryName)
        {
            Database db = AcadApp.DocumentManager.MdiActiveDocument.Database;
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                try
                {
                    DBDictionary oldD = (DBDictionary)dicId.GetObject(OpenMode.ForWrite);
                    oldD.Remove(dictionaryName);
                    tr.Commit();
                }
                catch (AcadExc exc)
                {
                    ed.WriteMessage(exc.Message);
                    tr.Abort();
                }
                catch (System.Exception exc)
                {
                    ed.WriteMessage(exc.Message);
                    tr.Abort();
                }
            }
        }
        /// <summary>
        /// Remueve un xRecord en el diccionario de extensión de una 
        /// entidad.
        /// </summary>
        /// <param name="dictionaryName">El nombre del diccionario</param>
        /// <param name="dicId">El id del diccionario</param>
        public static void RemoveXRecord(ObjectId dicId, String XrecordName)
        {
            Document doc = AcadApp.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                using (doc.LockDocument())
                {
                    try
                    {
                        DBDictionary oldD = (DBDictionary)dicId.GetObject(OpenMode.ForWrite);
                        Xrecord x = new Xrecord();
                        oldD.Remove(XrecordName);
                        tr.Commit();
                    }
                    catch (AcadExc exc)
                    {
                        ed.WriteMessage(exc.Message);
                        tr.Abort();
                    }
                    catch (System.Exception exc)
                    {
                        ed.WriteMessage(exc.Message);
                        tr.Abort();
                    }
                }
            }
        }
        /// <summary>
        /// Obtiene un diccionario del diccionario de AutoCAD
        /// </summary>
        /// <param name="dictionaryName">El nombre del diccionario</param>
        /// <returns>El object id del diccionario recien creado</returns>
        public static ObjectId GetDictionary(String dictionaryName)
        {
            Database db = AcadApp.DocumentManager.MdiActiveDocument.Database;
            ObjectId id = new ObjectId();
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                try
                {
                    DBDictionary NOD =
                        (DBDictionary)AcadApp.DocumentManager.MdiActiveDocument.Database.NamedObjectsDictionaryId.GetObject(OpenMode.ForRead);
                    id = NOD.GetAt(dictionaryName);
                    tr.Commit();
                }
                catch (AcadExc exc)
                {
                    ed.WriteMessage(exc.Message);
                    tr.Abort();
                }
                catch (System.Exception exc)
                {
                    ed.WriteMessage(exc.Message);
                    tr.Abort();
                }
            }
            return id;
        }
        /// <summary>
        /// Obtiene un diccionario del diccionario de extensión de una entidad
        /// </summary>
        /// <param name="dictionaryName">El nombre del diccionario</param>
        /// <param name="ent">La entidad agregar el diccionario</param>
        /// <returns>El object id del diccionario recien creado</returns>
        public static ObjectId GetDictionary(Entity ent, String dictionaryName)
        {
            Database db = AcadApp.DocumentManager.MdiActiveDocument.Database;
            ObjectId id = new ObjectId();
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                try
                {
                    Entity e = (Entity)ent.ObjectId.GetObject(OpenMode.ForRead);
                    DBDictionary extDic;
                    if (e.ExtensionDictionary.IsValid)
                    {
                        extDic = (DBDictionary)e.ExtensionDictionary.GetObject(OpenMode.ForRead);
                        id = extDic.GetAt(dictionaryName);
                    }
                    tr.Commit();
                }
                catch (AcadExc exc)
                {
                    ed.WriteMessage(exc.Message);
                    tr.Abort();
                }
                catch (System.Exception exc)
                {
                    ed.WriteMessage(exc.Message);
                    tr.Abort();
                }
            }
            return id;
        }
        /// <summary>
        /// Obtiene un diccionario a otro diccionario
        /// </summary>
        /// <param name="dictionaryName">El nombre del diccionario</param>
        /// <param name="dicId">El id del diccionario.</param>
        /// <returns>El object id del diccionario recien creado</returns>
        public static ObjectId GetDictionary(ObjectId dicId, String dictionaryName)
        {
            Database db = AcadApp.DocumentManager.MdiActiveDocument.Database;
            ObjectId id = new ObjectId();
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                try
                {
                    DBDictionary oldD = (DBDictionary)dicId.GetObject(OpenMode.ForRead);
                    id = oldD.GetAt(dictionaryName);
                    tr.Commit();
                }
                catch (AcadExc exc)
                {
                    ed.WriteMessage(exc.Message);
                    tr.Abort();
                }
                catch (System.Exception exc)
                {
                    ed.WriteMessage(exc.Message);
                    tr.Abort();
                }
            }
            return id;
        }
        /// <summary>
        /// Obtiene un xrecord de un diccionario
        /// </summary>
        /// <param name="dictionaryName">El nombre del diccionario</param>
        /// <param name="dicId">El id del diccionario.</param>
        /// <returns>El object id del xrecord recien creado</returns>
        public static ObjectId GetXRecord(ObjectId dicId, String XrecordName)
        {
            Database db = AcadApp.DocumentManager.MdiActiveDocument.Database;
            ObjectId id = new ObjectId();
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                try
                {
                    DBDictionary oldD = (DBDictionary)dicId.GetObject(OpenMode.ForRead);
                    String key = "";
                    foreach (var obj in oldD)
                    {
                        key = obj.Key;
                    }

                    id = oldD.GetAt(XrecordName);
                    tr.Commit();
                }
                catch (AcadExc exc)
                {
                    ed.WriteMessage(exc.Message);
                    tr.Abort();
                }
                catch (System.Exception exc)
                {
                    ed.WriteMessage(exc.Message);
                    tr.Abort();
                }
            }
            return id;
        }
        /// <summary>
        /// Obtiene información de un Xrecord
        /// </summary>
        /// <param name="XrecId">El id del Xrecord.</param>
        /// <returns>El object id del xrecord recien creado</returns>
        public static string[] GetData(ObjectId XrecId)
        {
            Database db = AcadApp.DocumentManager.MdiActiveDocument.Database;

            Document doc = AcadApp.DocumentManager.MdiActiveDocument;

            List<string> values = new List<string>();
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                using (DocumentLock dc = doc.LockDocument())
                {
                    try
                    {
                        Xrecord oldX = (Xrecord)XrecId.GetObject(OpenMode.ForWrite);
                        TypedValue[] tps = oldX.Data.AsArray();
                        foreach (TypedValue tp in tps)
                            values.Add((string)tp.Value);
                    }
                    catch (AcadExc exc)
                    {
                        ed.WriteMessage(exc.Message);
                        tr.Abort();
                    }
                    catch (System.Exception exc)
                    {
                        ed.WriteMessage(exc.Message);
                        tr.Abort();
                    }
                }
            }
            return values.ToArray();
        }

        public static ObjectIdCollection IdsByXRecord(string recordName)
        {
            ObjectIdCollection objCol = new ObjectIdCollection();

            Database db = AcadApp.DocumentManager.MdiActiveDocument.Database;

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                try
                {

                    BlockTable blkTab = db.BlockTableId.GetObject(OpenMode.ForRead) as BlockTable;
                    BlockTableRecord model = blkTab[BlockTableRecord.ModelSpace].GetObject(OpenMode.ForRead) as BlockTableRecord;
                    DBObject obj;
                    foreach (ObjectId id in model)
                    {
                        if (id.IsValid)
                        {
                            Entity ent = id.OpenEntity();

                            if (ent.ExtensionDictionary.IsValid)
                            {
                                DBDictionary dbDict = (ent.ExtensionDictionary).
                                    GetObject(OpenMode.ForRead) as DBDictionary;                               

                                if(dbDict.Contains(recordName))
                                {                                    
                                        objCol.Add(id);
                                }

                                
                            }
                        }
                    }                   
                }
                catch (AcadExc exc)
                {
                    ed.WriteMessage(exc.Message);
                    tr.Abort();
                }
                catch (Exception exc)
                {
                    ed.WriteMessage(exc.Message);
                    tr.Abort();
                }
            }

            return objCol;
        }

    }
}
