//PROGRAMA PRINCIPAL
//AQUI SE EJECUTA EL PROGRAMA EN GENERAL


/*using System;

namespace Monopoly
{
    class Program //punto de entrada unico; despacha segun el rol recibido
    {
        [STAThread] //requerido por Windows Forms para el rol cliente
        static void Main(string[] args) //decide que rol ejecutar
        {
            if (args.Length == 0) //sin argumento no se sabe que arrancar
            {
                Console.WriteLine("Uso: dotnet run -- [servidor|cliente|pruebas]");
                return;
            }

            switch (args[0].ToLower()) //despacha al rol correspondiente
            {
                case "servidor":
                    Console.WriteLine("[servidor] pendiente de implementar");
                    //new Monopoly.Red.Servidor().Iniciar(5000);
                    break;

                case "cliente":
                    Console.WriteLine("[cliente] pendiente de implementar");
                    //ApplicationConfiguration.Initialize();
                    //System.Windows.Forms.Application.Run(new Monopoly.Interfaz.VistaTablero());
                    break;

                case "pruebas":
                    Monopoly.Pruebas.PruebasAndron.Ejecutar(); //estructuras de Andron
                    //Monopoly.Pruebas.PruebasPalma.Ejecutar();
                    //Monopoly.Pruebas.PruebasAbigail.Ejecutar();
                    break;

                default:
                    Console.WriteLine("Rol invalido: " + args[0]);
                    break;
            }
        }
    }
}*/