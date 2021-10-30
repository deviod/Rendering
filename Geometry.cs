using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using GIS_FW;
using GIS_FW.DATA;
using GIS_FW.Shapes;
using GIS_FW.TERRAIN;

using GIS_FW.DATA.RASTER;
using GIS_FW.DATA.VECTOR;

namespace GIS_FW
{
    namespace DATA
    {
        namespace VECTOR
        {
            public class Geometry
            {
                bool fill;
                String Name;
                GeodeticExtent extent;
                List<List<Vector3>> list_vertices = new List<List<Vector3>>();
				Dictionary<String, shape> shapes = new Dictionary<String, shape>();
				Dictionary<String, GameObject> gobjs = new Dictionary<String, GameObject>();

                public Geometry(String name, bool _fill)
                {
                    Name = name;
                    fill = _fill;
                }

                public void SetActive(bool what)
                {
                    foreach (GameObject go in gobjs.Values)
                    {
                        go.SetActive(what);
                    }
                }

                public void Load(Shape _shape)
                {
                    int counter = 0;
                    int total_vertices = 0;
					if (_shape.Type == ShapeType.Point) {
						ShapePoint shapePoint = _shape as ShapePoint;
						List<Vector3> vertices = new List<Vector3> ();
						Vector3 vert = Ellipsoid.ToVector3 (Ellipsoid.Wgs84.ToVector3d (Trig.ToRadians (new Geodetic3D (shapePoint.Point.X, shapePoint.Point.Y))));
						vertices.Add (vert);
						list_vertices.Add (vertices);
						GameObject go = new GameObject ();
						PolyPoint polypoint = go.AddComponent<PolyPoint> ();
						polypoint.Initialize (vertices, Color.yellow);
                        go.layer = 9;
                        go.name = Name;
						gobjs.Add (Name, go);
					} 
					else if (_shape.Type == ShapeType.Polygon) {

                        int partcount = 0;
						ShapePolygon shapePolygon = _shape as ShapePolygon;
						foreach (PointD[] part in shapePolygon.Parts) {
							List<Vector3> vertices = new List<Vector3> ();

                            foreach (PointD point in part)
                                vertices.Add(Ellipsoid.ToVector3(Ellipsoid.Wgs84.ToVector3d(Trig.ToRadians(new Geodetic3D(point.X, point.Y)))));
							list_vertices.Add (vertices);
                            GameObject go = new GameObject();
                            Polyline polyline = go.AddComponent<Polyline>();
                            polyline.Initialize(vertices, 1.0f, Color.yellow);
                            go.layer = 9;

                            go.name = Name + partcount.ToString();
                            gobjs.Add(Name + partcount.ToString(), go);

                            if (fill)
                            {
                                go = new GameObject();
                                go.layer = 9;
                                Polygon polygon = go.AddComponent<Polygon>();
                                polygon.Initialize(vertices, Color.red);
                                go.name = Name + partcount.ToString() + "_fill";
                                gobjs.Add(Name + partcount.ToString()+"_fill", go);
                            }
                            partcount++; 
                        }
					}
					else if (_shape.Type == ShapeType.PolyLine) {

						ShapePolyLine shapePolyline = _shape as ShapePolyLine;
						foreach (PointD[] part in shapePolyline.Parts) {
							counter++;
							List<Vector3> vertices = new List<Vector3> ();
                            if ((total_vertices + part.Length) >= 20000)
                            {
                                GameObject go = new GameObject();
                                Polyline polyline = go.AddComponent<Polyline>();
                                polyline.Initialize(vertices, 1.0f, Color.yellow);
                                go.layer = 9;
                                go.name = Name + (gobjs.Count + 1).ToString();
                                gobjs.Add(Name + (gobjs.Count + 1).ToString(), go);
                                total_vertices = 0;
                            }
							foreach (PointD point in part) {
								Vector3 vert = Ellipsoid.ToVector3 (Ellipsoid.Wgs84.ToVector3d (Trig.ToRadians (new Geodetic3D (point.X, point.Y))));
								vertices.Add (vert);
                                total_vertices++;
							}
                            list_vertices.Add(vertices);
						}
						GameObject go1 = new GameObject ();
						Polyline polyline1 = go1.AddComponent<Polyline> ();
                        list_vertices.Clear();
                        go1.layer = 9;
                        go1.name = Name;
						gobjs.Add (Name, go1);
					}
				}

                public void Import(String path)
                {
                    ImportShape(path, "border");
                    ImportShape(path, "fill");
                    ImportShape(path, "label");
                }

                public bool ImportShape(String path, String type)
                {
                    GameObject go;
                    String bpath = Path.Combine(path, type);
                    if (Directory.Exists(bpath))
                    {
                        bpath = Path.Combine(bpath, Name+".asset");
                        Mesh mesh = (Mesh)AssetDatabase.LoadAssetAtPath(bpath, typeof(Mesh));
                        if (type == "border")
                        {
                            go = new GameObject();
                            Polyline polyline1 = go.AddComponent<Polyline>();
							polyline1.Initialize (mesh, 1.0f, Color.yellow);
                            gobjs.Add(Name, go);
						}
                        else if (type == "fill")
                        {
                            Debug.Log("Import Geometry fill" + bpath);
                            go = new GameObject();
                            Polygon polygon1 = new Polygon();
                            polygon1.Initialize(mesh, Color.red);
                            gobjs.Add(Name, go);
						}
                        else if (type == "label")
                        {
                            Debug.Log("Import Geometry label" + bpath);
                            go = new GameObject();
                            PolyPoint polypoint1 = go.AddComponent<PolyPoint>();
							polypoint1.Initialize (mesh, Color.red);
                            gobjs.Add(Name, go);
						}
                        return true;
                    }
					
                    return false;
                }

                public void Export(String path, String Name)
                {
                    int index = path.IndexOf("\\Assets\\");
                    foreach (KeyValuePair<String, shape> item in shapes)
                    {
                        String subpath = path+"\\"+item.Key+"\\"+Name;
                        subpath = subpath.Replace('?', '_'); 
                        subpath = subpath.Replace('.', '_');
                        subpath += ".asset";
                        if (!Directory.Exists(Path.GetDirectoryName(subpath))) 
                            Directory.CreateDirectory(Path.GetDirectoryName(subpath));
                        if(File.Exists(subpath)) File.Delete(subpath);
                        AssetDatabase.CreateAsset(item.Value.mesh, subpath.Substring(index + 1));
                    }            
                }

                public void Render()
                {
                    foreach (KeyValuePair<String, shape> item in shapes)
                        item.Value.Render();
                }

				public void Update()
				{
					foreach (KeyValuePair<String, shape> item in shapes)
						item.Value.Update();
				}
            }
        }
    }
}
































