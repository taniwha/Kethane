using Kethane.UserInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Kethane.VesselModules;

namespace Kethane.PartModules
{
    public class KethaneDetector : PartModule
    {
        public static bool ScanningSound
        {
            get { return Misc.Parse(SettingsManager.GetValue("ScanningSound"), true); }
            set { SettingsManager.SetValue("ScanningSound", value); }
        }

        [KSPField(isPersistant = false)]
        public float DetectingPeriod;

        [KSPField(isPersistant = false)]
        public float DetectingHeight;

        [KSPField(isPersistant = false)]
        public float PowerConsumption;

        [KSPField(isPersistant = true)]
        public bool IsDetecting;

        public string configString;

        internal List<string> resources;
		internal KethaneVesselScanner scanner;

        private double powerRatio;

        [KSPEvent(guiActive = true, guiName = "Activate Detector", active = true, externalToEVAOnly = true, guiActiveUnfocused = true, unfocusedRange = 1.5f)]
        public void EnableDetection()
        {
            IsDetecting = true;
			if (scanner != null) {
				scanner.UpdateDetecting (this);
			}
        }

        [KSPEvent(guiActive = true, guiName = "Deactivate Detector", active = false, externalToEVAOnly = true, guiActiveUnfocused = true, unfocusedRange = 1.5f)]
        public void DisableDetection()
        {
            IsDetecting = false;
			if (scanner != null) {
				scanner.UpdateDetecting (this);
			}
        }

        [KSPAction("Activate Detector")]
        public void EnableDetectionAction(KSPActionParam param)
        {
            EnableDetection();
        }

        [KSPAction("Deactivate Detector")]
        public void DisableDetectionAction(KSPActionParam param)
        {
            DisableDetection();
        }

        [KSPAction("Toggle Detector")]
        public void ToggleDetectionAction(KSPActionParam param)
        {
            IsDetecting = !IsDetecting;
			if (scanner != null) {
				scanner.UpdateDetecting (this);
			}
        }

        [KSPEvent(guiActive = true, guiName = "Enable Scan Tone", active = true, externalToEVAOnly = true, guiActiveUnfocused = true, unfocusedRange = 1.5f)]
        public void EnableSounds()
        {
            ScanningSound = true;
        }

        [KSPEvent(guiActive = true, guiName = "Disable Scan Tone", active = false, externalToEVAOnly = true, guiActiveUnfocused = true, unfocusedRange = 1.5f)]
        public void DisableSounds()
        {
            ScanningSound = false;
        }

        [KSPField(isPersistant = false, guiActive = true, guiName = "Status")]
        public string Status;

        public override string GetInfo()
        {
            return String.Format("Maximum Altitude: {0:N0}m\nPower Consumption: {1:F2}/s\nScanning Period: {2:F2}s\nDetects: {3}", DetectingHeight, PowerConsumption, DetectingPeriod, String.Join(", ", resources.ToArray()));
        }

        public override void OnStart(PartModule.StartState state)
        {
            if (state == StartState.Editor) { return; }
            this.part.force_activate();
        }

        public override void OnLoad(ConfigNode config)
        {
            if (this.configString == null)
            {
                this.configString = config.ToString();
            }

            config = Misc.Parse(configString).GetNode("MODULE");

            resources = config.GetNodes("Resource").Select(n => n.GetValue("Name")).ToList();
            if (resources.Count == 0)
            {
                resources = KethaneController.ResourceDefinitions.Select(r => r.Resource).ToList();
            }
        }

        public override void OnUpdate()
        {
            Events["EnableDetection"].active = !IsDetecting;
            Events["DisableDetection"].active = IsDetecting;
            Events["EnableSounds"].active = !ScanningSound;
            Events["DisableSounds"].active = ScanningSound;

            if (vessel.getTrueAltitude() <= this.DetectingHeight)
            {
                if (IsDetecting)
                {
                    Status = powerRatio > 0 ? "Active" : "Insufficient Power";
                }
                else
                {
                    Status = "Idle";
                }
            }
            else
            {
                Status = "Out Of Range";
            }

            foreach (var animator in part.Modules.OfType<IDetectorAnimator>())
            {
                animator.IsDetecting = IsDetecting;
                animator.PowerRatio = 1;//(float) powerRatio;
            }
        }
    }
}
