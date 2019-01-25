using GeodesicGrid;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

namespace Kethane.Generators
{
    internal class LegacyResourceGenerator : IResourceGenerator
    {
        private GeneratorConfiguration config;

        public LegacyResourceGenerator(ConfigNode node)
        {
            config = new GeneratorConfiguration(node);
        }

        public IBodyResources Load(CelestialBody body, ConfigNode node)
        {
            return new BodyDeposits(config.ForBody(body), node);
        }

        private class GeneratorConfiguration
        {
            public float MinRadius { get; private set; }
            public float MaxRadius { get; private set; }
            public double MinQuantity { get; private set; }
            public double MaxQuantity { get; private set; }
            public int MinVertices { get; private set; }
            public int MaxVertices { get; private set; }
            public float RadiusVariance { get; private set; }
            public int DepositCount { get; private set; }
            public int NumberOfTries { get; private set; }
            public bool CanReplenish { get; private set; }
            public double MinHalfLife { get; private set; }
            public double MaxHalfLife { get; private set; }

            private Dictionary<string, GeneratorConfiguration> bodies = new Dictionary<string, GeneratorConfiguration>();

            public GeneratorConfiguration(ConfigNode node)
            {
                load(node);
                foreach (var bodyNode in node.GetNodes("Body"))
                {
                    var body = (GeneratorConfiguration)this.MemberwiseClone();
                    body.load(bodyNode);
                    bodies[bodyNode.GetValue("name")] = body;
                }
            }

            public GeneratorConfiguration ForBody(CelestialBody body)
            {
                return bodies.ContainsKey(body.name) ? bodies[body.name] : this;
            }

            private void load(ConfigNode node)
            {
                MinRadius = Misc.Parse(node.GetValue("MinRadius"), MinRadius);
                MaxRadius = Misc.Parse(node.GetValue("MaxRadius"), MaxRadius);
                MinQuantity = Misc.Parse(node.GetValue("MinQuantity"), MinQuantity);
                MaxQuantity = Misc.Parse(node.GetValue("MaxQuantity"), MaxQuantity);
                MinVertices = Misc.Parse(node.GetValue("MinVertices"), MinVertices);
                MaxVertices = Misc.Parse(node.GetValue("MaxVertices"), MaxVertices);
                RadiusVariance = Misc.Parse(node.GetValue("RadiusVariance"), RadiusVariance);
                DepositCount = Misc.Parse(node.GetValue("DepositCount"), DepositCount);
                NumberOfTries = Misc.Parse(node.GetValue("NumberOfTries"), NumberOfTries);
                CanReplenish = Misc.Parse(node.GetValue("CanReplenish"), CanReplenish);
                MinHalfLife = Misc.Parse(node.GetValue("MinHalfLife"), MinHalfLife);
                MaxHalfLife = Misc.Parse(node.GetValue("MaxHalfLife"), MaxHalfLife);
            }
        }

        private class BodyDeposits : IBodyResources
        {
            private readonly List<Deposit> deposits;
            private readonly int seed;

            private static System.Random seedGenerator = new System.Random();

            public double MaxQuantity { get; private set; }
            public bool CanReplenish { get; private set; }

            public BodyDeposits(GeneratorConfiguration resource, ConfigNode node)
            {
                if (node == null) { node = new ConfigNode(); }

                this.deposits = new List<Deposit>();
                this.seed = Misc.Parse(node.GetValue("Seed"), seedGenerator.Next());

                var random = new System.Random(seed);

                for (int i = 0; i < resource.DepositCount; i++)
                {
                    float R = random.Range(resource.MinRadius, resource.MaxRadius);
                    for (int j = 0; j < resource.NumberOfTries; j++)
                    {
                        Vector2 Pos = new Vector2(random.Range(R, 360 - R), random.Range(R, 180 - R));
                        var deposit = Deposit.Generate(Pos, R, random, resource);
                        if (!deposits.Any(d => d.Shape.Vertices.Any(v => deposit.Shape.PointInPolygon(new Vector2(v.x, v.y)))) && !deposit.Shape.Vertices.Any(v => deposits.Any(d => d.Shape.PointInPolygon(new Vector2(v.x, v.y)))))
                        {
                            deposits.Add(deposit);
                            break;
                        }
                    }
                }

				if (resource.CanReplenish) {
					// generated separately to allow updating older saves without messing with their deposites due to changes in the RNG sequence.
					for (int i = 0; i < deposits.Count; i++)
					{
						// MinHalfLife and MaxHalfLife are specified in hours
						double halflife = 3600 * random.Range(resource.MinHalfLife, resource.MaxHalfLife);
						deposits[i].Lambda = Math.Log(2) / halflife;
					}
				}

                var depositValues = node.GetValues("Deposit");
                for (int i = 0; i < Math.Min(deposits.Count, depositValues.Length); i++)
                {
					var split = depositValues[i].Split(new []{',', ' '}, StringSplitOptions.RemoveEmptyEntries);
                    deposits[i].Quantity = Misc.Parse(split[0], deposits[i].InitialQuantity);
					if (split.Length >= 2 && resource.CanReplenish)
					{
						deposits[i].LastUT = Misc.Parse(split[1], deposits[i].LastUT);
					}
                }

                MaxQuantity = resource.MaxQuantity;
                CanReplenish = resource.CanReplenish;
            }

