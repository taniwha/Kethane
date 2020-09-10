using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

namespace KethaneParticles
{

	public class ParticleSystemCfg
	{
		public delegate Shader FindShaderDelegate(string name);
		public static FindShaderDelegate FindShader;
		public ParticleSystem psystem { get; private set; }
		public ParticleSystemRenderer psrenderer {get; private set; }
		public GameObject gameObject { get; private set; }

		public ParticleSystemCfg (string name)
		{
			gameObject = new GameObject (name, typeof (ParticleSystem));
			psystem = gameObject.GetComponent<ParticleSystem> ();
			if (psystem == null) {
				psystem = gameObject.AddComponent<ParticleSystem> ();
			}
			psrenderer = gameObject.GetComponent<ParticleSystemRenderer> ();
		}

		public ParticleSystemCfg (GameObject gameObject)
		{
			this.gameObject = gameObject;
			psystem = gameObject.GetComponent<ParticleSystem> ();
			if (psystem == null) {
				psystem = gameObject.AddComponent<ParticleSystem> ();
			}
			psrenderer = gameObject.GetComponent<ParticleSystemRenderer> ();
		}

		public static TEnum ParseEnum<TEnum> (ConfigNode node, string name, TEnum defaultValue)
		{
			if (node.HasValue (name)) {
				string enumStr = node.GetValue (name);
				return enumStr.ToEnum (defaultValue);
			}
			return defaultValue;
		}

		public static Transform ParseTransform (ConfigNode node, string name, Transform defaultValue)
		{
			if (node.HasValue (name)) {
				var go = GameObject.Find (node.GetValue (name));
				if (go != null) {
					return go.transform;
				}
			}
			return defaultValue;
		}

		public static ParticleSystem.MinMaxCurve ParseMinMaxCurve (ConfigNode node, string name, ParticleSystem.MinMaxCurve defaultValue)
		{
			if (node.HasValue (name)) {
				float val;
				float.TryParse (node.GetValue (name), out val);
				return new ParticleSystem.MinMaxCurve (val);
			} else if (node.HasNode (name)) {
				var curve = new MinMaxCurveCfg ();
				curve.Load (node.GetNode (name));
				return curve.curve;
			}
			return defaultValue;
		}

		public static ParticleSystem.MinMaxGradient ParseMinMaxGradient (ConfigNode node, string name, ParticleSystem.MinMaxGradient defaultValue)
		{
			if (node.HasNode (name)) {
				var gradient = new MinMaxGradientCfg ();
				gradient.Load (node.GetNode (name));
				return gradient.gradient;
			}
			return defaultValue;
		}

