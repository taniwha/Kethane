using System;
using System.Linq;
using UnityEngine;

namespace Kethane.PartModules
{
    public class KethaneParticleEmitter : PartModule
    {
        #region Particle property bindings

        public float AngularVelocity
        {
            get { return emitter.angularVelocity; }
            set { emitter.angularVelocity = value; }
        }

        public float CameraVelocityScale
        {
            get { return renderer.cameraVelocityScale; }
            set { renderer.cameraVelocityScale = value; }
        }

		public Color GetColorKey(int ind)
		{
			Color c = colorGradient.colorKeys[ind].color;
			c.a = colorGradient.alphaKeys[ind].alpha;
			return c;
		}

		public void SetColorKey(int ind, Color c)
		{
			colorGradient.colorKeys[ind].color = c;
			colorGradient.alphaKeys[ind].alpha = c.a;
		}

        public Color ColorAnimation1
        {
            get { return GetColorKey(0); }
            set { SetColorKey(0, value); }
        }

        public Color ColorAnimation2
        {
            get { return GetColorKey(1); }
            set { SetColorKey(1, value); }
        }

        public Color ColorAnimation3
        {
            get { return GetColorKey(2); }
            set { SetColorKey(2, value); }
        }

        public Color ColorAnimation4
        {
            get { return GetColorKey(3); }
            set { SetColorKey(3, value); }
        }

        public Color ColorAnimation5
        {
            get { return GetColorKey(4); }
            set { SetColorKey(4, value); }
        }

        public float Damping
        {
            get { return animator.damping; }
            set { animator.damping = value; }
        }

        public bool Emit
        {
            get { return emission.enabled; }
            set { emission.enabled = value; }
        }

        public float EmitterVelocityScale
        {
            get { return inheritVelocity.enabled ? inheritVelocity.curve : 0; }
            set {
				inheritVelocity.enabled = !Mathf.Approximately(value, 0.0f);
				inheritVelocity.curve = inheritVelocity.enabled ? value : 0.0f;
			}
        }

        public Vector3 Force
        {
            get { return animator.force; }
            set { animator.force = value; }
        }

        public float LengthScale
        {
            get { return renderer.lengthScale; }
            set { renderer.lengthScale = value; }
        }

        public Vector3 LocalRotationAxis
        {
            get { return animator.localRotationAxis; }
            set { animator.localRotationAxis = value; }
        }

        public Vector3 LocalVelocity
        {
            get { return emitter.localVelocity; }
            set { emitter.localVelocity = value; }
        }

        public float MaxEmission
        {
            get { return emitter.maxEmission; }
            set { emitter.maxEmission = value; }
        }

        public float MaxEnergy
        {
            get { return emitter.maxEnergy; }
            set { emitter.maxEnergy = value; }
        }

        public float MaxParticleSize
        {
            get { return renderer.maxParticleSize; }
            set { renderer.maxParticleSize = value; }
        }

        public float MaxSize
        {
            get { return emitter.maxSize; }
            set { emitter.maxSize = value; }
        }

        public float MinEmission
        {
            get { return emitter.minEmission; }
            set { emitter.minEmission = value; }
        }

        public float MinEnergy
        {
            get { return emitter.minEnergy; }
            set { emitter.minEnergy = value; }
        }

        public float MinSize
        {
            get { return emitter.minSize; }
            set { emitter.minSize = value; }
        }

        public float RandomAngularVelocity
        {
            get { return emitter.rndAngularVelocity; }
            set { emitter.rndAngularVelocity = value; }
        }

        public Vector3 RandomForce
        {
            get { return animator.rndForce; }
            set { animator.rndForce = value; }
        }

        public bool RandomRotation
        {
            get { return emitter.rndRotation; }
            set { emitter.rndRotation = value; }
        }

        public Vector3 RandomVelocity
        {
            get { return emitter.rndVelocity; }
            set { emitter.rndVelocity = value; }
        }

        public ParticleSystemRenderMode RenderMode
        {
            get { return renderer.particleRenderMode; }
            set { renderer.particleRenderMode = value; }
        }

        public float SizeGrow
        {
            get { return animator.sizeGrow; }
            set { animator.sizeGrow = value; }
        }

        public bool UseWorldSpace
        {
            get { return main.simulationSpace == ParticleSystemSimulationSpace.World; }
            set { main.simulationSpace = value ? ParticleSystemSimulationSpace.World : ParticleSystemSimulationSpace.Local; }
        }

        public float VelocityScale
        {
            get { return renderer.velocityScale; }
            set { renderer.velocityScale = value; }
        }

        #endregion

        public Vector3 EmitterPosition
        {
            get { return obj.transform.localPosition; }
            set { obj.transform.localPosition = value; }
        }

        public Vector3 EmitterScale
        {
            get { return obj.transform.localScale; }
            set { obj.transform.localScale = value; }
        }

        public Transform EmitterTransform
        {
            get { return obj.transform; }
        }

        public int ParticleCount
        {
            get { return psystem.particleCount; }
        }

        [KSPField(isPersistant = false)]
        public String Label;

        public string configString;

        private GameObject obj;
		private ParticleSystem psystem;
        private ParticleSystem.MainModule main;
        private ParticleSystem.EmissionModule emission;
        private ParticleSystem.ShapeModule shape;
        private ParticleSystem.InheritVelocityModule inheritVelocity;
		private Gradient colorGradient;
        private ParticleSystem.ColorOverLifetimeModule colorOverLifetime;
        private ParticleSystem.ForceOverLifetimeModule forceOverLifetime;
        private ParticleSystem.RotationOverLifetimeModule rotationOverLifetime;
        private ParticleSystem.SizeOverLifetimeModule sizeOverLifetime;
        private ParticleSystem.VelocityOverLifetimeModule velocityOverLifetime;
        private ParticleSystemRenderer renderer;

