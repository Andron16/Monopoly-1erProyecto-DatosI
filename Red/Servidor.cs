//andron

using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Monopoly.Dominio;
using Monopoly.Estructuras;
using Monopoly.Reportes;

namespace Monopoly.Red
{
    public class Servidor //acepta hasta 4 jugadores por TCP y atiende a cada uno en su propio hilo
    {
        public const int MaxJugadores = 4; //la partida es de exactamente 4 jugadores

        private int puerto; //puerto TCP donde escucha
        private TcpListener? escucha; //socket servidor que acepta conexiones
        private ConexionCliente?[] conexiones; //un espacio por jugador; null si esta libre
        private int conectados; //jugadores que ya enviaron CONECTAR
        private object candado; //un solo candado para todo el estado compartido
        private Juego juego; //estado oficial de la partida; solo el servidor lo modifica

        public Servidor(int puerto, Juego juego) //prepara el servidor sin abrir el puerto todavia
        {
            this.puerto = puerto;
            escucha = null;
            conexiones = new ConexionCliente?[MaxJugadores];
            conectados = 0;
            candado = new object();
            this.juego = juego; //la partida la crea Program y el servidor la administra
        }

        public void Iniciar() //abre el puerto y acepta conexiones; cada una ocupa el primer espacio libre
        {
            escucha = new TcpListener(IPAddress.Any, puerto); //Any: acepta conexiones desde otras computadoras
            escucha.Start();
            Console.WriteLine("[Servidor] Escuchando en el puerto " + puerto + " (Ctrl + C para cerrar)");

            while (true) //sigue aceptando: una conexion que se va sin presentarse deja su espacio libre
            {
                TcpClient socket = escucha.AcceptTcpClient();
                ConexionCliente? conexion = null;
                lock (candado)
                {
                    int libre = BuscarEspacioLibre();
                    if (libre >= 0) //hay lugar: el numero de jugador es el espacio + 1
                    {
                        conexion = new ConexionCliente(libre + 1, socket);
                        conexiones[libre] = conexion;
                    }
                }
                if (conexion == null) //los 4 lugares ya tienen jugador: se rechaza
                {
                    socket.Close();
                    Console.WriteLine("[Servidor] Conexion rechazada: no hay lugar");
                    continue;
                }

                ConexionCliente nueva = conexion; //copia no nula para el hilo
                Thread hilo = new Thread(() => AtenderCliente(nueva)); //un hilo por jugador
                hilo.IsBackground = true;
                hilo.Start();
                Console.WriteLine("[Servidor] Conexion aceptada: jugador " + nueva.Id);
            }
        }

        private int BuscarEspacioLibre() //primer espacio sin conexion y sin jugador inscrito; -1 si no hay; se llama dentro del candado
        {
            for (int i = 0; i < MaxJugadores; i++)
            {
                if (conexiones[i] == null && juego.ObtenerJugador(i + 1) == null) return i;
            }
            return -1;
        }

        private void AtenderCliente(ConexionCliente conexion) //lee los mensajes de un jugador hasta que se desconecte
        {
            try
            {
                string? linea = conexion.Lector.ReadLine();
                while (linea != null) //null significa que el cliente cerro la conexion
                {
                    Console.WriteLine("[Servidor] <- J" + conexion.Id + ": " + linea);
                    lock (candado) //un solo mensaje a la vez puede modificar el estado
                    {
                        ProcesarMensaje(conexion, linea);
                    }
                    linea = conexion.Lector.ReadLine();
                }
            }
            catch (Exception) //la conexion se corto de golpe
            {
            }

            lock (candado)
            {
                conexiones[conexion.Id - 1] = null; //libera su espacio
                if (conexion.Nombre != "") //solo se avisa si era un jugador inscrito, no una conexion de prueba
                {
                    Difundir(Protocolo.Armar(Comandos.Mensaje, "Se desconecto " + conexion.Nombre));
                }
            }
            conexion.Cerrar();
        }

