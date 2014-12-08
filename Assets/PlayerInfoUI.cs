using UnityEngine;
using System.Collections;

public class PlayerInfoUI : MonoBehaviour {

	// Use this for initialization
	UnityEngine.UI.Text playerLabel;
	UnityEngine.UI.Text playerName;
	UnityEngine.UI.Text pointLabel;
	UnityEngine.UI.Text pointAmount;

	void Awake(){
		playerLabel = this.transform.Find ("PlayerLabel").GetComponent<UnityEngine.UI.Text>();
		playerName = this.transform.Find ("PlayerName").GetComponent<UnityEngine.UI.Text>();
		pointLabel = this.transform.Find ("PointLabel").GetComponent<UnityEngine.UI.Text>();
		pointAmount = this.transform.Find ("PointAmount").GetComponent<UnityEngine.UI.Text>();

	}

	void Start () {
		// hook on to future changes to the score.
		GameStateManager.instance.OnEndRound.Sub(this.gameObject,onEndRoundHandler);
		GameStateManager.instance.OnStartGame.Sub(this.gameObject,onStartGameHandler);
	}

	private void onEndRoundHandler(UnityEngine.GameObject self, System.Object data){
		refresh();
	}
	private void onStartGameHandler(UnityEngine.GameObject self, System.Object data){
		refresh();
	}
	public void refresh(){
		PlayerModel p = GameStateManager.instance.GetPlayerModel();
		playerName.text = p.playerName.ToString();
		pointAmount.text = p.currentScore.ToString();	
	}
}
