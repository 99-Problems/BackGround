using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RawImageUVAnimation : MonoBehaviour
{
    RawImage rawImage;
    public float speed = 1;
    public Vector2 offset;

    void Start()
    {
        rawImage = GetComponent<RawImage>();
    }

    void Update()
    {
        var property = rawImage.uvRect;
        rawImage.uvRect = new Rect(property.x + Time.deltaTime * speed * offset.x,
            property.y + Time.deltaTime * speed * offset.y, property.width, property.height);
    }
}