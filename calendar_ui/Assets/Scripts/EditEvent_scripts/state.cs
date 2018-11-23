using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class state : MonoBehaviour
{

	private static string title, date, start_time, end_time;

	public static string Title
	{
		get { return title; }
		set { title = value; }
	}
	
	public static string Date
	{
		get { return date; }
		set { date = value; }
	}
	
	public static string Start_time
	{
		get { return start_time; }
		set { start_time = value; }
	}
	
	public static string End_time
	{
		get { return end_time; }
		set { end_time = value; }
	}
}
