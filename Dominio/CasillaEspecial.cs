//palma

namespace Monopoly.Dominio
{
    public enum TipoEspecial //variantes de casilla especial del tablero
    {
        Salida, //otorga el premio por pasar o caer en ella
        Carcel, //hace perder 1 turno al jugador
        Regalo, //Regala dinero a un jugador aleatorio
        Dinero, //Regala una cantidad de dinero al jugador
        Loteria, //Regala 7000 de dinero al usuario
        Provincia //casilla de esquina sin efecto
    }

    public class CasillaEspecial : Casilla //casilla sin dueno con un efecto fijo
    {
        private const int PremioLoteria = 7000; //monto fijo que entrega la loteria

        public TipoEspecial Tipo { get; set; } //efecto que aplica la casilla
        public int Monto { get; set; } //dinero o turnos involucrados segun el tipo

        public CasillaEspecial(int id, string nombre, TipoEspecial tipo, int monto) : base(id, nombre) //crea la casilla especial
        {
            Tipo = tipo;
            Monto = monto;
        }

        public override void AlCaer(Jugador jugador, Juego juego) //aplica el efecto segun el tipo de casilla
        {
            switch (Tipo) //cada tipo de casilla especial hace algo distinto
            {
                case TipoEspecial.Salida: //el premio de la salida lo maneja la logica de turno (PasoPorSalida); no se paga aqui para no duplicarlo
                    break;

                case TipoEspecial.Carcel: //hace perder un turno al jugador
                    jugador.TurnosPerdidos += 1;
                    break;

                case TipoEspecial.Dinero: //el banco le regala Monto al jugador
                    juego.Banco.Pagar(jugador, Monto, juego.NumeroTurno, TipoTransaccion.GananciaPorEvento, "Casilla " + Nombre);
                    break;

                case TipoEspecial.Loteria: //premio fijo de loteria
                    juego.Banco.Pagar(jugador, PremioLoteria, juego.NumeroTurno, TipoTransaccion.GananciaPorEvento, "Loteria");
                    break;

                case TipoEspecial.Regalo: //version simple: le regala Monto al jugador actual (la version aleatoria queda pendiente de coordinar)
                    juego.Banco.Pagar(jugador, Monto, juego.NumeroTurno, TipoTransaccion.GananciaPorEvento, "Regalo");
                    break;

                case TipoEspecial.Provincia: //casilla de esquina sin efecto
                    break;
            }
        }
    }
}