		void ParseMain (ConfigNode node)
		{
			var main = psystem.main;

			main.playOnAwake = Utils.ParseBool (node, "playOnAwake", main.playOnAwake);

			main.cullingMode = ParseEnum<ParticleSystemCullingMode> (node, "cullingMode", main.cullingMode);
			main.customSimulationSpace = ParseTransform (node, "customSimulationSpace", main.customSimulationSpace);
			main.duration = Utils.ParseFloat (node, "duration", main.duration);
			main.emitterVelocityMode = ParseEnum<ParticleSystemEmitterVelocityMode> (node, "emitterVelocityMode", main.emitterVelocityMode);
			main.flipRotation = Utils.ParseFloat (node, "flipRotation", main.flipRotation);
			main.gravityModifier = ParseMinMaxCurve (node, "gravityModifier", main.gravityModifier);
			main.gravityModifierMultiplier = Utils.ParseFloat (node, "gravityModifierMultiplier", main.gravityModifierMultiplier);
			main.loop = Utils.ParseBool (node, "loop", main.loop);
			main.maxParticles = Utils.ParseInt (node, "maxParticles", main.maxParticles);
			main.prewarm = Utils.ParseBool (node, "prewarm", main.prewarm);
			main.ringBufferLoopRange = Utils.ParseVector2 (node, "ringBufferLoopRange", main.ringBufferLoopRange);
			main.ringBufferMode = ParseEnum<ParticleSystemRingBufferMode> (node, "ringBufferMode", main.ringBufferMode);
			main.scalingMode = ParseEnum<ParticleSystemScalingMode> (node, "scalingMode", main.scalingMode);
			main.simulationSpace = ParseEnum<ParticleSystemSimulationSpace> (node, "simulationSpace", main.simulationSpace);
			main.simulationSpeed = Utils.ParseFloat (node, "simulationSpeed", main.simulationSpeed);
			main.startColor = ParseMinMaxGradient (node, "startColor", main.startColor);
			main.startDelay = ParseMinMaxCurve (node, "startDelay", main.startDelay);
			main.startDelayMultiplier = Utils.ParseFloat (node, "startDelayMultiplier", main.startDelayMultiplier);
			main.startLifetime = ParseMinMaxCurve (node, "startLifetime", main.startLifetime);
			main.startLifetimeMultiplier = Utils.ParseFloat (node, "startLifetimeMultiplier", main.startLifetimeMultiplier);
			main.startRotation = ParseMinMaxCurve (node, "startRotation", main.startRotation);
			main.startRotation3D = Utils.ParseBool (node, "startRotation3D", main.startRotation3D);
			main.startRotationMultiplier = Utils.ParseFloat (node, "startRotationMultiplier", main.startRotationMultiplier);
			main.startRotationX = ParseMinMaxCurve (node, "startRotationX", main.startRotationX);
			main.startRotationXMultiplier = Utils.ParseFloat (node, "startRotationXMultiplier", main.startRotationXMultiplier);
			main.startRotationY = ParseMinMaxCurve (node, "startRotationY", main.startRotationY);
			main.startRotationYMultiplier = Utils.ParseFloat (node, "startRotationYMultiplier", main.startRotationYMultiplier);
			main.startRotationZ = ParseMinMaxCurve (node, "startRotationZ", main.startRotationZ);
			main.startRotationZMultiplier = Utils.ParseFloat (node, "startRotationZMultiplier", main.startRotationZMultiplier);
			main.startSize = ParseMinMaxCurve (node, "startSize", main.startSize);
			main.startSize3D = Utils.ParseBool (node, "startSize3D", main.startSize3D);
			main.startSizeMultiplier = Utils.ParseFloat (node, "startSizeMultiplier", main.startSizeMultiplier);
			main.startSizeX = ParseMinMaxCurve (node, "startSizeX", main.startSizeX);
			main.startSizeXMultiplier = Utils.ParseFloat (node, "startSizeXMultiplier", main.startSizeXMultiplier);
			main.startSizeY = ParseMinMaxCurve (node, "startSizeY", main.startSizeY);
			main.startSizeYMultiplier = Utils.ParseFloat (node, "startSizeYMultiplier", main.startSizeYMultiplier);
			main.startSizeZ = ParseMinMaxCurve (node, "startSizeZ", main.startSizeZ);
			main.startSizeZMultiplier = Utils.ParseFloat (node, "startSizeZMultiplier", main.startSizeZMultiplier);
			main.startSpeed = ParseMinMaxCurve (node, "startSpeed", main.startSpeed);
			main.startSpeedMultiplier = Utils.ParseFloat (node, "startSpeedMultiplier", main.startSpeedMultiplier);
			main.stopAction = ParseEnum<ParticleSystemStopAction> (node, "stopAction", main.stopAction);
			main.useUnscaledTime = Utils.ParseBool (node, "useUnscaledTime", main.useUnscaledTime);
		}

		void ParseCollision (ConfigNode node)
		{
			var mod = psystem.collision;
			mod.bounce = ParseMinMaxCurve (node, "bounce", mod.bounce);
			mod.bounceMultiplier = Utils.ParseFloat (node, "bounceMultiplier", mod.bounceMultiplier);
			mod.colliderForce = Utils.ParseFloat (node, "colliderForce", mod.colliderForce);
			mod.collidesWith = Utils.ParseLayerMask (node, "collidesWith", mod.collidesWith);
			mod.dampen = ParseMinMaxCurve (node, "dampen", mod.dampen);
			mod.dampenMultiplier = Utils.ParseFloat (node, "dampenMultiplier", mod.dampenMultiplier);
			mod.enabled = Utils.ParseBool (node, "enabled", true);
			mod.enableDynamicColliders = Utils.ParseBool (node, "enableDynamicColliders", mod.enableDynamicColliders);
			mod.lifetimeLoss = ParseMinMaxCurve (node, "dampen", mod.dampen);
			mod.lifetimeLossMultiplier = Utils.ParseFloat (node, "lifetimeLossMultiplier", mod.lifetimeLossMultiplier);
			mod.maxCollisionShapes = Utils.ParseInt (node, "maxCollisionShapes", mod.maxCollisionShapes);
			mod.maxKillSpeed = Utils.ParseFloat (node, "maxKillSpeed", mod.maxKillSpeed);
			mod.minKillSpeed = Utils.ParseFloat (node, "minKillSpeed", mod.minKillSpeed);
			mod.mode = ParseEnum<ParticleSystemCollisionMode> (node, "mode", mod.mode);
			mod.multiplyColliderForceByCollisionAngle = Utils.ParseBool (node, "multiplyColliderForceByCollisionAngle", mod.multiplyColliderForceByCollisionAngle);
			mod.multiplyColliderForceByParticleSize = Utils.ParseBool (node, "multiplyColliderForceByParticleSize", mod.multiplyColliderForceByParticleSize);
			mod.multiplyColliderForceByParticleSpeed = Utils.ParseBool (node, "multiplyColliderForceByParticleSpeed", mod.multiplyColliderForceByParticleSpeed);
			mod.quality = ParseEnum<ParticleSystemCollisionQuality> (node, "quality", mod.quality);
			mod.radiusScale = Utils.ParseFloat (node, "radiusScale", mod.radiusScale);
			mod.sendCollisionMessages = Utils.ParseBool (node, "sendCollisionMessages", mod.sendCollisionMessages);
			mod.type = ParseEnum<ParticleSystemCollisionType> (node, "type", mod.type);
			mod.voxelSize = Utils.ParseFloat (node, "voxelSize", mod.voxelSize);
			//XXX
			if (node.HasValue ("planes")) {
				var planes = Utils.ParseArray (node.GetValue ("planes"));
				int index = 0;
				foreach (var p in planes) {
					var go = GameObject.Find (p);
					if (go != null) {
						mod.SetPlane (index++, go.transform);
					}
				}
			}
		}

