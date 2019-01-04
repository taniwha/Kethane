using Kethane.UserInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Kethane.PartModules;

namespace Kethane.VesselModules
{
public class KethaneVesselScanner : VesselModule
{
	List<KethaneDetector> detectors;
	List<KethaneProtoDetector> protoDetectors;
	Dictionary<uint, IKethaneBattery> batteries;
	List<IKethaneBattery> batteryList;

	AudioSource PingEmpty;
	AudioSource PingDeposit;

	bool started;
	bool isScanning;
	double generatorEC;
	double solarEC;
	int planetLayerMask;

	void FindDetectors ()
	{
		detectors = vessel.FindPartModulesImplementing<KethaneDetector>();
		protoDetectors = new List<KethaneProtoDetector>();
		foreach (var det in detectors) {
			det.scanner = new KethaneProtoDetector(det, this);
			protoDetectors.Add (det.scanner);
		}
	}

	static void FindBatteries (Dictionary<uint, IKethaneBattery> batteries,
							   List<IKethaneBattery> batteryList,
							   List<Part> parts)
	{
		//FIXME should not be hard-coded
		int resID = "ElectricCharge".GetHashCode();
		foreach (var p in parts) {
			var res = p.Resources.Get (resID);
			if (res != null) {
				var bat = new KethaneBattery (res);
				batteries[p.flightID] = bat;
				batteryList.Add (bat);
			}
		}
	}

	static void FindBatteries (Dictionary<uint, IKethaneBattery> batteries,
							   List<IKethaneBattery> batteryList,
							   List<ProtoPartSnapshot> parts)
	{
		foreach (var p in parts) {
			foreach (var res in p.resources) {
				//FIXME should not be hard-coded
				if (res.resourceName == "ElectricCharge") {
					var bat = new KethaneProtoBattery (res);
					batteries[p.flightID] = bat;
					batteryList.Add (bat);
				}
			}
		}
	}

	void FindBatteries ()
	{
		batteries = new Dictionary<uint, IKethaneBattery> ();
		batteryList = new List<IKethaneBattery> ();
		if (vessel.loaded) {
			FindBatteries (batteries, batteryList, vessel.parts);
		} else {
			FindBatteries (batteries, batteryList,
						   vessel.protoVessel.protoPartSnapshots);
		}
	}

	void FindGenerators ()
	{
		var generators = vessel.FindPartModulesImplementing<ModuleGenerator>();
		generatorEC = 0;
		for (int i = generators.Count; i-- > 0; ) {
			var gen = generators[i];
			var resHandler = gen.resHandler;
			bool active = (gen.moduleIsEnabled
						   && (gen.isAlwaysActive || gen.generatorIsActive));
			// Really don't want to deal with anything that consumes resources
			// to create EC or that produces anything other than EC (better to
			// pretend those module simply shutdown than to break how they work)
			if (active && resHandler.inputResources.Count == 0
				&& resHandler.outputResources.Count == 1
				&& resHandler.outputResources[0].name == "ElectricCharge") {
				generatorEC += resHandler.outputResources[0].rate;
			}
		}
	}

	void FindSolarPanels ()
	{
		var solarPanels = vessel.FindPartModulesImplementing<ModuleDeployableSolarPanel>();
		solarEC = 0;
		for (int i = solarPanels.Count; i-- > 0; ) {
			var sol = solarPanels[i];
			var resHandler = sol.resHandler;
			// Really don't want to deal with anything that consumes resources
			// to create EC or that produces anything other than EC (better to
			// pretend those module simply shutdown than to break how they work)
			if (resHandler.inputResources.Count == 0
				&& resHandler.outputResources.Count == 1
				&& resHandler.outputResources[0].name == "ElectricCharge") {
				solarEC += sol._flowRate * resHandler.outputResources[0].rate;
			}
		}
	}

	public void UpdateDetecting ()
	{
		// FixedUpdate will set to the correct value, but need to ensure
		// FixedUpdate gets run at least once.
		isScanning = true;
	}

	public override bool ShouldBeActive ()
	{
		bool active = base.ShouldBeActive ();
		active &= (!started || isScanning);
		return active;
	}

