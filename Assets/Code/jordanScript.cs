using UnityEngine;
using System.Collections;

public class jordanScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void button1(){
//		Notification.instance.Message(GameStateManager.instance.AsString(),-1);
//		string s= GameStateManager.instance.AsString();
//		GameStateManager.instance.CompareStateToServer(PlayerModel.GetPlayerId(),s);
		if( Network.isServer){
			GameStateManager.instance.StartRound(0);
		}
		this.gameObject.SetActive(false);
	}

	public void button2(){
		if(Network.isServer){
			GameStateManager.instance.EndRound();
		}
	}
}
