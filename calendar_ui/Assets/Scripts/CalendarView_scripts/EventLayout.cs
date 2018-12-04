using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;
using Image = UnityEngine.UI.Image;

public class EventLayout : MonoBehaviour
{
	//Should be set to the DateText GameObject
	public GameObject DateText;
	
	//should be set to the scrollbar
	public GameObject scrollbar;
	
	private DateObj.DateObject startDate = new DateObj.DateObject(0,0,0);

	private List<Dictionary<string, object>> events;
	private List<Dictionary<string, object>> displayable_events;
	
	//should be set to the Background sprite
	[FormerlySerializedAs("img")] public Sprite Img;
	
	//should be set to the days GameObject
	[FormerlySerializedAs("parent")] public GameObject Parent;
	
	//should be set to the Arial font
	[FormerlySerializedAs("tfon")] public Font Tfon;
	
	private readonly Color[] _eventColors =
	{
		new Color(0.65f, 0.81f, 0.89f, 1), new Color(0.12f, 0.47f, 0.71f, 1), new Color(0.70f, 0.87f, 0.54f, 1),
		new Color(0.2f, 0.63f, 0.17f, 1), new Color(0.2f, 0.60f, 0.60f, 1), new Color(0.99f, 0.75f, 0.44f, 1), 
		new Color(1, 0.50f, 0, 1), new Color(0.79f, 0.70f, 0.84f, 1),
	};

	private void get_data()
	{
		displayable_events.Clear();
		
		//get hardcoded events
		foreach (var e in events)
		{
			var endDate = startDate + new DateObj.DateObject(0, 0, 7);
			var date = e["date"].ToString().Split('/');
			var tday = new DateObj.DateObject(int.Parse(date[2]), int.Parse(date[0]), int.Parse(date[1]));
			
			if (tday >= startDate && tday < endDate) displayable_events.Add(e);
		}
		
		//get database events
		state.Comm.SendTcpMessage("02," + startDate.ToString("m:d:y") + "," + (startDate + new DateObj.DateObject(0,0,7)).ToString("m:d:y"));

		var stopwatch = new Stopwatch();
		stopwatch.Start();
		while (!state.displayable_events.Written && stopwatch.ElapsedMilliseconds < state.Timeout)
		{
			Thread.Sleep(200);
				
			Debug.Log("waiting for write for " + stopwatch.ElapsedMilliseconds);
			if (stopwatch.ElapsedMilliseconds < state.Timeout) continue;
			Debug.Log("timeout");
		}
		stopwatch.Stop();
		
		if (state.displayable_events.Written)
		{
			foreach (var evnt in state.displayable_events.Data)
			{
				Debug.Log("retrieved " + evnt["title"] +" from state.displayable_events.Data");
				displayable_events.Add(evnt);
				Debug.Log("added " + displayable_events[displayable_events.Count - 1]["title"] + "to list to be displayed");
			}
			
			Debug.Log("added events");
			
			state.displayable_events.Written = false;
		}
		else Debug.Log("RequestEventRange timeout");
	}

	// returns duration in pixels
	private static float Duration(string start, string end)
	{
		var ss = start.Split(':');
		var es = end.Split(':');

		float shour = int.Parse(ss[0]);
		var smin = int.Parse(ss[1].Substring(0,2));
		var shal = ss[1][2];

		float ehour = int.Parse(es[0]);
		var emin = int.Parse(es[1].Substring(0, 2));
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

		return (ehour - shour) * 160;

	}

	//returns y coord relative to the panel at which to put the event
	private float start_spot(string start, string end)
	{
		var ss = start.Split(':');
		float shour = int.Parse(ss[0]);
		var smin = int.Parse(ss[1].Substring(0,2));
		var shal = ss[1][2];
		
		//convert to military time for math purposes
		shour += shal == 'p' ? 12 : 0;
		//12a won't convert, so change it to 0a
		shour = shal == 'a' && shour == 12 ? 0 : shour;
		//change minutes to fraction of an hour and add it to the total
		shour += smin / 60.0f;
		
		var width_offset = (Duration(start, end) - 160) / 2;
		var scrollbar_offset = 2714 * scrollbar.GetComponent<Scrollbar>().value;
		
		// I can't explain how or why this works
		// but it has something to do with the event panel transforms being at the panel centers
		return -858 - shour * 160.0f - width_offset + scrollbar_offset;
	}

	private void create_text_go(string title, string text, int fontSize, FontStyle fStyle, Color tColor,
		GameObject tparent, Vector3 position, Vector2 size)
	{
		var tt = new GameObject(title);
			
		//add necessary components
		tt.AddComponent<RectTransform>();
		tt.AddComponent<CanvasRenderer>();
		tt.AddComponent<Text>();
			
		//edit text stuff
		var ttText = tt.GetComponent<Text>();
		ttText.text = text;
		ttText.font = Tfon;
		ttText.fontSize = fontSize;
		ttText.fontStyle = fStyle;
		ttText.color = tColor;
		ttText.alignment = TextAnchor.UpperCenter;
			
		//set location
		tt.transform.SetParent(tparent.transform);
		tt.transform.position = position;
		var ttrt = tt.GetComponent<RectTransform>();
		ttrt.sizeDelta = size;
		ttrt.anchorMin = new Vector2(0, 1);
		ttrt.anchorMax = new Vector2(0, 1);
	}

