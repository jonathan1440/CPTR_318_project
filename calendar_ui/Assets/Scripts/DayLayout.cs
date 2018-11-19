using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

public class DayLayout : MonoBehaviour
{
	//should be set to the Background sprite
	public Sprite img;
	
	//should be set to the days GameObject
	public GameObject parent;
	
	//should be set to Arial
	public Font tfon;

	private DateObj.DateObject startDate;
	private DateObj.DateObject today;
	// should be set to the DateText GameObject
	public GameObject placeholder;
	
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
		
		//Make panel to highlight the current day
		GameObject b = new GameObject("Today");
		
		//add necessary components
		b.AddComponent<RectTransform>();
		b.AddComponent<CanvasRenderer>();
		b.AddComponent<Image>();
		
		//edit Image component
		Image bimg = b.GetComponent<Image>();
		bimg.sprite = img;
		bimg.color = new Color(1, 0, 0, 255);
		bimg.type = Image.Type.Sliced;
		bimg.fillCenter = true;
		
		//set location
		b.transform.SetParent(parent.transform);
		b.transform.position = new Vector3(322.5f, 1224, 0);
		b.GetComponent<RectTransform>().sizeDelta = new Vector2(382, 120);
		
		string[] days = { "Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat"};
		for (int i = 0; i < days.Length; i ++)
		{
			GameObject a = new GameObject("Text");

			a.AddComponent<RectTransform>();
			a.AddComponent<CanvasRenderer>();
			a.AddComponent<Text>();

			Text t = a.GetComponent<Text>();
			t.font = tfon;
			t.fontSize = 50;
			t.fontStyle = FontStyle.Bold;
			t.color = new Color(0, 0, 0, 255);
			t.alignment = TextAnchor.UpperCenter;

			a.transform.SetParent(parent.transform);
			a.transform.position = new Vector3(324 + i * 388, 1216, 0);
			a.GetComponent<RectTransform>().sizeDelta = new Vector2(398, 130);
		}
	}
	
	// Update is called once per frame
	void Update () {
		//get startDate from date.cs
		startDate = placeholder.GetComponent<date>().display_date.week_start();
		
		Text[] children = gameObject.transform.GetComponentsInChildren<Text>(true);
		for (int i = 0; i < children.Length; i ++)
		{
			DateObj.DateObject t = startDate + new DateObj.DateObject(0, 0, i);
			
			children[i].text = t.ToString("D") + "\n" + t.ToString("d:mm:yy");
		}

		today = placeholder.GetComponent<date>().today;
		gameObject.transform.Find("Today").transform.position = new Vector3(322.5f + (today.day_of_the_week() - 1) * 388, 1224, 0);
	}
}
