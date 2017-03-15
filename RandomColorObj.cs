using UnityEngine;
using System.Collections;

public class RandomColorObj : MonoBehaviour
{

    // Use this for initialization
    void Start()
    {
        Color randomColor = GenerateRandomColor(0.9f, 0.9f);
        var objectRenderer = gameObject.GetComponent<Renderer>();
        objectRenderer.material.SetColor("_EmissionColor", randomColor);
    }

    // Update is called once per frame
    void Update()
    {

    }

    Color GenerateRandomColor(float s, float v)
    {
        var hue = Random.Range(0f, 1f);
        return Color.HSVToRGB(hue, s, v);
    }
}
