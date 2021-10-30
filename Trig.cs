
using System;
using System.IO;
using GIS_FW;
using UnityEngine;

namespace GIS_FW
{
    public static class Trig
    {
        public const double OneOverPi = 1.0 / Math.PI;
        public const double PiOverTwo = Math.PI * 0.5;
        public const double PiOverThree = Math.PI / 3.0;
        public const double PiOverFour = Math.PI / 4.0;
        public const double PiOverSix = Math.PI / 6.0;
        public const double ThreePiOver2 = (3.0 * Math.PI) * 0.5;
        public const double TwoPi = 2.0 * Math.PI;
        public const double OneOverTwoPi = 1.0 / (2.0 * Math.PI);
        public const double RadiansPerDegree = Math.PI / 180.0;

        public static double ToRadians(double degrees)
        {
            return degrees * RadiansPerDegree;
        }

        public static Geodetic3D ToRadians(Geodetic3D geodetic)
        {
            return new Geodetic3D(ToRadians(geodetic.Longitude), ToRadians(geodetic.Latitude), geodetic.Height);
        }

        public static Geodetic2D ToRadians(Geodetic2D geodetic)
        {
            return new Geodetic2D(ToRadians(geodetic.Longitude), ToRadians(geodetic.Latitude));
        }

        public static double ToDegrees(double radians)
        {
            return radians / RadiansPerDegree;
        }

        public static Geodetic3D ToDegrees(Geodetic3D geodetic)
        {
            return new Geodetic3D(ToDegrees(geodetic.Longitude), ToDegrees(geodetic.Latitude), geodetic.Height);
        }

        public static Geodetic2D ToDegrees(Geodetic2D geodetic)
        {
            return new Geodetic2D(ToDegrees(geodetic.Longitude), ToDegrees(geodetic.Latitude));
        }

		public static Vector3 ToVector3(Vector3d v)
		{
			return new Vector3((float)v.x, (float)v.y, (float)v.z);
		}

        public static Vector4 ToVector4(Vector3d v)
        {
            return new Vector4((float)v.x, (float)v.y, (float)v.z, 1.0f);
        }

        public static Vector3 ToVector3(Vector3 vec, Vector3d v)
        {
            vec.x = (float)v.x;
            vec.y = (float)v.y;
            vec.z = (float)v.z;

            return vec;
        }

		public static Vector3d ToVector3d(Vector3 v)
		{
			return new Vector3d(v.x, v.y, v.z);
		}

		public static Vector2 ToVector2(Vector2d v)
		{
			return new Vector2((float)v.x, (float)v.y);
		}
    }

    public static class Utility
    {
        static Geodetic3D G1 = new Geodetic3D();
        static Geodetic3D G2 = new Geodetic3D();

        public static void DrawAxis(Transform trans)
        {
            Vector3 vec = new Vector3(0.3f, 0.0f, 0.0f);
            vec = trans.localToWorldMatrix.MultiplyVector(vec);
            Debug.DrawLine(trans.position, trans.position + vec, Color.red, 1000000);
            vec = new Vector3(0.0f, 0.3f, 0.0f);
            vec = trans.localToWorldMatrix.MultiplyVector(vec);
            Debug.DrawLine(trans.position, trans.position + vec, Color.green, 1000000);
            vec = new Vector3(0.0f, 0.0f, 0.3f);
            vec = trans.localToWorldMatrix.MultiplyVector(vec);
            Debug.DrawLine(trans.position, trans.position + vec, Color.blue, 1000000);
        }

        public static float DMS2Deg(float Deg, float Min, float Sec)
        {
            return Deg + (Min / 60.0f) + (Sec / 3600.0f);
        }

        public static void Swap<T>(ref T left, ref T right)
        {
            T temp = left;
            left = right;
            right = temp;
        }

        public static float FindAngle(Vector3 P1, Vector3 P2)
        {
            G1.Update(P1);
            G2.Update(P2);
            return FindAngle(G1, G2);
        }

        public static float FindAngle(Geodetic3D P1, Geodetic3D P2)
        {
            double xDiff = P2.Longitude - P1.Longitude;
            double yDiff = P2.Latitude - P1.Latitude;
            return (float)(Mathd.Atan2(yDiff, xDiff)*180.0/Mathd.PI);
        }

        public static double DegreeBearing(Geodetic3D P1, Geodetic3D P2)
        {
            if (P1 == null || P2 == null) return 0.0;
            var dLon = ToRad(P2.Longitude - P1.Longitude);
            var dPhi = Math.Log(Math.Tan(ToRad(P2.Latitude) / 2 + Math.PI / 4) / Math.Tan(ToRad(P1.Latitude) / 2 + Math.PI / 4));
            if (Math.Abs(dLon) > Math.PI) dLon = dLon > 0 ? -(2 * Math.PI - dLon) : (2 * Math.PI + dLon);
            return ToBearing(Math.Atan2(dLon, dPhi));
        }

        public static double ToRad(double degrees)
        {
            return degrees * (Math.PI / 180);
        }

        public static double ToDegrees(double radians)
        {
            return radians * 180 / Math.PI;
        }

        public static double ToBearing(double radians)
        {
            // convert radians to degrees (as bearing: 0...360)
            return (ToDegrees(radians) + 360) % 360;
        }

        public static float Distance(Geodetic3D p1, Geodetic3D p2)
        {
            double d = p1.Latitude * 0.017453292519943295;
            double num3 = p1.Longitude * 0.017453292519943295;
            double num4 = p2.Latitude * 0.017453292519943295;
            double num5 = p2.Longitude * 0.017453292519943295;
            double num6 = num5 - num3;
            double num7 = num4 - d;
            double num8 = Math.Pow(Math.Sin(num7 / 2.0), 2.0) + ((Math.Cos(d) * Math.Cos(num4)) * Math.Pow(Math.Sin(num6 / 2.0), 2.0));
            double num9 = 2.0 * Math.Atan2(Math.Sqrt(num8), Math.Sqrt(1.0 - num8));
            return (float)(6376500.0 * num9);
        }

        /*
        public static void Location(Geodetic3D Ref, double bearing, double distance, Geodetic3D result)
        {
            double angDist = distance / 6376500.0;
            double latitude = Ref.Latitude * Math.PI / 180;
            double longitude = Ref.Longitude * Math.PI / 180;
            double lat2 = Math.Asin(Math.Sin(latitude) * Math.Cos(angDist) + Math.Cos(latitude) * Math.Sin(angDist) * Math.Cos(bearing));
            double forAtana = Math.Sin(bearing) * Math.Sin(angDist) * Math.Cos(latitude);
            double forAtanb = Math.Cos(angDist) - Math.Sin(latitude) * Math.Sin(lat2);
            double lon2 = longitude + Math.Atan2(forAtana, forAtanb);
            result.Latitude = lat2*180.0 / Math.PI;
            result.Longitude = long2*180.0 / Math.PI;
        }
        */
    }
}