using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RegimenCondominio.C
{
    public class Met_Autodesk
    {
        public static bool Entity(String msg, out ObjectId id, params Type[] tps)
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
    }
}
