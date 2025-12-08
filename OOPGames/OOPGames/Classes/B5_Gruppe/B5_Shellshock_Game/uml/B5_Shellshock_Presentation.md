# Abschlussvortrag: B5 Shellshock - OOP-Programmierübung

**Dauer:** 10 Minuten (±2 min)  
**Spiel:** Shellshock - Artillery Tank Combat Game

---

## 1. Spielbeschreibung & Vorführung (2 min)

### Was ist Shellshock?
- **Genre:** Turn-based Artillery Game (ähnlich Worms/Scorched Earth)
- **Spielprinzip:** 
  - 2 Panzer bekämpfen sich über destruktives Terrain
  - Spieler steuern Winkel, Schussleistung und Position
  - Ballistik-Physik mit Schwerkraft und Wind
  - Terrain kann zerstört werden (Explosionskrater)

### Spielablauf:
1. **Setup-Phase:** Startbildschirm, zufälliges Terrain wird generiert
2. **Spieler-Zug:** 
   - Bewegung (begrenzt pro Runde)
   - Winkel anpassen (W/S oder Pfeiltasten)
   - Schussleistung einstellen (Q/E)
   - Abfeuern (Space oder Mausklick)
3. **Projektil-Flug:** Physik-Simulation mit Parabel-Flugbahn
4. **Kollision:** Treffer auf Panzer → Schaden, Treffer auf Terrain → Zerstörung
5. **Rundenende:** Spielerwechsel oder Sieg bei zerstörtem Panzer

### Features:
- ✅ Realistische Ballistic-Physik
- ✅ Prozedural generiertes Terrain (4 verschiedene Generatoren)
- ✅ Destruktive Umgebung (Explosionskrater)
- ✅ Health-Packs zum Aufsammeln
- ✅ Flugbahn-Trails zur Visualisierung
- ✅ Wind-System (beeinflusst Flugbahn)

**[DEMO: Kurzes Spiel zeigen, 1-2 Schüsse demonstrieren]**

---

## 2. Framework-Integration (2 min)

### Wie integriert sich Shellshock in das OOPGames-Framework?

#### **Plugin-Architektur:**
Das Framework nutzt eine **Service Locator Pattern** mit Interfaces als Contracts:

```
Framework definiert:          Shellshock implementiert:
───────────────────          ────────────────────────
IGameRules              →    B5_Shellshock_Rules
IPaintGame              →    B5_Shellshock_Painter  
IGamePlayer             →    B5_Shellshock_HumanPlayer
IGameField              →    B5_Shellshock_Field
IPlayMove               →    B5_Shellshock_Move
```

#### **Registrierung (MainWindow.xaml.cs):**
```csharp
// In MainWindow.InitializeComponent():
OOPGamesManager.Singleton.RegisterRules(new B5_Shellshock_Rules());
OOPGamesManager.Singleton.RegisterPainter(new B5_Shellshock_Painter());
OOPGamesManager.Singleton.RegisterPlayer(new B5_Shellshock_HumanPlayer());
```

#### **Vorteil:**
- ✅ **Polymorphismus:** Framework arbeitet nur mit Interfaces
- ✅ **Erweiterbarkeit:** Neue Spiele ohne Framework-Änderung
- ✅ **Wiederverwendung:** UI, Timer, Input-Handling vom Framework

#### **Game Loop (40ms Tick):**
```
MainWindow (Framework)
    ↓
Timer.Tick (alle 40ms)
    ↓
Rules.TickGameCall()      → Physik-Updates (Projektil-Flug)
    ↓
Painter.TickPaintGameField()  → Animation & Rendering
```

**[UML-Diagramm zeigen: B5_Shellshock_Framework_Integration.puml]**

---

## 3. OOP-Konzepte & Klassen-Design (4 min)

### 3.1 Eigene Objekte/Klassen

#### **Klassen-Hierarchie:**

```
GameEntity (abstrakt)
├── CollidableEntity (abstrakt)
│   ├── Tank (konkret)
│   └── HealthPack (konkret)
└── Projectile (konkret)

TerrainGeneratorBase (abstrakt)
├── FlatTerrainGenerator
├── HillTerrainGenerator
├── CurvyTerrainGenerator
└── ValleyTerrainGenerator
```

### 3.2 Verwendete OOP-Prinzipien

