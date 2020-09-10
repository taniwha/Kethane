using System;
using UnityEngine;

namespace KethaneParticles
{

	public class MinMaxGradientCfg
	{
		public ParticleSystem.MinMaxGradient gradient { get; private set; }

		public MinMaxGradientCfg ()
		{
			gradient = new ParticleSystem.MinMaxGradient (Color.white);
		}

		Color ParseColor (string str)
		{
			var values = Utils.ParseFloatArray (str);

			if (values.Length == 3) {
				return new Color (values[0], values[1], values[2]);
			} else if (values.Length >= 4) {
				return new Color (values[0], values[1], values[2], values[3]);
			}
			return Color.black;
		}

		public void Load (ConfigNode node)
		{
			if (node.HasNode ("color")) {
				var color = node.GetNode ("color");
				var colorMin = color.GetValue ("min");
				var colorMax = color.GetValue ("max");
				gradient = new ParticleSystem.MinMaxGradient (ParseColor (colorMin), ParseColor (colorMax));
			} else if (node.HasValue ("color")) {
				var color = ParseColor (node.GetValue ("color"));
				gradient = new ParticleSystem.MinMaxGradient (color);
			} else if (node.HasNode ("gradientMin") && node.HasNode ("gradientMax")) {
				var gradientMin = new GradientCfg ();
				var gradientMax = new GradientCfg ();
				gradientMin.Load (node.GetNode ("gradientMin"));
				gradientMax.Load (node.GetNode ("gradientMax"));
				gradient = new ParticleSystem.MinMaxGradient (gradientMin.gradient, gradientMax.gradient);
			} else if (node.HasNode ("gradient")) {
				var gradient = new GradientCfg ();
				gradient.Load (node.GetNode ("gradient"));
				this.gradient = new ParticleSystem.MinMaxGradient (gradient.gradient);
			} else {
				Debug.Log ($"[MinMaxGradientCfg] need one of color (value or node), gradient (node), or gradientMin and gradientMax (node pair)");
			}
		}
	}

}
