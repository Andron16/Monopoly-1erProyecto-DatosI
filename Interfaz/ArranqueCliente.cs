//andron

using System;
using System.Windows.Forms;
using Monopoly.Red;

namespace Monopoly.Interfaz
{
    public static class ArranqueCliente //arma el lado del jugador: lobby, tablero, panel de acciones y conexion con el servidor
    {
        public const int PuertoTcp = 5000; //mismo puerto que usa el servidor

        public static void Iniciar() //lobby para conectarse; con los 4 listos pasa al tablero con todos los controles
        {
            Application.EnableVisualStyles();
            ControladorCliente controlador = new ControladorCliente(); //red y logica del cliente
            MenuJugador menu = new MenuJugador(); //lobby (Abigail)
            VistaTablero vista = new VistaTablero(); //tablero (Abigail), oculto hasta iniciar
            PanelAcciones panel = new PanelAcciones(controlador); //tirar, terminar turno, transacciones y registro (Andron)

            vista.Controls.Add(panel);
            panel.SendToBack(); //se acomoda primero abajo; el tablero y la columna derecha llenan el resto
            vista.Height += panel.Height; //agranda la ventana para que el tablero no pierda espacio
            _ = vista.Handle; //crea la ventana por dentro sin mostrarla, asi sus Invoke funcionan desde el lobby

            //del servidor hacia las ventanas: llegan en el hilo de red y cada metodo destino ya usa Invoke
            controlador.AlConectado = id => menu.ConexionConfirmada();
            controlador.AlActualizarJugador = (id, nombre, saldo, casilla, activo) =>
            {
                menu.ActualizarJugador(id, nombre); //lista del lobby
                vista.ActualizarJugador(id, nombre, saldo, casilla, activo); //fichas y saldos del tablero
            };
            controlador.AlActualizarCasilla = vista.ActualizarCasilla; //panel de la ultima casilla visitada
            controlador.AlCambiarTurno = panel.MostrarTurno; //"Es tu turno" / "Turno de X"
            controlador.AlRegistrar = panel.Registrar; //mensajes legibles en el registro

            //de las ventanas hacia el servidor
            vista.AlComprar += controlador.Comprar; //boton Comprar del panel de casilla
            vista.AlPasar += controlador.NoComprar; //boton Pasar del panel de casilla

            menu.AlConectar += (ip, nombre) => //el jugador apreto Conectar en el lobby
            {
                if (controlador.Conectar(ip, PuertoTcp, nombre)) vista.Text = "Monopoly Tico - " + nombre;
                else menu.ConexionFallida(); //servidor apagado o IP equivocada: deja reintentar
            };
            menu.AlIniciar += () => //con los 4 listos: se oculta el lobby y se muestra el tablero
            {
                menu.Hide();
                vista.Show();
            };

            vista.FormClosed += (s, e) => //al cerrar el tablero se corta la conexion y termina el programa
            {
                controlador.Desconectar();
                menu.Close();
            };
            menu.FormClosed += (s, e) => controlador.Desconectar(); //si se cierra el lobby antes de jugar

            Application.Run(menu); //el programa vive mientras exista el lobby, aunque este oculto
        }
    }
}