#### **A) Vererbung (Inheritance)**

**Warum:** Code-Wiederverwendung, gemeinsame Eigenschaften abstrahieren

**Beispiel - GameEntity (Basisklasse):**
```csharp
public abstract class B5_Shellshock_GameEntity : IRenderable
{
    protected double _x, _y;
    protected bool _isActive;
    
    public abstract string EntityType { get; }  // Erzwingt Implementierung
    
    public double DistanceTo(B5_Shellshock_GameEntity other) {
        // Gemeinsame Logik für alle Entities
    }
}
```

**Nutzen:**
- Tank, Projectile, HealthPack **teilen** X/Y-Position, IsActive-Status
- **Eliminiert Duplikation:** Ohne Vererbung → 3x derselbe Code
- **Type Safety:** Alle Entities haben garantiert Position

---

#### **B) Abstrakte Klassen**

**Warum:** Template-Logik bereitstellen, aber Spezialisierung erzwingen

**Beispiel - CollidableEntity:**
```csharp
public abstract class B5_Shellshock_CollidableEntity 
    : B5_Shellshock_GameEntity, ICollidable
{
    // Template Method Pattern:
    public bool CollidesWith(B5_Shellshock_Projectile projectile)
    {
        var bounds = GetCollisionBounds();  // ← Polymorphisch!
        return projectile.X >= bounds.Left && ...
    }
    
    protected abstract Bounds GetCollisionBounds();  // Jede Subklasse anders
}
```

**Konkrete Implementierungen:**
- **Tank:** Rechteckige Kollisionsfläche (breit & niedrig)
- **HealthPack:** Quadratische Kollisionsfläche

**Nutzen:**
- **Template Method Pattern:** Algorithmus-Skelett in Basisklasse, Details in Subklassen
- **DRY-Prinzip:** Kollisions-Logik nur einmal implementiert
- **Flexibilität:** Jede Entity kann eigene Kollisionsform haben

---

#### **C) Interfaces**

**Neu erstellte Interfaces:**

**1. IRenderable**
```csharp
public interface IRenderable
{
    double X { get; }
    double Y { get; }
    bool IsActive { get; }
}
```
**Warum:** Painter kann alles rendern, was Position hat  
**Vorteil:** Lose Kopplung zwischen Rendering und Game-Logik

**2. ICollidable**
```csharp
public interface ICollidable
{
    bool CollidesWith(B5_Shellshock_Projectile projectile);
}
```
**Warum:** Nur Tank & HealthPack können getroffen werden (Projektil nicht!)  
**Vorteil:** Type Safety - verhindert unsinnige Operationen

**3. ITerrainGenerator** (Strategy Pattern)
```csharp
public interface ITerrainGenerator
{
    string Name { get; }
    void Generate(double[] heightMap, int width);
}
```
**Warum:** Austauschbare Terrain-Generierungs-Algorithmen  
**Vorteil:** 
- **Open/Closed Principle:** Neue Terrain-Typen ohne Änderung bestehenden Codes
- **Runtime-Austauschbarkeit:** Verschiedene Levels ohne Code-Änderung

---

#### **D) Polymorphie**

**Beispiel 1 - Kollisionserkennung:**
```csharp
// Rules.cs - funktioniert für Tank UND HealthPack:
foreach (ICollidable collidable in collidableObjects) 
{
    if (collidable.CollidesWith(projectile)) {
        // Tank: TakeDamage()
        // HealthPack: Heal() & Deactivate()
        // ← Unterschiedliches Verhalten, gleiche Schnittstelle!
    }
}
```

**Beispiel 2 - Entity-Typen:**
- `tank.EntityType` → "Tank (Red)" / "Tank (Blue)"
- `projectile.EntityType` → "Projectile (P1)" / "Projectile (P2)"
- **Gleiche Methode, unterschiedliche Ausgabe**

---

#### **E) Encapsulation (Kapselung)**

**Beispiel - Tank-Klasse:**
```csharp
public class B5_Shellshock_Tank
{
    private double _angle;    // ← Privat, nicht direkt zugänglich
    
    // Öffentliches Property mit Validation:
    public double Angle 
    {
        get => _angle;
        set => _angle = Math.Max(0, Math.Min(180, value));  // Invariante!
    }
}
```

