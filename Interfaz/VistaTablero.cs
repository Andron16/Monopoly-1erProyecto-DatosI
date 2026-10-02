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

        //datos de los 4 jugadores, llenados por ActualizarJugador; nunca por objetos Jugador,
        //porque el cliente solo recibe texto del servidor (regla 2.5)
        private string?[] nombres = new string?[4];
        private int[] saldos = new int[4];
        private int[] casillas = new int[4]; //numero de casilla de cada jugador
        private bool[] activos = new bool[4]; //false: no se dibuja su ficha

        private Label[] etiquetasJugador = new Label[4]; //nombre y saldo en un panel lateral

        private static readonly Color[] coloresJugador = { Color.Red, Color.Blue, Color.Green, Color.Orange };

        public VistaTablero() //ya no recibe Jugador[]: ese objeto no existe del lado del cliente
        {
            this.DoubleBuffered = true; //evita parpadeo al redibujar

            InicializarCoordenadas();
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

        private void InicializarLienzo() //arma el PictureBox con la imagen de fondo y el panel lateral de jugadores
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

            Panel panelJugadores = new Panel(); //lateral con nombre y saldo de cada jugador
            panelJugadores.Dock = DockStyle.Right;
            panelJugadores.Width = 180;

            int i = 0;
            while (i < 4) //crea las 4 etiquetas, una por jugador, sin foreach
            {
                Label etiqueta = new Label();
                etiqueta.AutoSize = true;
                etiqueta.Location = new Point(10, 10 + i * 30);
                etiqueta.ForeColor = coloresJugador[i];
                etiqueta.Text = ""; //vacio hasta que llegue el primer ActualizarJugador
                panelJugadores.Controls.Add(etiqueta);
                etiquetasJugador[i] = etiqueta;
                i++;
            }

            this.Controls.Add(contenedor);
            this.Controls.Add(panelJugadores);
            this.Text = "Monopoly Distribuido";
            this.ClientSize = new Size(1080, 700);
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

        private void Lienzo_Paint(object? sender, PaintEventArgs e) //dibuja las fichas sobre la imagen
        {
            int i = 0;
            while (i < 4) //recorre los 4 jugadores con while, no foreach
            {
                if (activos[i] && nombres[i] != null) //solo dibuja jugadores ya anunciados y activos
                {
                    Point p = coordenadas[casillas[i]]; //coordenada tal cual la mediste, sin conversion

                    //pequeño desfase por jugador para que no queden apiladas exactamente encima
                    int offsetX = (i % 2) * 12;
                    int offsetY = (i / 2) * 12;

                    Brush pincel = new SolidBrush(coloresJugador[i]);
                    e.Graphics.FillEllipse(pincel, p.X + offsetX, p.Y + offsetY, 14, 14);
                    pincel.Dispose();
                }
                i++;
            }
        }
    }
}