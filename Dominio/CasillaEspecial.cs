//palma

namespace Monopoly.Dominio
{
    public enum TipoEspecial //variantes de casilla especial del tablero
    {
        Salida, //otorga el premio por pasar o caer en ella
        Carcel, //hace perder turnos al jugador
        Impuesto, //cobra un monto fijo al banco
        Descanso //no produce ningun efecto
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
