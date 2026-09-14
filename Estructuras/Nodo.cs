//andron

namespace Monopoly.Estructuras
{
    public class Nodo<T> //nodo de enlace simple: guarda un dato y apunta al siguiente
    {
        public T Dato { get; set; } //valor almacenado en el nodo
        public Nodo<T>? Siguiente { get; set; } //referencia al nodo que le sigue

        public Nodo(T dato) //crea un nodo suelto, sin enlazar
        {
            Dato = dato;
            Siguiente = null;
        }
    }
}