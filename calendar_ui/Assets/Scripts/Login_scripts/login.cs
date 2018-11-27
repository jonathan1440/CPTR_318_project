using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class login : MonoBehaviour {
	
	//should be set to UsernameField 
	public InputField Username;
	//should be set to PasswordField
	public InputField Password;

	//has to be initialized to a full array to enable the check to see if all the values have been supplied before submitting them
	private string[] newEventData = {"", ""};
	
	private readonly Color badData = new Color(0.91f, 0.26f, 0.26f, 255);
	private readonly Color goodData = new Color(1, 1, 1, 255);

	private bool check_string(string val)
	{
		bool bad = true;
		string[] vals = val.Split(' ');
		
		foreach (string p in vals)
		{
			if (p != "")
			{
				bad = false;
				break;
			}
		}

		return bad;
	}

	//should be used by the UsernameField
	public void username_input()
	{
		string val = Username.text;
		
		//if unusable
		if (check_string(val))
			Username.GetComponent<Image>().color = badData;
		else
		{
			Username.GetComponent<Image>().color = goodData;
			newEventData[0] = val;
		}
	}
	
	//should be used by the PasswordField
	public void password_input()
	{
		string val = Password.text;
		
		//if unusable
		if (check_string(val))
			Password.GetComponent<Image>().color = badData;
		else
		{
			Password.GetComponent<Image>().color = goodData;
			newEventData[1] = val;
		}
	}
	
	// used by the Login button
	public void loggin()
	{
		//if all values have been supplied
		if (ArrayUtility.IndexOf(newEventData, "") == -1)
		{
			// send newEventData to database with
			state.Comm.RequestLoginAuth(newEventData[0], newEventData[1]);

			float t = 0;
			while (t < state.Timeout && !state.RequestLoginAuth.written)
			{
				t += Time.deltaTime;
			}

			if (state.RequestLoginAuth.written)
			{
				if (state.RequestLoginAuth.data == "1")
				{
					state.RequestLoginAuth.written = false;
					
					Password.GetComponent<Image>().color = goodData;
					Username.GetComponent<Image>().color = goodData;

					SceneManager.LoadScene("CalendarView");
				}
				else
				{
					Username.GetComponent<Image>().color = badData;
					Password.GetComponent<Image>().color = badData;
				}
			}
		}
		else
		{
			if (newEventData[0] == "")
			{
				Username.GetComponent<Image>().color = badData;
			}

			if (newEventData[1] == "")
			{
				Password.GetComponent<Image>().color = badData;
			}
		}
		
		// for bypassing the network
		//SceneManager.LoadScene("CalendarView");
	}

	// used by the CreateAccount button
	public void create_new()
	{
		SceneManager.LoadScene("CreateAccount");
	}
}
