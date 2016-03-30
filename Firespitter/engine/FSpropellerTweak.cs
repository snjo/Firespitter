using System;
using System.Collections.Generic;
using UnityEngine;

namespace Firespitter.engine
{
    /// <summary>
    /// In game tweaking of the engine size, number of propeller blades and their length
    /// </summary>
    public class FSpropellerTweak : PartModule
    {
        /// <summary>
        /// The name of the spinning root transform that holds all the propeller blades
        /// </summary>
        [KSPField]
        public string propellerRootName = "propellerRoot";  
        /// <summary>
        /// The name of the blade object that gets duplicated to create the other blades
        /// </summary>
        [KSPField]
        public string bladeRootName = "bladeRoot";
        /// <summary>
        /// The name of the transform that gets scaled with blade length. Should contain both the blade mesh and the blur mesh
        /// </summary>
        [KSPField]
        public string bladeScalerName = "bladeScaler";
        /// <summary>
        /// The current (starting) number of blades on the engine
        /// </summary>
        [KSPField(isPersistant=true, guiActive=true, guiName="Blades")]
        public int bladeNumber = 2;
        /// <summary>
        /// The lowest allowed number of blades. 1 or higher.
        /// </summary>
        [KSPField]
        public int minBlades = 2;
        /// <summary>
        /// The highest allowed number of blades
        /// </summary>
        [KSPField]
        public int maxBlades = 8;
        /// <summary>
        /// if for some reason the original blade rotation is non-zero (like 0,270,0), this is the starting rotation for blade 0
        /// </summary>
        [KSPField]
        public float originalBladeRotation = 0f;
        /// <summary>
        /// The axis around which blades are duplicated. NOT the axis the whole propeller spins on when the engine is running (That is handled by another module)
        /// </summary>
        [KSPField]
        public Vector3 propellerRotationAxis = Vector3.forward;        
        /// <summary>
        /// The engine's maxThrust at the lowest engine size setting
        /// </summary>
        [KSPField]
        public float minThrust = 50f;
        /// <summary>
        /// The engine's maxThrust at the highest engine size setting
        /// </summary>
        [KSPField]
        public float maxThrust = 150f;
        [KSPField]
        public float minPowerProduction = 20f;
        [KSPField]
        public float maxPowerProduction = 20f;
        /// <summary>
        /// The weight of the part with 0 engine size (blades still get added to this)
        /// </summary>
        [KSPField]
        public float baseWeight = 0.2f;
        /// <summary>
        /// The weight added to the part if the engine is at max size (scales between 0 to 1 times this value)
        /// </summary>
        [KSPField]
	    public float engineMaxWeight = 1.0f;
        /// <summary>
        /// The weight of a single blade at scale 1. Gets scaled up by length, and multiplied by the number of blades
        /// </summary>
        [KSPField]
        public float bladeWeight = 0.01f;
        /// <summary>
        /// All objects with this name get destroyed immediately. These objects are for there for correct looks in the part catalog thumbnail.
        /// </summary>
        [KSPField]
        public string previewObjectsName = "preview";
        /// <summary>
        /// The name of a transfrom at the end of the scaling section of the engine. The movableSection gets moved to this location as the engine scales.
        /// </summary>
        [KSPField]
        public string engineEndPointName = "engineEndPoint";
        /// <summary>
        /// The name of the section that moves (but doesn't stretch) when the engine scales. Can contain the blades etc.
        /// </summary>
        [KSPField]
        public string movableSectionName = "movableSection";
        /// <summary>
        /// The name of the scaling, stretching section of the engine.
        /// </summary>
        [KSPField]
        public string engineExtenderName = "engineExtender";
        //[KSPField]
        //public string centerOfMassName = "centerOfMass";

        /// <summary>
        /// The current length multiplier of the engine
        /// </summary>
        [KSPField(isPersistant=true)]
        public float engineLengthSlider = 0f;
        /// <summary>
        /// The max scale of the engine. The mesh gets scaled on one axis by this 1 to 1 + this amount
        /// </summary>
        [KSPField]
        public Vector2 engineScaleRange = new Vector2(1f, 5f);
        /// <summary>
        /// The engine scales along this axis
        /// </summary>
        [KSPField]
        public Vector3 engineExtensionAxis = Vector3.forward;
        /// <summary>
        /// The name of the object that gets duplicated to make more exhaust sections
        /// </summary>
        [KSPField]
        public string exhaustName = "exhaust";
        /// <summary>
        /// How many exhausts to add at full engine scale. Might be this value + 1.
        /// </summary>
        [KSPField]
        public float exhaustFrequency = 1f;
        /// <summary>
        /// The distance between duplicated exhaust sections
        /// </summary>
        [KSPField]
        public float exhaustDistance = 0.25f;
        /// <summary>
        /// The exhaust sections are placed at an interval along this local axis
        /// </summary>
        [KSPField]
        public Vector3 exhaustTranslateAxis = Vector3.up;
        //[KSPField]
        //public float exhaustScale = 1.0f;

