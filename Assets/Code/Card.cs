using UnityEngine;
using System.Collections;
using System.Collections.Generic;


// object to hold information about a card.
public class Card{
	public enum TypeEnum { GREEN=0,BLUE=1,RED=2,YELLOW=3};
	private static Color[] TypeColors = {
		new Color(51.0f/255,246.0f/255,159.0f/255,1.0f),	
		new Color(0.0f,139.0f/255,1.0f,1.0f),	
		new Color(1.0f,45.0f/255,15.0f/255,1.0f),
		new Color(243.0f/255,1.0f,34.0f/255,1.0f)
	};

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

	public Color GetColor(){
		return Card.GetColor(cardType);
	}


	public static Color GetColor(TypeEnum cardType){
		return Card.TypeColors[(int)cardType];
	}
}