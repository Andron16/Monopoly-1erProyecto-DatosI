# Documentación de estructuras de datos

Proyecto 1 — Monopoly Distribuido con Estructuras Lineales
CE1103 Algoritmos y Estructuras de Datos I — ITCR — Semestre 2, 2026

---

## 1. Criterios generales

Todas las estructuras de este proyecto fueron implementadas desde cero. No se
utiliza `System.Collections.Generic` ni ninguna estructura nativa de C#
(`List`, `LinkedList`, `Queue`, `Stack`, `Dictionary`).

Esta restricción está garantizada a nivel de compilador: el archivo
`Monopoly.csproj` contiene `<ImplicitUsings>disable</ImplicitUsings>`, por lo
que cualquier intento de usar esas clases produce un error de compilación en
lugar de pasar desapercibido.

**Consecuencia técnica.** Sin `System.Collections.Generic` no existe
`IEnumerable<T>`, por lo tanto no es posible usar `foreach` sobre nuestras
estructuras. Todos los recorridos se realizan con `while` y una variable
`actual` que avanza siguiendo las referencias entre nodos.

**Sobre los genéricos.** Declarar `class ListaSimple<T>` es válido y no
requiere la biblioteca prohibida: la sintaxis genérica pertenece al lenguaje,
no a `System.Collections.Generic`. Lo prohibido es consumir las estructuras de
esa biblioteca, no parametrizar las propias.

**Sobre los arreglos.** El profesor autorizó el uso de arreglos nativos `T[]`
como almacenamiento interno, entendiendo que la lógica de la estructura
(inserción, eliminación, búsqueda, recorrido) es responsabilidad nuestra. Aun
así, se optó por implementaciones enlazadas con nodos en todas las estructuras
del proyecto, por coherencia con el tema del curso.

**Independencia de dominio.** Las clases de `Estructuras/` son genéricas y no
conocen nada del Monopoly. Ninguna menciona `Jugador`, `Casilla` ni
`Transaccion`. Esto permite que la misma estructura se reutilice en contextos
distintos y es lo que las convierte en estructuras de datos y no en clases del
juego.

---

## 2. Nodos

### `Nodo<T>` — enlace simple

| Miembro | Tipo | Descripción |
|---|---|---|
| `Dato` | `T` | Valor almacenado |
| `Siguiente` | `Nodo<T>?` | Referencia al nodo siguiente |

Constructor: `Nodo(T dato)` — crea un nodo suelto, sin enlazar.

Usado por: `ListaSimple`, `ColaCircular`, `ColaCartas`.

### `NodoDoble<T>` — enlace doble

| Miembro | Tipo | Descripción |
|---|---|---|
| `Dato` | `T` | Valor almacenado |
| `Anterior` | `NodoDoble<T>?` | Referencia al nodo previo |
| `Siguiente` | `NodoDoble<T>?` | Referencia al nodo siguiente |

Constructor: `NodoDoble(T dato)` — crea un nodo suelto, sin enlazar.

Usado por: `ListaCircularDoble`, `ListaDoble`.

---

## 3. `ListaSimple<T>` — propiedades de cada jugador

**Uso en el juego.** Cada `Jugador` mantiene una `ListaSimple<Propiedad>` con
los terrenos que ha adquirido. Al comprar una propiedad se agrega; al calcular
el patrimonio final se recorre para sumar el valor de todas.

**Por qué esta estructura.** Las propiedades de un jugador no tienen un orden
significativo ni requieren recorrido inverso. Las operaciones dominantes son
agregar y verificar pertenencia, que una lista enlazada simple cubre sin el
costo adicional de mantener referencias hacia atrás.

**Campos internos:** `cabeza` (primer nodo), `cantidad` (contador).

| Firma | Descripción |
|---|---|
| `void Agregar(T dato)` | Inserta un elemento al final de la lista |
| `bool Eliminar(T dato)` | Quita la primera coincidencia; devuelve `true` si la encontró |
| `bool Contiene(T dato)` | Indica si el elemento está en la lista |
| `T Obtener(int indice)` | Devuelve el elemento en la posición dada |
| `int Contar()` | Cantidad de elementos |
| `bool EstaVacia()` | `true` si no tiene elementos |

