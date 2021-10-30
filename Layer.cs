
using System;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;

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
            public class Layer
            {
                public String Name;
                public GeodeticExtent extent;
                public String FeatureNameAttrib;
                public String FeatureAdminAttrib;
                public Dictionary<String, Layer> subLayers = new Dictionary<String, Layer>();
                public Dictionary<String, Feature> Features = new Dictionary<String, Feature>();

                public Layer(String name, GeodeticExtent _extent)
                {
                    Name = name;
                    extent = _extent;
                }

                public void SetActive(String feature_name, bool what)
                {
                    Features[feature_name].SetActive(what);
                }

                public void SetActive(bool what)
                {
                    foreach (Feature feature in Features.Values) feature.SetActive(what); 
                }

                public void Load(String path, bool fill)
                {
                    Dictionary<String, Layer> SubLayers = null;
                    String file = Path.Combine(path, Name);
                    SubLayers = LoadSubLayers(file, fill);
                    file = Path.Combine(file, Name);
                    if (!File.Exists(file + ".shp")) { subLayers = SubLayers; return; }
                    if (!ReadMetadata(file)) { UnityEngine.Debug.Log(path + ".txt loading error"); return; }
                    using (Shapefile shapefile = new Shapefile(file))
                    {
                        foreach (Shape shape in shapefile)
                        {
                            Feature feature = new Feature(FeatureNameAttrib, FeatureAdminAttrib);
                            feature.Load(file, SubLayers, shape, fill);
                            if (!Features.ContainsKey(feature.name))
                                Features.Add(feature.name, feature);
                        }
                    }
                }

                public Dictionary<String, Layer> LoadSubLayers(String path, bool fill)
                {   
                    String[] layers = Directory.GetDirectories(path);
                    Dictionary<String, Layer> SubLayers = new Dictionary<String, Layer>();
                    foreach (String name in layers)
                    {
                        Layer layer = new Layer(Path.GetFileName(name), extent);
                        layer.Load(path, fill);
                        SubLayers.Add(layer.Name, layer);
                    }
                    return SubLayers;
                }

                public void Import(String path)
                {

                    String fpath = path + "\\Features\\Attributes";
                    if (Directory.Exists(fpath))
                    {
                        string[] fileEntries = Directory.GetFiles(fpath);
                        foreach (string fileName in fileEntries)
                        {
                            if (fileName.Contains(".meta")) continue;
                            int assetPathIndex = path.IndexOf("Assets");
                            string localPath = path.Substring(assetPathIndex);
                            Feature feature = new Feature(Path.GetFileNameWithoutExtension(fileName));
                            feature.Import(localPath);
                            Features.Add(feature.name, feature);
                        }
                    }
                    else 
                    {
                        String[] layers = Directory.GetDirectories(path);
                        foreach (String layer_path in layers)
                        {
                            if (layer_path.Contains(".meta")) continue;
                            Layer layer = new Layer(Path.GetFileName(layer_path), extent);
                            layer.Import(layer_path);
                            subLayers.Add(layer.Name, layer);
                        }
                    }
 
                }

                public void Export(String path)
                {
                    foreach (Feature feature in Features.Values)
                        feature.Export(Path.Combine(path, Name));
                    foreach (Layer layer in subLayers.Values)
                        layer.Export(Path.Combine(path, Name));
                }

                public void Render()
                {
                    foreach (Layer layer in subLayers.Values) layer.Render();
                    foreach (Feature feature in Features.Values) feature.Render();
                }

				public void Update()
				{
					foreach (Layer layer in subLayers.Values) layer.Update();
					foreach (Feature feature in Features.Values) feature.Update();
				}

                public bool ReadMetadata(String path)
                {
                    String line;
                    path += ".txt";
                    System.IO.StreamReader file = new System.IO.StreamReader(path);
                    while ((line = file.ReadLine()) != null)
                    {
                        if (line.Contains("name="))
                            FeatureNameAttrib = line.Substring(line.IndexOfAny("=".ToCharArray()) + 1);
                        else if (line.Contains("admin="))
                        {
                            FeatureAdminAttrib = line.Substring(line.IndexOfAny("=".ToCharArray()) + 1);
                            file.Close();
                            return true;
                        }
                    }
                    return false;
                }
            }
        }
    }
}
































