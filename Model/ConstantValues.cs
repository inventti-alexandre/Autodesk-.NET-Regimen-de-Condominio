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
        private static List<string> orientaciones = new List<string>()
        {
           "Norte",
           "Este",
           "Sur",
           "Oeste"
        };


        public static string[] PalabrasOmitiadas
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

        public static List<string> Orientaciones
        {
            get
            {
                return orientaciones;
            }
        }
    }
}
