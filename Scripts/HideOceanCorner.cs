// This game object is a corner of the ocean with displacement waves that is shown in the close-up at start of animation
// This script hides the object after the camera has pulled away from the closeup,
// by moving it down and back, under the main, flat ocean surface, and then, once it's barely visible, setting it to inactive

using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class HideOceanCorner : MonoBehaviour
{
    [SerializeField] private int frameToStartHide;

    private float amountToMoveY = -20f;
    private float amountToMoveZ = 80f;
    private Vector3 initialPos;
    private int frameCounter;
    private IEnumerator hider = null;

    private IEnumerator StartHide(int timeToHide)  // delay in frames before any motion
    {
        while (frameCounter < frameToStartHide+60)
        {
            yield return new WaitForFixedUpdate();

            transform.position = new Vector3(transform.position.x, transform.position.y - 0.33f, transform.position.z + 1.4f);
        }
        gameObject.SetActive(false);
    }

    void Start()
    {
        initialPos = transform.position;
        frameCounter = 0;

        hider = StartHide(frameToStartHide);
        
    }

    void FixedUpdate()
    {
        frameCounter++;
        if (frameCounter == frameToStartHide)
        {
            // Start the coroutine to move this object
            // when done, set inactive
            StartCoroutine(hider);
        }
    }
}
