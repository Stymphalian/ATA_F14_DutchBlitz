using UnityEngine;
using System.Collections;
using System.Collections.Generic;


// piles on the field
// these are the stacks of cards on the playing field in which players
// play onto.
public class Pile{
	public int pileId = 0;
	public Card topCard = null;
	
	public Pile(int newPileId){	
		pileId = newPileId;
	}

	public bool IsEmpty(){
		return (topCard == null);
	}

	public bool IsComplete(){
		if( topCard != null){
			return (topCard.number == Card.MAX_NUM);
		}
		return false;
	}

	public string AsString(){
		string s= "[Pile: " + pileId + "," +topCard.AsString() + "]";
		return s;
	}
	
}