		void ParseColorBySpeed (ConfigNode node)
		{
			var mod = psystem.colorBySpeed;

			mod.color = ParseMinMaxGradient (node, "color", mod.color);
			mod.enabled = Utils.ParseBool (node, "enabled", true);
			mod.range = Utils.ParseVector2 (node, "range", mod.range);
		}

		void ParseColorOverLifetime (ConfigNode node)
		{
			var mod = psystem.colorOverLifetime;

			mod.color = ParseMinMaxGradient (node, "color", mod.color);
			mod.enabled = Utils.ParseBool (node, "enabled", true);
		}

		void ParseEmission (ConfigNode node)
		{
			var mod = psystem.emission;

			mod.enabled = Utils.ParseBool (node, "enabled", true);
			mod.rateOverDistance = ParseMinMaxCurve (node, "rateOverDistance", mod.rateOverDistance);
			mod.rateOverDistanceMultiplier = Utils.ParseFloat (node, "rateOverDistanceMultiplier", mod.rateOverDistanceMultiplier);
			mod.rateOverTime = ParseMinMaxCurve (node, "rateOverTime", mod.rateOverTime);
			mod.rateOverTimeMultiplier = Utils.ParseFloat (node, "rateOverTimeMultiplier", mod.rateOverTimeMultiplier);
			//FIXME bursts
		}

		void ParseExternalForces (ConfigNode node)
		{
			var mod = psystem.externalForces;
			mod.enabled = Utils.ParseBool (node, "enabled", true);
			mod.influenceFilter = ParseEnum<ParticleSystemGameObjectFilter> (node, "influenceFilter", mod.influenceFilter);
			mod.influenceMask = Utils.ParseLayerMask (node, "influenceMask", mod.influenceMask);
			mod.multiplier = Utils.ParseFloat (node, "multiplier", mod.multiplier);
			mod.multiplierCurve = ParseMinMaxCurve (node, "multiplierCurve", mod.multiplierCurve);
			mod.RemoveAllInfluences ();
			if (node.HasValue ("forceFields")) {
				var forceFields = Utils.ParseArray (node.GetValue ("forceFields"));
				foreach (var ff in forceFields) {
					var go = GameObject.Find (ff);
					if (go != null) {
						mod.AddInfluence (go.GetComponent<ParticleSystemForceField> ());
					}
				}
			}
		}

		void ParseForceOverLifetime (ConfigNode node)
		{
			var mod = psystem.forceOverLifetime;

			mod.enabled = Utils.ParseBool (node, "enabled", true);
			mod.randomized = Utils.ParseBool (node, "randomized", mod.randomized);
			mod.space = ParseEnum<ParticleSystemSimulationSpace> (node, "space", mod.space);
			mod.x = ParseMinMaxCurve (node, "x", mod.x);
			mod.xMultiplier = Utils.ParseFloat (node, "xMultiplier", mod.xMultiplier);
			mod.y = ParseMinMaxCurve (node, "y", mod.y);
			mod.yMultiplier = Utils.ParseFloat (node, "yMultiplier", mod.yMultiplier);
			mod.z = ParseMinMaxCurve (node, "z", mod.z);
			mod.zMultiplier = Utils.ParseFloat (node, "zMultiplier", mod.zMultiplier);
		}

		void ParseInheritVelocity (ConfigNode node)
		{
			var mod = psystem.inheritVelocity;

			mod.curve = ParseMinMaxCurve (node, "curve", mod.curve);
			mod.curveMultiplier = Utils.ParseFloat (node, "curveMultiplier", mod.curveMultiplier);
			mod.enabled = Utils.ParseBool (node, "enabled", true);
			mod.mode = ParseEnum<ParticleSystemInheritVelocityMode> (node, "mode", mod.mode);
		}

