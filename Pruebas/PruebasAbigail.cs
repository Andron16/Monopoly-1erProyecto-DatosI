//para realizar pruebas

using System;
using Hardware;
using Monopoly.Dominio;

public class PruebasAbigail
{
    public static void PruebasDados(string[] args)
    {
        Console.WriteLine("===== PRUEBAS DE DADOS =====\n");
        
        // Prueba 1: Modo software
        Console.WriteLine("--- Prueba 1: Dados en modo SOFTWARE ---");
        PruebaModoSoftware();
        
        // Prueba 2: Modo hardware (si está conectado)
        Console.WriteLine("\n--- Prueba 2: Dados en modo HARDWARE ---");
        PruebaModoHardware();
        
        Console.WriteLine("\n===== FIN DE PRUEBAS =====");
    }
    
    private static void PruebaModoSoftware()
    {
        //crea un dado en modo software
        Dado dado = new Dado(usarHardware: false);
        
        Console.WriteLine($"¿Es modo físico? {dado.EsModoFisico()} (debe ser false)");
        
        // lanza 5 veces
        for (int i = 1; i <= 2; i++)
        {
            dado.Lanzar();
            int d1 = dado.Obtener1();
            int d2 = dado.Obtener2();
            int total = dado.ObtenerTotal();
            Console.WriteLine($"  Lanzamiento {i}: {d1} + {d2} = {total}");
        }
        
        Console.WriteLine("✓ Prueba software completada");
    }
    
    private static void PruebaModoHardware()
    {
        //intenta conectar al hardware
        Dado dado = new Dado(usarHardware: true, puertoSerial: "COM9");
        
        if (dado.EsModoFisico())
        {
            Console.WriteLine("✓ Conectado al hardware físico");
            Console.WriteLine("  Presiona el botón en el display para lanzar...");
            
            System.Threading.Thread.Sleep(2000);
            dado.Lanzar();
            
            int d1 = dado.Obtener1();
            int d2 = dado.Obtener2();
            int total = dado.ObtenerTotal();
            Console.WriteLine($"  Resultado: {d1} + {d2} = {total}");
        }
        else
        {
            Console.WriteLine("⚠ Hardware no disponible.");
        }
        
        dado.Desconectar();
        Console.WriteLine("✓ Desconectado");
    }
    
}