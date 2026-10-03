//abigail

using Monopoly.Estructuras;

namespace Monopoly.Dominio
{
    public class Tablero //envuelve la lista circular doble que contiene las casillas
    {
        private ListaCircularDoble<Casilla> casillas; //estructura real del tablero

        public Tablero() //inicializa el tablero vacio
        {
            casillas = new ListaCircularDoble<Casilla>();
        }

        public void Construir() //crea y enlaza las 32 casillas de la partida
        {
            // ---- Lado Cartago ----
            AgregarCasilla(new CasillaEspecial(0, "SalidaCartago", TipoEspecial.Salida, 100)); //el premio real lo paga Juego.PremioSalida
            AgregarCasilla(new CasillaEspecial(1, "Dinero", TipoEspecial.Dinero, 75));
            AgregarCasilla(new Propiedad(2, "Palmares", 60, 50)); //precio, alquiler (alquileres x5 para que haya quiebras)
            AgregarCasilla(new Propiedad(3, "Museo", 60, 50));
            AgregarCasilla(new Propiedad(4, "Fuente", 100, 100));
            AgregarCasilla(new Propiedad(5, "Teletica", 80, 75));
            AgregarCasilla(new CasillaEvento(6, "Evento"));
            AgregarCasilla(new Propiedad(7, "Zapote", 80, 75));

            // ---- LADO Alajuela ----
            AgregarCasilla(new CasillaEspecial(8, "Alajuela", TipoEspecial.Provincia, 0));
            AgregarCasilla(new CasillaEspecial(9, "Carcel", TipoEspecial.Carcel, 50)); //50 = fianza que se paga al banco
            AgregarCasilla(new Propiedad(10, "Jaco", 100, 100));
            AgregarCasilla(new Propiedad(11, "Isla", 120, 125));
            AgregarCasilla(new CasillaEvento(12, "Evento"));
            AgregarCasilla(new Propiedad(13, "Golfito", 120, 125));
            AgregarCasilla(new Propiedad(14, "Faro", 140, 150));
            AgregarCasilla(new Propiedad(15, "Caldera", 140, 150));

            // ---- Lado Puntarenas ----
            AgregarCasilla(new CasillaEspecial(16, "Puntarenas", TipoEspecial.Provincia, 0));
            AgregarCasilla(new Propiedad(17, "La Carpio", 160, 175));
            AgregarCasilla(new CasillaEspecial(18, "Loteria", TipoEspecial.Loteria, 0));
            AgregarCasilla(new Propiedad(19, "Desampa", 160, 175));
            AgregarCasilla(new Propiedad(20, "Mercado Central", 180, 200));
            AgregarCasilla(new CasillaEvento(21, "Evento"));
            AgregarCasilla(new Propiedad(22, "Mall", 180, 200));
            AgregarCasilla(new Propiedad(23, "Cali", 200, 225));

            // ---- LADO Chepe ----
            AgregarCasilla(new CasillaEspecial(24, "Chepe", TipoEspecial.Provincia, 0));
            AgregarCasilla(new Propiedad(25, "Estadio", 200, 225));
            AgregarCasilla(new Propiedad(26, "Sabana", 220, 250));
            AgregarCasilla(new Propiedad(27, "Casa", 220, 250));
            AgregarCasilla(new Propiedad(28, "Sanatorio", 240, 275));
            AgregarCasilla(new CasillaEspecial(29, "Regalo", TipoEspecial.Regalo, 100));
            AgregarCasilla(new CasillaEvento(30, "Evento"));
            AgregarCasilla(new Propiedad(31, "Cerro", 240, 275));
        }

        public void AgregarCasilla(Casilla casilla)
        {
            casilla.Id = casillas.Contar(); //la posicion en el circuito define el numero: 0, 1, 2...
            casillas.Agregar(casilla);
        } //añade una casilla al final del circuito

        public NodoDoble<Casilla> ObtenerNodo(int indice) { return casillas.ObtenerNodo(indice); } //nodo de la casilla en la posicion dada

        public Casilla ObtenerCasilla(int indice)
        {
            return casillas.ObtenerNodo(indice).Dato;
        } //casilla en la posicion dada

        public NodoDoble<Casilla> Mover(NodoDoble<Casilla> desde, int pasos) { return casillas.Avanzar(desde, pasos); } //recorre el tablero nodo por nodo

        public NodoDoble<Casilla> Retroceder(NodoDoble<Casilla> desde, int pasos) { return casillas.Retroceder(desde, pasos); } //recorre hacia atras con Anterior

        public bool PasoPorSalida(NodoDoble<Casilla> desde, int pasos)
        {
            NodoDoble<Casilla> actual = desde;
            bool cruzo = false;
            while (pasos > 0) //revisa cada casilla intermedia, no solo la final
            {
                actual = actual.Siguiente!;
                if (actual.Dato.Id == 0) cruzo = true;
                pasos--;
            }
            return cruzo;
        } //true si el recorrido cruza la casilla de salida

        public int Contar() { return casillas.Contar(); } //cantidad de casillas del tablero
    }
}