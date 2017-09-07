using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransitionData {
	
	public int winner;
	public Color winnerColor;
	
	public TransitionData(){}

	public TransitionData (int winner_, Color winnerColor_){
		winner = winner_;
		winnerColor = winnerColor_;
	}
}
