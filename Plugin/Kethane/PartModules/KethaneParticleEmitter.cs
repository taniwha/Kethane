using System;
using System.Linq;
using UnityEngine;

namespace Kethane.PartModules
{
    public class KethaneParticleEmitter : PartModule
    {
		ParticleManager.Emitter emitter;

        public bool Emit
        {
            get {
				if (emitter != null) {
					return emitter.enabled;
				}
				return false;
			}
            set {
				if (value && emitter == null) {
					emitter = ParticleManager.CreateEmitter (System);
					emitter.position = Position;
					emitter.direction = Direction;
				}
				if (emitter != null) {
					emitter.enabled = value;
				}
			}
        }

		public float Rate
		{
			get {
				if (emitter != null) {
					return emitter.rate;
				}
				return 0;
			}
			set {
				if (emitter == null) {
					Emit = false;
				}
				if (emitter != null) {
					emitter.rate = value;
				}
			}
		}

        [KSPField(isPersistant = false)]
        public string Label;

        [KSPField(isPersistant = false)]
		public string System;

        [KSPField(isPersistant = false)]
		public Vector3 Position;

        [KSPField(isPersistant = false)]
		public Vector3 Direction;

        public override void OnLoad(ConfigNode config)
        {
        }

        public override void OnSave(ConfigNode config)
        {
        }

        public override void OnStart(StartState state)
        {
            if (!HighLogic.LoadedSceneIsFlight) { return; }
        }
    }
}
