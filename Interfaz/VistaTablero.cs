//abigail

using System;
using System.Drawing;
using System.Windows.Forms;

namespace Monopoly.Interfaz
{
    public class VistaTablero : Form //ventana del tablero: imagen de fondo, fichas, jugadores y ultima casilla visitada
    {
        private const int TamanoFicha = 28; //lado en pixeles de cada ficha dibujada
        private const int DesfaseFicha = 30; //separacion entre fichas en la misma casilla (cuadricula 2x2)

        private Panel contenedor = null!; //permite hacer scroll sobre la imagen grande; se crea en InicializarLienzo
        private PictureBox lienzo = null!; //donde se dibuja la imagen y las fichas
        private Image fondoTablero = null!; //imagen de fondo, cargada una sola vez
        private Point[] coordenadas = null!; //presentacion: pixel de cada casilla, NO es el tablero real
        private Image[] imagenesJugador = new Image[4]; //ficha PNG de cada jugador

        //colores de las etiquetas, en el mismo orden que las fichas (azul, naranja, verde, rosa)
        private static readonly Color[] coloresJugador = { Color.RoyalBlue, Color.DarkOrange, Color.Green, Color.DeepPink };

        //datos de los 4 jugadores, llenados por ActualizarJugador; nunca por objetos Jugador,
        //porque el cliente solo recibe texto del servidor (regla 2.5)
        private string?[] nombres = new string?[4];
        private int[] saldos = new int[4];
        private int[] casillas = new int[4]; //numero de casilla de cada jugador
        private bool[] activos = new bool[4]; //false: no se dibuja su ficha

        private Label[] etiquetasJugador = new Label[4]; //nombre y saldo en el panel derecho

        private Label lblCasillaNombre = null!; //panel de la ultima casilla visitada
        private Label lblCasillaPropiedad = null!; //dueño de la casilla
        private Label lblCasillaCompra = null!; //precio de compra
        private Label lblCasillaAlquiler = null!; //precio de alquiler
        private Button btnComprar = null!; //compra la propiedad donde cayo el jugador
        private Button btnPasar = null!; //rechaza la compra

        public event Action? AlComprar; //el jugador apreto Comprar; quien lo use envia COMPRAR_PROPIEDAD
        public event Action? AlPasar; //el jugador apreto Pasar; quien lo use envia NO_COMPRAR

        public VistaTablero() //ya no recibe Jugador[]: ese objeto no existe del lado del cliente
        {
            this.DoubleBuffered = true; //evita parpadeo al redibujar

            InicializarCoordenadas();
            CargarImagenesJugadores();
            InicializarLienzo();
        }

        private void InicializarCoordenadas() //carga el mapa casilla -> pixel; SOLO es presentacion
        {
            coordenadas = new Point[32]; //arreglo nativo, no es la estructura del tablero
            //Cartago
            coordenadas[0] = new Point(87, 1514); //Salida
            coordenadas[1] = new Point(267, 1514);
            coordenadas[2] = new Point(440, 1514);
            coordenadas[3] = new Point(615, 1519); //corregida: mismo espaciado (~175 px) que el resto del lado
            coordenadas[4] = new Point(800, 1519);
            coordenadas[5] = new Point(975, 1519);
            coordenadas[6] = new Point(1150, 1519); //corregida: alineada con la casilla 10 y 18
            coordenadas[7] = new Point(1330, 1519); //corregida: alineada con la casilla 17
            //Alajuela
            coordenadas[8] = new Point(1519, 1519);
            coordenadas[9] = new Point(1514, 1342);
            coordenadas[10] = new Point(1514, 1150);
            coordenadas[11] = new Point(1514, 975);
            coordenadas[12] = new Point(1514, 800);
            coordenadas[13] = new Point(1514, 625);
            coordenadas[14] = new Point(1514, 442);
            coordenadas[15] = new Point(1514, 275);
            //Puntarenas
            coordenadas[16] = new Point(1500, 85);
            coordenadas[17] = new Point(1330, 85);
            coordenadas[18] = new Point(1150, 85);
            coordenadas[19] = new Point(975, 85);
            coordenadas[20] = new Point(800, 85);
            coordenadas[21] = new Point(615, 85);
            coordenadas[22] = new Point(440, 85);
            coordenadas[23] = new Point(270, 85);
            //Chepe
            coordenadas[24] = new Point(80, 85); //esquina Chepe (arriba a la izquierda)
            coordenadas[25] = new Point(80, 278);
            coordenadas[26] = new Point(80, 450);
            coordenadas[27] = new Point(80, 640);
            coordenadas[28] = new Point(80, 800);
            coordenadas[29] = new Point(80, 965);
            coordenadas[30] = new Point(80, 1150);
            coordenadas[31] = new Point(80, 1350); //Cerro Chirripo, justo arriba de la Salida
        }

