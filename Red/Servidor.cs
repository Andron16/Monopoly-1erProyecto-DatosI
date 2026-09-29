//andron

using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Monopoly.Red
{
    public class Servidor //acepta hasta 4 jugadores por TCP y atiende a cada uno en su propio hilo
    {
        public const int MaxJugadores = 4; //la partida es de exactamente 4 jugadores

        private int puerto; //puerto TCP donde escucha
        private TcpListener? escucha; //socket servidor que acepta conexiones
        private ConexionCliente?[] conexiones; //un espacio por jugador; null si esta libre
        private int conectados; //jugadores que ya enviaron CONECTAR
        private object candado; //un solo candado para todo el estado compartido

        public Servidor(int puerto) //prepara el servidor sin abrir el puerto todavia
        {
            this.puerto = puerto;
            escucha = null;
            conexiones = new ConexionCliente?[MaxJugadores];
            conectados = 0;
            candado = new object();
        }

        public void Iniciar() //abre el puerto y acepta conexiones hasta completar los 4 jugadores
        {
            escucha = new TcpListener(IPAddress.Any, puerto); //Any: acepta conexiones desde otras computadoras
            escucha.Start();
            Console.WriteLine("[Servidor] Escuchando en el puerto " + puerto);

            int aceptados = 0;
            while (aceptados < MaxJugadores) //se queda esperando a cada jugador
            {
                TcpClient socket = escucha.AcceptTcpClient();
                ConexionCliente conexion = new ConexionCliente(aceptados + 1, socket); //ids de 1 a 4
                lock (candado)
                {
                    conexiones[aceptados] = conexion;
                }
                aceptados++;

                Thread hilo = new Thread(() => AtenderCliente(conexion)); //un hilo por jugador
                hilo.IsBackground = true;
                hilo.Start();
                Console.WriteLine("[Servidor] Conexion aceptada: jugador " + conexion.Id);
            }

            Console.WriteLine("[Servidor] Cupo completo. Presione Enter para cerrar el servidor.");
            Console.ReadLine(); //mantiene vivo el proceso mientras los hilos atienden
        }

        private void AtenderCliente(ConexionCliente conexion) //lee los mensajes de un jugador hasta que se desconecte
        {
            try
            {
                string? linea = conexion.Lector.ReadLine();
                while (linea != null) //null significa que el cliente cerro la conexion
                {
                    Console.WriteLine("[Servidor] <- J" + conexion.Id + ": " + linea);
                    lock (candado) //un solo mensaje a la vez puede modificar el estado
                    {
                        ProcesarMensaje(conexion, linea);
                    }
                    linea = conexion.Lector.ReadLine();
                }
            }
            catch (Exception) //la conexion se corto de golpe
            {
            }

            lock (candado)
            {
                conexiones[conexion.Id - 1] = null; //libera su espacio
                Difundir(Protocolo.Armar(Comandos.Mensaje, "Se desconecto el jugador " + conexion.Id));
            }
            conexion.Cerrar();
        }

        private void ProcesarMensaje(ConexionCliente conexion, string mensaje) //decide que hacer con cada comando; siempre se llama dentro del candado
        {
            string[] partes = Protocolo.Separar(mensaje);
            string comando = partes[0];

            if (comando == Comandos.Conectar) //registra el nombre y confirma el numero de jugador
            {
                if (partes.Length < 2 || conexion.Nombre != "") //falta el nombre o ya se habia conectado
                {
                    Enviar(conexion, Protocolo.Armar(Comandos.Error, "CONECTAR invalido"));
                    return;
                }
                conexion.Nombre = partes[1];
                conectados++;
                Enviar(conexion, Protocolo.Armar(Comandos.Bienvenido, conexion.Id.ToString()));
                Difundir(Protocolo.Armar(Comandos.Mensaje, conexion.Nombre + " se unio (" + conectados + "/" + MaxJugadores + ")"));

                if (conectados == MaxJugadores) //ya estan los 4: arranca la partida
                {
                    Difundir(Protocolo.Armar(Comandos.Inicio));
                }
                return;
            }

            if (conexion.Nombre == "") //nadie puede jugar sin haberse presentado
            {
                Enviar(conexion, Protocolo.Armar(Comandos.Error, "Primero debe enviar CONECTAR"));
                return;
            }

            //TEMPORAL: aqui se conecta la logica de Juego cuando este completa
            Difundir(Protocolo.Armar(Comandos.Mensaje, conexion.Nombre + " envio " + comando));
        }

        private void Enviar(ConexionCliente conexion, string mensaje) //manda un mensaje a un solo jugador
        {
            try
            {
                conexion.Escritor.WriteLine(mensaje);
                Console.WriteLine("[Servidor] -> J" + conexion.Id + ": " + mensaje);
            }
            catch (Exception) //ese cliente ya no esta; su propio hilo se encarga de limpiarlo
            {
            }
        }

        private void Difundir(string mensaje) //manda el mismo mensaje a todos los jugadores conectados
        {
            for (int i = 0; i < conexiones.Length; i++) //recorre los 4 espacios
            {
                ConexionCliente? conexion = conexiones[i];
                if (conexion != null) //espacio ocupado
                {
                    Enviar(conexion, mensaje);
                }
            }
        }
    }
}