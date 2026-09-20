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

        public void AjustarSaldo(int monto) { } //suma o resta al saldo; monto negativo para cobros

        public bool PuedePagar(int monto) { return false; } //indica si el saldo alcanza para cubrir el pago

        public void AgregarPropiedad(Propiedad propiedad) { } //registra un terreno recien comprado

        public int CalcularPatrimonio() { return 0; } //saldo mas el valor de todas sus propiedades
    }
}