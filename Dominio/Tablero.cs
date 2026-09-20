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

        public void Construir() { } //crea y enlaza las 24 casillas de la partida

        public void AgregarCasilla(Casilla casilla) { } //añade una casilla al final del circuito

        public NodoDoble<Casilla> ObtenerNodo(int indice) { return default!; } //nodo de la casilla en la posicion dada

        public Casilla ObtenerCasilla(int indice) { return default!; } //casilla en la posicion dada

        public NodoDoble<Casilla> Mover(NodoDoble<Casilla> desde, int pasos) { return default!; } //recorre el tablero nodo por nodo

        public bool PasoPorSalida(NodoDoble<Casilla> desde, int pasos) { return false; } //true si el recorrido cruza la casilla de salida

        public int Contar() { return casillas.Contar(); } //cantidad de casillas del tablero
    }
}