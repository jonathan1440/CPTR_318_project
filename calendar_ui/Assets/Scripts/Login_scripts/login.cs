using System.Diagnostics;
using System.Linq;
using System.Threading;
using UnityEditor;
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

	//should be used by the UsernameField
	public void username_input()
	{
		var val = Username.text;
		
		//if unusable
		if (string.IsNullOrWhiteSpace(val)) Username.GetComponent<Image>().color = _badData;
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
		if (string.IsNullOrWhiteSpace(val)) Password.GetComponent<Image>().color = _badData;
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
			
			
			if (ClientComms.SendLoginRequest(_newEventData[0], _newEventData[1]))
			{
				state.RequestLoginAuth.Written = false;
					
				Password.GetComponent<Image>().color = _goodData;
				Username.GetComponent<Image>().color = _goodData;

				SceneManager.LoadScene("CalendarView");
			}
			else if(!ClientComms.SendLoginRequest(_newEventData[0], _newEventData[1]))
			{
				Username.GetComponent<Image>().color = _badData;
				Password.GetComponent<Image>().color = _badData;
			}
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

	private void Update()
	{
		
		
		if (state.Connected) return;
		Debug.Log("inside if");
		state.Comm.Connect();
		Debug.Log("called connect");
		state.Connected = true;
		Debug.Log(state.Connected);
	}
}
