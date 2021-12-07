using UnityEngine.VFX;
using UnityEngine;
using System.Collections;

public class ShipZMovement : MonoBehaviour
{
    [SerializeField] private int frameCounter = 0;
    [SerializeField] private int intervalCounter;
    [SerializeField] private int startDelayCounter = 0; // counter for the overall delay at start. TODO: move inside coroutine as local variable

    // Delay everything, don't start at all until startFrame
    [SerializeField] private int startFrame;
    public int shipStopFrame = 322; // stop movement, allow time to show data transmission, and then fade visibility. Counted from the startFrame (after dealy)
    IEnumerator starter = null; // 0

    public int shipToSatTransmissionTime; // how long to transmit from ship to Satellite, and then when done, start to transmit from Sat to Tower
    IEnumerator startCommTransmission = null; // 1

    public int commSatToTowerTransmissionTime;
    IEnumerator endCommTransmission = null; // 2

    IEnumerator fadeCommSat = null; // 4

    private bool[] startTimes; // for WaitForIt coroutine -- how many frames to run it

    // Settings for the Ship animation loop
    public int forwardInterval = 30;
    public int pauseInterval;
    public int initialPause; // a delay at the start of each looping animation, not the overall delay at start
    private int endPause;
    public float speed;


    public GameObject ribbon;
    private Material ribbonMat;

    public GameObject leftFrontWater; // needed to force its material to be visible. Does not show up until material selected. Bug? Transparency issue?
    private Material leftWaterMat;

    public GameObject datastreamGO;
    public GameObject commSatGO; // to reference the vfx datastream from comm sat to satellite dish tower
    public GameObject ship;

    private VisualEffect datastream;
    private VisualEffect datastreamCommSat;


    [SerializeField] private float transStart;
    [SerializeField] private bool paused = false;
    [SerializeField] private float zPos;

    private IEnumerator WaitForIt(int startDelay, int startTime)  // delay in frames before any motion
    {
        int delayCounter = 0;

        while (delayCounter < startDelay)
        {
            yield return new WaitForFixedUpdate();
            delayCounter++;
        }
        startTimes[startTime] = true;
    }

    private void Start()
    {
        if (forwardInterval < 1) { forwardInterval = 30; } // avoid divide by 0 errors in case forwardInterval was undefined and defaults to 0

        if (ribbon) { ribbonMat = ribbon.GetComponent<Renderer>().material; }
        if (leftFrontWater) { leftWaterMat = leftFrontWater.GetComponent<Renderer>().material; }
        leftWaterMat.renderQueue = 2650;
        leftWaterMat.SetFloat("Vector1_EF90DAF5", 1f);

        endPause = initialPause;
        zPos = transform.position.z;
        intervalCounter = 0;

        datastream = datastreamGO.GetComponent<VisualEffect>();
        datastream.Stop();
        datastream.SetInt("Count", 0); // Count (# particles emitted) was set to 0 so that the vfx can be off in the Editor (where it's a visual nuisance). Reset to 1 here.

        datastreamCommSat = commSatGO.GetComponent<VisualEffect>();
        datastreamCommSat.Stop();

        startTimes = new bool[6];
        

        starter = WaitForIt(startFrame, 0);
        startCommTransmission = WaitForIt(shipToSatTransmissionTime, 1);
        endCommTransmission = WaitForIt(shipToSatTransmissionTime, 2);
        fadeCommSat = WaitForIt(2600, 4); // set the frame when the comm sat disappears

        StartCoroutine(starter);
    }


    void FixedUpdate()
    {
        if (frameCounter == shipStopFrame)
        {
            EndShip();
        }

        if (startTimes[1]) // timeToTransmitFromSat
        {
            datastream.Stop(); // Stop stopped working
            datastream.SendEvent("TurnOff");
            datastreamCommSat.Play();
            startTimes[1] = false;
            StartCoroutine(endCommTransmission);
        }

        if (startTimes[2])
        {
            datastreamCommSat.Stop();
            datastreamCommSat.SendEvent("TurnOff");
            startTimes[2] = false;
            startTimes[3] = true;

        }

        if (startTimes[0]) // timeToStart
        {
            frameCounter += 1;
            intervalCounter += 1;

            if (paused)
            {
                if (intervalCounter % pauseInterval == 0)
                {
                    TogglePaused();
                }
            }
            else
            {
                if (intervalCounter > initialPause) // an initial pause at the start of each cycle, as the float goes to depth
                {
                    if (intervalCounter < (forwardInterval - endPause))
                    {
                        MoveShip();
                    }
                }

                if (intervalCounter % forwardInterval == 0)
                {
                    TogglePaused();
                }
            }
        }

        if (startTimes[3])
        {
            startTimes[5] = true; // allow ship to move again, past the cube
            speed = 42.5f; // move ship off the cube more slowly than before
            StartCoroutine(fadeCommSat);
            startTimes[3] = false;
        }

        if (startTimes[4])
        {
            commSatGO.transform.parent.gameObject.SetActive(false);
        }

        if (startTimes[5])
        {
            
            MoveShip();
        }

    }

    private void MoveShip()
    {
        zPos += speed;
        transform.position = new Vector3(transform.position.x, transform.position.y, zPos);
        // update how much of the data ribbon is visible (stop when fully visible eg transtart == 1
        if (transStart < 1)
        {
            transStart = transform.localPosition.z / 6000; // normalize from full width of ribbon (5992)
            if (transStart > 1f) { transStart = 1f; }
            ribbonMat.SetFloat("Vector1_EF90DAF5", transStart);
        }
    }

    private void TogglePaused()
    {
        paused = !paused;
        intervalCounter = 0;
    }

    private void EndShip()
    {
        datastream.SetInt("Count", 1);
        datastream.Play();
        speed = 0;
        StartCoroutine(startCommTransmission);
        StartCoroutine(fadeCommSat);

    }


}
