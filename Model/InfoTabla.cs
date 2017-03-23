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
        private static ObservableCollection<M.Medidas> _medidasGlobales = new ObservableCollection<M.Medidas>();

        private static ObservableCollection<M.Checked<LoteItem>> lotesItem 
                                                        = new ObservableCollection<M.Checked<LoteItem>>();

        private static TotalesMedidas totalesTabla = new TotalesMedidas();

        private static Totales totalCubPB = new Totales() { Columna = DetailColumns.CPlantaBaja};

        private static List<M.DataColumns> allProperties = new List<DataColumns>();

        private static ManzanaData rumboInverso = new ManzanaData();

        private static string calleFrente = "";

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

        private static Dictionary<string, string> descripcionPropiedades = new Dictionary<string, string>();

        private static Dictionary<string, bool> visibilidadPropiedades = new Dictionary<string, bool>();
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

        public static ObservableCollection<M.Checked<LoteItem>> LotesItem
        {
            get
            {
                return lotesItem;
            }

            set
            {
                lotesItem = value;
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

        public static string CalleFrente
        {
            get
            {
                return calleFrente;
            }

            set
            {
                calleFrente = value;
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

        public static Totales TotalCubPB
        {
            get
            {
                return totalCubPB;
            }

            set
            {
                totalCubPB = value;
            }
        }
    }
}