	// to be called by the edit buttons
	public void edit_button(Dictionary<string, object> eventDetails)
	{
		state.Title = eventDetails["title"].ToString();
		state.Date = eventDetails["date"].ToString();
		state.Start_time = eventDetails["start time"].ToString();
		state.End_time = eventDetails["end time"].ToString();
		
		SceneManager.LoadScene("EditEvent");
	}

	private void draw_events()
	{
		//clear out previously displayed events
		foreach (Transform child in Parent.transform) Destroy(child.gameObject);
		
		//create new events
		var de = displayable_events;

		for (var i = 0; i < de.Count; i++)
		{
			var date = de[i]["date"].ToString().Split('/');
			var date2 = new DateObj.DateObject(int.Parse(date[2]), int.Parse(date[0]), int.Parse(date[1]));

			var startTime = de[i]["start time"].ToString();
			var endTime= de[i]["end time"].ToString();
			var minDuration = Duration(startTime, endTime);
			var startPix = start_spot(startTime, endTime);

			var xChange = (date2.day_of_the_week() - 1) * 388;
			const float editButtonL = 40f;
			
			
			
			//BACK PANEL
			//init GameObject for the event
			var e = new GameObject("event " + i);

			//add necessary components
			e.AddComponent<RectTransform>();
			e.AddComponent<CanvasRenderer>();
			e.AddComponent<Image>();

			//edit Image component
			var eimg = e.GetComponent<Image>();
			eimg.sprite = Img;
			eimg.color = _eventColors[i % _eventColors.Length];
			eimg.type = Image.Type.Sliced;
			eimg.fillCenter = true;

			//set location
			e.transform.SetParent(Parent.transform);
			e.transform.position = new Vector3(-1046 + xChange, startPix, 0);
			var ert = e.GetComponent<RectTransform>();
			ert.sizeDelta = new Vector2(380, minDuration);
			ert.anchorMin = new Vector2(1, 1);
			ert.anchorMax = new Vector2(1, 1);
			

			
			//EDIT BUTTON
			var b = new GameObject("edit");

			//add necessary components
			b.AddComponent<RectTransform>();
			b.AddComponent<CanvasRenderer>();
			b.AddComponent<Image>();
			b.AddComponent<Button>();
			
			//set location
			b.transform.SetParent(e.transform);
			b.transform.position = new Vector3(310 + xChange - editButtonL/2f, 1916 + startPix, 0);
			var brt = b.GetComponent<RectTransform>();
			brt.sizeDelta = new Vector2(editButtonL, editButtonL);
			brt.anchorMin = new Vector2(1, 1);
			brt.anchorMax = new Vector2(1, 1);
			
			//edit Image component
			var bimg = b.GetComponent<Image>();
			bimg.sprite = Img;
			bimg.color = new Color(1, 1, 1, 0.4f);
			bimg.type = Image.Type.Sliced;
			bimg.fillCenter = true;
			
			//edit text stuff
			create_text_go("text", "✎", 35, FontStyle.Bold, Color.black, b,
				new Vector3(500 + xChange, 1871.25f + startPix + minDuration / 2 + editButtonL / 2, 0),
				new Vector2(editButtonL, editButtonL));
			
			//edit button.OnClick()
			var nam = e.name.Split(' ');
			var ind = Int32.Parse(nam[nam.Length-1]);
			b.GetComponent<Button>().onClick.AddListener(delegate { edit_button(de[ind]); });
			
			
			//TITLE TEXT
			create_text_go("title text", de[i]["title"].ToString(), 40, FontStyle.Bold, Color.black, e,
				new Vector3(500 + xChange - editButtonL / 2, 1913.25f + startPix, 0),
				new Vector2(380 - editButtonL, 45.5f));
			
			//TIME TEXT
			//if the event is long enough for there to be space on the event to show the time
			if (minDuration / 160.0f >= 0.5f)
				create_text_go("duration", de[i]["start time"] + " - " + de[i]["end time"], 30,
					FontStyle.Normal, Color.black, e, new Vector3(500 + xChange, 1871.25f + startPix, 0),
					new Vector2(380, 40));
			
			Debug.Log("panel created for " + de[i]["title"]);
		}
	}
	
	// Use this for initialization
	private void Start () {
		events = CSVReader.Read("events");
		displayable_events = new List<Dictionary<string, object>>();
	}
	
	// Update is called once per frame
	private void Update () {
		var temp = DateText.GetComponent<date>().display_date;

		if (temp == startDate && Time.frameCount >= 10) return;
		startDate = temp;
			
		get_data();
		draw_events();
	}
}
