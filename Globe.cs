
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using GIS_FW;
using GIS_FW.DATA;
using UnityEngine;

namespace GIS_FW
{
    public class Globe : MonoBehaviour
    {
        Mesh Cube;
        Material material;
        Texture2D BaseMap;
        int ShowGlobeGrid = 0;
        Vector4 light_dass = new Vector4(0.05f, .05f, .90f, 1.0f);

        public Globe()
        {
        }

		void Start()
		{
			Navigation.Local2ECEF(transform);
			Cube = BoxTessellator.Compute(Globals.Earth.ellipsoid.Radii);
			BaseMap = new Texture2D(512,512);
            byte[] data = File.ReadAllBytes(@"Data\Textures\earthtexture1.jpg");
			BaseMap.LoadImage(data);
			material = new Material (Shader.Find ("GIS_FW/Globe"));
			MeshRenderer renderer = gameObject.AddComponent<MeshRenderer> ();
			renderer.material = material;
			MeshFilter meshfilter = gameObject.AddComponent<MeshFilter> ();
			meshfilter.mesh = Cube;
			UpdateUniforms();
		}
        
        void UpdateUniforms()
        {
            Vector2 gridwidth = new Vector2(0.5f, 0.5f);
            Vector2 gridres = new Vector2(0.03f, 0.03f);
            Vector4 gridcolor = Color.white;

            Shader.SetGlobalInt("globe_draw_grid", Globals.Earth.ShowGlobeGrid);
            ShowGlobeGrid = Globals.Earth.ShowGlobeGrid;
            light_dass = new Vector4(0.05f, .05f, .90f, 1.0f);
            Shader.SetGlobalVector("globe_light_dsas", light_dass);
            Shader.SetGlobalInt("globe_UseAvgDepth", 1);

            Shader.SetGlobalTexture("GlobeTex", BaseMap);
            Shader.SetGlobalVector("globe_GridLineWidth",  gridwidth);
            Shader.SetGlobalVector("globe_GridResolution", gridres);
            Shader.SetGlobalVector("globe_GridColor", gridcolor);

            Shader.SetGlobalFloat("OneOverPi",  (float)Trig.OneOverPi);
            Shader.SetGlobalFloat("OneOver2Pi", (float)Trig.OneOverTwoPi);
            Shader.SetGlobalVector("OneOverR2", Trig.ToVector3(Globals.Earth.ellipsoid.OneOverRadiiSquared));
        }
        public void Update()
        {
            if (Globals.Earth.ShowGlobeGrid != ShowGlobeGrid)
            {
                ShowGlobeGrid = Globals.Earth.ShowGlobeGrid;
                Shader.SetGlobalInt("globe_draw_grid", ShowGlobeGrid);
            }

        }
    }
}

