//palma

namespace Monopoly.Dominio
{
    public class CasillaEvento : Casilla //casilla que obliga a tomar una carta del mazo
    {
        public CasillaEvento(int id, string nombre) : base(id, nombre) { } //crea la casilla de evento

        public override void AlCaer(Jugador jugador, Juego juego) //saca una carta del mazo y aplica su efecto
        {
            if (juego.Mazo.EstaVacia()) //sin cartas no hay nada que aplicar
            {
                return;
            }
            CartaEvento carta = juego.Mazo.Sacar(); //toma la carta del frente (y la recicla al final)
            carta.Aplicar(jugador, juego); //ejecuta el efecto sobre el jugador
        }
    }
}