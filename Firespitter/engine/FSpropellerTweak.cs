using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Firespitter.engine
{
    public class FSpropellerTweak : PartModule
    {
        [KSPField]
        public string propellerRootName = "propellerRoot";
        [KSPField]
        public string bladeRootName = "bladeRoot";
        [KSPField]
        public string bladeScalerName = "bladeScaler";
        [KSPField(isPersistant=true, guiActive=true, guiName="Blades")]
        public int bladeNumber = 2;
        [KSPField]
        public int minBlades = 2;
        [KSPField]
        public int maxBlades = 8;
        [KSPField]
        public float originalBladeRotation = 0f;
        [KSPField]
        public Vector3 propellerRotationAxis = Vector3.forward;

        [KSPField]
        public float minThrust = 0f;
        [KSPField]
        public float maxThrust = 0f;
        [KSPField]
        public float baseWeight = 0.2f;
        [KSPField]
	    public float engineMaxWeight = 1.0f;
        [KSPField]
        public float bladeWeight = 0.01f;

        [KSPField]
        public string previewObjectsName = "preview";
        [KSPField]
        public string engineEndPointName = "engineEndPoint";
        [KSPField]
        public string movableSectionName = "movableSection";
        [KSPField]
        public string engineExtenderName = "engineExtender";
        [KSPField]
        public string centerOfMassName = "centerOfMass";
        [KSPField(isPersistant=true)]
        public float engineLengthSlider = 0f;
        [KSPField]
        public Vector3 engineExtensionAxis = Vector3.forward;
        [KSPField]
        public string exhaustName = "exhaust";
        [KSPField]
        public float exhaustFrequency = 1f;
        [KSPField]
        public float exhaustDistance = 0.25f;
        [KSPField]
        public Vector3 exhaustTranslateAxis = Vector3.up;
        [KSPField]
        public float exhaustScale = 1.0f;


        [KSPField(isPersistant=true)]
        public float bladeLengthSlider = 1f;
        [KSPField]
        public float widthMultiplier = 0.5f;
        
        [KSPField(guiName="Blade Length"), UI_FloatRange(minValue=0.3f, maxValue=3f, stepIncrement=0.1f)]
        private float BladeLengthSliderRaw = 1f;
        private float oldBladeLength = 0f;

        private Rect sliderRectBase = new Rect(30f, 30f, 200f, 30f);
        private float lineHeight = 50f;
        private int currentGUILine = 0;
        //private Rect sliderLabelRect;
        private GUIStyle sliderStyle = new GUIStyle();
        [KSPField(guiName = "Blades"), UI_FloatRange(minValue = 2f, maxValue = 12.1f, stepIncrement = 1f)]
        private float bladeNumberRaw = 2f;
        private List<GameObject> blades = new List<GameObject>();

        private int exhaustNumber = 0;
        [KSPField]
        public float engineMaxScale = 5f;
        [KSPField(guiName = "Engine Size"), UI_FloatRange(minValue = 0f, maxValue = 1f, stepIncrement = 0.01f)]
        private float engineLengthSliderRaw = 0f;
        private GameObject engineExtension;
        private List<GameObject> exhausts = new List<GameObject>();

        private Transform propellerRoot;
        private Transform movableSection;
        private Transform engineEndPoint;
        private Transform centerOfMass;

        private Firespitter.engine.FSengineWrapper engine;

        private bool initialized = false;
        private bool maxThrustSet = false;
        private bool previewObjectsDestroyed = false;

        public float finalWeight
        {
            get
            {
                return baseWeight + (engineMaxWeight * engineLengthSlider) + (bladeWeight * blades.Count * bladeLengthSlider);
            }
        }

        private void updateBladeList()
        {            
            if (bladeNumber != blades.Count)
            {
                while (blades.Count < bladeNumber) // should convert that to not a while loop
                {
                    GameObject newBlade = (GameObject)GameObject.Instantiate(blades[0]);
                    blades.Add(newBlade);
                    newBlade.transform.parent = propellerRoot;
                    newBlade.transform.position = blades[0].transform.position;
                    newBlade.transform.localScale = Vector3.one;
                }
                if (blades.Count > bladeNumber)
                {
                    //Debug.Log("too many blades");
                    for (int i = bladeNumber; i < blades.Count; i++)
                    {
                        //Debug.Log("destroying blade " + i);
                        GameObject.Destroy(blades[i]);
                        blades.RemoveAt(i);
                    }
                }

                for (int i = 1; i < blades.Count; i++)
                {
                    blades[i].transform.localRotation = Quaternion.Euler(propellerRotationAxis * (originalBladeRotation + ((360f / bladeNumber) * (i))));
                }
            }
        }

        private void updateBladeLength()
        {
            if (oldBladeLength != bladeLengthSlider)
            {
                for (int i = 0; i < blades.Count; i++)
                {                    
                    foreach (Transform t in blades[i].GetComponentsInChildren<Transform>())
                    {
                        if (t.gameObject.name == bladeScalerName)
                        {                            
                            t.localScale = new Vector3(bladeLengthSlider, 1f + ((bladeLengthSlider - 1f) * widthMultiplier), 1f);                            
                        }
                    }
                }
            }
        }

        private void updateEngineLength()
        {
            if (engineExtension != null)
            {
                engineExtension.transform.localScale = Vector3.one + (engineExtensionAxis * engineLengthSlider * engineMaxScale);
                if (engineLengthSlider == 0f)
                {
                    updateExhaustNumber(0);
                }
                else
                {
                    updateExhaustNumber((int)((engineLengthSlider * exhaustFrequency) + 1f));
                }

                if (engineEndPoint != null && movableSection != null)
                {
                    movableSection.position = engineEndPoint.position;
                }

                if (engine != null)
                {
                    engine.maxThrust = Mathf.Lerp(minThrust, maxThrust, engineLengthSlider);
                }

                //if (centerOfMass != null)
                //{
                //    part.rigidbody.centerOfMass = centerOfMass.po;
                //}
            }
        }

        private void updateExhaustNumber(int amount)
        {
            if (amount <= 0)
            {
                exhausts[0].renderer.enabled = false;
            }
            else
            {
                exhausts[0].renderer.enabled = true;
            }

            if (amount != exhausts.Count)
            {

                while (exhausts.Count < amount) // booo!
                {
                    GameObject newExhaust = (GameObject)GameObject.Instantiate(exhausts[0]);
                    exhausts.Add(newExhaust);
                    newExhaust.transform.parent = exhausts[0].transform.parent;
                    newExhaust.transform.position = exhausts[0].transform.position;
                    newExhaust.transform.localRotation = exhausts[0].transform.localRotation;
                    newExhaust.transform.localScale = exhausts[0].transform.localScale; //Vector3.one * exhaustScale;
                }

                if (exhausts.Count > amount && exhausts.Count > 1)
                {
                    //Debug.Log("too many exhausts");
                    for (int i = amount; i < exhausts.Count; i++)
                    {
                        if (i > 0)
                        {
                            //Debug.Log("destroying exhaust " + i);
                            GameObject.Destroy(exhausts[i]);
                            exhausts.RemoveAt(i);
                        }
                    }
                }

                for (int i = 1; i < exhausts.Count; i++)
                {
                    exhausts[i].transform.localPosition = exhausts[0].transform.localPosition + exhaustTranslateAxis * exhaustDistance * i;
                }
            }
        }

        // Use this for initialization
        public override void OnStart(PartModule.StartState state)
        {
            if (!HighLogic.LoadedSceneIsFlight && !HighLogic.LoadedSceneIsEditor) return;

            destroyPreviewObjects();

            engine = new FSengineWrapper(part);

            bladeNumberRaw = bladeNumber;
            engineLengthSliderRaw = engineLengthSlider;
            BladeLengthSliderRaw = bladeLengthSlider;

            //Debug.Log("FSpropellerTweak: find blade root");

            Transform originalBlade = part.FindModelTransform(bladeRootName);
            if (originalBlade != null)
            {
                blades.Add(originalBlade.gameObject);
            }

            //Debug.Log("FSpropellerTweak: find exhaust");

            //GameObject originalExhaust = GameObject.Find(exhaustName);
            Transform originalExhaust = part.FindModelTransform(exhaustName);
            if (originalExhaust != null)
            {
                exhausts.Add(originalExhaust.gameObject);
            }

            //Debug.Log("FSpropellerTweak: find engine extension");

            Transform engineExtensionTransform = part.FindModelTransform(engineExtenderName);
            if (engineExtensionTransform != null)
                engineExtension = engineExtensionTransform.gameObject;

            //Debug.Log("FSpropellerTweak: find propeller root");

            propellerRoot = part.FindModelTransform(propellerRootName);
            if (propellerRoot == null) Debug.Log("FSpropellerTweak: Nasty error, no propeller root found named " + propellerRootName);

            movableSection = part.FindModelTransform(movableSectionName);
            engineEndPoint = part.FindModelTransform(engineEndPointName);
            centerOfMass = part.FindModelTransform(centerOfMassName);

            //Debug.Log("FSpropellerTweak: Blades: " + blades.Count);

            updateBladeList();
            updateEngineLength();
            updateBladeLength();
            
            part.mass = finalWeight;            

            initialized = true;
        }

        private void destroyPreviewObjects()
        {
            Transform[] previewObjects = part.FindModelTransforms(previewObjectsName);
            for (int i = 0; i < previewObjects.Length; i++)
            {
                Destroy(previewObjects[i].gameObject);
            }
            previewObjectsDestroyed = true;
        }

        private void destroyBladeObjects()
        {
            for (int i = blades.Count - 1; i > 0; i--)
            {                
                blades.RemoveAt(i);
            }
            foreach (Transform t in gameObject.GetComponentsInChildren<Transform>())
            {
                if (t.gameObject.name == bladeRootName + "(Clone)")
                {
                    Destroy(t.gameObject);
                }
            }
            //GameObject targetBlade = blades[target];            
            //GameObject.Destroy(targetBlade);
        }

        // In hangar
        public void Update()
        {
            if (HighLogic.LoadedSceneIsEditor && initialized)
            {
                bladeNumber = Mathf.FloorToInt(bladeNumberRaw);
                exhaustNumber = Mathf.FloorToInt(engineLengthSliderRaw);
                engineLengthSlider = (float)Math.Round(engineLengthSliderRaw, 2);
                bladeLengthSlider = (float)Math.Round(BladeLengthSliderRaw, 2);

                
                destroyBladeObjects();
                
                updateBladeList();
                updateEngineLength();
                updateBladeLength();

                part.mass = finalWeight;
            }
            if (!previewObjectsDestroyed)
                destroyPreviewObjects();
        }

        // In flight
        public override void OnUpdate()
        {
            // make sure max thrust is set
            if (!maxThrustSet)
                engine.maxThrust = Mathf.Lerp(minThrust, maxThrust, engineLengthSlider);            
        }

        private Rect sliderRect(int current)
        {
            return new Rect(sliderRectBase.x, sliderRectBase.y + lineHeight * currentGUILine, sliderRectBase.width, sliderRectBase.height);
        }

        private Rect nextRect()
        {
            currentGUILine++;
            return sliderRect(currentGUILine - 1);
        }

        private Rect labelRect()
        {
            Rect newRect = sliderRect(currentGUILine);
            return new Rect(newRect.x + newRect.width + 10f, newRect.y, 150f, newRect.height);
        }
    }
}
