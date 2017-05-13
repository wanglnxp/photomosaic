using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testcor : MonoBehaviour {
	List<int> test = new List<int>();
	List<int> result = new List<int>();
//	Texture2D[] textures = new Texture2D[3];
	List<Texture2D> textures = new List<Texture2D>();

	// Use this for initialization
	void Start () {
		StartCoroutine ("coroutMain");
		Debug.Log ("should show last");
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void checkList(){
		
	}
	IEnumerator coroutMain (){
		test.Add (1);
		test.Add (2);
		test.Add (3);
		yield return test;

		foreach (int i in test) {
//			yield return StartCoroutine ("corout");
			StartCoroutine ("corout", i);
		}
//		checkList();
//		yield return new WaitForSeconds (4);
		while (result.Count < 3) {
//			Debug.Log (result.Count);
			yield return new WaitForFixedUpdate ();
		}
		Debug.Log (result.Count);
	}
	IEnumerator corout (int i){
		Debug.Log ("Start");
		string imgURL = "http://i.dailymail.co.uk/i/pix/2016/03/08/22/006F877400000258-3482989-image-a-10_1457476109735.jpg";
		WWW wwwImg = new WWW (imgURL);
		yield return wwwImg;

		Texture2D tex;
		tex = new Texture2D(4, 4, TextureFormat.DXT1, false);
		wwwImg.LoadImageIntoTexture(tex);
		result.Add (2);
		textures.Add(tex);

		Debug.Log ("END");
	}
}
