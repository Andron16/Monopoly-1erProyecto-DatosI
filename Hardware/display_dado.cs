//Andron

using System;
using System.IO.Ports;
using System.Threading;

namespace Monopoly.Hardware
{
    public class DadosFisicos //driver del lado PC: habla con la Pico W por el puerto serial del USB
    {
        private SerialPort puerto; //puerto serial virtual que expone la Pico por USB
        private string nombrePuerto; //nombre del COM, usado en los mensajes
        private int dado1; //ultimo valor recibido del primer dado
        private int dado2; //ultimo valor recibido del segundo dado
        private int total; //ultima suma recibida
        private bool lanzamientoNuevo; //true si llego un resultado que todavia nadie consumio
        private bool conectado; //true mientras el puerto este abierto y el handshake haya pasado
        private object bloqueoDatos; //candado para compartir los valores con el hilo de lectura

        public DadosFisicos(string nombrePuerto = "COM9") //prepara el puerto sin abrirlo todavia
        {
            this.nombrePuerto = nombrePuerto;
            dado1 = 0;
            dado2 = 0;
            total = 0;
            lanzamientoNuevo = false;
            conectado = false;
            bloqueoDatos = new object();

            puerto = new SerialPort(nombrePuerto, 9600, Parity.None, 8, StopBits.One);
            puerto.ReadTimeout = 2000;
            puerto.WriteTimeout = 2000;
            puerto.DtrEnable = true; //sin DTR activo, MicroPython descarta lo que la Pico envia con print()
        }

        public bool Conectar() //abre el puerto y confirma con PING / DADOS:LISTO que main.py esta corriendo
        {
            try
            {
                if (puerto.IsOpen) //si quedo abierto de antes, se reinicia limpio
                {
                    puerto.Close();
                }

                puerto.Open();
                conectado = true;
                puerto.DiscardInBuffer(); //descarta datos viejos que hayan quedado en el buffer
                Thread.Sleep(500);

                puerto.WriteLine("PING");

                string? respuesta = null;
                int intentos = 0;
                while (respuesta == null && intentos < 5) //hasta 5 intentos esperando la respuesta de la Pico
                {
                    Thread.Sleep(300);
                    respuesta = LeerMensaje();
                    intentos++;
                }

                if (respuesta != null && respuesta.Contains("LISTO")) //la Pico respondio el handshake
                {
                    Console.WriteLine("[Dados] Conexion exitosa con la Pico W en " + nombrePuerto);
                    return true;
                }

                Console.WriteLine("[Dados] ERROR: la Pico W no respondio en " + nombrePuerto);
                puerto.Close();
                conectado = false;
                return false;
            }
            catch (Exception ex) //puerto inexistente, ocupado por Thonny, etc.
            {
                Console.WriteLine("[Dados] ERROR al conectar en " + nombrePuerto + ": " + ex.Message);
                conectado = false;
                return false;
            }
        }

        public void Desconectar() //cierra el puerto y detiene el hilo de lectura
        {
            try
            {
                conectado = false; //el hilo de lectura revisa esta bandera y termina
                if (puerto.IsOpen)
                {
                    puerto.Close();
                    Console.WriteLine("[Dados] Desconectado");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[Dados] ERROR al desconectar: " + ex.Message);
            }
        }

        public void IniciarEscucha() //lanza un hilo en segundo plano que procesa todo lo que envie la Pico
        {
            Thread hiloLectura = new Thread(() =>
            {
                while (conectado) //corre hasta que se llame a Desconectar
                {
                    string? mensaje = LeerMensaje();
                    if (mensaje != null)
                    {
                        ProcesarMensaje(mensaje);
                    }
                    Thread.Sleep(100);
                }
            });
            hiloLectura.IsBackground = true; //no impide que el programa se cierre
            hiloLectura.Start();
        }

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

        private string? LeerMensaje() //lee una linea del puerto; null si no hay nada
        {
            try
            {
                if (puerto.IsOpen && puerto.BytesToRead > 0) //solo lee si hay datos esperando
                {
                    return puerto.ReadLine().Trim();
                }
            }
            catch (TimeoutException)
            {
                //timeout normal de lectura, no es un error
            }
            catch (Exception ex)
            {
                Console.WriteLine("[Dados] ERROR en lectura: " + ex.Message);
            }

            return null;
        }

        private void ProcesarMensaje(string mensaje) //interpreta los mensajes con formato PREFIJO:datos
        {
            try
            {
                if (mensaje.StartsWith("DADOS:")) //formato DADOS:dado1,dado2,total
                {
                    string[] partes = mensaje.Substring(6).Split(',');

                    if (partes.Length == 3) //DADOS:LISTO tiene una sola parte y se ignora aqui
                    {
                        lock (bloqueoDatos)
                        {
                            dado1 = int.Parse(partes[0]);
                            dado2 = int.Parse(partes[1]);
                            total = int.Parse(partes[2]);
                            lanzamientoNuevo = true;
                        }
                        Console.WriteLine("[Dados] Lanzados: " + dado1 + " + " + dado2 + " = " + total);
                    }
                }
                else if (mensaje.StartsWith("DISPLAYS:"))
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

        public void EnviarComando(string comando) //envia una linea de texto a la Pico
        {
            try
            {
                if (puerto.IsOpen)
                {
                    puerto.WriteLine(comando);
                }
                else
                {
                    Console.WriteLine("[Dados] ERROR: el puerto no esta abierto");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[Dados] ERROR al enviar comando: " + ex.Message);
            }
        }

        public void MostrarEnDisplays(int valor1, int valor2) { EnviarComando("MOSTRAR:" + valor1 + "," + valor2); } //pide a la Pico mostrar dos valores

        public void ApagarDisplays() { EnviarComando("APAGAR"); } //apaga ambos displays

        public bool EstaConectado() { return conectado && puerto.IsOpen; } //true si el puerto sigue abierto y el handshake paso

        public int ObtenerDado1() { lock (bloqueoDatos) { return dado1; } } //ultimo valor del primer dado

        public int ObtenerDado2() { lock (bloqueoDatos) { return dado2; } } //ultimo valor del segundo dado

        public int ObtenerTotal() { lock (bloqueoDatos) { return total; } } //ultima suma recibida
    }
}