        private void CargarImagenesJugadores() //carga las 4 fichas PNG una sola vez
        {
            imagenesJugador[0] = Image.FromFile("docs/bolaAzul.png");
            imagenesJugador[1] = Image.FromFile("docs/bolaNaranja.png");
            imagenesJugador[2] = Image.FromFile("docs/bolaVerde.png");
            imagenesJugador[3] = Image.FromFile("docs/bolaRosa.png");
        }

        private void InicializarLienzo() //arma el tablero con scroll y el panel derecho (jugadores + ultima casilla)
        {
            fondoTablero = Image.FromFile("docs/Tablero.jpeg");

            lienzo = new PictureBox();
            lienzo.Size = new Size(1600, 1600); //tamaño real de la imagen, sin reescalar
            lienzo.SizeMode = PictureBoxSizeMode.AutoSize; //respeta el tamaño real
            lienzo.Image = fondoTablero;
            lienzo.Paint += Lienzo_Paint; //aqui se dibujan las fichas, encima de la imagen

            contenedor = new Panel();
            contenedor.Dock = DockStyle.Fill;
            contenedor.AutoScroll = true; //permite moverse por la imagen de 1600x1600
            contenedor.Controls.Add(lienzo);

            //panel de jugadores: arriba en la columna derecha
            Panel panelJugadores = new Panel();
            panelJugadores.Dock = DockStyle.Top;
            panelJugadores.Height = 200;
            panelJugadores.BackColor = Color.White;

            int i = 0;
            while (i < 4) //crea las 4 etiquetas, una por jugador, sin foreach
            {
                Label etiqueta = new Label();
                etiqueta.AutoSize = true;
                etiqueta.Location = new Point(10, 10 + i * 30);
                etiqueta.ForeColor = coloresJugador[i]; //mismo color que su ficha
                etiqueta.Font = new Font(etiqueta.Font, FontStyle.Bold);
                etiqueta.Text = ""; //vacio hasta que llegue el primer ActualizarJugador
                panelJugadores.Controls.Add(etiqueta);
                etiquetasJugador[i] = etiqueta;
                i++;
            }

            //panel de la ultima casilla visitada: debajo de los jugadores
            Panel pnlCasilla = new Panel();
            pnlCasilla.Dock = DockStyle.Fill;
            pnlCasilla.BackColor = Color.FromArgb(245, 245, 245);
            pnlCasilla.BorderStyle = BorderStyle.FixedSingle;
            pnlCasilla.Padding = new Padding(15);

            lblCasillaNombre = new Label { Text = "Nombre: --", AutoSize = true, Location = new Point(15, 15) };
            pnlCasilla.Controls.Add(lblCasillaNombre);

            lblCasillaPropiedad = new Label { Text = "Propiedad: --", AutoSize = true, Location = new Point(15, 45) };
            pnlCasilla.Controls.Add(lblCasillaPropiedad);

            lblCasillaCompra = new Label { Text = "Precio Compra: $0", AutoSize = true, Location = new Point(15, 75) };
            pnlCasilla.Controls.Add(lblCasillaCompra);

            lblCasillaAlquiler = new Label { Text = "Precio Alquiler: $0", AutoSize = true, Location = new Point(15, 105) };
            pnlCasilla.Controls.Add(lblCasillaAlquiler);

            btnComprar = new Button { Text = "Comprar", Location = new Point(15, 145), Width = 75, Height = 35 };
            btnComprar.Click += BtnComprar_Click;
            pnlCasilla.Controls.Add(btnComprar);

            btnPasar = new Button { Text = "Pasar", Location = new Point(100, 145), Width = 75, Height = 35 };
            btnPasar.Click += BtnPasar_Click;
            pnlCasilla.Controls.Add(btnPasar);

            //columna derecha: jugadores arriba (Top) y casilla abajo (Fill)
            Panel pnlDerechaConCasilla = new Panel();
            pnlDerechaConCasilla.Dock = DockStyle.Right;
            pnlDerechaConCasilla.Width = 200; //ancho suficiente para Comprar y Pasar sin cortarse
            pnlDerechaConCasilla.Controls.Add(pnlCasilla);
            pnlDerechaConCasilla.Controls.Add(panelJugadores);

            this.Text = "Monopoly Tico";
            this.ClientSize = new Size(1080, 700);
            this.Controls.Add(contenedor);
            this.Controls.Add(pnlDerechaConCasilla);
        }

