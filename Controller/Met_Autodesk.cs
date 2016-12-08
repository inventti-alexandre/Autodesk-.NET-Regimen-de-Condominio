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
    public class Met_Autodesk
    {
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

        public static Point3dCollection ExtraerVertices(ObjectId plId)
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

        public static ObjectIdCollection TomarEntidadLayer(string layerName, Point3dCollection pts, out string msjerror)
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
                    ent = AbrirEntidad(objid) as Entity;
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

        public static Entity AbrirEntidad(ObjectId entId)
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
    }
}
