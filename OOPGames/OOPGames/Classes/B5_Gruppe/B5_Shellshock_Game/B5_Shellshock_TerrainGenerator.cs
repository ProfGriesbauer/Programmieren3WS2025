using System;

namespace OOPGames
{
    /// <summary>
    /// Interface für Terrain-Generatoren.
    /// Ermöglicht polymorphe Terrain-Erzeugung mit verschiedenen Strategien.
    /// 
    /// Design Pattern: Strategy Pattern
    /// - Definiert eine Familie von Algorithmen (Terrain-Generierung)
    /// - Macht sie austauschbar
    /// - Lässt den Algorithmus unabhängig vom Client variieren
    /// 
    /// Vertrag (Contract):
    /// - Vorbedingung: heightMap.Length == width
    /// - Nachbedingung: alle Werte in heightMap sind im Bereich [0.0, 1.0]
    /// </summary>
    public interface ITerrainGenerator
    {
        /// <summary>Name des Generator-Algorithmus</summary>
        string Name { get; }
        
        /// <summary>
        /// Generiert Terrain-Höhenwerte.
        /// </summary>
        /// <param name="heightMap">Array für Höhenwerte (wird gefüllt)</param>
        /// <param name="width">Breite des Terrains in Pixeln</param>
        void Generate(double[] heightMap, int width);
    }

    /// <summary>
    /// Abstrakte Basisklasse für Terrain-Generatoren.
    /// Implementiert gemeinsame Funktionalität wie Smoothing.
    /// 
    /// OOP Concepts:
    /// - Abstrakte Klasse: Kann nicht direkt instanziiert werden
    /// - Template Method Pattern: Generate definiert Ablauf, 
    ///   Subklassen implementieren GenerateRaw
    /// - Vererbung: Subklassen erben Smoothing-Methode
    /// </summary>
    public abstract class B5_Shellshock_TerrainGeneratorBase : ITerrainGenerator
    {
        /// <summary>Name des Generators (muss von Subklassen implementiert werden)</summary>
        public abstract string Name { get; }
        
        /// <summary>Anzahl der Smoothing-Durchläufe</summary>
        protected virtual int SmoothingPasses => 3;

        /// <summary>
        /// Template Method: Definiert den Ablauf der Terrain-Generierung.
        /// 1. Subklasse generiert rohe Höhenwerte
        /// 2. Basisklasse wendet Smoothing an
        /// 
        /// Vorbedingung: heightMap.Length == width, width > 0
        /// Nachbedingung: heightMap enthält geglättete Werte in [0, 1]
        /// </summary>
        public void Generate(double[] heightMap, int width)
        {
            if (heightMap == null || heightMap.Length != width || width <= 0)
                throw new ArgumentException("Invalid heightMap or width");
            
            // Step 1: Let subclass generate raw terrain
            GenerateRaw(heightMap, width);
            
            // Step 2: Apply smoothing (can be overridden)
            ApplySmoothing(heightMap, width, SmoothingPasses);
            
            // Step 3: Clamp values to valid range
            ClampValues(heightMap, width);
        }

        /// <summary>
        /// Abstrakte Methode: Muss von Subklassen implementiert werden.
        /// Generiert die rohen Terrain-Höhenwerte ohne Smoothing.
        /// </summary>
        protected abstract void GenerateRaw(double[] heightMap, int width);

        /// <summary>
        /// Virtuelle Methode: Kann von Subklassen überschrieben werden.
        /// Wendet Smoothing auf das Terrain an.
        /// </summary>
        protected virtual void ApplySmoothing(double[] heightMap, int width, int passes)
        {
            for (int pass = 0; pass < passes; pass++)
            {
                double[] smoothed = new double[width];
                for (int i = 0; i < width; i++)
                {
                    if (i == 0)
                        smoothed[i] = (heightMap[i] * 2 + heightMap[i + 1]) / 3.0;
                    else if (i == width - 1)
                        smoothed[i] = (heightMap[i - 1] + heightMap[i] * 2) / 3.0;
                    else
                        smoothed[i] = (heightMap[i - 1] + heightMap[i] * 2 + heightMap[i + 1]) / 4.0;
                }
                Array.Copy(smoothed, heightMap, width);
            }
        }

        /// <summary>
        /// Begrenzt Werte auf gültigen Bereich [0.5, 0.92].
        /// </summary>
        protected virtual void ClampValues(double[] heightMap, int width)
        {
            for (int i = 0; i < width; i++)
            {
                heightMap[i] = Math.Max(0.5, Math.Min(0.92, heightMap[i]));
            }
        }
    }

    /// <summary>
    /// Konkrete Implementierung: Flaches Terrain.
    /// Demonstriert Vererbung und Polymorphie.
    /// </summary>
    public class B5_Shellshock_FlatTerrainGenerator : B5_Shellshock_TerrainGeneratorBase
    {
        private readonly Random _rand = new Random();
        
        public override string Name => "Flat";
        protected override int SmoothingPasses => 3;

