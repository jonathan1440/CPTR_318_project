using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class ServerComms : MonoBehaviour
{
	private static string current_user;
    private static int MaxReplyLen = 61;
    private bool load_data = true;
		
	public List<Dictionary<string, object>> Users = new List<Dictionary<string, object>>();
	public List<Dictionary<string, object>> Events = new List<Dictionary<string, object>>();
	
	private TcpListener tcpListener;
	private Thread thread;
	private TcpClient client;
    private static NetworkStream _stream;

    private enum MessageType
    {
        LoginRequest = 1,
        EventRequest = 2,
        NewEvent = 3,
        PreEditEvent = 4,
        PostEditEvent = 5,
        NewUser= 6,
        Logout= 7,
        LoginResponse = 51,
        EventResponse = 52,
        EventResponseLen = 53,
        NewEventResponse = 54,
        EditedEventResponse = 55,
        NewUserResponse = 56,
        LogoutResponse = 57
    }

    private void Start()
	{
		thread = new Thread(ListenForIncomingRequests);
		thread.IsBackground = true;
		thread.Start();
        Debug.Log("thread started");
	}

    private void Update()
    {
        if (!load_data) return;

        Users = CSVReader.Read("users");
        Debug.Log("got users");
        Events = CSVReader.Read("events");
        Debug.Log("got events");

        load_data = false;
    }
	
	public string FindUser(string username, string password)
	{
		int i = 0;
		while (i < Users.Count)
		{
            if (Users[i]["username"].ToString() == username && Users[i]["password"].ToString() == password) return (i + 1).ToString();

			i++;
		}
        
		return "-1";
	}
	
	public int AddUser(string username, string password)
	{
		Dictionary<string, object> user = new Dictionary<string, object>
		{
			{"id", (Users.Count + 1)}, {"username", username}, {"password", password}
		};

        // if user doesn't already exist
		if (FindUser(username, password) == "-1")
		{
			Users.Add(user);

            //record users
            var csv = new StringBuilder();
            csv.AppendLine("id,username,password");
            
            foreach (var usr in Users) csv.AppendLine(usr["id"] + "," + usr["username"] + "," + usr["password"]);

            Debug.Log(csv.ToString());

            File.WriteAllText(@"Assets\Resources\users.csv", csv.ToString());

            Debug.Log("Users recorded");

            return 1;
		}

        Debug.Log("user not added");
		return -1;
	}

	public int FindEvent(string title, string date, string start_time, string end_time)
	{
		int i = 0;
		while (i < Events.Count)
		{
			if(Events[i]["title"].ToString() == title && Events[i]["date"].ToString() == date && Events[i]["start time"].ToString() == start_time &&
               Events[i]["end time"].ToString() == end_time && Events[i]["user"] == current_user)
				return i;
			
			i++;
		}
        
		return -1;
	}

	public int EditEvent(string title1, string date1, string start_time1, string end_time1, string title2, string date2, string start_time2, string end_time2)
	{
		int i = FindEvent(title1, date1, start_time1, end_time1);

		if (i == -1)
		{
            Debug.Log("event not found");
			return -1;
		}
		
		Dictionary<string, object> evnt = new Dictionary<string, object>
		{
			{"id", i}, {"title", title2}, {"date", date2}, {"start time", start_time2}, {"end time", end_time2},
			{"user", current_user}
		};

		Events[i] = evnt;

        Debug.Log("event edited");

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
            Debug.Log("event added");
            return 1;
		}

        Debug.Log("event not added");
		return -1;
	}

    public int Logout()
    {
        //record events
        var csv = new StringBuilder();
        csv.AppendLine("id,title,date,start time,end time,user");

        foreach (var evnt in Events) csv.AppendLine(evnt["id"] + "," + evnt["title"] + "," + evnt["date"] + "," + evnt["start time"] + "," + evnt["end time"] + "," + evnt["user"]);

        Debug.Log(csv.ToString());

        File.WriteAllText(@"Assets\Resources\events.csv", csv.ToString());

        load_data = true;

        //reset
        current_user = "0";

        Debug.Log("reset");

        return 1;
    }

	public List<Dictionary<string, object>> GetRange(string start_date, string end_date)
	{
        List<Dictionary<string, object>> events = new List<Dictionary<string, object>>();
		
		string[] sd = start_date.Split('/');
		string[] ed = end_date.Split('/');

		DateObj.DateObject sds = new DateObj.DateObject(int.Parse(sd[2]), int.Parse(sd[0]), int.Parse(sd[1]));
		DateObj.DateObject eds = new DateObj.DateObject(int.Parse(ed[2]), int.Parse(ed[0]), int.Parse(ed[1]));

        Debug.Log("Events length: " + Events.Count);

		foreach (Dictionary<string, object> evnt in Events)
		{
            Debug.Log(evnt["date"]);
			string[] d = evnt["date"].ToString().Split('/');
            Debug.Log(d[0] + "," + d[1] + "," + d[2]);
			var dd = new DateObj.DateObject(int.Parse(d[2]), int.Parse(d[0]), int.Parse(d[1]));

            Debug.Log("made dateobj");
            Debug.Log("event user: " + evnt["user"].ToString());
            Debug.Log("match? " + evnt["user"].ToString() == current_user);

            if (dd >= sds && dd < eds && evnt["user"].ToString() == current_user) events.Add(evnt);
		}

        Debug.Log("past for loop");

		return events;
	}

	private void ListenForIncomingRequests()
	{
		try
		{
			tcpListener = new TcpListener(IPAddress.Any, 9999);
			tcpListener.Start();

			Byte[] bytes = new Byte[MaxReplyLen];

			//used to store the first message of the edit event stuff
			string[] stor = {"", "", "", ""};

			while (true)
			{
                Debug.Log("waiting");

				using (client = tcpListener.AcceptTcpClient())
				{
                    Debug.Log("got connection");

                    if (_stream == null) _stream = client.GetStream();
                    
                    Debug.Log("got stream");
                    int length;

					while ((length = _stream.Read(bytes, 0, bytes.Length)) != 0)
					{
						var incomingData = new byte[length];
							
						Array.Copy(bytes, 0, incomingData, 0, length);

						var clientMessage = Encoding.ASCII.GetString(incomingData);
						var strArr = clientMessage.Split(',');

                        Debug.Log("recieved client message " + clientMessage);

                        List<string> messages = new List<string>();
                        string log_message = "";

                        int temp;
                        if (!int.TryParse(strArr[0], out temp)) return;

						switch ((MessageType) int.Parse(strArr[0]))
						{
							case MessageType.LoginRequest:
                                Debug.Log("login request");
                                
								current_user = FindUser(strArr[1], strArr[2]);
                                Debug.Log("current user is now " + current_user);
                                    
								if (current_user == "-1") messages.Add((int) MessageType.LoginResponse + ",-1");
                                else messages.Add((int) MessageType.LoginResponse + ",1");

                                log_message = MessageType.LoginResponse.ToString();
								break;
								
							case MessageType.EventRequest:
                                Debug.Log("request received");
                                Debug.Log("current user: " + current_user);

                                Debug.Log("start date: " + strArr[1]);
                                Debug.Log("end date: " + strArr[2]);

								var events = GetRange(strArr[1], strArr[2]);
                                Debug.Log("events retrieved");

                                messages.Add(((int) MessageType.EventResponseLen).ToString() + "," + events.Count);

								foreach (Dictionary<string, object> evnt in events)
								{
                                    messages.Add(((int) MessageType.EventResponse).ToString() + "," + evnt["title"].ToString() + "," + evnt["date"].ToString() + "," + evnt["start time"].ToString() + "," + evnt["end time"].ToString());
								}

                                Debug.Log("messages recorded");

                                log_message = MessageType.EventResponse.ToString();
								break;
								
							case MessageType.NewEvent:
                                messages.Add(((int) MessageType.NewEventResponse) + "," + (AddEvent(strArr[1], strArr[2], strArr[3], strArr[4])));

                                log_message = MessageType.NewEventResponse.ToString();
								break;
								
							case MessageType.PreEditEvent:
								stor[0] = strArr[1];
								stor[1] = strArr[2];
								stor[2] = strArr[3];
								stor[3] = strArr[4];
                                break;

                            case MessageType.PostEditEvent:
                                messages.Add(((int) MessageType.EditedEventResponse).ToString() + "," + EditEvent(stor[0], stor[1], stor[2], stor[3], strArr[1], strArr[2], strArr[3], strArr[4]));

                                log_message = MessageType.EditedEventResponse.ToString();
                                break;
								
							case MessageType.NewUser:
                                messages.Add(((int) MessageType.NewUserResponse).ToString() + "," + AddUser(strArr[1], strArr[2]));

                                log_message = MessageType.NewUserResponse.ToString();
								break;

                            case MessageType.Logout:
                                messages.Add(((int) MessageType.LogoutResponse).ToString() + "," + Logout());

                                log_message = MessageType.LogoutResponse.ToString();

                                //client.GetStream().Close();
                                //client.Close();
                                break;
						}

                        if(messages.Count > 0) foreach(string message in messages) SendMessage(message, log_message);
					}
				}
			}
		}
		catch (SocketException socketException)
		{
			Debug.Log("Socket exception " + socketException.ToString());
		}
	}

	private void SendMessage(string message, string log_message)
	{
		if (client == null)
		{
			return;
		}

		try
		{
			if (_stream.CanWrite)
			{
                if(message.Length < MaxReplyLen - 1)
                {
                    message += ",";

                    while (message.Length < MaxReplyLen - 1)
                    {
                        message += " ";
                    }
                }
                message += ",";


				var messageAsByteArray = Encoding.ASCII.GetBytes(message);
				
				_stream.Write(messageAsByteArray, 0 , messageAsByteArray.Length);
                //_stream.Flush();

                Debug.Log(log_message + " sent: " + message);

                if (log_message!= "blank")
                    SendMessage("", "blank");
            }
		}
		catch(SocketException socketException)
		{
			Debug.Log("Socket exception " + socketException);
		}
	}
}
