using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class startScreenScript : MonoBehaviour {

    public GameObject drone;
    public GameObject droneHolder;
    public GameObject maze;
    public GameObject title;
    private bool droneLeft;
    public Text start;
    public Button startButton;
    private float greenValue;
    private bool greenUp;

	// Use this for initialization
	void Start () {
        droneLeft = true;
        droneHolder.transform.position = new Vector3(-8, 1, -5);
        startButton.onClick.AddListener(nextScene);
        greenValue = 250;
        greenUp = false;
	}

    void nextScene()
    {
        SceneManager.LoadScene("setupScene");
    }
	
	// Update is called once per frame
	void Update () {
		if(droneHolder.transform.position.x < -6.5)
        {
            droneLeft = false;
            drone.transform.rotation = Quaternion.Euler(110, 90, -90);
        }
        else if (droneHolder.transform.position.x > 6.5)
        {
            droneLeft = true;
            drone.transform.rotation = Quaternion.Euler(80, 90, -90);
        }
        if (droneLeft)
        {
            droneHolder.transform.Translate(Vector3.right * -5 * Time.deltaTime);
        }
        else
        {
            droneHolder.transform.Translate(Vector3.right * 5 * Time.deltaTime);
            
        }
        maze.transform.Rotate(Vector3.forward * 5 * Time.deltaTime);

        if (greenUp){ greenValue += 1;}
        else { greenValue -= 1; }
        if(greenValue > 250) { greenUp = false;  }
        else if(greenValue < 80){ greenUp = true; }



        start.color = new Color(0, (greenValue%255)/255, 0);
    }
}
