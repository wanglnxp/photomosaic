using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class BWEffect : MonoBehaviour {

	private Texture _texture;

	public void change (Texture2D test) {
		GetComponent<Renderer> ().material.mainTexture = test;
	}
}