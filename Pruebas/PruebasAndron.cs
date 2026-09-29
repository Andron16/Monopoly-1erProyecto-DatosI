//andron

using System;
using Monopoly.Estructuras;
using Monopoly.Dominio;

namespace Monopoly.Pruebas
{
    public class PruebasAndron //banco de pruebas de Andron
    {
        public static void Ejecutar() //corre todas las pruebas de este integrante
        {
            ProbarListaDoble();
            ProbarBanco();
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

        private static void ProbarBanco() //verifica pagos, cobros, compras, alquiler y eliminacion
        {
            Console.WriteLine("=== Banco ===");

            ListaDoble<Transaccion> historial = new ListaDoble<Transaccion>();
            Banco banco = new Banco(historial);
            Jugador ana = new Jugador(1, "Ana", 1500);
            Jugador beto = new Jugador(2, "Beto", 1500);
            Propiedad biblioteca = new Propiedad(5, "Biblioteca", 300, 50);

            banco.Pagar(ana, 200, 1, TipoTransaccion.GananciaPorEvento, "Prueba de pago");
            Console.WriteLine("Pagar: saldo Ana = " + ana.Saldo); //se espera 1700

            bool cobro = banco.Cobrar(ana, 100, 1, TipoTransaccion.PagoAlBanco, "Prueba de cobro");
            Console.WriteLine("Cobrar: " + cobro + ", saldo Ana = " + ana.Saldo); //se espera True, 1600

            bool compra = banco.ComprarPropiedad(ana, biblioteca, 1);
            Console.WriteLine("Comprar: " + compra + ", saldo Ana = " + ana.Saldo + ", propiedades = " + ana.Propiedades.Contar()); //se espera True, 1300, 1

            bool compraRepetida = banco.ComprarPropiedad(beto, biblioteca, 2);
            Console.WriteLine("Comprar propiedad con dueño: " + compraRepetida + ", saldo Beto = " + beto.Saldo); //se espera False, 1500

            banco.CobrarAlquiler(beto, biblioteca, 2);
            Console.WriteLine("Alquiler: saldo Beto = " + beto.Saldo + ", saldo Ana = " + ana.Saldo); //se espera 1450 y 1350

            banco.CobrarAlquiler(ana, biblioteca, 3);
            Console.WriteLine("Alquiler en propiedad propia: saldo Ana = " + ana.Saldo); //se espera 1350, sin cambio

            bool cobroImposible = banco.Cobrar(beto, 5000, 3, TipoTransaccion.PagoAlBanco, "Impuesto imposible");
            Console.WriteLine("Cobro sin saldo: " + cobroImposible + ", Beto activo = " + beto.Activo); //se espera False, False

            banco.EliminarJugador(ana);
            Console.WriteLine("Eliminar a Ana: activa = " + ana.Activo + ", Biblioteca disponible = " + biblioteca.EstaDisponible()); //se espera False, True

            Console.WriteLine("Transacciones registradas: " + historial.Contar()); //se espera 4
            NodoDoble<Transaccion>? actual = historial.Primero(); //de la mas antigua a la mas reciente
            while (actual != null) //recorre todo el historial
            {
                Transaccion t = actual.Dato;
                Console.WriteLine("  #" + t.Id + " " + t.Tipo + " | " + t.Origen + " -> " + t.Destino + " | " + t.Monto);
                actual = actual.Siguiente;
            }
        }
    }
}
