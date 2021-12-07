using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vortex : MonoBehaviour {


    public float speed;
    private Vector3 direction;
    public GameObject obj;

	void Start () {

        direction = obj.transform.forward;
	}
	
   [ExecuteInEditMode]
	void Update () {
       obj.transform.RotateAround(this.transform.position, direction, speed);
        Debug.Log(direction);
	}
}
