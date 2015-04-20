using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Core : MonoBehaviour 
{
	public static List<Baby> babies;

	public static int scoreWhite = 0, scoreBlack = 0;
	public static int level = 1, maxLevel = 4;
	public GUIStyle styleWhite, styleBlack;

	public float time = 0.0f; 

	void Start ()
	{
		babies = new List<Baby>();
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

	void OnGUI()
	{
		GUI.Label(new Rect(Screen.width / 2 - 50, 40, 30, 30), new GUIContent(scoreWhite.ToString()), styleWhite); 
		GUI.Label(new Rect(Screen.width / 2 + 50, 40, 30, 30), new GUIContent(scoreBlack.ToString()), styleBlack); 
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
