#if UNITY_4_3 || UNITY_4_3_0 || UNITY_4_3_1 || UNITY_4_3_2 || UNITY_4_3_3 || UNITY_4_3_4 || UNITY_4_3_5 || UNITY_4_3_6 || UNITY_4_3_7 || UNITY_4_3_8 || UNITY_4_3_9 || UNITY_4_4 || UNITY_4_4_0 || UNITY_4_4_1 || UNITY_4_4_2 || UNITY_4_4_3 || UNITY_4_4_4 || UNITY_4_4_5 || UNITY_4_4_6 || UNITY_4_4_7 || UNITY_4_4_8 || UNITY_4_4_9 || UNITY_4_5 || UNITY_4_5_0 || UNITY_4_5_1 || UNITY_4_5_2 || UNITY_4_5_3 || UNITY_4_5_4 || UNITY_4_5_5 || UNITY_4_5_6 || UNITY_4_5_7 || UNITY_4_5_8 || UNITY_4_5_9 || UNITY_4_6 || UNITY_4_6_0 || UNITY_4_6_1 || UNITY_4_6_2 || UNITY_4_6_3 || UNITY_4_6_4 || UNITY_4_6_5 || UNITY_4_6_6 || UNITY_4_6_7 || UNITY_4_6_8 || UNITY_4_6_9 || UNITY_4_7 || UNITY_4_7_0 || UNITY_4_7_1 || UNITY_4_7_2 || UNITY_4_7_3 || UNITY_4_7_4 || UNITY_4_7_5 || UNITY_4_7_6 || UNITY_4_7_7 || UNITY_4_7_8 || UNITY_4_7_9 || UNITY_4_8 || UNITY_4_8_0 || UNITY_4_8_1 || UNITY_4_8_2 || UNITY_4_8_3 || UNITY_4_8_4 || UNITY_4_8_5 || UNITY_4_8_6 || UNITY_4_8_7 || UNITY_4_8_8 || UNITY_4_8_9
#define UNITY_4_3
#elif UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2
#define UNITY_4
#elif UNITY_3_0 || UNITY_3_0_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5 || UNITY_3_6 || UNITY_3_5_7 || UNITY_3_8
#define UNITY_3
#endif

using UnityEngine;
using System.Collections;
using System.Linq;

namespace ProGrids2 
{
	public static class pg_Util
	{

		public static Color ColorWithString(string value)
		{
			string valid = "01234567890.,";
	        value = new string(value.Where(c => valid.Contains(c)).ToArray());
	        string[] rgba = value.Split(',');
	        
	        // BRIGHT pink
	        if(rgba.Length < 4)
	        	return new Color(1f, 0f, 1f, 1f);

			return new Color(
				float.Parse(rgba[0]),
				float.Parse(rgba[1]),
				float.Parse(rgba[2]),
				float.Parse(rgba[3]));
		}

#region SNAP
		
		public static float ValueFromMask(Vector3 val, Vector3 mask)
		{
			if(Mathf.Abs(mask.x) > .0001f)
				return val.x;
			else if(Mathf.Abs(mask.y) > .0001f)
				return val.y;
			else
				return val.z;
		}

		public static Vector3 SnapValue(Vector3 val, float snapValue)
		{
			float _x = val.x, _y = val.y, _z = val.z;
			return new Vector3(
				Snap(_x, snapValue),
				Snap(_y, snapValue),
				Snap(_z, snapValue)
				);
		}

		const float APPROX_ZERO = .0001f;
		public static Vector3 SnapValue(Vector3 val, Vector3 mask, float snapValue)
		{

			float _x = val.x, _y = val.y, _z = val.z;
			return new Vector3(
				( Mathf.Abs(mask.x) < APPROX_ZERO ? _x : Snap(_x, snapValue) ),
				( Mathf.Abs(mask.y) < APPROX_ZERO ? _y : Snap(_y, snapValue) ),
				( Mathf.Abs(mask.z) < APPROX_ZERO ? _z : Snap(_z, snapValue) )
				);
		}

		public static Vector3 SnapToCeil(Vector3 val, Vector3 mask, float snapValue)
		{
			float _x = val.x, _y = val.y, _z = val.z;
			return new Vector3(
				( Mathf.Abs(mask.x) < APPROX_ZERO ? _x : SnapToCeil(_x, snapValue) ),
				( Mathf.Abs(mask.y) < APPROX_ZERO ? _y : SnapToCeil(_y, snapValue) ),
				( Mathf.Abs(mask.z) < APPROX_ZERO ? _z : SnapToCeil(_z, snapValue) )
				);
		}

		public static Vector3 SnapToFloor(Vector3 val, float snapValue)
		{
			float _x = val.x, _y = val.y, _z = val.z;
			return new Vector3(
				SnapToFloor(_x, snapValue),
				SnapToFloor(_y, snapValue),
				SnapToFloor(_z, snapValue)
				);
		}

		public static Vector3 SnapToFloor(Vector3 val, Vector3 mask, float snapValue)
		{
			float _x = val.x, _y = val.y, _z = val.z;
			return new Vector3(
				( Mathf.Abs(mask.x) < APPROX_ZERO ? _x : SnapToFloor(_x, snapValue) ),
				( Mathf.Abs(mask.y) < APPROX_ZERO ? _y : SnapToFloor(_y, snapValue) ),
				( Mathf.Abs(mask.z) < APPROX_ZERO ? _z : SnapToFloor(_z, snapValue) )
				);
		}

		public static float Snap(float val, float round)
		{
			return round * Mathf.Round(val / round);
		}

		public static float SnapToFloor(float val, float snapValue)
		{
			return snapValue * Mathf.Floor(val / snapValue);
		}

		public static float SnapToCeil(float val, float snapValue)
		{
			return snapValue * Mathf.Ceil(val / snapValue);
		}

		public static Vector3 CeilFloor(Vector3 v)
		{
			v.x = v.x < 0 ? -1 : 1;
			v.y = v.y < 0 ? -1 : 1;
			v.z = v.z < 0 ? -1 : 1;

			return v;
		}
#endregion
	}

	public static class PGExtensions
	{
		public static bool Contains(this Transform[] t_arr, Transform t)
		{
			for(int i = 0; i < t_arr.Length; i++)
				if(t_arr[i] == t)
					return true;
			return false;
		}

		public static float Sum(this Vector3 v)
		{
			return v[0] + v[1] + v[2];
		}
		
		public static bool InFrustum(this Camera cam, Vector3 point)
		{
			Vector3 p = cam.WorldToViewportPoint(point);
			return  (p.x >= 0f && p.x <= 1f) &&
					(p.y >= 0f && p.y <= 1f) &&
					p.z >= 0f;
		}
	}

}