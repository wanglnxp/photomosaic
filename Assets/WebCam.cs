using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WebCam : MonoBehaviour {
	
	private WebCamTexture webCamTex;

	// Use this for initialization
	void Start () {
		webCamTex = new WebCamTexture();
		webCamTex.Play ();
		GetComponent<Renderer> ().material.mainTexture = webCamTex;
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
