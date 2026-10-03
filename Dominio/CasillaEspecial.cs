//palma

namespace Monopoly.Dominio
{
    public enum TipoEspecial //variantes de casilla especial del tablero
    {
        Salida, //otorga el premio por pasar o caer en ella
        Carcel, //hace perder 1 turno al jugador y cobra una fianza (PagoAlBanco)
        Regalo, //el jugador le regala dinero a otro jugador aleatorio (PagoEntreJugadores)
        Dinero, //Regala una cantidad de dinero al jugador
        Loteria, //premio fijo de dinero al jugador
        Provincia //casilla de esquina sin efecto
    }

    public class CasillaEspecial : Casilla //casilla sin dueno con un efecto fijo
    {
        private const int PremioLoteria = 300; //monto fijo que entrega la loteria (antes 7000: decidia la partida)

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

                case TipoEspecial.Carcel: //hace perder un turno al jugador y cobra la fianza
                    jugador.TurnosPerdidos += 1;
                    if (Monto > 0) //Monto es la fianza; se paga al banco
                    {
                        juego.Banco.Cobrar(jugador, Monto, juego.NumeroTurno, TipoTransaccion.PagoAlBanco, "Fianza de la carcel");
                    }
                    break;

                case TipoEspecial.Dinero: //el banco le regala Monto al jugador
                    juego.Banco.Pagar(jugador, Monto, juego.NumeroTurno, TipoTransaccion.GananciaPorEvento, "Casilla " + Nombre);
                    break;

                case TipoEspecial.Loteria: //premio fijo de loteria
                    juego.Banco.Pagar(jugador, PremioLoteria, juego.NumeroTurno, TipoTransaccion.GananciaPorEvento, "Loteria");
                    break;

                case TipoEspecial.Regalo: //el jugador le regala Monto a otro jugador activo elegido al azar
                    {
                        Jugador? receptor = juego.OtroJugadorAlAzar(jugador);
                        if (receptor != null) //si es el unico activo no hay a quien regalar
                        {
                            juego.Banco.Transferir(jugador, receptor, Monto, juego.NumeroTurno, TipoTransaccion.PagoEntreJugadores, "Regalo a " + receptor.Nombre);
                        }
                    }
                    break;

                case TipoEspecial.Provincia: //casilla de esquina sin efecto
                    break;
            }
        }
    }
}