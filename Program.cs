//Grupo

using System;

namespace Monopoly
{
    class Program //punto de entrada unico; despacha segun el rol recibido
    {
        private const int PuertoTcp = 5000; //puerto de red donde escucha el servidor
        private const int LimiteTurnos = 100; //turnos individuales antes de terminar por patrimonio

        [STAThread] //requerido por Windows Forms para abrir la ventana del cliente
        static void Main(string[] args) //decide que rol ejecutar
        {
            string rol = args.Length > 0 ? args[0].ToLower() : ""; //primer argumento: el rol a ejecutar

            switch (rol) //despacha al rol correspondiente
            {
                case "servidor":
                    {
                        bool usarPico = args.Length > 1; //la Pico solo se usa si se indica su COM
                        string puerto = usarPico ? args[1] : "COM9";
                        Console.WriteLine(usarPico ? "Iniciando servidor con la Pico en " + puerto + "..." : "Iniciando servidor con dados por software...");
                        Monopoly.Dominio.Juego juego = new Monopoly.Dominio.Juego(LimiteTurnos, usarPico, puerto);
                        new Monopoly.Red.Servidor(PuertoTcp, juego).Iniciar();
                        break;
                    }

                case "cliente":
                    Console.WriteLine("Iniciando cliente...");
                    //ApplicationConfiguration.Initialize();
                    //System.Windows.Forms.Application.Run(new Monopoly.Interfaz.VistaTablero());
                    break;

                case "consola":
                    {
                        string ip = args.Length > 1 ? args[1] : "127.0.0.1"; //IP del servidor; 127.0.0.1 es esta misma computadora
                        string nombre = args.Length > 2 ? args[2] : "Jugador"; //nombre con que se presenta
                        ClienteConsola(ip, nombre);
                        break;
                    }

                case "pruebas":
                    EjecutarPruebas();
                    break;

                case "hardware":
                    Monopoly.Pruebas.PruebasHardware.Ejecutar(); //prueba interactiva: requiere la Pico conectada
                    break;

                case "tablero":
                    Monopoly.Pruebas.PruebasAbigail.ProbarTableroVisual(); //prueba visual: abre la ventana
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

        static void ClienteConsola(string ip, string nombre) //cliente de texto para probar la red sin interfaz grafica
        {
            Monopoly.Red.Cliente cliente = new Monopoly.Red.Cliente();
            cliente.AlRecibir = mensaje => Console.WriteLine("<- " + mensaje); //cada mensaje del servidor se imprime
            if (!cliente.Conectar(ip, PuertoTcp, nombre)) //sin conexion no hay nada que hacer
            {
                return;
            }

            Console.WriteLine("Conectado. Escriba un comando (ej. TIRAR_DADOS) o SALIR.");
            string? linea = Console.ReadLine();
            while (linea != null && linea.ToUpper() != "SALIR") //envia todo lo que el usuario escriba
            {
                cliente.Enviar(linea.ToUpper());
                linea = Console.ReadLine();
            }
            cliente.Desconectar();
        }

        static void MostrarAyuda() //muestra como se usa el programa
        {
            Console.WriteLine("Uso: dotnet run -- [rol]");
            Console.WriteLine();
            Console.WriteLine("Roles disponibles:");
            Console.WriteLine("  dotnet run -- servidor [COMx]         : inicia el servidor (COMx = puerto de la Pico, opcional)");
            Console.WriteLine("  dotnet run -- cliente                 : inicia el cliente grafico del juego");
            Console.WriteLine("  dotnet run -- consola [IP] [nombre]   : cliente de texto para probar la red");
            Console.WriteLine("  dotnet run -- pruebas                 : ejecuta las pruebas de cada integrante");
            Console.WriteLine("  dotnet run -- hardware                : prueba interactiva de la Pico (dados + RFID)");
            Console.WriteLine("  dotnet run -- tablero                 : prueba visual del tablero");
        }
    }
}