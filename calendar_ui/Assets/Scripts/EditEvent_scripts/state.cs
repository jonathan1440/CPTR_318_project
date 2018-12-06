using System.Collections.Generic;
using UnityEngine;

public class state : MonoBehaviour
{
	private const string ServerIp = /*"192.168.2.152";*/"10.9.167.225";
	private const int ServerPort = 9999;
	private const int _timeout = 10000;
	private const int max_date_title_len = 30;
	private const int max_user_pass_len = 20;
	private const int max_reply_len = 61;

	public static RcvdData RequestLoginAuth = new RcvdData();
	public static RcvdData SendNewEvent = new RcvdData();
	public static RcvdData SendEditedEvent = new RcvdData();
	public static RcvdData SendNewUser = new RcvdData();
	public static RcvdData TerminateComm = new RcvdData();

	public static evnts displayable_events = new evnts();
	
	public abstract class DataStorage
	{
		public bool Written;
	}
	
	public class RcvdData : DataStorage
	{
		public string Data;
	}
	
	public class evnts : DataStorage
	{
		public List<Dictionary<string, object>> Data = new List<Dictionary<string, object>>();
	}
	
	
	private void Start()
	{
		RequestLoginAuth.Data = "";
		SendNewEvent.Data = "";
		SendEditedEvent.Data = "";
		SendNewUser.Data = "";
		TerminateComm.Data = "";
	}


	public static bool Connected { get; set; }

	public static int MaxUserPassLen => max_user_pass_len;

	public static int MaxReplyLen => max_reply_len;

	public static int MaxDateTitleLen => max_date_title_len;

	public static ClientComms Comm { get; } = new ClientComms();

	public static string Title { get; set; }

	public static string Date { get; set; }

	public static string Start_time { get; set; }

	public static string End_time { get; set; }

	public static string Server_ip => ServerIp;

	public static int Server_port => ServerPort;

	public static int Timeout => _timeout;
}