	void onVesselCreate (Vessel v)
	{
		if (v == vessel) {
			GameEvents.onVesselCreate.Remove (onVesselCreate);
			FindBatteries ();
		}
	}

	protected override void OnLoad (ConfigNode node)
	{
		protoDetectors = new List<KethaneProtoDetector> ();
		for (int i = 0; i < node.nodes.Count; i++) {
			ConfigNode n = node.nodes[i];
			if (n.name == "Detector") {
				var det = new KethaneProtoDetector (n, this);
				protoDetectors.Add (det);
			}
		}
		generatorEC = 0;
		if (node.HasValue ("generatorEC")) {
			double.TryParse (node.GetValue ("generatorEC"), out generatorEC);
		}

		solarEC = 0;
		if (node.HasValue ("solarEC")) {
			double.TryParse (node.GetValue ("solarEC"), out solarEC);
		}

		GameEvents.onVesselCreate.Add (onVesselCreate);

		// ensure the scanner runs at least once when the vessel is not loaded
		isScanning = true;
	}

	protected override void OnSave (ConfigNode node)
	{
		if (protoDetectors == null) {
			return;
		}
		if (vessel.loaded) {
			// need to find out what generators are available before saving,
			// but only when loaded as unloaded vessels already have
			// generatorEC and solarEC set.
			FindGenerators ();
			FindSolarPanels ();
		}
		node.AddValue ("generatorEC", generatorEC);
		node.AddValue ("solarEC", solarEC);
		for (int i = 0; i < protoDetectors.Count; i++) {
			protoDetectors[i].Save (node);
		}
	}

	void onVesselWasModified (Vessel v)
	{
		if (v == vessel) {
			FindDetectors ();
			FindBatteries ();
		}
	}

	protected override void OnAwake ()
	{
		GameEvents.onVesselWasModified.Add (onVesselWasModified);
		planetLayerMask = 1 << LayerMask.NameToLayer("Scaled Scenery");
	}

	void OnDestroy ()
	{
		GameEvents.onVesselWasModified.Remove (onVesselWasModified);
	}

	bool ValidVesselType (VesselType type)
	{
		if (type > VesselType.Base) {
			// EVA and Flag
			return false;
		}
		if (type == VesselType.SpaceObject
			|| type == VesselType.Unknown) {
			// asteroids
			return false;
		}
		// Debris, Probe, Relay, Rover, Lander, Ship, Plane, Station, Base
		return true;
	}

	protected override void OnStart ()
	{
		started = true;

		if (!ValidVesselType (vessel.vesselType)) {
			vessel.vesselModules.Remove (this);
			Destroy (this);
			return;
		}

		PingEmpty = gameObject.AddComponent<AudioSource>();
		PingEmpty.clip = GameDatabase.Instance.GetAudioClip("Kethane/Sounds/echo_empty");
		PingEmpty.volume = 1;
		PingEmpty.loop = false;
		PingEmpty.Stop();

		PingDeposit = gameObject.AddComponent<AudioSource>();
		PingDeposit.clip = GameDatabase.Instance.GetAudioClip("Kethane/Sounds/echo_deposit");
		PingDeposit.volume = 1;
		PingDeposit.loop = false;
		PingDeposit.Stop();
	}

	public override void OnLoadVessel ()
	{
		FindDetectors ();
		FindBatteries ();
	}

	public override void OnUnloadVessel ()
	{
		Debug.LogFormat("[KethaneVesselScanner] OnUnloadVessel: {0} {1} {2}", vessel.gameObject == gameObject, enabled, gameObject.activeInHierarchy);
		FindGenerators ();
		FindSolarPanels ();
	}

	public void OnVesselUnload()
	{
		Debug.LogFormat("[KethaneVesselScanner] OnVesselUnload: {0}", vessel.name);
	}

