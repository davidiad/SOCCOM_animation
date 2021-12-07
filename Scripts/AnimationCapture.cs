using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationCapture : MonoBehaviour {

    public string screenshotKey;
    public string screenshotFileName;
    public int scaleFactor;
    public int startFrame = 0;
    public int lastFrame = 30;
    public bool SaveMovieFrames;
    public int frameToHideObjects;
    public GameObject[] ObjectsToHide;
    public GameObject[] ObjectsToShow;


    [SerializeField]
    private int frameCounter = 0;

	void Start () {
        //screenshotKey = "Fire1"; //for Windows
        foreach (GameObject obj in ObjectsToShow)
        {

            obj.SetActive(false);
        }
    }

    // Use FixedUpdate so that the time elapsed is consistent per frame, for exporting the rendered animation
    void FixedUpdate()
    {
        frameCounter++;
        if (frameCounter == frameToHideObjects)
        {
            foreach (GameObject obj in ObjectsToHide)
            {
                obj.SetActive(false);
            }
            foreach (GameObject obj in ObjectsToShow)
            {
                obj.SetActive(true);
            }
        }
        if (frameCounter > lastFrame) { SaveMovieFrames = false; }

#if UNITY_EDITOR_OSX
        // if (Input.GetKeyDown(screenshotKey) && scaleFactor > 1)
        //{
        //    ScreenCapture.CaptureScreenshot(screenshotFileName + ".png", scaleFactor);
        //}
        //else if (Input.GetKeyDown(screenshotKey))
        //{
        //    ScreenCapture.CaptureScreenshot(screenshotFileName + ".png");
        //    Debug.Log("Capped");
        //}

        if (SaveMovieFrames && frameCounter >= startFrame)
        {
            ScreenCapture.CaptureScreenshot(screenshotFileName + "\\" + screenshotFileName + "_" + frameCounter.ToString("D4") + ".png");
            Debug.Log("Capped: " + frameCounter);
        }




#endif
#if UNITY_EDITOR_WIN
        if (SaveMovieFrames && frameCounter >= startFrame)
        {
            ScreenCapture.CaptureScreenshot( screenshotFileName + "\\" +  screenshotFileName + "_" + frameCounter.ToString("D4") + ".png");

        }

        
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
