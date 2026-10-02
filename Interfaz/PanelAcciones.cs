//andron

using System;
using System.Drawing;
using System.Windows.Forms;
using Monopoly.Red;

namespace Monopoly.Interfaz
{
    public class PanelAcciones : Panel //panel inferior del tablero: turno, botones del jugador y mensajes del servidor
    {
        private Label etiquetaTurno; //"Es tu turno" o "Turno de X"
        private TextBox registro; //mensajes del servidor, solo lectura

        public PanelAcciones(ControladorCliente controlador) //arma el panel y lo engancha al controlador
        {
            this.Dock = DockStyle.Bottom;
            this.Height = 150;

            etiquetaTurno = new Label();
            etiquetaTurno.AutoSize = true;
            etiquetaTurno.Location = new Point(10, 8);
            etiquetaTurno.Font = new Font(etiquetaTurno.Font, FontStyle.Bold); //resalta de quien es el turno
            etiquetaTurno.Text = "Esperando el primer turno...";
            this.Controls.Add(etiquetaTurno);

            //cada boton solo llama al controlador; el servidor decide si la accion es valida
            AgregarBoton("Tirar dados", 10, (s, e) => controlador.TirarDados());
            AgregarBoton("Terminar turno", 140, (s, e) => controlador.TerminarTurno());
            AgregarBoton("Transacciones", 270, (s, e) => controlador.PedirTransacciones()); //Comprar y Pasar estan en el panel de casilla de VistaTablero

            registro = new TextBox();
            registro.Multiline = true;
            registro.ReadOnly = true;
            registro.ScrollBars = ScrollBars.Vertical;
            registro.Dock = DockStyle.Bottom; //parte baja del panel, a todo lo ancho
            registro.Height = 80;
            this.Controls.Add(registro);
        }

        private void AgregarBoton(string texto, int x, EventHandler alPulsar) //crea un boton en la fila de acciones
        {
            Button boton = new Button();
            boton.Text = texto;
            boton.Location = new Point(x, 32);
            boton.Size = new Size(120, 28);
            boton.Click += alPulsar;
            this.Controls.Add(boton);
        }

        public void MostrarTurno(string texto) //actualiza la etiqueta de turno desde cualquier hilo
        {
            EnElHiloDeLaVentana(() => etiquetaTurno.Text = texto);
        }

        public void Registrar(string texto) //agrega una linea al registro desde cualquier hilo
        {
            EnElHiloDeLaVentana(() => registro.AppendText(texto + Environment.NewLine));
        }

        private void EnElHiloDeLaVentana(Action accion) //los avisos llegan del hilo de red; Windows Forms exige Invoke (regla 14.1)
        {
            if (this.InvokeRequired) this.Invoke(accion);
            else accion();
        }
    }
}