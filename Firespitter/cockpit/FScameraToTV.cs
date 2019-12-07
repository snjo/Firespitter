using UnityEngine;

public class FScameraToTV : InternalModule
{
    public string TVcamName = "TVcam";
    public string TVplaneName = "TVplane";
    Material newMat;
    RenderTexture newTex;

    private GameObject TVplaneObject;
    private Camera TVcam;
    private GameObject TVcamTransform;
    private float lerpProgress = 0f;
    public Shader repl = null;

    //Vector3 originalPosition = new Vector3();
    Quaternion originalRotation = new Quaternion();

	// TODO: Compiler este Shader e usá-lo corretamente
	public static Material normal = new Material(Shader.Find("Unlit/Texture") ?? Shader.Find("Standard"));

	// TODO: Compiler este Shader e usá-lo corretamente
	public static Material nightVision = new Material(Shader.Find("Nightvision") ?? Shader.Find("Standard"));

    public void Start()
    {        

        newTex = new RenderTexture(256, 256, 24);
        newTex.isPowerOfTwo = true;
        newTex.Create();
        newMat = new Material(Shader.Find("Diffuse"));
        //newMat = new Material(Shader.Find("KSP/Emissive/Diffuse"));
        //newMat = TVplaneObject.GetComponent<Renderer>().material;
        //newMat.CopyPropertiesFromMaterial(TVplaneObject.GetComponent<Renderer>().material);
        newMat.SetTexture("_MainTex", newTex);        

        TVplaneObject = base.internalProp.FindModelTransform(TVplaneName).gameObject;
        TVplaneObject.GetComponent<Renderer>().material = newMat;

        foreach (Part part in vessel.Parts)
        {
                Transform partTransform = part.FindModelTransform(TVcamName);
                if (partTransform != null)
                {
                    TVcamTransform = partTransform.gameObject;
                    //Debug.Log("FS: found cam transform");
                }
            //}
        }

        if (TVcamTransform != null)
        {            
            TVcam = TVcamTransform.GetComponent<Camera>();
            if (TVcam != null)
            {
                Vector3 originalPosition = TVcam.transform.position;
                Quaternion originalRotation = TVcam.transform.rotation;
                TVcam.targetTexture = newTex;
                TVcam.farClipPlane = 40000f;
                //TVcam.cullingMask = 32771;
                TVcam.cullingMask = Camera.main.cullingMask;

                TVcam.clearFlags = CameraClearFlags.Skybox; // CameraClearFlags.SolidColor;
                TVcam.backgroundColor = Color.magenta;
                //TVcam.transparencySortMode = TransparencySortMode.Default;
                //Debug.Log("FS: found camera ");
            }            
        }

        //repl = Shader.Find("Nightvision");
    }

    public void Update()
    {        
        if (TVcam != null && CameraManager.Instance.currentCameraMode == CameraManager.CameraMode.IVA)
        {          
            TVcam.transform.rotation = originalRotation;
            if (FlightGlobals.fetch.vesselTargetMode != VesselTargetModes.None)
            {
                ITargetable target = FlightGlobals.fetch.VesselTarget;                
                TVcam.transform.LookAt(target.GetTransform(), Firespitter.Tools.WorldUp(vessel));
                //TVcam.transform.localRotation = Quaternion.Euler(new Vector3(TVcam.transform.localRotation.eulerAngles.x, TVcam.transform.localRotation.eulerAngles.z, 0f));
                TVcam.fieldOfView = Mathf.Lerp(TVcam.fieldOfView, 5f, lerpProgress);
                if (lerpProgress < 1f) lerpProgress += 0.01f;
            }
            else
            {
                TVcam.fieldOfView = 100f;
                lerpProgress = 0f;
            }
            TVcam.Render();
        }
    }
}
