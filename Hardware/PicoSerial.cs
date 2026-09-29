using System;
using System.IO.Ports;
using System.Threading;

namespace Monopoly.Hardware
{
    public class PicoSerial : IDisposable //unico dueño del COM de la Pico; un hilo lee todo y reparte los mensajes
    {
        private readonly SerialPort puerto; //puerto compartido
        private readonly ManualResetEventSlim hayRespuesta = new ManualResetEventSlim(false); //se activa cuando llega la respuesta a un comando
        private readonly ManualResetEventSlim listo = new ManualResetEventSlim(false); //se activa cuando llega DADOS:LISTO (handshake)
        private readonly object candadoComando = new object(); //evita que dos comandos con respuesta se crucen
        private string? respuesta; //ultima respuesta recibida a un comando (UID, DARK, OK, ...)
        private volatile bool activo; //true mientras el hilo lector debe seguir corriendo
        private Thread? hilo; //hilo lector en segundo plano

        public event Action<string>? MensajeAsincrono; //mensajes que la Pico manda sola: DADOS:, DISPLAYS:, ERROR:

        public PicoSerial(string nombrePuerto = "COM9") //prepara el puerto sin abrirlo todavia
        {
            puerto = new SerialPort(nombrePuerto, 115200);
            puerto.NewLine = "\n";
            puerto.ReadTimeout = 500; //timeout corto para que el hilo pueda terminar
            puerto.WriteTimeout = 2000;
            puerto.DtrEnable = true; //sin DTR, MicroPython descarta lo que la Pico envia con print()
        }

        public bool EstaAbierto() { return activo && puerto.IsOpen; } //true si el puerto sigue abierto y el handshake paso

        public bool Abrir() //abre el puerto, arranca el hilo lector y confirma con PING / DADOS:LISTO
        {
            try
            {
                puerto.Open();
                puerto.DiscardInBuffer(); //descarta datos viejos del buffer
                activo = true;
                hilo = new Thread(BucleLectura);
                hilo.IsBackground = true; //no impide que el programa se cierre
                hilo.Start();

                for (int i = 0; i < 5 && !listo.IsSet; i++) //hasta 5 intentos de handshake
                {
                    puerto.WriteLine("PING");
                    listo.Wait(600);
                }

                if (listo.IsSet) //la Pico respondio
                {
                    Console.WriteLine("[Pico] Conexion exitosa en " + puerto.PortName);
                    return true;
                }

                Console.WriteLine("[Pico] ERROR: la Pico no respondio en " + puerto.PortName);
                Dispose();
                return false;
            }
            catch (Exception ex) //puerto inexistente, ocupado por Thonny, etc.
            {
                Console.WriteLine("[Pico] ERROR al abrir " + puerto.PortName + ": " + ex.Message);
                return false;
            }
        }

        private void BucleLectura() //lee lineas del puerto hasta que se cierre
        {
            while (activo)
            {
                try
                {
                    string linea = puerto.ReadLine().Trim();
                    if (linea.Length > 0)
                    {
                        Clasificar(linea);
                    }
                }
                catch (TimeoutException)
                {
                    //timeout normal, se vuelve a intentar
                }
                catch (Exception ex) //puerto desconectado o cerrado
                {
                    if (activo)
                    {
                        Console.WriteLine("[Pico] ERROR en lectura: " + ex.Message);
                    }
                    activo = false;
                }
            }
        }

        private void Clasificar(string linea) //decide a quien le toca cada linea recibida
        {
            if (linea == "DADOS:LISTO") //respuesta al PING
            {
                listo.Set();
            }
            else if (linea.StartsWith("DADOS:") || linea.StartsWith("DISPLAYS:") || linea.StartsWith("ERROR:")) //mensajes de los dados
            {
                MensajeAsincrono?.Invoke(linea);
            }
            else 
            {
                respuesta = linea;
                hayRespuesta.Set();
            }
        }

        public string? Solicitar(string comando, int timeoutMs) //envia un comando y espera su respuesta, null si no llega a tiempo
        {
            lock (candadoComando)
            {
                hayRespuesta.Reset(); //descarta cualquier respuesta vieja
                respuesta = null;
                puerto.WriteLine(comando);

                if (hayRespuesta.Wait(timeoutMs))
                {
                    return respuesta;
                }
                return null;
            }
        }

        public void Enviar(string comando) //envia un comando sin esperar respuesta 
        {
            lock (candadoComando)
            {
                if (puerto.IsOpen)
                {
                    puerto.WriteLine(comando);
                }
            }
        }

        public void Dispose() //detiene el hilo lector y cierra el puerto
        {
            activo = false;
            try
            {
                if (puerto.IsOpen)
                {
                    puerto.Close();
                }
            }
            catch (Exception)
            {
                //ya estaba cerrado
            }
        }
    }
}