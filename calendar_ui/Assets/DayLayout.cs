using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DayLayout : MonoBehaviour
{
	//should be set to the Background sprite
	public Sprite img;
	
	//should be set to the days GameObject
	public GameObject parent;
	
	//should be set to Arial
	public Font tfon;
	
	// Use this for initialization
	void Start () {
		
		//make vertical lines to divide the days
		for (int i = 0; i < 6; i++)
		{
			//make a GameObject named panel
			GameObject a = new GameObject("Panel");
			
			//add necessary components
			a.AddComponent<RectTransform>();
			a.AddComponent<CanvasRenderer>();
			a.AddComponent<Image>();

			//edit Image component
			Image aimg = a.GetComponent<Image>();
			aimg.sprite = img;
			aimg.color = new Color(0, 0, 0, 255);
			aimg.type = Image.Type.Sliced;
			aimg.fillCenter = true;

			//set location
			a.transform.SetParent(parent.transform);
			a.transform.position = new Vector3(517 + i * 388, 1235, 0);
			a.GetComponent<RectTransform>().sizeDelta = new Vector2(10, 156);
		}

		string[] days = { "Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat"};
		for (int i = 0; i < days.Length; i ++)
		{
			GameObject a = new GameObject("Text");

			a.AddComponent<RectTransform>();
			a.AddComponent<CanvasRenderer>();
			a.AddComponent<Text>();

			Text t = a.GetComponent<Text>();
			t.text = days[i];
			t.font = tfon;
			t.fontSize = 60;
			t.fontStyle = FontStyle.Bold;
			t.color = new Color(0,0,0,255);
			t.alignment = TextAnchor.UpperCenter;

			a.transform.SetParent(parent.transform);
			a.transform.position = new Vector3(324 + i * 388, 1221, 0);
			a.GetComponent<RectTransform>().sizeDelta = new Vector2(140, 80);
		}
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