        public override void OnLoad(ConfigNode config)
        {
            if (this.configString == null)
            {
                this.configString = config.ToString();
            }
        }

        public override void OnStart(StartState state)
        {
            if (state == StartState.Editor) { return; }
            Setup();
        }

        public void Setup()
        {
            if (part.partInfo == null) { return; }
            if (obj != null) { return; }
            ConfigNode config = Misc.Parse(configString).GetNode("MODULE");
            var shaderName = config.GetValue("ShaderName");
            var textureName = config.GetValue("TextureName");

            obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
			var c = obj.GetComponent<Collider>();
            c.enabled = false;
			var r = obj.GetComponent<Renderer>();
            r.material.color = new Color(0, 0, 0, 0);
            r.material.shader = Shader.Find("Transparent/Diffuse");
            obj.transform.parent = part.transform;
            obj.transform.localRotation = Quaternion.identity;

			psystem = obj.AddComponent<ParticleSystem>();
			main = psystem.main;
			emission = psystem.emission;
			shape = psystem.shape;
			inheritVelocity = psystem.inheritVelocity;
			colorOverLifetime = psystem.colorOverLifetime;
			forceOverLifetime = psystem.forceOverLifetime;
			rotationOverLifetime = psystem.rotationOverLifetime;
			sizeOverLifetime = psystem.sizeOverLifetime;
			velocityOverLifetime = psystem.velocityOverLifetime;
            renderer = obj.GetComponent<ParticleSystemRenderer>();

            var material = new Material(Shader.Find(shaderName));
            material.mainTexture = GameDatabase.Instance.GetTexture(textureName, false);
            material.color = Color.white;

            renderer.materials = new Material[] { material };
			colorGradient = new Gradient();
			colorGradient.colorKeys = new GradientColorKey[] {
				new GradientColorKey(Color.black, 0.0f),
				new GradientColorKey(Color.black, 0.25f),
				new GradientColorKey(Color.black, 0.5f),
				new GradientColorKey(Color.black, 0.75f),
				new GradientColorKey(Color.black, 1.0f),
			};
			colorGradient.alphaKeys = new GradientAlphaKey[] {
				new GradientAlphaKey(0, 0.0f),
				new GradientAlphaKey(0, 0.25f),
				new GradientAlphaKey(0, 0.5f),
				new GradientAlphaKey(0, 0.75f),
				new GradientAlphaKey(0, 1.0f),
			};
			colorOverLifetime.enabled = true;
			colorOverLifetime.color = new ParticleSystem.MinMaxGradient(colorGradient);

            //if (Misc.Parse(config.GetValue("Collision"), false))
            //{
            //    obj.AddComponent<WorldParticleCollider>();
            //}

            AngularVelocity         = Misc.Parse(config.GetValue("AngularVelocity"), 0f);
            CameraVelocityScale     = Misc.Parse(config.GetValue("CameraVelocityScale"), 0f);
            ColorAnimation1         = ConfigNode.ParseColor(config.GetValue("ColorAnimation1"));
            ColorAnimation2         = ConfigNode.ParseColor(config.GetValue("ColorAnimation2"));
            ColorAnimation3         = ConfigNode.ParseColor(config.GetValue("ColorAnimation3"));
            ColorAnimation4         = ConfigNode.ParseColor(config.GetValue("ColorAnimation4"));
            ColorAnimation5         = ConfigNode.ParseColor(config.GetValue("ColorAnimation5"));
            Damping                 = Misc.Parse(config.GetValue("Damping"), 1f);
            Emit                    = Misc.Parse(config.GetValue("Emit"), true);
            EmitterVelocityScale    = Misc.Parse(config.GetValue("EmitterVelocityScale"), 1f);
            Force                   = Misc.Parse(config.GetValue("Force"), Vector3.zero);
            LengthScale             = Misc.Parse(config.GetValue("LengthScale"), 1f);
            LocalVelocity           = Misc.Parse(config.GetValue("LocalVelocity"), Vector3.zero);
            MaxEmission             = Misc.Parse(config.GetValue("MaxEmission"), 0f);
            MaxEnergy               = Misc.Parse(config.GetValue("MaxEnergy"), 0f);
            MaxParticleSize         = Misc.Parse(config.GetValue("MaxParticleSize"), 0f);
            MaxSize                 = Misc.Parse(config.GetValue("MaxSize"), 0f);
            MinEmission             = Misc.Parse(config.GetValue("MinEmission"), 0f);
            MinEnergy               = Misc.Parse(config.GetValue("MinEnergy"), 0f);
            MinSize                 = Misc.Parse(config.GetValue("MinSize"), 0f);
            RandomAngularVelocity   = Misc.Parse(config.GetValue("RandomAngularVelocity"), 0f);
            RandomForce             = Misc.Parse(config.GetValue("RandomForce"), Vector3.zero);
            RandomRotation          = Misc.Parse(config.GetValue("RandomRotation"), false);
            RandomVelocity          = Misc.Parse(config.GetValue("RandomVelocity"), Vector3.zero);
            RenderMode              = Misc.Parse(config.GetValue("RenderMode"), ParticleSystemRenderMode.Billboard);
            SizeGrow                = Misc.Parse(config.GetValue("SizeGrow"), 0f);
            UseWorldSpace           = Misc.Parse(config.GetValue("UseWorldSpace"), false);
            VelocityScale           = Misc.Parse(config.GetValue("VelocityScale"), 0f);

            EmitterPosition         = Misc.Parse(config.GetValue("EmitterPosition"), Vector3.zero);
            EmitterScale            = Misc.Parse(config.GetValue("EmitterScale"), Vector3.zero);
        }
    }
}
