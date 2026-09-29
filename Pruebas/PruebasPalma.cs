//palma

using System;
using Monopoly.Estructuras;
using Monopoly.Dominio;

namespace Monopoly.Pruebas
{
    public class PruebasPalma //banco de pruebas de las estructuras de Palma
    {
        public static void Ejecutar() //corre todas las pruebas de esta integrante
        {
            ProbarListaSimple();
            ProbarJugador();
            ProbarPropiedad();
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

        private static void ProbarJugador() //verifica saldo, pago, propiedades y patrimonio
        {
            Console.WriteLine("=== Jugador ===");

            Jugador jugador = new Jugador(1, "Palma", 25000); //saldo inicial 25000
            Console.WriteLine("Saldo inicial: " + jugador.Saldo); //se espera 25000

            jugador.AjustarSaldo(-6000); //le cobran un alquiler
            Console.WriteLine("Saldo tras cobro de 6000: " + jugador.Saldo); //se espera 19000
            jugador.AjustarSaldo(3000); //recibe dinero
            Console.WriteLine("Saldo tras recibir 3000: " + jugador.Saldo); //se espera 22000

            Console.WriteLine("Puede pagar 20000: " + jugador.PuedePagar(20000)); //se espera True
            Console.WriteLine("Puede pagar 30000: " + jugador.PuedePagar(30000)); //se espera False

            //le agrega dos propiedades (precios 12000 y 3000)
            Propiedad p1 = new Propiedad(10, "La Cali", 12000, 6000);
            Propiedad p2 = new Propiedad(11, "Desampa", 3000, 750);
            jugador.AgregarPropiedad(p1);
            jugador.AgregarPropiedad(p2);
            Console.WriteLine("Cantidad de propiedades: " + jugador.Propiedades.Contar()); //se espera 2

            //patrimonio = saldo 22000 + precios 12000 + 3000
            Console.WriteLine("Patrimonio: " + jugador.CalcularPatrimonio()); //se espera 37000
        }

        private static void ProbarPropiedad() //verifica disponibilidad y pertenencia de una propiedad
        {
            Console.WriteLine("=== Propiedad ===");

            Propiedad prop = new Propiedad(10, "La Cali", 12000, 6000);
            Console.WriteLine("Disponible al inicio: " + prop.EstaDisponible()); //se espera True

            Jugador dueno = new Jugador(1, "Palma", 25000);
            Jugador otro = new Jugador(2, "Andron", 25000);

            prop.Propietario = dueno; //se le asigna un dueño

            Console.WriteLine("Disponible con dueño: " + prop.EstaDisponible()); //se espera False
            Console.WriteLine("EsDe dueño: " + prop.EsDe(dueno)); //se espera True
            Console.WriteLine("EsDe otro: " + prop.EsDe(otro));   //se espera False
        }
    }
}