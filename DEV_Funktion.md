# DEV Funktion — Phantombite WaterElectrolyzer

Stand: 2026-09-19 · Version 1.0.0 · Workshop-ID 3708390650 · Core-Kanal 1995012

## Zweck
Erzeugt Wasser aus Wasserstoff/Sauerstoff-Anlagen: Ein Wasserstoffmotor (`WaterElectrolyzerInput`) liefert
im Sekundentakt `WaterFuel` in einen benachbarten Elektrolyseur (`WaterElectrolyzerOutput`).

## Zusammenspiel der Blöcke
- **Motor** (`MyObjectBuilder_HydrogenEngine`, `WaterElectrolyzerInput`): sucht 30 Frames nach dem Start
  einen direkt angrenzenden Elektrolyseur. Findet er keinen, schaltet er sich ab.
- **Elektrolyseur** (`MyObjectBuilder_OxygenGenerator`, `WaterElectrolyzerOutput`): Bedienelemente wie An/Aus,
  Auto-Refill und Conveyor sind ausgeblendet, seine Steuerung übernimmt der Motor.
- Pro Sekunde (60 Ticks) werden 10 `WaterFuel` ins Inventar des Elektrolyseurs gelegt, solange der Motor arbeitet.

## Dateien
```
Data/Blueprints/, PhysicalItems/PhysicalItems.sbc      WaterFuel und Blaupausen
Data/CubeBlocks/CubeBlocks_Logistics.sbc, _Production.sbc, Cubeblocks_Category.sbc
Data/Scripts/WaterElectrolyzer/Modules/
  WaterElectrolyzer session.cs   Session: Core-Anbindung, statischer PerfLevel und Log-Level
  WaterElectrolyzer_Main.cs      Motor- und Generator-Logik
```

## Core-Anbindung
- Meldet sich als `waterelectrolyzer` an (keine Commands). Empfängt `LOGLEVEL` und `PERFLEVEL`.
- **Performance-Level 3:** Motor wird abgeschaltet (kein Wasserstoffverlust), die An/Aus-Taste ist gesperrt.
  Unter Level 3 schaltet sich der Motor wieder ein.
- Log-Meldungen werden nach dem Log-Level des Core gefiltert.

## Offene Punkte / Roadmap
- [ ] Angleichen an `0_Phantombite_MOD_TEMPLATE.md` (README, patch_notes, thumb.jpg, Ordner `Core/` mit Session/IModule/ModuleManager)
- [ ] Die Steuerung „An/Aus“ wird für alle Wasserstoffmotoren gesetzt (Bedingung prüft nur den Subtyp) — im Spiel gegenprüfen
