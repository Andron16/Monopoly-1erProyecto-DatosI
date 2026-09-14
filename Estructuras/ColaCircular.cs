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

        public void Encolar(T dato) { } //agrega un elemento al final y cierra el circulo

        public T Desencolar() { return default!; } //saca y devuelve el elemento del frente

        public T Frente() { return default!; } //consulta el elemento del frente sin sacarlo

        public void Avanzar() { } //pasa el turno al siguiente sin sacar a nadie de la cola

        public bool Eliminar(T dato) { return false; } //saca a un jugador de la rotacion al quedar eliminado

        public int Contar() { return cantidad; } //cantidad de elementos en la cola

        public bool EstaVacia() { return cantidad == 0; } //true si la cola no tiene elementos
    }
}