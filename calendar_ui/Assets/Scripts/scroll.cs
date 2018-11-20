using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class scroll : MonoBehaviour
{
	private Vector3 original;
	private Vector3 new_pos;
	private int hour;

	//should be set to the scrollbar
	public GameObject scrollbar; 
	
	public void on_scroll(float value)
	{
		scrollbar.GetComponent<Scrollbar>().value = value;
		gameObject.transform.position = original + new Vector3(0, 2714 * value, 0);
	}
	
	// Use this for initialization
	void Start ()
	{
		original = gameObject.transform.position;
		
		hour = DateTime.Now.Hour;
		
		new_pos = original + new Vector3(0, 2714 * (hour / 24f), 0);
	}
	
	// Update is called once per frame
	void Update ()
	{
		int temp = int.Parse(DateTime.Now.Hour.ToString());
		
		if (temp != hour)
		{
			hour = temp;
			
			on_scroll(hour / 24f);
		}

		// have to do it like this to account for the very first check in the start() method
		// If I change the position in the start() method, then the dynamically instantiated objects won't change with it
		// But if I let this happen other than at the very beginning, nothing will actually scroll
		if(Time.renderedFrameCount <= 2)
			on_scroll(hour / 24f);
	}
}
