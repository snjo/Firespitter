using System.Linq;
using UnityEngine;

namespace Firespitter.engine
{
    public class FSalternator : PartModule
    {
        [KSPField]
        public string resourceName = "ElectricCharge";
        [KSPField]
        public float chargeRate = 0.005f;

        private FSengineWrapper engine;
        private FSpropellerTweak propTweak;
        private int resourceID = 0;
        private float engineScaleMultiplier = 1f;

        public override void OnStart(PartModule.StartState state)
        {
            if (!HighLogic.LoadedSceneIsFlight) return;

            engine = new FSengineWrapper(part);
            resourceID = resourceName.GetHashCode();
            propTweak = part.Modules.OfType<FSpropellerTweak>().FirstOrDefault();
            if (propTweak != null)
            {
                engineScaleMultiplier = Mathf.Max(0.1f, propTweak.engineLengthSlider); // engine scale can be 0 or negative. That would be a bad multiplier.
            }
        }

        public override void OnFixedUpdate()
        {
            if (engine.type == FSengineWrapper.EngineType.FSengine)
            {
                part.RequestResource(resourceID, engine.fsengine.RPMnormalized * -chargeRate * engineScaleMultiplier);                
            }
            else
            {
                part.RequestResource(resourceID, engine.finalThrustNormalized * -chargeRate * engineScaleMultiplier);
            }
        }
    }
}
