using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Core : MonoBehaviour 
{
	public static GameObject babyMe;
	public static List<GameObject> babies;
	public static int level = 1, maxLevel = 4;

	public float time = 0.0f; 

	void Start ()
	{
		babies = new List<GameObject>();
	}

	void Update()
	{
		time += Time.deltaTime;

		if(Input.GetKey(KeyCode.V)) PreviousLevel();
		else if(Input.GetKey(KeyCode.B)) NextLevel();
		else if(Input.GetKey(KeyCode.R)) Application.LoadLevel("Scene" + level);

		GetComponent<AudioSource>().volume = time / 20.0f + 0.1f; 
		GetComponent<AudioSource>().pitch = time / 20.0f + 0.5f; 
	}

	public static void NextLevel()
	{
		if(level < maxLevel) ++level;
		Application.LoadLevel("Scene" + level);
	}
	
	public static void PreviousLevel()
	{
		if(level > 1) --level;
		Application.LoadLevel("Scene" + level);
	}
}
