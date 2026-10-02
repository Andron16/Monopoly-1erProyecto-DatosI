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
                    {
                        string ip = args.Length > 1 ? args[1] : "127.0.0.1"; //IP del servidor
                        string nombre = args.Length > 2 ? args[2] : "Jugador"; //nombre con que se presenta
                        ClienteGrafico(ip, nombre);
                        break;
                    }

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
        static void ClienteGrafico(string ip, string nombre) //ventana del tablero conectada al servidor por medio del ControladorCliente
        {
            System.Windows.Forms.Application.EnableVisualStyles();
            Monopoly.Interfaz.VistaTablero vista = new Monopoly.Interfaz.VistaTablero();
            Monopoly.Red.ControladorCliente controlador = new Monopoly.Red.ControladorCliente();

            controlador.AlActualizarJugador = vista.ActualizarJugador; //mueve fichas y saldos; ActualizarJugador ya hace Invoke
            controlador.AlCambiarTurno = texto => Console.WriteLine(">> " + texto); //TEMPORAL: a la terminal hasta que la ventana lo muestre
            controlador.AlRegistrar = texto => Console.WriteLine(texto); //TEMPORAL: igual

            vista.Shown += (s, e) => //se conecta recien con la ventana visible, asi Invoke ya funciona
            {
                if (!controlador.Conectar(ip, PuertoTcp, nombre)) Console.WriteLine("No se pudo conectar a " + ip);
            };

            vista.FormClosed += (s, e) => //al cerrar la ventana se corta la conexion
            {
                Console.WriteLine("[Cliente] Ventana cerrada");
                controlador.Desconectar();
            };

            System.Threading.Thread entrada = new System.Threading.Thread(() => LeerComandos(controlador)); //TEMPORAL: comandos por teclado hasta que existan los botones
            entrada.IsBackground = true;
            entrada.Start();

            System.Windows.Forms.Application.Run(vista); //bloquea hasta cerrar la ventana
        }

        static void LeerComandos(Monopoly.Red.ControladorCliente controlador) //TEMPORAL: t, c, n, f, h, e desde la terminal
        {
            Console.WriteLine("Comandos: t = tirar, c = comprar, n = no comprar, f = terminar turno, h = transacciones, e = estado");
            string? linea = Console.ReadLine();
            while (linea != null) //hasta que se cierre la terminal
            {
                string tecla = linea.Trim().ToLower();
                if (tecla == "t") controlador.TirarDados();
                else if (tecla == "c") controlador.Comprar();
                else if (tecla == "n") controlador.NoComprar();
                else if (tecla == "f") controlador.TerminarTurno();
                else if (tecla == "h") controlador.PedirTransacciones();
                else if (tecla == "e") controlador.PedirEstado();
                linea = Console.ReadLine();
            }
        }

        static void MostrarAyuda() //muestra como se usa el programa
        {
            Console.WriteLine("Uso: dotnet run -- [rol]");
            Console.WriteLine();
            Console.WriteLine("Roles disponibles:");
            Console.WriteLine("  dotnet run -- servidor [COMx]         : inicia el servidor (COMx = puerto de la Pico, opcional)");
            Console.WriteLine("  dotnet run -- cliente [IP] [nombre]   : ventana del tablero conectada al servidor");
            Console.WriteLine("  dotnet run -- consola [IP] [nombre]   : cliente de texto para probar la red");
            Console.WriteLine("  dotnet run -- pruebas                 : ejecuta las pruebas de cada integrante");
            Console.WriteLine("  dotnet run -- hardware                : prueba interactiva de la Pico (dados + RFID)");
            Console.WriteLine("  dotnet run -- tablero                 : prueba visual del tablero");
        }
    }
}