		void ParseLimitVelocityOverLifetime (ConfigNode node)
		{
			var mod = psystem.limitVelocityOverLifetime;

			mod.separateAxes = Utils.ParseBool (node, "separateAxes", mod.separateAxes);
			mod.dampen = Utils.ParseFloat (node, "dampen", mod.dampen);
			//mod.drag = ParseMinMaxCurve (node, "drag", mod.drag);
			mod.dragMultiplier = Utils.ParseFloat (node, "dragMultiplier", mod.dragMultiplier);
			mod.enabled = Utils.ParseBool (node, "enabled", true);
			mod.limit = ParseMinMaxCurve (node, "limit", mod.limit);
			mod.limitMultiplier = Utils.ParseFloat (node, "limitMultiplier", mod.limitMultiplier);
			mod.limitX = ParseMinMaxCurve (node, "limitX", mod.limitX);
			mod.limitXMultiplier = Utils.ParseFloat (node, "limitXMultiplier", mod.limitXMultiplier);
			mod.limitY = ParseMinMaxCurve (node, "limitY", mod.limitY);
			mod.limitYMultiplier = Utils.ParseFloat (node, "limitYMultiplier", mod.limitYMultiplier);
			mod.limitZ = ParseMinMaxCurve (node, "limitZ", mod.limitZ);
			mod.limitZMultiplier = Utils.ParseFloat (node, "limitZMultiplier", mod.limitZMultiplier);
			mod.multiplyDragByParticleSize = Utils.ParseBool (node, "multiplyDragByParticleSize", mod.multiplyDragByParticleSize);
			mod.multiplyDragByParticleVelocity = Utils.ParseBool (node, "multiplyDragByParticleVelocity", mod.multiplyDragByParticleVelocity);
			mod.space = ParseEnum<ParticleSystemSimulationSpace> (node, "space", mod.space);
		}

		void ParseRotationBySpeed (ConfigNode node)
		{
			var mod = psystem.rotationBySpeed;
			mod.enabled = Utils.ParseBool (node, "enabled", true);
			mod.range = Utils.ParseVector2 (node, "range", mod.range);
			mod.separateAxes = Utils.ParseBool (node, "separateAxes", mod.separateAxes);
			mod.x = ParseMinMaxCurve (node, "x", mod.x);
			mod.xMultiplier = Utils.ParseFloat (node, "xMultiplier", mod.xMultiplier);
			mod.y = ParseMinMaxCurve (node, "y", mod.y);
			mod.yMultiplier = Utils.ParseFloat (node, "yMultiplier", mod.yMultiplier);
			mod.z = ParseMinMaxCurve (node, "z", mod.z);
			mod.zMultiplier = Utils.ParseFloat (node, "zMultiplier", mod.zMultiplier);
		}

		void ParseRotationOverLifetime (ConfigNode node)
		{
			var mod = psystem.rotationOverLifetime;
			mod.enabled = Utils.ParseBool (node, "enabled", true);
			mod.separateAxes = Utils.ParseBool (node, "separateAxes", mod.separateAxes);
			mod.x = ParseMinMaxCurve (node, "x", mod.x);
			mod.xMultiplier = Utils.ParseFloat (node, "xMultiplier", mod.xMultiplier);
			mod.y = ParseMinMaxCurve (node, "y", mod.y);
			mod.yMultiplier = Utils.ParseFloat (node, "yMultiplier", mod.yMultiplier);
			mod.z = ParseMinMaxCurve (node, "z", mod.z);
			mod.zMultiplier = Utils.ParseFloat (node, "zMultiplier", mod.zMultiplier);
		}

