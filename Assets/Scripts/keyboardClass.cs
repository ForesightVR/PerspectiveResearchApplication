using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Runtime.InteropServices;
using System;
using TMPro;

public class keyboardClass : MonoBehaviour, ISelectHandler {

    public string promptHeader;

	[DllImport("__Internal")]
	private static extern void focusHandleAction (string _name, string _str);

	public void ReceiveInputData(string value) {
		gameObject.GetComponent<TMP_InputField> ().text = value;
	}

	public void OnSelect(BaseEventData data) {
		#if UNITY_WEBGL
		try{
			focusHandleAction (gameObject.name, gameObject.GetComponent<TMP_InputField> ().text);
		}
		catch(Exception error){}
		#endif
	}
}
