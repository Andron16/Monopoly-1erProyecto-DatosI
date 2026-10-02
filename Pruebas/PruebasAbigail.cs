//abigail

using System;
using System.Windows.Forms;
using Monopoly.Interfaz;
using Monopoly.Estructuras;

namespace Monopoly.Pruebas
{
    public class PruebasAbigail //banco de pruebas del tablero y la interfaz de esta integrante
    {
        public static void Ejecutar() //pruebas que NO esperan nada del usuario; la prueba visual vive aparte
        {
            ProbarMenuJugador();
        }

        private static void ProbarColaCircular() //verifica cola vacia, un elemento, Avanzar y los 4 casos de Eliminar
        {
            Console.WriteLine("=== ColaCircular ===");

            ColaCircular<string> cola = new ColaCircular<string>();
            Console.WriteLine("Vacia al inicio: " + cola.EstaVacia()); //se espera True
            Console.WriteLine("Cantidad inicial: " + cola.Contar()); //se espera 0

            cola.Encolar("J1");
            Console.WriteLine("Un elemento - Frente: " + cola.Frente() + ", Cantidad: " + cola.Contar()); //se espera J1, 1

            cola.Avanzar(); //con un solo elemento, avanzar no debe cambiar nada
            Console.WriteLine("Avanzar con un solo elemento - Frente sigue siendo: " + cola.Frente()); //se espera J1

            cola.Encolar("J2");
            cola.Encolar("J3");
            cola.Encolar("J4");
            Console.WriteLine("Cantidad tras encolar 4: " + cola.Contar()); //se espera 4

            Console.Write("Avanzar 4 veces, vuelve al primero: ");
            int i = 0;
            while (i < 4) //un giro completo del circulo
            {
                Console.Write(cola.Frente() + " ");
                cola.Avanzar();
                i++;
            }
            Console.WriteLine(); //se espera J1 J2 J3 J4
            Console.WriteLine("Frente tras el giro completo: " + cola.Frente()); //se espera J1, volvio al inicio

            Console.WriteLine("Eliminar J1 (frente): " + cola.Eliminar("J1")); //se espera True
            Console.WriteLine("Frente ahora: " + cola.Frente() + ", Cantidad: " + cola.Contar()); //se espera J2, 3

            Console.WriteLine("Eliminar J3 (medio): " + cola.Eliminar("J3")); //se espera True
            Console.WriteLine("Cantidad: " + cola.Contar()); //se espera 2

            Console.WriteLine("Eliminar J4 (ultimo): " + cola.Eliminar("J4")); //se espera True
            Console.WriteLine("Frente: " + cola.Frente() + ", Cantidad: " + cola.Contar()); //se espera J2, 1

            Console.WriteLine("Eliminar J2 (el unico): " + cola.Eliminar("J2")); //se espera True
            Console.WriteLine("Vacia al final: " + cola.EstaVacia()); //se espera True

            Console.WriteLine("Eliminar en cola vacia: " + cola.Eliminar("J5")); //se espera False, no debe tronar
        }

        public static void ProbarTableroVisual() //abre la ventana; va en su propio rol porque bloquea
        {
            Console.WriteLine("=== VistaTablero ===");

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            VistaTablero vista = new VistaTablero(); //ya no recibe Jugador[]; el cliente real tampoco los tiene

            //simula los mensajes JUGADOR|id|nombre|saldo|casilla|activo que mandaria el servidor
            vista.ActualizarJugador(1, "Ana", 1500, 0, true);
            vista.ActualizarJugador(2, "Beto", 1500, 9, true);
            vista.ActualizarJugador(3, "Carla", 1500, 16, true);
            vista.ActualizarJugador(4, "Dani", 1500, 31, true);

            Application.Run(vista); //bloquea hasta que se cierre la ventana
        }

        private static void ProbarMenuJugador()
        {
            Console.WriteLine("=== MenuJugador ===");

            MenuJugador menu = new MenuJugador();
            menu.Show(); //muestra la ventana

            //simula actualizaciones de jugadores desde otro hilo (como si vinieran del servidor)
            System.Threading.Thread thread = new System.Threading.Thread(() =>
            {
                System.Threading.Thread.Sleep(2000);
                menu.ActualizarJugador(1, "Andron");
                System.Threading.Thread.Sleep(1000);
                menu.ActualizarJugador(2, "Palma");
                System.Threading.Thread.Sleep(1000);
                menu.ActualizarJugador(3, "Abigail");
                System.Threading.Thread.Sleep(1000);
                menu.ActualizarJugador(4, "Celeste");
                System.Threading.Thread.Sleep(1000);
                menu.ConexionConfirmada();
            });
            thread.IsBackground = true;
            thread.Start();

            System.Windows.Forms.Application.Run(menu); //abre la ventana y espera que la cierres manualmente
        }
    }
}