		void ParseShape (ConfigNode node)
		{
			var mod = psystem.shape;
			mod.alignToDirection = Utils.ParseBool (node, "alignToDirection", mod.alignToDirection);
			mod.angle = Utils.ParseFloat (node, "angle", mod.angle);
			mod.arc = Utils.ParseFloat (node, "arc", mod.arc);
			mod.arcMode = ParseEnum<ParticleSystemShapeMultiModeValue> (node, "arcMode", mod.arcMode);
			mod.arcSpeed = ParseMinMaxCurve (node, "arcSpeed", mod.arcSpeed);
			mod.arcSpeedMultiplier = Utils.ParseFloat (node, "arcSpeedMultiplier", mod.arcSpeedMultiplier);
			mod.arcSpread = Utils.ParseFloat (node, "arcSpread", mod.arcSpread);
			mod.boxThickness = Utils.ParseVector3 (node, "boxThickness", mod.boxThickness);
			mod.donutRadius = Utils.ParseFloat (node, "donutRadius", mod.donutRadius);
			mod.enabled = Utils.ParseBool (node, "enabled", true);
			mod.length = Utils.ParseFloat (node, "length", mod.length);
			//FIXME mod.mesh = Utils.ParseBool (node, "mesh", mod.mesh);
			if (node.HasValue ("mesh")) {
				var go = GameObject.Find (node.GetValue ("mesh"));
				var mf = go.GetComponent<MeshFilter> ();
				if (mf) {
					mod.mesh = mf.mesh;
				}
				mod.meshRenderer = go.GetComponent<MeshRenderer> ();
			}
			mod.meshMaterialIndex = Utils.ParseInt (node, "meshMaterialIndex", mod.meshMaterialIndex);
			//FIXME mod.meshRenderer = Utils.ParseBool (node, "meshRenderer", mod.meshRenderer);
			mod.meshShapeType = ParseEnum<ParticleSystemMeshShapeType> (node, "meshShapeType", mod.meshShapeType);
			mod.meshSpawnMode = ParseEnum<ParticleSystemShapeMultiModeValue> (node, "meshSpawnMode", mod.meshSpawnMode);
			mod.meshSpawnSpeed = ParseMinMaxCurve (node, "meshSpawnSpeed", mod.meshSpawnSpeed);
			mod.meshSpawnSpeedMultiplier = Utils.ParseFloat (node, "meshSpawnSpeedMultiplier", mod.meshSpawnSpeedMultiplier);
			mod.meshSpawnSpread = Utils.ParseFloat (node, "meshSpawnSpread", mod.meshSpawnSpread);
			mod.normalOffset = Utils.ParseFloat (node, "normalOffset", mod.normalOffset);
			mod.position = Utils.ParseVector3 (node, "position", mod.position);
			mod.radius = Utils.ParseFloat (node, "radius", mod.radius);
			mod.radiusMode = ParseEnum<ParticleSystemShapeMultiModeValue> (node, "radiusMode", mod.radiusMode);
			mod.radiusSpeed = ParseMinMaxCurve (node, "radiusSpeed", mod.radiusSpeed);
			mod.radiusSpeedMultiplier = Utils.ParseFloat (node, "radiusSpeedMultiplier", mod.radiusSpeedMultiplier);
			mod.radiusSpread = Utils.ParseFloat (node, "radiusSpread", mod.radiusSpread);
			mod.radiusThickness = Utils.ParseFloat (node, "radiusThickness", mod.radiusThickness);
			mod.randomDirectionAmount = Utils.ParseFloat (node, "randomDirectionAmount", mod.randomDirectionAmount);
			mod.randomPositionAmount = Utils.ParseFloat (node, "randomPositionAmount", mod.randomPositionAmount);
			mod.rotation = Utils.ParseVector3 (node, "rotation", mod.rotation);
			mod.scale = Utils.ParseVector3 (node, "scale", mod.scale);
			mod.shapeType = ParseEnum<ParticleSystemShapeType> (node, "shapeType", mod.shapeType);
			//FIXME mod.skinnedMeshRenderer = Utils.ParseBool (node, "skinnedMeshRenderer", mod.skinnedMeshRenderer);
			mod.sphericalDirectionAmount = Utils.ParseFloat (node, "sphericalDirectionAmount", mod.sphericalDirectionAmount);
			//FIXME mod.sprite = Utils.ParseBool (node, "sprite", mod.sprite);
			//FIXME mod.spriteRenderer = Utils.ParseBool (node, "spriteRenderer", mod.spriteRenderer);
			//FIXME mod.texture = Utils.ParseBool (node, "texture", mod.texture);
			mod.textureAlphaAffectsParticles = Utils.ParseBool (node, "textureAlphaAffectsParticles", mod.textureAlphaAffectsParticles);
			mod.textureBilinearFiltering = Utils.ParseBool (node, "textureBilinearFiltering", mod.textureBilinearFiltering);
			mod.textureClipChannel = ParseEnum<ParticleSystemShapeTextureChannel> (node, "textureClipChannel", mod.textureClipChannel);
			mod.textureClipThreshold = Utils.ParseFloat (node, "textureClipThreshold", mod.textureClipThreshold);
			mod.textureColorAffectsParticles = Utils.ParseBool (node, "textureColorAffectsParticles", mod.textureColorAffectsParticles);
			mod.textureUVChannel = Utils.ParseInt (node, "textureUVChannel", mod.textureUVChannel);
			mod.useMeshColors = Utils.ParseBool (node, "useMeshColors", mod.useMeshColors);
			mod.useMeshMaterialIndex = Utils.ParseBool (node, "useMeshMaterialIndex", mod.useMeshMaterialIndex);
		}

		void ParseSizeBySpeed (ConfigNode node)
		{
			var mod = psystem.sizeBySpeed;
			mod.enabled = Utils.ParseBool (node, "enabled", true);
			mod.range = Utils.ParseVector2 (node, "range", mod.range);
			mod.separateAxes = Utils.ParseBool (node, "separateAxes", mod.separateAxes);
			mod.size = ParseMinMaxCurve (node, "size", mod.x);
			mod.sizeMultiplier = Utils.ParseFloat (node, "sizeMultiplier", mod.sizeMultiplier);
			mod.x = ParseMinMaxCurve (node, "x", mod.x);
			mod.xMultiplier = Utils.ParseFloat (node, "xMultiplier", mod.xMultiplier);
			mod.y = ParseMinMaxCurve (node, "y", mod.y);
			mod.yMultiplier = Utils.ParseFloat (node, "yMultiplier", mod.yMultiplier);
			mod.z = ParseMinMaxCurve (node, "z", mod.z);
			mod.zMultiplier = Utils.ParseFloat (node, "zMultiplier", mod.zMultiplier);
		}

