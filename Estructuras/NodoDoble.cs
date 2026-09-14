//Andron

namespace Monopoly.Estructuras
{
    public class NodoDoble<T> //nodo de enlace doble: apunta al anterior y al siguiente
    {
        public T Dato { get; set; } //valor almacenado en el nodo
        public NodoDoble<T>? Anterior { get; set; } //referencia al nodo previo
        public NodoDoble<T>? Siguiente { get; set; } //referencia al nodo que le sigue

        public NodoDoble(T dato) //crea un nodo suelto, sin enlazar
        {
            Dato = dato;
            Anterior = null;
            Siguiente = null;
        }
    }
}