//abigail

using System;

namespace Monopoly.Dominio
{
    public class Dado //par de dados; funciona por software o con el modulo fisico
    {
        private Random generador; //fuente de aleatoriedad del modo software

        public int Valor1 { get; private set; } //resultado del primer dado
        public int Valor2 { get; private set; } //resultado del segundo dado

        public Dado() //inicializa el generador y deja los dados en cero
        {
            generador = new Random();
            Valor1 = 0;
            Valor2 = 0;
        }

        public void Lanzar() { } //genera dos valores aleatorios entre 1 y 6

        public void EstablecerValores(int valor1, int valor2) { } //recibe el resultado del dado electronico

        public int Suma() { return Valor1 + Valor2; } //total de pasos que debe avanzar el jugador

        public bool EsDoble() { return Valor1 == Valor2; } //true si ambos dados muestran el mismo valor
    }
}