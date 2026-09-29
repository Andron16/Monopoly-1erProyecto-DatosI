//abigail

using System;
using System.Windows.Forms;
using Monopoly.Dominio;
using Monopoly.Interfaz;

namespace Monopoly.Pruebas
{
    public class PruebasAbigail //banco de pruebas del tablero y la interfaz de esta integrante
    {
        public static void Ejecutar() //corre todas las pruebas de esta integrante
        {
            ProbarTablero();
        }

        private static void ProbarTablero() //abre la ventana del tablero con jugadores fijos en casillas distintas
        {
            Console.WriteLine("=== VistaTablero ===");

            Application.EnableVisualStyles(); //necesario para que Windows Forms se vea bien
            Application.SetCompatibleTextRenderingDefault(false);

            Tablero tablero = new Tablero();
            tablero.Construir(); //arma las 32 casillas reales

            Jugador[] jugadoresPrueba = new Jugador[4];
            jugadoresPrueba[0] = new Jugador(1, "Rojo", 1500);
            jugadoresPrueba[1] = new Jugador(2, "Azul", 1500);
            jugadoresPrueba[2] = new Jugador(3, "Verde", 1500);
            jugadoresPrueba[3] = new Jugador(4, "Naranja", 1500);

            //los ubico en casillas distintas para ver que cada ficha cae en su lugar
            jugadoresPrueba[0].Posicion = tablero.ObtenerNodo(0);  //Salida
            jugadoresPrueba[1].Posicion = tablero.ObtenerNodo(8);  //Carcel
            jugadoresPrueba[2].Posicion = tablero.ObtenerNodo(16); //lado sur
            jugadoresPrueba[3].Posicion = tablero.ObtenerNodo(24); //lado oeste

            Application.Run(new VistaTablero(jugadoresPrueba)); //bloquea hasta que se cierre la ventana
        }
    }
}