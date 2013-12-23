using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using System.Reflection;
using UnityEngine;

public class FSwingletRangeAdjustment : PartModule
{
    private float defaultRange;
    ControlSurface winglet = new ControlSurface();
    private bool FARActive = false;

    [KSPField]
    public float stepAngle = 5f;
    [KSPField]
    public float lockModifier = 0.001f; //remove?
    [KSPField]
    public float maxRange = 60f;

    [KSPField(guiActive = true, guiName = "Control range", isPersistant = true, guiActiveEditor=true), UI_FloatRange(controlEnabled=true, minValue=0f, maxValue=50f, stepIncrement=0.02f)]
    public float currentControlRange = 16;
    public float oldControlRange = 16;
    private bool currentControlRangeSet = false;
    private bool locked = false;

    [KSPEvent(name = "decreaseRange", active = true, guiActive = true, guiName = "Decrease control range", guiActiveEditor=true)]
    public void decreaseRangeEvent()
    {
        alterRange(-stepAngle);
    }
    [KSPAction("Decrease Control Range")]
    public void decreaseRangeAction(KSPActionParam param)
    {
        alterRange(-stepAngle);
    }

    [KSPEvent(name = "increaseRange", active = true, guiActive = true, guiName = "Increase control range", guiActiveEditor=true)]
    public void increaseRangeEvent()
    {
        alterRange(stepAngle);
    }
    [KSPAction("Increase Control Range")]
    public void increaseRangeAction(KSPActionParam param)
    {
        alterRange(stepAngle);
    }

    private void alterRange(float amount)
    {
        currentControlRange += amount;
        if (currentControlRange < 0) currentControlRange = 0;
        if (currentControlRange > maxRange) currentControlRange = maxRange;
        winglet.ctrlSurfaceRange = currentControlRange;
    }

    [KSPEvent(name = "lockRange", active = true, guiActive = true, guiName = "Toggle lock")]
    public void lockRangeEvent()
    {
        lockRange();
    }
    [KSPAction("Toggle lock")]
    public void lockRangeAction(KSPActionParam param)
    {
        lockRange();
    }

    private void lockRange()
    {
        if (!locked)
        {
            winglet.ctrlSurfaceRange = 0;
            locked = true;
        }
        else
        {
            winglet.ctrlSurfaceRange = currentControlRange;
            locked = false;
        }
    }

    public override void OnUpdate()
    {
        if (!FARActive)
        {
            if (currentControlRange != oldControlRange)
            {
                currentControlRange = Mathf.Round(currentControlRange);
                winglet.ctrlSurfaceRange = currentControlRange;
                oldControlRange = currentControlRange;
            }
        }
    }

    public override void OnStart(PartModule.StartState state)
    {
        base.OnStart(state);
        FARActive = AssemblyLoader.loadedAssemblies.Any(a => a.assembly.GetName().Name.Equals("FerramAerospaceResearch", StringComparison.InvariantCultureIgnoreCase));
        // This line breaks the plugin :(
        if (FARActive)
        {
            foreach (BaseField f in Fields)
            {
                f.guiActive = false;
            }
            foreach (BaseEvent e in Events)
            {
                e.active = false;
                e.guiActive = false;
                e.guiActiveEditor = false;
            }
            foreach (BaseAction a in Actions)
            {
                a.active = false;
            }
            this.enabled = false;
        }
        else
        {
            //winglet = part.Modules.OfType<ControlSurface>().FirstOrDefault();
            winglet = part as ControlSurface;
            defaultRange = winglet.ctrlSurfaceRange;
            if (!currentControlRangeSet)
            {
                currentControlRange = defaultRange;
                currentControlRangeSet = true;
            }
        }
    }
}

