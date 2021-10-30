using UnityEngine;
using System.Collections;
using GIS_FW;

namespace GIS_FW {

	public class Navigation {
        public static Vector3 vec3;
	    public static Transform local2ecef;
        public static Transform ecef2neu;

        public static Vector3 one = Vector3.one;
        public static Vector3 zero = Vector3.zero;
        public static Quaternion identity = Quaternion.identity;

        static Vector3 Vec = new Vector3();
        static Geodetic3D Pos = new Geodetic3D();
        static Geodetic3D Pos_rad = new Geodetic3D();

	    public static void InitNavigation()
	    {
            GameObject go = new GameObject("WGS");
	        local2ecef = go.transform;
            go = new GameObject("NEU");
            ecef2neu = go.transform;
            ecef2neu.parent = local2ecef;
            Local2ECEF(local2ecef);
            ECEF2Neu(ecef2neu);
        }

	    public static void Local2ECEF(Transform transform)
	    {
	        ResetAll(transform);
	        transform.localRotation = Quaternion.Euler(90.0f, 0.0f, -90.0f);
	        transform.localScale = new Vector3(1.0f, 1.0f, -1.0f);
	    }

	    public static void ECEF2Neu(Transform transform)
	    {
	        ResetAll(transform);
	        transform.localRotation = Quaternion.Euler(0.0f, -90.0f, 0.0f);
	        transform.localScale = new Vector3(1.0f, 1.0f, -1.0f);
	    }

        public static void ViewPoint(Transform transform, Vector3 Point)
        {
           Pos.Update(Point);
           ViewPoint(transform, Pos);
        }

        public static void ViewPoint(Transform transform, Geodetic3D Pos)
        {
           if (transform.parent != ecef2neu) transform.parent = ecef2neu;

           Pos_rad.Update(Trig.ToRadians(Pos.Longitude), Trig.ToRadians(Pos.Latitude), Pos.Height);
           vec3 = Trig.ToVector3(Vec, Ellipsoid.Wgs84.ToVector3d(Pos_rad));
           vec3 = local2ecef.localToWorldMatrix.MultiplyVector(vec3);
           transform.localPosition = ecef2neu.worldToLocalMatrix.MultiplyVector(vec3);
           transform.localRotation = (Quaternion.Euler(-(float)Pos.Longitude, 0.0f, 0.0f))*Quaternion.Euler(0.0f, (float)Pos.Latitude, 0.0f);
        }

        public static void SetPosition(Transform transform, Geodetic3D Pos)
        {
            ResetAll(transform);
            if (transform.parent != wgs2neu) transform.parent = wgs2neu;

            Pos_rad.Update(Trig.ToRadians(Pos.Longitude), Trig.ToRadians(Pos.Latitude), Pos.Height);
            vec3 = Trig.ToVector3(Vec, Ellipsoid.Wgs84.ToVector3d(Pos_rad));
            vec3 = local2ecef.localToWorldMatrix.MultiplyVector(vec3);
            transform.localPosition = ecef2neu.worldToLocalMatrix.MultiplyVector(vec3);
        }

        public static void SetRotation(Transform transform, Geodetic3D Pos)
        {
            transform.localRotation = Quaternion.Euler(-(float)Pos.Longitude, 0.0f, 0.0f);
            transform.localRotation *= Quaternion.Euler(0.0f, (float)Pos.Latitude, 0.0f);
        }

        public static void ResetLocal(Transform transform)
        {
            transform.localRotation = identity;
            transform.localPosition = zero;
            transform.localScale = one;
        }

	    public static void ResetAll(Transform transform)
	    {
            transform.position = zero;
            transform.rotation = identity;
	        transform.localRotation = identity;
	        transform.localPosition = zero;
	        transform.localScale = one;
	    }

        public static void Neu2Neu(Transform transform, Geodetic3D GeoPos)
        {
            Reset(transform);
            transform.Rotate(Vector3.up, -(float)GeoPos.Longitude);
            transform.Rotate(Vector3.right, -(float)GeoPos.Latitude);
        }

        public static void Neu2NNeu(Transform transform, float longitude)
        {
            Reset(transform);
            transform.RotateAround(Vector3.zero, Vector3.right, 90.0f);
            transform.RotateAround(Vector3.zero, Vector3.up, longitude);
        }
        
	    public static void ViewPoint(Transform transform, Geodetic3D Pos)
	    {
	       
            Reset(transform);
            if (transform.parent != wgs2neu) transform.parent = wgs2neu; 
            float gclat = (float)Mathd.Atan((1 - Ellipsoid.e2) * Mathd.Tan(Pos.Latitude));
	        transform.RotateAround(Vector3.zero, Vector3.right, gclat);
	        transform.RotateAround(Vector3.zero, Vector3.down, (float)Pos.Longitude);
	    }


	}
}