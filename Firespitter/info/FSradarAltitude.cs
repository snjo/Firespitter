using System;

namespace Firespitter.info
{
    public class FSradarAltitude : PartModule
    {

        [KSPField(guiActive = true, guiName = "Radar Altitude")]
        public double radarAltitude;

        public override void OnFixedUpdate()
        {
            base.OnFixedUpdate();
            double pqsAltitude = vessel.pqsAltitude;
            if (pqsAltitude < 0) pqsAltitude = 0;
            radarAltitude = Math.Floor(vessel.altitude - pqsAltitude);
        }
    }
}