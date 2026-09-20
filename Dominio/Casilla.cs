//palma

namespace Monopoly.Dominio
{
    public abstract class Casilla //clase base de toda casilla del tablero
    {
        public int Id { get; set; } //numero de casilla dentro del tablero
        public string Nombre { get; set; } //nombre visible de la casilla

        public Casilla(int id, string nombre) //inicializa los datos comunes a toda casilla
        {
            Id = id;
            Nombre = nombre;
        }

        public abstract void AlCaer(Jugador jugador, Juego juego); //comportamiento propio de cada tipo de casilla
    }
}