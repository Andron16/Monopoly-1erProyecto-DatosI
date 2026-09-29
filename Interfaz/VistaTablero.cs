//abigail

using System;
using System.Drawing;
using System.Windows.Forms;
using Monopoly.Dominio;

namespace Monopoly.Interfaz
{
    public class VistaTablero : Form
    {
        private Panel contenedor; //permite hacer scroll sobre la imagen grande
        private PictureBox lienzo; //donde se dibuja la imagen y las fichas
        private Image fondoTablero; //imagen de fondo, cargada una sola vez
        private Point[] coordenadas; //presentacion: pixel de cada casilla, NO es el tablero real
        private Jugador[] jugadores; //referencia a los 4 jugadores para saber donde dibujarlos

        private static readonly Color[] coloresJugador = { Color.Red, Color.Blue, Color.Green, Color.Orange };

        public VistaTablero(Jugador[] jugadores)
        {
            this.jugadores = jugadores;
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
            coordenadas[5] = new Point(975,1519 );
            coordenadas[6] = new Point(1160,1519 );
            coordenadas[7] = new Point(1325,1519 );
            //Alajuela
            coordenadas[8] = new Point(1519,1519 );
            coordenadas[9] = new Point(1514,1342 );
            coordenadas[10] = new Point( 1514,1150 );
            coordenadas[11] = new Point( 1514,975 );
            coordenadas[12] = new Point( 1514, 800 );
            coordenadas[13] = new Point( 1514, 625);
            coordenadas[14] = new Point( 1514, 442);
            coordenadas[15] = new Point( 1514, 275);
            //Puntarenas
            coordenadas[16] = new Point( 1500, 85);
            coordenadas[17] = new Point( 1330, 85);
            coordenadas[18] = new Point( 1150, 85);
            coordenadas[19] = new Point( 975, 85);
            coordenadas[20] = new Point( 800, 85);
            coordenadas[21] = new Point( 615, 85);
            coordenadas[22] = new Point( 440, 85);
            coordenadas[23] = new Point( 270, 85);
            //Chepe
            coordenadas[24] = new Point( 80, 278);
            coordenadas[25] = new Point( 80, 450);
            coordenadas[26] = new Point( 80, 640);
            coordenadas[27] = new Point( 80, 800);
            coordenadas[28] = new Point( 80, 965);
            coordenadas[29] = new Point( 80, 1150);
            coordenadas[30] = new Point( 80, 1350);
            coordenadas[31] = new Point( 80, 1515);

        }

        private void InicializarLienzo() //arma el PictureBox con la imagen de fondo
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

            this.Controls.Add(contenedor);
            this.Text = "Monopoly Distribuido";
            this.ClientSize = new Size(900, 700);
        }

        private void Lienzo_Paint(object? sender, PaintEventArgs e) //dibuja las fichas sobre la imagen
        {
            int i = 0;
            while (i < jugadores.Length) //recorre los 4 jugadores con while, no foreach
            {
                Jugador jugador = jugadores[i];
                if (jugador.Activo && jugador.Posicion != null)
                {
                    int idCasilla = jugador.Posicion.Dato.Id;
                    Point p = coordenadas[idCasilla]; //coordenada tal cual la mediste, sin conversion

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

        public void ActualizarPosiciones() //llamar esto cuando llegue un mensaje del servidor
        {
            if (this.InvokeRequired) //el hilo de red no puede tocar el control directamente
            {
                this.Invoke(new Action(ActualizarPosiciones));
                return;
            }
            lienzo.Invalidate(); //fuerza a que Lienzo_Paint se ejecute de nuevo
        }
    }
}