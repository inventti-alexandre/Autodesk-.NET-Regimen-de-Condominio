using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;

namespace RegimenCondominio.C
{
    public static class Met_Manzana
    {
        /// <summary>
        /// Obtengo la lista de acuerdo a Rumbo de Frente
        /// </summary>
        /// <param name="RumboFrente">Rumbo de Frente de Manzana</param>
        /// <returns>Lista de Rumbo</returns>
        public static List<string> OrientacionFrente(string RumboFrente)
        {
            List<string> OrientacionesFinales = new List<string>();

            int PrimerDimension = ObtengoPosicion(RumboFrente, 0);

            //Obtengo tipo de orientación (0 o 1)
            int TipoOrientacion = (M.Constant.Orientaciones[PrimerDimension, 1]).Count();

            List<string> l = Obtengolista(TipoOrientacion);

            return CalculaRutaOrientacion(l, RumboFrente);
        }

        /// <summary>
        /// Itero hasta que encuentro la posición dentro del valor de Arreglo de 2 dimensiones
        /// </summary>
        /// <param name="ValorComparar">Valor a encontrar dentro del array</param>
        /// <param name="DimensionEstatica">Dimension del arreglo no cambiante</param>
        /// <returns></returns>
        public static int ObtengoPosicion(string ValorComparar, int DimensionEstatica)
        {
            string ValorActual = "";
            int PrimerDimension = 0;

            do
            {
                ValorActual = M.Constant.Orientaciones[PrimerDimension, DimensionEstatica];

                if (PrimerDimension + 1 <= M.Constant.Orientaciones.GetLength(DimensionEstatica))
                    PrimerDimension++;
            }
            while (ValorComparar != ValorActual);

            return PrimerDimension - 1;
        }

        public static int Elimina(this List<M.ManzanaData> listado, M.ManzanaData itemBuscar)
        {
            int RowsDeleted = 0;
            try
            {
                //Revisa que ya se haya insertado esa polilinea
                RowsDeleted = 
                    listado.RemoveAll(x=> x.hndPlColindancia == itemBuscar.hndPlColindancia);   
                
                //En dado caso que no encuentre Handle, busca si se repite alguna colindancia.
                if(RowsDeleted == 0)
                {
                    for(int i = listado.Count -1; i == 0; i--)
                    {
                        if(listado[i].rumboActual == itemBuscar.rumboActual)
                        {
                            listado.RemoveAt(i);
                            RowsDeleted = 1;
                        }
                    }
                }             
            }
            catch (Exception ex)
            {
                ex.Message.ToEditor();
            }

            return RowsDeleted;
        }

        private static List<string> CalculaRutaOrientacion(List<string> l, string rumboFrente)
        {
            //Reviso la posición dentro de la variable
            int index = l.IndexOf(rumboFrente);

            List<string> listaOrdenada = new List<string>();

            //Realizo array hacia adelante
            for (int i = index; i < l.Count; i++)
            {
                listaOrdenada.Add(l[i]);
            }

            //Realizo array hacia atras
            for (int j = 0; j < index; j++)
            {
                listaOrdenada.Add(l[j]);
            }

            return listaOrdenada;
        }

        private static List<string> Obtengolista(int tipoOrientacion)
        {
            List<string> ListaOrienta = new List<string>();

            for (int i = 0; i < M.Constant.Orientaciones.GetLength(0); i++)
            {
                if (M.Constant.Orientaciones[i, 1].Count() == tipoOrientacion)
                {
                    ListaOrienta.Add(M.Constant.Orientaciones[i, 0]);
                }
            }

            return ListaOrienta;
        }

        internal static List<string> DespliegoOrientaciones()
        {
            List<string> todasOrientaciones = new List<string>();

            for (int i = 0; i < M.Constant.Orientaciones.GetLength(0); i++)
            {
                todasOrientaciones.Add(M.Constant.Orientaciones[i, 0]);
            }

            return todasOrientaciones;
        }

        internal static int InsertoColindancia(this M.ManzanaData insertedData)
        {
            int sigPosicion = -1;
            //Inserto en la polilinea
            if (Met_Autodesk.InsertDictionary(insertedData.hndPlColindancia.toObjectId(),
                                            M.Constant.XRecordColindancia,
                                            insertedData.rumboActual,
                                            insertedData.textColindancia))
            {
                //Encapsulo en lista de colindancia
                M.Manzana.ColindanciaManzana.Add(insertedData);

                //Obtengo la siguiente posición de orientación de rumbo del listado
                sigPosicion = M.Manzana.OrientacionCalculada.LastIndexOf(insertedData.rumboActual) + 1;
            }

            return sigPosicion;
        }

        internal static int ReasignoColindancia(this M.ManzanaData insertedData, bool PolilineaNueva, bool RumboNuevo)
        {            

            //Si ya existe Polilinea en la lista con Rumbo Nuevo
            if (!PolilineaNueva && RumboNuevo)
            {                
                //Elimino de la lista
                M.Manzana.ColindanciaManzana.
                    RemoveAll(x => x.hndPlColindancia.Value == insertedData.hndPlColindancia.Value);
            }
            //Si es Nueva Polilinea en la lista con Rumbo ya insertado
            else if (PolilineaNueva && !RumboNuevo)
            {
                //Buscar Polilinea de Rumbo anterior y eliminarlo---------------
                //Polilinea repetida
                M.ManzanaData itemRepetido = new M.ManzanaData();

                itemRepetido = M.Manzana.ColindanciaManzana.
                    Where(x => x.rumboActual == insertedData.rumboActual).FirstOrDefault();

                Entity entPl = itemRepetido.hndPlColindancia.toObjectId().OpenEntity();                

                DManager.RemoveXRecord(entPl.ExtensionDictionary, M.Constant.XRecordColindancia);
                //----------------------------------------------------------------
                
                //Eliminar de lista Rumbo                
                M.Manzana.ColindanciaManzana.
                    RemoveAll(x => x.rumboActual == insertedData.rumboActual);
            }
            //Si es Polilinea y Rumbo ya insertados en la lista
            else if (!PolilineaNueva && !RumboNuevo)
            {
                M.ManzanaData itemRumbo = new M.ManzanaData();

                //Calculo item de Polilinea--------------------------------------------------------
                //Elimino de la lista
                M.Manzana.ColindanciaManzana.
                    RemoveAll(x => x.hndPlColindancia.Value == insertedData.hndPlColindancia.Value);            
                //----------------------------------------------------------------------------------

                //Calculo item de Rumbo-------------------------------------------------------------                
                itemRumbo = M.Manzana.ColindanciaManzana.
                    Where(x => x.rumboActual == insertedData.rumboActual).FirstOrDefault();

                if (itemRumbo != null)
                {
                    ObjectId idPl = new ObjectId();

                    idPl = itemRumbo.hndPlColindancia.toObjectId();

                    Entity entPl = idPl.OpenEntity();

                    DManager.RemoveXRecord(entPl.ExtensionDictionary, M.Constant.XRecordColindancia);

                    //Eliminar de lista Rumbo                
                    M.Manzana.ColindanciaManzana.
                        RemoveAll(x => x.rumboActual == insertedData.rumboActual);
                    //----------------------------------------------------------------------------------
                }
            }

            return insertedData.InsertoColindancia();
        }
    }
}
