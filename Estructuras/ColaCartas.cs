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

        public void Encolar(T carta) //agrega una carta al final del mazo
        {
            Nodo<T> nuevo = new Nodo<T>(carta); //crea el nodo de la carta
            if (final == null) //mazo vacio: la carta es frente y final a la vez
            {
                frente = nuevo;
                final = nuevo;
            }
            else //hay cartas: el final actual apunta a la nueva
            {
                final.Siguiente = nuevo;
                final = nuevo;
            }
            cantidad++; //una carta mas
        }

        public T Sacar() //toma la carta del frente y la reinserta al final
        {
            if (frente == null) return default!; //mazo vacio: no hay carta que sacar

            Nodo<T> tomada = frente; //la carta que se va a usar
            if (frente == final) //una sola carta: sigue siendo frente y final, no hay que mover nada
            {
                return tomada.Dato;
            }

            frente = frente.Siguiente; //el frente avanza a la siguiente carta
            tomada.Siguiente = null;   //la tomada se desengancha del inicio
            final!.Siguiente = tomada; //se agrega al final del mazo
            final = tomada;            //y pasa a ser el nuevo final
            return tomada.Dato;        //devuelve la carta usada
        }

        public int Contar() { return cantidad; } //cantidad de cartas en el mazo

        public bool EstaVacia() { return cantidad == 0; } //true si el mazo no tiene cartas
    }
}