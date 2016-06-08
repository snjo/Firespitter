using System.Linq;
using UnityEngine;

namespace Firespitter.engine
{
    class FSengineWrapper
    {
        public enum EngineType
        {
            NONE,
            ModuleEngine,
            ModuleEngineFX,
            FSengine,
        }
        public EngineType type = EngineType.NONE;
        public ModuleEngines engine;
        public ModuleEnginesFX engineFX;
        public FSengine fsengine;

        public FSengineWrapper(Part part)
        {
            engine = part.Modules.OfType<ModuleEngines>().FirstOrDefault();
            if (engine != null)
            {
                type = EngineType.ModuleEngine;
            }
            else
            {
                engineFX = part.Modules.OfType<ModuleEnginesFX>().FirstOrDefault();
                if (engineFX != null)
                {
                    type = EngineType.ModuleEngineFX;
                }
                else
                {
                    fsengine = part.Modules.OfType<FSengine>().FirstOrDefault();
                    if (fsengine != null)
                    {
                        type = EngineType.FSengine;
                    }
                }
            }
            //Debug.Log("FSengineWrapper: engine type is " + type.ToString());
        }

        public FSengineWrapper(Part part, string name)
        {
            engineFX = part.Modules.OfType<ModuleEnginesFX>().Where(p => p.engineID == name).FirstOrDefault();
            if (engineFX != null)
                type = EngineType.ModuleEngineFX;
            //Debug.Log("FSengineWrapper: engine type is " + type.ToString());
            
        }

        public float maxThrust
        {
            get
            {
                switch (type)
                {
                    case EngineType.ModuleEngine:
                        return engine.maxThrust;
                    case EngineType.ModuleEngineFX:
                        return engineFX.maxThrust;
                    case EngineType.FSengine:
                        return fsengine.maxThrust;
                    default:
                        return 0f;
                }
            }
            set
            {
                switch (type)
                {
                    case EngineType.ModuleEngine:
                        engine.maxThrust = value;
                        break;
                    case EngineType.ModuleEngineFX:
                        engineFX.maxThrust = value;
                        break;
                    case EngineType.FSengine:
                        fsengine.maxThrust = value;
                        break;
                }
            }
        }

        public float minThrust
        {
            get
            {
                switch (type)
                {
                    case EngineType.ModuleEngine:
                        return engine.minThrust;
                    case EngineType.ModuleEngineFX:
                        return engineFX.minThrust;
                    //case EngineType.FSengine:
                    //    return fsengine.minThrust;
                    default:
                        return 0f;
                }
            }
            set
            {
                switch (type)
                {
                    case EngineType.ModuleEngine:
                        engine.minThrust = value;
                        break;
                    case EngineType.ModuleEngineFX:
                        engineFX.minThrust = value;
                        break;
                    //case EngineType.FSengine:
                    //    fsengine.minThrust = value;
                    //    break;
                }
            }
        }

        public bool EngineIgnited
        {
            get
            {
                switch (type)
                {
                    case EngineType.ModuleEngine:
                        return engine.EngineIgnited;
                    case EngineType.ModuleEngineFX:
                        return engineFX.EngineIgnited;
                    case EngineType.FSengine:
                        return fsengine.EngineIgnited;
                    default:
                        return false;
                }
            }
            set
            {
                switch (type)
                {
                    case EngineType.ModuleEngine:
                        engine.EngineIgnited = value;
                        break;
                    case EngineType.ModuleEngineFX:
                        engineFX.EngineIgnited = value;
                        break;
                    case EngineType.FSengine:
                        fsengine.EngineIgnited = value;
                        break;
                }
            }
        }

        public bool flameout
        {
            get
            {
                switch (type)
                {
                    case EngineType.ModuleEngine:
                        return engine.flameout;
                    case EngineType.ModuleEngineFX:
                        return engineFX.flameout;
                    case EngineType.FSengine:
                        return fsengine.flameout;
                    default:
                        return false;
                }
            }
            set
            {
                switch (type)
                {
                    case EngineType.ModuleEngine:
                        engine.flameout = value;
                        break;
                    case EngineType.ModuleEngineFX:
                        engineFX.flameout = value;
                        break;
                    case EngineType.FSengine:
                        fsengine.flameout = value;
                        break;
                }
            }
        }

