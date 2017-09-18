using System.Collections.Generic;
using UnityEngine;

public class Vector3Comparer : IEqualityComparer<Vector3>
{
	public bool Equals(Vector3 x, Vector3 y)
	{
		return Mathf.Approximately(x.x, y.x) && 
		       Mathf.Approximately(x.y, y.y) && 
		       Mathf.Approximately(x.z, y.z);
	}
 
	public int GetHashCode(Vector3 vector)
	{
		return vector.x.GetHashCode() ^ vector.y.GetHashCode() << 2 ^ vector.z.GetHashCode() >> 2;
	}
}