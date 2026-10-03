//andron

using System;
using Monopoly.Estructuras;

namespace Monopoly.Dominio
{
    public class Juego //estado completo y oficial de la partida; vive solo en el servidor
    {
        public const int SaldoInicial = 400; //dinero con el que arranca cada jugador (ajustado para que haya quiebras en la partida)
        public const int PremioSalida = 100; //lo que paga el banco al pasar por la salida
        public const int MaxJugadores = 4; //la partida es de exactamente 4

        private Jugador[] jugadores = new Jugador[MaxJugadores]; //todos los inscritos, incluso eliminados; la cola solo tiene a los activos
        private int cantidadJugadores = 0; //cuantos se han inscrito
        private Random azar = new Random(); //elige a quien recibe el regalo de la casilla Regalo
        public bool Iniciado { get; private set; } //true despues de llamar a Iniciar()
        public Tablero Tablero { get; private set; } //casillas enlazadas del tablero
        public ColaCircular<Jugador> Turnos { get; private set; } //rotacion de turnos de los jugadores
        public ColaCartas<CartaEvento> Mazo { get; private set; } //cartas de evento reutilizables
        public ListaDoble<Transaccion> Historial { get; private set; } //todas las transacciones de la partida
        public Banco Banco { get; private set; } //arbitro economico
        public Dado Dado { get; private set; } //par de dados de la partida

        public int NumeroTurno { get; private set; } //turno actual, empieza en 1
        public int LimiteTurnos { get; set; } //tope configurable para terminar la partida
        public bool Terminado { get; private set; } //true cuando ya hay un ganador

        public Juego(int limiteTurnos, bool usarHardware = false, string puertoSerial = "COM9") //arma el estado inicial; el dado usa la Pico si se pide
        {
            Tablero = new Tablero();
            Turnos = new ColaCircular<Jugador>();
            Mazo = new ColaCartas<CartaEvento>();
            Historial = new ListaDoble<Transaccion>();
            Banco = new Banco(Historial);
            Dado = new Dado(usarHardware, puertoSerial);
            NumeroTurno = 1;
            LimiteTurnos = limiteTurnos;
            Terminado = false;
            Iniciado = false;
        }

        public void Iniciar() //construye tablero y mazo, y coloca a los jugadores en la salida
        {
            if (Iniciado) return; //evita construir el tablero dos veces
            Tablero.Construir();
            CrearMazo();
            int i = 0;
            while (i < cantidadJugadores) //todos arrancan en la casilla 0
            {
                jugadores[i].Posicion = Tablero.ObtenerNodo(0);
                i++;
            }
            NumeroTurno = 1;
            Iniciado = true;
        }

        public bool AgregarJugador(Jugador jugador) //inscribe un jugador antes de iniciar; false si ya empezo o esta lleno
        {
            if (Iniciado || cantidadJugadores == MaxJugadores) return false; //no se aceptan mas
            jugadores[cantidadJugadores] = jugador;
            cantidadJugadores++;
            Turnos.Encolar(jugador); //el orden de llegada define el orden de los turnos
            return true;
        }

        public Jugador? ObtenerJugador(int id) //busca un inscrito por su numero; null si no existe
        {
            int i = 0;
            while (i < cantidadJugadores)
            {
                if (jugadores[i].Id == id) return jugadores[i];
                i++;
            }
            return null;
        }

        public int CantidadJugadores() { return cantidadJugadores; } //inscritos, activos o no

        public Jugador? OtroJugadorAlAzar(Jugador excluido) //un jugador activo distinto al indicado; null si no queda ninguno
        {
            Jugador[] candidatos = new Jugador[MaxJugadores]; //arreglo nativo T[], permitido
            int cantidad = 0;
            int i = 0;
            while (i < cantidadJugadores) //junta a los activos que no son el excluido
            {
                if (jugadores[i].Activo && jugadores[i] != excluido)
                {
                    candidatos[cantidad] = jugadores[i];
                    cantidad++;
                }
                i++;
            }
            if (cantidad == 0) return null;
            return candidatos[azar.Next(cantidad)]; //Next(n) da un indice de 0 a n-1
        }

