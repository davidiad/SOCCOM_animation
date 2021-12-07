using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetRenderQueueOrder : MonoBehaviour
{

    public Material waterMat; // top of water
    public Material frontDataMat;
    public Material midDataMat;
    public Material backDataMat;
    public Material leftWaterMat;
    public Material shipDataMat;

    public GameObject leftWater;

    private void Start()
    {
        //// force left from water to be visible -- was dropping out for reasons unknown having to do with render q
        //leftWater.SetActive(false);
        //leftWater.SetActive(true); 

        waterMat.renderQueue = 2700; // top of water
        frontDataMat.renderQueue = 3200;
        midDataMat.renderQueue = 2750;
        backDataMat.renderQueue = 2730;
        leftWaterMat.renderQueue = 2600;
        shipDataMat.renderQueue = 2650;
    }

}
