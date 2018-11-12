using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using System;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class date : MonoBehaviour {

	//display date
	public int day;
	public int month;
	public int year;

	public DateTime current_time;

	private Text this_text;

	private readonly string[] months = { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"};
	private readonly string[] month_abrvs = {"Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov"};

	//returns length of specified month in days
	private int month_len(int mon, int yr)
	{
		int ml = 0;

		if (mon == 1)
		{
			ml = 31;
		}

		if (mon == 2)
		{
			ml = (yr % 4 == 0) ? 29 : 28;
		}

		if (mon == 3)
		{
			ml = 31;
		}

		if (mon == 4)
		{
			ml = 30;
		}

		if (mon == 5)
		{
			ml = 31;
		}

		if (mon == 6)
		{
			ml = 30;
		}

		if (mon == 7)
		{
			ml = 31;
		}

		if (mon == 8)
		{
			ml = 31;
		}

		if (mon == 9)
		{
			ml = 30;
		}

		if (mon == 10)
		{
			ml = 31;
		}

		if (mon == 11)
		{
			ml = 30;
		}

		if (mon == 12)
		{
			ml = 31;
		}

		return ml;
	}
	
	private readonly string[] days = {"Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"};
	private readonly string[] day_abrvs = {"Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat"};

	//doesn't allow for going before 2018
	private int total_days (int day, int mon, int yr)
	{
		int num_days = day;
		
		for (int i = 0; i < yr - 2018; i++)
		{
			num_days += ((i + 2) % 4 == 0) ? 366 : 365;
		}

		for (int i = 1; i < mon; i++)
		{
			num_days += month_len(i - 1, yr);
		}

		return num_days;
	}

	private int total_days()
	{
		return total_days(day, month, year);
	}
	
	private string date_to_day(int day, int mon, int yr)
	{
		return days[total_days(day, mon, yr) % 7];
	}

	private int week_start;
	private int week_end;
	
	
	//used for last week button
	public void last_week()
	{
		week_start -= 7;
		week_end -= 7;
		
		//update displayed date ranges
		
		//get events from database
	}
	
	//used for next week button
	public void next_week()
	{
		week_start += 7;
		week_end += 7;
		
		//update displayed date ranges
		
		//get events from database
	}

	//used for this week button, also updating week-dependent variables
	public void this_week()
	{
		week_start = total_days() - total_days() % 7 + 1;
		week_end = week_start + 6;
		
		//update displayed date ranges
		
		//get events from database
	}
	
	//used by the Create event button
	public void create_event()
	{
		//link to edit event scene
		SceneManager.LoadScene("EditEvent");
	}

	//used by the Logout button
	public void logout()
	{
		//clear calendar data
		
		//link to login scene
		SceneManager.LoadScene("Login");
	}
	
	//
	public int week_start_date()
	{
		int a = week_start;
		for(int i = month; )
	}
	
	// Use this for initialization
	void Start ()
	{
		current_time = DateTime.Now;

		this_text = GetComponent<Text>();

		this_week();
	}
	
	// Update is called once per frame
	void Update ()
	{
		DateTime new_time = DateTime.Now;

		if (new_time.Day != current_time.Day)
		{
			day += 1;

			if (total_days(day, month, year) > week_end)
			{
				this_week();
			}
		}

		current_time = new_time;
		
		//display events
	}
}
