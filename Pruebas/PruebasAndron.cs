//para realizar pruebas

using System;
using Monopoly.Estructuras;

namespace Monopoly.Pruebas
{
    public class PruebasAndron //banco de pruebas de las estructuras de Andron
    {
        public static void Ejecutar() //corre todas las pruebas de este integrante
        {
            ProbarListaDoble();
        }

        private static void ProbarListaDoble() //verifica agregado, conteo y recorridos en ambos sentidos
        {
            Console.WriteLine("=== ListaDoble ===");

            ListaDoble<string> lista = new ListaDoble<string>();
            Console.WriteLine("Vacia al inicio: " + lista.EstaVacia()); //se espera True

            lista.Agregar("T1");
            lista.Agregar("T2");
            lista.Agregar("T3");
            Console.WriteLine("Cantidad: " + lista.Contar()); //se espera 3

            Console.Write("De la mas antigua: ");
            NodoDoble<string>? actual = lista.Primero(); //arranca en la cabeza
            while (actual != null) //avanza mientras queden nodos hacia adelante
            {
                Console.Write(actual.Dato + " ");
                actual = actual.Siguiente;
            }
            Console.WriteLine(); //se espera T1 T2 T3

            Console.Write("De la mas reciente: ");
            actual = lista.Ultimo(); //arranca en la cola
            while (actual != null) //retrocede mientras queden nodos hacia atras
            {
                Console.Write(actual.Dato + " ");
                actual = actual.Anterior;
            }
            Console.WriteLine(); //se espera T3 T2 T1
        }
    }
}