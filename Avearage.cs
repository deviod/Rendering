using System;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using GIS_FW;
using GAME_FW;
using UnityEngine;
using GIS_FW.TERRAIN;
using GIS_FW.DATA.RASTER;

namespace GIS_FW
{
    public class Average
    {
        public float sum;
        public int Items;
        private int Index;
        private int Counter;
        public float[] Values;

        public Average(int items)
        {
            sum = 0;
            Index = 0;
            Counter = 0;
            Items = items;
            Values = new float[Items];
            for (int i = 0; i < Items; i++) Values[i] = 0.0f;
        }

        public float Update(float value)
        {
            sum -= Values[Index];
            Values[Index++] = value;
            if (Index == Items) Index = 0;
            sum += value;
            if (Counter < Items) Counter++;
            return sum / Counter;
        }
    }

    public class Average_time
    {
        public float Sum;
        public int Items;
        private int Index;
        public float[] Values;
        public float[] Times;

        public Average_time(int items)
        {
            Sum = 0;
            Index = 0;
            Items = items;
            Times = new float[Items];
            Values = new float[Items];
            for (int i = 0; i < Items; i++) { Values[i] = 0.0f; Times[i] = 0.0f; }
        }

        public float Update(float value, float time)
        {
            Times[Index] = time;
            Sum -= Values[Index];
            Values[Index++] = value;
            if (Index == Items) Index = 0;
            Sum += value;
            float del_t = time - Times[Index];
            if (del_t > 0.0f) return Sum / del_t;
            else return 0.0f;
        }
    }
}
