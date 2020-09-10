using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

using KethaneParticles;

namespace Kethane
{
	[KSPAddon (KSPAddon.Startup.Flight, false)]
	public class ParticleManager : MonoBehaviour
	{
		public class System {
			public ParticleSystem psystem;
			public ParticlePhysics physics;
		}

		public class Emitter
		{
			public bool enabled;
			public float rate;
			public Vector3 position;
			public Vector3 direction;
			public System system;
		}

		static Dictionary<string, ConfigNode> systemCfg;
		static ParticleManager instance;

		Dictionary<string, System> systems;

		Emitter createEmitter (string system)
		{
			System sys;
			if (!systems.TryGetValue (system, out sys)) {
				sys = createSystem (system);
				if (sys == null) {
					return null;
				}
				systems[system] = sys;
			}
			var emitter = new Emitter ();
			emitter.system = sys;
			return emitter;
		}

		public static Emitter CreateEmitter (string system)
		{
			return instance.createEmitter (system);
		}

		static System createSystem (string name)
		{
			ConfigNode node;
			System sys;

			if (!systemCfg.TryGetValue (name, out node)) {
				Debug.Log ($"[Kethane.ParticleManager] unknown system: {name}");
				return null;
			}

			var go = new GameObject ($"KethaneParticleSystem:{name}");
			Debug.Log ($"[Kethane.ParticleManager] creating {go.name}");
			var psys = new ParticleSystemCfg (go);
			psys.Load (node.GetNode ("Particles"));
			sys = new System();
			sys.psystem = psys.psystem;
			sys.physics = go.AddComponent<ParticlePhysics> ();
			sys.physics.psystem = psys.psystem;
			sys.physics.Load (node.GetNode ("Physics"));
			sys.psystem.Play ();
			return sys;
		}

		static void LoadSystemConfigs ()
		{
			var dbase = GameDatabase.Instance;
			var node_list = dbase.GetConfigNodes ("KethaneParticleSystem");

			systemCfg = new Dictionary<string, ConfigNode> ();
			for (int i = 0; i < node_list.Length; i++) {
				var node = node_list[i];
				string name = node.GetValue ("name");
				if (String.IsNullOrEmpty (name)) {
					Debug.Log ($"[Kethane.ParticleManager] skipping unnamed system");
					continue;
				}
				if (systemCfg.ContainsKey (name)) {
					Debug.Log ($"[Kethane.ParticleManager] duplicate system name: {name}");
					continue;
				}
				systemCfg[name] = node;
			}
		}

		void onFloatingOriginShift (Vector3d refPos, Vector3d nonFrame)
		{
		}

		void Awake ()
		{
			instance = this;
			systems = new Dictionary<string, System> ();
			if (systemCfg == null) {
				LoadSystemConfigs ();
			}
			GameEvents.onFloatingOriginShift.Add (onFloatingOriginShift);
		}

		void OnDestroy ()
		{
			Debug.Log ($"[ParticleManager] OnDestroy");
			GameEvents.onFloatingOriginShift.Remove (onFloatingOriginShift);
		}
	}
}
