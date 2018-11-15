using Kethane.UserInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Kethane.PartModules;

namespace Kethane.VesselModules
{
public class KethaneBattery: IKethaneBattery
{
	public uint flightID { get; set; }

	PartResource partResource;

	public KethaneBattery (PartResource res)
	{
		partResource = res;
	}

	public double amount
	{
		get { return partResource.amount; }
		set { partResource.amount = value; }
	}

	public double maxAmount
	{
		get { return partResource.maxAmount; }
		// no plan on setting at this stage
		//set { partResource.maxAmount = value; }
	}

	public bool flowState
	{
		get { return partResource.flowState; }
		// no plan on setting at this stage
		//set { partResource.flowState = value; }
	}
}
}