		void ParseSizeOverLifetime (ConfigNode node)
		{
			var mod = psystem.sizeOverLifetime;
			mod.enabled = Utils.ParseBool (node, "enabled", true);
			mod.separateAxes = Utils.ParseBool (node, "separateAxes", mod.separateAxes);
			mod.size = ParseMinMaxCurve (node, "size", mod.x);
			mod.sizeMultiplier = Utils.ParseFloat (node, "sizeMultiplier", mod.sizeMultiplier);
			mod.x = ParseMinMaxCurve (node, "x", mod.x);
			mod.xMultiplier = Utils.ParseFloat (node, "xMultiplier", mod.xMultiplier);
			mod.y = ParseMinMaxCurve (node, "y", mod.y);
			mod.yMultiplier = Utils.ParseFloat (node, "yMultiplier", mod.yMultiplier);
			mod.z = ParseMinMaxCurve (node, "z", mod.z);
			mod.zMultiplier = Utils.ParseFloat (node, "zMultiplier", mod.zMultiplier);
		}

		void ParseTrails (ConfigNode node)
		{
			var mod = psystem.trails;

			mod.attachRibbonsToTransform = Utils.ParseBool (node, "attachRibbonsToTransform", mod.attachRibbonsToTransform);
			mod.colorOverLifetime = ParseMinMaxGradient (node, "colorOverLifetime", mod.colorOverLifetime);
			mod.colorOverTrail = ParseMinMaxGradient (node, "colorOverTrail", mod.colorOverTrail);
			mod.dieWithParticles = Utils.ParseBool (node, "dieWithParticles", mod.dieWithParticles);
			mod.enabled = Utils.ParseBool (node, "enabled", true);
			mod.generateLightingData = Utils.ParseBool (node, "generateLightingData", mod.generateLightingData);
			mod.inheritParticleColor = Utils.ParseBool (node, "inheritParticleColor", mod.inheritParticleColor);
			mod.lifetime = ParseMinMaxCurve (node, "lifetime", mod.lifetime);
			mod.lifetimeMultiplier = Utils.ParseFloat (node, "lifetimeMultiplier", mod.lifetimeMultiplier);
			mod.minVertexDistance = Utils.ParseFloat (node, "minVertexDistance", mod.minVertexDistance);
			mod.mode = ParseEnum<ParticleSystemTrailMode> (node, "mode", mod.mode);
			mod.ratio = Utils.ParseFloat (node, "ratio", mod.ratio);
			mod.ribbonCount = Utils.ParseInt (node, "ribbonCount", mod.ribbonCount);
			mod.shadowBias = Utils.ParseFloat (node, "shadowBias", mod.shadowBias);
			mod.sizeAffectsLifetime = Utils.ParseBool (node, "sizeAffectsLifetime", mod.sizeAffectsLifetime);
			mod.sizeAffectsWidth = Utils.ParseBool (node, "sizeAffectsWidth", mod.sizeAffectsWidth);
			mod.splitSubEmitterRibbons = Utils.ParseBool (node, "splitSubEmitterRibbons", mod.splitSubEmitterRibbons);
			mod.textureMode = ParseEnum<ParticleSystemTrailTextureMode> (node, "textureMode", mod.textureMode);
			mod.widthOverTrail = ParseMinMaxCurve (node, "widthOverTrail", mod.widthOverTrail);
			mod.widthOverTrailMultiplier = Utils.ParseFloat (node, "widthOverTrailMultiplier", mod.widthOverTrailMultiplier);
			mod.worldSpace = Utils.ParseBool (node, "worldSpace", mod.worldSpace);
		}

		void ParseTrigger (ConfigNode node)
		{
			var mod = psystem.trigger;

			mod.enabled = Utils.ParseBool (node, "enabled", true);
			mod.enter = ParseEnum<ParticleSystemOverlapAction> (node, "enter", mod.enter);
			mod.exit = ParseEnum<ParticleSystemOverlapAction> (node, "exit", mod.exit);
			mod.inside = ParseEnum<ParticleSystemOverlapAction> (node, "inside", mod.inside);
			mod.outside = ParseEnum<ParticleSystemOverlapAction> (node, "inside", mod.inside);
			mod.radiusScale = Utils.ParseFloat (node, "radiusScale", mod.radiusScale);
			if (node.HasValue ("colliders")) {
				var colliders = Utils.ParseArray (node.GetValue ("colliders"));
				int index = 0;
				//XXX
				foreach (var c in colliders) {
					var go = GameObject.Find (c);
					if (go != null) {
						var cols = go.GetComponents<Collider> ();
						foreach (var col in cols) {
							mod.SetCollider (index++, col);
						}
					}
				}
			}
		}