        public void ActualizarJugador(int id, string nombre, int saldo, int casilla, bool activo) //llamado al recibir JUGADOR|id|nombre|saldo|casilla|activo
        {
            if (this.InvokeRequired) //el hilo de red no puede tocar controles directamente (regla 14.1)
            {
                this.Invoke(new Action(() => ActualizarJugador(id, nombre, saldo, casilla, activo)));
                return;
            }

            int indice = id - 1; //id de jugador va de 1 a 4; los arreglos son de 0 a 3
            nombres[indice] = nombre;
            saldos[indice] = saldo;
            casillas[indice] = casilla;
            activos[indice] = activo;

            etiquetasJugador[indice].Text = activo ? (nombre + ": " + saldo) : (nombre + " (eliminado)");

            lienzo.Invalidate(); //fuerza a que Lienzo_Paint se ejecute de nuevo
        }

        public void ActualizarCasilla(string nombre, string propiedad, int precioCompra, int precioAlquiler) //llamado al recibir CASILLA|nombre|dueño|precio|alquiler
        {
            if (InvokeRequired) //viene del hilo de red: se pasa al hilo de la ventana
            {
                Invoke(new Action(() => ActualizarCasilla(nombre, propiedad, precioCompra, precioAlquiler)));
                return;
            }

            lblCasillaNombre.Text = "Nombre: " + nombre;
            lblCasillaPropiedad.Text = "Propiedad: " + propiedad;
            lblCasillaCompra.Text = "Precio Compra: $" + precioCompra;
            lblCasillaAlquiler.Text = "Precio Alquiler: $" + precioAlquiler;
        }

        private void BtnComprar_Click(object? sender, EventArgs e) //avisa afuera; la ventana no conoce la red
        {
            AlComprar?.Invoke();
        }

        private void BtnPasar_Click(object? sender, EventArgs e) //avisa afuera; la ventana no conoce la red
        {
            AlPasar?.Invoke();
        }

        private void Lienzo_Paint(object? sender, PaintEventArgs e) //dibuja las fichas sobre la imagen
        {
            int i = 0;
            while (i < 4) //recorre los 4 jugadores con while, no foreach
            {
                if (activos[i] && nombres[i] != null) //solo dibuja jugadores ya anunciados y activos
                {
                    Point p = coordenadas[casillas[i]];
                    int offsetX = (i % 2) * DesfaseFicha; //cuadricula 2x2 para que no se encimen
                    int offsetY = (i / 2) * DesfaseFicha;

                    e.Graphics.DrawImage(imagenesJugador[i], p.X + offsetX, p.Y + offsetY, TamanoFicha, TamanoFicha);
                }
                i++;
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e) //libera las imagenes para no dejar los archivos bloqueados
        {
            int i = 0;
            while (i < 4)
            {
                if (imagenesJugador[i] != null) imagenesJugador[i].Dispose();
                i++;
            }
            fondoTablero.Dispose();
            base.OnFormClosed(e);
        }
    }
}