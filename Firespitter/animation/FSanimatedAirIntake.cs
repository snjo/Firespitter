using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using UnityEngine;

public class FSanimatedAirIntake : PartModule
{
    [KSPField]
    public string intakeMeshName = "intakeMesh";
    //[KSPField]
    //public Vector3 axisMultiplier = new Vector3(0f, 0f, 1f);
    [KSPField]
    public Vector3 startPosition = new Vector3(0f, 0f, 0f);
    [KSPField]
    public Vector3 endPosition = new Vector3(0f, 1f, 0f);
    [KSPField]
    public float flowAtAnimateStart = 10f;
    [KSPField]
    public float flowAtAnimateEnd = 100f;

    private Transform intakeMeshTransform;
    private ModuleResourceIntake intakeModule;

    public override void OnStart(PartModule.StartState state)
    {
        base.OnStart(state);
        intakeMeshTransform = part.FindModelTransform(intakeMeshName);
        intakeModule = part.Modules.OfType<ModuleResourceIntake>().FirstOrDefault();
    }

    public override void OnFixedUpdate()
    {
        base.OnFixedUpdate();
        if (!HighLogic.LoadedSceneIsFlight || !vessel.isActiveVessel) return;
        if (intakeMeshTransform != null && intakeModule != null)
        {
            float modifiedFlow = intakeModule.airFlow - flowAtAnimateStart;
            if (modifiedFlow > flowAtAnimateEnd) modifiedFlow = flowAtAnimateEnd;
            if (modifiedFlow < 0) modifiedFlow = 0;
            modifiedFlow = modifiedFlow / flowAtAnimateEnd;

            intakeMeshTransform.localPosition = Vector3.Lerp(startPosition, endPosition, modifiedFlow);
        }
    }
}
