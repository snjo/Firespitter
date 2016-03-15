using UnityEngine;
using System.Reflection;

    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class FSversionCheck : MonoBehaviour
    {
        static int CompatibleWithMajor = 1;
        static int CompatibleWithMinor = 0;
        static int CompatibleWithRevision = 5;
        static System.Version FSversion;

        public void Start()
        {
            FSversion = Assembly.GetExecutingAssembly().GetName().Version;            
            Debug.Log("firespitter.dll version: " + FSversion.ToString() + ", compiled for KSP " + CompatibleWithMajor + "." + CompatibleWithMinor + "." + CompatibleWithRevision);
        }

        public static bool IsCompatible()
        {                        

            if (Versioning.version_major != CompatibleWithMajor
                ||
                Versioning.version_minor != CompatibleWithMinor
                ||
                Versioning.Revision != CompatibleWithRevision)
            {
                //warnPlayer();
                return false;
            }
            else
                return true;
        }

        internal static bool IsUnityCompatible()
        {
            return true;
        }
    }
