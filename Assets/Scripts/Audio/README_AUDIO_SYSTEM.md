# Sistema de Audio para Videojuego Online

Este sistema de audio permite reproducir sonidos tanto localmente como en la red con soporte para proximidad espacial.

## Características

- **Sonidos Locales**: Solo los escucha el jugador que los ejecuta
- **Sonidos de Red**: Los escuchan otros jugadores según proximidad
- **Proximidad Espacial**: Los sonidos se atenúan según la distancia
- **Integración con Photon PUN**: Funciona con el sistema de networking existente

## Componentes Principales

### 1. AudioManager
- **Ubicación**: `Assets/Scripts/Audio/AudioManager.cs`
- **Función**: Gestiona todos los sonidos del juego
- **Uso**: Singleton accesible desde cualquier script

### 2. FootstepAudioController
- **Ubicación**: `Assets/Scripts/Audio/FootstepAudioController.cs`
- **Función**: Maneja automáticamente los sonidos de pasos
- **Uso**: Agregar al prefab del jugador

### 3. ImpactAudioController
- **Ubicación**: `Assets/Scripts/Audio/ImpactAudioController.cs`
- **Función**: Maneja sonidos de impacto de balas
- **Uso**: Agregar al prefab del jugador

### 4. AudioProximityController
- **Ubicación**: `Assets/Scripts/Audio/AudioProximityController.cs`
- **Función**: Controla la proximidad de sonidos 3D
- **Uso**: Se agrega automáticamente a sonidos de red

## Configuración

### Paso 1: Configurar AudioManager
1. Crear un GameObject vacío en la escena
2. Agregar el componente `AudioManager`
3. Configurar los AudioClips en el Inspector

### Paso 2: Configurar Jugador
1. Agregar `FootstepAudioController` al prefab del jugador
2. Agregar `ImpactAudioController` al prefab del jugador
3. Configurar los parámetros en el Inspector

### Paso 3: Configurar Sonidos
Los sonidos deben estar configurados con estos nombres:
- `footstep_slow` - Pasos caminando
- `footstep_fast` - Pasos corriendo
- `shot_pistol` - Disparo de pistola
- `shot_rifle` - Disparo de rifle
- `shot_machinegun` - Disparo de ametralladora
- `impact_wall` - Impacto en pared
- `impact_player` - Impacto en jugador
- `player_death` - Muerte del jugador

## Uso en Código

### Reproducir Sonido Local
```csharp
AudioManager.Instance.PlayLocalSound("shot_pistol");
```

### Reproducir Sonido de Red
```csharp
// Desde la posición del jugador
AudioManager.Instance.PlayNetworkSoundFromPlayer("footstep_slow");

// Desde una posición específica
Vector3 position = new Vector3(10, 0, 5);
AudioManager.Instance.PlayNetworkSound("impact_wall", position);
```

### Agregar Nuevo Sonido
```csharp
var newClip = new AudioManager.AudioClipData
{
    name = "nuevo_sonido",
    clip = audioClip,
    volume = 0.8f,
    is3D = true,
    maxDistance = 15f
};
AudioManager.Instance.AddAudioClip(newClip);
```

## Sonidos Incluidos

El sistema ya está configurado para usar los sonidos existentes en `Assets/Audio/SFX/`:
- `footstep slow.wav` → `footstep_slow`
- `footstep fast.wav` → `footstep_fast`
- `shot (pistol).wav` → `shot_pistol`
- `shot (rifle).wav` → `shot_rifle`
- `shot (machinegun).wav` → `shot_machinegun`
- `impact (hard).wav` → `impact_wall`
- `impact (player).wav` → `impact_player`
- `player death.wav` → `player_death`

## Pruebas

Usar el script `AudioSystemExample.cs` para probar los sonidos:
- **F** - Sonido de paso
- **G** - Sonido de disparo
- **H** - Sonido de impacto
- **J** - Sonido de muerte

## Notas Importantes

1. **PhotonView**: El AudioManager necesita un PhotonView para funcionar en red
2. **Proximidad**: Los sonidos 3D se atenúan automáticamente según la distancia
3. **Performance**: Los sonidos de red se destruyen automáticamente después de reproducirse
4. **Configuración**: Usar `AudioInitializer` para configuración automática

## Troubleshooting

- **No se escuchan sonidos**: Verificar que el AudioManager esté inicializado
- **Sonidos muy bajos**: Ajustar el volumen en la configuración del AudioClip
- **Problemas de red**: Verificar que PhotonNetwork esté conectado
- **Sonidos no se atenúan**: Verificar que `is3D = true` en la configuración
