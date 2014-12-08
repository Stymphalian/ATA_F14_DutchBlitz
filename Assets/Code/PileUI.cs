using UnityEngine;
using System.Collections;

public class PileUI : MonoBehaviour {
	
	UnityEngine.UI.Button button;
	UnityEngine.UI.Image buttonImage;
	UnityEngine.UI.Text buttonText;
	Pile pileRef;

	void Awake(){
		button = this.transform.Find("Button").GetComponent<UnityEngine.UI.Button>();
		buttonImage = this.transform.Find("Button").GetComponent<UnityEngine.UI.Image>();
		buttonText = this.transform.Find("Button/Text").GetComponent<UnityEngine.UI.Text>();
	}
	
	void Start(){
		GameStateManager.instance.OnMainToPile.Sub(this.gameObject,refreshPileHandler);
		GameStateManager.instance.OnDiscardToPile.Sub(this.gameObject,refreshPileHandler);
	}

	void refreshPileHandler(UnityEngine.GameObject self, System.Object data){
		if( this.gameObject.activeSelf == false){return;}
		GameStateManager.GameStateEvent ev = (GameStateManager.GameStateEvent)(data);
		int pileId = (int)ev.data;
		if( pileId != pileRef.pileId){ return;}
		refresh();		
	}			   

	void refresh(){
		if( pileRef.topCard != null){
			buttonText.text = pileRef.topCard.number.ToString();
			buttonImage.color = pileRef.topCard.GetColor();
		}else{
			StartCoroutine(lookForTopCard()) ;
		}
	}
	IEnumerator lookForTopCard(){
		while(pileRef.topCard == null){
			yield return new WaitForSeconds(0.1f);
		}
		refresh();
	}

	public void Setup(Pile p){
		pileRef = p;
		refresh();
	}
}
