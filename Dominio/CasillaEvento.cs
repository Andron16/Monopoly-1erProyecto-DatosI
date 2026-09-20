//palma

namespace Monopoly.Dominio
{
    public class CasillaEvento : Casilla //casilla que obliga a tomar una carta del mazo
    {
        public CasillaEvento(int id, string nombre) : base(id, nombre) { } //crea la casilla de evento

        public override void AlCaer(Jugador jugador, Juego juego) { } //saca una carta del mazo y aplica su efecto
    }
}