//andron

namespace Monopoly.Red
{
    public static class Protocolo //arma y separa los mensajes de texto que viajan por el socket
    {
        public const char Separador = '|'; //divide el comando de sus campos

        public static string Armar(string comando, params string[] campos) //une el comando y sus campos: COMANDO|campo1|campo2
        {
            string mensaje = comando;
            for (int i = 0; i < campos.Length; i++) //agrega cada campo precedido por el separador
            {
                mensaje += Separador + Limpiar(campos[i]);
            }
            return mensaje;
        }

        public static string[] Separar(string mensaje) //divide un mensaje en sus partes; la posicion 0 es el comando
        {
            return mensaje.Split(Separador);
        }

        public static string Comando(string mensaje) //devuelve solo el nombre del comando
        {
            return Separar(mensaje)[0];
        }

        private static string Limpiar(string campo) //evita que un campo rompa el formato del mensaje
        {
            return campo.Replace(Separador, '/').Replace('\n', ' ').Replace('\r', ' ');
        }
    }
}