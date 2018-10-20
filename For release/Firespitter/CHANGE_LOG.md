# Firespitter :: Change Log

* 2018-0801 : 7.9.0.1 (Lisias) Unofficial
	+ Moving PluginData back to <KSP_ROOT> where it belongs
	+ Converting some WAV files from ADPCM to PCM, as Unity doesn't support this format.
	+ Ressurecting the FSMoveCraftAtLaunch module (and the respective part, fsmovecraftgadget/FS3WL Water Launch System). #hurray :)
	+ Dirty hack to prevent Null Pointer Exceptions on FSEngine at craft destroy or recovering.
	+ Parts fixed (needs rebalancing however, IMHO):
		-  FSstrutConnectorWire/FS4SW Biplane wire strut connector (Legacy)
		-  FSstrutConnectorWood/FS4SD Biplane wooden beam connector(legacy)
		-  FSdropTank/FS3FD Fuel Drop Tank
	+ Temporary hack to allow the plugin to work while I try to solve an issue with custom/missing shaders
* 2018-0326 7.9.0 (BobPalmer) for KSP 1.4.1
	+ v7.9.0
	+ KSP 1.4.1 
* 2017-0528 7.6.0 (BobPalmer) for KSP 1.3
	+ Merge pull request #185 from snjo/DEVELOP
	+ Merge pull request #184 from snjo/master
* 2017-0105 7.5.1 (BobPalmer) for KSP 1.2.2
	+ Added a screen message when EVA repainting 
* 2016-1217 7.5.0 (BobPalmer) for KSP 1.2.2
	+ No changelog provided 
* 2016-1107 7.4.2 (BobPalmer) for KSP 1.2.1
	+ Compile update, and some new options for ModuleAnimateGeneric from linuxgurugamer 
* 2016-0911 7.4.1 (BobPalmer) for KSP 1.2
	+ Recompile for KSP 1.2
	+ Fixed biplane hatch
	+ Converted textures to DDS 
* 2016-0911 7.4.0 (BobPalmer) for KSP 1.2 PRE
	+ Modder Preview - DLL Update 
* 2016-0712 7.3.0 (BobPalmer) for KSP 1.1.3
	+ Re-versioning 
* 2016-0628 7.3 (BobPalmer) for KSP 1.1.3
	+ Compatibility Update for KSP 1.1.3 
* 2016-0514 7.2.4 (BobPalmer) for KSP 1.1.2
	+ New biplane model and textures, old parts moved to legacy. 
* 2016-0430 7.2.3 (BobPalmer) for KSP 1.1.2
	+ Recompile for 1.1.2 
* 2016-0416: 7.2.1 (BobPalmer) for KSP 1.1 (PRE)
	+ Minor update to FuelSwitch for 1.1 compatibility
* 2016-0402: 7.2.0 (BobPalmer) for KSP 1.1 (PRE)
	+ Merge pull request #143 from snjo/DEVELOP
	+ Updates for KSP 1.1
* 2015-1109: 7.1.5 (BobPalmer) for KSP 1.0.5
	+ This is a preliminary compatibility release for KSP 1.0.5 - please be sure to report any bugs/etc. in the forum thread.
	+ Also includes a large number of config fixes fro Ruedii and khr15714n! 
* 2015-0624: 7.1.4 (BobPalmer) for KSP 1.0.4
	+ Version compatibility update. Also included a separate ZIP for plugin only.
	+ Note that this is just a patch to change version checking to 1.0.4, I've not encountered any breaking changes, but as always, balance will need to be reviewed.
* 2015-0516: 7.1.3 (BobPalmer) for KSP 1.0.2
	+ Wing and control surface fixes 
* 2015-0511: 7.1.2 (BobPalmer) for KSP 1.0.2
	+ Engine updates - should prevent propellers from blowing your wings off and taking them to space. 
* 2015-0505: 7.1.1 (BobPalmer) for KSP 1.0.2
	+ Fixed issues with various attachment nodes
* 2015-0504: 7.1 (BobPalmer) for KSP 1.0.2
	+ KSP 1.0.2 Compatibility
	+ (see 7.0.PRE2 below for additional notes)
* 2014-0901: 6.3.5 (snjo) for KSP 0.24.2
	+ Plugin update for compatibility with various mods like B9
	+ Some minor fixes to prices in cfg
	+ Removed unneeded pngs, and optimized others.
* 2014-0714: 6.3.4 (snjo) for KSP 0.24.2
	+ KSP 0.24.2 Compatibility
	+ Module cost configurable in FSfuelSwitch
