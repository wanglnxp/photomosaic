using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine;
using UnityEditor;

public class Combined : MonoBehaviour
{
	public PhotoMosaic PMscript;
	public BWEffect BWscript;

	public string username;
	private string filepath = "Assets/PhotoMosaic/Album/";
	List<Texture2D> downloaded = new List<Texture2D>();

	// downloads all Instagram media for a user
	public void DownloadMedia ()
	{
		IEnumerator coroutine = DownloadMediaForUsername (username);
		StartCoroutine (coroutine);
	}

//	private void CleanDirectory(){
//		System.IO.Directory.CreateDirectory(filepath);
//		System.IO.DirectoryInfo di = new DirectoryInfo(filepath);
//
//		foreach (FileInfo file in di.GetFiles())
//		{
//			file.Delete(); 
//		}
//		foreach (DirectoryInfo dir in di.GetDirectories())
//		{
//			dir.Delete(true); 
//		}
//	}

	private IEnumerator DownloadMediaForUsername (string username)
	{
		downloaded.Clear ();
		Debug.Log ("Downloading Instagram media for user " + username);

		// could probably do it faster by pulling each page as a coroutine
		topLevelData data;
		string queryOptions = "";
		int count = 0;
		do {
			// build API request
			string url = "https://www.instagram.com/" + username + "/media/" + queryOptions;

			// grab the response JSON
			WWW www = new WWW (url);
			yield return www;
			string json = www.text;
			data = JsonUtility.FromJson<topLevelData> (json);
			Debug.Log ("Response status: " + data.status);
			Debug.Log ("More data available: " + data.more_available);

			// download each image in the response. 
			// Currently getting the low-res 320x320 preview for each image and video. 
			// To pull higher-res images or actual video files, change the arguments below.
			foreach (items item in data.items) {
				count += 1;
				string imgURL = item.images.low_resolution.url;
				Debug.Log ("Downloading image: " + imgURL);
				IEnumerator imgDownloadCoroutine = DownloadImageFromURL (imgURL);
				StartCoroutine (imgDownloadCoroutine);
				if (count > 200)
					break;
			}
				
			//yield 
			string lastItemID = data.items [data.items.Count - 1].id;
			queryOptions = "?max_id=" + lastItemID;

			if (count > 200) {
				break;
			}
			// if there are more images, pull the next page
		} while (data.more_available);

		while (downloaded.Count < count) {
			yield return new WaitForFixedUpdate ();
		}
		Debug.Log ("Finish ============");

		string[] folders = {"Assets/PhotoMosaic/Album"};
		var guids = AssetDatabase.FindAssets("t:Texture2D", folders);
		var textures = new Texture2D[downloaded.Count];
	

		Debug.Log(downloaded.Count);
		for (var i = 0; i < downloaded.Count; i++) {
			textures [i] = downloaded[i];
		}

		//		var photos = textures;
		var rowCount = (textures.Length + _photosPerRow - 1) / _photosPerRow;
		Debug.Log ("updated0");
		var album = new Texture2D(_albumWidth, _photoWidth * rowCount);
		var photoColors = new Vector3[textures.Length];
		Debug.Log ("updated1");
		for (var i = 0; i < textures.Length; i++)
		{
			var column = i % _photosPerRow;
			var row = i / _photosPerRow;
			Blit(textures[i], album, column * _photoWidth, row * _photoWidth);
			photoColors[i] = GetAverageColor(textures[i]);
		}

//		var lut = new Texture2D();
		var lut = new Texture2D(_lutWidth * _lutWidth, _lutWidth);
		lut = CreateLut (photoColors);


		album = ExportAndReturn(album, "AlbumTexture.png");
		lut = ExportAndReturn(lut, "AlbumLut.png");
//		ExportAndDestroy(lut, "AlbumLut.png");
		PMscript.UpdateTex (album, lut);
//		BWscript.change (album);
//		Object.DestroyImmediate(album);
//		Object.DestroyImmediate(lut);
		Debug.Log ("updated");
	}

	// save to file the image at imgURL
	private IEnumerator DownloadImageFromURL (string imgURL)
	{
		Debug.Log ("Downloading image: " + imgURL);
		WWW wwwImg = new WWW (imgURL);
		yield return wwwImg;			

		if(wwwImg.error != null)
			Debug.Log ("Error: " + wwwImg.error);
		else {
			Debug.Log ("File was successfully download online.");
			Texture2D tex;
			tex = new Texture2D(4, 4, TextureFormat.DXT1, false);
			wwwImg.LoadImageIntoTexture(tex);
			//		tex.Resize (256, 256);
			downloaded.Add (tex);
			//		BWscript.change (tex);

			//		byte[] imgBytes = wwwImg.bytes;
			//		
			//		string fileName = Path.GetFileName (imgURL);
			//		
			//		File.WriteAllBytes (filepath + fileName, imgBytes);
			wwwImg.Dispose();
			wwwImg = null;
		} 
	}

	const int _albumWidth = 4096;
	const int _photoWidth = 320;
	const int _photosPerRow = _albumWidth / _photoWidth;
	const int _lutWidth = 16;

	static Texture2D ExportAndReturn(Texture2D texture, string filename)
	{
		var path = Path.Combine ("Assets/PhotoMosaic/Texture", filename);
		File.WriteAllBytes(path, texture.EncodeToPNG());
		byte[] pngBytes = texture.EncodeToPNG ();

		texture.LoadImage(pngBytes);
		return texture;
//		Object.DestroyImmediate(texture);
	}

	static void Blit(Texture2D src, Texture2D dst, int ox, int oy)
	{
		dst.SetPixels32(ox, oy, src.width, src.height, src.GetPixels32());
	}

	static Vector3 GetAverageColor(Texture2D texture)
	{
		var sum = Vector4.zero;
		for (var y = 0; y < texture.height; y++)
			for (var x = 0; x < texture.width; x++)
				sum += (Vector4)texture.GetPixel(x, y);
		return sum / (texture.width * texture.height);
	}

	static int SearchNearestColor(Vector3[] colors, Vector3 target)
	{
		var min_d = 1.0f;
		var min_i = -1;

		for (var i = 0; i < colors.Length; i++)
		{
			var d = (colors[i] - target).magnitude;
			if (d < min_d)
			{
				min_d = d;
				min_i = i;
			}
		}

		return min_i;
	}

	static Texture2D CreateLut(Vector3[] colors)
	{
		var texture = new Texture2D(_lutWidth * _lutWidth, _lutWidth);
		var rowCount = (colors.Length + _photosPerRow - 1) / _photosPerRow;

		for (var x = 0; x < _lutWidth; x++)
		{
			for (var y = 0; y < _lutWidth; y++)
			{
				for (var z = 0; z < _lutWidth; z++)
				{
					var c = new Vector3(x, y, z) / _lutWidth;
					var i = SearchNearestColor(colors, c);
					var r = (float)(i % _photosPerRow) / _photosPerRow;
					var g = (float)(i / _photosPerRow) / rowCount;
					texture.SetPixel(x + z * _lutWidth, y, new Color(r, g, 0, 1));
				}
			}
		}

		return texture;
	}
		
}

// for JSON parsing
[System.Serializable]

public struct topLevelData
{
	public string status;
	public bool more_available;
	public List<items> items;
}

[System.Serializable]
public class items
{
	public string id;
	public images images;

}

[System.Serializable]
public class images
{
	public lowResolution low_resolution;

}

[System.Serializable]
public class lowResolution
{
	public int height;
	public int width;
	public string url;

}
