using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Kethane.Utilities.Kopernicus
{
	public class Templates {
		static Type KopernicusTemplates_class;
		static FieldInfo kop_menuBody;

		public static String MenuBody
		{
			get { return (String)kop_menuBody.GetValue(null); }
		}

		internal static bool Initialize (Assembly kopAsm)
		{
			var types = kopAsm.GetTypes ();

			foreach (var t in types) {
				if (t.Name == "Templates") {
					KopernicusTemplates_class = t;
					kop_menuBody = KopernicusTemplates_class.GetField ("MenuBody", BindingFlags.Public | BindingFlags.Static);
					Debug.Log ($"[Kethane] Kopernicus.Templates `{kop_menuBody}'");
					return true;
				}
			}
			return false;
		}
	}

	public class Events {
		static Type KopernicusEvents_class;
		static PropertyInfo kop_oruum;

		public static EventVoid OnRuntimeUtilityUpdateMenu
		{
			get { return (EventVoid)kop_oruum.GetValue(null, null); }
		}

		internal static bool Initialize (Assembly kopAsm)
		{
			var types = kopAsm.GetTypes ();

			foreach (var t in types) {
				if (t.Name == "Events") {
					KopernicusEvents_class = t;
					kop_oruum = KopernicusEvents_class.GetProperty ("OnRuntimeUtilityUpdateMenu", BindingFlags.Public | BindingFlags.Static);
					Debug.Log ($"[Kethane] Kopernicus.Events `{kop_oruum}'");
					return true;
				}
			}
			return false;
		}
	}

	public class KopernicusWrapper {
		static bool inited = false;
		static bool haveKopernicus = false;

		public static bool Initialize ()
		{
			if (!inited) {
				inited = true;
				AssemblyLoader.LoadedAssembly kopAsm = null;

				foreach (var la in AssemblyLoader.loadedAssemblies) {
					if (la.assembly.GetName ().Name.Equals ("Kopernicus", StringComparison.InvariantCultureIgnoreCase)) {
						kopAsm = la;
					}
				}
				if (kopAsm != null) {
					Debug.Log ($"[Kethane] found Kopernicus {kopAsm}");
					Events.Initialize (kopAsm.assembly);
					Templates.Initialize (kopAsm.assembly);
					haveKopernicus = true;
				}
			}
			return haveKopernicus;
		}
	}
}
