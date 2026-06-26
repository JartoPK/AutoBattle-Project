# AutoBattle Project — Documentación de scripts

> Documento vivo. **Se actualiza cada vez que se añaden o cambian scripts.**
> Pensado para que cualquier desarrollador (incluido quien hace el **combate**)
> sepa qué hace cada pieza y dónde está la frontera entre capas.

Juego de táctica en tiempo real (referente: *Bad North*): escuadrón de 3 unidades
autónomas, órdenes en tiempo real, permadeath, campaña de conquista persistente.
Unity 6 (6000.3.18f1) · URP · Input System (solo el nuevo).

---

## 1. Arquitectura general (módulos / asmdefs)

El código vive en `Assets/_Project/` y se divide en módulos con Assembly Definitions
que **compilan por separado**:

| Módulo | Asmdef | Qué contiene | Depende de |
|---|---|---|---|
| **Core** | `AutoBattle.Core` | Contrato compartido + infraestructura (datos de unidad, eventos, guardado) | — |
| **Meta** | `AutoBattle.Meta` | Capa de gestión: roster, economía, base, reclutamiento, herencia, árbol, campaña | Core |
| **App** | `AutoBattle.App` | Presentación: mundos 3D + UI (mapa, base, paneles) | Core, Meta, UnityEngine.UI, Unity.InputSystem |
| **Combat** | `AutoBattle.Combat` | *(vacío)* El combate lo construye la otra persona | Core |

**Regla de oro:** `Meta` y `Combat` son hermanos: **ambos dependen de `Core`, pero
NO se referencian entre sí**. El combate solo necesita leer tipos de `Core`.

> **El combate (Fases 2–6) NO está hecho.** Todo lo de este documento es la capa
> "meta" (fuera del combate) + su UI.

---

## 2. La frontera Meta ↔ Combate (lo que necesita el dev de combate)

Lo único que el combate consume de esta capa:

- **`UnitInstance`** (`Core/Units`): la unidad concreta. El combate la **lee** para
  instanciar tropas. Usar **siempre `UnitInstance.EffectiveStats`** (no `baseStats`)
  para las stats finales.
- **`ClassData` / `PassiveData` / `OrderData` / `SpellData`** (`Core/Units`): definiciones
  (datos). Su **comportamiento** (cómo ataca, qué hace una orden/pasiva/hechizo) se
  implementa en `Combat`, leyendo el `id` correspondiente.
- **Flags del árbol de mejoras**: el combate pregunta qué desbloqueó el jugador con
  `UpgradeService.IsFlagUnlocked(state, tree, effectType)` y
  `UpgradeService.GetUnlockedTargets(...)` (3.ª ranura, órdenes/pasivas/órdenes globales).
- **`CampaignNodeData.formation`** (`Meta/Campaign`): la composición enemiga de un nodo
  (qué clases y dónde) — base para montar el encuentro.

> **Pendiente de definir** (cuando se aborde el combate): el contrato formal de batalla
> `BattleRequest` (3 unidades + formación rival) / `BattleResult` (quién murió, HP mínimo)
> e interfaz `ICombatRunner`. Aún no existe.

---

## 3. Módulo Core (`Assets/_Project/Core`)

### Infraestructura
- **`GameManager.cs`** — Singleton persistente (`Instance`). Mantiene `State` (`GameState`)
  y publica `GameStateChangedEvent` al cambiarlo con `ChangeState(next)`.
- **`GameState.cs`** — Enum: `Boot, MainMenu, CampaignMap, Base, Deployment, Combat, PostCombat`.
- **`Events/IGameEvent.cs`** — Interfaz marcador de eventos (implementar como struct).
- **`Events/EventBus.cs`** — Bus pub/sub estático y tipado: `Subscribe<T>`, `Unsubscribe<T>`,
  `Publish<T>`, `Clear()`. Desacopla sistemas (no es thread-safe; hilo principal).
- **`Events/CoreEvents.cs`** — `GameStateChangedEvent { Previous, Current }`.
- **`Save/SaveSystem.cs`** — Persistencia JSON en `persistentDataPath/saves/{clave}.json`:
  `Save<T>`, `TryLoad<T>`, `Exists`, `Delete`, `DeleteAll`. (JsonUtility: sin Dictionary ni polimorfismo).

### Unidades (datos — el "ADN" del juego)
- **`UnitClass.cs`** — Enum: `Guerrero, Arquero, Mago`.
- **`Rarity.cs`** — Enum ordenado: `Comun < PocoComun < Rara < Epica < Legendaria`.
- **`StatRange.cs`** — `struct { float min, max; Roll(rng) }`. Tirada en [min,max].
- **`UnitStats.cs`** — `struct { hp, attack, attackSpeed (ataques/s), moveSpeed, mana }`.
- **`UnitInstance.cs`** — ⭐ Unidad serializable del roster. Campos clave:
  `id`, `displayName`, **`hasClass`** (las reclutas nacen sin clase), `classId`, `rarity`,
  `baseStats`, `passiveId`, `battlesSurvived`. Propiedad **`EffectiveStats`** (hoy = baseStats;
  único punto donde irán modificadores futuros).
