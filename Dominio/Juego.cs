//andron

using Monopoly.Estructuras;

namespace Monopoly.Dominio
{
    public class Juego //estado completo y oficial de la partida; vive solo en el servidor
    {
        public Tablero Tablero { get; private set; } //casillas enlazadas del tablero
        public ColaCircular<Jugador> Turnos { get; private set; } //rotacion de turnos de los jugadores
        public ColaCartas<CartaEvento> Mazo { get; private set; } //cartas de evento reutilizables
        public ListaDoble<Transaccion> Historial { get; private set; } //todas las transacciones de la partida
        public Banco Banco { get; private set; } //arbitro economico
        public Dado Dado { get; private set; } //par de dados de la partida

        public int NumeroTurno { get; private set; } //turno actual, empieza en 1
        public int LimiteTurnos { get; set; } //tope configurable para terminar la partida
        public bool Terminado { get; private set; } //true cuando ya hay un ganador

        public Juego(int limiteTurnos) //arma el estado inicial vacio de la partida
        {
            Tablero = new Tablero();
            Turnos = new ColaCircular<Jugador>();
            Mazo = new ColaCartas<CartaEvento>();
            Historial = new ListaDoble<Transaccion>();
            Banco = new Banco(Historial);
            Dado = new Dado();
            NumeroTurno = 1;
            LimiteTurnos = limiteTurnos;
            Terminado = false;
        }

        public void Iniciar() { } //construye tablero y mazo, y coloca a los jugadores en la salida

        public bool AgregarJugador(Jugador jugador) { return false; } //encola un jugador antes de iniciar la partida

        public Jugador JugadorActual() { return default!; } //jugador que tiene el turno en este momento

        public bool EsSuTurno(int idJugador) { return false; } //valida que el jugador pueda actuar ahora

        public bool TirarDados(int idJugador) { return false; } //lanza, mueve y aplica el efecto de la casilla

        public bool ComprarPropiedadActual(int idJugador) { return false; } //compra la propiedad donde esta parado el jugador

        public void TerminarTurno(int idJugador) { } //avanza la cola al siguiente jugador activo

        public ListaSimple<Transaccion> BuscarPorJugador(string nombre) { return default!; } //transacciones donde el jugador es origen o destino

        public ListaSimple<Transaccion> BuscarPorTipo(TipoTransaccion tipo) { return default!; } //transacciones de una categoria determinada

        public Jugador? Ganador() { return null; } //ultimo activo o el de mayor patrimonio al agotar los turnos

        public bool VerificarFin() { return false; } //revisa si ya se cumplio una condicion de fin de partida
    }
}