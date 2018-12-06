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

	private DateObj.DateObject startDate = new DateObj.DateObject(0, 0, 0);
	private DateObj.DateObject today;
	// should be set to the DateText GameObject
	public GameObject DateText;
	
	// Use this for initialization
	void Start () {
		//make vertical lines to divide the days
		for (int i = 0; i < 6; i++)
		{
			//make a GameObject named panel
			GameObject a = new GameObject("divider " + i);
			
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
			a.transform.position = new Vector3(-918 + i * 388, 1159, 0);
			RectTransform art = a.GetComponent<RectTransform>();
			art.sizeDelta = new Vector2(10, 124);
			art.anchorMin = new Vector2(1, 1);
			art.anchorMax = new Vector2(1, 1);
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
		RectTransform brt = b.GetComponent<RectTransform>();
		brt.sizeDelta = new Vector2(382, 120);
		brt.anchorMin = new Vector2(1, 1);
		brt.anchorMax = new Vector2(1, 1);
		
		string[] days = { "Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat"};
		for (int i = 0; i < days.Length; i ++)
		{
			GameObject a = new GameObject(days[i]);

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
			a.transform.position = new Vector3(-1106 + i * 388, 1154, 0);
			RectTransform art = a.GetComponent<RectTransform>();
			art.sizeDelta = new Vector2(398, 130);
			art.anchorMin = new Vector2(1, 1);
			art.anchorMax = new Vector2(1, 1);
		}
	}
	
	// Update is called once per frame
	void Update () {
		//get startDate from date.cs
		DateObj.DateObject temp = DateText.GetComponent<date>().DisplayDate;

		if (temp != startDate)
		{
			startDate = temp;
			
			// set date text labels
			Text[] children = gameObject.transform.GetComponentsInChildren<Text>(true);
			for (int i = 0; i < children.Length; i++)
			{
				DateObj.DateObject t = startDate + new DateObj.DateObject(0, 0, i);

				children[i].text = t.ToString("D") + "\n" + t.ToString("d:mm:yy");
			}


			//update position of "today" marker
			today = DateText.GetComponent<date>().Today;
			int x_pos = (today < startDate || today > startDate + new DateObj.DateObject(0, 0, 7))
				? -2 : (today.day_of_the_week() - 1);
			gameObject.transform.Find("Today").transform.position = new Vector3(310f + x_pos * 388, 1222, 0);
		}
	}
}
