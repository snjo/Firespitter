using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Firespitter.engine
{
    public class FSresource
    {
        //public PartResource resource;
        public string name;
        public float ratio;
        public float currentSupply = 0f;
        public float amount = 0f;
        public float maxAmount = 0f;

        public FSresource(string _name, float _ratio)
        {
            name = _name;
            ratio = _ratio;
        }

        public FSresource(string _name)
        {
            name = _name;
            ratio = 1f;
        }
    }
}
