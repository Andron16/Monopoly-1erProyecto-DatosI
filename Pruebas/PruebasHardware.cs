//Hardware

using System;
using Monopoly.Hardware;

namespace Monopoly.Pruebas
{
    public class PruebasHardware //banco de pruebas de la Pico: dados y lector RFID por el mismo puerto
    {
        private const string PuertoPico = "COM4"; //cambiar segun el COM de la PC

        public static void Ejecutar() //abre la Pico y deja probar el boton y el lector a la vez
        {
            Console.WriteLine("=== Hardware (dados + RFID) ===");

            ModuloFisico modulo = new ModuloFisico(PuertoPico);
            if (!modulo.Conectar())
            {
                Console.WriteLine("No se pudo conectar (cierra Thonny, reconecta el USB y revisa el COM).");
                return;
            }

            modulo.Dados.LanzamientoRecibido += MostrarLanzamiento; //cada vez que se aprieta el boton fisico

            Console.WriteLine("Aprieta el boton de la Pico cuando quieras (se espera ver los dos dados).");
            Console.WriteLine("Enter = leer llavero, SALIR para terminar...");

            while (true) //repite hasta que se escriba SALIR
            {
                string entrada = (Console.ReadLine() ?? "").Trim().ToUpper();
                if (entrada == "SALIR")
                {
                    break;
                }

                Console.WriteLine("Acerque el llavero...");
                string? id = modulo.Lector.EsperarTarjeta();
                Console.WriteLine(id == null ? "No se leyo ninguna tarjeta." : "ID recibido: " + id); //se espera el UID en hexadecimal

                Console.WriteLine("Enter = leer llavero, SALIR para terminar...");
            }

            modulo.Desconectar();
        }

        private static void MostrarLanzamiento(int d1, int d2) //imprime el resultado del boton fisico
        {
            Console.WriteLine("Dados: " + d1 + " + " + d2 + " = " + (d1 + d2)); //se espera un valor de 1 a 6 en cada dado
        }
    }
}