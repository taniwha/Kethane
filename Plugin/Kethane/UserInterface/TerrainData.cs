﻿using GeodesicGrid;
using System;
using System.Collections.Generic;

namespace Kethane.UserInterface
{
    public class TerrainData
    {
        private static readonly Dictionary<string, TerrainData> bodies = new Dictionary<string, TerrainData>();

        public static void Clear()
        {
            bodies.Clear();
        }

        public static TerrainData ForBody(CelestialBody body)
        {
            if (body == null) { throw new ArgumentException("Body may not be null"); }
            if (!bodies.ContainsKey(body.name))
            {
                bodies[body.name] = new TerrainData(body);
            }
            return bodies[body.name];
        }

        private readonly CellMap<float> heightRatios;

        private TerrainData(CelestialBody body)
        {
            if (body.pqsController == null) { throw new ArgumentException("Body doesn't have a PQS controller"); }
            heightRatios = new CellMap<float>(KethaneData.GridLevel, c => (float)(body.pqsController.GetSurfaceHeight(c.Position) / body.pqsController.radius));
        }

        public float GetHeightRatio(Cell cell)
        {
            return heightRatios[cell];
        }
    }
}
