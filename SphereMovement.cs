using UnityEngine;
using System.Collections;

public class SphereMovement : MonoBehaviour
{
    public float angleDeg;
    private Transform sphere;
    private float angleRad;
    private Vector3 moveVector;
    public bool bitSwitch;
    public float time;

    // Use this for initialization
    void Start()
    {
        sphere = transform;
        angleDeg = 0f;
        bitSwitch = false;
        time = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        Move();
    }

    void Move()
    {
        time = Time.time;
        angleRad = angleDeg * Mathf.Deg2Rad;
        //moveVector = new Vector3(Mathf.Cos(angleRad), transform.position.y, Mathf.Sin(angleRad));
        moveVector.z += Mathf.Sin(angleRad) * Time.deltaTime;
        moveVector.x += Mathf.Cos(angleRad) * Time.deltaTime;
        //Vector3 _newPosition = new Vector3();
        //_newPosition.x += Mathf.Cos(Time.time) * Time.deltaTime;
        //_newPosition.z += Mathf.Sin(Time.time) * Time.deltaTime;
        transform.position = moveVector*30;
        if (bitSwitch)
            angleDeg -= 8;
        else
            angleDeg += 8;
        if (angleDeg == 360)
        {
            angleDeg = 0;
            bitSwitch = !bitSwitch;
        }
        else if (angleDeg == -360)
        {
            angleDeg = 0;
            bitSwitch = !bitSwitch;
        }
    }
}