using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Firespitter.cockpit
{
    public class AnalogCounter
    {
        public List<Transform> wheels = new List<Transform>();
        public Vector3 defaultRotation = Vector3.zero;
        public Vector3 rotationAxis = Vector3.right;

        private float wheelValue = 0f;
        private float transition = 0f;

        public AnalogCounter()
        {
        }

        public AnalogCounter(List<Transform> _wheels, Vector3 _defaultRotation, Vector3 _rotationAxis)
        {
            wheels = _wheels;
            defaultRotation = _defaultRotation;
            rotationAxis = _rotationAxis;
        }

        public AnalogCounter(Transform wheel, Vector3 _defaultRotation, Vector3 _rotationAxis)
        {
            wheels.Clear();
            wheels.Add(wheel);
            defaultRotation = _defaultRotation;
            rotationAxis = _rotationAxis;
        }

        private void setRotation(Transform wheel, float newValue)
        {
            wheel.localRotation = Quaternion.Euler(defaultRotation + (newValue * rotationAxis * 36f));
        }

        public void updateNumber(float number)
        {
            transition = number % 1;
            updateNumber(number, transition);
        }

        public void updateNumber(float number, float transition)
        {            
            for (int i = 0; i < wheels.Count; i++)
            {
                wheelValue = number % 10f;
                setRotation(wheels[i], Mathf.Floor(wheelValue) + transition);
                if (wheelValue > 9f)
                    transition = number % 1f;
                else
                    transition = 0f;
                number = number / 10f;
            }            
        }
    }
}
