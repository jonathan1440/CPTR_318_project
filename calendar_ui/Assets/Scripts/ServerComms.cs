using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class ServerComms : MonoBehaviour
{

	private TcpListener tcpListener;
	private Thread thread;
	private TcpClient client;

	private void Start()
	{
		thread = new Thread(ListenForIncomingRequests);
		thread.IsBackground = true;
		thread.Start();
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
									int result = '1';
									if (true)
									{
										SendMessage("51," + result);
									}
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
