//PROGRAMA PRINCIPAL
//AQUI SE EJECUTA EL PROGRAMA EN GENERAL

using System;
class Program
{
    static void Main(string[] args)
    {
        // despacha según el rol: servidor, cliente o pruebas
        string rol = args.Length > 0 ? args[0] : "";

        switch (rol)
        {
            case "servidor":
                Console.WriteLine("Iniciando servidor...");
                // new Servidor().Iniciar();
                break;

            case "cliente":
                Console.WriteLine("Iniciando cliente...");
                // new VistaTablero().MostrarVentana();
                break;

            case "pruebas":
                EjecutarPruebas(args);
                break;

            default:
                MostrarAyuda();
                break;
        }
    }

    static void EjecutarPruebas(string[] args)
    {
        //ejecuta las pruebas según el responsable
        Console.WriteLine("===== MODULO DE PRUEBAS =====\n");

        // PruebasAndron.PruebasDados(args);
        PruebasAbigail.PruebasDados(args);
        // PruebasPalma.PruebasDados(args);
    }

    static void MostrarAyuda()
    {
        //muestra las opciones de uso
        Console.WriteLine("Uso: dotnet run -- [rol]");
        Console.WriteLine();
        Console.WriteLine("Roles disponibles:");
        Console.WriteLine("  dotnet run -- servidor    : Inicia el servidor del juego");
        Console.WriteLine("  dotnet run -- cliente     : Inicia el cliente del juego");
        Console.WriteLine("  dotnet run -- pruebas    : Ejecuta las pruebas unitarias");
        Console.WriteLine();
    }
}