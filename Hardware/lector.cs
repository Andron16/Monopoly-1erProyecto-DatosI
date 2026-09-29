namespace Monopoly.Hardware
{
    public class LectorRfid //lector usa el puerto compartido con los dados; devuelve el UID de la tarjeta o null si no se acerco ninguna
    {
        private readonly PicoSerial pico; //puerto compartido con los dados

        public LectorRfid(PicoSerial pico) { this.pico = pico; } //recibe el puerto ya creado

        public string? EsperarTarjeta() //activa el lector y espera la tarjeta, devuelve el ID o null si no se acerco ninguna
        {
            string? id = pico.Solicitar("READID", 15000); //un poco mas que los 10 s de espera de la Pico
            return (string.IsNullOrEmpty(id) || id == "TIMEOUT") ? null : id;
        }

        
    }
}