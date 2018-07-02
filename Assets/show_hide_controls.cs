using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class show_hide_controls : MonoBehaviour {

    public Button controlsToggle;
    public GameObject controlPanel;
    public Vector3 active_pos;
    public Vector3 hidden_pos;
    private bool controlsShow;
    
	// Use this for initialization
	void Start () {
        controlsShow = true;

        controlsToggle.onClick.AddListener(toggleControls);
		
	}

    void toggleControls()
    {
        if (controlsShow)
        {
            controlPanel.transform.localPosition = active_pos;
            controlsShow = false;
        }
        else
        {
            controlPanel.transform.localPosition = hidden_pos;
            controlsShow = true;
        }
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