            private Deposit getDeposit(Cell cell)
            {
                var pos = cell.Position;
                var lat = (float)(Math.Atan2(pos.y, Math.Sqrt(pos.x * pos.x + pos.z * pos.z)) * 180 / Math.PI);
                var lon = (float)(Math.Atan2(pos.z, pos.x) * 180 / Math.PI);

                var x = lon + 180f;
                var y = 90f - lat;

                return deposits.FirstOrDefault(d => d.Shape.PointInPolygon(new Vector2(x, y)));
            }

            public double? GetQuantity(Cell cell)
            {
                var deposit = getDeposit(cell);
                if (deposit == null) { return null; }
                return deposit.Quantity;
            }

            public double Extract(Cell cell, double amount)
            {
                var deposit = getDeposit(cell);
                if (deposit == null) { return 0; }

                var current = deposit.Quantity;
                var delta = Math.Min(current, Math.Max(0, amount));
                deposit.Quantity = current - delta;
                return delta;
            }

            public ConfigNode Save()
            {
                var node = new ConfigNode();
                node.AddValue("Seed", seed);
                foreach (var deposit in deposits)
                {
					if (CanReplenish) {
						node.AddValue("Deposit", $"{deposit.Quantity:G17},{deposit.LastUT:G17}");
					} else {
						node.AddValue("Deposit", deposit.Quantity);
					}
                }
                return node;
            }
        }

        private class Deposit
        {
            public Polygon Shape;

            public double Quantity { get; set; }
            public double InitialQuantity { get; set; }
            public double Lambda { get; set; }
            public double LastUT { get; set; }

            public Deposit(Polygon shape, double quantity, double initialQuantity)
            {
                Shape = shape;
                Quantity = quantity;
                InitialQuantity = initialQuantity;
				Lambda = 0;
            }

            public static Deposit Generate(Vector2 Pos, float radius, System.Random random, GeneratorConfiguration resource)
            {
                var initialQuantity = random.Range(resource.MinQuantity, resource.MaxQuantity);

                var vertices = new List<Vector2>();
                int vertexCount = random.Next(resource.MinVertices, resource.MaxVertices);
                for (int i = 0; i < vertexCount; i++)
                {
                    float randomRadius = random.Range(resource.RadiusVariance * radius, radius);
                    float angle = 2.0f * (float)Math.PI * ((float)i / (float)vertexCount);
                    float x = Pos.x + randomRadius * (float)Math.Cos(angle);
                    float z = Pos.y - randomRadius * (float)Math.Sin(angle);

                    vertices.Add(new Vector2(x, z));
                }
                var Shape = new Polygon(vertices.ToArray());

                return new Deposit(Shape, initialQuantity, initialQuantity);
            }
        }

        private class Polygon
        {
            private Vector2[] _vertices;

            public Polygon(Vector2[] vertices)
            {
                _vertices = vertices.ToArray();
            }

            public ReadOnlyCollection<Vector2> Vertices
            {
                get { return new ReadOnlyCollection<Vector2>(_vertices); }
            }

            public bool PointInPolygon(Vector2 p)
            {
                bool isInside = false;
                for (int i = 0, j = _vertices.Length - 1; i < _vertices.Length; j = i++)
                {
                    if (((_vertices[i].y > p.y) != (_vertices[j].y > p.y)) &&
                    (p.x < (_vertices[j].x - _vertices[i].x) * (p.y - _vertices[i].y) / (_vertices[j].y - _vertices[i].y) + _vertices[i].x))
                    {
                        isInside = !isInside;
                    }
                }
                return isInside;
            }
        }
    }
}
