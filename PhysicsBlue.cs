using UnityEngine;
using System.Collections;

public class PhysicsBlue : MonoBehaviour {

    public Renderer rend;

	// Use this for initialization
	void Start ()
    {
        rend = gameObject.GetComponent<Renderer>();
	}
	
	// Update is called once per frame
	void Update ()
    {
        Color objColor = rend.material.color;
        if (objColor.Equals(Color.blue))
        {
            PhysicMaterial boxPhys = gameObject.GetComponent<BoxCollider>().material;
            boxPhys.bounciness = 1;
        }
	}
}
