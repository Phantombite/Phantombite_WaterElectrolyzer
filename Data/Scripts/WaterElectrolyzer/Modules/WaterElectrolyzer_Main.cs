using System.Collections.Generic;
using Sandbox.Common.ObjectBuilders;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces.Terminal;
using VRage;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRage.Utils;

namespace Phantombite_WaterElectrolyzer
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_HydrogenEngine), false, "WaterElectrolyzerInput")]
    public class WaterElectrolyzer_MotorLogic : MyGameLogicComponent
    {
        private const string ELECTROLYZER_SUBTYPE = "WaterElectrolyzerOutput";
        private const long   LOG_CHANNEL      = 1995999L;
        private const string MOD_NAME         = "Phantombite_WaterElectrolyzer";
        private const string ITEM_SUBTYPE         = "WaterFuel";
        private const float  WATER_PER_SECOND     = 10f;
        private const int    TICK_INTERVAL        = 60;

        private IMyFunctionalBlock _motor;
        private IMyGasGenerator    _electrolyzer;
        private bool               _initialized;
        private int                _tick       = 58;
        private int                _startDelay = 30;

        private readonly List<IMySlimBlock> _reusableNeighbours = new List<IMySlimBlock>();
        private bool _disabledByPerf = false; // Wurde der Motor durch PerfLevel deaktiviert?

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            if (!MyAPIGateway.Multiplayer.IsServer && MyAPIGateway.Multiplayer.MultiplayerActive)
                return;

            _motor = Entity as IMyFunctionalBlock;
            if (_motor == null) return;

            NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
        }

        public override void UpdateOnceBeforeFrame()
        {
            if (_motor == null) return;

            _motor.CubeGrid.OnBlockAdded   += OnBlockAdded;
            _motor.CubeGrid.OnBlockRemoved += OnBlockRemoved;
            _motor.IsWorkingChanged        += OnMotorWorkingChanged;
            _motor.EnabledChanged          += OnMotorEnabledChanged;

            // On/Off Control ausgr auen wenn PerfLevel >= 3
            try
            {
                System.Collections.Generic.List<IMyTerminalControl> controls;
                MyAPIGateway.TerminalControls.GetControls<Sandbox.ModAPI.IMyHydrogenEngine>(out controls);
                foreach (var ctrl in controls)
                {
                    if (ctrl.Id == "OnOff")
                    {
                        ctrl.Enabled = b => b.BlockDefinition.SubtypeId != "WaterElectrolyzerInput"
                                         || WaterElectrolyzer_Session.PerfLevel < 3;
                        break;
                    }
                }
            }
            catch { }

            _initialized = true;
            NeedsUpdate |= MyEntityUpdateEnum.EACH_FRAME;
        }

        public override void UpdateBeforeSimulation()
        {
            if (!_initialized || _motor == null) return;

            if (_startDelay > 0)
            {
                _startDelay--;
                if (_startDelay == 0)
                    FindPartner();
                return;
            }

            if (_electrolyzer == null) return;
            if (!_motor.IsWorking)     return;

            // PerfLevel 3: Motor deaktivieren damit H2 nicht verloren geht
            if (WaterElectrolyzer_Session.PerfLevel >= 3)
            {
                if (!_disabledByPerf)
                {
                    _disabledByPerf = true;
                    _motor.Enabled  = false;
                    Log("PerfLevel 3: Motor deaktiviert — kein H2-Verlust");
                }
                return;
            }
            else if (_disabledByPerf)
            {
                _disabledByPerf = false;
                _motor.Enabled  = true;
                Log("PerfLevel 0: Motor wieder aktiviert");
            }

            _tick++;
            if (_tick < TICK_INTERVAL) return;
            _tick = 0;

            var inventory = _electrolyzer.GetInventory(0);
            if (inventory == null) return;

            inventory.AddItems((MyFixedPoint)WATER_PER_SECOND,
                MyObjectBuilderSerializer.CreateNewObject<MyObjectBuilder_Ore>(ITEM_SUBTYPE));
            Log("Produktion: +" + WATER_PER_SECOND + " WaterFuel", 1);
        }

        private void FindPartner()
        {
            _reusableNeighbours.Clear();
            _motor.SlimBlock.GetNeighbours(_reusableNeighbours);

            foreach (var neighbour in _reusableNeighbours)
            {
                if (neighbour.FatBlock == null) continue;

                var gen = neighbour.FatBlock as IMyGasGenerator;
                if (gen != null && gen.BlockDefinition.SubtypeId == ELECTROLYZER_SUBTYPE)
                {
                    Log("FindPartner: Elektrolyseur gefunden");
                    SetPartner(gen);
                    return;
                }
            }

            Log("FindPartner: kein Elektrolyseur gefunden");
            SetPartner(null);
        }

        private void SetPartner(IMyGasGenerator electrolyzer)
        {
            _electrolyzer = electrolyzer;

            if (_electrolyzer == null)
            {
                _motor.Enabled = false;
                return;
            }

            _electrolyzer.Enabled             = _motor.Enabled;
            _electrolyzer.AutoRefill          = false;
            _electrolyzer.UseConveyorSystem   = false;
            _electrolyzer.ShowInInventory     = false;
            _electrolyzer.ShowInToolbarConfig = false;
        }

        private void OnMotorWorkingChanged(IMyCubeBlock block)
        {
            if (_electrolyzer == null) return;
            _electrolyzer.Enabled = _motor.IsWorking;
        }

        private void OnMotorEnabledChanged(IMyTerminalBlock block)
        {
            if (_electrolyzer == null && _motor.Enabled)
                _motor.Enabled = false;
        }

        private void OnBlockAdded(IMySlimBlock block)
        {
            if (_electrolyzer != null) return;
            if (block.FatBlock == null) return;

            var gen = block.FatBlock as IMyGasGenerator;
            if (gen != null && gen.BlockDefinition.SubtypeId == ELECTROLYZER_SUBTYPE)
                SetPartner(gen);
        }

        private void OnBlockRemoved(IMySlimBlock block)
        {
            if (_electrolyzer == null)           return;
            if (block.FatBlock != _electrolyzer) return;
            SetPartner(null);
        }

        private void Log(string msg, int level = 0)
        {
            try
            {
                MyLog.Default.WriteLineAndConsole("[PB.WaterElectrolyzer] [" + level + "] " + msg);
                MyAPIGateway.Utilities.SendModMessage(LOG_CHANNEL,
                    "LOG|" + MOD_NAME + "|" + level + "|WaterElectrolyzer_Motor|" + msg);
            }
            catch { }
        }

        public override void Close()
        {
            if (_motor == null) return;
            _motor.CubeGrid.OnBlockAdded   -= OnBlockAdded;
            _motor.CubeGrid.OnBlockRemoved -= OnBlockRemoved;
            _motor.IsWorkingChanged        -= OnMotorWorkingChanged;
            _motor.EnabledChanged          -= OnMotorEnabledChanged;
        }
    }

    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_OxygenGenerator), false, "WaterElectrolyzerOutput")]
    public class WaterElectrolyzer_GeneratorLogic : MyGameLogicComponent
    {
        private IMyGasGenerator _generator;
        private static bool     _controlsHidden;

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            _generator = Entity as IMyGasGenerator;
            if (_generator == null) return;

            NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
        }

        public override void UpdateOnceBeforeFrame()
        {
            if (_generator == null) return;

            if (!_controlsHidden)
            {
                List<IMyTerminalControl> controls;
                MyAPIGateway.TerminalControls.GetControls<IMyGasGenerator>(out controls);

                foreach (var control in controls)
                {
                    switch (control.Id)
                    {
                        case "OnOff":
                        case "Auto-Refill":
                        case "UseConveyor":
                        case "Refill":
                            control.Visible = b => b.BlockDefinition.SubtypeId != "WaterElectrolyzerOutput";
                            break;
                    }
                }

                _controlsHidden = true;
            }
        }

        public override void Close()
        {
            _generator = null;
        }
    }
}