using VRage.Game;
using VRage.Game.Components;

namespace Phantombite_WaterElectrolyzer
{
    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
    public class WaterElectrolyzer_Session : MySessionComponentBase
    {
        // Logik laeuft vollstaendig ueber MyGameLogicComponent auf den Bloecken.
        // Session wird nur benoetigt damit der Namespace korrekt geladen wird.
    }
}
