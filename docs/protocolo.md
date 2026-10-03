# Protocolo cliente-servidor

Proyecto 1 — Monopoly Distribuido con Estructuras Lineales
CE1103 Algoritmos y Estructuras de Datos I — ITCR — Semestre 2, 2026

---

## 1. Resumen

| Aspecto | Decisión |
|---|---|
| Transporte | Sockets **TCP** sobre la red local (WiFi o cable) |
| Puerto | **5000** (`Program.PuertoTcp`) |
| Formato | Texto plano, **una línea por mensaje** (termina en salto de línea) |
| Separador de campos | Barra vertical: `COMANDO\|campo1\|campo2\|...` |
| Codificación | UTF-8 **sin BOM** (`new UTF8Encoding(false)`) en ambos extremos |
| Jugadores | Exactamente 4; el servidor acepta hasta 4 conexiones de jugador |
| Clases que lo implementan | `Red/Protocolo.cs`, `Red/Comandos.cs`, `Red/Servidor.cs`, `Red/ConexionCliente.cs`, `Red/Cliente.cs`, `Red/ControladorCliente.cs` |

Se eligió texto plano porque se puede leer al depurar (el servidor imprime
cada mensaje que envía y recibe) y porque interpretar un mensaje se reduce a
separarlo por `|`.

**Por qué UTF-8 sin BOM.** Con `Encoding.UTF8`, .NET escribe una marca
invisible (BOM) al inicio del flujo. Entonces el primer mensaje llega como
`"﻿CONECTAR|Ana"` y el servidor no lo reconoce como `CONECTAR`.

---

## 2. Principios de diseño

