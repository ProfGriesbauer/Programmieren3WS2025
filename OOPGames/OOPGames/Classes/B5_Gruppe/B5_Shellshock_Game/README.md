# B5 Shellshock Tank Game

A physics-based artillery game where two tanks battle across destructible terrain.

## Game Overview

Two tanks face each other on randomly generated terrain. Players take turns adjusting their tank's angle and power, then fire projectiles to destroy the opponent. The game features realistic physics, destructible terrain, and wind effects.

## How to Play

### Controls

**Player Controls (single human controls both tanks sequentially):**
- **A / Left Arrow**: Move active tank left (3 moves for Flat terrain, 7 for others)
- **D / Right Arrow**: Move active tank right (3 moves for Flat terrain, 7 for others)
- **W / Up Arrow**: Increase shooting angle (unlimited)
- **S / Down Arrow**: Decrease shooting angle (unlimited)
- **Q**: Decrease power (unlimited)
- **E**: Increase power (unlimited)
- **Space / Click**: Fire projectile (turn will switch only after collision)

### Game Rules

1. Players take turns controlling their tank
2. **During your turn, you can:**
   - Move your tank left/right up to **3 times** (Flat terrain) or **7 times** (other terrains)
   - Adjust cannon angle **unlimited times** (W/S keys)
   - Adjust power **unlimited times** (Q/E keys)
3. **Your turn ends only after the fired projectile collides** (press **Space/Click** to shoot)
4. Projectile follows realistic physics with gravity and wind
5. Direct hits deal 20 damage (5 hits to destroy)
6. First tank to reach 0 health loses
7. Wind affects projectile trajectory (shown at top center)
8. Movement counter shows "Movements: X/3" (or X/7) and resets each turn
9. Health packs spawn randomly (50% chance after each shot) - shoot them for +25 HP healing

### Terrain Types

- **Flat**: Level battlefield with minimal noise (3 movements per turn)
- **Hill**: Sine-wave based hills (7 movements per turn)
- **Curvy**: Perlin-noise based varied terrain (7 movements per turn)
- **Valley**: V-shaped valley terrain (not currently in random rotation)

Terrain is randomly selected at game start.

## Game Features

### Physics Simulation
- Realistic projectile trajectory with gravity (9.8 m/s²)
- Wind effects on projectile flight
- 40ms tick updates for smooth animation

### Destructible Terrain
- Projectile impacts create craters (30 pixel radius)
- Terrain deformation affects tank positioning
- Tanks automatically adjust to terrain changes

### Visual Elements
- Color-coded tanks (Red vs Blue)
- Health bars with dynamic colors (Green > Orange > Red)
- Power bars showing current power setting
- Angle indicators on tanks
- Wind indicator with directional arrow
- Smooth projectile animation

## Implementation Details

### Turn System

The turn system is tank-centric and delays switching until the projectile has finished its flight:

- **Single human controller**: One human player operates both tanks in alternating turns.
- **Active tank tracking**: `ActiveTankNumber` in the Field indicates which tank is currently under control.
- **Direct state mutation**: Movement, angle, and power keys mutate the active tank directly and return `null` moves so the framework does not auto-switch players prematurely.
- **Shooting flow**: Pressing **Space/Click** returns a Shoot move; Rules spawns the projectile and sets a pending turn switch.
- **Delayed switch**: The actual turn handoff occurs only after the projectile collides (hit ground, tank, or health pack). At that moment movement count resets to 3 (Flat) or 7 (other terrains) and the other tank becomes active.
- **Input blocking**: While a projectile is in flight, all inputs (except internal tick updates) are ignored to prevent mid-flight adjustments.
- **Movement limit**: Up to 3 left/right moves per turn (Flat terrain) or 7 moves (Hill/Curvy); angle and power changes are unlimited.

This approach cleanly decouples the framework's player swapping from game logic and guarantees the turn does not advance until the shot outcome is resolved.

### Classes

#### OOP Konzepte (German/English Documentation)

**Vererbungshierarchie (Inheritance Hierarchy):**

