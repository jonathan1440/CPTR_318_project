using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class state : MonoBehaviour
{

	private static string _title, _date, _startTime, _endTime;
	private static bool _connected;

	private const string ServerIp = /*"192.168.2.152";//*/"10.9.167.225";
	private const int ServerPort = 9999;
	private const int _timeout = 10000;
	private const int max_date_title_len = 30;
	private const int max_user_pass_len = 20;
	private const int max_reply_len = 61;
	private static ClientComms comm = new ClientComms();
	
	public struct RcvdData
	{
		public string Data;
		public bool Written;
	}

	public static RcvdData RequestLoginAuth, SendNewEvent, SendEditedEvent, SendNewUser, TerminateComm;
	
	
	
	public class evnts
	{
		public List<Dictionary<string, object>> Data = new List<Dictionary<string, object>>();
		public bool Written;
	}

	public static readonly evnts displayable_events = new evnts();
	
	private void Start()
	{
		RequestLoginAuth.Data = "";
		SendNewEvent.Data = "";
		SendEditedEvent.Data = "";
		SendNewUser.Data = "";
		TerminateComm.Data = "";
	}

	private void Update()
	{
		/*if (SendNewUser.data == "1")
		{
			SendNewUser.written = false;
			SceneManager.LoadScene("Login");
		}*/
	}


	public static bool Connected
	{
		get { return _connected; }
		set { _connected = value; }
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
		get { return _title; }
		set { _title = value; }
	}
	
	public static string Date
	{
		get { return _date; }
		set { _date = value; }
	}
	
	public static string Start_time
	{
		get { return _startTime; }
		set { _startTime = value; }
	}
	
	public static string End_time
	{
		get { return _endTime; }
		set { _endTime = value; }
	}

	public static string Server_ip
	{
		get { return ServerIp; }
	}

	public static int Server_port
	{
		get { return ServerPort; }
	}

	public static int Timeout
	{
		get { return _timeout; }
	}
}
