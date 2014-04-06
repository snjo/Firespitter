using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Reflection;

    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class FSversionCheck : MonoBehaviour
    {
        static int CompatibleWithMajor = 0;
        static int CompatibleWithMinor = 23;
        static int CompatibleWithRevision = 5;
        static System.Version FSversion;
        //static bool versionError = false;

        //private FSGUIPopup warningPopup;
        //private string KSPversion = string.Empty;
        //private string expectedVersion = string.Empty;

        //public void Start()
        public static bool IsCompatible()
        {
            //string KSPversion = Versioning.version_major + "." + Versioning.version_minor + "." + Versioning.Revision;
            //string expectedVersion = CompatibleWithMajor + "." + CompatibleWithMinor + "." + CompatibleWithRevision;

            //Debug.Log("Firespitter version check. KSP version: " + KSPversion);

            FSversion = Assembly.GetExecutingAssembly().GetName().Version;

            Debug.Log("firespitter.dll version: " + FSversion.ToString() + ", compiled for KSP " + CompatibleWithMajor + "." + CompatibleWithMinor + "." + CompatibleWithRevision);

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

        //private void warnPlayer()
        //{
        //    //versionError = true;
        //    PopupDialog.SpawnPopupDialog("Firespitter version warning", "This version of the firespitter plugin was made for KSP version "
        //     + expectedVersion + ". You are using " + KSPversion + "."
        //     + "\nThis may cause errors both in Firespitter parts, other mods, and in the game in general.\nProceed at your own risk, and don't submit bugs when running incompatible versions.", "OK", false, HighLogic.Skin);
        //    Debug.Log("Warning player of incompatible version via popup");
        //}
    }
