//abigail

using System;
using System.Drawing;
using System.Windows.Forms;

namespace Monopoly.Interfaz
{
    public class MenuJugador : Form //lobby: IP, nombre, conexion y lista de jugadores antes de la partida
    {
        private TextBox txtIp = null!; //IP del servidor; los controles se crean en InicializarControles
        private TextBox txtNombre = null!; //nombre del jugador
        private Button btnConectar = null!; //pide la conexion
        private Label lblEstado = null!; //desconectado / conectando / conectado
        private Label lblLista = null!; //nombres de los conectados, uno por linea; sin colecciones nativas (regla 2.2)
        private Label lblContador = null!; //"n/4 listos"
        private Button btnIniciar = null!; //pasa al tablero cuando estan los 4

        private string?[] nombres = new string?[4]; //nombre de cada jugador por id (1 a 4 -> indice 0 a 3)
        private int cantidadListos = 0; //cuantos jugadores distintos se han anunciado

        public event Action<string, string>? AlConectar; //ip y nombre escritos por el jugador
        public event Action? AlIniciar; //el jugador pidio pasar al tablero

        public MenuJugador() //arma la ventana del lobby
        {
            Text = "Monopoly Tico";
            ClientSize = new Size(600, 800);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(252, 240, 218);
            Font = new Font("Segoe UI", 10);

            InicializarControles();
        }

        private void InicializarControles() //crea y acomoda todos los controles del lobby
        {
            //panel superior con el titulo
            Panel pnlHeader = new Panel();
            pnlHeader.Dock = DockStyle.Top;
            pnlHeader.Height = 120;
            pnlHeader.BackColor = Color.FromArgb(49, 91, 140); //azul vibrante
            pnlHeader.Padding = new Padding(30);

            Label lblTitulo = new Label();
            lblTitulo.Text = "Monopoly Tico";
            lblTitulo.Font = new Font("Segoe UI", 24, FontStyle.Bold);
            lblTitulo.ForeColor = Color.White;
            lblTitulo.AutoSize = true;
            lblTitulo.Location = new Point(30, 20);
            pnlHeader.Controls.Add(lblTitulo);

            Label lblSubtitulo = new Label();
            lblSubtitulo.Text = "Ingresa tu informacion y espera a los demas jugadores";
            lblSubtitulo.Font = new Font("Segoe UI", 10);
            lblSubtitulo.ForeColor = Color.FromArgb(200, 220, 255);
            lblSubtitulo.AutoSize = true;
            lblSubtitulo.Location = new Point(30, 70);
            pnlHeader.Controls.Add(lblSubtitulo);

            Controls.Add(pnlHeader);

            //panel principal con los campos
            Panel pnlMain = new Panel();
            pnlMain.Dock = DockStyle.Fill;
            pnlMain.BackColor = Color.FromArgb(20, 33, 61);
            pnlMain.Padding = new Padding(30);

            //fila 1: IP
            Label lblIp = new Label();
            lblIp.Text = "IP del servidor";
            lblIp.ForeColor = Color.FromArgb(200, 220, 255);
            lblIp.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            lblIp.AutoSize = true;
            lblIp.Location = new Point(30, 30);
            pnlMain.Controls.Add(lblIp);

            txtIp = new TextBox();
            txtIp.Location = new Point(30, 60);
            txtIp.Size = new Size(540, 40);
            txtIp.Text = "127.0.0.1";
            txtIp.Font = new Font("Segoe UI", 11);
            txtIp.BackColor = Color.FromArgb(40, 50, 80);
            txtIp.ForeColor = Color.White;
            txtIp.BorderStyle = BorderStyle.FixedSingle;
            pnlMain.Controls.Add(txtIp);

            //fila 2: nombre
            Label lblNombre = new Label();
            lblNombre.Text = "Tu nombre";
            lblNombre.ForeColor = Color.FromArgb(200, 220, 255);
            lblNombre.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            lblNombre.AutoSize = true;
            lblNombre.Location = new Point(30, 135);
            pnlMain.Controls.Add(lblNombre);

            txtNombre = new TextBox();
            txtNombre.Location = new Point(30, 175);
            txtNombre.Size = new Size(540, 40);
            txtNombre.Font = new Font("Segoe UI", 11);
            txtNombre.BackColor = Color.FromArgb(40, 50, 80);
            txtNombre.ForeColor = Color.White;
            txtNombre.BorderStyle = BorderStyle.FixedSingle;
            pnlMain.Controls.Add(txtNombre);

            //boton conectar
            btnConectar = new Button();
            btnConectar.Text = "Conectar";
            btnConectar.Location = new Point(30, 230);
            btnConectar.Size = new Size(540, 45);
            btnConectar.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            btnConectar.BackColor = Color.FromArgb(76, 175, 80); //verde
            btnConectar.ForeColor = Color.White;
            btnConectar.FlatStyle = FlatStyle.Flat;
            btnConectar.FlatAppearance.BorderSize = 0;
            btnConectar.Cursor = Cursors.Hand;
            btnConectar.Click += BtnConectar_Click;
            pnlMain.Controls.Add(btnConectar);

            //estado de la conexion
            lblEstado = new Label();
            lblEstado.Text = "Desconectado";
            lblEstado.ForeColor = Color.FromArgb(244, 67, 54); //rojo
            lblEstado.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            lblEstado.AutoSize = true;
            lblEstado.Location = new Point(30, 280);
            pnlMain.Controls.Add(lblEstado);

            //separador visual
            Panel sep1 = new Panel();
            sep1.BackColor = Color.FromArgb(60, 80, 120);
            sep1.Height = 1;
            sep1.Location = new Point(30, 310);
            sep1.Size = new Size(540, 1);
            pnlMain.Controls.Add(sep1);

            //titulo de la lista de jugadores
            Label lblJugadores = new Label();
            lblJugadores.Text = "Jugadores conectados";
            lblJugadores.ForeColor = Color.FromArgb(200, 220, 255);
            lblJugadores.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            lblJugadores.AutoSize = true;
            lblJugadores.Location = new Point(30, 320);
            pnlMain.Controls.Add(lblJugadores);

            //lista de jugadores: un Label de varias lineas armado desde el arreglo nombres[]
            lblLista = new Label();
            lblLista.Location = new Point(30, 360);
            lblLista.Size = new Size(540, 150);
            lblLista.BackColor = Color.FromArgb(40, 50, 80);
            lblLista.ForeColor = Color.FromArgb(76, 175, 80);
            lblLista.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            lblLista.BorderStyle = BorderStyle.FixedSingle;
            lblLista.Padding = new Padding(8);
            pnlMain.Controls.Add(lblLista);

            //contador de jugadores listos
            lblContador = new Label();
            lblContador.Text = "0/4 listos";
            lblContador.ForeColor = Color.FromArgb(255, 235, 59); //amarillo
            lblContador.Font = new Font("Segoe UI", 13, FontStyle.Bold);
            lblContador.AutoSize = true;
            lblContador.Location = new Point(30, 525);
            pnlMain.Controls.Add(lblContador);

            //boton para pasar al tablero
            btnIniciar = new Button();
            btnIniciar.Text = "Iniciar partida";
            btnIniciar.Location = new Point(30, 560);
            btnIniciar.Size = new Size(540, 50);
            btnIniciar.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            btnIniciar.BackColor = Color.FromArgb(156, 39, 176); //purpura
            btnIniciar.ForeColor = Color.White;
            btnIniciar.FlatStyle = FlatStyle.Flat;
            btnIniciar.FlatAppearance.BorderSize = 0;
            btnIniciar.Enabled = false; //se habilita cuando estan los 4
            btnIniciar.Cursor = Cursors.Hand;
            btnIniciar.Click += BtnIniciar_Click;
            pnlMain.Controls.Add(btnIniciar);

            Controls.Add(pnlMain);
            pnlMain.BringToFront(); //el panel Fill se acomoda al final: ocupa lo que deja el encabezado y no queda tapado
        }

