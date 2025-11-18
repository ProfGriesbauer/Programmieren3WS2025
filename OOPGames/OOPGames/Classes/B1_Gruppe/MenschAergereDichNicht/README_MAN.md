Mensch Ärger dich nicht (Gruppe B1)

Dieses Verzeichnis enthält die objektorientierte Basisimplementierung für das klassische Brettspiel "Mensch ärgere dich nicht".

Enthaltene Klassen:
- B1_MAN_Piece.cs: Repräsentiert eine Spielstein
- B1_MAN_Player.cs: Repräsentiert einen Spieler (4 Steine)
- B1_MAN_Board.cs: Simplifiziertes Brettmodell (Hauptloop 0..39, Home-Slots)
- B1_MAN_Rules.cs: Basis-Regeln: Würfeln, valide Züge, Ausführen von Zügen

Ziele:
- 2..4 Spieler (Mensch oder Computer)
- Schlanker, gut testbarer Domain-Layer

Nächste Schritte:
- Implementiere Computer-Spieler (KI / Heuristik)
- UI-Painter und Integration in das OOPGames-Framework
- Unit-Tests für Move-Logik (Schlagen, Heimzug)
