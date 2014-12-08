using UnityEngine;
using System.Collections;

public class LobbyUI : MonoBehaviour {
	UnityEngine.UI.Text title;
	UnityEngine.UI.Text waitingText;
	UnityEngine.UI.Text connectedText;
	UnityEngine.UI.Button button;
	int connectedCount = 0;
	int timeDown = 3;// seconds

	void Awake(){
		title = this.transform.Find ("Title").GetComponent<UnityEngine.UI.Text>();
		waitingText = this.transform.Find ("WaitingText").GetComponent<UnityEngine.UI.Text>();
		connectedText = this.transform.Find ("ConnectedText").GetComponent<UnityEngine.UI.Text>();
		button = this.transform.Find ("Button").GetComponent<UnityEngine.UI.Button>();
	}

	// Use this for initialization
	void Start () {
		button.onClick.RemoveAllListeners();
		connectedCount = Network.connections.Length + 1;
		if( Network.isServer){
			title.text = "You are the server";
			connectedText.text = connectedCount.ToString () + " connected";
			button.gameObject.SetActive (true);
			button.onClick.AddListener(startGame);
		}else{
			title.text = "You are a client";
			connectedText.text = connectedCount.ToString () + " connected";
			button.gameObject.SetActive(false);
		}
	}

	[RPC]
	private void startGame(){
		if( Network.isServer){
			Network.maxConnections = 0;
			networkView.RPC("startGame",RPCMode.Others);
		}
		StartCoroutine(tickDownTimer(timeDown));
	}
	private IEnumerator tickDownTimer(int seconds){
		waitingText.text = "Starting the game";
		connectedText.text = seconds.ToString();
		while( seconds > 0){
			yield return new WaitForSeconds(1);
			seconds -= 1;
			connectedText.text = seconds.ToString ();
		}
		this.gameObject.SetActive(false);

		if(Network.isServer){
			GameStateManager.instance.StartGame();
		}
	}
	
	void OnConnectedToServer(){
		Debug.Log ("OnConnectedToServer");
		connectedCount = Network.connections.Length + 1;
		connectedText.text = connectedCount.ToString() + " Connected";
	}

	void OnPlayerConnected(){
		Debug.Log ("OnPlayerConnected");
		connectedCount = Network.connections.Length + 1;
		connectedText.text = connectedCount.ToString() + " Connected";
	}
}
