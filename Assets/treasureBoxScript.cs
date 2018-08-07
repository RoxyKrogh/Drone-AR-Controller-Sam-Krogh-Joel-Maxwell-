using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class treasureBoxScript : MonoBehaviour {

    public GameObject treasureBox;
    public Camera playerView;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (Vector3.Distance(treasureBox.transform.position, playerView.transform.position) < 5)
        {
            treasureBox.GetComponent<Animation>().Play("box_open");
            treasureBox.transform.rotation = Quaternion.Euler(0, 180, 0);
        }
	}
}
