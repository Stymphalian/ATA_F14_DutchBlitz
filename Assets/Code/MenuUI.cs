using UnityEngine;
using System.Collections;

public class MenuUI : MonoBehaviour {

	// Use this for initialization
	UnityEngine.UI.Text title;

	void Awake(){
		title = this.transform.Find("Title").GetComponent<UnityEngine.UI.Text>();
		title.text = "DutchBlitz";


		// new player, therefore setup a new playerId...
		if( PlayerPrefs.HasKey ("PlayerModel_playerId") == false){
			PlayerPrefs.SetInt("PlayerModel_playerId",UnityEngine.Random.Range (0,int.MaxValue-1));
		}
	}
	
	public void playHandler(){
		NetworkManager.instance.Play (_play);
	}
	public void _play(){
		Application.LoadLevel("instance");
	}

	public void quitGame(){
		title.text = "Quitting";
		Application.Quit();
	}
}
