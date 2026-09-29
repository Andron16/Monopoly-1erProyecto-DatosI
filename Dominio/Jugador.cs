//palma

using Monopoly.Estructuras;

namespace Monopoly.Dominio
{
    public class Jugador //representa a un participante de la partida
    {
        public int Id { get; set; } //identificador unico del jugador
        public string Nombre { get; set; } //nombre visible en la interfaz
        public int Saldo { get; private set; } //dinero disponible; solo el Banco lo modifica
        public NodoDoble<Casilla>? Posicion { get; set; } //nodo del tablero donde se encuentra
        public bool Activo { get; set; } //false cuando queda eliminado de la partida
        public ListaSimple<Propiedad> Propiedades { get; set; } //terrenos adquiridos
        public string IdRfid { get; set; } //identificador de su tarjeta RFID
        public int TurnosPerdidos { get; set; } //turnos que debe saltarse por un evento
        public bool YaTiroDados { get; set; } //evita lanzar dos veces en el mismo turno

        public Jugador(int id, string nombre, int saldoInicial) //crea un jugador activo sin propiedades
        {
            Id = id;
            Nombre = nombre;
            Saldo = saldoInicial;
            Posicion = null;
            Activo = true;
            Propiedades = new ListaSimple<Propiedad>();
            IdRfid = "";
            TurnosPerdidos = 0;
            YaTiroDados = false;
        }

        public void AjustarSaldo(int monto) //suma o resta al saldo; monto negativo para cobros
        {
            Saldo = Saldo + monto; //unico punto que modifica el saldo
        }

        public bool PuedePagar(int monto) //indica si el saldo alcanza para cubrir el pago
        {
            return Saldo >= monto;
        }

        public void AgregarPropiedad(Propiedad propiedad) //registra un terreno recien comprado
        {
            Propiedades.Agregar(propiedad); //lo guarda en su lista de propiedades
        }

        public int CalcularPatrimonio() //saldo mas el valor de todas sus propiedades
        {
            int total = Saldo; //arranca con el dinero en mano
            int i = 0;
            while (i < Propiedades.Contar()) //recorre cada propiedad por indice, sin foreach
            {
                Propiedad propiedad = Propiedades.Obtener(i); //propiedad en la posicion i
                total = total + propiedad.Precio; //suma su precio al patrimonio
                i++;
            }
            return total;
        }
    }
}