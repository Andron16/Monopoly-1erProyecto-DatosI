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
            AgregarCasilla(new CasillaEspecial(0, "SalidaCartago", TipoEspecial.Salida, 200)); //Cantidad
            AgregarCasilla(new CasillaEspecial(1, "Dinero", TipoEspecial.Dinero, 75));
            AgregarCasilla(new Propiedad(2, "Palmares", 60, 10)); //Primer num precio, segundo num, alquiler
            AgregarCasilla(new Propiedad(3, "Museo", 60, 10));
            AgregarCasilla(new Propiedad(4, "Fuente", 100, 20));
            AgregarCasilla(new Propiedad(5, "Teletica", 80, 15));
            AgregarCasilla(new CasillaEvento(6, "Evento"));
            AgregarCasilla(new Propiedad(7, "Zapote", 80, 15));

            // ---- LADO Alajuela ----
            AgregarCasilla(new CasillaEspecial(8, "Alajuela", TipoEspecial.Provincia, 0));
            AgregarCasilla(new CasillaEspecial(9, "Carcel", TipoEspecial.Carcel, 0));
            AgregarCasilla(new Propiedad(10, "Jaco", 100, 20));
            AgregarCasilla(new Propiedad(11, "Isla", 120, 25));
            AgregarCasilla(new CasillaEvento(12, "Evento"));
            AgregarCasilla(new Propiedad(13, "Golfito", 120, 25));
            AgregarCasilla(new Propiedad(14, "Faro", 140, 30)); 
            AgregarCasilla(new Propiedad(15, "Caldera", 140, 30));

            // ---- Lado Puntarenas ----
            AgregarCasilla(new CasillaEspecial(16, "Puntarenas", TipoEspecial.Provincia, 0));
            AgregarCasilla(new Propiedad(17, "La Carpio", 160, 35));
            AgregarCasilla(new CasillaEspecial(18, "Loteria", TipoEspecial.Loteria, 0));
            AgregarCasilla(new Propiedad(19, "Desampa", 160, 35));
            AgregarCasilla(new Propiedad(20, "Mercado Central", 180, 40));
            AgregarCasilla(new CasillaEvento(21, "Evento"));
            AgregarCasilla(new Propiedad(22, "Mall", 180, 40));
            AgregarCasilla(new Propiedad(23, "Cali", 200, 45));

            // ---- LADO Chepe ----
            AgregarCasilla(new CasillaEspecial(24, "Chepe", TipoEspecial.Provincia, 0));
            AgregarCasilla(new Propiedad(25, "Estadio", 200, 45));
            AgregarCasilla(new Propiedad(26, "Sabana", 220, 50));
            AgregarCasilla(new Propiedad(27, "Casa", 220, 50));
            AgregarCasilla(new Propiedad(28, "Sanatorio", 240, 55));
            AgregarCasilla(new CasillaEspecial(29, "Regalo", TipoEspecial.Regalo, 100));
            AgregarCasilla(new CasillaEvento(30, "Evento"));
            AgregarCasilla(new Propiedad(31, "Cerro", 240, 55));
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