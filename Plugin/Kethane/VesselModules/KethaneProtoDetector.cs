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
	public double powerRatio;

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
}
