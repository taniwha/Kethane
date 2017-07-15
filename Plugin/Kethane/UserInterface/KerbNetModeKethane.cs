using GeodesicGrid;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using UnityEngine;

using KSP.UI.Screens;
using KSP.UI.Dialogs;
using KerbNet;

namespace Kethane.UserInterface
{
	public class KerbNetModeKethane : KerbNetMode
	{
		CelestialBody body;
		BodyResourceData bodyResources;
		ResourceDefinition resource;
		List<ResourceDefinition> resourceDefinitions;
		int resourceIndex;

		public KerbNetModeKethane ()
		{
		}

		public override void OnInit ()
		{
			name = "Kethane";
			buttonSprite = Resources.Load<Sprite>("Scanners/resource");
			localCoordinateInfoLabel = "Quantity";
			doTerrainContourPass = true;
			doAnomaliesPass = true;
			doCoordinatePass = true;

			resourceDefinitions = KethaneController.ResourceDefinitions.ToList();
			resourceIndex = 0;
			resource = resourceDefinitions[resourceIndex];

			customButtonCaption = "Resource: " + resource.Resource;
			customButtonCallback = OnResourceClick;
			customButtonTooltip = "Cycle Displayed Resource";
		}

		void OnResourceClick ()
		{
			if (++resourceIndex >= resourceDefinitions.Count) {
				resourceIndex = 0;
			}
			resource = resourceDefinitions[resourceIndex];
			customButtonCaption = "Resource: " + resource.Resource;
			KerbNetDialog.Instance.ActivateDisplayMode (this);
		}

		public override void OnPrecache (Vessel vessel)
		{
			body = vessel.mainBody;
			bodyResources = KethaneData.Current[resource.Resource][body];
		}

		public override Color GetCoordinateColor(Vessel vessel, double currentLatitude, double currentLongitude)
		{
			Cell cell = MapOverlay.GetCellUnder (body, currentLatitude, currentLongitude);
			return MapOverlay.GetCellColor (cell, bodyResources, resource);
		}
	}
}
