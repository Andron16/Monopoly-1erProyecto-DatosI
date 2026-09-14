//palma
//cartas de evento

namespace Monopoly.Estructuras
{
    public class ColaCartas<T> //cola del mazo de cartas; la carta usada regresa al final
    {
        private Nodo<T>? frente; //primera carta del mazo
        private Nodo<T>? final; //ultima carta del mazo
        private int cantidad; //numero de cartas en el mazo

        public ColaCartas() //inicializa el mazo vacio
        {
            frente = null;
            final = null;
            cantidad = 0;
        }

        public void Encolar(T carta) { } //agrega una carta al final del mazo

        public T Sacar() { return default!; } //toma la carta del frente y la reinserta al final

        public int Contar() { return cantidad; } //cantidad de cartas en el mazo

        public bool EstaVacia() { return cantidad == 0; } //true si el mazo no tiene cartas
    }
}