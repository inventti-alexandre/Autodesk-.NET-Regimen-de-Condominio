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
        [CommandMethod("REGIMEN")]
        public void IniciaReg()
        {
            V.ModuloManzana win = new V.ModuloManzana();
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
                        ent.GetType().Name, data[0], data[1]).ToAutodeskEditor();
                }
            }
            else
                string.Format("No hay ids con record: {0}", M.Constant.XRecordColindancia)
                    .ToAutodeskEditor();
        }
    }
}
