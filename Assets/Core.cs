using UnityEngine;
using System.Collections;

public class Core : MonoBehaviour 
{
	public static int scoreWhite, scoreBlack;
	public GUIStyle style;

	void Start () 
	{
		scoreWhite = 0;
		scoreBlack = 0;
		DontDestroyOnLoad(gameObject);
	}

	void OnGUI()
	{
		GUI.Label(new Rect(Screen.width / 2, 40, 30, 30), new GUIContent(scoreWhite + " - " + scoreBlack), style); 
	}
}
