//abigail

using System;
using Monopoly.Dominio;

namespace Monopoly.Pruebas
{
    public class PruebasAbigail //banco de pruebas de Abigail
    {
        private const string Puerto = "COM9"; //cambiar segun el COM que Windows le asigne a la Pico

        public static void Ejecutar() //corre todas las pruebas de esta integrante
        {
            Console.WriteLine("=== Dado ===");
            PruebaModoSoftware();
            PruebaModoHardware();
        }

        private static void PruebaModoSoftware() //cinco lanzamientos sin hardware
        {
            Console.WriteLine("--- Modo software ---");
            Dado dado = new Dado(usarHardware: false);
            Console.WriteLine("Es modo fisico: " + dado.EsModoFisico()); //se espera False

            for (int i = 1; i <= 5; i++) //cinco lanzamientos seguidos
            {
                dado.Lanzar();
                Console.WriteLine("  Lanzamiento " + i + ": " + dado.Obtener1() + " + " + dado.Obtener2() + " = " + dado.ObtenerTotal()); //cada dado entre 1 y 6, total entre 2 y 12
            }
        }

        private static void PruebaModoHardware() //un lanzamiento real con el boton de la Pico
        {
            Console.WriteLine("--- Modo hardware ---");
            Dado dado = new Dado(usarHardware: true, puertoSerial: Puerto);

            if (!dado.EsModoFisico()) //sin Pico conectada no hay nada que probar
            {
                Console.WriteLine("  Hardware no disponible, se omite la prueba.");
                return;
            }

            Console.WriteLine("  Presiona el boton de la Pico (hay 15 segundos)...");
            dado.Lanzar();
            Console.WriteLine("  Resultado: " + dado.Obtener1() + " + " + dado.Obtener2() + " = " + dado.ObtenerTotal()); //debe coincidir con los displays
            dado.Desconectar();
        }
    }
}