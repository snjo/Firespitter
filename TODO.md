# Firespitter :: TODO list

* Convert the workarounds PNG into DDS.
* 'Firespitter/Parts/Wheel/FS_apacheLandingGearFlip/part/FSapacheLandingGearFlip'
	+ Fix the animation for raising/lowering on Flying Scene (works fine on SPH!)
* Rebalance Parts
	+ Open Cockpits with 3400ºK of heat resistance? :-D
* Figure out a way to fix the FSWheel.
	+ It kinda works, except on spawn:
		+ `Physics.ClosestPoint can only be used with a BoxCollider, SphereCollider, CapsuleCollider and a convex MeshCollider.`
		+ `[FireSpitter Test 1]: ground contact! - error. Moving Vessel  up 998.158m`
	+ Note: It's something on FSWheel for sure.
* Implement Aircraft attitude on FSmoveCraftAtLaunch (FS3WL Water Launch System)
