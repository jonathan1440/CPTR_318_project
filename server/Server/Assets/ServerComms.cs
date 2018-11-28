using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class ServerComms : MonoBehaviour
{
	private static int current_user; 
		
	public List<Dictionary<string, object>> Users = new List<Dictionary<string, object>>();
	public List<Dictionary<string, object>> Events = new List<Dictionary<string, object>>();
	
	private TcpListener tcpListener;
	private Thread thread;
	private TcpClient client;

	private void Start()
	{
		thread = new Thread(ListenForIncomingRequests);
		thread.IsBackground = true;
		thread.Start();
	}
	
	public int FindUser(string username, string password)
	{
		int i = 0;
		while (i < Users.Count)
		{
			if (Users[i]["username"].ToString() == username && Users[i]["password"].ToString() == password)
			{
				return i;
			}

			i++;
		}

		return -1;
	}
	
	public int AddUser(string username, string password)
	{
		Dictionary<string, object> user = new Dictionary<string, object>
		{
			{"id", Users.Count}, {"username", username}, {"password", password}
		};

		if (FindUser(username, password) == -1)
		{
			Users.Add(user);
			return 1;
		}

		return -1;
	}

	public int FindEvent(string title, string date, string start_time, string end_time)
	{
		int i = 0;
		while (i < Events.Count)
		{
			if(Events[i]["title"].ToString() == title && Events[i]["date"].ToString() == date && Events[i]["start time"].ToString() == start_time && Events[i]["end time"].ToString() == end_time)
			{
				return i;
			}
			
			i++;
		}

		return -1;
	}

	public int EditEvent(string title1, string date1, string start_time1, string end_time1, string title2, string date2, string start_time2, string end_time2)
	{
		int i = FindEvent(title1, date1, start_time1, end_time1);

		if (i == -1)
		{
			return -1;
		}
		
		Dictionary<string, object> evnt = new Dictionary<string, object>
		{
			{"id", i}, {"title", title2}, {"date", date2}, {"start time", start_time2}, {"end time", end_time2},
			{"user", current_user}
		};

		Events[i] = evnt;

		return 1;
	}

	public int AddEvent(string title, string date, string start_time, string end_time)
	{
		Dictionary<string, object> evnt = new Dictionary<string, object>
		{
			{"id", Events.Count}, {"title", title}, {"date", date}, {"start time", start_time}, {"end time", end_time},
			{"user", current_user}
		};

		if (FindEvent(title, date, start_time, end_time) == -1)
		{
			Events.Add(evnt);
			return 1;
		}

		return -1;
	}

	public List<Dictionary<string, object>> GetRange(string start_date, string end_date)
	{
		List<Dictionary<string, object>> events = new List<Dictionary<string, object>>();
		
		string[] sd = start_date.Split('/');
		string[] ed = end_date.Split('/');

		DateObj.DateObject sds = new DateObj.DateObject(int.Parse(sd[2]), int.Parse(sd[0]), int.Parse(sd[1]));
		DateObj.DateObject eds = new DateObj.DateObject(int.Parse(ed[2]), int.Parse(ed[0]), int.Parse(ed[1]));

		foreach (Dictionary<string, object> evnt in Events)
		{
			string[] d = evnt["date"].ToString().Split('/');
			DateObj.DateObject dd = new DateObj.DateObject(int.Parse(d[2]), int.Parse(d[0]), int.Parse(d[1]));

			if (sds <= dd && eds >= dd)
			{
				events.Add(evnt);
			}
		}

		return events;
	}

	private void ListenForIncomingRequests()
	{
		try
		{
			Debug.Log("Need to revisit IP address and port number");
			tcpListener = new TcpListener(IPAddress.Parse("127.0.0.1"), 8888);
			tcpListener.Start();

			Debug.Log("Should revisit this");
			Byte[] bytes = new Byte[60];

			//used to store the first message of the edit event stuff
			string[] stor = {"", "", "", ""};

			while (true)
			{
				using (client = tcpListener.AcceptTcpClient())
				{
					// get a stream object for reading
					using (NetworkStream stream = client.GetStream())
					{
						int length;

						while ((length = stream.Read(bytes, 0, bytes.Length)) != 0)
						{
							var incomingData = new byte[length];
							
							Array.Copy(bytes, 0, incomingData, 0, length);

							string clientMessage = Encoding.ASCII.GetString(incomingData);
							string[] strArr = clientMessage.Split(',');

							switch (strArr[0])
							{
								case "01":
									//search db for user and pass combo, if success:
									int result = FindUser(strArr[1], strArr[2]);
									if (result != -1)
									{
										SendMessage("51," + 1);
									}
									break;
								
								case "02":
									var events = GetRange(strArr[1], strArr[2]);
									
									SendMessage("52,a,"+events.Count);
									
									foreach (Dictionary<string, object> evnt in events)
									{
										SendMessage("52,b," + evnt["title"] + evnt["date"] + evnt["start time"] +
										            evnt["end time"]);
									}

									break;
								
								case "03":
									SendMessage("53,"+AddEvent(strArr[1], strArr[2], strArr[3], strArr[4]));
									break;
								
								case "04":
									if (strArr[1] == "a")
									{
										stor[0] = strArr[2];
										stor[1] = strArr[3];
										stor[2] = strArr[4];
										stor[3] = strArr[5];
									}

									if (strArr[1] == "b")
									{
										SendMessage("54," + EditEvent(stor[0], stor[1], stor[2], stor[3], strArr[2],
											            strArr[3], strArr[4], strArr[5]));
									}

									break;
								
								case "05":
									SendMessage("55,"+AddUser(strArr[1], strArr[2]));
									break;
							}
						}
					}
				}
			}
		}
		catch (SocketException socketException)
		{
			Debug.Log("Socket exception " + socketException.ToString());
		}
	}

	private void SendMessage(string message)
	{
		if (client == null)
		{
			return;
		}

		try
		{
			NetworkStream stream = client.GetStream();

			if (stream.CanWrite)
			{
				byte[] messageAsByteArray = Encoding.ASCII.GetBytes(message);
				
				stream.Write(messageAsByteArray, 0 , messageAsByteArray.Length);
			}
		}
		catch(SocketException socketException)
		{
			Debug.Log("Socket exception " + socketException);
		}
	}
}
