using Kethane.UserInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Kethane.PartModules;

namespace Kethane.VesselModules
{
public class KethaneProtoDetector
{
	public float DetectingPeriod;
	public float DetectingHeight;
	public float PowerConsumption;
	public List<string> resources;
	public bool IsDetecting;
	public double TimerEcho;

	public KethaneProtoDetector (KethaneDetector det)
	{
		DetectingPeriod = det.DetectingPeriod;
		DetectingHeight = det.DetectingHeight;
		PowerConsumption = det.PowerConsumption;
		resources = det.resources;
		IsDetecting = det.IsDetecting;
		TimerEcho = 0;
	}

	public KethaneProtoDetector (ConfigNode node)
	{
		string s;
		if (node.HasValue ("DetectingPeriod")) {
			s = node.GetValue ("DetectingPeriod");
			float.TryParse (s, out DetectingPeriod);
		}
		if (node.HasValue ("DetectingHeight")) {
			s = node.GetValue ("DetectingHeight");
			float.TryParse (s, out DetectingHeight);
		}
		if (node.HasValue ("PowerConsumption")) {
			s = node.GetValue ("PowerConsumption");
			float.TryParse (s, out PowerConsumption);
		}
		if (node.HasValue ("IsDetecting")) {
			s = node.GetValue ("IsDetecting");
			bool.TryParse (s, out IsDetecting);
		}
		if (node.HasValue ("TimerEcho")) {
			s = node.GetValue ("TimerEcho");
			double.TryParse (s, out TimerEcho);
		}
		resources = node.GetNodes("Resource").Select(n => n.GetValue("Name")).ToList();
	}

	public void Save (ConfigNode node)
	{
		var n = node.AddNode ("Detector");
		n.AddValue ("DetectingPeriod", DetectingPeriod);
		n.AddValue ("DetectingHeight", DetectingHeight);
		n.AddValue ("PowerConsumption", PowerConsumption);
		n.AddValue ("IsDetecting", IsDetecting);
		n.AddValue ("TimerEcho", TimerEcho);
		foreach (var res in resources) {
			var resnode = n.AddNode ("Resource");
			resnode.AddValue("Name", res);
		}
	}
}

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
