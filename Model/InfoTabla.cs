using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RegimenCondominio.M
{
     public static class InfoTabla
    {

        #region Catalogos

        private static Dictionary<string, string> diccionarioRumboInverso = new Dictionary<string, string>()
        {
            {"Norte", "Sur" },
            {"Sur", "Norte" },
            {"Este", "Oeste" },
            {"Oeste", "Este" },
            {"Noreste", "Suroeste" },
            {"Suroeste", "Noreste" },
            {"Sureste", "Noroeste" },
            {"Noroeste", "Sureste" }
        };

        #endregion


        private static ObservableCollection<M.Medidas> _medidasGlobales = new ObservableCollection<M.Medidas>();

        private static ObservableCollection<M.Checked<LoteItem>> lotesSelected
                                                        = new ObservableCollection<M.Checked<LoteItem>>();

        private static TotalesMedidas totalesTabla = new TotalesMedidas();        

        private static List<DataColumns> allProperties = new List<DataColumns>();

        private static ManzanaData rumboInverso = new ManzanaData();       

        private static Dictionary<long, bool> lotesBase = new Dictionary<long, bool>();               

        private static Dictionary<string, string> descripcionPropiedades = new Dictionary<string, string>();

        private static Dictionary<string, bool> visibilidadPropiedades = new Dictionary<string, bool>();

        private static List<Bloques> resultadoBloques = new List<Bloques>();

        private static List<Variables> resultadoVariables = new List<Variables>();

        private static List<Bloques> bloquesCalculados = new List<Bloques>();

        public static ObservableCollection<M.Medidas> MedidasGlobales
        {
            get
            {
                return _medidasGlobales;
            }

            set
            {
                _medidasGlobales = value;
            }
        }

        public static Dictionary<string, string> DiccionarioRumboInverso
        {
            get
            {                
                return diccionarioRumboInverso;
            }
        }

        public static ObservableCollection<M.Checked<LoteItem>> LotesSelected
        {
            get
            {
                return lotesSelected;
            }

            set
            {
                lotesSelected = value;
            }
        }

        public static ManzanaData RumboInverso
        {
            get
            {
                return rumboInverso;
            }

            set
            {
                rumboInverso = value;
            }
        }       

               

        public static List<DataColumns> AllProperties
        {
            get
            {
                return allProperties;
            }

            set
            {
                allProperties = value;
            }
        }

        public static TotalesMedidas TotalesTabla
        {
            get
            {
                return totalesTabla;
            }

            set
            {
                totalesTabla = value;
            }
        }

        public static Dictionary<string, string> DescripcionPropiedades
        {
            get
            {
                descripcionPropiedades.Clear();

                foreach (DataColumns dtCol in AllProperties)
                    descripcionPropiedades.Add(dtCol.PropertyName, dtCol.Descripcion);

                return descripcionPropiedades;
            }
        }

        public static Dictionary<string, bool> VisibilidadPropiedades
        {
            get
            {
                visibilidadPropiedades.Clear();

                foreach (DataColumns dtCol in AllProperties)
                    visibilidadPropiedades.Add(dtCol.PropertyName, dtCol.esVisible);

                return visibilidadPropiedades;
            }
        }

        public static Dictionary<long, bool> LotesBase
        {
            get
            {
                return lotesBase;
            }

            set
            {
                lotesBase = value;
            }
        }

        public static List<Bloques> ResultadoBloques
        {
            get
            {
                return resultadoBloques;
            }

            set
            {
                resultadoBloques = value;
            }
        }

        public static List<Variables> ResultadoVariables
        {
            get
            {
                return resultadoVariables;
            }

            set
            {
                resultadoVariables = value;
            }
        }

        public static List<Bloques> BloquesCalculados
        {
            get
            {
                return bloquesCalculados;
            }

            set
            {
                bloquesCalculados = value;
            }
        }
    }
}
