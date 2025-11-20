# B5 Shellshock Tank Game

A physics-based artillery game where two tanks battle across destructible terrain.

## Game Overview

Two tanks face each other on randomly generated terrain. Players take turns adjusting their tank's angle and power, then fire projectiles to destroy the opponent. The game features realistic physics, destructible terrain, and wind effects.

## How to Play

### Controls

**Player Controls (single human controls both tanks sequentially):**
- **A / Left Arrow**: Move active tank left (max 5 moves before firing)
- **D / Right Arrow**: Move active tank right (max 5 moves before firing)
- **W / Up Arrow**: Increase shooting angle (unlimited)
- **S / Down Arrow**: Decrease shooting angle (unlimited)
- **Q**: Decrease power (unlimited)
- **E**: Increase power (unlimited)
- **F**: Fire projectile (turn will switch only after collision)

### Game Rules

1. Players take turns controlling their tank
2. **During your turn, you can:**
   - Move your tank left/right up to **5 times** (A/D keys)
   - Adjust cannon angle **unlimited times** (W/S keys)
   - Adjust power **unlimited times** (Q/E keys)
3. **Your turn ends only after the fired projectile collides** (press **F** to shoot)
4. Projectile follows realistic physics with gravity and wind
5. Direct hits deal 50 damage
6. First tank to reach 0 health loses
7. Wind affects projectile trajectory (shown at top center)
8. Movement counter shows "Movements: X/5" and resets each turn

### Terrain Types

- **Flat**: Level battlefield (simplified for testing)

## Game Features

### Physics Simulation
- Realistic projectile trajectory with gravity (9.8 m/s²)
- Wind effects on projectile flight
- 40ms tick updates for smooth animation

### Destructible Terrain
- Destructible terrain temporarily disabled for testing
- Explosions register but don't modify terrain

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
- **Shooting flow**: Pressing **F** returns a Shoot move; Rules spawns the projectile and sets a pending turn switch.
- **Delayed switch**: The actual turn handoff occurs only after the projectile collides (hit ground or tank). At that moment movement count resets to 5 and the other tank becomes active.
- **Input blocking**: While a projectile is in flight, all inputs (except internal tick updates) are ignored to prevent mid-flight adjustments.
- **Movement limit**: Up to 5 left/right moves per turn before firing; angle and power changes are unlimited.

This approach cleanly decouples the framework's player swapping from game logic and guarantees the turn does not advance until the shot outcome is resolved.

### Classes

1. **B5_Shellshock_Field**: Game state container (IGameField)
2. **B5_Shellshock_Rules**: Game logic and physics (IGameRules, IGameRules2)
3. **B5_Shellshock_Painter**: Rendering engine (IPaintGame2)
4. **B5_Shellshock_Tank**: Tank object with position, angle, power, health
5. **B5_Shellshock_Projectile**: Physics-based projectile
6. **B5_Shellshock_Terrain**: Destructible height-mapped terrain
7. **B5_Shellshock_Move**: Player action wrapper (IPlayMove)
8. **B5_Shellshock_HumanPlayer**: Human player input handler (IHumanGamePlayer)

**Note:** AI player and destructible terrain temporarily disabled for testing

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
- **Damage per Hit**: 50 HP (2 hits to destroy)
- **Angle Range**: 0° - 180°
- **Power Range**: 0 - 100
- **Gravity**: 9.8 m/s²
- **Wind Range**: -5 to +5
- **Crater Radius**: 20 units

## Authors

B5 Team - Programmieren 3 WS2025
