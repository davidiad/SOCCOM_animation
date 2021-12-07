using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class SatelliteMovement : MonoBehaviour
{
    [SerializeField] private AnimationCurve fadeInCurve; // curve to control alpha of beam fade
    [SerializeField] private AnimationCurve fadeOutCurve; // curve to control alpha of beam fade
    public Vector3 satelliteStartPos; // Allows moving Satellite around in the Editor, but will still start from correct position
    // Delay everything, don't start at all until startFrame
    private bool timeToStart = false;
    private int startDelayCounter = 0; // counter for the overall delay at start
    private IEnumerator starter = null;
    private IEnumerator dataDelay = null; // delay at end of satellite movement, before data transmission starts
    private IEnumerator fadeBeamIn = null;
    private IEnumerator fadeBeamOut = null;

    public int satelliteStartFrame; // satellite waits offscreen until this frame
    public float zStartPos; // start with saellite out of view
    public float zEndPos; // end with satellite out of view
    public float beamOnPos; // turn on beam at far edge of cube
    public float beamOffPos; // turn off beam at near edge of cube; also the point where the satellite stops or pauses at the end
    private bool beamOn;

    public float xPosShift; // how far to move satellite over with each pass
    //public float pauseBetweenPasses; // how long to wait until the next pass because the satellite orbits the entire Earth! (not currently in use)
    public int numPasses; // how many times to fly the satellite over the ocean cube
    private int currentPass;

    public float speed;

    [SerializeField] private float xPos;
    [SerializeField] private float zPos;

    // Control the appearance (thru transparency) of the chlorophyll in strips (as the satellite passes over)
    public GameObject[] chlorophyllStrips;
    public Material[] chlorophyllMaps;
    private float transStart;

    [SerializeField] private GameObject datastreamGO;
    [SerializeField] private GameObject beamGO;
    private VisualEffect aquaDatastream;
    private VisualEffect beam;
    private float beamAlpha;

    private IEnumerator WaitForIt(int startDelay)  // delay in frames before any motion
    {
        while (startDelayCounter < startDelay)
        {
            yield return new WaitForFixedUpdate();
            startDelayCounter++;
        }
        timeToStart = true;
        SetChlorophyllActive(true);
        startDelayCounter = 0; // reset for next time coroutine is called
    }

    // TODO: use delegates to combine Waitfirits
    private IEnumerator WaitForIt2(int startDelay)  // delay in frames beam turns off and datastream starts
    {
        while (startDelayCounter < startDelay)
        {
            yield return new WaitForFixedUpdate();
            startDelayCounter++;
        }
        //beamGO.SetActive(false); // turn beam off
        //StartCoroutine(FadeBeam(fadeOutCurve, 75));  // fade out beam
        aquaDatastream.Play();

        startDelayCounter = 0; // reset for next time coroutine is called

        while (startDelayCounter < startDelay * 2)
        {
            yield return new WaitForFixedUpdate();
            startDelayCounter++;
        }
        aquaDatastream.SendEvent("TurnOff");
    }

    private IEnumerator FadeBeam (AnimationCurve curve, int duration)
    {
        int beamFadeCounter = 0;
        float t;// = 1f;
        float alpha;// = 1f;

        while (beamFadeCounter < duration)
        {
            beamFadeCounter++;
            t = (float)beamFadeCounter / (float)duration;
            alpha = curve.Evaluate(t);
            beam.SetFloat("Overall Alpha", alpha);
            
            yield return new WaitForFixedUpdate();

        }
    }

    void Start()
    {
        transform.position = satelliteStartPos;
        xPos = transform.position.x;
        zStartPos = transform.position.z;
        zPos = zStartPos;
        transStart = zPos;
        currentPass = 0;

        SetChlorophyllActive(true); // is it neccesary for the game objects to be active in order to set material properties? If not, delete this line.
        ResetChlorophyllTrans(); // just in case the chlorophyll is showing, which it should not, at the start
        SetChlorophyllActive(false);

        for (int i=0; i<chlorophyllMaps.Length; i++){ chlorophyllMaps[i].renderQueue = 2800; }
      

        aquaDatastream = datastreamGO.GetComponent<VisualEffect>();
        aquaDatastream.Stop(); // Stop no working, use TurnOff instead
        aquaDatastream.SendEvent("TurnOff");
        beam = beamGO.GetComponent<VisualEffect>();

        starter = WaitForIt(satelliteStartFrame);
        StartCoroutine(starter);
        dataDelay = WaitForIt2(90); // called later when satellite is done moving
        fadeBeamIn = FadeBeam(fadeInCurve, 480);
        fadeBeamOut = FadeBeam(fadeOutCurve, 45);

    }



    void FixedUpdate()
    {
        if (timeToStart)
        {
            //if (zPos >= beamOnPos)
            //{
            //    //beamAlpha = 1 - (zPos - beamOnPos) / (satelliteStartPos.z - beamOnPos);
            //    //beam.SetFloat("Overall Alpha", beamAlpha);
            //}
            //if (zPos < beamOnPos+600) // from starting position of a pass, start the fade once sat. passes this pt
            //{
            //    //StopCoroutine(fadeBeamOut);
            //    //StartCoroutine(fadeBeamIn);
            //    //StopCoroutine(FadeBeam(fadeOutCurve,66));
            //    //StartCoroutine(FadeBeam(fadeInCurve,444));
            //    if (!beamOn) { ToggleBeam(); }
            //}
            if ( (zPos < beamOnPos + 600) && (zPos > beamOffPos + 300) ) // from starting position of a pass, start the fade once sat. passes this pt
            {
                if (!beamOn) { ToggleBeam(); }
            }
            if (currentPass < numPasses && zPos >= zEndPos)
            {
                zPos += speed;
                transform.position = new Vector3(xPos, transform.position.y, zPos);

                // set how much of the chlor map is showing to correspond to satellite beam
                // go from beamOnPos to beamOffPos (note: going down not up in value)
                // remap from zpos ~4000 to -2000
                if ( zPos < beamOnPos )
                {
                    //if (!beamOn) { ToggleBeam(); }
                    transStart = -1f * (zPos - beamOnPos) / (beamOnPos - beamOffPos); // normalize from full width of cube (beamOnPos - beamOffPos, or ~6000)
                    if (transStart>1f) { transStart = 1f; }
                    chlorophyllMaps[currentPass].SetFloat("Vector1_3FC18944", transStart);
                }
            if (zPos <= beamOffPos+100) // minus an arbitrary offset, so beam has slight delay before turn off on passes 1 and 2
                {
                  if (beamOn) { ToggleBeam(); }
                }
            }
            else
            {
                zPos = zStartPos; // reset zPos for 2nd and 3rd pass
                currentPass++;
                xPos += xPosShift;
                if (currentPass == numPasses - 1) // stop at the end of the last pass for data stream to tower
                {
                    zEndPos = beamOffPos;
                    if (transform.position.z <= zEndPos)
                    {
                        StartCoroutine(dataDelay);
                    }
                }
            }
        }

        //TODO: Show data stream from Aqua Sat to Tower

        //TODO: once completed, ResetChlorophyllTrans(), otherwise the chlorophyll is showing in the Editor after Play is finished
    }


    // reset all chlor. strips to be fully transparent
    private void ResetChlorophyllTrans()
    {
        for (int i = 0; i < numPasses; i++)
        {
            chlorophyllMaps[i].SetFloat("Vector1_3FC18944", 0f);
        }
    }

    private void SetChlorophyllActive(bool on)
    {
        for (int i=0; i<chlorophyllStrips.Length; i++)
        {
            chlorophyllStrips[i].SetActive(on);
        }
    }

    private void ToggleBeam()
    {
        beamOn = !beamOn;
        //beamGO.SetActive(beamOn);

        if (beamOn)
        {
            //beam.Play();
            //StopCoroutine(FadeBeam(fadeOutCurve, 66));
            StartCoroutine(FadeBeam(fadeInCurve, 60));
        }
        else
        {
            //StopCoroutine(FadeBeam(fadeInCurve, 333));
            StartCoroutine(FadeBeam(fadeOutCurve, 30));
        }


    }
}
