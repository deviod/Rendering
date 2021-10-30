using System;
using System.IO;
using System.Collections.Generic;

using GIS_FW;
using GIS_FW.DATA;
using GIS_FW.TERRAIN;
using GIS_FW.DATA.VECTOR;

using UnityEngine;
using GIS_FW.Shapes;

namespace GIS_FW
{
    namespace DATA
    {
        namespace VECTOR
        {
            public class Level
            {
                public int No;
                public GeodeticExtent extent;
                public Dictionary<String, Layer> Layers = new Dictionary<String, Layer>();

                public Level(int no, GeodeticExtent _extent)
                {
                    No = no;
                    extent = _extent;
                }

                public void SetActive(String layer_name, String feature_name, bool what)
                {
                    Layers[layer_name].SetActive(feature_name, what);
                }

                public void SetActive(String layer_name, bool what)
                {
                    Layers[layer_name].SetActive(what);
                }

                public void SetActive(bool what)
                {
                    foreach (Layer layer in Layers.Values) layer.SetActive(what);
                }

                public void Load(String path, String name, bool fill)
                {
                    Layer layer = new Layer(name, extent);
                    layer.Load(Path.Combine(path, No.ToString()), fill);
                    Layers.Add(name, layer);
                }

                public void Load(String path, List<String> names, bool fill)
                {
                    foreach (String name in names)
                        Load(path, name, fill);
                }

                public void Export(String path)
                {
                    foreach (Layer layer in Layers.Values)
                        layer.Export(Path.Combine(path, No.ToString()));
                }

                public void Import(String path)
                {
                    String[] layers = Directory.GetDirectories(path);
                    foreach (String layer_path in layers)
                    {
                        if (layer_path.Contains(".meta")) continue;
                        Layer layer = new Layer(Path.GetFileName(layer_path), extent);
                        layer.Import(layer_path);
                        Layers.Add(layer.Name, layer);
                    }
                }

                
                public void Import(String Path, int no, bool fill)
                {
                    Level level = new Level(no, extent);
                    level.Import(Path + Name, fill);
                    Levels.Add(no, level);
                }

                public void Import(String Path, bool fill)
                {
                    for (int no = 0; no < nLevels; no++)
                        Import(Path, no, fill);
                }
                
                public void Render()
                {
                    foreach (Layer layer in Layers.Values)
                        layer.Render();
                }
                
				public void Update()
				{
					foreach (Layer layer in Layers.Values)
						layer.Update();
				}
					
                public void Unload(int no)
                {
                   Layers.Remove(no);
                }

                public void Unload()
                {
                    Layers.RemoveAll();
                }
            }
        }
    }
}