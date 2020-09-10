using System;
using UnityEngine;

namespace KethaneParticles
{

	public class GradientCfg
	{
		public Gradient gradient { get; private set; }

		public Color this [int index]
		{
			get {
				Color c = gradient.colorKeys[index].color;
				c.a = gradient.alphaKeys[index].alpha;
				return c;
			}
			set {
				gradient.colorKeys[index].color = value;
				gradient.alphaKeys[index].alpha = value.a;
			}
		}

		public GradientCfg ()
		{
			gradient = new Gradient ();
		}

		public void Load (ConfigNode node)
		{
			var keys = node.GetValues ("key");
			var colors = new GradientColorKey[keys.Length];
			var alphas = new GradientAlphaKey[keys.Length];
			for (int i = 0; i < keys.Length; i++) {
				var vals = Utils.ParseFloatArray (keys[i]);
				if (vals.Length < 4) {
					Debug.Log ($"[GradientCfg] key {i}: need at least 4 values: x, r, g, b[, a]: {vals}");
				} else {
					var c = new Color (vals[1], vals[2], vals[3]);
					float a = 1;
					if (vals.Length > 4) {
						a = vals[4];
					}
					if (vals.Length > 5) {
						Debug.Log ($"[GradientCfg] key {i}: ignoring excess values: {vals}");
					}
					colors[i] = new GradientColorKey (c, vals[0]);
					alphas[i] = new GradientAlphaKey (a, vals[0]);
				}
			}
			gradient.colorKeys = colors;
			gradient.alphaKeys = alphas;
		}
	}

}
