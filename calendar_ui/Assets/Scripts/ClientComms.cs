using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

// adapted from gist.github.com/danielbierwirth/0636650b005834204cb19ef5ae6ccedb

public class ClientComms : MonoBehaviour
{
	private static readonly object Lock = new object();
	private static TcpClient _server;
	private static Thread _clientThread;
	private static NetworkStream _stream; 

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
			state.Connected = true;
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

		while (true)
		{
			Debug.Log("waiting");
			
			//using (var stream = _server.GetStream())
			//{
				int length;
				
				Debug.Log("got stream");

				while ((length = _stream.Read(bytes, 0, bytes.Length)) != 0)
				{
					var incommingData = new byte[length];
					
					Array.Copy(bytes, 0, incommingData, 0, length);
					
					string serverMessage = Encoding.ASCII.GetString(incommingData); 
					string[] strArr = serverMessage.Split(',');

					Debug.Log("new message recieved: " + serverMessage);
					
					switch (strArr[0])
					{
						case "51":
							state.RequestLoginAuth.Data = strArr[1];

							state.RequestLoginAuth.Written = true;
							
							Debug.Log("login auth response received");
							
							break;
							
						case "52":
							switch (strArr[1])
							{
								//convert to list of dictionaries
								case "a":
									events_to_rcv = int.Parse(strArr[2]);
									Debug.Log("events to receive: " + events_to_rcv);
									state.displayable_events.Data.Clear();
									break;
								case "b":
								{
									var evnt = new Dictionary<string, object>
									{
										{"title", strArr[2]},
										{"date", strArr[3]},
										{"start time", strArr[4]},
										{"end time", strArr[5]}
									};

									state.displayable_events.Data.Add(evnt);
									Debug.Log("event added to displayable_events.Data");

									events_rcvd++;
									Debug.Log("total events received: " + events_rcvd);
									
									break;
								}
							}

							if (events_rcvd == events_to_rcv)
							{
								events_rcvd = 0;
								events_to_rcv = 0;

								state.displayable_events.Written = true;
								Debug.Log("Event range received");
							}
							
							break;
						
						case "53":
							state.SendNewEvent.Data = strArr[1];

							state.SendNewEvent.Written = true;
							Debug.Log("new event response received");
							
							break;
							
						case "54":
							state.SendEditedEvent.Data = strArr[1];

							state.SendEditedEvent.Written = true;
							Debug.Log("edited event response received");
							break;
						
						case "55":
							state.SendNewUser.Data = strArr[1];
							Debug.Log(strArr[1]);

							state.SendNewUser.Written = true;
							Debug.Log("new user response processed");
							
							break;
						
						case "56":
							state.TerminateComm.Data = strArr[1];

							state.TerminateComm.Written = true;
							Debug.Log("received response for terminate comm");
							
							_server.GetStream().Close();
							Debug.Log("connection stream closed");
							_server.Close();
							Debug.Log("connection closed");
							
							break;
					}
				}
			//}
		}
	}

	public void SendTcpMessage(string message)
	{
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
}