---

## 4. `ListaCircularDoble<T>` — el tablero

**Uso en el juego.** Es el tablero. Cada nodo representa una casilla y
contiene referencias a la casilla anterior y a la siguiente. El tablero tiene
un mínimo de 24 casillas exigido por el enunciado; el grupo decidió subirlo a
**32 casillas** (4 lados de 8, con temática de provincias de Costa Rica). La
posición de cada jugador se guarda como una referencia directa al nodo
(`NodoDoble<Casilla>`), no como un número entero.

**Por qué esta estructura.** Es un requisito explícito del enunciado, y
responde a la naturaleza del tablero: un circuito cerrado donde el último nodo
enlaza con el primero. Eso hace que dar la vuelta al tablero sea el
comportamiento natural de la estructura y no un caso especial que haya que
detectar con condicionales.

El enlace doble permite retroceder. Se usa de verdad: la carta *"Presa en la
General Cañas: retrocede 2"* llama a `Tablero.Retroceder`, que delega en
`ListaCircularDoble.Retroceder` y recorre dos nodos siguiendo la referencia
`Anterior`. Desde la Salida (casilla 0), retroceder 2 deja al jugador en la
casilla 30, porque `cabeza.Anterior` es el último nodo del círculo.

**Campos internos:** `cabeza` (primer nodo, cuyo `Anterior` es el último),
`cantidad` (contador).

| Firma | Descripción |
|---|---|
| `void Agregar(T dato)` | Inserta al final y vuelve a cerrar el círculo |
| `NodoDoble<T> ObtenerNodo(int indice)` | Devuelve el nodo en la posición dada |
| `NodoDoble<T> Avanzar(NodoDoble<T> desde, int pasos)` | Recorre nodo por nodo hacia `Siguiente` |
| `NodoDoble<T> Retroceder(NodoDoble<T> desde, int pasos)` | Recorre nodo por nodo hacia `Anterior` |
| `int Contar()` | Cantidad de nodos |
| `bool EstaVacia()` | `true` si no tiene nodos |

**Nota de implementación sobre `Avanzar`.** El método recorre la lista nodo
por nodo, siguiendo la referencia `Siguiente` una vez por cada paso. No
calcula la posición destino mediante aritmética de índices. Esta decisión es
deliberada: el movimiento del jugador por el tablero es un recorrido real de
la estructura enlazada, que es precisamente lo que el proyecto busca ejercitar.

Con 32 casillas, avanzar más pasos que casillas tiene el tablero confirma que
el círculo cierra bien (por ejemplo, avanzar 40 pasos desde la casilla 0 debe
terminar en la casilla 8, ya que 40 mod 32 = 8). La vuelta completa no
necesita ningún `if`: después del nodo 31 viene el nodo 0 porque así están
enlazados.

**Pasar por la Salida.** `Tablero.PasoPorSalida(desde, pasos)` hace el mismo
recorrido nodo por nodo y revisa si alguna casilla intermedia o la final es la
casilla 0. Así se paga el premio aunque el jugador solo cruce la Salida sin
caer en ella.

**Envoltura en el dominio.** `Dominio/Tablero.cs` envuelve la lista
(`ListaCircularDoble<Casilla>`) y expone `Mover`, `Retroceder`,
`ObtenerNodo`, `ObtenerCasilla`, `PasoPorSalida` y `Contar`.
`Tablero.Construir()` crea las 32 casillas (`Propiedad`, `CasillaEvento` y
`CasillaEspecial`, que heredan de `Casilla`) y las agrega en orden; cada
casilla recibe como `Id` su posición en el circuito.

---

## 5. `ColaCircular<T>` — turnos de los jugadores

**Uso en el juego.** Administra el orden de los turnos. El jugador al frente
de la cola es quien tiene el turno actual. Al terminar su turno se avanza, y
el turno pasa al siguiente jugador. Cuando un jugador queda eliminado por no
poder cubrir un pago, se retira de la rotación con `Eliminar`.

**Por qué esta estructura.** Los turnos son cíclicos: después del cuarto
jugador vuelve el primero. Una cola circular modela ese ciclo directamente. El
último nodo apunta al frente, de modo que avanzar el turno nunca requiere
comprobar si se llegó al final.

