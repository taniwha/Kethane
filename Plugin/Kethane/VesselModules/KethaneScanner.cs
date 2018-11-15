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

	AudioSource PingEmpty;
	AudioSource PingDeposit;

	bool started;
	bool isScanning;

	void FindDetectors ()
	{
		detectors = vessel.FindPartModulesImplementing<KethaneDetector>();
		protoDetectors = new List<KethaneProtoDetector>();
		foreach (var det in detectors) {
			protoDetectors.Add (new KethaneProtoDetector(det));
			det.scanner = this;
		}
	}

	public void UpdateDetecting (KethaneDetector det)
	{
		int index = detectors.IndexOf (det);
		protoDetectors[index].IsDetecting = det.IsDetecting;
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

	protected override void OnLoad (ConfigNode node)
	{
		protoDetectors = new List<KethaneProtoDetector> ();
		for (int i = 0; i < node.nodes.Count; i++) {
			ConfigNode n = node.nodes[i];
			if (n.name == "Detector") {
				var det = new KethaneProtoDetector (n);
				protoDetectors.Add (det);
			}
		}

		// ensure the scanner runs at least once when the vessel is not loaded
		isScanning = true;
	}

	protected override void OnSave (ConfigNode node)
	{
		Debug.LogFormat("[KethaneVesselScanner] OnSave: {0} {1} {2}", vessel.gameObject == gameObject, enabled, gameObject.activeInHierarchy);
		if (protoDetectors == null) {
			return;
		}
		for (int i = 0; i < protoDetectors.Count; i++) {
			protoDetectors[i].Save (node);
		}
	}

	void onVesselWasModified (Vessel v)
	{
		if (v == vessel) {
			FindDetectors ();
		}
	}

	protected override void OnAwake ()
	{
		GameEvents.onVesselWasModified.Add (onVesselWasModified);
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
		PingEmpty.loop = false;
		PingDeposit.Stop();
	}

	public override void OnLoadVessel ()
	{
		FindDetectors ();
	}

	public override void OnUnloadVessel ()
	{
		Debug.LogFormat("[KethaneVesselScanner] OnUnloadVessel: {0} {1} {2}", vessel.gameObject == gameObject, enabled, gameObject.activeInHierarchy);
	}

	public void OnVesselUnload()
	{
		Debug.LogFormat("[KethaneVesselScanner] OnVesselUnload: {0}", vessel.name);
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
		for (int i = protoDetectors.Count; i-- > 0; ) {
			var detector = protoDetectors[i];
			if (!detector.IsDetecting) {
				continue;
			}
			isScanning = true;
			if (Altitude < detector.DetectingHeight) {
				detector.TimerEcho += TimeWarp.deltaTime; //FIXME * detector.powerRatio
				var TimerThreshold = detector.DetectingPeriod * (1 + Altitude * 2e-6);
				if (detector.TimerEcho >= TimerThreshold) {
					var cell = MapOverlay.GetCellUnder(body, position);
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
	}
}
}
