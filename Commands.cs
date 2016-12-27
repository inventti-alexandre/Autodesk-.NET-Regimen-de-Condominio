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

namespace RegimenCondominio
{
    public class Commands
    {
        [CommandMethod("IniciaReg")]
        public void IniciaReg()
        {

            V.ModuloColindante win = new V.ModuloColindante();
            Autodesk.AutoCAD.ApplicationServices.Core.Application.ShowModalWindow(win);
        }

    }
}
