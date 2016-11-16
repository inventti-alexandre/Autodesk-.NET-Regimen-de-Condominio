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
            View.ModuloInicial win = new View.ModuloInicial();
            Autodesk.AutoCAD.ApplicationServices.Core.Application.ShowModelessWindow(win);
        }

    }
}
