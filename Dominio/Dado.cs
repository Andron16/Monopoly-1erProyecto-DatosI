//abigail

using System;
using Hardware;

namespace Monopoly.Dominio
{
    public class Dado //par de dados; funciona por software o con el modulo fisico
    {
        private DadosFisicos? dadosFisicos;
        private bool modoFisico; //fuente de aleatoriedad del modo software

        public int valor1; //resultado del primer dado
        public int valor2; //resultado del segundo dado

        public Dado(bool usarHardware = false, string puertoSerial = "COM9") //inicializa el generador y deja los dados en cero
        {
            modoFisico = usarHardware;
            valor1 = 0;
            valor2 = 0;
            dadosFisicos = null;

            if (usarHardware)
            {
                dadosFisicos = new DadosFisicos(puertoSerial);
                if (!dadosFisicos.Conectar())
                {
                    Console.WriteLine("[Dado] No se pudo conectar a hardware.");
                    modoFisico = false;
                    dadosFisicos = null;
                }
                else
                {
                    dadosFisicos.IniciarEscucha();
                }
            }
        }

        public void Lanzar()
        {
            if (modoFisico && dadosFisicos != null && dadosFisicos.EstaConectado())
            {
                Console.WriteLine("[Dado] Esperando lanzamiento en display fisico...");
                System.Threading.Thread.Sleep(500);
                (valor1, valor2, _) = dadosFisicos.ObtenerUltimoLanzamiento();

                if (valor1 == 0 && valor1 == 0)
                {
                    Console.WriteLine("[Dado] Lanzamiento no detectado.");
                }
            }
        } //genera dos valores aleatorios entre 1 y 6


        public int Obtener1() => valor1; //valor del primer dado

        public int Obtener2() => valor2; //valor del segundo dado

        public int ObtenerTotal() => valor1 + valor2; //suma de ambos

        public bool EsModoFisico() => modoFisico && dadosFisicos != null && dadosFisicos.EstaConectado(); //indica modo actual

        public void CambiarPuerto(string nuevoPuerto)
        {
            //permite cambiar el puerto si es necesario
            if (dadosFisicos != null)
            {
                dadosFisicos.Desconectar();
                dadosFisicos = new DadosFisicos(nuevoPuerto);
                if (dadosFisicos.Conectar())
                {
                    dadosFisicos.IniciarEscucha();
                    Console.WriteLine($"[Dado] Reconectado al puerto {nuevoPuerto}");
                }
            }
        }

        public void Desconectar()
        {
            //libera recursos
            if (dadosFisicos != null)
            {
                dadosFisicos.Desconectar();
                dadosFisicos = null;
                modoFisico = false;
            }
        }
    }
}