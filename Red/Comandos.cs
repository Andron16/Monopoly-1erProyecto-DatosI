//andron

namespace Monopoly.Red
{
    public static class Comandos //nombres de todos los mensajes del protocolo cliente-servidor
    {
        //cliente -> servidor
        public const string Conectar = "CONECTAR"; //CONECTAR|nombre
        public const string TirarDados = "TIRAR_DADOS"; //TIRAR_DADOS
        public const string ComprarPropiedad = "COMPRAR_PROPIEDAD"; //COMPRAR_PROPIEDAD
        public const string NoComprar = "NO_COMPRAR"; //NO_COMPRAR
        public const string TerminarTurno = "TERMINAR_TURNO"; //TERMINAR_TURNO
        public const string ConsultarEstado = "CONSULTAR_ESTADO"; //CONSULTAR_ESTADO
        public const string ConsultarTransacciones = "CONSULTAR_TRANSACCIONES"; //CONSULTAR_TRANSACCIONES

        //servidor -> cliente
        public const string Bienvenido = "BIENVENIDO"; //BIENVENIDO|idJugador, respuesta a CONECTAR
        public const string Error = "ERROR"; //ERROR|motivo, accion rechazada por el servidor
        public const string Inicio = "INICIO"; //INICIO, la partida arranca con los 4 jugadores
        public const string Turno = "TURNO"; //TURNO|idJugador|nombre
        public const string Dados = "DADOS"; //DADOS|idJugador|dado1|dado2
        public const string Movimiento = "MOVIMIENTO"; //MOVIMIENTO|idJugador|indiceCasilla|nombreCasilla
        public const string OfrecerCompra = "OFRECER_COMPRA"; //OFRECER_COMPRA|nombrePropiedad|precio, solo al jugador en turno
        public const string Estado = "ESTADO"; //ESTADO|..., formato por definir con Interfaz/
        public const string Mensaje = "MENSAJE"; //MENSAJE|texto, aviso informativo para todos
        public const string Transaccion = "TRANSACCION"; //TRANSACCION|linea, una por transaccion al consultar el historial
        public const string Fin = "FIN"; //FIN|nombreGanador
    }
}