        protected override void GenerateRaw(double[] heightMap, int width)
        {
            for (int i = 0; i < width; i++)
            {
                heightMap[i] = 0.8 + (_rand.NextDouble() * 0.01 - 0.005);
            }
        }
    }

    /// <summary>
    /// Konkrete Implementierung: Hügel-Terrain.
    /// Demonstriert unterschiedliche Generierungs-Strategie.
    /// </summary>
    public class B5_Shellshock_HillTerrainGenerator : B5_Shellshock_TerrainGeneratorBase
    {
        private readonly Random _rand = new Random();
        
        public override string Name => "Hill";
        protected override int SmoothingPasses => 5;

        protected override void GenerateRaw(double[] heightMap, int width)
        {
            double hillCenter = 0.3 + _rand.NextDouble() * 0.4;
            double hillHeight = 0.15 + _rand.NextDouble() * 0.15;
            double hillWidth = 0.2 + _rand.NextDouble() * 0.3;
            double leftSlope = 0.5 + _rand.NextDouble() * 1.5;
            double rightSlope = 0.5 + _rand.NextDouble() * 1.5;
            
            for (int i = 0; i < width; i++)
            {
                double x = (double)i / width;
                double distFromCenter = Math.Abs(x - hillCenter);
                
                if (distFromCenter < hillWidth)
                {
                    double slope = x < hillCenter ? leftSlope : rightSlope;
                    double normalizedDist = distFromCenter / hillWidth;
                    double hillFactor = Math.Pow(1 - normalizedDist, slope);
                    heightMap[i] = 0.85 - hillHeight * hillFactor;
                }
                else
                {
                    heightMap[i] = 0.85;
                }
            }
        }
    }

    /// <summary>
    /// Konkrete Implementierung: Gewelltes Terrain.
    /// Verwendet Sinus-Wellen für sanfte Hügel.
    /// </summary>
    public class B5_Shellshock_CurvyTerrainGenerator : B5_Shellshock_TerrainGeneratorBase
    {
        private readonly Random _rand = new Random();
        
        public override string Name => "Curvy";
        protected override int SmoothingPasses => 8;

        protected override void GenerateRaw(double[] heightMap, int width)
        {
            double phase1 = _rand.NextDouble() * 10;
            double phase2 = _rand.NextDouble() * 10;
            
            for (int i = 0; i < width; i++)
            {
                double value = 0;
                double frequency = 0.008;
                double amplitude = 0.12;
                
                value += Math.Sin(i * frequency * 2 + phase1) * amplitude;
                value += Math.Sin(i * frequency * 4 + phase2) * amplitude * 0.4;
                
                heightMap[i] = 0.75 + value;
            }
        }
    }

    /// <summary>
    /// Konkrete Implementierung: Tal-Terrain (V-Form).
    /// </summary>
    public class B5_Shellshock_ValleyTerrainGenerator : B5_Shellshock_TerrainGeneratorBase
    {
        public override string Name => "Valley";
        protected override int SmoothingPasses => 0; // No smoothing needed

        protected override void GenerateRaw(double[] heightMap, int width)
        {
            for (int i = 0; i < width; i++)
            {
                double x = (double)i / width;
                heightMap[i] = 0.5 + Math.Abs(x - 0.5) * 0.6;
            }
        }
    }

    /// <summary>
    /// Factory für Terrain-Generatoren.
    /// Demonstriert Factory Pattern für Objekterzeugung.
    /// 
    /// Vorteile:
    /// - Kapselt Instanziierungs-Logik
    /// - Ermöglicht einfache Erweiterung mit neuen Terrain-Typen
    /// - Client-Code ist unabhängig von konkreten Generator-Klassen
    /// </summary>
    public static class B5_Shellshock_TerrainGeneratorFactory
    {
        /// <summary>
        /// Erzeugt den passenden Generator für den angegebenen Terrain-Typ.
        /// 
        /// Polymorphie: Rückgabetyp ist ITerrainGenerator,
        /// aber es wird eine konkrete Implementierung zurückgegeben.
        /// </summary>
        public static ITerrainGenerator Create(B5_Shellshock_TerrainType terrainType)
        {
            return terrainType switch
            {
                B5_Shellshock_TerrainType.Flat => new B5_Shellshock_FlatTerrainGenerator(),
                B5_Shellshock_TerrainType.Hill => new B5_Shellshock_HillTerrainGenerator(),
                B5_Shellshock_TerrainType.Curvy => new B5_Shellshock_CurvyTerrainGenerator(),
                B5_Shellshock_TerrainType.Valley => new B5_Shellshock_ValleyTerrainGenerator(),
                _ => new B5_Shellshock_FlatTerrainGenerator()
            };
        }

        /// <summary>
        /// Gibt einen zufälligen Generator zurück.
        /// </summary>
        public static ITerrainGenerator CreateRandom()
        {
            B5_Shellshock_TerrainType[] types = { 
                B5_Shellshock_TerrainType.Flat, 
                B5_Shellshock_TerrainType.Hill, 
                B5_Shellshock_TerrainType.Curvy 
            };
            return Create(types[new Random().Next(types.Length)]);
        }
    }
}
