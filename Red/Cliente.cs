//andron

using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Monopoly.Red
{
    public class Cliente //conexion de un jugador con el servidor; solo envia y recibe texto
    {
        private TcpClient? socket; //conexion TCP con el servidor
        private StreamReader? lector; //lee lo que envia el servidor
        private StreamWriter? escritor; //envia comandos al servidor
        private bool conectado; //true mientras la conexion este viva

        public Action<string>? AlRecibir { get; set; } //funcion que se ejecuta con cada mensaje recibido; la define la interfaz

        public Cliente() //cliente sin conectar
        {
            socket = null;
            lector = null;
            escritor = null;
            conectado = false;
            AlRecibir = null;
        }

        public bool Conectar(string ip, int puerto, string nombre) //abre la conexion, arranca la escucha y se presenta con CONECTAR
        {
            try
            {
                socket = new TcpClient(ip, puerto);
                NetworkStream flujo = socket.GetStream();
                lector = new StreamReader(flujo, Encoding.UTF8);
                escritor = new StreamWriter(flujo, new UTF8Encoding(false)); //sin BOM, igual que el servidor
                escritor.AutoFlush = true;
                conectado = true;

                Thread hilo = new Thread(Escuchar); //los mensajes del servidor llegan en un hilo aparte
                hilo.IsBackground = true;
                hilo.Start();

                Enviar(Protocolo.Armar(Comandos.Conectar, nombre));
                return true;
            }
            catch (Exception ex) //servidor apagado, IP equivocada, firewall, etc.
            {
                Console.WriteLine("[Cliente] No se pudo conectar a " + ip + ":" + puerto + " -> " + ex.Message);
                return false;
            }
        }

        public void Enviar(string mensaje) //manda una linea al servidor
        {
            if (!conectado || escritor == null) //sin conexion no hay a quien enviar
            {
                return;
            }
            try
            {
                escritor.WriteLine(mensaje);
            }
            catch (Exception) //la conexion se cayo
            {
                conectado = false;
            }
        }

        public void Desconectar() //cierra la conexion con el servidor
        {
            conectado = false;
            if (socket != null)
            {
                socket.Close();
            }
        }

        private void Escuchar() //lee mensajes del servidor hasta que se corte la conexion
        {
            try
            {
                string? linea = lector!.ReadLine(); //lector ya existe: se crea antes de arrancar este hilo
                while (linea != null) //null: el servidor cerro la conexion
                {
                    if (AlRecibir != null) //la interfaz decide como mostrar el mensaje
                    {
                        AlRecibir(linea);
                    }
                    linea = lector.ReadLine();
                }
            }
            catch (Exception ex) //conexion cortada de golpe; se muestra la causa para depurar
            {
                Console.WriteLine("[Cliente] Error en la conexion: " + ex.Message);
            }

            conectado = false;
            if (AlRecibir != null)
            {
                AlRecibir(Protocolo.Armar(Comandos.Mensaje, "Conexion con el servidor cerrada"));
            }
        }
    }
}