using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using KSPAssets.Loaders;

namespace Kethane.ShaderLoader
{
    [KSPAddon(KSPAddon.Startup.Instantly, true)]
    public class KethaneShaderLoader: MonoBehaviour
    {
		const string gamedata = "GameData/Kethane/";
        static Dictionary<string, Shader> shaderDictionary = null;
        public static bool loaded = false;

        private void Start()
        {
            LoadShaders();
        }

        private void LoadShaders()
        {
			string kethane_bundle = "kethane.ksp";

            if (shaderDictionary == null) {
                shaderDictionary = new Dictionary<string, Shader>();
				string root = KSPUtil.ApplicationRootPath;
				string path = root + gamedata + kethane_bundle;

				var bundle = AssetBundle.LoadFromFile(path);
				if (!bundle) {
					Debug.LogFormat("[Kethane] Could not load {0}",
									kethane_bundle);
					return;
				}

				Shader[] shaders = bundle.LoadAllAssets<Shader>();

				foreach (Shader shader in shaders) {
					Debug.LogFormat ("[Kethane] Shader {0} loaded",
									 shader.name);
					shaderDictionary[shader.name] = shader;
				}
				bundle.Unload(false);

                loaded = true;
            }
        }

        public static Shader FindShader(string name)
        {
            if (shaderDictionary == null) {
                Debug.Log("[Kethane] Trying to find shader before assets loaded");
                return null;
            }
            if (shaderDictionary.ContainsKey(name))
            {
                return shaderDictionary[name];
            }
            KSPLog.print("[Kethane] Could not find shader " + name);
            return null;
        }
    }
}
