# 🎮 Guía de Configuración del Sistema de Audio

## ✅ Checklist de Configuración

### Paso 1: Configurar el AudioManager en la Escena

1. **Crear GameObject vacío:**
   - Click derecho en Hierarchy → Create Empty
   - Nombre: `AudioManager`

2. **Agregar Componentes:**
   - Add Component → `AudioManager`
   - Add Component → `Photon View` ⚠️ **IMPORTANTE**

3. **Configurar Audio Clips en el AudioManager:**
   
   En la lista `Audio Clips`, agrega estos clips con **ESTOS NOMBRES EXACTOS**:

   #### Pasos:
   - **Name**: `footstep_slow` ← nombre exacto
   - **Clip**: Arrastra `footstep slow.wav`
   - **Volume**: `0.7`
   - **Pitch**: `1`
   - **Is 3D**: `✓` (marcado)
   - **Max Distance**: `12`

   - **Name**: `footstep_fast` ← nombre exacto
   - **Clip**: Arrastra `footstep fast.wav`
   - **Volume**: `0.8`
   - **Pitch**: `1`
   - **Is 3D**: `✓`
   - **Max Distance**: `12`

   #### Disparos:
   - **Name**: `shot_pistol` ← nombre exacto
   - **Clip**: Arrastra `shot (pistol).wav`
   - **Volume**: `0.8`
   - **Pitch**: `1`
   - **Is 3D**: `✓`
   - **Max Distance**: `20`

   - **Name**: `shot_rifle` ← nombre exacto
   - **Clip**: Arrastra `shot (rifle).wav`
   - **Volume**: `0.9`
   - **Pitch**: `1`
   - **Is 3D**: `✓`
   - **Max Distance**: `25`

   - **Name**: `shot_machinegun` ← nombre exacto
   - **Clip**: Arrastra `shot (machinegun).wav`
   - **Volume**: `0.9`
   - **Pitch**: `1`
   - **Is 3D**: `✓`
   - **Max Distance**: `25`

   #### Impactos:
   - **Name**: `impact_wall` ← nombre exacto
   - **Clip**: Arrastra `impact (hard).wav`
   - **Volume**: `0.6`
   - **Pitch**: `1`
   - **Is 3D**: `✓`
   - **Max Distance**: `15`

   - **Name**: `impact_player` ← nombre exacto
   - **Clip**: Arrastra `impact (player).wav`
   - **Volume**: `0.8`
   - **Pitch**: `1`
   - **Is 3D**: `✓`
   - **Max Distance**: `15`

   #### Muerte:
   - **Name**: `player_death` ← nombre exacto
   - **Clip**: Arrastra `player death.wav`
   - **Volume**: `0.9`
   - **Pitch**: `1`
   - **Is 3D**: `✓`
   - **Max Distance**: `20`

### Paso 2: Configurar el Prefab del Jugador

1. **Abrir el prefab del jugador** (debería estar en `Assets/Resources/`)

2. **Agregar Componentes al jugador:**
   - Add Component → `FootstepAudioController`
   - Add Component → `ImpactAudioController`

3. **Configurar FootstepAudioController:**
   - **Walk Footstep Sound**: `footstep_slow`
   - **Run Footstep Sound**: `footstep_fast`
   - **Walk Step Interval**: `0.6`
   - **Run Step Interval**: `0.4`
   - **Max Distance**: `12`
   - **Ground Layer**: Selecciona la capa del suelo

4. **NO necesitas configurar nada en ImpactAudioController**

### Paso 3: Verificar PlayerShootController

El `PlayerShootController` ya debería tener el código actualizado. Verifica que:
- Esté en el prefab del jugador
- Tenga los campos configurados (bulletImpact, playerImpact, etc.)

## 🐛 Solución de Problemas

### "No se escuchan sonidos"

**Verifica:**
1. ✅ El GameObject `AudioManager` tiene un `PhotonView`
2. ✅ Los nombres en `Audio Clips` coinciden EXACTAMENTE con los nombres de arriba
3. ✅ Los AudioClips están asignados correctamente
4. ✅ El jugador tiene `FootstepAudioController` e `ImpactAudioController`
5. ✅ Estás en modo Play y conectado a Photon

### "Los pasos no suenan"

**Verifica:**
1. ✅ El jugador se está moviendo (usa WASD)
2. ✅ `Ground Layer` está configurado en `FootstepAudioController`
3. ✅ Los nombres son `footstep_slow` y `footstep_fast` (con guión bajo)

### "Los disparos no suenan"

**Verifica:**
1. ✅ El `PlayerShootController` tiene el código actualizado
2. ✅ Los nombres de las armas contienen "pistol", "rifle" o "machinegun"
3. ✅ El AudioManager está en la escena

### "Error: AudioManager is null"

**Solución:**
- Asegúrate de que el GameObject `AudioManager` existe en la escena
- Verifica que el componente `AudioManager` esté agregado

## 🎯 Testing Rápido

1. Entra en modo Play
2. Conecta a Photon
3. Muévete con WASD → Deberías escuchar pasos
4. Dispara con Click → Deberías escuchar disparo
5. Si no funciona, revisa la consola en busca de errores

## 📝 Nombres Importantes

**¡ESTOS NOMBRES DEBEN SER EXACTOS!**
- `footstep_slow` (NO "footstep slow")
- `footstep_fast` (NO "footstep fast")
- `shot_pistol` (NO "shot pistol")
- `shot_rifle` (NO "shot rifle")
- `shot_machinegun` (NO "shot machinegun")
- `impact_wall` (NO "impact wall")
- `impact_player` (NO "impact player")
- `player_death` (NO "player death")

Los guiones bajos (_) son importantes porque así están definidos en el código.

