using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
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
	public Sprite img;
	
	//should be set to the days GameObject
	public GameObject parent;
	
	//should be set to the Arial font
	public Font tfon;
	
	private Color[] eventColors =
	{
		new Color(0.65f, 0.81f, 0.89f, 1), new Color(0.12f, 0.47f, 0.71f, 1), new Color(0.70f, 0.87f, 0.54f, 1),
		new Color(0.2f, 0.63f, 0.17f, 1), new Color(0.2f, 0.60f, 0.60f, 1), new Color(0.99f, 0.75f, 0.44f, 1), 
		new Color(1, 0.50f, 0, 1), new Color(0.79f, 0.70f, 0.84f, 1),
	};

	private void get_data()
	{
		//replace the code below with some protocol query function that gets the events within a certain date range and sets them to displayable_events

		displayable_events.Clear();
		
		foreach (var t in events)
		{
			DateObj.DateObject endDate = startDate + new DateObj.DateObject(0, 0, 7);
			string[] date = t["date"].ToString().Split('/');
			DateObj.DateObject tday = new DateObj.DateObject(int.Parse(date[2]), int.Parse(date[0]), int.Parse(date[1]));
			
			if (tday >= startDate && tday < endDate)
			{
				
				displayable_events.Add(t);
			}
		}
	}

	// returns duration in pixels
	private float duration(string start, string end)
	{
		string[] ss = start.Split(':');
		string[] es = end.Split(':');

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

		return (ehour - shour) * 160;

	}

	//returns y coord relative to the panel at which to put the event
	private float start_spot(string start, string end)
	{
		string[] ss = start.Split(':');
		float shour = int.Parse(ss[0]);
		int smin = int.Parse(ss[1].Substring(0,2));
		char shal = ss[1][2];
		
		//convert to military time for math purposes
		shour += shal == 'p' ? 12 : 0;
		//12a won't convert, so change it to 0a
		shour = shal == 'a' && shour == 12 ? 0 : shour;
		//change minutes to fraction of an hour and add it to the total
		shour += smin / 60.0f;
		
		float width_offset = (duration(start, end) - 160) / 2;
		float scrollbar_offset = 2714 * scrollbar.GetComponent<Scrollbar>().value;
		
		// I can't explain how or why this works
		// but it has something to do with the event panel transforms being at their centers
		return -858 - shour * 160.0f - width_offset + scrollbar_offset;
	}

	private void create_text_go(string title, string text, int fontSize, FontStyle fStyle, Color tColor,
		GameObject tparent, Vector3 position, Vector2 size)
	{
		GameObject tt = new GameObject(title);
			
		//add necessary components
		tt.AddComponent<RectTransform>();
		tt.AddComponent<CanvasRenderer>();
		tt.AddComponent<Text>();
			
		//edit text stuff
		Text ttText = tt.GetComponent<Text>();
		ttText.text = text;
		ttText.font = tfon;
		ttText.fontSize = fontSize;
		ttText.fontStyle = fStyle;
		ttText.color = tColor;
		ttText.alignment = TextAnchor.UpperCenter;
			
		//set location
		tt.transform.SetParent(tparent.transform);
		tt.transform.position = position;
		RectTransform ttrt = tt.GetComponent<RectTransform>();
		ttrt.sizeDelta = size;
		ttrt.anchorMin = new Vector2(0, 1);
		ttrt.anchorMax = new Vector2(0, 1);
	}

	private void draw_events()
	{
		//clear out previously displayed events
		foreach (Transform child in parent.transform)
		{
			Destroy(child.gameObject);
		}
		
		//create new events
		List<Dictionary<string, object>> de = displayable_events;

		for (int i = 0; i < de.Count; i++)
		{
			string[] date = de[i]["date"].ToString().Split('/');
			DateObj.DateObject date2 = new DateObj.DateObject(int.Parse(date[2]), int.Parse(date[0]), int.Parse(date[1]));

			string start_time = de[i]["start time"].ToString();
			string end_time = de[i]["end time"].ToString();
			float min_duration = duration(start_time, end_time);
			float start_pix = start_spot(start_time, end_time);

			int x_change = (date2.day_of_the_week() - 1) * 388;
			
			//BACK PANEL
			//init GameObject for the event
			GameObject e = new GameObject("event " + i);
			
			//add necessary components
			e.AddComponent<RectTransform>();
			e.AddComponent<CanvasRenderer>();
			e.AddComponent<Image>();
			
			//edit Image component
			Image eimg = e.GetComponent<Image>();
			eimg.sprite = img;
			eimg.color = eventColors[i % eventColors.Length];
			eimg.type = Image.Type.Sliced;
			eimg.fillCenter = true;
			
			//set location
			e.transform.SetParent(parent.transform);
			e.transform.position = new Vector3(-1046 + x_change, start_pix, 0);
			RectTransform ert = e.GetComponent<RectTransform>();
			ert.sizeDelta = new Vector2(380, min_duration);
			ert.anchorMin = new Vector2(1, 1);
			ert.anchorMax = new Vector2(1, 1);
			
			//TITLE TEXT
			create_text_go("title text", de[i]["title"].ToString(), 40, FontStyle.Bold, Color.black, e,
				new Vector3(500 + x_change, 1913.25f + start_pix, 0), new Vector2(380, 45.5f));
			//TIME TEXT
			if(min_duration / 160.0f >= 0.5f)
				create_text_go("duration", de[i]["start time"] + " - " + de[i]["end time"], 30,
					FontStyle.Normal, Color.black, e, new Vector3(500 + x_change, 1871.25f + start_pix, 0), new Vector2(380, 40));
		}
	}
	
	// Use this for initialization
	void Start () {
		//delete when protocol is implemented
		events = CSVReader.Read("events");
		displayable_events = new List<Dictionary<string, object>>();
	}
	
	// Update is called once per frame
	void Update () {
		DateObj.DateObject temp = DateText.GetComponent<date>().display_date;
		
		if (temp != startDate || Time.frameCount < 10)
		{
			startDate = temp;
			
			get_data();
			
			draw_events();
		}
	}
}
