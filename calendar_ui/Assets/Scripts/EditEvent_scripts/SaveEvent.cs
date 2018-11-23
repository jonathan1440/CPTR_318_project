using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class SaveEvent : MonoBehaviour
{
	//should be set to TitleField 
	public InputField Title;
	//should be set to DateField
	public InputField Date;
	//should be set to StartTimeField
	public InputField StartTime;
	//should be set to EndTimeField
	public InputField EndTime;

	//has to be initialized to a full array to enable the check to see if all the values have been supplied before submitting them
	private string[] newEventData = {"", "", "", ""};
	
	private readonly Color badData = new Color(0.91f, 0.26f, 0.26f, 255);
	private readonly Color goodData = new Color(1, 1, 1, 255);

	//should be used by the TitleField
	public void title_input()
	{
		string val = Title.text;
		
		//check to see if the data is usable
		bool bad = true;
		string[] vals = val.Split(' ');
		
		foreach (string p in vals)
		{
			if (p != "")
			{
				bad = false;
				break;
			}
		}

		//if unusable
		if (bad)
			Title.GetComponent<Image>().color = badData;
		else
		{
			Title.GetComponent<Image>().color = goodData;
			newEventData[0] = val;
		}
	}
	
	//should be used by the DateField
	public void date_input()
	{
		string val = Date.text;
		
		//check to see if the data is usable
		bool bad = false;
		string[] vals = val.Split('/');

		if (vals.Length > 3)
		{
			bad = true;
		}
		else
		{
			for (int i = 0; i < 3; i ++)
			{
				int temp;
				bad = !Int32.TryParse(vals[i], out temp);
			}
			
			if (!bad)
			{
				DateObj.DateObject temp = new DateObj.DateObject(int.Parse(vals[2]), int.Parse(vals[0]), int.Parse(vals[1]));
				
				if (temp.day != int.Parse(vals[1]) ||
				    temp.month != int.Parse(vals[0]) ||
				    temp.year != int.Parse(vals[2]))
				{
					bad = true;
				}
			}

			if (vals[2].Length != 4)
				bad = true;
		}
		
		//if unusable
		if (bad)
		{
			Date.GetComponent<Image>().color = badData;
		}
		else
		{
			Date.GetComponent<Image>().color = goodData;
			newEventData[1] = val;
		}
	}
	
	//should be used by the StartTimeField
	public void start_time_input()
	{
		string val = StartTime.text;
		
		//check to see if the data is usable
		bool bad;
		string[] vals = val.Split(':');

		string hour;
		string min;
		string hal;
		
		if(vals.Length == 2)
		{
			hour = vals[0];

			if (vals[1].Length == 3)
			{
				min = vals[1].Substring(0,2);
				hal = vals[1].Substring(2,1);

				int tmin, thour;
				if (int.TryParse(hour, out thour) && int.TryParse(min, out tmin) && (hal == "a" || hal == "p"))
				{
					bad = thour < 0 && thour > 13 && tmin < 0 && tmin > 60;
				}
				else
				{
					bad = true;
				}
			}
			else
			{
				bad = true;
			}
		}
		else
		{
			bad = true;
		}

		//if unusable
		if (bad)
			StartTime.GetComponent<Image>().color = badData;
		else
		{
			StartTime.GetComponent<Image>().color = goodData;
			newEventData[2] = val;
		}
	}

	//should be used by the EndTimeField
	public void end_time_input()
	{
		string val = EndTime.text;
		
		//check to see if the data is usable
		bool bad;
		string[] vals = val.Split(':');

		string hour;
		string min;
		string hal;
		
		if(vals.Length == 2)
		{
			hour = vals[0];

			if (vals[1].Length == 3)
			{
				min = vals[1].Substring(0,2);
				hal = vals[1].Substring(2,1);

				int tmin, thour;
				if (int.TryParse(hour, out thour) && int.TryParse(min, out tmin) && (hal == "a" || hal == "p"))
				{
					bad = thour < 0 && thour > 13 && tmin < 0 && tmin > 60;
				}
				else
				{
					bad = true;
				}
			}
			else
			{
				bad = true;
			}
		}
		else
		{
			bad = true;
		}

		//if unusable
		if (bad)
			EndTime.GetComponent<Image>().color = badData;
		else
		{
			EndTime.GetComponent<Image>().color = goodData;
			newEventData[3] = val;
		}
	}

	// used by the Save button
	public void create_event()
	{
		//if all values have been supplied
		if (ArrayUtility.IndexOf(newEventData, "") == -1)
		{
			//check to make sure end time is after start time
			string[] ss = newEventData[2].Split(':');
			string[] es = newEventData[3].Split(':');

			float shour = int.Parse(ss[0]);
			int smin = int.Parse(ss[1].Substring(0,2));
			char shal = ss[1][2];

			float ehour = int.Parse(es[0]);
			int emin = int.Parse(es[1].Substring(0, 2));
			int ehal = es[1][2];
		
			//convert to military time for math purposes
			shour += shal == 'p' ? 12 : 0;
			ehour += ehal == 'p' ? 12 : 0;
		
			//12a won't convert, so change it to 0a
			shour = shal == 'a' && shour == 12 ? 0 : shour;
			ehour = shal == 'a' && ehour == 12 ? 0 : ehour;

			//change minutes to fraction of an hour and add it to the total
			shour += smin / 60.0f;
			ehour += emin / 60.0f;

			if (shour <= ehour)
			{
				// send newEventData to to database with protocol

				if (state.Title != "Title")
				{
					// send data back with signal that it was edited
				}
				else
				{
					//send back normal data
				}
				
				
				
				
				
				
				
				// *** ***

				SceneManager.LoadScene("CalendarView");
			}
			else
			{
				StartTime.GetComponent<Image>().color = badData;
				EndTime.GetComponent<Image>().color = badData;
			}
		}
		else
		{
			if(newEventData[0] == "")
				Title.GetComponent<Image>().color = badData;
			if(newEventData[1] == "")
				Date.GetComponent<Image>().color = badData;
			if(newEventData[2] == "")
				StartTime.GetComponent<Image>().color = badData;
			if(newEventData[3] == "")
				EndTime.GetComponent<Image>().color = badData;
		}
	}

	// used by the Cancel button
	public void cancel()
	{
		SceneManager.LoadScene("CalendarView");
	}

	private void Start()
	{
		if (state.Title == "")
		{
			state.Title = "Title";
		}

		if (state.Date == "")
		{
			state.Date = "Date (m/d/yyyy)";
		}

		if (state.Start_time == "")
		{
			state.Start_time = "Start time (11:30a)";
		}

		if (state.End_time == "")
		{
			state.End_time = "End time (7:45p)";
		}
		
		Title.GetComponentInChildren<Text>().text = state.Title;
		Date.GetComponentInChildren<Text>().text = state.Date;
		StartTime.GetComponentInChildren<Text>().text = state.Start_time;
		EndTime.GetComponentInChildren<Text>().text = state.End_time;
	}
}
