//andron

using Monopoly.Estructuras;

namespace Monopoly.Dominio
{
    public class Banco //arbitro economico; unico punto que modifica saldos
    {
        private ListaDoble<Transaccion> historial; //donde se archiva cada operacion
        private int siguienteId; //consecutivo para numerar las transacciones

        public const string NombreBanco = "BANCO"; //etiqueta usada como origen o destino

        public Banco(ListaDoble<Transaccion> historial) //recibe el historial compartido de la partida
        {
            this.historial = historial;
            siguienteId = 1;
        }

        public bool Cobrar(Jugador jugador, int monto, int turno, TipoTransaccion tipo, string descripcion) { return false; } //el jugador le paga al banco

        public void Pagar(Jugador jugador, int monto, int turno, TipoTransaccion tipo, string descripcion) { } //el banco le paga al jugador

        public bool Transferir(Jugador origen, Jugador destino, int monto, int turno, TipoTransaccion tipo, string descripcion) { return false; } //mueve dinero entre dos jugadores

        public bool ComprarPropiedad(Jugador jugador, Propiedad propiedad, int turno) { return false; } //valida saldo, asigna dueño y registra la compra

        public bool CobrarAlquiler(Jugador visitante, Propiedad propiedad, int turno) { return false; } //cobra al visitante y le paga al propietario

        public void PremioPorSalida(Jugador jugador, int monto, int turno) { } //paga el premio por pasar por la casilla de salida

        public void EliminarJugador(Jugador jugador) { } //marca al jugador como inactivo y libera sus propiedades

        private void Registrar(TipoTransaccion tipo, string origen, string destino, int monto, int turno, string descripcion) { } //crea la transaccion y la archiva en el historial
    }
}