	double DrawEC (double amount)
	{
		double availEC = 0;
		for (int i = batteryList.Count; i-- > 0; ) {
			if (batteryList[i].flowState) {
				availEC += batteryList[i].amount;
			}
		}
		if (amount >= availEC) {
			amount = availEC;
			if (availEC > 0) {
				for (int i = batteryList.Count; i-- > 0; ) {
					batteryList[i].amount = 0;
				}
			}
		} else {
			for (int i = batteryList.Count; i-- > 0; ) {
				var bat = batteryList[i];
				if (bat.flowState) {
					double bamt = bat.amount;
					double amt = amount * bamt / availEC;
					if (amt > bamt) {
						amt = bamt;
					}
					bat.amount = bamt - amt;
				}
			}
		}
		return amount;
	}

	double PushEC (double amount)
	{
		double availEC = 0;
		for (int i = batteryList.Count; i-- > 0; ) {
			if (batteryList[i].flowState) {
				availEC += batteryList[i].maxAmount - batteryList[i].amount;
			}
		}
		if (amount >= availEC) {
			amount = availEC;
			if (availEC > 0) {
				for (int i = batteryList.Count; i-- > 0; ) {
					batteryList[i].amount = batteryList[i].maxAmount;
				}
			}
		} else {
			for (int i = batteryList.Count; i-- > 0; ) {
				var bat = batteryList[i];
				if (bat.flowState) {
					double bamt = bat.amount;
					double max = bat.maxAmount - bamt;
					double amt = amount * max / availEC;
					if (amt > max) {
						amt = max;
					}
					bat.amount = bamt + amt;
				}
			}
		}
		return amount;
	}

	void RunGenerators ()
	{
		PushEC (generatorEC * TimeWarp.fixedDeltaTime);
	}

	void RunSolarPanels ()
	{
		float distance = float.MaxValue;
		var target = Planetarium.fetch.Sun;
		var tgtScaled = target.scaledBody.transform;
		var pos = ScaledSpace.LocalToScaledSpace(vessel.transform.position);
		var tgt = ScaledSpace.LocalToScaledSpace(target.transform.position);
		Ray ray = new Ray(pos, (tgt - pos).normalized);
		RaycastHit hit;
		if (!Physics.Raycast (ray, out hit, distance, planetLayerMask)
			|| hit.transform == tgtScaled) {
			PushEC (solarEC * TimeWarp.fixedDeltaTime);
		}
	}

	void FixedUpdate ()
	{
		if (protoDetectors == null) {
			return;
		}

		double Altitude = vessel.getTrueAltitude ();
		var body = vessel.mainBody;
		var position = vessel.transform.position;
		var detected = false;
		var ping = false;
		isScanning = false;
		var cell = MapOverlay.GetCellUnder(body, position);
		for (int i = protoDetectors.Count; i-- > 0; ) {
			var detector = protoDetectors[i];
			if (!detector.IsDetecting) {
				continue;
			}
			isScanning = true;
			if (Altitude < detector.DetectingHeight) {
				double req = detector.PowerConsumption * TimeWarp.fixedDeltaTime;
				double drawn = DrawEC (req);
				detector.powerRatio = drawn / req;

				detector.TimerEcho += TimeWarp.deltaTime * detector.powerRatio;
				var TimerThreshold = detector.DetectingPeriod * (1 + Altitude * 2e-6);
				if (detector.TimerEcho >= TimerThreshold) {
					for (int j = detector.resources.Count; j-- > 0; ) {
						var resource = detector.resources[j];
						var data = KethaneData.Current[resource][body];
						if (data.IsCellScanned(cell)) {
							continue;
						}
						ping = true;
						data.ScanCell(cell);
						if (data.Resources.GetQuantity(cell) != null) {
							detected = true;
						}
					}
					MapOverlay.Instance.RefreshCellColor(cell, body);
					detector.TimerEcho = 0;
				}
			}
		}
		if (ping && KethaneDetector.ScanningSound
			&& vessel == FlightGlobals.ActiveVessel) {
			(detected ? PingDeposit : PingEmpty).Play();
			(detected ? PingDeposit : PingEmpty).loop = false;
		}

		// when the vessel is loaded, let KSP deal with power generation
		if (!vessel.loaded) {
			RunGenerators ();
			if (solarEC > 0) {
				RunSolarPanels ();
			}
		}
	}
}
}
