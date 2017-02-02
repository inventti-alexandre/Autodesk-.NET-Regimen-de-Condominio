using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Windows;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using RegimenCondominio.C;

namespace RegimenCondominio
{
    public class Commands
    {
        [CommandMethod("REGIMEN", CommandFlags.Session)]
        public void IniciaReg()
        {
            //V.ModuloColindante win = new V.ModuloColindante();
            V.ModuloInicial win = new V.ModuloInicial();
            Autodesk.AutoCAD.ApplicationServices.Core.Application.ShowModelessWindow(win);
        }

        [CommandMethod("DICTIONARY_ENTITIES")]
        public void DicEntities()
        {
            ObjectIdCollection ids = DManager.IdsByXRecord(M.Constant.XRecordColindancia);

            if (ids.Count > 0)
            {
                foreach (ObjectId id in ids)
                {
                    Entity ent = id.OpenEntity() as Entity;

                    ObjectId idXrecord = DManager.GetXRecord(ent.ExtensionDictionary, M.Constant.XRecordColindancia);

                    string[] data = DManager.GetData(idXrecord);

                    string.Format("{0} tiene colindancia al {1} con {2}\n",
                        ent.GetType().Name, data[0], data[1]).ToEditor();
                }
            }
            else
                string.Format("No hay ids con record: {0}", M.Constant.XRecordColindancia)
                    .ToEditor();
        }

        [CommandMethod("DELETE_DBPOINTS")]
        public void DBPoints()
        {

            Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;

            
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                using (DocumentLock dc = doc.LockDocument())
                {
                    try
                    {

                        BlockTable blkTab = db.BlockTableId.GetObject(OpenMode.ForRead) as BlockTable;
                        BlockTableRecord model = blkTab[BlockTableRecord.ModelSpace].GetObject(OpenMode.ForRead) as BlockTableRecord;
                        foreach (ObjectId id in model)
                        {
                            if (id.OpenEntity() is DBPoint)
                            {
                                (id.OpenForWrite() as DBPoint).Erase(true);
                            }
                        }

                        tr.Commit();
                    }
                    catch (Autodesk.AutoCAD.Runtime.Exception ex)
                    {
                        ex.Message.ToEditor();
                        tr.Abort();
                    }
                }
            }
        }

        [CommandMethod("CHECKVERTEX")]
        public void Vertex()
        {
            //Nuevo ObjectId
            ObjectId objid = new ObjectId();

            //Obtengo la entidad que es polilínea
            Met_Autodesk.Entity("Selecciona Polilinea", out objid, typeof(Polyline));

            //Abro entidad y la convierto en Polilinea
            Polyline pl = objid.OpenEntity() as Polyline;

            List<Point3d> listPoints = pl.ClockwisePoints();
            
            for(int i = 0; i < listPoints.Count; i++)
            {
                ////Punto A
                Point3d ptActual = listPoints[i];                

                int idxPtActual = Met_Autodesk.GetPointIndex(pl, ptActual);

                SegmentType seg = pl.GetSegmentType(idxPtActual);

                string.Format("El Seg {0}:{1} en el punto X:{2} y Y:{3}\n",
                    seg.ToString(), i+1, Math.Round(ptActual.X, 2), Math.Round(ptActual.Y, 2)).ToEditor();
            }

        }

        [CommandMethod("APARTMENTS")]
        public void Apartamentos()
        {
            //Nuevo ObjectId
            ObjectId objid = new ObjectId();

            //Obtengo la entidad que es polilínea
            Met_Autodesk.Entity("Selecciona Polilinea", out objid, typeof(Polyline));

            //Abro entidad y la convierto en Polilinea
            Polyline pl = objid.OpenEntity() as Polyline;

            //Obtengo Apartamentos dentro del Lote
            ObjectIdCollection idsApartments
                = Met_Autodesk.ObjectsInside(pl.GeometricExtents.MinPoint,
                                             pl.GeometricExtents.MaxPoint,
                                             typeof(Polyline).Filter(M.Constant.LayerApartamento));                  

            foreach(ObjectId idAP in idsApartments)
            {
                Polyline plAp = idAP.OpenEntity() as Polyline;

                Met_Autodesk.TextInWindow(plAp.GeometricExtents.MinPoint, 
                                          plAp.GeometricExtents.MaxPoint, 
                                          M.Constant.LayerApartamento).ToEditor();
            }
        }
    }
}
