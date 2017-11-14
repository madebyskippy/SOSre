using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class TutorialBoard : MonoBehaviour {

	Image[] tiles;
	Color[] colors;
	bool goswitchcolor;

	// Use this for initialization
	void Start () {

		colors = new Color[4];
		colors [0] = normalizeColors (255f, 128f, 152f);
		colors [1] = normalizeColors (255f, 249f, 68f);
		colors [2] = normalizeColors (120f, 236f, 179f);
		colors [3] = normalizeColors (79f, 197f, 255f);

		tiles = new Image[4];
		for (int i = 0; i < tiles.Length; i++) {
			tiles [i] = transform.GetChild (i).gameObject.GetComponent<Image> ();;
			tiles [i].color = colors [i];
		}

		StartCoroutine (switchColors ());
	}
	
	// Update is called once per frame
	void Update () {
		if (goswitchcolor) {
			goswitchcolor = false;
			StartCoroutine (switchColors ());

		}
	}

	Color normalizeColors(float r, float g, float b){
		return new Color (r / 255f, g / 255f, b / 255f);

	}

	void GenerateRandomList(List<int> finishedNumbers, List<int> uniqueNumbers, int maxNumbers){
		for(int i = 0; i<maxNumbers; i++){
			uniqueNumbers.Add (i);
		}
		for (int i = 0; i < maxNumbers; i++) {
			int ranNum = uniqueNumbers [Random.Range (0, uniqueNumbers.Count)];
			finishedNumbers.Add (ranNum);
			uniqueNumbers.Remove (ranNum);
		}
	}

	IEnumerator switchColors(){

		List<int> uniqueNumbers = new List<int> ();
		List<int> finishedNumbers = new List<int> ();
		int maxNumbers = 4;
		GenerateRandomList (finishedNumbers, uniqueNumbers, maxNumbers);
		for (int i = 0; i < tiles.Length; i++) {
			tiles [i].color = colors[finishedNumbers [i]];
		}
		yield return new WaitForSeconds (1.0f);
		goswitchcolor = true;
	}
}
