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

	//should be used by the UsernameField
	public void username_input()
	{
		var val = Username.text;
		
		//if unusable
		if (string.IsNullOrWhiteSpace(val) || val.Length > state.MaxUserPassLen)
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
		Password.GetComponent<Image>().color = string.IsNullOrWhiteSpace(val) || val.Length > state.MaxUserPassLen? _badData : _goodData;
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
			if (!ClientComms.SendNewUser(_newUserData[0], _newUserData[1])) return;
			
			state.SendNewUser.Written = false;
			SceneManager.LoadScene("Login");
		}
		else
		{
			if (_newUserData[0] == "") Username.GetComponent<Image>().color = _badData;

			if (_newUserData[1] != "") return;
			
			Password.GetComponent<Image>().color = _badData;
			Confirm.GetComponent<Image>().color = _badData;
		}
	}

	// used by the Cancel button
	public void Cancel()
	{
		SceneManager.LoadScene("Login");
	}
}
