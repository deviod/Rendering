using System;
using System.IO;
using GIS_FW;
using System.Collections;
using System.Collections.Generic;
using GIS_FW.DATA;
using GIS_FW.TERRAIN;
using GIS_FW.DATA.RASTER;

using UnityEngine;
using UnityEngine.Rendering;

namespace GIS_FW
{
    namespace TERRAIN
    {
        public class Extent
        {
            public Extent() { }

            public Extent(int west, int south, int east, int north)
            {
                West = west;
                South = south;
                East = east;
                North = north;
            }

            public void Update(int west, int south, int east, int north)
            {
                West = west;
                South = south;
                East = east;
                North = north;
            }

            public bool IsEqual(Extent ext)
            {
                if (ext.North == North && ext.South == South && ext.East == East && ext.West == West) return true;
                return false;
            }

            public void Update(Extent ext)
            {
                West = ext.West;
                East = ext.East;
                North = ext.North;
                South = ext.South;
            }

            public String Message()
            {
                return "West:" + West.ToString() + "South:" + South.ToString() + "East:" + East.ToString() + "North:" + North.ToString();
            }

            public int West;
            public int South;
            public int East;
            public int North;
        }
    }
}
