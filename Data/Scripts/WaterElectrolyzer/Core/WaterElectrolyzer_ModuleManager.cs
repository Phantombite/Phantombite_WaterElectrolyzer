using System;
using System.Collections.Generic;

namespace Phantombite_WaterElectrolyzer
{
    public class WaterElectrolyzer_ModuleManager
    {
        private readonly List<WaterElectrolyzer_IModule> _modules = new List<WaterElectrolyzer_IModule>();

        public void Register(WaterElectrolyzer_IModule module)
        {
            _modules.Add(module);
        }

        public void Initialize()
        {
            foreach (var module in _modules)
            {
                try { module.Initialize(); }
                catch (Exception ex) { Log(ex.Message); }
            }
        }

        public void Update()
        {
            foreach (var module in _modules)
            {
                try { module.Update(); }
                catch (Exception ex) { Log(ex.Message); }
            }
        }

        public void Unload()
        {
            foreach (var module in _modules)
            {
                try { module.Unload(); }
                catch (Exception ex) { Log(ex.Message); }
            }
            _modules.Clear();
        }

        private void Log(string msg)
        {
            VRage.Utils.MyLog.Default.WriteLine("[WaterElectrolyzer] " + msg);
        }
    }
}
