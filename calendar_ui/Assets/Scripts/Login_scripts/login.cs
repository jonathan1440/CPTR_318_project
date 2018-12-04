using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

public class login : MonoBehaviour {
	
	//should be set to UsernameField 
	public InputField Username;
	//should be set to PasswordField
	public InputField Password;

	//has to be initialized to a full array to enable the check to see if all the values have been supplied before submitting them
	private readonly string[] _newEventData = {"", ""};
	
	private readonly Color _badData = new Color(0.91f, 0.26f, 0.26f, 255);
	private readonly Color _goodData = new Color(1, 1, 1, 255);

	private static bool check_string(string val)
	{
		var vals = val.Split(' ');

		// if any part of vals equals "" then the value is bad
		return vals.All(p => p == "");
	}

	//should be used by the UsernameField
	public void username_input()
	{
		var val = Username.text;
		
		//if unusable
		if (check_string(val)) Username.GetComponent<Image>().color = _badData;
		else
		{
			Username.GetComponent<Image>().color = _goodData;
			_newEventData[0] = val;
		}
	}
	
	//should be used by the PasswordField
	public void password_input()
	{
		var val = Password.text;
		
		//if unusable
		if (check_string(val)) Password.GetComponent<Image>().color = _badData;
		else
		{
			Password.GetComponent<Image>().color = _goodData;
			_newEventData[1] = val;
		}
	}
	
	// used by the Login button
	public void loggin()
	{
		//if all values have been supplied
		if (ArrayUtility.IndexOf(_newEventData, "") == -1)
		{
			// send newEventData to database
			state.Comm.SendTcpMessage("01," + _newEventData[0] + "," + _newEventData[1]);
		
			Debug.Log("message sent");
			
			var stopwatch = new Stopwatch();
			stopwatch.Start();
			while (!state.RequestLoginAuth.Written && stopwatch.ElapsedMilliseconds < state.Timeout)
			{
				Thread.Sleep(200);
				
				Debug.Log("written? " + state.RequestLoginAuth.Written);
				Debug.Log("waiting for write for " + stopwatch.ElapsedMilliseconds);
				if (stopwatch.ElapsedMilliseconds < state.Timeout) continue;
				Debug.Log("timeout");
			}
			stopwatch.Stop();
			
			Debug.Log("response " + state.RequestLoginAuth.Data);
			Debug.Log("Written? " + state.RequestLoginAuth.Written);
			if (!state.RequestLoginAuth.Written) return;
			if (state.RequestLoginAuth.Data == "1")
			{
				state.RequestLoginAuth.Written = false;
					
				Password.GetComponent<Image>().color = _goodData;
				Username.GetComponent<Image>().color = _goodData;

				SceneManager.LoadScene("CalendarView");
			}
			else
			{
				Username.GetComponent<Image>().color = _badData;
				Password.GetComponent<Image>().color = _badData;
			}
			Debug.Log("written? " + state.SendNewUser.Written);
		}
		else
		{
			if (_newEventData[0] == "") Username.GetComponent<Image>().color = _badData;

			if (_newEventData[1] == "") Password.GetComponent<Image>().color = _badData;
		}
		
		// for bypassing the networking stuff
		//SceneManager.LoadScene("CalendarView");
	}

	// used by the CreateAccount button
	public void create_new()
	{
		SceneManager.LoadScene("CreateAccount");
	}

	private void Start()
	{
		Debug.Log("Login scene started");
		Debug.Log(state.Connected);
		
		if (state.Connected) return;
		Debug.Log("inside if");
		state.Comm.Connect();
		Debug.Log("called connect");
		state.Connected = true;
		Debug.Log(state.Connected);
	}
}