* 2014-0718: 6.3.3 (snjo) for KSP 0.24
	+ KSP 0.24 Compatibility.
	+ Lots of plugin features for upcoming version 7, and features used in other mods like B9
* 2014-0718: 7 Plugin (snjo) for KSP 0.23.5
	+ v7plugin0.23.5
	+ updated dll
* 2014-0619: 7.0 PRE2 (snjo) for KSP 0.23.5
	+ Some parts moved to the Firespitter Legacy Parts Pack. Download that to keep using existing craft files.
	+ Tweakable engines! Adjust the number of propeller blades, their length, and the engine size in the hangar!
	+ Updated helicopter engine and tail rotors/fenestrons using aerodynamic blade lift, and a much improved hover code.
	+ Modular fuel tanks. A single part with alternate texutres and fuel tanks. Toggle through the choices in the hangar. (oblong multi-tank and fuel drop tank)
	+ Wheel alignment guides end the scourge of crooked gear placement. Press F2 to toggle guide lines.
	+ New tail boom model added to the old tail boom (Switch model in hangar) (WIP, untextured)
	+ A new engine module that supports atmospheric engines much better, separates engine start up time from throttle response, and allows for electric engines without the extra FScoolant resource.
	+ Apache cockpit monitors are off by default to reduce lag. Click a button to turn them on.
	+ Helicopter engines display a guide arrow to assist in putting them on the right way.
	+ New texture switch, mesh switch and fuel tank switch modules allows for many varieties in a single part
	+ Added optional Part Catalog icon from Kwirkilee
	+ Added visual brake response to flight control pedals
	+ Various bug fixes
* 2014-0531: 6.4 PRE1 (snjo) for KSP 0.23.5
	+ Some parts moved to the Firespitter Legacy Parts Pack (not yet packaged)
	+ Tweakable engines! Adjust the number of propeller blades, their length, and the engine size in the hangar!
	+ New helicopter main rotor using aerodynamic lift on the blades and particle ground FX (currently constantly on)
	+ Electric propeller and electric heli engine are using the new engine module, which trades some silly throttle bugs for new and exciting bugs
	+ Modular fuel tanks. A single part with alternate texutres and fuel tanks. Toggle through the choices in the hangar. (oblong multi-tank and fuel drop tank)
	+ Wheel alignment guides end the scourge of crooked gear placement. Press F2 to toggle guide lines.
	+ A new engine module that supports atmospheric engines much better, separates engine start up time from throttle response, and allows for electric engines without the extra FScoolant resource.
	+ Apache cockpit monitors are off by default to reduce lag. Click a button to turn them on.
	+ Helicopter engines display a guide arrow to assist in putting them on the right way.
	+ New texture switch, mesh switch and fuel tank switch modules allows for many varieties in a single part. Added normal-map-only support to tex switch.

WARNING: Note that some parts are still WIP, including some that have replaced parts moved to legacy. This release is not part complete, and should only be used by the very curious 
* 2014-0531: Legacy Parts (snjo) for KSP 0.23.5
	+ legacy
	+ removed old craft
* 2014-0530: 6.3.1 (snjo) for KSP 0.23.5
	+ Wheel alignment guides end the scourge of crooked gear placement. Press F2 to toggle guide lines.
* 2014-0522: Final 6.4 PRE1 (snjo) for KSP 0.23.5
	+ Tweakable engines! Adjust the number of propeller blades, their length, and the engine size in the hangar!
	+ Wheel alignment guides end the scourge of crooked gear placement. Press F2 to toggle guide lines.
	+ A new engine module that supports atmospheric engines much better, separates engine start up time from throttle response, and allows for electric engines without the extra FScoolant resource.
* 2014-0506: Final 6.3 (snjo) for KSP 0.23.5
	+ Oblong round noses, short and long
	+ Oblong to 0.625m adapter
	+ Helicopter landing pads by Justin Kerbice
	+ Warning message on the Main Menu if you are using an incompatible KSP version
	+ W.I.P. turboprop engine. This will see changes to performance, sound and looks
	+ FSengineSounds: Implemented disengage, running, flameout sounds, fixed bugs.
	+ FSwing: Made leading edge action name cfg editable for use in extending flaps etc.
	+ FSwheel: supports altering retract animation speed in cfg
	+ FSslowtato: key/action group based rotator module
	+ FSmeshSwitch: swap meshes instead of textures for better memory conservation
* 2014-0406: Final 6.2 (snjo) for KSP 0.23.5
	+ v6.2 is compiled for KSP v0.23.5 (The ARM mission pack)

