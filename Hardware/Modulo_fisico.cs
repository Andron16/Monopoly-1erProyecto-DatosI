using System;

namespace Monopoly.Hardware
{
    public class ModuloFisico : IDisposable //agrupa el puerto de la Pico y los dispositivos que viajan por el: dados y lector RFID
    {
        private PicoSerial pico; //puerto compartido; solo esta clase lo abre y lo cierra

        public DadosFisicos Dados { get; private set; } //boton, displays y lanzamientos
        public LectorRfid Lector { get; private set; } //lector de tarjetas LED 

        public ModuloFisico(string puertoSerial = "COM4") //prepara todo sin abrir el puerto todavia
        {
            pico = new PicoSerial(puertoSerial);
            Dados = new DadosFisicos(pico);
            Lector = new LectorRfid(pico);
        }

        public bool Conectar() { return pico.Abrir(); } //abre el puerto y hace el handshake, false si la Pico no responde

        public bool EstaConectado() { return pico.EstaAbierto(); } //true si la Pico sigue respondiendo

        public void Desconectar() { pico.Dispose(); } //cierra el puerto compartido

        public void Dispose() { pico.Dispose(); }
    }
}