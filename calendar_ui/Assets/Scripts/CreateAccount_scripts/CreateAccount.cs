using System.Diagnostics;
using System.Linq;
using System.Threading;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

public class CreateAccount : MonoBehaviour {

	//should be set to UsernameField 
	public InputField Username;
	//should be set to PasswordField
	public InputField Password;
	//should be set to ConfirmField
	public InputField Confirm;

	//has to be initialized to a full array to enable the check to see if all the values have been supplied before submitting them
	private readonly string[] _newUserData = {"", ""};
	
	private readonly Color _badData = new Color(0.91f, 0.26f, 0.26f, 255);
	private readonly Color _goodData = new Color(1, 1, 1, 255);

	private static bool check_string(string val)
	{
		var vals = val.Split(' ');

		// if any part of vals equals "" or val is too long, then the value is bad
		return vals.All(p => p == "") || val.Length > state.MaxUserPassLen;
	}

	//should be used by the UsernameField
	public void username_input()
	{
		var val = Username.text;
		
		//if unusable
		if (check_string(val))
			Username.GetComponent<Image>().color = _badData;
		else
		{
			Username.GetComponent<Image>().color = _goodData;
			_newUserData[0] = val;
		}
	}
	
	//should be used by the PasswordField
	public void password_input()
	{
		var val = Password.text;
		
		//if unusable, set color to _badData, else set to _goodData
		Password.GetComponent<Image>().color = check_string(val) ? _badData : _goodData;
	}

	//should be used by the ConfirmField
	public void confirm_input()
	{
		if (Confirm.text != Password.text)
		{
			Password.GetComponent<Image>().color = _badData;
			Confirm.GetComponent<Image>().color = _badData;
		}
		else
		{
			Password.GetComponent<Image>().color = _goodData;
			Confirm.GetComponent<Image>().color = _goodData;
			_newUserData[1] = Password.text;
		}
	}
	
	// used by the Save button
	public void create_account()
	{
		Debug.Log("save new account button pressed");
		
		//if all values have been supplied
		if (ArrayUtility.IndexOf(_newUserData, "") == -1)
		{
			// send newUserData to database
			state.Comm.SendTcpMessage("05," + _newUserData[0] + "," + _newUserData[1]);
			
			var stopwatch = new Stopwatch();
			stopwatch.Start();
			while (!state.SendNewUser.Written && stopwatch.ElapsedMilliseconds < state.Timeout)
			{
				Thread.Sleep(200);
				
				Debug.Log("waiting for write for " + stopwatch.ElapsedMilliseconds);
				if (stopwatch.ElapsedMilliseconds < state.Timeout) continue;
				Debug.Log("timeout");
			}
			stopwatch.Stop();
			
			Debug.Log("response " + state.SendNewUser.Data);
			if (state.SendNewUser.Data == "1")
			{
				state.SendNewUser.Written = false;
				
				SceneManager.LoadScene("Login");
			}
			Debug.Log("written? " + state.SendNewUser.Written);
		}
		else
		{
			if (_newUserData[0] == "")
			{
				Username.GetComponent<Image>().color = _badData;
			}

			if (_newUserData[1] == "")
			{
				Password.GetComponent<Image>().color = _badData;
				Confirm.GetComponent<Image>().color = _badData;
			}
		}
		
		// for bypassing the networking stuff
		//SceneManager.LoadScene("Login");
	}

	// used by the Cancel button
	public void Cancel()
	{
		SceneManager.LoadScene("Login");
	}
}
