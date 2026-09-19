# Phantombite WaterElectrolyzer

Erzeugt **Wasser** mit einer Elektrolyse-Anlage: Ein Wasserstoffmotor liefert jede Sekunde 10 `WaterFuel` in einen direkt
angrenzenden Elektrolyseur.

## Funktionen
- Zwei zusammenarbeitende Blöcke: Motor (`WaterElectrolyzerInput`) und Elektrolyseur (`WaterElectrolyzerOutput`)
- Der Motor findet den angrenzenden Elektrolyseur selbst. Ohne Partner schaltet er sich ab.
- Bei hoher Serverlast pausiert die Produktion (der Motor wird abgeschaltet, damit kein Wasserstoff verloren geht) und startet
  danach wieder von selbst (über den Phantombite Core)

## Commands
Keine.

## Voraussetzungen
Keine. Mit dem **Phantombite Core** wirkt die Laststeuerung.

Workshop-ID: 3708390650
