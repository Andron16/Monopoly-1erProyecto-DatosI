//abigail

using System;
using System.Drawing;
using System.Windows.Forms;

namespace Monopoly.Interfaz
{
    public class VistaTablero : Form
    {
        private Panel contenedor; //permite hacer scroll sobre la imagen grande
        private PictureBox lienzo; //donde se dibuja la imagen y las fichas
        private Image fondoTablero; //imagen de fondo, cargada una sola vez
        private Point[] coordenadas; //presentacion: pixel de cada casilla, NO es el tablero real
        private Image[] imagenesJugador = new Image[4];

        //datos de los 4 jugadores, llenados por ActualizarJugador; nunca por objetos Jugador,
        //porque el cliente solo recibe texto del servidor (regla 2.5)
        private string?[] nombres = new string?[4];
        private int[] saldos = new int[4];
        private int[] casillas = new int[4]; //numero de casilla de cada jugador
        private bool[] activos = new bool[4]; //false: no se dibuja su ficha

        private Label[] etiquetasJugador = new Label[4]; //nombre y saldo en un panel lateral

        private Label lblCasillaNombre;
        private Label lblCasillaPropiedad;
        private Label lblCasillaCompra;
        private Label lblCasillaAlquiler;
        private Button btnComprar;
        private Button btnPasar;

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
            coordenadas[3] = new Point(662, 1519);
            coordenadas[4] = new Point(800, 1519);
            coordenadas[5] = new Point(975, 1519);
            coordenadas[6] = new Point(1160, 1519);
            coordenadas[7] = new Point(1325, 1519);
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
            coordenadas[24] = new Point(80, 278);
            coordenadas[25] = new Point(80, 450);
            coordenadas[26] = new Point(80, 640);
            coordenadas[27] = new Point(80, 800);
            coordenadas[28] = new Point(80, 965);
            coordenadas[29] = new Point(80, 1150);
            coordenadas[30] = new Point(80, 1350);
            coordenadas[31] = new Point(80, 1515);
        }
        private void CargarImagenesJugadores()
        {
            imagenesJugador[0] = Image.FromFile("docs/bolaAzul.png");
            imagenesJugador[1] = Image.FromFile("docs/bolaNaranja.png");
            imagenesJugador[2] = Image.FromFile("docs/bolaVerde.png");
            imagenesJugador[3] = Image.FromFile("docs/bolaRosa.png");
        }

        private void InicializarLienzo()
        {
            fondoTablero = Image.FromFile("docs/Tablero.jpeg");

            lienzo = new PictureBox();
            lienzo.Size = new Size(1600, 1600);
            lienzo.SizeMode = PictureBoxSizeMode.AutoSize;
            lienzo.Image = fondoTablero;
            lienzo.Paint += Lienzo_Paint;

            contenedor = new Panel();
            contenedor.Dock = DockStyle.Fill;
            contenedor.AutoScroll = true;
            contenedor.Controls.Add(lienzo);

            Panel panelJugadores = new Panel();
            panelJugadores.Dock = DockStyle.Top;
            panelJugadores.Height = 200;
            panelJugadores.BackColor = Color.White;

            int i = 0;
            while (i < 4)
            {
                Label etiqueta = new Label();
                etiqueta.AutoSize = true;
                etiqueta.Location = new Point(10, 10 + i * 30);
                etiqueta.Text = "";
                panelJugadores.Controls.Add(etiqueta);
                etiquetasJugador[i] = etiqueta;
                i++;
            }

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

            Panel pnlDerechaConCasilla = new Panel();
            pnlDerechaConCasilla.Dock = DockStyle.Right;
            pnlDerechaConCasilla.Width = 180;
            pnlDerechaConCasilla.Controls.Add(pnlCasilla);
            pnlDerechaConCasilla.Controls.Add(panelJugadores);
            

            this.Text = "Monopoly Tico";
            this.ClientSize = new Size(1080, 700);
            this.Controls.Add(contenedor);
            this.Controls.Add(pnlDerechaConCasilla);
        }


        public void ActualizarJugador(int id, string nombre, int saldo, int casilla, bool activo) //llamado por Red/Cliente al recibir JUGADOR|id|nombre|saldo|casilla|activo
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

        public void ActualizarCasilla(string nombre, string propiedad, int precioCompra, int precioAlquiler)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => ActualizarCasilla(nombre, propiedad, precioCompra, precioAlquiler)));
                return;
            }

            lblCasillaNombre.Text = "Nombre: " + nombre;
            lblCasillaPropiedad.Text = "Propiedad: " + propiedad;
            lblCasillaCompra.Text = "Precio Compra: $" + precioCompra;
            lblCasillaAlquiler.Text = "Precio Alquiler: $" + precioAlquiler;
        }

        private void BtnComprar_Click(object? sender, EventArgs e)
        {
            //enviar comando al servidor
        }

        private void BtnPasar_Click(object? sender, EventArgs e)
        {
            //enviar comando al servidor
        }

        private void Lienzo_Paint(object? sender, PaintEventArgs e) //dibuja las fichas sobre la imagen
        {
            int i = 0;
            while (i < 4) //recorre los 4 jugadores con while, no foreach
            {
                if (activos[i] && nombres[i] != null) //solo dibuja jugadores ya anunciados y activos
                {
                    Point p = coordenadas[casillas[i]];
                    int offsetX = (i % 2) * 12;
                    int offsetY = (i / 2) * 12;

                    e.Graphics.DrawImage(imagenesJugador[i], p.X + offsetX, p.Y + offsetY, 64, 64);
                }
                i++;
            }
        }
    }
}