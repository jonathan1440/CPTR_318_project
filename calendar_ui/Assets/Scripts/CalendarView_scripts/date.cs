using UnityEngine;
using System;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class date : MonoBehaviour {

	//display date
	public int Day;
	public int Month;
	public int Year;

	public DateObj.DateObject Today;
	
	//the first day of the week currently being displayed
	public DateObj.DateObject DisplayDate;
	
	//used for last week button
	public void last_week()
	{
		DisplayDate -= new DateObj.DateObject(0, 0, 7);
	}
	
	//used for next week button
	public void next_week()
	{
		DisplayDate += new DateObj.DateObject(0, 0, 7);
	}

	//used for this week button
	public void this_week()
	{
		DisplayDate = Today.week_start();
	}
	
	//used by the Create event button
	public void create_event()
	{
		state.Title = "Title";
		state.Date = "Date (m/d/yyyy)";
		state.Start_time = "Start time (11:30a)";
		state.End_time = "End time (7:45p)";
		
		//link to edit event scene
		SceneManager.LoadScene("EditEvent");
	}

	//used by the Logout button
	public void logout()
	{
		//terminate session/connection
		if (!ClientComms.SendLogout()) return;
		
		state.TerminateComm.Written = false;
			
		SceneManager.LoadScene("Login");
	}
	
	
	// Use this for initialization
	private void Start ()
	{
		var ct = DateTime.Now;
		
		//allow today to be set by the user for simulation/testing purposes
		if (Year != 0 && Month != 0 && Day != 0) Today = new DateObj.DateObject(Year, Month, Day);
		else Today = new DateObj.DateObject(ct.Year, ct.Month, ct.Day);
		
		DisplayDate = Today.week_start();

		GetComponent<Text>().text = Today.ToString("d:M:Y");

		this_week();
	}
	
	// Update is called once per frame
	private void Update ()
	{
		//check to see if it's a new day yet
		var ct = DateTime.Now;

		if (ct.Year == Today.year && ct.Month == Today.month && ct.Day == Today.day) return;
		
		if (Year != 0 && Month != 0 && Day != 0) Today = new DateObj.DateObject(Year, Month, Day);
		else Today = new DateObj.DateObject(ct.Year, ct.Month, ct.Day);
			
		GetComponent<Text>().text = Today.ToString("d:M:Y");
	}
}
