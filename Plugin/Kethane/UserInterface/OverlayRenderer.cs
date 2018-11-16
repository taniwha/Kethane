using GeodesicGrid;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

using Kethane.ShaderLoader;

namespace Kethane.UserInterface
{
    internal class OverlayRenderer : MonoBehaviour
    {
        private Mesh mesh;

        private int gridLevel = 0;
        private Func<Cell, float> heightMap = c => 1;
        private float radiusMultiplier = 1;
        private Transform target = null;

		private Vector3 []vertices;
		private Color32 []colors32;
		private int []triangles;
		private bool colorsDirty;

        public bool IsVisible
        {
            get
            {
                var renderer = gameObject.GetComponent<MeshRenderer>();
                if (renderer == null) { return false; }
                return renderer.enabled;
            }
            set
            {
                var renderer = gameObject.GetComponent<MeshRenderer>();
                if (renderer == null) { throw new InvalidOperationException("OverlayRenderer has not started"); }
                renderer.enabled = value;
            }
        }

        protected void Awake()
        {
            setUpComponents();
			updateArrays();
            updateTriangles();
            updateVertices();
            updateTarget();
        }

		protected void Update ()
		{
			if (colorsDirty) {
				colorsDirty = false;
				mesh.colors32 = colors32;
			}
		}

        #region Configuration setters

        public void SetGridLevel(int gridLevel)
        {
            SetGridLevelAndHeightMap(gridLevel, heightMap);
        }

        public void SetHeightMap(Func<Cell, float> heightMap)
        {
            SetGridLevelAndHeightMap(gridLevel, heightMap);
        }

        public void SetGridLevelAndHeightMap(int gridLevel, Func<Cell, float> heightMap)
        {
            if (gridLevel < 0) { throw new ArgumentOutOfRangeException("gridLevel"); }
            if (heightMap == null) { throw new ArgumentNullException("heightMap"); }

            if (gridLevel != this.gridLevel)
            {
                this.gridLevel = gridLevel;
				updateArrays();
                updateTriangles();
            }
            else
            {
                if (heightMap == this.heightMap) { return; }
            }

            this.heightMap = heightMap;
            updateVertices();
        }

        public void SetRadiusMultiplier(float radiusMultiplier)
        {
            if (radiusMultiplier < 0) { throw new ArgumentOutOfRangeException("radiusMultiplier"); }
            if (radiusMultiplier != this.radiusMultiplier)
            {
                this.radiusMultiplier = radiusMultiplier;
                updateScale();
            }
        }

        public void SetTarget(Transform target)
        {
            if (target != this.target)
            {
                this.target = target;
                updateTarget();
            }
        }

        #endregion

        #region Cell color setters

        public void SetCellColor(Cell cell, Color32 color)
        {
            setCellColor(cell, color);
        }

        public void SetCellColors(IDictionary<Cell, Color32> assignments)
        {
            setCellColors(assignments);
        }

        public void SetCellColors(CellMap<Color32> assignments)
        {
            setCellColors(assignments);
        }

        private void setCellColors(IEnumerable<KeyValuePair<Cell, Color32>> assignments)
        {
            foreach (var assignment in assignments)
            {
                setCellColor(assignment.Key, assignment.Value);
            }
        }

        private void setCellColor(Cell cell, Color32 color)
        {
            var idx = cell.Index * 6;
            for (var i = idx; i < idx + 6; i++)
            {
                colors32[i] = color;
            }
			colorsDirty = true;
        }

        #endregion

        private void setUpComponents()
        {
            mesh = gameObject.AddComponent<MeshFilter>().mesh;
            var renderer = gameObject.AddComponent<MeshRenderer>();

            renderer.enabled = false;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;

			var shader = KethaneShaderLoader.FindShader("Kethane/AlphaUnlitVertexColored");
			if (shader != null) {
				var material = new Material(shader);

				var color = Color.white;
				color.a = 0.4f;
				material.color = color;

				renderer.material = material;
			}
        }

		private void updateArrays()
		{
            mesh.Clear();
			uint faces = Cell.CountAtLevel(gridLevel);
			vertices = new Vector3[faces * 6];
			colors32 = new Color32[vertices.Length];
			triangles = new int[3 * (4 * (faces - 12) + 5 * 12)];
			mesh.vertices = vertices;
			mesh.colors32 = colors32;
		}

        private void updateTriangles()
        {
			int tri = 0;
            foreach (var cell in Cell.AtLevel(gridLevel))
            {
                var t = (int)cell.Index * 6;
                if (cell.IsPentagon)
                {
                    for (var i = 0; i < 5; i++, tri += 3)
                    {
						triangles[tri + 0] = t + 1 + i;
						triangles[tri + 1] = t + 1 + (i + 1) % 5;
						triangles[tri + 2] = t;
                    }
                }
                else
                {
                    for (var i = 0; i < 3; i++, tri += 3)
					{
						triangles[tri + 0] = t + 0 + i * 2;
						triangles[tri + 1] = t + 1 + i * 2;
						triangles[tri + 2] = t + (2 + i * 2) % 6;
					}
					triangles[tri + 0] = t + 0;
					triangles[tri + 1] = t + 2;
					triangles[tri + 2] = t + 4;
					tri += 3;
                }
            }

            mesh.triangles = triangles;
        }

        private void updateVertices()
        {
			int vert = 0;

            foreach (var cell in Cell.AtLevel(gridLevel))
            {
                var center = cell.Position * heightMap(cell);

                if (cell.IsPentagon)
                {
                    vertices[vert++] = center;
                }

                var blend = 0.08f;
                center *= blend;

                foreach (var vertex in cell.GetVertices(gridLevel, heightMap))
                {
                    vertices[vert++] = center + vertex * (1 - blend);
                }
            }

            mesh.vertices = vertices;
            mesh.RecalculateBounds();
        }

        private void updateTarget()
        {
            if (target != null)
            {
                gameObject.layer = target.gameObject.layer;
            }
            gameObject.transform.parent = target;
            gameObject.transform.localPosition = Vector3.zero;
            gameObject.transform.localRotation = Quaternion.identity;
            updateScale();
        }

        private void updateScale()
        {
            gameObject.transform.localScale = Vector3.one * 1000 * radiusMultiplier;
        }
    }
}
