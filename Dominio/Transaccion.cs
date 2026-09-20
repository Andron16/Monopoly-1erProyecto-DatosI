//andron

using System;

namespace Monopoly.Dominio
{
    public enum TipoTransaccion //categorias de operacion economica registrables
    {
        CompraPropiedad, //un jugador compra un terreno al banco
        PagoAlquiler, //un jugador le paga a otro por caer en su propiedad
        PagoAlBanco, //impuesto u otro cobro del banco
        PagoEntreJugadores, //transferencia directa entre dos jugadores
        GananciaPorEvento, //la carta le da dinero al jugador
        PerdidaPorEvento, //la carta le quita dinero al jugador
        PremioPorInicio //pago por pasar por la casilla de salida
    }

    public class Transaccion //registro inmutable de una operacion economica
    {
        public int Id { get; } //numero consecutivo de la transaccion
        public DateTime FechaHora { get; } //momento exacto en que ocurrio
        public int Turno { get; } //numero de turno de la partida
        public TipoTransaccion Tipo { get; } //categoria de la operacion
        public string Origen { get; } //quien paga; puede ser el banco
        public string Destino { get; } //quien recibe; puede ser el banco
        public int Monto { get; } //cantidad de dinero involucrada
        public string Descripcion { get; } //detalle legible de la operacion

        public Transaccion(int id, int turno, TipoTransaccion tipo, string origen, string destino, int monto, string descripcion) //crea y sella la transaccion
        {
            Id = id;
            FechaHora = DateTime.Now;
            Turno = turno;
            Tipo = tipo;
            Origen = origen;
            Destino = destino;
            Monto = monto;
            Descripcion = descripcion;
        }

        public string ATexto() { return ""; } //linea formateada para el reporte TXT
    }
}