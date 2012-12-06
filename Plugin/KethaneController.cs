﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Kethane
{
    internal class KethaneController
    {
        #region Static factory

        private static Dictionary<WeakReference<Vessel>, KethaneController> controllers = new Dictionary<WeakReference<Vessel>, KethaneController>();

        public static KethaneController GetInstance(Vessel vessel)
        {
            foreach (var kvp in controllers.ToArray())
            {
                var wr = kvp.Key;
                var v = wr.Target;
                if (v == null)
                {
                    controllers.Remove(wr);
                    RenderingManager.RemoveFromPostDrawQueue(3, kvp.Value.drawGui);
                }
                else if (v == vessel)
                {
                    return controllers[wr];
                }
            }

            var commander = new KethaneController();
            controllers[new WeakReference<Vessel>(vessel)] = commander;
            return commander;
        }

        #endregion

        private KethaneController()
        {
            LoadKethaneDeposits();
            RenderingManager.AddToPostDrawQueue(3, drawGui);
        }

        public Vessel Vessel
        {
            get { return controllers.Single(p => p.Value == this).Key.Target; }
        }

        public static Dictionary<string, KethaneDeposits> PlanetDeposits;

        public static Dictionary<string, Texture2D> PlanetTextures = new Dictionary<string, Texture2D>();

        private long lastSaveFrame = -1;

        public void SetMaps()
        {
            if (FlightGlobals.fetch == null) { return; }
            foreach (CelestialBody body in FlightGlobals.Bodies)
            {
                if (!PlanetTextures.ContainsKey(body.name))
                {
                    PlanetTextures.Add(body.name, new Texture2D(256, 128, TextureFormat.ARGB32, false));
                }
                if (KSP.IO.File.Exists<KethaneController>(body.name + ".png"))
                {
                    PlanetTextures[body.name].LoadImage(KSP.IO.File.ReadAllBytes<KethaneController>(body.name + ".png"));
                }
                else
                {
                    for (int y = 0; y < PlanetTextures[body.name].height; y++)
                        for (int x = 0; x < PlanetTextures[body.name].width; x++)
                            PlanetTextures[body.name].SetPixel(x, y, Color.black);
                    PlanetTextures[body.name].Apply();
                }
            }
        }

        public void SaveAllMaps()
        {
            if (FlightGlobals.fetch == null) { return; }
            foreach (CelestialBody body in FlightGlobals.Bodies)
            {
                if (PlanetTextures.ContainsKey(body.name))
                {
                    var pbytes = PlanetTextures[body.name].EncodeToPNG();
                    KSP.IO.File.WriteAllBytes<KethaneController>(pbytes, body.name + ".png", null);
                }
            }
        }

        public void DrawMap(bool deposit)
        {
            if (Vessel.mainBody != null && PlanetTextures.ContainsKey(Vessel.mainBody.name))
            {
                Texture2D planetTex = PlanetTextures[Vessel.mainBody.name];

                if (this.Vessel != null)
                {
                    int x = Misc.GetXOnMap(Misc.clampDegrees(Vessel.mainBody.GetLongitude(Vessel.transform.position)), planetTex.width);
                    int y = Misc.GetYOnMap(Vessel.mainBody.GetLatitude(Vessel.transform.position), planetTex.height);
                    if (deposit)
                        planetTex.SetPixel(x, y, XKCDColors.Green);
                    else
                        planetTex.SetPixel(x, y, XKCDColors.Grey);
                }

                planetTex.Apply();
            }
        }

        public void SaveKethaneDeposits()
        {
            if (lastSaveFrame == Time.frameCount) { return; }
            lastSaveFrame = Time.frameCount;
            try
            {
                if (PlanetDeposits == null)
                    return;

                byte[] DepositsToSave = KSP.IO.IOUtils.SerializeToBinary(PlanetDeposits);
                int HowManyToSave = DepositsToSave.Length;
                KSP.IO.BinaryWriter Writer = KSP.IO.BinaryWriter.CreateForType<KethaneController>("Deposits.dat");
                Writer.Write(HowManyToSave);
                Writer.Write(DepositsToSave);
                Writer.Close();
            }
            catch (Exception e)
            {
                MonoBehaviour.print("Kethane plugin - deposit save error: " + e);
            }
            SaveAllMaps();
        }

        public void LoadKethaneDeposits()
        {
            if (PlanetDeposits != null) { return; }
            if (KSP.IO.File.Exists<KethaneController>("Deposits.dat"))
            {
                try
                {
                    KSP.IO.BinaryReader Loader = KSP.IO.BinaryReader.CreateForType<KethaneController>("Deposits.dat");
                    int HowManyToLoad = Loader.ReadInt32();
                    byte[] DepositsToLoad = new byte[HowManyToLoad];
                    Loader.Read(DepositsToLoad, 0, HowManyToLoad);
                    Loader.Close();
                    object ObjectToLoad = KSP.IO.IOUtils.DeserializeFromBinary(DepositsToLoad);
                    PlanetDeposits = (Dictionary<string, KethaneDeposits>)ObjectToLoad;
                    return;
                }
                catch (Exception e)
                {
                    MonoBehaviour.print("Kethane plugin - deposit load error: " + e);
                    MonoBehaviour.print("Generating new kethane deposits");
                }
            }
            GenerateKethaneDeposits();
            SetMaps();
        }

        public void GenerateKethaneDeposits()
        {
            if (FlightGlobals.fetch == null) { return; }
            PlanetDeposits = new Dictionary<string, KethaneDeposits>();
            foreach (CelestialBody CBody in FlightGlobals.Bodies)
                PlanetDeposits.Add(CBody.name, new KethaneDeposits(CBody));
            SaveKethaneDeposits();
            foreach (CelestialBody body in FlightGlobals.Bodies)
            {
                if (KSP.IO.File.Exists<KethaneController>(body.name + ".png"))
                    KSP.IO.File.Delete<KethaneController>(body.name + ".png");
            }
        }

        public KethaneDeposit GetDepositUnder()
        {
            if (!PlanetDeposits.ContainsKey(Vessel.mainBody.name)) { return null; }
            KethaneDeposits Deposits = KethaneController.PlanetDeposits[Vessel.mainBody.name];

            double lon = Misc.clampDegrees(Vessel.mainBody.GetLongitude(Vessel.transform.position));
            double lat = Vessel.mainBody.GetLatitude(Vessel.transform.position);

            double x = Math.Round((lon + 180d) * (Deposits.Width / 360d));
            double y = Math.Round(((90d - lat) * (Deposits.Height / 180d)));

            Vector3 PointUnder = new Vector3((float)x, 0, (float)y);

            return Deposits.GetDepositOver(PointUnder);
        }

        public bool ShowDetectorWindow;

        public bool ScanningSound = true;

        public double LastLat, LastLon;

        private Rect DetectorWindowPosition = new Rect(Screen.width * 0.20f, 250, 10, 10);

        private void drawGui()
        {
            if (FlightGlobals.fetch == null) { return; }
            if (FlightGlobals.ActiveVessel != Vessel)
            { return; }

            GUI.skin = HighLogic.Skin;

            if (ShowDetectorWindow)
            {
                DetectorWindowPosition = GUILayout.Window(12358, DetectorWindowPosition, DetectorWindowGUI, "Detecting", GUILayout.MinWidth(300), GUILayout.MaxWidth(300), GUILayout.MinHeight(20));
            }
        }

        private void DetectorWindowGUI(int windowID)
        {
            #region Detector
            GUILayout.BeginVertical();

            if (Vessel.mainBody != null && KethaneController.PlanetTextures.ContainsKey(Vessel.mainBody.name))
            {
                Texture2D planetTex = KethaneController.PlanetTextures[Vessel.mainBody.name];
                GUILayout.Box(planetTex);
                Rect Last = UnityEngine.GUILayoutUtility.GetLastRect();

                float xVar = ((Last.xMin + Last.xMax) / 2) - (planetTex.width / 2) + DetectorWindowPosition.x;
                float yVar = ((Last.yMin + Last.yMax) / 2) - (planetTex.height / 2) + DetectorWindowPosition.y;
                xVar = xVar - UnityEngine.Input.mousePosition.x;
                yVar = (Screen.height - yVar) - UnityEngine.Input.mousePosition.y;

                bool inbound = true;
                if (yVar > planetTex.height || yVar < 0)
                    inbound = false;
                if (-xVar > planetTex.width || -xVar < 0)
                    inbound = false;

                GUILayout.BeginHorizontal();
                GUILayout.Label("Mouse Latitude: ");
                GUILayout.Label(" " + (inbound ? Misc.GetLatOnMap(yVar, planetTex.height).ToString("#0.0") : "-"));
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("Mouse Longitude: ");
                GUILayout.Label(" " + (inbound ? Misc.GetLonOnMap(xVar, planetTex.width).ToString("#0.0") : "-"));
                GUILayout.EndHorizontal();

            }

            if (Vessel.Parts.SelectMany(p => p.Modules.OfType<KethaneDetector>()).Count() > 0)
            {
                GUILayout.BeginHorizontal();
                string BoxLabelAmount = "No reading";
                string BoxLabelDepth = "-";
                var DepositUnder = GetDepositUnder();
                if (DepositUnder != null)
                {
                    BoxLabelAmount = "~" + Math.Round(DepositUnder.Kethane, 1) + " [l]";
                    BoxLabelDepth = "~" + Math.Round(DepositUnder.Depth, 1) + " [m]";
                }
                GUILayout.BeginVertical();
                GUILayout.Label(BoxLabelAmount);
                GUILayout.Label(BoxLabelDepth);
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
                GUILayout.Label("");
                GUILayout.BeginHorizontal();
                GUILayout.Label("Last known latitude: ");
                GUILayout.Label(LastLat.ToString("#0.000"));
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("Last known longitude: ");
                GUILayout.Label(LastLon.ToString("#0.000"));
                GUILayout.EndHorizontal();
                ScanningSound = GUILayout.Toggle(ScanningSound, "Detection sound");
            }
            else
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Kethane detector: ");
                GUILayout.Label("Not found");
                GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();
            GUI.DragWindow(new Rect(0, 0, 300, 60));
            #endregion
        }
    }
}
