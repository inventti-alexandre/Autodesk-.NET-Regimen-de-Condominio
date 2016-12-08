using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RegimenCondominio.M
{
    public class ConstantValues
    {
        //Palabras que no se envían a mayuscula en Método C.Met_Inicio.FormateString
        private static string[] palabrasOmitidas = new string[]{
                "DE",
                "DEL",
                "LA",
                "LAS",
                "LOS",
                "EL"
            };

        //Usuario a Mostrar obtenido de sesión de Windows
        private static string usuario = Environment.UserName.ToUpper();

        //Orientaciones disponibles en Módulo Manzana
        private static string[,] orientaciones = new string[,]
        {
           { "Norte",   "N"},//0,0 - 0,1 -
           { "Este",   "E"},//1,0 - 1,1
           { "Sur",  "S"},//2,0 - 2,1
           { "Oeste",  "O"},//3,0 - 3,1
           { "Noreste", "NE" },//4,0 - 4,1
           { "Sureste", "SE" },//5,0 - 5,1
           { "Suroeste", "SO"},//6,0 - 6,1
           { "Noroeste", "NO" },//7,0 - 7,1
        };


        private static List<string> tipoColindancias = new List<string>()
        {
            "Calle",
            "Otro..."
        };
        //Layer Manzana
        private static string layerManzana = "MANZANA";


        public static string[] PalabrasOmitidas
        {
            get
            {
                return palabrasOmitidas;
            }
        }

        public static string Usuario
        {
            get { return usuario; }
        }

        public static string[,] Orientaciones
        {
            get
            {
                return orientaciones;
            }
        }

        public static string LayerManzana
        {
            get
            {
                return layerManzana;
            }
        }

        public static List<string> TipoColindancias
        {
            get
            {
                return tipoColindancias;
            }

            set
            {
                tipoColindancias = value;
            }
        }
    }
}
