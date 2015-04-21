using UnityEngine;
using System.Collections;

public class BabyInfo
{
	public Vector3 position;
	public Quaternion rotation;
	public int networkId;

	public BabyInfo()
	{
		position = Vector3.zero;
		rotation = Quaternion.identity;

		networkId = -1;
	}
}