        private void BtnConectar_Click(object? sender, EventArgs e) //valida los datos y avisa con AlConectar
        {
            if (string.IsNullOrWhiteSpace(txtIp.Text) || string.IsNullOrWhiteSpace(txtNombre.Text)) //faltan datos
            {
                MessageBox.Show("Ingresa la IP y tu nombre", "Falta informacion", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            btnConectar.Enabled = false; //evita conectar dos veces
            lblEstado.Text = "Conectando...";
            lblEstado.ForeColor = Color.FromArgb(255, 193, 7); //naranja

            AlConectar?.Invoke(txtIp.Text, txtNombre.Text);
        }

        public void ConexionConfirmada() //muestra conectado; se puede llamar desde el hilo de red
        {
            if (InvokeRequired) //viene del hilo de red: se pasa al hilo de la ventana
            {
                Invoke(new Action(ConexionConfirmada));
                return;
            }

            lblEstado.Text = "Conectado ✓";
            lblEstado.ForeColor = Color.FromArgb(76, 175, 80); //verde
        }

        public void ConexionFallida() //muestra el error y vuelve a habilitar Conectar
        {
            if (InvokeRequired) //viene del hilo de red: se pasa al hilo de la ventana
            {
                Invoke(new Action(ConexionFallida));
                return;
            }

            lblEstado.Text = "Error de conexion";
            lblEstado.ForeColor = Color.FromArgb(244, 67, 54); //rojo
            btnConectar.Enabled = true; //permite reintentar
        }

        public void ActualizarJugador(int id, string nombre) //agrega o actualiza un jugador en la lista
        {
            if (InvokeRequired) //viene del hilo de red: se pasa al hilo de la ventana
            {
                Invoke(new Action(() => ActualizarJugador(id, nombre)));
                return;
            }

            int indice = id - 1; //id de 1 a 4 -> indice de 0 a 3
            bool esNuevo = nombres[indice] == null; //solo cuenta la primera vez que aparece
            nombres[indice] = nombre;

            if (esNuevo)
                cantidadListos++;

            RedibujarLista();
        }

        private void RedibujarLista() //arma el texto de la lista recorriendo el arreglo nombres[]
        {
            string texto = "";
            int i = 0;
            while (i < 4) //los 4 espacios posibles, sin foreach
            {
                if (nombres[i] != null) texto += "✓ " + nombres[i] + Environment.NewLine;
                i++;
            }
            lblLista.Text = texto;

            lblContador.Text = cantidadListos + "/4 listos";
            btnIniciar.Enabled = (cantidadListos == 4); //solo se puede pasar al tablero con los 4

            if (cantidadListos == 4)
                btnIniciar.BackColor = Color.FromArgb(103, 58, 183); //purpura mas brillante
        }

        private void BtnIniciar_Click(object? sender, EventArgs e) //avisa con AlIniciar que hay que pasar al tablero
        {
            AlIniciar?.Invoke();
        }
    }
}