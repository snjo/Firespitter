using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Firespitter.animation
{
    public class FSslowtato : PartModule
    {
        [KSPField]
        public float rotationSpeed = 5f;
        [KSPField]
        public Vector3 rotationAxis = Vector3.forward;
        [KSPField]
        public string keyPlus = "page up";
        [KSPField]
        public string keyMinus = "page down";
        [KSPField]
        public string rotatorName = "rotator";
        [KSPField]
        public bool useKeys = true;

        [KSPField]
        public bool rotationInitialized = false;
        [KSPField]
        public Vector3 currentRotation = Vector3.zero;

        private float rotationAmount = 0f;
        private bool actionGroupPressed = false;

        private Transform rotator;

        [KSPAction("Rotate +")]
        public void RotatePlusAction(KSPActionParam param)
        {
            if (param.type == KSPActionType.Activate)
            {
                rotationAmount = 1f;
                actionGroupPressed = true;
            }
            else
            {
                rotationAmount = 0f;
                actionGroupPressed = false;
            }
        }

        [KSPAction("Rotate -")]
        public void RotateMinusAction(KSPActionParam param)
        {
            if (param.type == KSPActionType.Activate)
            {
                rotationAmount = -1f;
                actionGroupPressed = true;
            }
            else
            {
                rotationAmount = 0f;
                actionGroupPressed = false;
            }
        }

        public override void OnStart(PartModule.StartState state)
        {
            if (!HighLogic.LoadedSceneIsFlight) return;
            rotator = part.FindModelTransform(rotatorName);
            if (rotator == null)
            {
                Debug.Log("Could not find transform " + rotatorName);
            }
            else
            {
                if (rotationInitialized)
                {
                    rotator.rotation = Quaternion.Euler(currentRotation);
                }
                else
                {
                    currentRotation = rotator.rotation.eulerAngles;
                    rotationInitialized = true;
                }
            }

        }

        public override void OnUpdate()
        {
            if (!HighLogic.LoadedSceneIsFlight) return;
            if (rotator != null)
            {
                if (useKeys)
                {
                    if (Input.GetKey(keyPlus))
                    {
                        rotationAmount = 1f;
                        //rotator.Rotate(rotationAxis, rotationSpeed * TimeWarp.deltaTime);
                    }
                    else if (Input.GetKey(keyMinus))
                    {
                        rotationAmount = -1f;
                    }
                    else if (actionGroupPressed == false)
                    {
                        rotationAmount = 0f;
                    }
                }

                if (rotationAmount != 0)
                    rotator.Rotate(rotationAxis, rotationSpeed * rotationAmount * TimeWarp.deltaTime);
            }
        }
    }
}
