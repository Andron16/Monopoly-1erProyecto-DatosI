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

        public bool Cobrar(Jugador jugador, int monto, int turno, TipoTransaccion tipo, string descripcion) //el jugador le paga al banco; pago obligatorio
        {
            if (!jugador.PuedePagar(monto)) //no le alcanza: se aplica la regla de eliminacion
            {
                EliminarJugador(jugador);
                return false;
            }

            jugador.AjustarSaldo(-monto); //monto negativo: resta del saldo
            Registrar(tipo, jugador.Nombre, NombreBanco, monto, turno, descripcion); //el banco es el destino del dinero
            return true;
        }

        public void Pagar(Jugador jugador, int monto, int turno, TipoTransaccion tipo, string descripcion) //el banco le paga al jugador
        {
            jugador.AjustarSaldo(monto); //monto positivo: suma al saldo
            Registrar(tipo, NombreBanco, jugador.Nombre, monto, turno, descripcion); //el banco es el origen del dinero
        }

        public bool Transferir(Jugador origen, Jugador destino, int monto, int turno, TipoTransaccion tipo, string descripcion) //mueve dinero entre dos jugadores; pago obligatorio
        {
            if (!origen.PuedePagar(monto)) //no le alcanza: se aplica la regla de eliminacion
            {
                EliminarJugador(origen);
                return false;
            }

            origen.AjustarSaldo(-monto); //le resta al que paga
            destino.AjustarSaldo(monto); //le suma al que cobra
            Registrar(tipo, origen.Nombre, destino.Nombre, monto, turno, descripcion);
            return true;
        }

        public bool ComprarPropiedad(Jugador jugador, Propiedad propiedad, int turno) //compra voluntaria: si no alcanza, no pasa nada
        {
            if (!propiedad.EstaDisponible() || !jugador.PuedePagar(propiedad.Precio)) //ya tiene dueño o no le alcanza
            {
                return false;
            }

            jugador.AjustarSaldo(-propiedad.Precio); //paga el precio al banco
            propiedad.Propietario = jugador; //pasa a ser el dueño
            jugador.AgregarPropiedad(propiedad); //queda en su lista de propiedades
            Registrar(TipoTransaccion.CompraPropiedad, jugador.Nombre, NombreBanco, propiedad.Precio, turno, "Compra de " + propiedad.Nombre);
            return true;
        }

        public bool CobrarAlquiler(Jugador visitante, Propiedad propiedad, int turno) //cobra al visitante y le paga al propietario
        {
            Jugador? propietario = propiedad.Propietario;
            if (propietario == null || propietario == visitante) //sin dueño o es suya: no se paga nada
            {
                return true;
            }

            return Transferir(visitante, propietario, propiedad.Alquiler, turno, TipoTransaccion.PagoAlquiler, "Alquiler de " + propiedad.Nombre);
        }

        public void PremioPorSalida(Jugador jugador, int monto, int turno) //paga el premio por pasar por la casilla de salida
        {
            Pagar(jugador, monto, turno, TipoTransaccion.PremioPorInicio, "Premio por pasar por la salida");
        }

        public void EliminarJugador(Jugador jugador) //marca al jugador como inactivo y libera sus propiedades
        {
            jugador.Activo = false; //queda fuera de la partida

            for (int i = 0; i < jugador.Propiedades.Contar(); i++) //recorre sus propiedades por indice
            {
                jugador.Propiedades.Obtener(i).Propietario = null; //vuelve a estar disponible para comprar
            }
        }

        private void Registrar(TipoTransaccion tipo, string origen, string destino, int monto, int turno, string descripcion) //crea la transaccion y la archiva en el historial
        {
            Transaccion transaccion = new Transaccion(siguienteId, turno, tipo, origen, destino, monto, descripcion); //sella la operacion con el siguiente numero
            historial.Agregar(transaccion); //queda como la mas reciente del historial
            siguienteId++; //la proxima transaccion lleva el numero siguiente
        }
    }
}