- **`PassiveData.cs`** (SO) — `id, displayName, rarity, description, icon`. Una unidad solo
  puede tener pasivas de rareza ≤ la suya.
- **`OrderData.cs`** (SO) — `id, displayName, description, icon`. Orden de unidad (comportamiento en Combat).
- **`SpellData.cs`** (SO) — `id, displayName, description, manaCost, icon`. Hechizo de mago.
- **`ClassData.cs`** (SO) — Molde de clase: `classId, displayName, unitPrefab, usesMana,
  possiblePassives[], availableOrders[], spells[]`. **No** lleva stats (van en el baremo universal).
- **`UnitGenerationConfig.cs`** (SO) — Baremo **universal** de stats: `StatRange hp, attack,
  attackSpeed, moveSpeed, mana`. Igual para todas las clases ("ADN puro").
- **`RarityConfig.cs`** (SO) — Por rareza, una ventana de percentil [0..1] del baremo
  (`GetPercentileWindow(rarity)`): común tira bajo, legendaria alto.
- **`ClassDatabase.cs`** (SO) — Resuelve `UnitClass → ClassData` (`Get(classId)`), para save/load.
- **`UnitFactory.cs`** — Crea unidades. `CreateClassless(...)` (solo stats, sin clase),
  `AssignClass(unit, classData, ...)` (fija clase, rola maná y pasiva), `Create(...)` (las dos juntas).
- **`NameGenerator.cs`** — `Next(rng)`: nombre aleatorio para reclutas.
- **`Roster.cs`** — `[Serializable]` contenedor: `List<UnitInstance> units` + `Add/Remove/Get/Count`.

### Editor
- **`Editor/DefaultDataSetup.cs`** — Menú `AutoBattle/Setup/Crear datos por defecto` (clases,
  pasivas, config, database) y `AutoBattle/Debug/Generar 3 unidades de prueba`.

---

## 4. Módulo Meta (`Assets/_Project/Meta`)

### Estado y guardado
- **`MetaGameState.cs`** — ⭐ Estado persistente de la partida: `roster`, `wallet`,
  `baseLevel`, `upgrades` (`UpgradeState`) + modificadores del árbol
  (`upgradeBonusRosterCap`, `recruitQualityBonus`, `inheritBonus`). Helpers:
  `GetRosterCap(baseConfig)`, `GetInheritPercent(baseConfig)`. Fábrica `New(startingCoins)`.
- **`MetaSaveService.cs`** — `Save/Load/Exists/DeleteSave` del `MetaGameState` (clave "savegame").
- **`GameConfig.cs`** (SO) — Punto único de datos para runtime (en `Resources/`). Refs:
  `generationConfig, classDatabase, rarityConfig, baseConfig, recruitmentConfig, upgradeTree,
  campaignMap`, `startingCoins`.

### Economía y base
- **`Economy/Wallet.cs`** — `[Serializable]` monedero: `coins`, `CanAfford`, `Add`, `Spend`.
- **`Base/BaseConfig.cs`** (SO) — `baseRosterCap`, `rosterCapPerLevel`, `statInheritPercent`.
  `GetRosterCap(baseLevel)`. El tope de roster crece con el nivel de base.

### Reclutamiento y herencia
- **`Recruitment/RecruitmentConfig.cs`** (SO) — `tiers[]` (RecruitTier: `displayName, cost,
  rarityTable[]` = ruleta ponderada) + refs (`generationConfig, classDatabase, rarityConfig`).
- **`Recruitment/RecruitmentService.cs`** — `Recruit(state, tier, config, baseConfig)`:
  valida tope+monedas, tira rareza en la ruleta, crea recluta **sin clase**. Devuelve `RecruitResult`.
- **`Recruitment/ClassAssignmentService.cs`** — `Assign(state, unitId, chosenClass, config)`:
  el jugador asigna la clase a una recluta (rola pasiva y maná). No reasigna clases ya fijadas.
- **`Inheritance/InheritanceService.cs`** — `Inherit(state, veteranId, chosenClass, config, baseConfig)`:
  consume un veterano y crea una recluta que hereda su pasiva y un % de stats; la clase la elige el jugador.

### Árbol de mejoras
- **`Upgrades/UpgradeBranch.cs`** — Enum: `Comandante, Guerrero, Arquero, Mago`.
- **`Upgrades/UpgradeEffectType.cs`** — Efectos **meta** (IncreaseBaseLevel, IncreaseRosterCap,
  RecruitQualityBonus, InheritanceQualityBonus) y **flags de combate** (UnlockOrder, UnlockPassive,
  UnlockGlobalOrder, UnlockThirdOrderSlot, IncreaseStatCap).
- **`Upgrades/UpgradeNode.cs`** (SO) — `id, displayName, description, branch, cost,
  requiredBaseLevel, prerequisiteIds[], effect, effectValue, effectTargetId`.
