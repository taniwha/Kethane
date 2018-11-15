using Kethane.UserInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Kethane.PartModules;

namespace Kethane.VesselModules
{
public interface IKethaneBattery
{
	uint flightID { get; set; }

	double amount { get; set; }

	// no plan on setting at this stage
	double maxAmount { get; /*set;*/ }

	// no plan on setting at this stage
	bool flowState { get; /*set;*/ }
}
}
