using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RegimenCondominio.Model
{
     public static class InfoTabla
    {
        private static ObservableCollection<M.Medidas> _medidasGlobales = new ObservableCollection<M.Medidas>();

        private static ObservableCollection<M.Medidas> _medidasActuales = new ObservableCollection<M.Medidas>();

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

        public static ObservableCollection<M.Medidas> MedidasActuales
        {
            get
            {
                return _medidasActuales;
            }

            set
            {
                _medidasActuales = value;
            }
        }
    }
}
