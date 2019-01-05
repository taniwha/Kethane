using GeodesicGrid;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Kethane
{
	[KSPScenario(ScenarioCreationOptions.AddToAllGames,
				 GameScenes.SPACECENTER, GameScenes.FLIGHT,
				 GameScenes.TRACKSTATION)]
    public class KethaneData : ScenarioModule
    {
        public const int GridLevel = 5;

        public static KethaneData Current { get; private set; }

        private Dictionary<string, ResourceData> resources = new Dictionary<string, ResourceData>();

        public ResourceData this[string resourceName]
        {
            get { return resources[resourceName]; }
        }

        public void ResetGeneratorConfig(ResourceDefinition resource)
        {
            resources[resource.Resource] = Kethane.ResourceData.Load(resource, new ConfigNode());
        }

        public override void OnLoad(ConfigNode config)
        {
            var timer = System.Diagnostics.Stopwatch.StartNew();

            resources.Clear();

            var resourceNodes = config.GetNodes("Resource");

            foreach (var resource in KethaneController.ResourceDefinitions)
            {
                var resourceName = resource.Resource;
                var resourceNode = resourceNodes.SingleOrDefault(n => n.GetValue("Resource") == resourceName) ?? new ConfigNode();
                resources[resourceName] = Kethane.ResourceData.Load(resource, resourceNode);
            }

            timer.Stop();
            Debug.LogWarning(String.Format("Kethane deposits loaded ({0}ms)", timer.ElapsedMilliseconds));
			if (UserInterface.MapOverlay.Instance != null) {
				UserInterface.MapOverlay.Instance.ClearBody ();
			}
        }

        public override void OnSave(ConfigNode configNode)
        {
            var timer = System.Diagnostics.Stopwatch.StartNew();

            configNode.AddValue("Version", System.Diagnostics.FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetExecutingAssembly().Location).ProductVersion);

            foreach (var resource in resources)
            {
                var resourceNode = new ConfigNode("Resource");
                resourceNode.AddValue("Resource", resource.Key);
                resource.Value.Save(resourceNode);
                configNode.AddNode(resourceNode);
            }

            timer.Stop();
            Debug.LogWarning(String.Format("Kethane deposits saved ({0}ms)", timer.ElapsedMilliseconds));
        }

		public override void OnAwake ()
		{
			Current = this;
			Debug.LogFormat("[KethaneData] OnAwake");
		}

		void OnDestroy ()
		{
			Current = null;
		}
    }
}
