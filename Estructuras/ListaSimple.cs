//palma
//propiedades del jugador

namespace Monopoly.Estructuras
{
    public class ListaSimple<T> //lista enlazada simple; almacena las propiedades de un jugador
    {
        private Nodo<T>? cabeza; //primer nodo de la lista
        private int cantidad;    //numero de elementos almacenados

        public ListaSimple() //inicializa la lista vacia
        {
            cabeza = null;
            cantidad = 0;
        }

        public void Agregar(T dato) //inserta un elemento al final de la lista
        {
            Nodo<T> nuevo = new Nodo<T>(dato); //crea el nodo a insertar
            if (cabeza == null) //lista vacia: el nuevo pasa a ser la cabeza
            {
                cabeza = nuevo;
            }
            else //hay elementos: recorre hasta el ultimo para engancharlo ahi
            {
                Nodo<T> actual = cabeza;
                while (actual.Siguiente != null) //avanza mientras exista un siguiente
                {
                    actual = actual.Siguiente;
                }
                actual.Siguiente = nuevo; //el ultimo nodo ahora apunta al nuevo
            }
            cantidad++; //un elemento mas
        }

        public bool Eliminar(T dato) //quita la primera coincidencia; true si la encontro
        {
            if (cabeza == null) return false; //lista vacia: nada que eliminar

            if (object.Equals(cabeza.Dato, dato)) //el dato esta en la cabeza
            {
                cabeza = cabeza.Siguiente; //la cabeza pasa a ser el segundo nodo
                cantidad--;
                return true;
            }

            Nodo<T> actual = cabeza;
            while (actual.Siguiente != null) //busca el nodo anterior al que hay que borrar
            {
                if (object.Equals(actual.Siguiente.Dato, dato)) //el siguiente es el que se elimina
                {
                    actual.Siguiente = actual.Siguiente.Siguiente; //se salta el nodo eliminado
                    cantidad--;
                    return true;
                }
                actual = actual.Siguiente;
            }
            return false; //no se encontro el dato
        }

        public bool Contiene(T dato) //indica si el elemento esta en la lista
        {
            Nodo<T>? actual = cabeza;
            while (actual != null) //recorre nodo por nodo
            {
                if (object.Equals(actual.Dato, dato)) return true; //coincidencia encontrada
                actual = actual.Siguiente;
            }
            return false; //no esta en la lista
        }

        public T Obtener(int indice) //devuelve el elemento en la posicion dada
        {
            if (indice < 0 || indice >= cantidad) //indice invalido
            {
                throw new System.IndexOutOfRangeException("Indice fuera de rango en ListaSimple");
            }
            Nodo<T> actual = cabeza!; //hay al menos indice+1 elementos, cabeza no es null
            int i = 0;
            while (i < indice) //avanza hasta la posicion pedida
            {
                actual = actual.Siguiente!; //el indice es valido, el siguiente existe
                i++;
            }
            return actual.Dato;
        }

        public int Contar() //cantidad de elementos en la lista
        {
            return cantidad;
        }

        public bool EstaVacia() //true si la lista no tiene elementos
        {
            return cantidad == 0;
        }
    }
}

