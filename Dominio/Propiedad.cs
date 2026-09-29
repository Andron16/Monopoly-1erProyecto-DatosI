//palma

namespace Monopoly.Dominio
{
    public class Propiedad : Casilla //casilla comprable que cobra alquiler a terceros
    {
        public int Precio { get; set; } //costo de compra de la propiedad
        public int Alquiler { get; set; } //monto que cobra al dueño de otro jugador
        public Jugador? Propietario { get; set; } //null mientras no tenga dueño

        public Propiedad(int id, string nombre, int precio, int alquiler) : base(id, nombre) //crea la propiedad sin dueño
        {
            Precio = precio;
            Alquiler = alquiler;
            Propietario = null;
        }

        public bool EstaDisponible() { return Propietario == null; } //true si todavia nadie la compro

        public bool EsDe(Jugador jugador) //true si el jugador es el propietario actual
        {
            return Propietario == jugador;
        }

        public override void AlCaer(Jugador jugador, Juego juego) //ofrece compra, cobra alquiler o no hace nada
        {
            if (EstaDisponible()) //libre: la compra la decide el jugador con su comando; el servidor la ofrece
            {
                return; //no hay cambio de estado automatico
            }
            if (EsDe(jugador)) //propia: no se cobra nada
            {
                return;
            }
            //ajena: el visitante le paga alquiler al dueño, siempre a traves del banco
            juego.Banco.CobrarAlquiler(jugador, this, juego.NumeroTurno);
        }
    }
}