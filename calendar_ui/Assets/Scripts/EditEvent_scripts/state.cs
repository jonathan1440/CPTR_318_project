using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class state : MonoBehaviour
{

	private static string title, date, start_time, end_time;
	
	private static string server_ip = "127.0.0.1";
	private static int server_port = 8888;
	private static int timeout = 20;
	private static int max_date_title_len = 30;
	private static int max_user_pass_len = 20;
	private static int max_reply_len = 60;
	private static ClientComms comm = new ClientComms();
	
	public struct rcvd_data
	{
		public object data;
		public bool written;
		public bool read;
	}

	public static rcvd_data RequestLoginAuth, SendNewEvent, SendEditedEvent, 
		SendNewUser;


	public struct events
	{
		public List<Dictionary<string, object>> data;
		public bool written;
		public bool read;
	}
	
	public static events displayable_events;
	
	private void Start()
	{
		displayable_events.data = new List<Dictionary<string, object>>();
	}


	public static int MaxUserPassLen
	{
		get { return max_user_pass_len; }
	}

	public static int MaxReplyLen
	{
		get { return max_reply_len; }
	}

	public static int MaxDateTitleLen
	{
		get { return max_date_title_len; }
	}

	public static ClientComms Comm
	{
		get { return comm; }
	}

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

	public static string Server_ip
	{
		get { return server_ip; }
	}

	public static int Server_port
	{
		get { return server_port; }
	}

	public static int Timeout
	{
		get { return timeout; }
		set { timeout = value; }
	}
}
