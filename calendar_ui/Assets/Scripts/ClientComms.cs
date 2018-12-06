using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using Debug = UnityEngine.Debug;

// adapted from gist.github.com/danielbierwirth/0636650b005834204cb19ef5ae6ccedb

public class ClientComms : MonoBehaviour
{
	private static readonly object Lock = new object();
	private static TcpClient _server;
	private static Thread _clientThread;
	private static NetworkStream _stream;

	private enum MessageType
	{
		LoginRequest = 1,
		EventRequest = 2,
		NewEvent = 3,
		PreEditEvent = 4,
		PostEditEvent = 5,
		NewUserRequest = 6,
		LogoutRequest = 7,
		LoginResponse = 51,
		EventResponse = 52,
		EventResponseLen = 53,
		NewEventResponse = 54,
		EditedEventResponse = 55,
		NewUserResponse = 56,
		LogoutResponse = 57
	}

	public void Connect()
	{
		try
		{
			_clientThread = new Thread(Execute) {IsBackground = true};
			_clientThread.Start();

			Debug.Log("thread created");

			//SetupSocket();
		}
		catch (Exception e)
		{
			Debug.Log("On client connect exception " + e);
		}
	}

	private static void SetupSocket()
	{		
		lock (Lock)
		{
			Debug.Log("ip " + state.Server_ip);
			Debug.Log("port " + state.Server_port);
			
			_server = new TcpClient(state.Server_ip, state.Server_port);
			state.Connected = _server.Connected;
			Debug.Log("Connection started");
			
			_stream = _server.GetStream();
		}
	}

	private static void Execute()
	{
		if (_server == null) SetupSocket();
		if (!_server.Connected) SetupSocket();

		var bytes = new byte[state.MaxReplyLen];
		
		var events_rcvd = 0;
		var events_to_rcv = 0;
		
		Debug.Log("executing");

		var wait = true;

		while (wait)
		{
			Debug.Log("waiting");

			int length;

			while ((length = _stream.Read(bytes, 0, bytes.Length)) != 0)
			{
				var incommingData = new byte[length];

				Array.Copy(bytes, 0, incommingData, 0, length);

				var serverMessage = Encoding.ASCII.GetString(incommingData);
				var strArr = serverMessage.Split(',');

				Debug.Log("new message received: " + serverMessage);

				int temp;
				if (!int.TryParse(strArr[0], out temp)) continue;
				Debug.Log("signal phrase " + int.Parse(strArr[0]));
				Debug.Log((MessageType) int.Parse(strArr[0]));
				
				switch ((MessageType) int.Parse(strArr[0]))
				{
					case MessageType.LoginResponse:
						state.RequestLoginAuth.Data = strArr[1];

						state.RequestLoginAuth.Written = true;

						Debug.Log("login auth response received");

						break;

					case MessageType.EventResponseLen:
						events_to_rcv = int.Parse(strArr[1]);
						Debug.Log("events to receive: " + events_to_rcv);
						
						state.displayable_events.Data.Clear();
						break;

					case MessageType.EventResponse:
						var evnt = new Dictionary<string, object>
						{
							{"title", strArr[1]},
							{"date", strArr[2]},
							{"start time", strArr[3]},
							{"end time", strArr[4]}
						};

						state.displayable_events.Data.Add(evnt);
						Debug.Log("event added to displayable_events.Data");

						events_rcvd++;
						Debug.Log("total events received: " + events_rcvd);

						if (events_rcvd == events_to_rcv)
						{
							events_rcvd = 0;
							events_to_rcv = 0;

							state.displayable_events.Written = true;
							Debug.Log("Event range received");
						}

						break;

					case MessageType.NewEventResponse:
						state.SendNewEvent.Data = strArr[1];

						state.SendNewEvent.Written = true;
						Debug.Log("new event response received");

						break;

					case MessageType.EditedEventResponse:
						state.SendEditedEvent.Data = strArr[1];

						state.SendEditedEvent.Written = true;
						Debug.Log("edited event response received");
						break;

					case MessageType.NewUserResponse:
						state.SendNewUser.Data = strArr[1];
						Debug.Log(strArr[1]);

						state.SendNewUser.Written = true;
						Debug.Log("new user response processed");

						break;

					case MessageType.LogoutResponse:
						state.TerminateComm.Data = strArr[1];

						state.TerminateComm.Written = true;
						Debug.Log("received response for terminate comm");

						//_server.GetStream().Close();
						Debug.Log("connection stream closed");
						//_server.Close();
						Debug.Log("connection closed");

						wait = false;

						break;
				}
			}
		}
		
		//_clientThread.Abort();
	}

