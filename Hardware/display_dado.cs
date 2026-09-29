using System;
using System.Threading;

namespace Monopoly.Hardware
{
    public class DadosFisicos //recibe los dados mediante el puerto compartido
    {
        private readonly PicoSerial pico; //puerto compartido; ya no se abre uno propio aqui
        private int dado1; //ultimo valor recibido del primer dado
        private int dado2; //ultimo valor recibido del segundo dado
        private int total; //ultima suma recibida
        private bool lanzamientoNuevo; //true si llego un resultado que todavia nadie consumio
        private readonly object bloqueoDatos = new object(); //candado para compartir los valores con el hilo de lectura

        public event Action<int, int>? LanzamientoRecibido; //se dispara cada vez que se aprieta el boton fisico

        public DadosFisicos(PicoSerial pico) //se suscribe a los mensajes que reparte el hilo lector de PicoSerial
        {
            this.pico = pico;
            pico.MensajeAsincrono += ProcesarMensaje; //recibe los DADOS:, DISPLAYS: y ERROR: de la Pico
        }

        public bool EstaConectado() { return pico.EstaAbierto(); } //true si el puerto compartido sigue abierto y el handshake paso

        public bool EsperarLanzamiento(int milisegundos, out int valor1, out int valor2) //descarta lanzamientos previos y espera uno nuevo hasta el tiempo limite
        {
            lock (bloqueoDatos)
            {
                lanzamientoNuevo = false; //lo presionado antes de pedir el lanzamiento no cuenta
            }

            int esperado = 0;
            while (esperado < milisegundos) //revisa cada 100 ms si llego un resultado nuevo
            {
                lock (bloqueoDatos)
                {
                    if (lanzamientoNuevo) //llego un lanzamiento despues de la solicitud
                    {
                        lanzamientoNuevo = false;
                        valor1 = dado1;
                        valor2 = dado2;
                        return true;
                    }
                }
                Thread.Sleep(100);
                esperado += 100;
            }

            valor1 = 0;
            valor2 = 0;
            return false;
        }

        private void ProcesarMensaje(string mensaje) //interpreta los mensajes con formato PREFIJO:datos
        {
            try
            {
                if (mensaje.StartsWith("DADOS:")) //formato DADOS:dado1,dado2,total
                {
                    string[] partes = mensaje.Substring(6).Split(',');

                    if (partes.Length == 3) //DADOS:LISTO lo atiende PicoSerial en el handshake y no llega aqui
                    {
                        lock (bloqueoDatos)
                        {
                            dado1 = int.Parse(partes[0]);
                            dado2 = int.Parse(partes[1]);
                            total = int.Parse(partes[2]);
                            lanzamientoNuevo = true;
                        }
                        Console.WriteLine("[Dados] Lanzados: " + dado1 + " + " + dado2 + " = " + total);
                        LanzamientoRecibido?.Invoke(dado1, dado2); //avisa al servidor sin esperar a que nadie pregunte
                    }
                }
                else if (mensaje.StartsWith("DISPLAYS:")) //confirmacion de MOSTRAR o APAGAR
                {
                    Console.WriteLine("[Dados] " + mensaje);
                }
                else if (mensaje.StartsWith("ERROR:"))
                {
                    Console.WriteLine("[Dados] ERROR en la Pico W: " + mensaje);
                }
            }
            catch (Exception ex) //mensaje mal formado
            {
                Console.WriteLine("[Dados] ERROR al procesar '" + mensaje + "': " + ex.Message);
            }
        }

        public void MostrarEnDisplays(int valor1, int valor2) { pico.Enviar("MOSTRAR:" + valor1 + "," + valor2); } //pide a la Pico mostrar dos valores

        public void ApagarDisplays() { pico.Enviar("APAGAR"); } //apaga ambos displays

        public int ObtenerDado1() { lock (bloqueoDatos) { return dado1; } } //ultimo valor del primer dado

        public int ObtenerDado2() { lock (bloqueoDatos) { return dado2; } } //ultimo valor del segundo dado

        public int ObtenerTotal() { lock (bloqueoDatos) { return total; } } //ultima suma recibida
    }
}