```
IRenderable (Interface)
└── B5_Shellshock_GameEntity (abstrakte Klasse)
    ├── B5_Shellshock_CollidableEntity (abstrakte Klasse, implements ICollidable)
    │   ├── B5_Shellshock_Tank (konkrete Klasse)
    │   └── B5_Shellshock_HealthPack (konkrete Klasse)
    └── B5_Shellshock_Projectile (konkrete Klasse)

ITerrainGenerator (Interface)
└── B5_Shellshock_TerrainGeneratorBase (abstrakte Klasse)
    ├── B5_Shellshock_FlatTerrainGenerator (konkrete Klasse)
    ├── B5_Shellshock_HillTerrainGenerator (konkrete Klasse)
    ├── B5_Shellshock_CurvyTerrainGenerator (konkrete Klasse)
    └── B5_Shellshock_ValleyTerrainGenerator (konkrete Klasse)
```

**Interfaces:**
- `ICollidable`: Ermöglicht polymorphe Kollisionserkennung
- `IRenderable`: Definiert Position und Aktivitätsstatus für Rendering
- `ITerrainGenerator`: Strategy Pattern für Terrain-Generierung

**Abstrakte Klassen (Abstract Classes):**
- `B5_Shellshock_GameEntity`: Basisklasse für alle Spielobjekte
- `B5_Shellshock_CollidableEntity`: Erweitert GameEntity mit Kollisionsfähigkeit
- `B5_Shellshock_TerrainGeneratorBase`: Template Method Pattern für Terrain

**Konkrete Klassen (Concrete Classes):**
1. **B5_Shellshock_Field**: Game state container (IGameField)
2. **B5_Shellshock_Rules**: Game logic and physics (IGameRules, IGameRules2)
3. **B5_Shellshock_Painter**: Rendering engine (IPaintGame2)
4. **B5_Shellshock_Tank**: Tank mit Vererbung von CollidableEntity
5. **B5_Shellshock_Projectile**: Physik-basiertes Projektil
6. **B5_Shellshock_Terrain**: Nutzt Strategy Pattern für Generierung
7. **B5_Shellshock_HealthPack**: Sammelbare Heilung

**OOP Prinzipien implementiert:**

| Konzept | Implementierung |
|---------|-----------------|
| **Vererbung** | GameEntity → CollidableEntity → Tank/HealthPack |
| **Polymorphie** | GetCollisionBounds() in Tank vs HealthPack |
| **Kapselung** | Private Felder, öffentliche Properties mit Validierung |
| **Abstrakte Klassen** | GameEntity, CollidableEntity, TerrainGeneratorBase |
| **Interfaces** | ICollidable, IRenderable, ITerrainGenerator |
| **Liskov-Prinzip** | Tank/HealthPack substituierbar für ICollidable |
| **Vorbedingungen** | Dokumentiert in XML-Kommentaren |
| **Nachbedingungen** | Dokumentiert in XML-Kommentaren |
| **Invarianten** | Angle [0,180], Power [0,100], Health ≥ 0 |
| **Factory Pattern** | TerrainGeneratorFactory |
| **Strategy Pattern** | ITerrainGenerator Implementierungen |
| **Template Method** | TerrainGeneratorBase.Generate() |

**Note:** Computer player is not currently implemented - human vs human gameplay only

### Registration

To enable the game, add to `MainWindow.xaml.cs`:

```csharp
OOPGamesManager.Singleton.RegisterPainter(new B5_Shellshock_Painter());
OOPGamesManager.Singleton.RegisterRules(new B5_Shellshock_Rules());
OOPGamesManager.Singleton.RegisterPlayer(new B5_Shellshock_HumanPlayer());
```

## Technical Details

- **Framework**: OOPGames (WPF)
- **Language**: C#
- **Rendering**: WPF Canvas with Polygons, Rectangles, Lines, Ellipses
- **Physics**: Custom implementation with deltaTime integration
- **Update Rate**: 40ms (25 FPS)

## Game Balance

- **Tank Health**: 100 HP
- **Damage per Hit**: 20 HP (5 hits to destroy)
- **Health Pack Healing**: +25 HP (shoot to collect)
- **Angle Range**: 0° - 180°
- **Power Range**: 0 - 100
- **Gravity**: 9.8 m/s²
- **Wind Range**: -5 to +5
- **Crater Radius**: 30 units
- **Movement per Turn**: 3 (Flat) or 7 (Hill/Curvy)

## Authors

B5 Team - Programmieren 3 WS2025
