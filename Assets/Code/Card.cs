using UnityEngine;
using System.Collections;
using System.Collections.Generic;


// object to hold information about a card.
public class Card{
	public enum TypeEnum { GREEN=0,BLUE=1,RED=2,YELLOW=3};
	public const int MIN_NUM = 1;
	public const int MAX_NUM = 10;
	
	public int number = Card.MIN_NUM;
	public TypeEnum cardType = TypeEnum.GREEN;
	
	public Card(int number, TypeEnum cardType){
		this.number = number;
		this.cardType = cardType;
	}

	public string AsString(){
		string s = "[Card: ";
		s +=  number + "," + cardType.ToString() + "]";	
		return s;
	}
}