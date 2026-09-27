// Hardware/Dados.cs - Control de dados físicos vía Raspberry Pi Pico W
// Proyecto: Monopoly Distribuido - CE1103 ITCR

using System;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;

namespace Hardware
{
    public class DadosFisicos
    {
        private SerialPort puerto;
        private string puerto_nombre;
        private int dado1;
        private int dado2;
        private int total;
        private bool conectado;
        private object bloqueo_datos; //para thread-safety

        public DadosFisicos(string nombrePuerto = "COM9")
        {
            puerto_nombre = nombrePuerto;
            dado1 = 0;
            dado2 = 0;
            total = 0;
            conectado = false;
            bloqueo_datos = new object();

            // inicializar puerto serial
            puerto = new SerialPort(nombrePuerto, 9600, Parity.None, 8, StopBits.One)
            {
                ReadTimeout = 2000,
                WriteTimeout = 2000
            };
        }

        public bool Conectar()
        {
            //intenta abrir conexión con la Pico W
            try
            {
                if (puerto.IsOpen)
                    puerto.Close();

                puerto.Open();
                conectado = true;

                // limpia cualquier dato viejo que haya quedado en el buffer
                puerto.DiscardInBuffer();
                Thread.Sleep(500);

                // envia una pregunta y espera la respuesta (handshake)
                puerto.WriteLine("PING");

                string respuesta = null;
                int intentos = 0;
                while (respuesta == null && intentos < 5)
                {
                    Thread.Sleep(300);
                    respuesta = LeerMensaje();
                    intentos++;
                }

                if (respuesta != null && respuesta.Contains("LISTO"))
                {
                    Console.WriteLine("[Dados] Conexión exitosa con Raspberry Pi Pico W");
                    return true;
                }

                Console.WriteLine("[Dados] ERROR: Pico W no respondió correctamente");
                puerto.Close();
                conectado = false;
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Dados] ERROR al conectar: {ex.Message}");
                conectado = false;
                return false;
            }
        }

        public void Desconectar()
        {
            //cierra la conexión serial
            try
            {
                if (puerto != null && puerto.IsOpen)
                {
                    puerto.Close();
                    conectado = false;
                    Console.WriteLine("[Dados] Desconectado");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Dados] ERROR al desconectar: {ex.Message}");
            }
        }

        public void IniciarEscucha()
        {
            //inicia un hilo para escuchar mensajes del Pico W
            Thread hilo_lectura = new Thread(() =>
            {
                while (conectado)
                {
                    string mensaje = LeerMensaje();
                    if (mensaje != null)
                    {
                        ProcesarMensaje(mensaje);
                    }
                    Thread.Sleep(100);
                }
            })
            {
                IsBackground = true
            };

            hilo_lectura.Start();
        }

        private string LeerMensaje()
        {
            //lee una línea del puerto serial
            try
            {
                if (puerto != null && puerto.IsOpen && puerto.BytesToRead > 0)
                {
                    return puerto.ReadLine().Trim();
                }
            }
            catch (TimeoutException)
            {
                // timeout normal, no es error
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Dados] ERROR en lectura: {ex.Message}");
            }

            return null;
        }

        private void ProcesarMensaje(string mensaje)
        {
            //procesa mensajes recibidos del Pico W
            try
            {
                if (mensaje.StartsWith("DADOS:"))
                {
                    // Formato: DADOS:1,2,3 (dado1, dado2, total)
                    string datos = mensaje.Substring(6);
                    string[] partes = datos.Split(',');

                    if (partes.Length == 3)
                    {
                        lock (bloqueo_datos)
                        {
                            dado1 = int.Parse(partes[0]);
                            dado2 = int.Parse(partes[1]);
                            total = int.Parse(partes[2]);
                        }

                        Console.WriteLine($"[Dados] Lanzados: {dado1} + {dado2} = {total}");
                    }
                }
                else if (mensaje.StartsWith("DISPLAYS:"))
                {
                    Console.WriteLine($"[Dados] {mensaje}");
                }
                else if (mensaje.StartsWith("ERROR:"))
                {
                    Console.WriteLine($"[Dados] ERROR en Pico W: {mensaje}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Dados] ERROR al procesar mensaje '{mensaje}': {ex.Message}");
            }
        }

        public void EnviarComando(string comando)
        {
            //envía un comando al Pico W
            try
            {
                if (puerto != null && puerto.IsOpen)
                {
                    puerto.WriteLine(comando);
                    Console.WriteLine($"[Dados] Comando enviado: {comando}");
                }
                else
                {
                    Console.WriteLine("[Dados] ERROR: Puerto no está abierto");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Dados] ERROR al enviar comando: {ex.Message}");
            }
        }

        public void MostrarEnDisplays(int valor1, int valor2)
        {
            //ordena al Pico W que muestre dos valores en los displays
            EnviarComando($"MOSTRAR:{valor1},{valor2}");
        }

        public void ApagarDisplays()
        {
            //apaga todos los displays
            EnviarComando("APAGAR");
        }

        public (int, int, int) ObtenerUltimoLanzamiento()
        {
            //devuelve (dado1, dado2, total) del último lanzamiento
            lock (bloqueo_datos)
            {
                return (dado1, dado2, total);
            }
        }

        public bool EstaConectado()
        {
            //indica si está conectado al Pico W
            return conectado && puerto != null && puerto.IsOpen;
        }

        public int ObtenerDado1() => dado1; //getter para dado 1
        public int ObtenerDado2() => dado2; //getter para dado 2
        public int ObtenerTotal() => total; //getter para el total
    }
}
