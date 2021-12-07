using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenCap : MonoBehaviour {

    public string screenshotKey;
    public string screenshotFileName;
    public int scaleFactor;

	void Start () {
        //screenshotKey = "Fire1"; //for Windows
	}
	
    void Update()
    {
#if UNITY_EDITOR_OSX
         if (Input.GetKeyDown(screenshotKey) && scaleFactor > 1)
        {
            ScreenCapture.CaptureScreenshot(screenshotFileName + ".png", scaleFactor);
        }
        else if (Input.GetKeyDown(screenshotKey))
        {
            ScreenCapture.CaptureScreenshot(screenshotFileName + ".png");
            Debug.Log("Capped");
        }
#endif
#if UNITY_EDITOR_WIN
        if (Input.GetButton(screenshotKey) && scaleFactor > 1)
        {
            ScreenCapture.CaptureScreenshot(screenshotFileName + ".png", scaleFactor);
        }
        else if (Input.GetButton(screenshotKey))
        {
            ScreenCapture.CaptureScreenshot(screenshotFileName + ".png");
            Debug.Log("Capped");
        }
#endif
    }

}
