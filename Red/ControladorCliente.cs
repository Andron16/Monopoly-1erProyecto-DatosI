//andron

using System;

namespace Monopoly.Red
{
    public class ControladorCliente //logica del lado del jugador: conecta, interpreta mensajes y envia comandos; no conoce ventanas
    {
        private Cliente cliente; //conexion TCP; solo envia y recibe texto (regla 2.5)
        public int MiId { get; private set; } //numero que asigna el servidor; 0 hasta recibir BIENVENIDO

        //avisos para la interfaz: se ejecutan en el HILO DE RED, la interfaz debe usar Invoke (regla 14.1)
        public Action<int, string, int, int, bool>? AlActualizarJugador { get; set; } //id, nombre, saldo, casilla, activo
        public Action<string>? AlCambiarTurno { get; set; } //"Es tu turno", "Turno de X" o fin de partida
        public Action<string>? AlRegistrar { get; set; } //texto legible de cada mensaje del servidor

        public ControladorCliente() //prepara el controlador sin conectar
        {
            cliente = new Cliente();
            cliente.AlRecibir = ProcesarMensaje; //todo lo que llega del servidor pasa por aqui
            MiId = 0;
        }

        public bool Conectar(string ip, int puerto, string nombre) { return cliente.Conectar(ip, puerto, nombre); } //se presenta con CONECTAR; false si no hay servidor

        public void Desconectar() { cliente.Desconectar(); } //cierra la conexion

        //acciones del jugador: solo envian el comando; el servidor decide si se puede
        public void TirarDados() { cliente.Enviar(Comandos.TirarDados); }
        public void Comprar() { cliente.Enviar(Comandos.ComprarPropiedad); }
        public void NoComprar() { cliente.Enviar(Comandos.NoComprar); }
        public void TerminarTurno() { cliente.Enviar(Comandos.TerminarTurno); }
        public void PedirEstado() { cliente.Enviar(Comandos.ConsultarEstado); }
        public void PedirTransacciones() { cliente.Enviar(Comandos.ConsultarTransacciones); }

        private void ProcesarMensaje(string mensaje) //interpreta cada mensaje del servidor y avisa a la interfaz
        {
            string[] p = Protocolo.Separar(mensaje);
            string c = p[0];

            if (c == Comandos.EstadoJugador && p.Length >= 6) //JUGADOR|id|nombre|saldo|casilla|activo
            {
                if (AlActualizarJugador != null) AlActualizarJugador(Numero(p[1]), p[2], Numero(p[3]), Numero(p[4]), p[5] == "1");
                return; //no va al registro: se ve en el tablero
            }
            if (c == Comandos.Bienvenido && p.Length >= 2) MiId = Numero(p[1]); //guarda quien soy
            if (c == Comandos.Turno && p.Length >= 3) Avisar(AlCambiarTurno, Numero(p[1]) == MiId ? "Es tu turno" : "Turno de " + p[2]);
            if (c == Comandos.Fin && p.Length >= 2) Avisar(AlCambiarTurno, "Partida terminada. Gano " + p[1]);

            Avisar(AlRegistrar, Traducir(p));
        }

        private string Traducir(string[] p) //convierte un mensaje del protocolo en texto legible
        {
            string c = p[0];
            if (c == Comandos.Dados && p.Length >= 4) return "Jugador " + p[1] + " saco " + p[2] + " y " + p[3];
            if (c == Comandos.Movimiento && p.Length >= 4) return "Jugador " + p[1] + " cayo en " + p[3] + " (casilla " + p[2] + ")";
            if (c == Comandos.OfrecerCompra && p.Length >= 3) return "Podes comprar " + p[1] + " por " + p[2];
            if (c == Comandos.Turno && p.Length >= 3) return "Turno de " + p[2];
            if (c == Comandos.Error && p.Length >= 2) return "ERROR: " + p[1];
            if (c == Comandos.Mensaje && p.Length >= 2) return p[1];
            if (c == Comandos.Transaccion && p.Length >= 2) return p[1];
            if (c == Comandos.Bienvenido && p.Length >= 2) return "Conectado como jugador " + p[1];
            if (c == Comandos.Inicio) return "La partida empezo";
            if (c == Comandos.Fin && p.Length >= 2) return "FIN DE LA PARTIDA. Gano " + p[1];
            return string.Join("|", p); //cualquier otro mensaje se muestra tal cual
        }

        private void Avisar(Action<string>? aviso, string texto) { if (aviso != null) aviso(texto); } //llama al aviso solo si la interfaz lo engancho

        private static int Numero(string texto) { int valor; return int.TryParse(texto, out valor) ? valor : 0; } //texto a numero; 0 si no es valido
    }
}