**Distinción entre `Desencolar` y `Avanzar`.** `Avanzar` rota el turno sin
sacar a nadie de la cola: el jugador que termina pasa al final y sigue en la
partida. `Desencolar` sí extrae al elemento del frente. En el flujo normal del
juego se usa `Avanzar`; `Desencolar` queda disponible como operación básica de
la estructura.

**Cómo la usa `Juego`.** `Turnos` es una `ColaCircular<Jugador>`.
`JugadorActual()` es `Turnos.Frente()`; `TerminarTurno` llama a
`Turnos.Avanzar()` y luego salta a quien tenga turnos perdidos (cárcel o
carta) avanzando de nuevo; cuando un jugador quiebra, `Turnos.Eliminar(j)` lo
saca y el turno pasa solo al siguiente. La partida termina cuando
`Turnos.Contar() <= 1`. El orden de la cola es el orden de conexión de los
jugadores.

**Campos internos:** `frente` (turno actual), `final` (último, cuyo
`Siguiente` apunta al frente), `cantidad` (contador).

| Firma | Descripción |
|---|---|
| `void Encolar(T dato)` | Agrega un elemento al final y cierra el círculo |
| `T Desencolar()` | Saca y devuelve el elemento del frente |
| `T Frente()` | Consulta el elemento del frente sin sacarlo |
| `void Avanzar()` | Pasa el turno al siguiente sin sacar a nadie |
| `bool Eliminar(T dato)` | Saca a un jugador de la rotación al quedar eliminado |
| `int Contar()` | Cantidad de elementos |
| `bool EstaVacia()` | `true` si no tiene elementos |

**Casos cubiertos en pruebas.** Cola vacía, un solo elemento (frente y final
son el mismo nodo, apuntándose a sí mismo), un giro completo de `Avanzar`
sobre varios elementos (vuelve al primero), y `Eliminar` sobre el frente, el
medio, el último y el único elemento de la cola, verificando en cada caso que
el círculo sigue cerrado (`final.Siguiente == frente`).

---

## 6. `ColaCartas<T>` — mazo de cartas de evento

**Uso en el juego.** Contiene las cartas de evento. Cuando un jugador cae en
una `CasillaEvento`, se saca la carta del frente, se aplica su efecto y la
carta se reinserta al final del mazo para poder reutilizarse más adelante.

**Por qué esta estructura.** El enunciado establece que una carta usada debe
pasar al final del mazo. Ese comportamiento es exactamente el de una cola
circular, y `Sacar()` lo encapsula en una sola operación en lugar de dejar la
reinserción como responsabilidad de quien la llama, donde podría olvidarse.

**Campos internos:** `frente` (primera carta), `final` (última carta),
`cantidad` (contador).

| Firma | Descripción |
|---|---|
| `void Encolar(T carta)` | Agrega una carta al final del mazo |
| `T Sacar()` | Toma la carta del frente y la reinserta al final |
| `T? UltimaSacada` | Propiedad de solo lectura: la última carta que salió |
| `int Contar()` | Cantidad de cartas |
| `bool EstaVacia()` | `true` si el mazo no tiene cartas |

**Cómo funciona `Sacar()`.** Toma el nodo del frente, mueve `frente` al
siguiente, desengancha el nodo tomado y lo vuelve a enlazar después de
`final`. No se crea ni se destruye ningún nodo: la misma carta cambia de lugar.
Con una sola carta, la devuelve sin mover nada. Por eso `Contar()` no cambia
al sacar.

**Cómo la usa el juego.** `Juego.CrearMazo()` encola 10 cartas fijas al
iniciar la partida (`Mazo` es una `ColaCartas<CartaEvento>`). Cuando un
jugador cae en una `CasillaEvento`, se llama a `Mazo.Sacar()` y luego a
`carta.Aplicar(jugador, juego)`. Las cartas salen en orden y se reciclan, así
que después de 10 eventos el mazo vuelve a empezar.

