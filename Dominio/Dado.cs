//abigail

using System;
using Monopoly.Hardware;

namespace Monopoly.Dominio
{
    public class Dado //par de dados; usa el modulo fisico si esta conectado, si no genera por software
    {
        private const int EsperaMaxima = 15000; //milisegundos que se espera el boton antes de pasar a software

        private ModuloFisico? modulo; //Pico con dados y lector RFID; null en modo software
        private Random generador; //fuente de aleatoriedad del modo software
        private int valor1; //resultado del primer dado
        private int valor2; //resultado del segundo dado

        public Dado(bool usarHardware = false, string puertoSerial = "COM9") //arranca en software y, si se pide, intenta conectar la Pico
        {
            generador = new Random();
            valor1 = 0;
            valor2 = 0;
            modulo = null;

            if (usarHardware) //solo se toca el puerto si se pidio el modo fisico
            {
                ConectarHardware(puertoSerial);
            }
        }

        public void Lanzar() //obtiene los dos valores del boton fisico o, si no hay, por software
        {
            if (modulo != null && modulo.EstaConectado()) //modo fisico activo
            {
                Console.WriteLine("[Dado] Esperando que se presione el boton...");
                if (modulo.Dados.EsperarLanzamiento(EsperaMaxima, out valor1, out valor2)) //el jugador presiono a tiempo
                {
                    return;
                }
                Console.WriteLine("[Dado] No se detecto lanzamiento fisico; se usa modo software.");
            }

            valor1 = generador.Next(1, 7); //Next excluye el limite superior: da valores de 1 a 6
            valor2 = generador.Next(1, 7);
        }

        public int Obtener1() { return valor1; } //valor del primer dado

        public int Obtener2() { return valor2; } //valor del segundo dado

        public int ObtenerTotal() { return valor1 + valor2; } //suma de ambos dados

        public bool EsModoFisico() { return modulo != null && modulo.EstaConectado(); } //true si la Pico esta respondiendo

        public ModuloFisico? ObtenerModulo() { return modulo; } //permite al servidor usar el lector RFID por el mismo puerto; null sin hardware

        public void CambiarPuerto(string nuevoPuerto) //cierra la conexion actual e intenta en otro puerto
        {
            Desconectar();
            ConectarHardware(nuevoPuerto);
        }

        public void Desconectar() //libera el puerto serial y vuelve a modo software
        {
            if (modulo != null)
            {
                modulo.Desconectar();
                modulo = null;
            }
        }

        private void ConectarHardware(string puerto) //abre el puerto; si la Pico no responde queda en software
        {
            modulo = new ModuloFisico(puerto);
            if (!modulo.Conectar()) //la Pico no respondio al handshake
            {
                Console.WriteLine("[Dado] Sin hardware; se usa modo software.");
                modulo = null;
            }
        }
    }
}