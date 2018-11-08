using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class CalendarLayout : MonoBehaviour {
	// so I can access the prefab
	[Tooltip("This should have the hour prefab in it")]
	public GameObject hour;

	public GameObject parent;
	
	// Use this for initialization
	void Start () {
		// Instantiate time prefab to put hours and gridlines on the calendar
		string[] txts = { "12a","1","2","3","4","5","6","7","8","9","10","11","12p","1","2","3","4","5","6","7","8","9","10","11","12a" }; 
		for (int i = 0; i < txts.Length; i++)
		{
			GameObject a = Instantiate(hour, parent.transform.position, parent.transform.rotation);//new Vector3(-1393, 260 - i * 160, 40), Quaternion.identity);
			a.transform.SetParent(parent.transform);
			a.transform.position = new Vector3(60, 1160 - i * 160, 40);
			a.GetComponent<Text>().text = txts[i];
			a.name = txts[i];
		}
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
