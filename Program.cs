//Grupo

using System;

namespace Monopoly
{
    class Program //punto de entrada unico; despacha segun el rol recibido
    {
        [STAThread] //requerido por Windows Forms para abrir la ventana del cliente
        static void Main(string[] args) //decide que rol ejecutar
        {
            string rol = args.Length > 0 ? args[0].ToLower() : ""; //primer argumento: servidor, cliente o pruebas

            switch (rol) //despacha al rol correspondiente
            {
                case "servidor":
                    {
                        string puerto = args.Length > 1 ? args[1] : "COM9"; //segundo argumento opcional: puerto de la Pico
                        Console.WriteLine("Iniciando servidor (modulo fisico en " + puerto + ")...");
                        //new Monopoly.Red.Servidor(puerto).Iniciar(5000);
                        break;
                    }

                case "cliente":
                    Console.WriteLine("Iniciando cliente...");
                    //ApplicationConfiguration.Initialize();
                    //System.Windows.Forms.Application.Run(new Monopoly.Interfaz.VistaTablero());
                    break;

                case "pruebas":
                    EjecutarPruebas();
                    break;

                default:
                    MostrarAyuda();
                    break;
            }
        }

        static void EjecutarPruebas() //corre el banco de pruebas de cada integrante
        {
            Console.WriteLine("===== MODULO DE PRUEBAS =====");
            Monopoly.Pruebas.PruebasAndron.Ejecutar();
            Monopoly.Pruebas.PruebasAbigail.Ejecutar();
            Monopoly.Pruebas.PruebasPalma.Ejecutar();
        }

        static void MostrarAyuda() //muestra como se usa el programa
        {
            Console.WriteLine("Uso: dotnet run -- [rol]");
            Console.WriteLine();
            Console.WriteLine("Roles disponibles:");
            Console.WriteLine("  dotnet run -- servidor [COMx] : inicia el servidor (COMx = puerto de la Pico, opcional)");
            Console.WriteLine("  dotnet run -- cliente         : inicia el cliente del juego");
            Console.WriteLine("  dotnet run -- pruebas         : ejecuta las pruebas de cada integrante");
        }
    }
}