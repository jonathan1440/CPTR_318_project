using System.Diagnostics;
using System.Linq;
using System.Threading;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

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
	private readonly string[] _newEventData = {"", "", "", ""};
	private readonly string[] _original = {"", "", "", ""};
	
	private readonly Color _badData = new Color(0.91f, 0.26f, 0.26f, 255);
	private readonly Color _goodData = new Color(1, 1, 1, 255);

	//should be used by the TitleField
	public void title_input()
	{
		var val = Title.text;
		
		//check to see if the data is usable
		var vals = val.Split(' ');

		// if any part of vals equals "" or val is too long, then the value is bad
		var bad = vals.All(p => p == "") || val.Length > state.MaxDateTitleLen;

		//if unusable
		if (bad) Title.GetComponent<Image>().color = _badData;
		else
		{
			Title.GetComponent<Image>().color = _goodData;
			_newEventData[0] = val;
		}
	}
	
	//should be used by the DateField
	public void date_input()
	{
		var val = Date.text;
		
		//check to see if the data is usable
		var bad = false;
		var vals = val.Split('/');

		if (vals.Length > 3) bad = true;
		else
		{
			//check if vals includes anything that can't be converted to an int
			for (var i = 0; i < 3; i ++)
			{
				int temp;
				bad = !int.TryParse(vals[i], out temp);
			}
			
			if (!bad)
			{
				var temp = new DateObj.DateObject(int.Parse(vals[2]), int.Parse(vals[0]), int.Parse(vals[1]));

				if (temp.day != int.Parse(vals[1]) || temp.month != int.Parse(vals[0]) ||
				    temp.year != int.Parse(vals[2])) bad = true;
			}

			if (vals[2].Length != 4) bad = true;
		}
		
		//if unusable
		if (bad) Date.GetComponent<Image>().color = _badData;
		else
		{
			Date.GetComponent<Image>().color = _goodData;
			_newEventData[1] = val;
		}
	}
	
	//should be used by the StartTimeField
	public void start_time_input()
	{
		var val = StartTime.text;
		
		//check to see if the data is usable
		bool bad;
		var vals = val.Split(':');

		if(vals.Length == 2)
		{
			var hour = vals[0];

			if (vals[1].Length == 3)
			{
				var min = vals[1].Substring(0,2);
				var hal = vals[1].Substring(2,1);

				int tmin, thour;
				if (int.TryParse(hour, out thour) && int.TryParse(min, out tmin) && (hal == "a" || hal == "p"))
					bad = thour < 0 && thour > 13 && tmin < 0 && tmin > 60;
				else bad = true;
			}
			else bad = true;
		}
		else bad = true;

		//if unusable
		if (bad) StartTime.GetComponent<Image>().color = _badData;
		else
		{
			StartTime.GetComponent<Image>().color = _goodData;
			_newEventData[2] = val;
		}
	}

	//should be used by the EndTimeField
	public void end_time_input()
	{
		var val = EndTime.text;
		
		//check to see if the data is usable
		bool bad;
		var vals = val.Split(':');

		if(vals.Length == 2)
		{
			var hour = vals[0];

			if (vals[1].Length == 3)
			{
				var min = vals[1].Substring(0,2);
				var hal = vals[1].Substring(2,1);

				int tmin, thour;
				if (int.TryParse(hour, out thour) && int.TryParse(min, out tmin) && (hal == "a" || hal == "p"))
					bad = thour < 0 && thour > 13 && tmin < 0 && tmin > 60;
				else bad = true;
			}
			else bad = true;
		}
		else bad = true;

		//if unusable
		if (bad) EndTime.GetComponent<Image>().color = _badData;
		else
		{
			EndTime.GetComponent<Image>().color = _goodData;
			_newEventData[3] = val;
		}
	}

	// used by the Save button
	public void create_event()
	{
		//if all values have been supplied
		if (ArrayUtility.IndexOf(_newEventData, "") == -1)
		{
			//check to make sure end time is after start time
			var ss = _newEventData[2].Split(':');
			var es = _newEventData[3].Split(':');

			float shour = int.Parse(ss[0]);
			var smin = int.Parse(ss[1].Substring(0,2));
			int shal = ss[1][2];

			float ehour = int.Parse(es[0]);
			var emin = int.Parse(es[1].Substring(0, 2));
			int ehal = es[1][2];
		
			//convert to military time for math purposes
			shour += (shal == 'p' && shour != 12)? 12 : 0;
			ehour += (ehal == 'p' && ehour != 12)? 12 : 0;
		
			//12a won't convert, so change it to 0a
			shour = shal == 'a' && shour == 12 ? 0 : shour;
			ehour = shal == 'a' && ehour == 12 ? 0 : ehour;

			//change minutes to fraction of an hour and add it to the total
			shour += smin / 60.0f;
			ehour += emin / 60.0f;

			if (shour <= ehour)
			{
				// if editing an event, instead of adding a new one
				if (state.Title != "Title")
				{
					// send data back with signal that it was edited
					state.Comm.SendPreEditEvent(_original[0], _original[1], _original[2], _original[3]);
					
					if (!ClientComms.SendPostEditEvent(_newEventData[0], _newEventData[1], _newEventData[2], _newEventData[3])) return;
					
					state.SendEditedEvent.Written = false;
					SceneManager.LoadScene("CalendarView");
				}
				else
				{
					//send back normal data
					if (!ClientComms.SendNewEvent(_newEventData[0], _newEventData[1], _newEventData[2], _newEventData[3])) return;
					
					state.SendNewEvent.Written = false;
					SceneManager.LoadScene("CalendarView");
				}
			}
			else
			{
				StartTime.GetComponent<Image>().color = _badData;
				EndTime.GetComponent<Image>().color = _badData;
			}
		}
		else
		{
			if(_newEventData[0] == "") Title.GetComponent<Image>().color = _badData;
			if(_newEventData[1] == "") Date.GetComponent<Image>().color = _badData;
			if(_newEventData[2] == "") StartTime.GetComponent<Image>().color = _badData;
			if(_newEventData[3] == "") EndTime.GetComponent<Image>().color = _badData;
		}
	}

	// used by the Cancel button
	public void Cancel()
	{
		SceneManager.LoadScene("CalendarView");
	}

	private void Start()
	{
		if (state.Title == "") state.Title = "Title";
		if (state.Date == "") state.Date = "Date (m/d/yyyy)";
		if (state.Start_time == "") state.Start_time = "Start time (11:30a)";
		if (state.End_time == "") state.End_time = "End time (7:45p)";
		
		Title.GetComponentInChildren<Text>().text = state.Title;
		Date.GetComponentInChildren<Text>().text = state.Date;
		StartTime.GetComponentInChildren<Text>().text = state.Start_time;
		EndTime.GetComponentInChildren<Text>().text = state.End_time;

		_original[0] = Title.GetComponentInChildren<Text>().text;
		_original[1] = Date.GetComponentInChildren<Text>().text;
		_original[2] = StartTime.GetComponentInChildren<Text>().text;
		_original[3] = EndTime.GetComponentInChildren<Text>().text;
	}
}
