B1 TicTacToe Integration
=========================

Kurz: Dieses Repo enthält eine B1-spezifische, objektorientierte TicTacToe-Implementierung.

Was wurde hinzugefügt
- `OOPGames/Classes/B1_Gruppe/*` — Domain-Klassen (Symbol, Cross, Circle, Cell, Board, Player)
- `B1_TicTacToe.cs` — Painter, Rules, Field, Move, Human- und Computer-Player (Minimax)
- `OOPGames.Tests` — MSTest-Projekt mit einfachen Tests für `B1_Board`

Wie man baut und testet (PowerShell)
1) Build mit eurer Task oder dotnet build:

```powershell
dotnet build ".\OOPGames\OOPGames\OOPGames.csproj" -c Debug
```

2) App starten:

```powershell
dotnet run --project ".\OOPGames\OOPGames\OOPGames.csproj" -c Debug
```

3) Tests ausführen:

```powershell
dotnet test ".\OOPGames\OOPGames.Tests\OOPGames.Tests.csproj"
```

Hinweis
- Die `B1_TicTacToeField` verwendet intern `B1_Board` und mappt zur bestehenden IX_TicTacToeField-API, sodass die vorhandenen Painter/Player-Registrierungen kompatibel bleiben.
