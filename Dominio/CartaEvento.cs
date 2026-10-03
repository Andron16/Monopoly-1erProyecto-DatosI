//palma

namespace Monopoly.Dominio
{
    public enum TipoEfecto //efectos posibles de una carta de evento
    {
        RecibirDinero, //el banco le paga al jugador
        PagarDinero, //el jugador le paga al banco
        Avanzar, //mueve al jugador hacia adelante
        Retroceder, //mueve al jugador hacia atras
        PerderTurno, //el jugador se salta su proximo turno
        IrACasilla //envia al jugador a una casilla determinada
    }

    public class CartaEvento //carta del mazo con un efecto sobre el jugador
    {
        public int Id { get; set; } //identificador de la carta
        public string Descripcion { get; set; } //texto que se muestra al jugador
        public TipoEfecto Efecto { get; set; } //accion que ejecuta la carta
        public int Valor { get; set; } //monto, cantidad de pasos o numero de casilla

        public CartaEvento(int id, string descripcion, TipoEfecto efecto, int valor) //crea una carta del mazo
        {
            Id = id;
            Descripcion = descripcion;
            Efecto = efecto;
            Valor = valor;
        }

        public void Aplicar(Jugador jugador, Juego juego) //ejecuta el efecto sobre el jugador indicado
        {
            switch (Efecto) //cada tipo de carta hace algo distinto
            {
                case TipoEfecto.RecibirDinero: //el banco le paga al jugador
                    juego.Banco.Pagar(jugador, Valor, juego.NumeroTurno, TipoTransaccion.GananciaPorEvento, Descripcion);
                    break;

                case TipoEfecto.PagarDinero: //el jugador le paga al banco
                    juego.Banco.Cobrar(jugador, Valor, juego.NumeroTurno, TipoTransaccion.PerdidaPorEvento, Descripcion);
                    break;

                case TipoEfecto.Avanzar: //mueve la ficha adelante Valor pasos
                    if (jugador.Posicion != null) //solo si la ficha ya esta en el tablero
                    {
                        jugador.Posicion = juego.Tablero.Mover(jugador.Posicion, Valor);
                    }
                    break;

                case TipoEfecto.Retroceder: //mueve la ficha hacia atras Valor pasos usando los enlaces Anterior
                    if (jugador.Posicion != null)
                    {
                        jugador.Posicion = juego.Tablero.Retroceder(jugador.Posicion, Valor);
                    }
                    break;

                case TipoEfecto.PerderTurno: //se salta Valor turnos
                    jugador.TurnosPerdidos += Valor;
                    break;

                case TipoEfecto.IrACasilla: //lo envia directo a la casilla numero Valor
                    jugador.Posicion = juego.Tablero.ObtenerNodo(Valor);
                    break;
            }
        }
    }
}