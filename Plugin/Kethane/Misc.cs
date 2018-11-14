using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Kethane
{
    internal static class Misc
    {
        #region Parsing utility methods

        public static float Parse(string s, float defaultValue)
        {
            float value;
            if (!float.TryParse(s, out value))
            {
                value = defaultValue;
            }
            return value;
        }

        public static double Parse(string s, double defaultValue)
        {
            double value;
            if (!double.TryParse(s, out value))
            {
                value = defaultValue;
            }
            return value;
        }

        public static int Parse(string s, int defaultValue)
        {
            int value;
            if (!int.TryParse(s, out value))
            {
                value = defaultValue;
            }
            return value;
        }

        public static bool Parse(string s, bool defaultValue)
        {
            bool value;
            if (!bool.TryParse(s, out value))
            {
                value = defaultValue;
            }
            return value;
        }

        public static Vector3 Parse(string s, Vector3 defaultValue)
        {
            try
            {
                return ConfigNode.ParseVector3(s);
            }
            catch
            {
                return defaultValue;
            }
        }

        public static Color32 Parse(string s, Color32 defaultValue)
        {
            if (s == null) { return defaultValue; }
            return ConfigNode.ParseColor32(s);
        }

        static MethodInfo PreFormatConfig = typeof(ConfigNode).GetMethods(BindingFlags.NonPublic | BindingFlags.Static).Where(m => m.Name == "PreFormatConfig" && m.GetParameters().Length == 1).FirstOrDefault();
        static MethodInfo RecurseFormat = typeof(ConfigNode).GetMethods(BindingFlags.NonPublic | BindingFlags.Static).Where(m => m.Name == "RecurseFormat" && m.GetParameters().Length == 1).FirstOrDefault();
        public static ConfigNode Parse(string s)
        {
            var lines = s.Split(new char[]{'\n', '\r'});
            object obj = PreFormatConfig.Invoke(null, new object[] {lines});
            return (ConfigNode) RecurseFormat.Invoke(null, new object[] {obj});
        }

        public static ParticleRenderMode Parse(string s, ParticleRenderMode defaultValue)
        {
            try
            {
                return (ParticleRenderMode)Enum.Parse(typeof(ParticleRenderMode), s);
            }
            catch
            {
                return defaultValue;
            }
        }

        #endregion

        #region Encoding

        public static byte[] FromBase64String(string encoded)
        {
            return Convert.FromBase64String(encoded.Replace('.', '/').Replace('%', '='));
        }

        public static string ToBase64String(byte[] data)
        {
            return Convert.ToBase64String(data).Replace('/', '.').Replace('=', '%');
        }

        #endregion

        // Get true altitude above terrain (from MuMech lib)
        // Also from: http://kerbalspaceprogram.com/forum/index.php?topic=10324.msg161923#msg161923
        public static double getTrueAltitude(this Vessel vessel)
        {
            Vector3 CoM = vessel.CoM;
            Vector3 up = (CoM - vessel.mainBody.position).normalized;
            double altitudeASL = vessel.mainBody.GetAltitude(CoM);
            double altitudeTrue = 0.0;
            RaycastHit sfc;
            if (Physics.Raycast(CoM, -up, out sfc, (float)altitudeASL + 10000.0F, 1 << 15))
                altitudeTrue = sfc.distance;
            else if (vessel.mainBody.pqsController != null)
                altitudeTrue = vessel.mainBody.GetAltitude(CoM) - (vessel.mainBody.pqsController.GetSurfaceHeight(QuaternionD.AngleAxis(vessel.mainBody.GetLongitude(CoM), Vector3d.down) * QuaternionD.AngleAxis(vessel.mainBody.GetLatitude(CoM), Vector3d.forward) * Vector3d.right) - vessel.mainBody.pqsController.radius);
            else
                altitudeTrue = vessel.mainBody.GetAltitude(CoM);
            return altitudeTrue;
        }
    }
}
