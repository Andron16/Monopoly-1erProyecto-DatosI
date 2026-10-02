
from machine import Pin, ADC
import time
import random
import sys
import select
from mfrc522 import MFRC522

DEBUG = False  # True imprime mensajes que pueden confundir al servidor


def log(msg): #Se activa si debug esta activo
    if DEBUG:
        print("LOG:" + msg)


DISPLAY1_PINS = {  
    'a': Pin(28, Pin.OUT),
    'b': Pin(27, Pin.OUT),
    'c': Pin(2, Pin.OUT),
    'd': Pin(3, Pin.OUT),  
    'e': Pin(8, Pin.OUT),
    'f': Pin(9, Pin.OUT),
    'g': Pin(10, Pin.OUT),
}

DISPLAY2_PINS = {
    'a': Pin(11, Pin.OUT),
    'b': Pin(12, Pin.OUT),
    'c': Pin(13, Pin.OUT),
    'd': Pin(14, Pin.OUT),
    'e': Pin(15, Pin.OUT),
    'f': Pin(18, Pin.OUT),
    'g': Pin(19, Pin.OUT),
}

BOTON = Pin(16, Pin.IN, Pin.PULL_UP)
LED_ESTADO = Pin(20, Pin.OUT)



led = Pin(21, Pin.OUT)      # GP21 - LED 

rfid = MFRC522(sck=6, mosi=7, miso=4, rst=22, cs=5, spi_id=0)

TIMEOUT_READID_MS = 10000
INTERVALO_RFID_MS = 100
ESPERA_ENTRE_LANZAMIENTOS_MS = 500
DEBOUNCE_MS = 50


PATRONES = {
    0: [0, 0, 0, 0, 0, 0, 0],
    1: [0, 1, 1, 0, 0, 0, 0],
    2: [1, 1, 0, 1, 1, 0, 1],
    3: [1, 1, 1, 1, 0, 0, 1],
    4: [0, 1, 1, 0, 0, 1, 1],
    5: [1, 0, 1, 1, 0, 1, 1],
    6: [1, 0, 1, 1, 1, 1, 1],
}
SEGMENTOS = ['a', 'b', 'c', 'd', 'e', 'f', 'g']



def mostrar_numero(numero, display_pins):
    if numero < 0 or numero > 6: #Si el numero es menor a 0 o mayor a 6 se pone en 0
        numero = 0
    patron = PATRONES[numero]
    for i, seg in enumerate(SEGMENTOS):
        display_pins[seg].value(patron[i])


def mostrar_dados(v1, v2):
    mostrar_numero(v1, DISPLAY1_PINS)
    mostrar_numero(v2, DISPLAY2_PINS)


def apagar_displays():
    mostrar_dados(0, 0)


def lanzar_dados():
    return random.randint(1, 6), random.randint(1, 6)



def intentar_leer_uid(): #Busca una tarjeta y devuelve su UID, si no hay devuelve None  
    """Un solo intento de lectura. Devuelve el UID en hex o None."""
    rfid.init()
    (estado, _tipo) = rfid.request(rfid.REQIDL)
    if estado == rfid.OK:
        (estado, uid) = rfid.SelectTagSN()
        LED_ESTADO.on()
        time.sleep_ms(1000)
        LED_ESTADO.off()
        if estado == rfid.OK:
            return "".join("{:02X}".format(b) for b in uid)
    return None



poller = select.poll()  #Bucle de eventos para leer stdin sin bloquear
poller.register(sys.stdin, select.POLLIN)
buffer_rx = ""


def leer_comando(): #Funcion que lee un comando de la entrada estándar y devuelve una línea completa si ya llegó o None.
    """Devuelve una linea completa si ya llego, o None. Nunca bloquea."""
    global buffer_rx
    while poller.poll(0):
        ch = sys.stdin.read(1)
        if ch == "\n":
            cmd = buffer_rx.strip()
            buffer_rx = ""
            if cmd:
                return cmd
        elif ch != "\r":
            buffer_rx += ch
    return None



def procesar_comando(cmd, estado): #Procesa un comando recibido por stdin y actualiza el estado del sistema.
    if cmd == "PING":
        print("DADOS:LISTO")
    elif cmd == "APAGAR":
        apagar_displays()
        print("DISPLAYS:APAGADOS")
    elif cmd.startswith("MOSTRAR:"):
        partes = cmd.split(":")[1].split(",")
        if len(partes) == 2:
            try:
                v1, v2 = int(partes[0]), int(partes[1])
                mostrar_dados(v1, v2)
                print("DISPLAYS:{},{}".format(v1, v2))
            except ValueError:
                print("ERROR:FORMATO_INVALIDO")
        else:
            print("ERROR:FORMATO_INVALIDO")
    
    elif cmd == "READID":
        estado["readid"] = True
        estado["readid_inicio"] = time.ticks_ms()
        estado["readid_ultimo"] = 0
    
    else:
        print("ERROR_COMANDO_DESCONOCIDO")



def main():
    print("DADOS:LISTO")

    estado = {
        "readid": False,
        "readid_inicio": 0,
        "readid_ultimo": 0,
    }
    boton_bloqueado_hasta = 0   # evita relanzar antes de tiempo
    boton_presionado = False    
    led_apagar_en= None

    while True:
        ahora = time.ticks_ms()

        # 1) Boton de dados 
        if BOTON.value() == 0:
            if (not boton_presionado
                    and time.ticks_diff(ahora, boton_bloqueado_hasta) >= 0):
                time.sleep_ms(DEBOUNCE_MS)
                if BOTON.value() == 0:
                    boton_presionado = True
                    LED_ESTADO.on()
                    led_apagar_en = time.ticks_add(time.ticks_ms(),1000)
                    d1, d2 = lanzar_dados()
                    mostrar_dados(d1, d2)
                    print("DADOS:{},{},{}".format(d1, d2, d1 + d2))
                    log("{} + {} = {}".format(d1, d2, d1 + d2))
        else:
            if boton_presionado:
                boton_presionado = False
                boton_bloqueado_hasta = time.ticks_add(
                    time.ticks_ms(), ESPERA_ENTRE_LANZAMIENTOS_MS)
            if led_apagar_en is not None and time.ticks_diff(time.ticks_ms(), led_apagar_en) >=0:
                LED_ESTADO.off()
                led_apagar_en = None

        cmd = leer_comando()
        if cmd:
            procesar_comando(cmd, estado)

        if estado["readid"]: #Verica el tiempo de espera y si ya pasó el intervalo para leer el RFID
            ahora = time.ticks_ms()
            if time.ticks_diff(ahora, estado["readid_inicio"]) >= TIMEOUT_READID_MS:
                estado["readid"] = False
                print("TIMEOUT")
            elif time.ticks_diff(ahora, estado["readid_ultimo"]) >= INTERVALO_RFID_MS:
                estado["readid_ultimo"] = ahora
                uid = intentar_leer_uid()
                if uid:
                    estado["readid"] = False
                    print(uid)

        time.sleep_ms(10)


try:
    main()
except KeyboardInterrupt:
    apagar_displays()
    LED_ESTADO.off()
    led.off()