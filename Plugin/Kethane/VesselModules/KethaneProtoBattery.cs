using Kethane.UserInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Kethane.PartModules;

namespace Kethane.VesselModules
{
public class KethaneProtoBattery: IKethaneBattery
{
	public uint flightID { get; set; }

	ProtoPartResourceSnapshot protoResource;

	public KethaneProtoBattery (ProtoPartResourceSnapshot res)
	{
		protoResource = res;
	}

	public double amount
	{
		get { return protoResource.amount; }
		set { protoResource.amount = value; }
	}

	public double maxAmount
	{
		get { return protoResource.maxAmount; }
		// no plan on setting at this stage
		//set { protoResource.maxAmount = value; }
	}

	public bool flowState
	{
		get { return protoResource.flowState; }
		// no plan on setting at this stage
		//set { protoResource.flowState = value; }
	}
}
}