	private static void SendTcpMessage(MessageType messageType, params string[] messageParts)
	{
		var message = (int) messageType + "," + string.Join(",", messageParts);
		
		Debug.Log("message to send: " + message);
		
		if (_server == null) return;
		if (!_server.Connected) return;
		
		try
		{
			Debug.Log(_stream.CanWrite);
			if (!_stream.CanWrite) return;
			
			Debug.Log("Can write to stream");
			
			//add to message until up to necessary length
			if (message.Length < state.MaxReplyLen-1)
			{
				message += ",";
					
				while (message.Length < state.MaxReplyLen - 1)
				{
					message += " ";
				}
			}
			message += ",";                            
				
			Debug.Log("message padded");
				
			//turn message from string into bytes
			var clientMessageAsByteArray = Encoding.ASCII.GetBytes(message);
			Debug.Log("message encoded");
				
			//write byte array to socket stream
			_stream.Write(clientMessageAsByteArray, 0, clientMessageAsByteArray.Length);
			_stream.Flush();
			
			Debug.Log("message written to stream: " + message);
		}
		catch (SocketException socketException)
		{
			Debug.Log("Socket exception: " + socketException);
		}
	}

	private static void Waiting(state.DataStorage dataStorageLocation)
	{
		var stopwatch = new Stopwatch();
		stopwatch.Start();
		while (!dataStorageLocation.Written && stopwatch.ElapsedMilliseconds < state.Timeout)
		{
			Thread.Sleep(200);
				
			Debug.Log("waiting for write for " + stopwatch.ElapsedMilliseconds);
			if (stopwatch.ElapsedMilliseconds < state.Timeout) continue;
			Debug.Log("timeout");
		}
		stopwatch.Stop();
		
		Debug.Log($"Waiting() says data is {(dataStorageLocation.Written ? "" : "not")} written");
	}
	
	public static bool SendNewUser(string username, string password)
	{
		SendTcpMessage(MessageType.NewUserRequest, username, password);

		Waiting(state.SendNewUser);
		
		Debug.Log($"Send___() says data is {(state.SendNewUser.Written ? "" : "not")} written");
		return state.SendNewUser.Data == "1";
	}

	public void SendPreEditEvent(string title, string date, string startTime, string endTime)
	{
		SendTcpMessage(MessageType.PreEditEvent, title, date, startTime, endTime);
	}

	public static bool SendPostEditEvent(string title, string date, string startTime, string endTime)
	{
		SendTcpMessage(MessageType.PostEditEvent, title, date, startTime, endTime);
		
		Waiting(state.SendEditedEvent);
		
		Debug.Log($"Send___() says data is {(state.SendEditedEvent.Written ? "" : "not")} written");
		return state.SendEditedEvent.Data == "1";
	}

	public static bool SendNewEvent(string title, string date, string startTime, string endTime)
	{
		SendTcpMessage(MessageType.NewEvent, title, date, startTime, endTime);
		
		Waiting(state.SendNewEvent);
		
		Debug.Log($"Send___() says data is {(state.SendNewEvent.Written ? "" : "not")} written");
		return state.SendNewEvent.Data == "1";
	}

	public static bool SendLoginRequest(string username, string password)
	{
		SendTcpMessage(MessageType.LoginRequest, username, password);
		
		Waiting(state.RequestLoginAuth);

		Debug.Log($"Send___() says data is {(state.RequestLoginAuth.Written ? "" : "not")} written");
		return state.RequestLoginAuth.Data == "1";
	}

	public static bool SendEventRequest(string startDate, string endDate)
	{
		SendTcpMessage(MessageType.EventRequest, startDate, endDate);
		
		Waiting(state.displayable_events);

		Debug.Log($"Send___() says data is {(state.displayable_events.Written ? "" : "not")} written");
		return state.displayable_events.Written;
	}

	public static bool SendLogout()
	{
		SendTcpMessage(MessageType.LogoutRequest);
		
		Waiting(state.TerminateComm);

		Debug.Log($"Send___() says data is {(state.TerminateComm.Written ? "" : "not")} written");
		return state.TerminateComm.Data == "1";
	}
}
