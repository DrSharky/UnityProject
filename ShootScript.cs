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
            if(Physics.Raycast(ray, out hit) && hit.transform.gameObject.tag == "PhysicMaterialObj")
            {
                try
                {
                    GameObject hitObject = hit.transform.gameObject;
                    Renderer objRenderer = hit.transform.gameObject.GetComponent<Renderer>();
                    objRenderer.material.SetColor("_Color", Color.blue);
                    PhysicMaterial hitMaterial = hitObject.GetComponent<Collider>().material;

                    hitMaterial.bounciness = 0.9f;
                    hitMaterial.staticFriction = 0.0f;
                    hitMaterial.dynamicFriction = 0.2f;
                    hitMaterial.bounceCombine = PhysicMaterialCombine.Maximum;

                }
                catch (Exception ex)
                {

                }
            }
        }
	}
}