**El servidor es la única autoridad.** El estado del juego (`Juego`, con el
tablero, los jugadores, los saldos, los turnos y el historial) existe
**solo en el servidor**. El cliente envía *intenciones* ("quiero tirar los
dados") y dibuja lo que el servidor le contesta. El cliente no calcula, no
valida y no decide nada. Por eso el cliente no tiene ningún objeto `Jugador`,
`Casilla` ni `Juego`: solo recibe texto.

**El cliente no envía su número de jugador.** El servidor le asigna un número
(1 a 4) a cada conexión y, desde ahí, sabe quién envió cada mensaje por el
socket del que llegó. Si confiara en un número enviado por el cliente
(`TIRAR_DADOS|3`), cualquiera podría jugar en el turno de otro.

**Un hilo por jugador y un candado único.** El servidor atiende cada
conexión en su propio hilo, pero procesa los mensajes uno a la vez dentro de
un `lock` único. Así dos jugadores actuando al mismo tiempo no pueden
corromper el estado.

**Validar siempre en el servidor.** Los botones deshabilitados de la interfaz
son comodidad visual, no seguridad. El servidor valida cada comando como si
viniera de un cliente modificado.

---

## 3. Cómo se arman y se leen los mensajes (`Protocolo`)

| Llamada | Resultado |
|---|---|
| `Protocolo.Armar(Comandos.Dados, "2", "3", "4")` | `"DADOS\|2\|3\|4"` |
| `Protocolo.Armar(Comandos.TirarDados)` | `"TIRAR_DADOS"` |
| `Protocolo.Separar("DADOS\|2\|3\|4")` | `["DADOS", "2", "3", "4"]` |
| `Protocolo.Comando("DADOS\|2\|3\|4")` | `"DADOS"` |

`Armar` **limpia** cada campo antes de unirlo: reemplaza `|` por `/` y los
saltos de línea por espacios. Así un nombre como `Ana|Beto` no puede partir el
mensaje en campos falsos ni en dos líneas.

Los nombres de los comandos se usan siempre por su constante
(`Comandos.TirarDados`), nunca escritos a mano. Si alguien escribe mal un
comando, el compilador lo detecta.

---

## 4. Mensajes del cliente al servidor

| Mensaje | Cuándo se envía | Validaciones del servidor |
|---|---|---|
| `CONECTAR\|nombre` | Automáticamente al conectarse (botón *Conectar* del lobby) | Nombre no vacío (se le quitan espacios); solo una vez por conexión |
| `TIRAR_DADOS` | Botón *Tirar dados* | Partida iniciada y no terminada; es su turno; no ha tirado en este turno |
| `COMPRAR_PROPIEDAD` | Botón *Comprar* | Es su turno; ya tiró; la casilla es una propiedad sin dueño; le alcanza el saldo; confirma con su llavero RFID (si lo tiene) |
| `NO_COMPRAR` | Botón *Pasar* | Es su turno; hay una compra pendiente (ya tiró y está en una propiedad libre) |
| `TERMINAR_TURNO` | Botón *Terminar turno* | Es su turno; ya tiró |
| `CONSULTAR_ESTADO` | (Cliente de consola) | Se permite en cualquier momento de la partida |
| `CONSULTAR_TRANSACCIONES` | Botón *Transacciones* | Se permite en cualquier momento de la partida |

---

## 5. Mensajes del servidor al cliente

| Mensaje | Destinatario | Significado |
|---|---|---|
| `BIENVENIDO\|idJugador` | Solo al que se conectó | Respuesta a `CONECTAR`; le informa su número (1 a 4) |
| `ERROR\|motivo` | Solo al que envió el comando | La acción fue rechazada (ver sección 8) |
| `INICIO` | Todos | Ya están los 4 jugadores; la partida empezó |
| `TURNO\|idJugador\|nombre` | Todos | A quién le toca ahora |
| `DADOS\|idJugador\|dado1\|dado2` | Todos | Resultado del lanzamiento |
| `MOVIMIENTO\|idJugador\|indiceCasilla\|nombreCasilla` | Todos | Dónde quedó el jugador |
| `CASILLA\|nombre\|dueño\|precio\|alquiler` | Todos | Datos de la casilla donde quedó el jugador en turno. `dueño` es `Libre`, `No se vende` o el nombre del propietario |
| `OFRECER_COMPRA\|nombrePropiedad\|precio` | Solo al jugador en turno | Cayó en una propiedad libre y puede comprarla |
| `JUGADOR\|id\|nombre\|saldo\|casilla\|activo` | Todos (o solo a quien pidió el estado) | Estado de **un** jugador; `activo` es `1` o `0`. Se envía una línea por jugador inscrito |
| `MENSAJE\|texto` | Todos | Aviso informativo: transacciones, cartas, conexiones, dado físico, llavero RFID (ver sección 7) |
| `TRANSACCION\|linea` | Solo a quien consultó | Una línea del historial, con el formato del reporte TXT |
| `FIN\|nombreGanador` | Todos | La partida terminó |

Notas:
- **No existe un mensaje `INICIADO`.** La partida arranca sola en el servidor
  cuando se conecta el 4.º jugador. El botón *Iniciar partida* del lobby solo
  cambia de ventana en esa computadora.
- **`TRANSACCION` va en varias líneas porque el reporte tiene saltos de
  línea.** Si fuera en un solo mensaje, se partiría en varios; por eso se
  envía una línea por transacción.
- **La constante `ESTADO` no se usa.** Existe en `Comandos.cs`, pero la
  reemplazó `JUGADOR`.

---

## 6. Secuencias completas

### 6.1 Conexión y lobby

```
Cliente                                   Servidor
   | -- (conexión TCP al puerto 5000) -->   |  ocupa el primer espacio libre (1 a 4)
   | -- CONECTAR|Ana ------------------->   |  crea el Jugador y lo inscribe en Juego
   | <-- BIENVENIDO|1 ------------------    |  (solo a Ana)
   | <-- MENSAJE|Ana se unio (1/4) -----    |  (a todos)
   | <-- JUGADOR|1|Ana|800|0|1 ---------    |  (a todos, un JUGADOR por inscrito)
   |                                        |  con la Pico conectada: registro del llavero
   | <-- MENSAJE|Ana: acerca tu llavero al lector para registrarlo
   | <-- MENSAJE|Ana registro su llavero
   ...  (lo mismo con el 2.º, 3.º y 4.º jugador)
   | <-- INICIO ------------------------    |  al inscribirse el 4.º
   | <-- JUGADOR|... (x4) --------------    |
   | <-- TURNO|1|Ana -------------------    |
```

- El orden de los turnos es el orden de conexión.
- Una conexión que entra y se va sin enviar `CONECTAR` (por ejemplo,
  `Test-NetConnection` para probar el puerto) libera su lugar al irse y no
  le quita el número a nadie.
- Si ya hay 4 jugadores, la conexión nueva se cierra.

### 6.2 Respuesta del servidor a cada comando

Antes de procesar una jugada, el servidor revisa en este orden:
1. Que la conexión ya haya enviado `CONECTAR`.
2. Que la partida haya iniciado.
3. Que no haya terminado.
4. Que sea el turno de quien envía.

`CONSULTAR_ESTADO` y `CONSULTAR_TRANSACCIONES` se responden siempre, sin
importar de quién sea el turno.

| Comando | Mensajes que produce, en orden |
|---|---|
| `TIRAR_DADOS` | 1. Con la Pico: `MENSAJE\|X: presiona el boton del dado (15 s)` · 2. Si hay un pago, pedido del llavero (sección 7) · 3. `DADOS` · 4. Si salió una carta: `MENSAJE\|X saco una carta: ...` · 5. `MOVIMIENTO` · 6. `CASILLA` · 7. Si quebró: `MENSAJE\|X quedo eliminado`; si no, y la propiedad está libre: `OFRECER_COMPRA` (solo a él) |
| `COMPRAR_PROPIEDAD` | Pedido del llavero → `CASILLA` con el nuevo dueño, o `ERROR\|No se pudo comprar` |
| `NO_COMPRAR` | `MENSAJE\|X no compro`, o `ERROR\|No hay compra pendiente` |
| `TERMINAR_TURNO` | (pasa el turno) o `ERROR\|Primero debe tirar los dados` |
| `CONSULTAR_ESTADO` | Una línea `JUGADOR` por jugador inscrito, solo a quien pidió |
| `CONSULTAR_TRANSACCIONES` | Una línea `TRANSACCION` por transacción, solo a quien pidió; si no hay ninguna, `MENSAJE\|Todavia no hay transacciones` |

**Después de cada acción válida** (`TIRAR_DADOS`, `COMPRAR_PROPIEDAD`,
`NO_COMPRAR`, `TERMINAR_TURNO`), el servidor envía a todos:
1. Un `MENSAJE` por cada **transacción nueva**, con el formato
   `Origen -> Destino: monto (descripcion)`. Para encontrar la primera
   transacción nueva, el servidor parte del final del historial
   (`Ultimo()`) y **retrocede con `Anterior`**. Es un uso real de la lista
   doblemente enlazada.
2. Las líneas `JUGADOR` de todos los inscritos (saldos y posiciones al día).
3. `FIN|ganador` si la partida terminó, o `TURNO|id|nombre` si cambió el
   turno (porque terminó su turno o quebró).

### 6.3 Ejemplo real (prueba con 2 PCs y la Pico)

```
<- J1: TIRAR_DADOS
-> todos: MENSAJE|Andron: presiona el boton del dado (15 s)
          [se presiona el boton fisico: la Pico envia DADOS:2,5,7]
-> todos: DADOS|1|2|5
-> todos: MOVIMIENTO|1|7|Zapote
-> todos: CASILLA|Zapote|Libre|80|15
-> J1:    OFRECER_COMPRA|Zapote|80
-> todos: JUGADOR|1|Andron|1500|7|1   (... una linea por jugador)
<- J1: COMPRAR_PROPIEDAD
-> todos: CASILLA|Zapote|Andron|80|15
-> todos: MENSAJE|Andron -> BANCO: 80 (Compra de Zapote)
-> todos: JUGADOR|1|Andron|1420|7|1   (...)
<- J1: TERMINAR_TURNO
-> todos: JUGADOR|... (x4)
-> todos: TURNO|2|Cele
<- J4: TIRAR_DADOS
-> J4:    ERROR|No es su turno
```

(Los montos son de la economía anterior; con la actual el saldo inicial es
800 y los alquileres son 5 veces mayores.)

---

## 7. Mensajes informativos (`MENSAJE|texto`)

| Situación | Texto |
|---|---|
| Alguien entra | `X se unio (n/4)` |
| Alguien se desconecta | `Se desconecto X` |
| Cada transacción nueva | `Origen -> Destino: monto (descripcion)` |
| Sale una carta | `X saco una carta: descripcion` |
| No compra | `X no compro` |
| Quiebra | `X quedo eliminado` |
| Dado físico | `X: presiona el boton del dado (15 s)` |
| Registro del llavero | `X: acerca tu llavero al lector para registrarlo` · `X registro su llavero` · `Ese llavero ya es de Y` · `X juega sin llavero` |
| Pago con llavero | `X: acerca tu llavero para pagar N (descripcion)` · `Pago de X confirmado con su llavero` · `Ese llavero no es de X` · `No se detecto ningun llavero` · `No se confirmo el pago de X con llavero` |
| Historial vacío | `Todavia no hay transacciones` |

**Cobro con RFID.** La tarjeta solo **identifica** al jugador; el saldo
oficial siempre está en el servidor. La compra se cancela si no se confirma
con el llavero correcto. Los pagos obligatorios (alquiler, multas, fianza,
regalo) se cobran aunque no se confirme después de 2 intentos, para que la
partida no se trabe. Si el saldo no alcanza, el jugador queda eliminado sin
pedir la tarjeta.

---

## 8. Errores (`ERROR|motivo`)

Todos se envían **solo** al cliente que mandó el comando.

| Motivo | Causa |
|---|---|
| `CONECTAR invalido` | Falta el nombre, viene vacío o la conexión ya se había presentado |
| `Primero debe enviar CONECTAR` | Envió un comando sin haberse presentado |
| `La partida no ha iniciado` | Todavía no hay 4 jugadores |
| `La partida ya termino` | Ya se envió `FIN` |
| `No es su turno` | Jugar fuera de turno |
| `Ya tiro los dados en este turno` | Tirar dos veces en el mismo turno |
| `No se pudo comprar` | No es propiedad, ya tiene dueño, no le alcanza o no confirmó con su llavero |
| `No hay compra pendiente` | `NO_COMPRAR` sin estar en una propiedad libre después de tirar |
| `Primero debe tirar los dados` | Terminar el turno sin tirar |
| `Comando desconocido` | Cualquier otro texto |

Así se cubren las validaciones obligatorias del enunciado: jugar fuera de
turno, comprar sin saldo, comprar una propiedad con dueño, tirar varias veces
en el mismo turno, pagar sin saldo sin aplicar la eliminación y modificar
información desde el cliente.

---

## 9. Fin de partida y desconexiones

- La partida termina cuando queda **un solo jugador activo** o cuando se
  alcanza el **límite de turnos** (`Program.LimiteTurnos = 100` turnos
  individuales). Gana el jugador activo con mayor patrimonio (saldo + valor
  de sus propiedades).
- Al terminar, el servidor envía `FIN|ganador` y exporta el reporte a
  `docs/transacciones/reporte_partida.txt`. Después de `FIN`, cualquier
  jugada responde `ERROR|La partida ya termino`.
- Si un cliente se desconecta, el servidor libera su conexión y avisa
  `Se desconecto X`. **Limitación conocida:** si eso ocurre a mitad de la
  partida, el jugador sigue en la cola de turnos y la partida queda esperando
  su turno.
- Cuando el cliente pierde la conexión, se muestra localmente
  `Conexion con el servidor cerrada`. Ese mensaje lo genera el propio cliente,
  no viaja por la red.

---

## 10. Lado del cliente

`ControladorCliente` recibe cada línea desde el hilo de escucha de `Cliente`,
la separa con `Protocolo.Separar` y avisa a la interfaz por medio de
`Action`, sin conocer ninguna ventana:

| Mensaje | Qué hace el cliente |
|---|---|
| `JUGADOR` | `AlActualizarJugador` → mueve la ficha y actualiza el saldo (no va al registro) |
| `CASILLA` | `AlActualizarCasilla` → panel de la última casilla (no va al registro) |
| `BIENVENIDO` | Guarda `MiId` y avisa `AlConectado` al lobby |
| `INICIO` | `AlEmpezarPartida` |
| `TURNO` | `AlCambiarTurno`: muestra "Es tu turno" si el id es el propio, o "Turno de X" |
| `FIN` | `AlCambiarTurno`: muestra "Partida terminada. Gano X" |
| Los demás | Se traducen a texto legible y van al registro (`AlRegistrar`) |

Como esos avisos llegan desde el hilo de la red, los métodos de las ventanas
usan `Invoke` para actualizar los controles desde el hilo de la interfaz.

---

## 11. Protocolo PC ↔ Raspberry Pi Pico W (aparte del de red)

La Pico **no** es un cliente TCP. Va conectada por **USB** a la PC del
servidor y habla por el puerto serie virtual (USB-CDC) a **115200 baudios**,
también con líneas de texto, pero con `:` y `,` como separadores. La maneja
`Hardware/PicoSerial.cs`, que es el único dueño del puerto COM. Dados y
lector RFID comparten ese único puerto.

| Dirección | Mensaje | Significado |
|---|---|---|
| PC → Pico | `PING` | Confirma que `main.py` está corriendo (handshake al conectar) |
| PC → Pico | `MOSTRAR:a,b` | Muestra dos valores en los displays |
| PC → Pico | `APAGAR` | Apaga los displays |
| PC → Pico | `READID` | Activa el lector RFID hasta 10 s |
| Pico → PC | `DADOS:LISTO` | Respuesta al `PING` (y al arrancar) |
| Pico → PC | `DADOS:d1,d2,total` | Se presionó el botón físico |
| Pico → PC | `DISPLAYS:...` | Confirmación de `MOSTRAR` o `APAGAR` |
| Pico → PC | UID en hexadecimal (p. ej. `A1B2C3D4`) | Respuesta a `READID`: se leyó un llavero |
| Pico → PC | `TIMEOUT` | Respuesta a `READID`: no se acercó ningún llavero |
| Pico → PC | `ERROR:...` | Error reportado por la Pico |

Los mensajes `DADOS:`, `DISPLAYS:` y `ERROR:` llegan en cualquier momento y se
reparten por un evento. Cualquier otra línea es la respuesta al último
comando enviado (`PicoSerial.Solicitar`).