		void ParseVelocityOverLifetime (ConfigNode node)
		{
			var mod = psystem.velocityOverLifetime;

			mod.enabled = Utils.ParseBool (node, "enabled", true);
			mod.orbitalOffsetX = ParseMinMaxCurve (node, "orbitalOffsetX", mod.orbitalOffsetX);
			mod.orbitalOffsetXMultiplier = Utils.ParseFloat (node, "orbitalOffsetXMultiplier", mod.orbitalOffsetXMultiplier);
			mod.orbitalOffsetY = ParseMinMaxCurve (node, "orbitalOffsetY", mod.orbitalOffsetY);
			mod.orbitalOffsetYMultiplier = Utils.ParseFloat (node, "orbitalOffsetYMultiplier", mod.orbitalOffsetYMultiplier);
			mod.orbitalOffsetZ = ParseMinMaxCurve (node, "orbitalOffsetZ", mod.orbitalOffsetZ);
			mod.orbitalOffsetZMultiplier = Utils.ParseFloat (node, "orbitalOffsetZMultiplier", mod.orbitalOffsetZMultiplier);
			mod.orbitalX = ParseMinMaxCurve (node, "orbitalX", mod.orbitalX);
			mod.orbitalXMultiplier = Utils.ParseFloat (node, "orbitalXMultiplier", mod.orbitalXMultiplier);
			mod.orbitalY = ParseMinMaxCurve (node, "orbitalY", mod.orbitalY);
			mod.orbitalYMultiplier = Utils.ParseFloat (node, "orbitalYMultiplier", mod.orbitalYMultiplier);
			mod.orbitalZ = ParseMinMaxCurve (node, "orbitalZ", mod.orbitalZ);
			mod.orbitalZMultiplier = Utils.ParseFloat (node, "orbitalZMultiplier", mod.orbitalZMultiplier);
			mod.radial = ParseMinMaxCurve (node, "radial", mod.radial);
			mod.radialMultiplier = Utils.ParseFloat (node, "radialMultiplier", mod.radialMultiplier);
			mod.space = ParseEnum<ParticleSystemSimulationSpace> (node, "space", mod.space);
			mod.speedModifier = ParseMinMaxCurve (node, "speedModifier", mod.speedModifier);
			mod.speedModifierMultiplier = Utils.ParseFloat (node, "speedModifierMultiplier", mod.speedModifierMultiplier);
			mod.x = ParseMinMaxCurve (node, "x", mod.x);
			mod.xMultiplier = Utils.ParseFloat (node, "xMultiplier", mod.xMultiplier);
			mod.y = ParseMinMaxCurve (node, "y", mod.y);
			mod.yMultiplier = Utils.ParseFloat (node, "yMultiplier", mod.yMultiplier);
			mod.z = ParseMinMaxCurve (node, "z", mod.z);
			mod.zMultiplier = Utils.ParseFloat (node, "zMultiplier", mod.zMultiplier);
		}

		Material ParseMaterial (ConfigNode node, string name, Material mat)
		{
			if (node.HasNode (name)) {
				node = node.GetNode (name);
				Debug.Log ($"[ParticleSystemCfg] ParseMaterial: {name}:{node}");
				string shaderName = node.GetValue ("shader");
				Shader shader = null;
				if (shader == null && FindShader != null) {
					shader = FindShader (shaderName);
				}
				if (shader == null) {
					shader = Shader.Find (shaderName);
				}
				if (shader == null) {
					Debug.Log ($"[ParticleSystemCfg] could not find shader: {shaderName}");
					return mat;
				}
				mat = new Material (shader);
				mat.mainTexture = GameDatabase.Instance.GetTexture (node.GetValue ("texture"), false);
			}
			return mat;
		}

		void ParseRenderer (ConfigNode node)
		{
			psrenderer.alignment = ParseEnum<ParticleSystemRenderSpace> (node, "alignment", psrenderer.alignment);
			psrenderer.allowRoll = Utils.ParseBool (node, "allowRoll", psrenderer.allowRoll);
			psrenderer.cameraVelocityScale = Utils.ParseFloat (node, "cameraVelocityScale", psrenderer.cameraVelocityScale);
			psrenderer.enableGPUInstancing = Utils.ParseBool (node, "enableGPUInstancing", psrenderer.enableGPUInstancing);
			psrenderer.flip = Utils.ParseVector3 (node, "flip", psrenderer.flip);
			psrenderer.lengthScale = Utils.ParseFloat (node, "lengthScale", psrenderer.lengthScale);
			psrenderer.maskInteraction = ParseEnum<SpriteMaskInteraction> (node, "maskInteraction", psrenderer.maskInteraction);
			psrenderer.maxParticleSize = Utils.ParseFloat (node, "maxParticleSize", psrenderer.maxParticleSize);
			//FIXME psrenderer.mesh = Utils.ParseBool (node, "mesh", psrenderer.mesh);
			psrenderer.minParticleSize = Utils.ParseFloat (node, "minParticleSize", psrenderer.minParticleSize);
			psrenderer.normalDirection = Utils.ParseFloat (node, "normalDirection", psrenderer.normalDirection);
			psrenderer.pivot = Utils.ParseVector3 (node, "pivot", psrenderer.pivot);
			psrenderer.renderMode = ParseEnum<ParticleSystemRenderMode> (node, "renderMode", psrenderer.renderMode);
			psrenderer.shadowBias = Utils.ParseFloat (node, "shadowBias", psrenderer.shadowBias);
			psrenderer.sortingFudge = Utils.ParseFloat (node, "sortingFudge", psrenderer.sortingFudge);
			psrenderer.sortMode = ParseEnum<ParticleSystemSortMode> (node, "sortMode", psrenderer.sortMode);
			psrenderer.trailMaterial = ParseMaterial (node, "trailMaterial", psrenderer.trailMaterial);
			psrenderer.velocityScale = Utils.ParseFloat (node, "velocityScale", psrenderer.velocityScale);

			psrenderer.material = ParseMaterial (node, "material", psrenderer.material);
			psrenderer.receiveShadows = Utils.ParseBool (node, "receiveShadows", psrenderer.receiveShadows);
			psrenderer.shadowCastingMode = ParseEnum<ShadowCastingMode> (node, "shadowCastingMode", psrenderer.shadowCastingMode);
			psrenderer.shadowBias = Utils.ParseFloat (node, "shadowBias", psrenderer.shadowBias);

			if (node.HasValue ("vertexStreams")) {
				var values = Utils.ParseArray (node.GetValue ("vertexStreams"));
				var vertexStreams = new List<ParticleSystemVertexStream> ();
				foreach (var v in values) {
					vertexStreams.Add (v.ToEnum<ParticleSystemVertexStream> (ParticleSystemVertexStream.Position));
				}
				psrenderer.SetActiveVertexStreams (vertexStreams);
			}
		}