**`UltimaSacada`.** `Sacar()` la actualiza. El servidor guarda su valor antes
de tirar los dados y lo compara después: si cambió, anuncia a todos qué carta
salió. Es una propiedad de consulta y no altera el comportamiento de cola.

---

## 7. `ListaDoble<T>` — historial de transacciones

**Uso en el juego.** Almacena todas las transacciones generadas durante la
partida. Toda operación económica (compra de propiedad, pago de alquiler, pago
al banco, pago entre jugadores, ganancia o pérdida por evento, premio por
pasar por la salida) produce una `Transaccion` que se agrega aquí.

**Por qué esta estructura.** El enunciado exige poder recorrer el historial
desde la transacción más antigua y también desde la más reciente. Una lista
doblemente enlazada, manteniendo referencias a la cabeza y a la cola, permite
ambos recorridos con el mismo costo y sin invertir la estructura.

**Campos internos:** `cabeza` (más antigua), `cola` (más reciente),
`cantidad` (contador).

| Firma | Descripción |
|---|---|
| `void Agregar(T dato)` | Inserta un elemento al final de la lista |
| `NodoDoble<T>? Primero()` | Nodo inicial, para recorrer de la más antigua a la más reciente |
| `NodoDoble<T>? Ultimo()` | Nodo final, para recorrer de la más reciente a la más antigua |
| `int Contar()` | Cantidad de elementos |
| `bool EstaVacia()` | `true` si no tiene elementos |

**Recorridos en ambos sentidos, en uso real:**
- *De la más antigua a la más reciente* (`Primero()` y `Siguiente`): las
  búsquedas, la consulta `CONSULTAR_TRANSACCIONES` y el reporte en orden
  normal.
- *De la más reciente a la más antigua* (`Ultimo()` y `Anterior`):
  `ReporteTransacciones.GenerarTexto(historial, masRecientePrimero: true)`.
  Además, el servidor, para anunciar solo las transacciones nuevas de cada
  jugada, parte de `Ultimo()` y retrocede con `Anterior` tantas veces como
  transacciones nuevas haya.

**Quién agrega.** Solo el `Banco`, en su método privado `Registrar`: cada
operación económica crea su `Transaccion` (inmutable) y la agrega. Ninguna
otra clase escribe en el historial.

**Búsquedas.** Las consultas por jugador y por tipo no son métodos de esta
estructura, porque implicarían que la lista conozca el dominio del juego. Se
resuelven en la capa de dominio (`Juego.BuscarPorJugador` y
`Juego.BuscarPorTipo`), recorriendo la lista con `while` desde `Primero()` y
acumulando las coincidencias en una `ListaSimple<T>` que se devuelve como
resultado.

---

## 8. Costo de las operaciones

Esta tabla distingue las operaciones que resuelven de inmediato, sin recorrer
la estructura, de las que necesitan avanzar nodo por nodo.

| Estructura | Operación | ¿Recorre la estructura? |
|---|---|---|
| `ListaSimple` | `Agregar` | Sí, hasta el último nodo |
| `ListaSimple` | `Eliminar` | Sí, hasta encontrar el elemento |
| `ListaSimple` | `Contiene` | Sí, hasta encontrar el elemento |
| `ListaSimple` | `Obtener` | Sí, hasta llegar a la posición |
| `ListaCircularDoble` | `Agregar` | No, `cabeza.Anterior` es el último nodo |
| `ListaCircularDoble` | `Avanzar` | Recorre tantos nodos como pasos indiquen los dados |
| `ListaCircularDoble` | `Retroceder` | Recorre tantos nodos como pasos, hacia `Anterior` |
| `ListaCircularDoble` | `ObtenerNodo` | Sí, desde la cabeza |
| `ColaCircular` | `Encolar` | No, mantiene referencia a `final` |
| `ColaCircular` | `Desencolar` | No, acceso directo al frente |
| `ColaCircular` | `Avanzar` | No, solo mueve las referencias |
| `ColaCircular` | `Eliminar` | Sí, hasta encontrar al jugador |
| `ColaCartas` | `Encolar` | No, mantiene referencia a `final` |
| `ColaCartas` | `Sacar` | No, frente y final en acceso directo |
| `ListaDoble` | `Agregar` | No, mantiene referencia a `cola` |
| `ListaDoble` | `Primero`, `Ultimo` | No, acceso directo a `cabeza` y `cola` |
| `ListaDoble` | Recorrido completo | Sí, en cualquiera de los dos sentidos |
| `ColaCartas` | `UltimaSacada` | No, es una propiedad guardada |
| Todas | `Contar`, `EstaVacia` | No, devuelven el contador interno |

