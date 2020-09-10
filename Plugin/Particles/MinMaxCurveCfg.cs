using System;
using UnityEngine;

namespace KethaneParticles
{

	public class MinMaxCurveCfg
	{
		public ParticleSystem.MinMaxCurve curve { get; private set; }

		public MinMaxCurveCfg ()
		{
			curve = new ParticleSystem.MinMaxCurve (0);
		}

		AnimationCurve ParseCurve (ConfigNode node)
		{
			var keys = node.GetValues("key");
			var frames = new Keyframe[keys.Length];
			for (int i = 0; i < keys.Length; i++) {
				var values = Utils.ParseFloatArray (keys[i]);
				if (values.Length >= 2) {
					frames[i] = new Keyframe (values[0], values[1]);
				} else {
					Debug.Log ($"[MinMaxCurveCfg] key {i}: need 2 or 4 values: {values}");
					frames[i] = new Keyframe (0, 0);
				}
				if (values.Length >= 4) {
					frames[i].inTangent = values[2];
					frames[i].outTangent = values[3];
					if (values.Length > 4) {
						Debug.Log ($"[MinMaxCurveCfg] key {i}: ignorning extra values: {values}");
					}
				} else if (values.Length == 3) {
					Debug.Log ($"[MinMaxCurveCfg] key {i}: ignorning extra value (missing out-tangent value?): {values}");
				}
			}
			return new AnimationCurve (frames);
		}

		public void Load (ConfigNode node)
		{
			float scalar = 1;
			if (node.HasValue ("scalar")) {
				if (!float.TryParse (node.GetValue ("scalar"), out scalar)) {
					scalar = 1;
				}
			}
			if (node.HasNode ("curve")) {
				var curve = ParseCurve (node.GetNode ("curve"));
				if (curve != null) {
					this.curve = new ParticleSystem.MinMaxCurve (scalar, curve);
				} else {
					Debug.Log ("[MinMaxCurveCfg] bogus curve");
				}
			} else if (node.HasNode ("minCurve") && node.HasNode ("maxCurve")) {
				var minCurve = ParseCurve (node.GetNode ("minCurve"));
				var maxCurve = ParseCurve (node.GetNode ("maxCurve"));
				if (minCurve != null && maxCurve != null) {
					curve = new ParticleSystem.MinMaxCurve (scalar, minCurve, maxCurve);
				} else {
					Debug.Log ("[MinMaxCurveCfg] bogus minCurve or maxCurve");
				}
			} else if (node.HasValue ("constant")) {
				var constant = node.GetValue ("constant");
				var values = Utils.ParseFloatArray (constant);
				if (values.Length == 1) {
					curve = new ParticleSystem.MinMaxCurve (values[0]);
				} else if (values.Length >= 2) {
					curve = new ParticleSystem.MinMaxCurve (values[0], values[1]);
					if (values.Length > 2) {
						Debug.Log ($"[MinMaxCurveCfg] ignoring excess values: {constant}");
					}
				} else {
					Debug.Log ($"[MinMaxCurveCfg] need 1 or two values: {constant}");
				}
			} else {
				Debug.Log ($"[MinMaxCurveCfg] need one of constant (value), curve (node), or minCurve and maxCurve (node pair)");
			}
		}
	}

}
