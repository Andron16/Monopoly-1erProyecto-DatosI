//para realizar pruebas
//palma

using System;
using Monopoly.Estructuras;

namespace Monopoly.Pruebas
{
    public class PruebasPalma //banco de pruebas de las estructuras de Palma
    {
        public static void Ejecutar() //corre todas las pruebas de esta integrante
        {
            ProbarListaSimple();
        }

        private static void ProbarListaSimple() //verifica agregado, busqueda, obtencion y eliminacion
        {
            Console.WriteLine("=== ListaSimple ===");

            ListaSimple<string> lista = new ListaSimple<string>();
            Console.WriteLine("Vacia al inicio: " + lista.EstaVacia()); //se espera True
            Console.WriteLine("Cantidad inicial: " + lista.Contar());   //se espera 0

            lista.Agregar("A");
            lista.Agregar("B");
            lista.Agregar("C");
            Console.WriteLine("Cantidad tras agregar 3: " + lista.Contar()); //se espera 3
            Console.WriteLine("Vacia tras agregar: " + lista.EstaVacia());    //se espera False

            Console.WriteLine("Contiene B: " + lista.Contiene("B")); //se espera True
            Console.WriteLine("Contiene Z: " + lista.Contiene("Z")); //se espera False

            Console.WriteLine("Obtener(0): " + lista.Obtener(0)); //se espera A
            Console.WriteLine("Obtener(2): " + lista.Obtener(2)); //se espera C

            Console.WriteLine("Eliminar A (primero): " + lista.Eliminar("A")); //se espera True
            Console.WriteLine("Eliminar C (ultimo): " + lista.Eliminar("C"));  //se espera True
            Console.WriteLine("Eliminar Z (no existe): " + lista.Eliminar("Z")); //se espera False
            Console.WriteLine("Cantidad final: " + lista.Contar()); //se espera 1

            //caso de eliminar del medio, con una lista nueva de enteros
            ListaSimple<int> nums = new ListaSimple<int>();
            nums.Agregar(1);
            nums.Agregar(2);
            nums.Agregar(3);
            Console.WriteLine("Eliminar 2 (medio): " + nums.Eliminar(2)); //se espera True
            Console.WriteLine("Contiene 2: " + nums.Contiene(2));         //se espera False
            Console.WriteLine("Cantidad nums: " + nums.Contar());         //se espera 2
        }
    }
}