using UnityEngine;
using System.Collections;

public class Notification : MonoBehaviour {

	// Use this for initialization
	public static Notification instance;
	UnityEngine.UI.Text text;
	UnityEngine.UI.Button button;

	void Awake(){
		Notification.instance = this;
		text = this.transform.Find ("Text").GetComponent<UnityEngine.UI.Text>();
		button = this.transform.Find ("Button").GetComponent<UnityEngine.UI.Button>();

		button.onClick.AddListener(hide);
		this.gameObject.SetActive(false);
	}
	
	public void Message(string msg,float timeout){
		StopAllCoroutines();
		this.gameObject.SetActive(true);
		text.text = msg;
		if( timeout > 0){
			StartCoroutine(timeoutHandler(timeout));
		}
	}

	private IEnumerator timeoutHandler(float timeout){
		yield return new WaitForSeconds(timeout);
		hide();
	}
	private void hide(){
		this.gameObject.SetActive (false);
	}
}