**Invarianten (immer gültig):**
- Angle: [0°, 180°]
- Power: [0, 100]
- Health: ≥ 0

**Vorteil:**
- **Data Integrity:** Unmöglich, ungültige Werte zu setzen
- **Information Hiding:** Interne Implementierung kann sich ändern
- **Validation:** Alle Zugriffe werden überprüft

---

### 3.3 Design Patterns

#### **1. Strategy Pattern** (ITerrainGenerator)
**Problem:** Verschiedene Terrain-Generierungs-Algorithmen benötigt  
**Lösung:** Interface + Austauschbare Implementierungen

```csharp
public class B5_Shellshock_Terrain 
{
    private ITerrainGenerator _generator;  // ← Dependency Injection
    
    public void GenerateTerrain() {
        _generator.Generate(_heightMap, Width);  // Polymorphisch!
    }
}
```

**Konkrete Strategien:**
- `FlatTerrainGenerator` → Flaches Terrain mit minimalem Noise
- `HillTerrainGenerator` → Sine-Wave basierte Hügel
- `CurvyTerrainGenerator` → Perlin-Noise Landschaft
- `ValleyTerrainGenerator` → Tal in der Mitte

**Vorteile:**
- ✅ **Open/Closed Principle:** Neue Terrain-Typen ohne Änderung von Terrain.cs
- ✅ **Single Responsibility:** Jeder Generator hat EINE Aufgabe
- ✅ **Testbarkeit:** Generatoren isoliert testbar

---

#### **2. Template Method Pattern**
**Problem:** Terrain-Generatoren brauchen gleiche Pipeline (Generate → Smooth → Clamp)  
**Lösung:** Abstrakte Basisklasse definiert Algorithmus-Skelett

```csharp
public abstract class TerrainGeneratorBase : ITerrainGenerator
{
    // Template Method (finales Algorithmus-Skelett):
    public void Generate(double[] heightMap, int width)
    {
        GenerateRaw(heightMap, width);      // ← Subklasse
        ApplySmoothing(heightMap, width);   // ← Gemeinsam
        ClampHeights(heightMap);            // ← Gemeinsam
    }
    
    protected abstract void GenerateRaw(...);  // Individuell
    protected void ApplySmoothing(...) { }     // Geteilt
}
```

**Vorteile:**
- ✅ Code-Wiederverwendung (Smoothing nur einmal)
- ✅ Konsistenz (alle Generatoren durchlaufen gleiche Pipeline)
- ✅ Erweiterbarkeit (neue Generatoren nur GenerateRaw() implementieren)

---

#### **3. Factory Pattern**
**Problem:** Client sollte nicht wissen, welcher Generator instanziiert wird  
**Lösung:** Factory kapselt Objekterzeugung

```csharp
public static class TerrainGeneratorFactory
{
    public static ITerrainGenerator Create(TerrainType type)
    {
        return type switch {
            TerrainType.Flat => new FlatTerrainGenerator(),
            TerrainType.Hill => new HillTerrainGenerator(),
            // ...
        };
    }
    
    public static ITerrainGenerator CreateRandom() {
        var types = Enum.GetValues<TerrainType>();
        return Create(types[random.Next(types.Length)]);
    }
}
```

**Vorteil:** Zentralisierte Objekterzeugung, einfache Erweiterung

---

## 4. Framework-Grenzen & Erweiterungen (1.5 min)

### Grenzen des Frameworks:

#### **1. Kein eingebautes Physik-System**
**Problem:** Framework hat keinen Game Loop für kontinuierliche Updates  
**Lösung:** `IGameRules2.TickGameCall()` genutzt (wird alle 40ms aufgerufen)

```csharp
public void TickGameCall()
{
    if (_field.ProjectileInFlight) {
        UpdateProjectilePhysics(deltaTime: 0.04);  // 40ms
        CheckCollisions();
    }
}
```

**Erweiterung:** Eigene Physik-Engine implementiert (Schwerkraft, Parabel-Flug)

---

#### **2. Keine Animation-Unterstützung**
**Problem:** Framework rendert nur nach Moves  
**Lösung:** `IPaintGame2.TickPaintGameField()` für kontinuierliches Rendering

```csharp
public void TickPaintGameField(Canvas canvas, IGameField field)
{
    // Wird 25x pro Sekunde aufgerufen → flüssige Animation
    DrawProjectileWithTrail(canvas, field);
}
```