**Nota sobre `ListaSimple.Agregar`.** Es la única inserción del proyecto que
obliga a recorrer toda la estructura, porque la clase no mantiene una
referencia al último nodo: para insertar al final hay que llegar hasta él
partiendo de la cabeza. La decisión es aceptable en este contexto, ya que un
jugador acumula a lo sumo una decena de propiedades durante la partida y la
inserción ocurre pocas veces por turno. Si el volumen fuera mayor, añadir un
campo `cola` permitiría insertar sin recorrer nada, igual que en `ListaDoble`.

**Nota sobre recorrer `ListaSimple` por índice.** `Jugador.CalcularPatrimonio`
y `Banco.EliminarJugador` recorren las propiedades con `Obtener(i)` dentro de
un ciclo. Cada `Obtener(i)` vuelve a empezar desde la cabeza, así que el
recorrido completo visita los nodos varias veces. Con una decena de
propiedades por jugador no tiene impacto. Un recorrido con un nodo `actual`
que avanza por `Siguiente` lo haría en una sola pasada.

---

## 9. Estructuras que NO son estructuras de datos del juego

Esta sección existe para evitar una confusión al revisar el código.

La capa de interfaz (`Interfaz/VistaTablero.cs`) usa un arreglo
`Point[] coordenadas`, de 32 posiciones, que traduce el número de cada casilla
a una coordenada en píxeles sobre la imagen de fondo del tablero
(`docs/Tablero.jpeg`, 1600x1600). Cada posición se midió a mano; por ejemplo
`coordenadas[0]` es el punto donde se dibuja la ficha de un jugador parado en
la casilla de Salida.

**Ese arreglo es exclusivamente de presentación.** No es el tablero. El
tablero real es la `ListaCircularDoble<Casilla>` descrita en la sección 4,
envuelta por `Dominio/Tablero.cs`. El arreglo de coordenadas solo le dice a
`VistaTablero` en qué píxel dibujar la ficha (imagen PNG) de cada jugador; la
lógica del juego nunca lo consulta: el movimiento de los jugadores, los
propietarios, los alquileres y los efectos de casilla se resuelven
íntegramente recorriendo la lista enlazada, del lado del servidor.

Prueba de esa separación: `VistaTablero` ni siquiera conoce la clase
`Jugador` ni `Tablero`. Su método público `ActualizarJugador(int id, string
nombre, int saldo, int casilla, bool activo)` solo recibe datos simples, que
llegan por el socket como texto (mensaje `JUGADOR|id|nombre|saldo|casilla|activo`
del servidor). El número de casilla que indexa `coordenadas` es un `int`
suelto, no una referencia al nodo real del tablero.

Dicho de otro modo: si se eliminara la interfaz gráfica, el juego seguiría
funcionando completo. Si se eliminara la lista circular, no habría juego.

**Otros arreglos nativos `T[]` del proyecto (autorizados).** Todos son de
tamaño fijo 4 (uno por jugador, indexados por `id - 1`) o de presentación.
Ninguno reemplaza una estructura exigida:

| Arreglo | Clase | Para qué |
|---|---|---|
| `Jugador[4] jugadores` | `Juego` | Guardar a los inscritos, incluso eliminados, para informar su estado y elegir ganador. Los turnos viven en la `ColaCircular`. |
| `ConexionCliente?[4] conexiones` | `Servidor` | Un espacio por conexión de jugador |
| `string?[4] nombres` | `MenuJugador` | Nombres del lobby (en lugar de un `ListBox`, que es una colección nativa) |
| `string?[4]`, `int[4]`, `bool[4]` | `VistaTablero` | Último estado recibido de cada jugador para dibujarlo |
| `Point[32] coordenadas`, `Image[4]`, `Color[4]` | `VistaTablero` | Solo presentación (ver arriba) |
| `Jugador[4] candidatos` | `Juego.OtroJugadorAlAzar` | Arreglo local para elegir un jugador al azar (casilla Regalo) |

