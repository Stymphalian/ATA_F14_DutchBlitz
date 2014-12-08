using UnityEngine;
using System.Collections;

public class InterfaceUI : MonoBehaviour {

	// Use this for initialization
//	UnityEngine.GameObject gameBoard;
	UnityEngine.GameObject lobby;
	void Awake(){
//		gameBoard = this.transform.Find ("GameBoard").gameObject;
		lobby = this.transform.Find ("Lobby").gameObject;

		// place the lobby in the correct position..
		(lobby.transform as RectTransform).offsetMax = Vector2.zero;
		(lobby.transform as RectTransform).offsetMin = Vector2.zero;
	}

	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}	
}