        /// <summary>
        /// The current length of the blades
        /// </summary>
        [KSPField(isPersistant=true)]
        public float bladeLengthSlider = 1f;
        /// <summary>
        /// When the blades are scaled lengthwise, they are also scaled in width, muliplied by this amount.
        /// </summary>
        [KSPField]
        public float widthMultiplier = 0.5f;
        /// <summary>
        /// Should the engine size affect the maxThrust of the engine module? Set to false for a purely decorative engine scaler. Mass is still set according to the other weight values.
        /// </summary>
        [KSPField]
        public bool affectEngineModule = true;

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
        public List<GameObject> blades = new List<GameObject>();

        private int exhaustNumber = 0;
        
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

        /// <summary>
        /// the calculated total weight of the part based on engine scale, blade length and number of blades, plus a base weight
        /// </summary>
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
                    //for (int i = bladeNumber; i < blades.Count; i++)
                    for (int i = blades.Count - 1; i >= bladeNumber-1 && i > 0; i--)
                    {
                        //Debug.Log("too many blades, destroying blade " + i);
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
                engineExtension.transform.localScale = Vector3.one + (engineExtensionAxis * Mathf.Lerp(engineScaleRange.x, engineScaleRange.y, engineLengthSlider));
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

                if (engine != null && affectEngineModule)
                {
                    engine.maxThrust = Mathf.Lerp(minThrust, maxThrust, engineLengthSlider);
                }

                //if (centerOfMass != null)
                //{
                //    part.GetComponent<Rigidbody>().centerOfMass = centerOfMass.po;
                //}
            }
        }

        private void updateExhaustNumber(int amount)
        {
            if (exhausts.Count > 0)
            {
                if (amount == 0)
                {
                    exhausts[0].GetComponent<Renderer>().enabled = false;
                }
                else
                {
                    exhausts[0].GetComponent<Renderer>().enabled = true;
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
        }

        // Use this for initialization
        public override void OnStart(PartModule.StartState state)
        {
            if (!HighLogic.LoadedSceneIsFlight && !HighLogic.LoadedSceneIsEditor) return;            

            //Debug.Log("FSpropellerTweak Onstart running on " + part.GetInstanceID());

            blades = new List<GameObject>();
            exhausts = new List<GameObject>();

            destroyPreviewObjects();

            //initialized = false;
            initialize();
            
        }

        public void initialize()
        {
            if (!initialized)
            {
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
                //centerOfMass = part.FindModelTransform(centerOfMassName);

                //Debug.Log("FSpropellerTweak: Blades: " + blades.Count);

                updateBladeList();
                updateEngineLength();
                updateBladeLength();

                part.mass = finalWeight;

                initialized = true;
            }
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
                //Debug.Log("destroying blade " + i);
                Destroy(blades[i]);
                blades.RemoveAt(i);
            }
            foreach (Transform t in gameObject.GetComponentsInChildren<Transform>())
            {
                if (t.gameObject.name == bladeRootName + "(Clone)")
                {
                    Destroy(t.gameObject);
                }
            }
        }

        private void destroyExhaustObjects()
        {
            for (int i = exhausts.Count - 1; i > 0; i--)
            {
                //Debug.Log("destroying blade " + i);
                Destroy(exhausts[i]);
                exhausts.RemoveAt(i);
            }
            foreach (Transform t in gameObject.GetComponentsInChildren<Transform>())
            {
                if (t.gameObject.name == exhaustName + "(Clone)")
                {
                    Destroy(t.gameObject);
                }
            }
        }

        // In hangar
        public void Update()
        {
            if (HighLogic.LoadedSceneIsEditor && initialized && part.parent != null)
            {
                //Debug.Log("FSpropellerTweak Update running on " + part.GetInstanceID());

                bladeNumber = Mathf.FloorToInt(bladeNumberRaw);
                exhaustNumber = Mathf.FloorToInt(engineLengthSliderRaw);
                engineLengthSlider = (float)Math.Round(engineLengthSliderRaw, 2);
                bladeLengthSlider = (float)Math.Round(BladeLengthSliderRaw, 2);

                
                destroyBladeObjects();
                destroyExhaustObjects();
                
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
            if (!maxThrustSet && affectEngineModule && engine != null)
            {
                engine.maxThrust = Mathf.Lerp(minThrust, maxThrust, engineLengthSlider);
                if (engine.type == FSengineWrapper.EngineType.FSengine)
                {
                    engine.fsengine.powerProduction = Mathf.Lerp(minPowerProduction, maxPowerProduction, engineLengthSlider);
                }
                maxThrustSet = true;
            }
        }

        public void OnDestroy()
        {
            destroyBladeObjects();
            destroyExhaustObjects();
        }

        //private Rect sliderRect(int current)
        //{
        //    return new Rect(sliderRectBase.x, sliderRectBase.y + lineHeight * currentGUILine, sliderRectBase.width, sliderRectBase.height);
        //}

        //private Rect nextRect()
        //{
        //    currentGUILine++;
        //    return sliderRect(currentGUILine - 1);
        //}

        //private Rect labelRect()
        //{
        //    Rect newRect = sliderRect(currentGUILine);
        //    return new Rect(newRect.x + newRect.width + 10f, newRect.y, 150f, newRect.height);
        //}
    }
}