        private void ProcesarMensaje(ConexionCliente conexion, string mensaje) //decide que hacer con cada comando; siempre se llama dentro del candado
        {
            string[] partes = Protocolo.Separar(mensaje);
            string comando = partes[0];

            if (comando == Comandos.Conectar) //registra el nombre y confirma el numero de jugador
            {
                if (partes.Length < 2 || partes[1].Trim() == "" || conexion.Nombre != "") //falta el nombre, viene vacio o ya se habia conectado
                {
                    Enviar(conexion, Protocolo.Armar(Comandos.Error, "CONECTAR invalido"));
                    return;
                }
                conexion.Nombre = partes[1].Trim(); //quita espacios sobrantes al inicio y al final
                conectados++;
                juego.AgregarJugador(new Jugador(conexion.Id, conexion.Nombre, Juego.SaldoInicial)); //el id de la conexion es el id del jugador
                Enviar(conexion, Protocolo.Armar(Comandos.Bienvenido, conexion.Id.ToString()));
                Difundir(Protocolo.Armar(Comandos.Mensaje, conexion.Nombre + " se unio (" + conectados + "/" + MaxJugadores + ")"));
                DifundirEstado(); //todos reciben JUGADOR|... de los que ya entraron; el lobby se llena de a uno

                if (conectados == MaxJugadores) //ya estan los 4: arranca la partida
                {
                    juego.Iniciar(); //tablero, mazo y fichas en la salida
                    Difundir(Protocolo.Armar(Comandos.Inicio));
                    DifundirEstado();
                    AnunciarTurno();
                }
                return;
            }

            if (conexion.Nombre == "") //nadie puede jugar sin haberse presentado
            {
                Enviar(conexion, Protocolo.Armar(Comandos.Error, "Primero debe enviar CONECTAR"));
                return;
            }

            if (!juego.Iniciado) //todavia faltan jugadores
            {
                Enviar(conexion, Protocolo.Armar(Comandos.Error, "La partida no ha iniciado"));
                return;
            }
            ProcesarJugada(conexion, comando);
        }

