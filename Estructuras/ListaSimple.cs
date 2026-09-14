//palma
//propiedades del jugador

namespace Monopoly.Estructuras
{
    public class ListaSimple<T> //lista enlazada simple; almacena las propiedades de un jugador
    {
        private Nodo<T>? cabeza; //primer nodo de la lista
        private int cantidad; //numero de elementos almacenados

        public ListaSimple() //inicializa la lista vacia
        {
            cabeza = null;
            cantidad = 0;
        }

        public void Agregar(T dato) { } //inserta un elemento al final de la lista

        public bool Eliminar(T dato) { return false; } //quita la primera coincidencia; true si la encontro

        public bool Contiene(T dato) { return false; } //indica si el elemento esta en la lista

        public T Obtener(int indice) { return default!; } //devuelve el elemento en la posicion dada

        public int Contar() { return cantidad; } //cantidad de elementos en la lista

        public bool EstaVacia() { return cantidad == 0; } //true si la lista no tiene elementos
    }
}