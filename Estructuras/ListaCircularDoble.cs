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

        public void Agregar(T dato)  //inserta al final y vuelve a cerrar el circulo
        {
            NodoDoble<T> nuevo = new NodoDoble<T>(dato);
            if (cabeza == null)
            {
                cabeza = nuevo;
                nuevo.Siguiente = nuevo; //se apunta a si mismo, es el unico nodo
                nuevo.Anterior = nuevo;
            }
            else
            {
                NodoDoble<T> ultimo = cabeza.Anterior!; //el ultimo es el anterior de cabeza
                ultimo.Siguiente = nuevo;
                nuevo.Anterior = ultimo;
                nuevo.Siguiente = cabeza;
                cabeza.Anterior = nuevo; //se vuelve a cerrar el circulo
            }
            cantidad++;
        }

        public NodoDoble<T> ObtenerNodo(int indice)
        {
            NodoDoble<T> actual = cabeza!;
            while (indice > 0) //avanza nodo por nodo hasta llegar al indice
            {
                actual = actual.Siguiente!;
                indice--;
            }
            return actual;
        } //devuelve el nodo en la posicion dada

        public NodoDoble<T> Avanzar(NodoDoble<T> desde, int pasos)
        {
            NodoDoble<T> actual = desde;
            while (pasos > 0) //un paso real por iteracion, sin aritmetica de indices
            {
                actual = actual.Siguiente!;
                pasos--;
            }
            return actual;
        } //recorre nodo por nodo hacia Siguiente

        public NodoDoble<T> Retroceder(NodoDoble<T> desde, int pasos)
        {
            NodoDoble<T> actual = desde;
            while (pasos > 0)
            {
                actual = actual.Anterior!;
                pasos--;
            }
            return actual;
        } //recorre nodo por nodo hacia Anterior

        public int Contar() { return cantidad; } //cantidad de nodos en la lista

        public bool EstaVacia() { return cantidad == 0; } //true si la lista no tiene nodos
    }
}