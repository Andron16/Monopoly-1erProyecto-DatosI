//abigail
//turnos

namespace Monopoly.Estructuras
{
    public class ColaCircular<T> //cola circular enlazada; administra los turnos de los jugadores
    {
        private Nodo<T>? frente; //nodo al que le corresponde el turno actual
        private Nodo<T>? final; //ultimo nodo; su Siguiente apunta al frente
        private int cantidad; //numero de elementos en la cola

        public ColaCircular() //inicializa la cola vacia
        {
            frente = null;
            final = null;
            cantidad = 0;
        }

        public void Encolar(T dato)
        {
            Nodo<T> nuevo = new Nodo<T>(dato);
            if (frente == null) //cola vacia: el nuevo es el unico nodo
            {
                frente = nuevo;
                final = nuevo;
                nuevo.Siguiente = nuevo; //se apunta a si mismo
            }
            else
            {
                final!.Siguiente = nuevo; //el final actual ahora apunta al nuevo
                nuevo.Siguiente = frente; //el nuevo cierra el circulo hacia el frente
                final = nuevo; //el nuevo pasa a ser el final
            }
            cantidad++;
        } //agrega un elemento al final y cierra el circulo

        public T Desencolar()
        {
            T dato = frente!.Dato;
            if (frente == final) //era el unico elemento
            {
                frente = null;
                final = null;
            }
            else
            {
                frente = frente.Siguiente; //el segundo pasa a ser el frente
                final!.Siguiente = frente; //se vuelve a cerrar el circulo
            }
            cantidad--;
            return dato;
        } //saca y devuelve el elemento del frente

        public T Frente()
        {
            return frente!.Dato;
        } //consulta el elemento del frente sin sacarlo

        public void Avanzar()
        {
            if (frente == null) return; //cola vacia, no hay turno que pasar
            final = frente; //el que tenia el turno pasa a ocupar el ultimo lugar
            frente = frente.Siguiente; //el siguiente pasa a tener el turno
        } //pasa el turno al siguiente sin sacar a nadie de la cola

        public bool Eliminar(T dato)
        {
            if (frente == null) return false; //cola vacia, nada que eliminar

            if (object.Equals(frente.Dato, dato)) //coincide el del frente
            {
                if (frente == final) //es el unico elemento
                {
                    frente = null;
                    final = null;
                }
                else
                {
                    frente = frente.Siguiente; //el segundo pasa a ser el frente
                    final!.Siguiente = frente; //se vuelve a cerrar el circulo
                }
                cantidad--;
                return true;
            }

            Nodo<T> anterior = frente;
            Nodo<T> actual = frente.Siguiente!;
            while (actual != frente) //recorre el circulo completo sin pasarse del punto de partida
            {
                if (object.Equals(actual.Dato, dato)) //encontro el nodo a eliminar
                {
                    anterior.Siguiente = actual.Siguiente; //se salta el nodo eliminado
                    if (actual == final) //era el ultimo: el anterior pasa a ser el nuevo final
                    {
                        final = anterior;
                    }
                    cantidad--;
                    return true;
                }
                anterior = actual;
                actual = actual.Siguiente!;
            }
            return false; //no se encontro el dato
        } //saca a un jugador de la rotacion al quedar eliminado

        public int Contar() { return cantidad; } //cantidad de elementos en la cola

        public bool EstaVacia() { return cantidad == 0; } //true si la cola no tiene elementos
    }
}