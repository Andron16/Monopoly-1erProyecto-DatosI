//andron
//historial de transacciones


namespace Monopoly.Estructuras
{
    public class ListaDoble<T> //lista doblemente enlazada; historial de transacciones
    {
        private NodoDoble<T>? cabeza; //primer nodo, la transaccion mas antigua
        private NodoDoble<T>? cola; //ultimo nodo, la transaccion mas reciente
        private int cantidad; //numero de elementos almacenados

        public ListaDoble() //inicializa la lista vacia
        {
            cabeza = null;
            cola = null;
            cantidad = 0;
        }

        public void Agregar(T dato) //inserta un elemento al final de la lista
        {
            NodoDoble<T> nuevo = new NodoDoble<T>(dato); //nuevo nodo con el dato recibido
            if (cola == null)
            {
                cabeza = nuevo; //el nodo nuevo es el primero
                cola = nuevo; //y tambien el ultimo
            }
            else
            {
                nuevo.Anterior = cola; //el nuevo mira hacia atras a la cola actual
                cola.Siguiente = nuevo; //la cola actual mira hacia adelante al nuevo
                cola = nuevo; //el nuevo pasa a ser la cola
            }
            cantidad++; //un elemento mas en la lista
        }

        public NodoDoble<T>? Primero() { return cabeza; } //nodo inicial, para recorrer del mas antiguo al mas reciente

        public NodoDoble<T>? Ultimo() { return cola; } //nodo final, para recorrer del mas reciente al mas antiguo

        public int Contar() { return cantidad; } //cantidad de elementos en la lista

        public bool EstaVacia() { return cantidad == 0; } //true si la lista no tiene elementos
    }
}