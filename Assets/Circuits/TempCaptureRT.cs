using System.IO;
using UnityEngine;
 
public class TempCaptureRT : MonoBehaviour {
 
	public int FileCounter = 0;
 
	private void LateUpdate()
	{
		if (Input.GetKeyDown(KeyCode.F9))
		{
			CamCapture();  
		}
	}
 
	void CamCapture() {

		
		
		ScreenCapture.CaptureScreenshot(@"C:\Files\Programming\UnityProjects\DesktopPortal\Assets\Backgrounds\yay.png");

		return;
		
		Camera Cam = GetComponent<Camera>();
 
		RenderTexture currentRT = RenderTexture.active;
		RenderTexture.active = Cam.targetTexture;

		Cam.Render();
 
		Texture2D Image = new Texture2D(Cam.targetTexture.width, Cam.targetTexture.height);
		Image.ReadPixels(new Rect(0, 0, Cam.targetTexture.width, Cam.targetTexture.height), 0, 0);
		Image.Apply();
		RenderTexture.active = currentRT;
 
		var Bytes = Image.EncodeToPNG();
		Destroy(Image);
 
		File.WriteAllBytes(Application.dataPath + "/Backgrounds/" + FileCounter + ".png", Bytes);
		FileCounter++;
	}
   
}