        private void ProcesarJugada(ConexionCliente conexion, string comando) //ejecuta un comando de juego; siempre dentro del candado
        {
            int id = conexion.Id; //el servidor sabe quien es por la conexion, no por lo que diga el mensaje

            if (comando == Comandos.ConsultarEstado) { EnviarEstado(conexion); return; } //consultas: se permiten en cualquier momento
            if (comando == Comandos.ConsultarTransacciones) { EnviarHistorial(conexion); return; }

            if (juego.Terminado)
            {
                Enviar(conexion, Protocolo.Armar(Comandos.Error, "La partida ya termino"));
                return;
            }
            if (!juego.EsSuTurno(id)) //validacion: jugar fuera de turno
            {
                Enviar(conexion, Protocolo.Armar(Comandos.Error, "No es su turno"));
                return;
            }

            int transaccionesAntes = juego.Historial.Contar(); //para avisar solo las nuevas
            int turnoAntes = juego.NumeroTurno; //para saber si el turno cambio
            Jugador jugador = juego.JugadorActual();

            if (comando == Comandos.TirarDados)
            {
                CartaEvento? cartaAntes = juego.Mazo.UltimaSacada; //para saber si en esta tirada salio una carta nueva
                if (juego.Dado.EsModoFisico() && !jugador.YaTiroDados) //con la Pico: avisa antes de esperar el boton fisico
                {
                    Difundir(Protocolo.Armar(Comandos.Mensaje, jugador.Nombre + ": presiona el boton del dado (15 s)")); //llega al registro de todos antes de la espera
                }
                if (!juego.TirarDados(id)) //validacion: lanzar dos veces
                {
                    Enviar(conexion, Protocolo.Armar(Comandos.Error, "Ya tiro los dados en este turno"));
                    return;
                }
                Casilla casilla = jugador.Posicion!.Dato;
                Difundir(Protocolo.Armar(Comandos.Dados, id.ToString(), juego.Dado.Obtener1().ToString(), juego.Dado.Obtener2().ToString()));
                CartaEvento? cartaNueva = juego.Mazo.UltimaSacada; //ultima carta del mazo despues de tirar
                if (cartaNueva != null && cartaNueva != cartaAntes) //cayo en un Evento y salio una carta: se anuncia a todos
                {
                    Difundir(Protocolo.Armar(Comandos.Mensaje, jugador.Nombre + " saco una carta: " + cartaNueva.Descripcion));
                }
                Difundir(Protocolo.Armar(Comandos.Movimiento, id.ToString(), casilla.Id.ToString(), casilla.Nombre));
                Difundir(LineaCasilla(casilla)); //datos de la casilla para el panel de la interfaz

                if (!jugador.Activo) //quebro en esta jugada
                {
                    Difundir(Protocolo.Armar(Comandos.Mensaje, jugador.Nombre + " quedo eliminado"));
                }
                else
                {
                    Propiedad? propiedad = casilla as Propiedad; //null si no es propiedad
                    if (propiedad != null && propiedad.EstaDisponible()) //libre: se le ofrece solo a el
                    {
                        Enviar(conexion, Protocolo.Armar(Comandos.OfrecerCompra, propiedad.Nombre, propiedad.Precio.ToString()));
                    }
                }
            }
            else if (comando == Comandos.ComprarPropiedad)
            {
                if (!juego.ComprarPropiedadActual(id)) //validacion: sin saldo, con dueño o no es propiedad
                {
                    Enviar(conexion, Protocolo.Armar(Comandos.Error, "No se pudo comprar"));
                    return;
                }
                Difundir(LineaCasilla(jugador.Posicion!.Dato)); //la casilla ahora muestra al nuevo dueño
            }
            else if (comando == Comandos.NoComprar)
            {
                Propiedad? libre = jugador.Posicion!.Dato as Propiedad; //null si la casilla no es una propiedad
                if (!jugador.YaTiroDados || libre == null || !libre.EstaDisponible()) //validacion: solo se puede rechazar una compra que se ofrecio
                {
                    Enviar(conexion, Protocolo.Armar(Comandos.Error, "No hay compra pendiente"));
                    return;
                }
                Difundir(Protocolo.Armar(Comandos.Mensaje, jugador.Nombre + " no compro"));
            }
            else if (comando == Comandos.TerminarTurno)
            {
                if (!juego.TerminarTurno(id)) //validacion: no puede pasar sin tirar
                {
                    Enviar(conexion, Protocolo.Armar(Comandos.Error, "Primero debe tirar los dados"));
                    return;
                }
            }
            else
            {
                Enviar(conexion, Protocolo.Armar(Comandos.Error, "Comando desconocido"));
                return;
            }

            //despues de cada accion importante se actualiza a todos
            DifundirTransaccionesNuevas(transaccionesAntes);
            DifundirEstado();
            if (juego.Terminado) AnunciarFin();
            else if (juego.NumeroTurno != turnoAntes) AnunciarTurno(); //termino su turno o quebro
        }

        private void DifundirTransaccionesNuevas(int desde) //avisa las transacciones que genero la ultima accion
        {
            int nuevas = juego.Historial.Contar() - desde;
            if (nuevas <= 0) return;
            NodoDoble<Transaccion> actual = juego.Historial.Ultimo()!;
            int i = 1;
            while (i < nuevas) //retrocede con Anterior hasta la primera transaccion nueva
            {
                actual = actual.Anterior!;
                i++;
            }
            NodoDoble<Transaccion>? nodo = actual;
            while (nodo != null) //y de ahi avanza hasta la ultima
            {
                Transaccion t = nodo.Dato;
                Difundir(Protocolo.Armar(Comandos.Mensaje, t.Origen + " -> " + t.Destino + ": " + t.Monto + " (" + t.Descripcion + ")"));
                nodo = nodo.Siguiente;
            }
        }

