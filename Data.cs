
using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

using GIS_FW;
using GIS_FW.DATA;
using GIS_FW.DATA.VECTOR;

using GIS_FW.TERRAIN;
using GIS_FW.Shapes;

namespace GIS_FW
{
    namespace DATA
    {
        namespace VECTOR
        {
            public class Data
            {
                public String Name;
                public int nLevels;
                public String basepath;
                public GeodeticExtent extent;
                public Dictionary<int, Level> Levels = new Dictionary<int, Level>();

                public Data(String path, String name, GeodeticExtent _extent, int nlevels)
                {
                    Name = name;
                    basepath = path;
                    extent = _extent;
                    nLevels = nlevels;
                }

                public void SetActive(int level_no, String layer_name, String[] feature_names, bool what)
                {
                    foreach (String feature_name in feature_names)
                        Levels[level_no].SetActive(layer_name, feature_name, what);
                }

                public void SetActive(int level_no, String layer_name, String feature_name, bool what)
                {
                    Levels[level_no].SetActive(layer_name, feature_name, what);
                }

                public void SetActive(int level_no, bool what)
                {
                    Levels[level_no].SetActive(what);
                }

                public void SetActive(bool what)
                {
                    foreach (Level level in Levels.Values) level.SetActive(what);
                }

                public void Load(int no, String layer, bool fill)
                {
                    Level level = GetLevel(no);
                   level.Load(basepath, layer, fill);
                }

                public void Load(int no, List<String> layers, bool fill)
                {
                    Level level = GetLevel(no);
                    level.Load(basepath, layers, fill);
                }

                public void Load(String layer, bool fill)
                {
                    for (int i = 0; i < nLevels; i++)
                    {
                        Level level = GetLevel(i);
                        level.Load(basepath, layer, fill);
                    }
                }

                public void Load(List<String> layers, bool fill)
                {
                    for (int i = 0; i < nLevels; i++)
                    {
                        Level level = GetLevel(i);
                        level.Load(basepath, layers, fill);
                    }
                }

                public Level GetLevel(int no)
                {
                    Level level = null;
                    if (Levels.ContainsKey(no)) level = Levels[no];
                    else
                    {
                        level = new Level(no, extent);
                        Levels.Add(no, level);
                    }
                    return level;
                }

                public bool Import(String path)
                {
                    basepath = path;
                    String[] levels = Directory.GetDirectories(path);
                    foreach (String path_level in levels)
                    {
                        if (path_level.Contains(".meta")) continue;
                        Level level = new Level(Convert.ToInt32(Path.GetFileName(path_level)), extent);// GetLevel(Convert.ToInt32(Path.GetFileName(path_level)));
                        level.Import(path_level);
                        Levels.Add(level.No, level);
                        nLevels++;
                    }
                    return true;
                }

                public void Import(List<String> layers, int nlevels, int level, bool fill)
                {
                    String[] levels = Directory.GetDirectories(basepath);
                    foreach (String name in layers)
                    {
                        Layer layer = new Layer(name, extent, nlevels);
                        layer.Import(basepath, level, fill);
                        Layers.Add(name, layer);
                    }
                }

                public void Import(List<String> layers, int nlevels, int level, bool fill)
                {
                    foreach (String name in layers)
                    {
                        Layer layer = new Layer(name, extent, nlevels);
                        layer.Import(basepath, level, fill);
                        Layers.Add(name, layer);
                    }
                }

                public void Import(List<String> layers, int nlevels, bool fill)
                {
                    foreach (String name in layers)
                    {
                        Layer layer = new Layer(name, extent, nlevels);
                        layer.Import(basepath, fill);
                        Layers.Add(name, layer);
                    }
                }
                
                public void Export(String Path)
                {
                    foreach (Level level in Levels.Values)
                        level.Export(Path);
                }

                public void UnloadLevel(String name, int level)
                {
                    Levels.Remove(name);
                }

                public void UnloadLevels(List<String> names)
                {
                    foreach (String name in names)
                        UnloadLevel(name);
                }

                public void UnloadLevel(String name)
                {
                    Levels.Remove(name);
                }

                public void UnloadLevels(List<String> names, int level)
                {
                    foreach (String name in names)
                        UnloadLevel(name, level);
                }

                public void Render()
                {
                    foreach (Level level in Levels.Values)
                        level.Render();
                }

				public void Update()
				{
					foreach (Level level in Levels.Values)
						level.Update();
				}
            }
        }
    }
}
































