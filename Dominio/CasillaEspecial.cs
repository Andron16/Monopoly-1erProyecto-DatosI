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
        Provincia

    }

    public class CasillaEspecial : Casilla //casilla sin dueño con un efecto fijo
    {
        public TipoEspecial Tipo { get; set; } //efecto que aplica la casilla
        public int Monto { get; set; } //dinero o turnos involucrados segun el tipo

        public CasillaEspecial(int id, string nombre, TipoEspecial tipo, int monto) : base(id, nombre) //crea la casilla especial
        {
            Tipo = tipo;
            Monto = monto;
        }

        public override void AlCaer(Jugador jugador, Juego juego) { } //aplica premio, carcel o impuesto segun el tipo
    }
}
