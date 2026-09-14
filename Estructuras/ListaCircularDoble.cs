//abigail
//tablero
namespace Monopoly.Estructuras
{
    public class ListaCircularDoble<T> //lista circular doblemente enlazada; estructura del tablero
    {
        private NodoDoble<T>? cabeza; //primer nodo; su Anterior es el ultimo de la lista
        private int cantidad; //numero de nodos enlazados

        public ListaCircularDoble() //inicializa la lista vacia
        {
            cabeza = null;
            cantidad = 0;
        }

        public void Agregar(T dato) { } //inserta al final y vuelve a cerrar el circulo

        public NodoDoble<T> ObtenerNodo(int indice) { return default!; } //devuelve el nodo en la posicion dada

        public NodoDoble<T> Avanzar(NodoDoble<T> desde, int pasos) { return default!; } //recorre nodo por nodo hacia Siguiente

        public NodoDoble<T> Retroceder(NodoDoble<T> desde, int pasos) { return default!; } //recorre nodo por nodo hacia Anterior

        public int Contar() { return cantidad; } //cantidad de nodos en la lista

        public bool EstaVacia() { return cantidad == 0; } //true si la lista no tiene nodos
    }
}