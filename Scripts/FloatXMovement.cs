 using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class FloatXMovement : MonoBehaviour
{
    public Texture2D zDataTexture; // read for Z movement data
    // Delay everything, don't start at all until startFrame
    public int startFrame;
    public bool timeToStart = false;
    public int floatStopFrame;
    IEnumerator starter = null;
    [SerializeField] private int startDelayCounter = 0; // counter for the overall delay at start

    public GameObject frontFloat; // need a reference so it can be set to active, and start animating vertically, when X movement starts
    public GameObject onboardFloat; // need a reference so it can be set to inactive while it is offscreen, and before frontFloat becomes active


    // Settings for the Float animation loop
    // NOTE: forwardInterval and pauseInterval need to match up with the animation clip of the float's vertical transform
    // (pause happens when float is at surface)
    public int frameCounter = 0;
    public int cycleCount = 0; // track which cycle we are on -- 3rd cycle is under ice, so topPos is set lower, and data vfx doesn't stream
    public int underIceCycle;
    [SerializeField] private int intervalCounter;
    public int forwardInterval = 30;
    public int pauseInterval;
    public int initialPause; // pause x movement to allow the float to drop to depth
    public int endPause; // pause x movement to allow float to rise to surface
    public float speed;

    public GameObject waterFront; // ref to water material so the transparency can be animated
    public GameObject ribbon; // refto data ribbon material so the transparency can be animated
    public GameObject datastreamGO;
    // capture the initial local position of the datastream vfx, for positioning later, when it starts moving again
    // (allowing the datastream vfx to be decoupled from the float - otherwise the vfx moves along with the float as it is finishing up a "transmission")
    private Vector3 datastreamOffset; 

    private Material waterMat;
    private Material dataMat;
    private VisualEffect datastream;

    [SerializeField] private float transStart;
    [SerializeField] private bool paused = false;
    [SerializeField] private float topPos; // set the upper position of the float
    [SerializeField] private float bottomPos; // set the bottom position
    [SerializeField] private float xPos;
    [SerializeField] private float yPos; 
    [SerializeField] private float zPos;
    [SerializeField] private float ribbonWidth; // the width of the data panel
    [SerializeField] private float zMovementScale; // match the z movement to the z position of the data panel from the shader
    [SerializeField] private int zMovementControl; // manually control z movement in Editor for testing
    [SerializeField] private float waterOverlap; // the amount the water overlaps the data panel
    private float zPosInitial;
    private float waterOverlapScale; // modulate the amount the water overlaps the data panel

    [SerializeField] private AnimationCurve notUnderIceCurve; // curve to control vertical Y movement
    [SerializeField] private AnimationCurve underIceCurve; // curve to control vertical Y movement when the cycle beins under ice
    private AnimationCurve verticalCurve;

    private IEnumerator WaitForIt(int startDelay)  // delay in frames before any motion
    {
        while (startDelayCounter < startDelay)
        {
            yield return new WaitForFixedUpdate();
            startDelayCounter++;
            if (startDelayCounter == 90) { onboardFloat.SetActive(false); } // make the onboard (snaller) version of the float invisible while it is offscreen)
        }
        timeToStart = true;
    }

    private void Start()
    {
        if (forwardInterval < 1) { forwardInterval = 30; } // avoid divide by 0 errors in case forwardInterval was undefined and defaults to 0

        if (ribbon) { dataMat = ribbon.GetComponent<Renderer>().material; }
        if (waterFront) { waterMat = waterFront.GetComponent<Renderer>().material; }

        frontFloat.SetActive(false);

        xPos = transform.position.x;
        intervalCounter = 0;

        yPos = topPos;

        zPosInitial = transform.position.z;
        zPos = zPosInitial;

        datastream = datastreamGO.GetComponent<VisualEffect>();
        datastream.Stop();
        datastreamOffset = datastreamGO.transform.localPosition;

        verticalCurve = notUnderIceCurve;

        starter = WaitForIt(startFrame);
        StartCoroutine(starter);

    }


    void FixedUpdate()
    {
        if (frameCounter == floatStopFrame)
        {
            EndFloat();
        }

        if (timeToStart)
        {
            // Set Float to active, so it starts its vertical animation
            //frontFloat.SetActive(true);
            // but wait about half a second for it to be visible, so it becomes visible while offscreen for smooth transition between onboard and front floats
            if (frameCounter == 10) { frontFloat.SetActive(true); }

            frameCounter += 1;
            intervalCounter += 1;

            if (paused)
            {
                if (intervalCounter % pauseInterval == 0)
                {
                    TogglePaused();
                }
            }
            else // float is moving in x direction
            {
                if (intervalCounter > initialPause) // an initial pause at the start of each cycle, as the float goes to depth
                {
                    if (intervalCounter < (forwardInterval - endPause))
                    {
                        xPos += speed;
                        //yPos = bottomPos + (-bottomPos * ((float)intervalCounter / (float)forwardInterval)); // animate by calculation
                        yPos = bottomPos + ((topPos-bottomPos) * verticalCurve.Evaluate((float)intervalCounter / (float)forwardInterval)); // animate by curve
                        if (zDataTexture != null)
                        {
                            zMovementControl = (int)(transStart * 1024); // 1024 is pixel width of noise image
                            zPos = zPosInitial + zMovementScale * zDataTexture.GetPixel(zMovementControl, 1).grayscale;
                        }
                        transform.position = new Vector3(xPos, yPos, zPos);

                        // update how much of the data ribbon is visible
                        if (transStart < 1)
                        {
                            transStart = transform.localPosition.x / ribbonWidth; // normalize from full width of ribbon (6100)
                            if (transStart > 1) { transStart = 1;  }
                            dataMat.SetFloat("Vector1_EF90DAF5", transStart);

                            // update visibility of the front water
                            if (waterFront) // don't require an an overlapping water panel (that's only for the front one)
                            {
                                float adjustedWaterOverlap = waterOverlap;
                                //if ((1 - transStart) < waterOverlap) { adjustedWaterOverlap = 1 - transStart; } // reduce the overlap to 0 at the end
                                waterMat.SetFloat("Vector1_EF90DAF5", transStart - adjustedWaterOverlap);
                            }
                            
                        }
                    }
                }

                if (intervalCounter % forwardInterval == 0)
                {
                    TogglePaused();
                }
            }
        }
    }

    private void EndFloat()
    {
        frontFloat.SetActive(false);
        datastreamGO.SetActive(false);
        speed = 0;
    }

    private void TogglePaused()
    {
        paused = !paused;
        if (paused) {
            cycleCount += 1; // track the cycles so we know when it's under the ice (currently cycle 3)
            forwardInterval += 36; // compensate for perspective distortion by going a bit further each cycle

            // The last time the front float streams its data, the Aqua satellite comes too close to the data stream
            // So, reduce the particle lifetime for the last datastream only (cycle #4). Only the front float will have 4 cycles, so
            // the back and middle floats aren't affected
            if (cycleCount == 4)
            {
                datastream.SetFloat("Particle Lifetime", 1.35f);
                // while we're here, customize additional properties for a nice curved stream 
                datastream.SetFloat("Vertcal Initial Velocity Adjustment", 0.35f);
                datastream.SetFloat("Downward Velocity", -1000f);
            }
            
        } 
        intervalCounter = 0;

        // when under the ice, don't rise all the way to surface at the end of the cycle
        if (cycleCount == underIceCycle-1) { topPos = -200f; }
        // also need to keep the float from popping thru the ice as the next cycle begins
        // so set the curve to the alternate curve which starts at a lower point
        else if (cycleCount == underIceCycle) { topPos = 0f; verticalCurve = underIceCurve;  }

        // turn data stream to satellite on or off
        if (paused && cycleCount != underIceCycle)
        {
            datastreamGO.transform.parent = transform; // reparent the vfx so that the particles emanate from where the float is
            datastreamGO.transform.localPosition = datastreamOffset; // put the vfx in correct position
            datastreamGO.transform.parent = null; // unparent the vfx so that the particles won't move with the object, once it starts moving again
            datastream.Play();
        } else
        { 
            datastream.Stop(); // Stop not working?
            datastream.SendEvent("TurnOff");
        }
    }


}