		public void Load (ConfigNode node)
		{
			psystem.Stop ();
			if (node.HasValue ("useAutoRandomSeed")) {
				bool useAutoRandomSeed;
				bool.TryParse (node.GetValue ("useAutoRandomSeed"), out useAutoRandomSeed);
				psystem.useAutoRandomSeed = useAutoRandomSeed;
			}
			if (node.HasValue ("randomSeed")) {
				uint randomSeed;
				uint.TryParse (node.GetValue ("randomSeed"), out randomSeed);
				psystem.randomSeed = randomSeed;
			}
			if (node.HasNode ("main")) {
				ParseMain (node.GetNode ("main"));
			}
			if (node.HasNode ("collision")) {
				ParseCollision (node.GetNode ("collision"));
			}
			if (node.HasNode ("colorBySpeed")) {
				ParseColorBySpeed (node.GetNode ("colorBySpeed"));
			}
			if (node.HasNode ("colorOverLifetime")) {
				ParseColorOverLifetime (node.GetNode ("colorOverLifetime"));
			}
			//if (node.HasNode ("customData")) {
			//	ParseCustomData (node.GetNode ("customData"));
			//}
			if (node.HasNode ("emission")) {
				ParseEmission (node.GetNode ("emission"));
			}
			if (node.HasNode ("externalForces")) {
				ParseExternalForces (node.GetNode ("externalForces"));
			}
			if (node.HasNode ("forceOverLifetime")) {
				ParseForceOverLifetime (node.GetNode ("forceOverLifetime"));
			}
			if (node.HasNode ("inheritVelocity")) {
				ParseInheritVelocity (node.GetNode ("inheritVelocity"));
			}
			//if (node.HasNode ("lights")) {
			//	ParseLights (node.GetNode ("lights"));
			//}
			if (node.HasNode ("limitVelocityOverLifetime")) {
				ParseLimitVelocityOverLifetime (node.GetNode ("limitVelocityOverLifetime"));
			}
			//if (node.HasNode ("noise")) {
			//	ParseNoise (node.GetNode ("noise"));
			//}
			if (node.HasNode ("rotationBySpeed")) {
				ParseRotationBySpeed (node.GetNode ("rotationBySpeed"));
			}
			if (node.HasNode ("rotationOverLifetime")) {
				ParseRotationOverLifetime (node.GetNode ("rotationOverLifetime"));
			}
			if (node.HasNode ("shape")) {
				ParseShape (node.GetNode ("shape"));
			}
			if (node.HasNode ("sizeBySpeed")) {
				ParseSizeBySpeed (node.GetNode ("sizeBySpeed"));
			}
			if (node.HasNode ("sizeOverLifetime")) {
				ParseSizeOverLifetime (node.GetNode ("sizeOverLifetime"));
			}
			//if (node.HasNode ("subEmitters")) {
			//	ParseSubEmitters (node.GetNode ("subEmitters"));
			//}
			//if (node.HasNode ("textureSheetAnimation")) {
			//	ParseTextureSheetAnimation (node.GetNode ("textureSheetAnimation"));
			//}
			if (node.HasNode ("trails")) {
				ParseTrails (node.GetNode ("trails"));
			}
			if (node.HasNode ("trigger")) {
				ParseTrigger (node.GetNode ("trigger"));
			}
			if (node.HasNode ("velocityOverLifetime")) {
				ParseVelocityOverLifetime (node.GetNode ("velocityOverLifetime"));
			}
			if (node.HasNode ("renderer")) {
				ParseRenderer (node.GetNode ("renderer"));
			}
		}
	}

}