        private void CrearMazo() //llena el mazo con las cartas de evento; salen en este orden y se reciclan
        {
            Mazo.Encolar(new CartaEvento(1, "Ganaste un concurso de cafe", TipoEfecto.RecibirDinero, 100));
            Mazo.Encolar(new CartaEvento(2, "Multa de transito", TipoEfecto.PagarDinero, 50));
            Mazo.Encolar(new CartaEvento(3, "Agarraste la ruta rapida: avanza 3", TipoEfecto.Avanzar, 3));
            Mazo.Encolar(new CartaEvento(4, "Presa en la General Canas: retrocede 2", TipoEfecto.Retroceder, 2));
            Mazo.Encolar(new CartaEvento(5, "Se te fue el bus: pierdes un turno", TipoEfecto.PerderTurno, 1));
            Mazo.Encolar(new CartaEvento(6, "Vuelve a la salida", TipoEfecto.IrACasilla, 0));
            Mazo.Encolar(new CartaEvento(7, "Aguinaldo adelantado", TipoEfecto.RecibirDinero, 150));
            Mazo.Encolar(new CartaEvento(8, "Pagas el marchamo", TipoEfecto.PagarDinero, 100));
            Mazo.Encolar(new CartaEvento(9, "Te mandan a la carcel", TipoEfecto.IrACasilla, 9));
            Mazo.Encolar(new CartaEvento(10, "Encontraste plata en la calle", TipoEfecto.RecibirDinero, 50));
        }

        public Jugador JugadorActual() { return Turnos.Frente(); } //el del frente de la cola tiene el turno; solo llamar con la partida iniciada

        public bool EsSuTurno(int idJugador) //valida que el jugador pueda actuar ahora
        {
            if (!Iniciado || Terminado || Turnos.EstaVacia()) return false; //no hay partida en curso
            return Turnos.Frente().Id == idJugador;
        }

        public bool TirarDados(int idJugador) //lanza, mueve y aplica el efecto de la casilla; false si no le toca o ya tiro
        {
            if (!EsSuTurno(idJugador)) return false; //jugar fuera de turno
            Jugador jugador = JugadorActual();
            if (jugador.YaTiroDados) return false; //un solo lanzamiento por turno

            Dado.Lanzar(); //boton fisico o software
            jugador.YaTiroDados = true;
            int pasos = Dado.ObtenerTotal();
            NodoDoble<Casilla> origen = jugador.Posicion!;

            if (Tablero.PasoPorSalida(origen, pasos)) //cruzo o cayo en la salida
            {
                Banco.PremioPorSalida(jugador, PremioSalida, NumeroTurno);
            }
            jugador.Posicion = Tablero.Mover(origen, pasos); //recorre nodo por nodo
            CaerEnCasilla(jugador);

            if (!jugador.Activo) SacarEliminado(jugador); //quebro durante su propio turno
            return true;
        }

        private void CaerEnCasilla(Jugador jugador) //aplica la casilla donde quedo; si una carta lo movio, aplica tambien la de destino
        {
            Casilla casilla = jugador.Posicion!.Dato;
            casilla.AlCaer(jugador, this); //polimorfismo: cada casilla sabe que hacer
            if (!jugador.Activo) return; //quedo eliminado, no sigue

            Casilla destino = jugador.Posicion!.Dato;
            if (destino != casilla && !(destino is CasillaEvento)) //una carta lo movio; no se saca otra carta en cadena
            {
                if (destino.Id == 0) Banco.PremioPorSalida(jugador, PremioSalida, NumeroTurno); //la carta lo mando a la salida
                destino.AlCaer(jugador, this);
            }
        }