---

#### **3. Input-Handling zu generisch**
**Problem:** Framework gibt nur `IMoveSelection` (Click oder Key)  
**Lösung:** Eigener Input-Adapter in HumanPlayer

```csharp
public IPlayMove GetMove(IMoveSelection selection, IGameField field)
{
    if (selection is IKeySelection keySelection) {
        return keySelection.Key switch {
            Key.W or Key.Up => new Move(ActionType.AdjustAngleUp),
            Key.S or Key.Down => new Move(ActionType.AdjustAngleDown),
            Key.Space => new Move(ActionType.Shoot),
            // ...
        };
    }
}
```

**Erweiterung:** Multi-Key-Support (Bewegung + Zielen gleichzeitig)

---

#### **4. Keine State Machine im Framework**
**Problem:** Framework hat kein Konzept für Spielphasen  
**Lösung:** Eigene State Machine in Rules implementiert

```csharp
public enum B5_Shellshock_GamePhase
{
    Setup,              // Startbildschirm
    PlayerTurn,         // Spieler zielt/bewegt
    ProjectileInFlight, // Physik-Simulation
    GameOver            // Sieg-Bildschirm
}
```

---

### Framework-Erweiterungen:

✅ **Eigene Interfaces** (IRenderable, ICollidable, ITerrainGenerator)  
✅ **Physik-Engine** (Ballistik, Kollisionserkennung)  
✅ **Terrain-System** (Prozedural, destruktiv)  
✅ **Animation-System** (Trails, Partikeln)  
✅ **State Machine** (Spielphasen-Verwaltung)

**Wichtig:** Framework wurde NICHT modifiziert, nur erweitert!  
→ **Open/Closed Principle:** Offen für Erweiterung, geschlossen für Änderung

---

## 5. Software-Entwurfs-Prinzipien (1.5 min)

### SOLID-Prinzipien:

#### **S - Single Responsibility Principle**
✅ **Jede Klasse hat EINE Aufgabe:**
- `B5_Shellshock_Rules` → Game Logic
- `B5_Shellshock_Painter` → Rendering
- `B5_Shellshock_Field` → State Storage
- `B5_Shellshock_Terrain` → Terrain Management

**Vorteil:** Leicht zu verstehen, zu testen, zu ändern

---

#### **O - Open/Closed Principle**
✅ **Offen für Erweiterung, geschlossen für Änderung:**

**Beispiel:** Neue Terrain-Typen hinzufügen:
```csharp
// KEINE Änderung in Terrain.cs nötig!
public class MountainTerrainGenerator : TerrainGeneratorBase {
    protected override void GenerateRaw(...) { /* neue Logik */ }
}

// Nur Factory erweitern:
TerrainType.Mountain => new MountainTerrainGenerator()
```

---

#### **L - Liskov Substitution Principle**
✅ **Subklassen sind substituierbar:**

```csharp
ICollidable collidable = GetRandomCollidable();  // Tank ODER HealthPack
bool hit = collidable.CollidesWith(projectile);  // Funktioniert für beide!
```

**Vor-/Nachbedingungen dokumentiert:**
```csharp
/// <summary>
/// Precondition: projectile must be active
/// Postcondition: Returns true if collision detected, object state unchanged
/// </summary>
public bool CollidesWith(Projectile projectile) { ... }
```

---

#### **I - Interface Segregation Principle**
✅ **Kleine, fokussierte Interfaces:**

Statt ein großes `IGameEntity` Interface:
```csharp
interface IRenderable { /* nur Rendering-relevantes */ }
interface ICollidable { /* nur Kollisions-relevantes */ }
```

**Vorteil:** Projectile implementiert NUR IRenderable (keine unnötigen Methoden)

---

#### **D - Dependency Inversion Principle**
✅ **Abhängigkeit von Abstraktionen, nicht Konkretionen:**

```csharp
public class Terrain 
{
    private ITerrainGenerator _generator;  // ← Interface, nicht konkrete Klasse!
    
    public Terrain(ITerrainGenerator generator) {
        _generator = generator;  // Dependency Injection
    }
}
```

**Vorteil:** Terrain ist unabhängig von konkretem Generator-Typ

---

### Weitere Prinzipien:

#### **DRY - Don't Repeat Yourself**
✅ Collision-Logic nur in `CollidableEntity` (nicht in Tank + HealthPack dupliziert)

#### **Separation of Concerns**
✅ **MVC-ähnliche Struktur:**
- Model: `Field` (Data)
- View: `Painter` (Rendering)
- Controller: `Rules` (Logic)

#### **Composition over Inheritance**
✅ Terrain **hat** einen `ITerrainGenerator` (nicht: Terrain **ist** ein Generator)

---

## 6. Zusammenfassung & Lessons Learned (1 min)

### Implementierte OOP-Konzepte:

| Konzept | Beispiel | Nutzen |
|---------|----------|--------|
| **Vererbung** | GameEntity → CollidableEntity → Tank | Code-Wiederverwendung |
| **Abstrakte Klassen** | TerrainGeneratorBase | Template-Logik + Spezialisierung |
| **Interfaces** | ICollidable, ITerrainGenerator | Polymorphismus, lose Kopplung |
| **Polymorphie** | CollidesWith() für Tank & HealthPack | Flexibilität, Erweiterbarkeit |
| **Encapsulation** | Tank.Angle mit Validation | Data Integrity |
| **Strategy Pattern** | Austauschbare Terrain-Generatoren | Open/Closed Principle |
| **Template Method** | Generate() Pipeline | Code-Wiederverwendung |
| **Factory Pattern** | TerrainGeneratorFactory | Zentralisierte Objekterzeugung |

### Lessons Learned:

✅ **Framework-Integration:** Plugin-Architektur ermöglicht saubere Trennung  
✅ **Design Patterns:** Reduzieren Komplexität und verbessern Wartbarkeit  
✅ **SOLID-Prinzipien:** Führen zu flexiblem, erweiterbarem Code  
✅ **Abstraktion:** Interfaces ermöglichen polymorphe Nutzung  
✅ **Dokumentation:** XML-Kommentare mit Vor-/Nachbedingungen essentiell  

### Technische Highlights:

🎯 **Ballistik-Physik** mit realistischer Schwerkraft  
🎯 **Prozedurales Terrain** mit 4 verschiedenen Generatoren  
🎯 **Destruktive Umgebung** mit Explosionskratern  
🎯 **Smooth Animation** dank 40ms-Tick-System  
🎯 **Vollständige OOP-Architektur** mit klarer Hierarchie  

---

## Anhang: Demo-Checkliste

**Vor der Präsentation:**
- [ ] Projekt builden (`dotnet build`)
- [ ] OOPGames.exe starten
- [ ] Shellshock in Dropdowns auswählen
- [ ] 2x Human Player einstellen
- [ ] "Start Game" klicken

**Während Demo:**
1. ✅ Startbildschirm zeigen (Space drücken)
2. ✅ Terrain-Variation zeigen (mehrmals neu starten)
3. ✅ Bewegung demonstrieren (A/D)
4. ✅ Zielen demonstrieren (W/S)
5. ✅ Power anpassen (Q/E)
6. ✅ Schuss abfeuern (Space)
7. ✅ Parabel-Flugbahn zeigen
8. ✅ Treffer zeigen (Health-Anzeige)
9. ✅ Terrain-Zerstörung zeigen (Explosionskrater)
10. ✅ Sieg-Bildschirm zeigen

**UML-Diagramme bereithalten:**
- `B5_Shellshock_Core_Architecture.puml` (Klassen-Struktur)
- `B5_Shellshock_Framework_Integration.puml` (Framework-Integration)

---

## Timing-Guide:

| Abschnitt | Zeit | Inhalt |
|-----------|------|--------|
| 1. Spielbeschreibung | 2 min | Was ist Shellshock? + Demo |
| 2. Framework-Integration | 2 min | Plugin-Architektur, Interfaces |
| 3. OOP-Konzepte | 4 min | Vererbung, Interfaces, Patterns |
| 4. Framework-Grenzen | 1.5 min | Limitationen & Lösungen |
| 5. Design-Prinzipien | 1.5 min | SOLID, DRY, SoC |
| 6. Zusammenfassung | 1 min | Lessons Learned |
| **Gesamt** | **12 min** | **Puffer für Fragen: -2 min** |

---

**Viel Erfolg bei der Präsentation! 🎮🚀**
