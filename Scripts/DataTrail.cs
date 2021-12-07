using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ara;
using System.Linq; // used for sorting array

public class DataTrail : MonoBehaviour {
    [SerializeField]
    private GameObject trackPointsParent;
    [SerializeField]
    private Transform[] trackPoints;


    private int counter = 0;
    private AraTrail araTrail;

	void Start () {

        SortPoints();
        araTrail = this.GetComponent<AraTrail>();
        StartCoroutine("MoveTrail");
        
	}

    private IEnumerator MoveTrail()
    {
        for (int i = 0; i < trackPoints.Length; i++)
        {
            yield return new WaitForSeconds(0.3f);
            this.transform.position = trackPoints[counter].position;
            if (araTrail)
            {
                araTrail.EmitPoint(trackPoints[counter].position);
            }
            counter += 1;
        }
    }

    // helper to sort points hierarchically
    private void SortPoints()
    {
        trackPoints = new Transform[trackPointsParent.transform.childCount];

        foreach (Transform child in trackPointsParent.transform)//GetComponentsInChildren<Transform>()) // GetComponentsInChildren also returns parent, which is not wanted
        { 
            int index = child.GetSiblingIndex();
  
            trackPoints[index] = child; 
        }
        // to sort using Linq
        //trackPoints = GameObject.FindGameObjectsWithTag("Waypoint").OrderBy(go => go.name).ToArray();
    }
}