- **`Upgrades/UpgradeTree.cs`** (SO) — `nodes[]`; `Get(id)`, `GetByBranch(branch)`.
- **`Upgrades/UpgradeState.cs`** — `[Serializable]` lista de ids desbloqueados (`IsUnlocked`, `Unlock`).
- **`Upgrades/UpgradeService.cs`** — `CanPurchase`, `Purchase` (aplica efecto meta o registra flag),
  y consultas para el combate: **`IsFlagUnlocked`**, **`GetUnlockedTargets`**.

### Campaña
- **`Campaign/CampaignNodeData.cs`** (SO) — Nodo del mapa: `id, displayName, type (NodeType),
  difficulty (1-5), coinReward, mapPosition (x,z), formation (EnemyUnit[])`.
  `NodeType = { Combate, Elite, Jefe, Reclutamiento, Recursos }`.
  `EnemyUnit = { unitClass, col, row }` (posición en rejilla 4x3).
- **`Campaign/CampaignMap.cs`** (SO) — `nodes[]`: todos los nodos del mapa.

### Editor
- **`Editor/BaseDataSetup.cs`** — Menú: crear datos de base/economía + test del ciclo (reclutar/herencia/guardado).
- **`Editor/UpgradeDataSetup.cs`** — Menú: crear árbol por defecto + test de compra.
- **`Editor/AppDataSetup.cs`** — Menú **`AutoBattle/Setup/Crear TODO + GameConfig (para la UI)`**
  (crea todo + `GameConfig` + `CampaignMap` en `Resources/`) y `AutoBattle/Debug/Borrar partida guardada`.

---

## 5. Módulo App (`Assets/_Project/App`) — presentación

Se arranca solo al dar **Play** (sin tocar escenas), construyendo todo por código.
UI funcional **placeholder** (cubos y uGUI), pendiente de arte/escena reales.

- **`GameBootstrap.cs`** — ⭐ Entrada (`[RuntimeInitializeOnLoadMethod]`). Crea cámara 3D,
  luz, EventSystem (`InputSystemUIInputModule` + `PhysicsRaycaster`), canvas, mundos y UI.
  Gestiona el cambio mapa ↔ base (`ShowMap/ShowBase`).
- **`GameContext.cs`** — Estado vivo para la UI: `Config` (GameConfig) + `State` (MetaGameState) + `Save()`.
- **`UIFactory.cs`** — Helpers uGUI por código: `Panel, Label, Button, Stretch, Anchor`, `DefaultFont`.
- **`WorldBuilder.cs`** — Helpers 3D: `Ground, Box, Label3D, Lit(color)` (material URP).
- **`ClickableBuilding.cs`** — `IPointerClickHandler` para objetos 3D: invoca `OnClicked`.
- **`MapWorld.cs`** — Mapa 3D: genera un marcador por nodo del `CampaignMap` (color por tipo),
  clicable. Cámara isométrica.
- **`MapPanHandler.cs`** — `IDragHandler` sobre el suelo: arrastrar mueve la cámara (X/Z, con límites).
- **`NodeInfoPanel.cs`** — Modal al pulsar un nodo: tipo, dificultad, recompensa y la **formación
  enemiga** (rejilla de cuadros por clase). Botón "Combatir" de marcador.
- **`BaseWorld.cs`** — Base 3D con edificios clicables (**Reclutamiento**, **Mejoras**) y las
  **tropas como cubos** (`RefreshTroops`, color por rareza) que deambulan sin atravesar edificios.
- **`Wanderer.cs`** — Mueve un cubo lentamente por el suelo evitando obstáculos (huellas de edificios).
- **`Hud.cs`** — HUD 2D: monedas + botones de contexto (BASE / VOLVER).
- **`RecruitPanel.cs`** — Modal de reclutamiento: botones de tier (con **ruleta**), lista del roster
  con todas las stats y **asignación de clase** a las reclutas sin clase.
- **`RarityRoulette.cs`** — Ruleta visual: gira por las rarezas y para en la obtenida (solo visual).
- **`RarityVisuals.cs`** — `Of(rarity)` (color) y `Name(rarity)` (nombre legible).
- **`UpgradeTreePanel.cs`** — Modal del árbol: nodos por rama con estado (comprado/comprable/bloqueado)
  y compra vía `UpgradeService`.

---

## 6. Cómo poner en marcha (recordatorio)

1. En Unity: **`AutoBattle → Setup → Crear TODO + GameConfig (para la UI)`** (crea datos + GameConfig).
2. **Play**: aparece el mapa. Botón **BASE** (abajo dcha.) → base con edificios clicables.
3. Menús de prueba en `AutoBattle/Debug/...`. Reiniciar partida: `Borrar partida guardada`.

---

## 7. Estado por fases (GDD)

| Fase | Qué | Estado |
|---|---|---|
| 0 | Andamiaje (Core, EventBus, GameManager, SaveSystem) | ✅ |
| 1 | Unidades y datos | ✅ |
| 2–6 | **Combate** (auto-ataque, órdenes, magos, heridas) | ⬜ *(otra persona)* |
| 7 | Campaña (mapa, nodos, deployment) | 🟡 mapa + info de nodo (sin deployment/combate) |
| 8 | Base y economía (reclutamiento, herencia, árbol) | ✅ |
| — | UI/UX (mundos 3D + paneles) | 🟡 placeholder funcional |