        private void PasarAlSiguiente() //deja listo el turno del siguiente jugador que pueda jugar
        {
            if (Turnos.EstaVacia()) return;
            NumeroTurno++;
            while (Turnos.Frente().TurnosPerdidos > 0) //salta a quien tenga turnos perdidos
            {
                Turnos.Frente().TurnosPerdidos--; //cumple uno de sus turnos de castigo
                Turnos.Avanzar();
            }
            Turnos.Frente().YaTiroDados = false; //el nuevo jugador todavia no ha tirado
        }

        private void SacarEliminado(Jugador jugador) //quita de la rotacion al que quebro en su propio turno
        {
            Turnos.Eliminar(jugador); //era el frente: el turno pasa solo al siguiente
            PasarAlSiguiente();
            VerificarFin();
        }

        public bool ComprarPropiedadActual(int idJugador) //compra la propiedad donde esta parado; false si no se puede
        {
            if (!EsSuTurno(idJugador)) return false; //fuera de turno
            Jugador jugador = JugadorActual();
            if (!jugador.YaTiroDados) return false; //primero tiene que tirar y caer en la casilla
            Propiedad? propiedad = jugador.Posicion!.Dato as Propiedad; //null si la casilla no es una propiedad
            if (propiedad == null) return false;
            return Banco.ComprarPropiedad(jugador, propiedad, NumeroTurno); //el banco valida dueño y saldo
        }

        public bool TerminarTurno(int idJugador) //pasa el turno al siguiente; false si no le toca o todavia no tiro
        {
            if (!EsSuTurno(idJugador)) return false;
            if (!JugadorActual().YaTiroDados) return false; //no puede pasar sin tirar
            Turnos.Avanzar(); //el frente pasa al siguiente de la cola circular
            PasarAlSiguiente();
            VerificarFin();
            return true;
        }

        public ListaSimple<Transaccion> BuscarPorJugador(string nombre) //transacciones donde el jugador es origen o destino
        {
            ListaSimple<Transaccion> resultados = new ListaSimple<Transaccion>();
            NodoDoble<Transaccion>? actual = Historial.Primero(); //de la mas antigua a la mas reciente
            while (actual != null) //revisa cada transaccion del historial
            {
                if (actual.Dato.Origen == nombre || actual.Dato.Destino == nombre) //el jugador pago o cobro
                {
                    resultados.Agregar(actual.Dato);
                }
                actual = actual.Siguiente;
            }
            return resultados;
        }

        public ListaSimple<Transaccion> BuscarPorTipo(TipoTransaccion tipo) //transacciones de una categoria determinada
        {
            ListaSimple<Transaccion> resultados = new ListaSimple<Transaccion>();
            NodoDoble<Transaccion>? actual = Historial.Primero(); //de la mas antigua a la mas reciente
            while (actual != null) //revisa cada transaccion del historial
            {
                if (actual.Dato.Tipo == tipo) //coincide la categoria
                {
                    resultados.Agregar(actual.Dato);
                }
                actual = actual.Siguiente;
            }
            return resultados;
        }

        public Jugador? Ganador() //el unico activo o, si se agotaron los turnos, el activo con mayor patrimonio
        {
            if (!Terminado) return null; //todavia no hay ganador
            Jugador? mejor = null;
            int i = 0;
            while (i < cantidadJugadores) //revisa a todos los inscritos
            {
                Jugador j = jugadores[i];
                if (j.Activo && (mejor == null || j.CalcularPatrimonio() > mejor.CalcularPatrimonio())) //solo compiten los activos
                {
                    mejor = j;
                }
                i++;
            }
            return mejor;
        }

        public bool VerificarFin() //termina la partida si queda uno solo o se agotaron los turnos
        {
            if (!Iniciado) return false; //antes de empezar no hay fin
            if (Turnos.Contar() <= 1 || NumeroTurno > LimiteTurnos) Terminado = true;
            return Terminado;
        }
    }

}