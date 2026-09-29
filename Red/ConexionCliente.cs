//jugador conectado del lado de servidor

//andron

using System.IO;
using System.Net.Sockets;
using System.Text;

namespace Monopoly.Red
{
    public class ConexionCliente //un jugador conectado del lado del servidor: su socket y sus canales de lectura y escritura
    {
        public int Id { get; } //numero de jugador asignado por el servidor, de 1 a 4
        public string Nombre { get; set; } //nombre recibido en CONECTAR; vacio hasta entonces
        public StreamReader Lector { get; } //lee las lineas que envia el cliente
        public StreamWriter Escritor { get; } //envia lineas al cliente
        private TcpClient socket; //conexion TCP subyacente

        public ConexionCliente(int id, TcpClient socket) //prepara los canales de texto sobre el socket
        {
            Id = id;
            Nombre = "";
            this.socket = socket;
            NetworkStream flujo = socket.GetStream();
            Lector = new StreamReader(flujo, Encoding.UTF8);
            Escritor = new StreamWriter(flujo, new UTF8Encoding(false)); //sin BOM, para no ensuciar el primer mensaje
            Escritor.AutoFlush = true; //cada WriteLine sale de inmediato por la red
        }

        public void Cerrar() { socket.Close(); } //corta la conexion
    }
}