        private string LineaJugador(Jugador j) //arma JUGADOR|id|nombre|saldo|casilla|activo
        {
            int casilla = j.Posicion == null ? 0 : j.Posicion.Dato.Id;
            return Protocolo.Armar(Comandos.EstadoJugador, j.Id.ToString(), j.Nombre, j.Saldo.ToString(), casilla.ToString(), j.Activo ? "1" : "0");
        }

        private string LineaCasilla(Casilla casilla) //arma CASILLA|nombre|dueño|precio|alquiler para el panel de la interfaz
        {
            Propiedad? propiedad = casilla as Propiedad; //solo las propiedades tienen dueño y precios
            if (propiedad == null) return Protocolo.Armar(Comandos.Casilla, casilla.Nombre, "No se vende", "0", "0");
            string duenio = propiedad.Propietario == null ? "Libre" : propiedad.Propietario.Nombre;
            return Protocolo.Armar(Comandos.Casilla, casilla.Nombre, duenio, propiedad.Precio.ToString(), propiedad.Alquiler.ToString());
        }

        private void DifundirEstado() //manda el estado de los jugadores inscritos a todos
        {
            for (int id = 1; id <= MaxJugadores; id++) //recorre los 4 numeros posibles; los vacios se saltan
            {
                Jugador? j = juego.ObtenerJugador(id);
                if (j != null) Difundir(LineaJugador(j));
            }
        }

        private void EnviarEstado(ConexionCliente conexion) //manda el estado de los jugadores inscritos solo a quien lo pidio
        {
            for (int id = 1; id <= MaxJugadores; id++) //recorre los 4 numeros posibles; los vacios se saltan
            {
                Jugador? j = juego.ObtenerJugador(id);
                if (j != null) Enviar(conexion, LineaJugador(j));
            }
        }

        private void EnviarHistorial(ConexionCliente conexion) //manda el historial completo, una linea por transaccion
        {
            if (juego.Historial.EstaVacia()) { Enviar(conexion, Protocolo.Armar(Comandos.Mensaje, "Todavia no hay transacciones")); return; } //avisa en vez de no responder
            NodoDoble<Transaccion>? actual = juego.Historial.Primero();
            while (actual != null)
            {
                Enviar(conexion, Protocolo.Armar(Comandos.Transaccion, actual.Dato.ATexto()));
                actual = actual.Siguiente;
            }
        }

        private void AnunciarTurno() //avisa a todos de quien es el turno
        {
            Jugador j = juego.JugadorActual();
            Difundir(Protocolo.Armar(Comandos.Turno, j.Id.ToString(), j.Nombre));
        }

        private void AnunciarFin() //anuncia al ganador y exporta el reporte de la partida
        {
            Jugador? ganador = juego.Ganador();
            Difundir(Protocolo.Armar(Comandos.Fin, ganador == null ? "nadie" : ganador.Nombre));
            bool exportado = ReporteTransacciones.Exportar(juego.Historial, "docs/transacciones/reporte_partida.txt");
            Console.WriteLine("[Servidor] Reporte exportado: " + exportado);
        }

        private void Enviar(ConexionCliente conexion, string mensaje) //manda un mensaje a un solo jugador
        {
            try
            {
                conexion.Escritor.WriteLine(mensaje);
                Console.WriteLine("[Servidor] -> J" + conexion.Id + ": " + mensaje);
            }
            catch (Exception) //ese cliente ya no esta; su propio hilo se encarga de limpiarlo
            {
            }
        }

        private void Difundir(string mensaje) //manda el mismo mensaje a todos los jugadores conectados
        {
            for (int i = 0; i < conexiones.Length; i++) //recorre los 4 espacios
            {
                ConexionCliente? conexion = conexiones[i];
                if (conexion != null) //espacio ocupado
                {
                    Enviar(conexion, mensaje);
                }
            }
        }
    }
}