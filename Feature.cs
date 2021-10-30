using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.Specialized;

using UnityEngine;

using GIS_FW;
using GIS_FW.DATA;
using GIS_FW.TERRAIN;
using GIS_FW.DATA.VECTOR;

namespace GIS_FW
{
    namespace DATA
    {
        namespace VECTOR
        {
            public class Feature
            {
                public String name;
                public String admin;
                public GeodeticExtent extent;
                public GIS_FW.DATA.VECTOR.RectangleD boundingbox;
                public GameObject text = null;// = new GameObject();


                public Geometry geometry;
                public StringDictionary Attributes;
                public Dictionary<String, Layer> subLayers = new Dictionary<String, Layer>();

                public Feature(String Name)
                {
                    name = Name;
                }
                
                public void SetActive(bool what)
                {
                    geometry.SetActive(what);
                }

                public Feature(String Name, String Admin) {
                    name = Name;
                    admin = Admin;
                }

                public void Load(String path, Dictionary<String, Layer> SubLayers, Shape shape, bool fill)
                {
                    Attributes = shape._metadata;
                    ReadAttributes(shape);
					if(name == "Sri Lanka" || name == "Mongolia" || name == "Yemen" ) geometry = new Geometry(name, true);
					else geometry = new Geometry(name, false); 
					geometry.Load(shape);
                    if(SubLayers.Count > 0) LoadSubLayers(SubLayers, fill);

                    if (shape.Type == ShapeType.Point) return;
                    else if (shape.Type == ShapeType.PolyLine)
                    {
                        boundingbox = (GIS_FW.DATA.VECTOR.RectangleD)((ShapePolyLine)shape).BoundingBox;

                    }
                    else if (shape.Type == ShapeType.Polygon)
                    {
                        boundingbox = (GIS_FW.DATA.VECTOR.RectangleD)((ShapePolygon)shape).BoundingBox;
                    }
                }

                public void LoadSubLayers(Dictionary<String, Layer> SubLayers, bool fill)
                {
                    foreach (Layer sublayer in SubLayers.Values)
                    {
                        Layer layer = new Layer(sublayer.Name, sublayer.extent);
                        foreach (Feature feature in sublayer.Features.Values)
                        {
                            if (feature.name == "Feature") layer.Features.Add(feature.name, feature);
                            else if (feature.admin == name) layer.Features.Add(feature.name, feature);
                        }
                        foreach (Feature feature in layer.Features.Values)
                            sublayer.Features.Remove(feature.name);

                        subLayers.Add(layer.Name, layer);
                    }
                }
                
                public void Import(String path)
                {
                    String fpath = Path.Combine(path, "Features");
                    ImportAttributes(fpath);
                    ImportGeometry(fpath);
                    ImportSubLayers(path);
                }

                public void Export(String path)
                {
                    String fpath = Path.Combine(path, "Features");
                    if (!Directory.Exists(path)) Directory.CreateDirectory(fpath);
                    ExportAttributes(fpath);
                    geometry.Export(fpath, name);
                    ExportSubLayers(path);
                }

                void ExportAttributes(String file)
                {
                    file = Path.Combine(file, "Attributes");
                    if (!Directory.Exists(file)) Directory.CreateDirectory(file);
                    file = Path.Combine(file, name);
                    using (FileStream fs = File.OpenWrite(file+".txt"))
                    using (BinaryWriter writer = new BinaryWriter(fs))
                    {
                        writer.Write(Attributes.Count);
                        foreach (string key in Attributes.Keys)
                        {
                            writer.Write(key);
                            writer.Write(Attributes[key]);
                        }
                    }
                }

                public void ImportGeometry(String path)
                {
                    geometry = new Geometry(name, false);
                    geometry.Import(path);
                }

                public void ImportSubLayers(String path)
                {
                    path = Path.Combine(path, "SubLayers");
                    if (!Directory.Exists(path)) return;
                    path = Path.Combine(path, name);
                    if (!Directory.Exists(path)) return;
                    String[] layers = Directory.GetDirectories(path);
                    foreach (String layer_path in layers)
                    {
                        if (layer_path.Contains(".meta")) continue;
                        Layer layer = new Layer(Path.GetFileName(layer_path), extent);
                        layer.Import(layer_path);
                        subLayers.Add(layer.Name, layer);
                    }
                }

                public void ImportAttributes(string file)
                {
                    file = Path.Combine(file, "Attributes");
                    if (!Directory.Exists(file)) return;
                    file = Path.Combine(file, name);
                    Attributes = new StringDictionary();
                    using (FileStream fs = File.OpenRead(file+".txt"))
                    using (BinaryReader reader = new BinaryReader(fs))
                    {
                        int count = reader.ReadInt32();
                        for (int i = 0; i < count; i++)
                        {
                            string key = reader.ReadString();
                            string value = reader.ReadString();
                            Attributes[key] = value;
                        }
                    }
                }

                public void ExportSubLayers(String path)
                {
                    path = Path.Combine(path, "SubLayers");
                    path = Path.Combine(path, name);
                    foreach (Layer layer in subLayers.Values)
                        layer.Export(path);
                }
                
                public void RenderSubLayers()
                {
                    foreach (Layer layer in subLayers.Values) 
                        layer.Render();
                }

				public void UpdateSubLayers()
				{
					foreach (Layer layer in subLayers.Values) 
						layer.Update();
				}

				public void Update()
				{
					RenderText();
					geometry.Update();
					UpdateSubLayers();
				}
                
                public void ReadAttributes(Shape shape)
                {
                    if (name == "shapeno") name = shape.RecordNumber.ToString();
                    else
                    {
                        name = Attributes[name].Trim();
                        if (name == "") name = shape.RecordNumber.ToString();
                    }

                    if (admin != "layer")
                    {
                        if (admin == "shapeno") admin = shape.RecordNumber.ToString();
                        else
                        {
                            admin = Attributes[admin].Trim();
                            if (admin == "") admin = shape.RecordNumber.ToString();
                        }
                    }
                }
            }
        }
    }
}
