(Nota histórica: en una versión anterior de este documento se planteaba usar
una matriz `int[,]` de 7x7 en vez de un arreglo de coordenadas medidas a mano.
El grupo descartó esa opción y usó una imagen de fondo con coordenadas
`Point[]` en su lugar; la advertencia de esta sección sigue aplicando igual.)

---

## 10. Estado de implementación

**Las siete estructuras están completas, probadas y en uso en el juego.**

| Archivo | Responsable | Estado |
|---|---|---|
| `Nodo.cs` | Andron | Completo |
| `NodoDoble.cs` | Andron | Completo |
| `ListaSimple.cs` | Palma | Completa (Agregar, Eliminar, Contiene, Obtener; probada) |
| `ListaCircularDoble.cs` | Abigail | Completa (Avanzar y Retroceder en uso; 32 casillas) |
| `ListaDoble.cs` | Andron | Completa (probada con recorrido en ambos sentidos) |
| `ColaCircular.cs` | Abigail | Completa (probados los 4 casos de `Eliminar` y un giro completo de `Avanzar`) |
| `ColaCartas.cs` | Palma | Completa (`Sacar` recicla la carta al final; `UltimaSacada`; probada) |

**Regla del grupo.** Una vez acordada una firma pública, no se modifica sin
avisar al grupo. Cambiarla rompe en silencio el código de quien ya esté
escribiendo contra ella.

---

## 11. Resumen: dónde vive cada estructura

| Estructura | Instancia en el juego | Clase dueña | Para qué |
|---|---|---|---|
| `ListaCircularDoble<Casilla>` | `casillas` | `Tablero` (dentro de `Juego`) | El tablero de 32 casillas; movimiento hacia adelante y hacia atrás |
| `NodoDoble<Casilla>` | `Jugador.Posicion` | `Jugador` | La posición es una referencia al nodo, no un número |
| `ColaCircular<Jugador>` | `Turnos` | `Juego` | Orden de turnos, salto por turnos perdidos, salida por quiebra |
| `ColaCartas<CartaEvento>` | `Mazo` | `Juego` | Mazo de 10 cartas que se reciclan |
| `ListaDoble<Transaccion>` | `Historial` | `Juego` (la comparte con `Banco`) | Historial de transacciones en ambos sentidos |
| `ListaSimple<Propiedad>` | `Propiedades` | `Jugador` | Terrenos de cada jugador |
| `ListaSimple<Transaccion>` | Resultado de las búsquedas | `Juego.BuscarPorJugador` / `BuscarPorTipo` | Devolver las coincidencias del historial |

Todas viven en el **servidor**. El cliente no tiene ninguna estructura del
juego: solo recibe texto por la red y lo dibuja.

---

## 12. Cómo se prueban

`dotnet run -- pruebas` corre las pruebas no interactivas. Cada línea imprime
el resultado junto al valor esperado en un comentario del código.

| Estructura | Prueba | Qué verifica |
|---|---|---|
| `ListaDoble` | `PruebasAndron.ProbarListaDoble` | Agregar, contar, recorrer de la más antigua a la más reciente y al revés |
| `ListaSimple` | `PruebasPalma.ProbarListaSimple` | Agregar, Contiene, Obtener, Eliminar al inicio, en el medio y al final |
| `ColaCircular` | `PruebasAbigail.ProbarColaCircular` | Vacía, un elemento, giro completo de `Avanzar`, `Eliminar` del frente, medio, último y único |
| `ColaCartas` | `PruebasPalma.ProbarColaCartas` | Encolar, `Sacar` recicla al final y la cantidad no cambia |
| `ListaCircularDoble` | `PruebasAndron.ProbarJuego` (indirecta) | Una partida completa con varias vueltas al tablero (el círculo cierra), premio por pasar por la Salida y cartas de evento |
| Historial y búsquedas | `PruebasAndron.ProbarBusquedas` y `ProbarReporte` | Búsqueda por jugador y tipo; reporte en ambos sentidos y exportación a TXT |