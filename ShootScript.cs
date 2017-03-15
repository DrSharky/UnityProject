using UnityEngine;
using System.Collections;
using System;

public class ShootScript : MonoBehaviour
{
    public Color color1;

	// Use this for initialization
	void Start ()
    {
        color1 = Color.blue;
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (Input.GetMouseButtonDown(0)){
            RaycastHit hit = new RaycastHit();
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if(Physics.Raycast(ray, out hit))
            {
                try
                {
                    Renderer objRenderer = hit.transform.gameObject.GetComponent<Renderer>();
                    objRenderer.material.SetColor("_Color", Color.blue);
                }
                catch (Exception ex)
                {

                }
            }
        }
	}
}
