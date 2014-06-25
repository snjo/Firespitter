
namespace Firespitter.engine
{
    class FSengineHandCrank : PartModule
    {
        private FSengineWrapper engine;

        public override void OnStart(PartModule.StartState state)
        {
            base.OnStart(state);
            engine = new FSengineWrapper(part);
            if (engine.type == FSengineWrapper.EngineType.ModuleEngine)
            {
                engine.engine.Events["Activate"].guiActiveUnfocused = true;
                engine.engine.Events["Activate"].unfocusedRange = 5f;
                engine.engine.Events["Shutdown"].guiActiveUnfocused = true;
                engine.engine.Events["Shutdown"].unfocusedRange = 5f;
            }
            else if (engine.type == FSengineWrapper.EngineType.ModuleEngineFX)
            {
                engine.engineFX.Events["Activate"].guiActiveUnfocused = true;
                engine.engineFX.Events["Activate"].unfocusedRange = 5f;
                engine.engineFX.Events["Shutdown"].guiActiveUnfocused = true;
                engine.engineFX.Events["Shutdown"].unfocusedRange = 5f;
            }
        }

        /*
        [KSPEvent(name = "ignitionOn", guiActive = false, active = true, guiName = "Ignition On", externalToEVAOnly = true, unfocusedRange = 6f, guiActiveUnfocused = true)]
        public void ignitionOnEvent()
        {
            if (engine != null)
            {
                //gameObject.active = true;
                //engine.staged = true;
                engine.EngineIgnited = true;            
            }
        }

        [KSPEvent(name = "ignitionOff", guiActive = false, active = true, guiName = "Ignition Off", externalToEVAOnly = true, unfocusedRange = 6f, guiActiveUnfocused = true)]
        public void ignitionOffEvent()
        {
            if (engine != null)
            {
                //engine.staged = true;
                engine.EngineIgnited = false;
            }
        }*/
    }
}