//andron

using System;
using Monopoly.Estructuras;
using Monopoly.Dominio;
using Monopoly.Reportes;
using Monopoly.Red;

namespace Monopoly.Pruebas
{
    public class PruebasAndron //banco de pruebas de Andron
    {
        public static void Ejecutar() //corre todas las pruebas de este integrante
        {
            ProbarListaDoble();
            ProbarBanco();
            ProbarReporte();
            ProbarBusquedas();
            ProbarProtocolo();
            ProbarJuego();
            ProbarEliminacion();
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
        private static void ProbarReporte() //verifica el formato del reporte y la exportacion a TXT
        {
            Console.WriteLine("=== Reporte de transacciones ===");

            ListaDoble<Transaccion> historial = new ListaDoble<Transaccion>();
            Banco banco = new Banco(historial);
            Jugador ana = new Jugador(1, "Ana", 1500);
            Jugador beto = new Jugador(2, "Beto", 1500);
            Propiedad biblioteca = new Propiedad(5, "Biblioteca", 300, 50);

            banco.PremioPorSalida(ana, 200, 1);
            banco.ComprarPropiedad(ana, biblioteca, 1);
            banco.CobrarAlquiler(beto, biblioteca, 2);

            Console.WriteLine(ReporteTransacciones.GenerarTexto(historial)); //se espera #1, #2, #3 en ese orden
            Console.WriteLine(ReporteTransacciones.GenerarTexto(historial, true)); //se espera #3, #2, #1

            bool exportado = ReporteTransacciones.Exportar(historial, "docs/transacciones/reporte_prueba.txt");
            Console.WriteLine("Exportado: " + exportado); //se espera True
        }
        private static void ProbarBusquedas() //verifica las busquedas del historial por jugador y por tipo
        {
            Console.WriteLine("=== Busquedas del historial ===");

            Juego juego = new Juego(50);
            Jugador ana = new Jugador(1, "Ana", 1500);
            Jugador beto = new Jugador(2, "Beto", 1500);
            Propiedad biblioteca = new Propiedad(5, "Biblioteca", 300, 50);
            Propiedad soda = new Propiedad(6, "Soda", 200, 30);

            juego.Banco.PremioPorSalida(ana, 200, 1); //#1 BANCO -> Ana
            juego.Banco.ComprarPropiedad(ana, biblioteca, 1); //#2 Ana -> BANCO
            juego.Banco.ComprarPropiedad(beto, soda, 2); //#3 Beto -> BANCO
            juego.Banco.CobrarAlquiler(beto, biblioteca, 3); //#4 Beto -> Ana

            Console.WriteLine("De Ana:"); //se espera #1, #2, #4
            ImprimirResultados(juego.BuscarPorJugador("Ana"));

            Console.WriteLine("De Beto:"); //se espera #3, #4
            ImprimirResultados(juego.BuscarPorJugador("Beto"));

            Console.WriteLine("De Carlos (no existe):"); //se espera 0 resultados
            ImprimirResultados(juego.BuscarPorJugador("Carlos"));

            Console.WriteLine("Compras de propiedad:"); //se espera #2, #3
            ImprimirResultados(juego.BuscarPorTipo(TipoTransaccion.CompraPropiedad));

            Console.WriteLine("Alquileres:"); //se espera #4
            ImprimirResultados(juego.BuscarPorTipo(TipoTransaccion.PagoAlquiler));

            Console.WriteLine("Perdidas por evento:"); //se espera 0 resultados
            ImprimirResultados(juego.BuscarPorTipo(TipoTransaccion.PerdidaPorEvento));
        }

        private static void ImprimirResultados(ListaSimple<Transaccion> resultados) //muestra la cantidad y el detalle de cada resultado
        {
            Console.WriteLine("  Resultados: " + resultados.Contar());
            for (int i = 0; i < resultados.Contar(); i++) //recorre los resultados por indice
            {
                Transaccion t = resultados.Obtener(i);
                Console.WriteLine("  #" + t.Id + " " + t.Tipo + " | " + t.Origen + " -> " + t.Destino + " | " + t.Monto);
            }
        }
        private static void ProbarProtocolo() //verifica que los mensajes se armen y se separen correctamente
        {
            Console.WriteLine("=== Protocolo ===");

            string dados = Protocolo.Armar(Comandos.Dados, "2", "3", "4");
            Console.WriteLine("Armar con campos: " + dados); //se espera DADOS|2|3|4

            string tirar = Protocolo.Armar(Comandos.TirarDados);
            Console.WriteLine("Armar sin campos: " + tirar); //se espera TIRAR_DADOS

            string[] partes = Protocolo.Separar(dados);
            Console.WriteLine("Partes: " + partes.Length + " -> " + partes[0] + ", " + partes[1] + ", " + partes[2] + ", " + partes[3]); //se espera 4 -> DADOS, 2, 3, 4

            Console.WriteLine("Comando: " + Protocolo.Comando(dados)); //se espera DADOS

            string conectar = Protocolo.Armar(Comandos.Conectar, "Ana|Beto");
            Console.WriteLine("Campo con separador: " + conectar); //se espera CONECTAR|Ana/Beto
            Console.WriteLine("Partes: " + Protocolo.Separar(conectar).Length); //se espera 2
        }
        private static void ProbarJuego() //simula una partida completa sin red: turnos, compras, eventos, eliminacion y fin
        {
            Console.WriteLine("=== Juego ===");

            Juego juego = new Juego(60); //limite de 60 turnos, dados por software
            //saldo bajo a proposito para que alguien quiebre y se pruebe la eliminacion
            juego.AgregarJugador(new Jugador(1, "Ana", 150));
            juego.AgregarJugador(new Jugador(2, "Beto", 150));
            juego.AgregarJugador(new Jugador(3, "Carla", 150));
            juego.AgregarJugador(new Jugador(4, "Dani", 150));
            Console.WriteLine("Quinto jugador rechazado: " + !juego.AgregarJugador(new Jugador(5, "Eva", 400))); //se espera True

            juego.Iniciar();
            Console.WriteLine("Todos en la salida: " + (juego.ObtenerJugador(4)!.Posicion!.Dato.Id == 0)); //se espera True

            //validaciones antes de jugar
            Console.WriteLine("Turno de Ana: " + juego.EsSuTurno(1)); //se espera True
            Console.WriteLine("Beto fuera de turno rechazado: " + !juego.TirarDados(2)); //se espera True
            Console.WriteLine("Ana no puede terminar sin tirar: " + !juego.TerminarTurno(1)); //se espera True
            Console.WriteLine("Ana tira: " + juego.TirarDados(1)); //se espera True
            Console.WriteLine("Ana no puede tirar dos veces: " + !juego.TirarDados(1)); //se espera True
            juego.TerminarTurno(1);

            //partida automatica: cada jugador tira, compra si puede y pasa
            int vueltas = 0; //tope de seguridad por si algo queda en ciclo
            while (!juego.Terminado && vueltas < 200)
            {
                Jugador actual = juego.JugadorActual();
                int turno = juego.NumeroTurno;
                juego.TirarDados(actual.Id);
                bool compro = actual.Activo && juego.ComprarPropiedadActual(actual.Id); //intenta comprar donde cayo

                Console.WriteLine("T" + turno + " " + actual.Nombre
                    + " dados " + juego.Dado.Obtener1() + "+" + juego.Dado.Obtener2()
                    + " -> " + actual.Posicion!.Dato.Id + " " + actual.Posicion.Dato.Nombre
                    + " | saldo " + actual.Saldo
                    + (compro ? " | COMPRO" : "")
                    + (actual.TurnosPerdidos > 0 ? " | pierde turno" : "")
                    + (!actual.Activo ? " | ELIMINADO" : ""));

                if (actual.Activo) juego.TerminarTurno(actual.Id); //si quebro, el turno ya paso solo
                vueltas++;
            }

            Jugador? ganador = juego.Ganador();
            Console.WriteLine("Terminado: " + juego.Terminado + " en el turno " + juego.NumeroTurno);
            Console.WriteLine("Jugadores en la cola: " + juego.Turnos.Contar());
            Console.WriteLine("Ganador: " + (ganador == null ? "ninguno" : ganador.Nombre + " con patrimonio " + ganador.CalcularPatrimonio()));
            Console.WriteLine("Transacciones registradas: " + juego.Historial.Contar());
            Console.WriteLine("  Compras: " + juego.BuscarPorTipo(TipoTransaccion.CompraPropiedad).Contar());
            Console.WriteLine("  Alquileres: " + juego.BuscarPorTipo(TipoTransaccion.PagoAlquiler).Contar());
            Console.WriteLine("  Premios salida: " + juego.BuscarPorTipo(TipoTransaccion.PremioPorInicio).Contar());
            Console.WriteLine("  Eventos (+): " + juego.BuscarPorTipo(TipoTransaccion.GananciaPorEvento).Contar());
            Console.WriteLine("  Eventos (-): " + juego.BuscarPorTipo(TipoTransaccion.PerdidaPorEvento).Contar());
            Console.WriteLine();
        }
        private static void ProbarEliminacion() //fuerza quiebras: Beto es dueño de todo y los demas empiezan sin dinero
        {
            Console.WriteLine("=== Eliminacion y fin por ultimo jugador ===");

            Juego juego = new Juego(200); //limite alto: la partida debe terminar por eliminacion, no por turnos
            Jugador beto = new Jugador(2, "Beto", 1500);
            juego.AgregarJugador(new Jugador(1, "Ana", 0));
            juego.AgregarJugador(beto);
            juego.AgregarJugador(new Jugador(3, "Carla", 0));
            juego.AgregarJugador(new Jugador(4, "Dani", 0));
            juego.Iniciar();

            int i = 0;
            while (i < juego.Tablero.Contar()) //le asigna a Beto todas las propiedades del tablero
            {
                Propiedad? p = juego.Tablero.ObtenerCasilla(i) as Propiedad; //null si no es propiedad
                if (p != null)
                {
                    p.Propietario = beto;
                    beto.AgregarPropiedad(p);
                    p.Alquiler = 10000; //alquiler imposible de pagar: el que caiga en una propiedad quiebra
                }
                i++;
            }
            Console.WriteLine("Propiedades de Beto: " + beto.Propiedades.Contar()); //se espera 21

            int vueltas = 0;
            while (!juego.Terminado && vueltas < 200)
            {
                Jugador actual = juego.JugadorActual();
                int turno = juego.NumeroTurno;
                juego.TirarDados(actual.Id);
                Console.WriteLine("T" + turno + " " + actual.Nombre + " -> " + actual.Posicion!.Dato.Nombre
                    + " | saldo " + actual.Saldo + (!actual.Activo ? " | ELIMINADO" : ""));
                if (actual.Activo) juego.TerminarTurno(actual.Id); //si quebro, el turno ya paso solo
                vueltas++;
            }

            Jugador? ganador = juego.Ganador();
            Console.WriteLine("Terminado: " + juego.Terminado + " en el turno " + juego.NumeroTurno); //se espera True, mucho antes de 200
            Console.WriteLine("Jugadores en la cola: " + juego.Turnos.Contar()); //se espera 1
            Console.WriteLine("Ganador: " + (ganador == null ? "ninguno" : ganador.Nombre)); //se espera Beto
            Console.WriteLine("Ana puede actuar: " + juego.EsSuTurno(1)); //se espera False
            Console.WriteLine("Alquileres cobrados: " + juego.BuscarPorTipo(TipoTransaccion.PagoAlquiler).Contar()); //se espera 0: con saldo 0 nadie paga, quiebra
            Console.WriteLine();
        }
    }
}
