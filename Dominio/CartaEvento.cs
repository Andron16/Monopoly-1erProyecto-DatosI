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

        public void Aplicar(Jugador jugador, Juego juego) { } //ejecuta el efecto sobre el jugador indicado
    }
}