        public bool getIgnitionState
        {
            get
            {
                switch (type)
                {
                    case EngineType.ModuleEngine:
                        return engine.getIgnitionState;
                    case EngineType.ModuleEngineFX:
                        return engineFX.getIgnitionState;
                    case EngineType.FSengine:
                        return fsengine.EngineIgnited;
                    default:
                        return false;
                }
            }            
        }

        public bool getFlameoutState
        {
            get
            {
                switch (type)
                {
                    case EngineType.ModuleEngine:
                        return engine.getFlameoutState;
                    case EngineType.ModuleEngineFX:
                        return engineFX.getFlameoutState;
                    case EngineType.FSengine:
                        return fsengine.flameout;
                    default:
                        return false;
                }
            }
        }

        public float engineAccelerationSpeed
        {
            get
            {
                switch (type)
                {
                    case EngineType.ModuleEngine:
                        return engine.engineAccelerationSpeed;
                    case EngineType.ModuleEngineFX:
                        return engineFX.engineAccelerationSpeed;
                    case EngineType.FSengine:
                        return fsengine.powerProduction;  // not an accurate alternative
                    default:
                        return 0f;
                }
            }
            set
            {
                switch (type)
                {
                    case EngineType.ModuleEngine:
                        engine.engineAccelerationSpeed = value;
                        break;
                    case EngineType.ModuleEngineFX:
                        engineFX.engineAccelerationSpeed = value;
                        break;
                    case EngineType.FSengine:
                        fsengine.powerProduction = value;  // not an accurate alternative
                        break;
                }
            }
        }

        public float engineDecelerationSpeed
        {
            get
            {
                switch (type)
                {
                    case EngineType.ModuleEngine:
                        return engine.engineDecelerationSpeed;
                    case EngineType.ModuleEngineFX:
                        return engineFX.engineDecelerationSpeed;
                    case EngineType.FSengine:
                        return fsengine.powerDrain; // not an accurate alternative
                    default:
                        return 0f;
                }
            }
            set
            {
                switch (type)
                {
                    case EngineType.ModuleEngine:
                        engine.engineDecelerationSpeed = value;
                        break;
                    case EngineType.ModuleEngineFX:
                        engineFX.engineDecelerationSpeed = value;
                        break;
                    case EngineType.FSengine:
                        fsengine.powerDrain = value;  // not an accurate alternative
                        break;
                }
            }
        }

        public float finalThrust
        {
            get
            {
                switch (type)
                {
                    case EngineType.ModuleEngine:
                        return engine.finalThrust;
                    case EngineType.ModuleEngineFX:
                        return engineFX.finalThrust;
                    case EngineType.FSengine:
                        return fsengine.finalThrust; // not an accurate alternative
                    default:
                        return 0f;
                }
            }
            set
            {
                switch (type)
                {
                    case EngineType.ModuleEngine:
                        engine.finalThrust = value;
                        break;
                    case EngineType.ModuleEngineFX:
                        engineFX.finalThrust = value;
                        break;
                    case EngineType.FSengine:
                        fsengine.finalThrust = value;  // not an accurate alternative
                        break;
                }
            }
        }

        public float finalThrustNormalized
        {
            get
            {
                return finalThrust / maxThrust;
            }
        }

        public bool throttleLocked
        {
            get
            {
                switch (type)
                {
                    case EngineType.ModuleEngine:
                        return engine.throttleLocked;
                    case EngineType.ModuleEngineFX:
                        return engineFX.throttleLocked;
                    //case EngineType.FSengine:
                    //    return fsengine.throttleLocked;
                    default:
                        return false;
                }
            }
            set
            {
                switch (type)
                {
                    case EngineType.ModuleEngine:
                        engine.throttleLocked = value;
                        break;
                    case EngineType.ModuleEngineFX:
                        engineFX.throttleLocked = value;
                        break;
                    //case EngineType.FSengine:
                    //    fsengine.flameout = value;
                    //    break;
                }
            }
        }
    }
}
