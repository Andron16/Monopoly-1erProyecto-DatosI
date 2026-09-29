//para generar los reportes de las transacciones

using System;
using System.IO;
using Monopoly.Dominio;
using Monopoly.Estructuras;

namespace Monopoly.Reportes
{
    public class ReporteTransacciones //genera y exporta el historial de transacciones en texto
    {
        public static string GenerarTexto(ListaDoble<Transaccion> historial, bool masRecientePrimero = false) //arma el reporte recorriendo el historial en el sentido pedido
        {
            string salto = Environment.NewLine; //salto de linea correcto para Windows
            string texto = "REPORTE DE TRANSACCIONES - MONOPOLY" + salto;
            texto += "Generado: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + salto;
            texto += "Total de transacciones: " + historial.Contar() + salto;
            texto += "Orden: " + (masRecientePrimero ? "de la mas reciente a la mas antigua" : "de la mas antigua a la mas reciente") + salto + salto;
            texto += Transaccion.Encabezado() + salto;
            texto += new string('-', 100) + salto;

            NodoDoble<Transaccion>? actual = masRecientePrimero ? historial.Ultimo() : historial.Primero(); //extremo desde donde arranca el recorrido
            while (actual != null) //recorre la lista doble en el sentido elegido
            {
                texto += actual.Dato.ATexto() + salto;
                actual = masRecientePrimero ? actual.Anterior : actual.Siguiente; //hacia atras o hacia adelante
            }

            return texto;
        }

        public static bool Exportar(ListaDoble<Transaccion> historial, string ruta) //escribe el reporte en un archivo TXT; false si no se pudo
        {
            try
            {
                string? carpeta = Path.GetDirectoryName(ruta);
                if (!string.IsNullOrEmpty(carpeta)) //crea la carpeta si todavia no existe
                {
                    Directory.CreateDirectory(carpeta);
                }

                File.WriteAllText(ruta, GenerarTexto(historial)); //crea o reemplaza el archivo
                return true;
            }
            catch (Exception ex) //sin permisos, archivo abierto en otro programa, etc.
            {
                Console.WriteLine("[Reporte] No se pudo exportar: " + ex.Message);
                return false;
            }
        }
    }
}