using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class texureScaleScript : MonoBehaviour
{
    public Vector3 initialScale;
    public Vector2 initialSize;
    public Transform parentobject;
    public Transform grassTransform;
    void Start()
    {
        grassTransform.localScale = new Vector3(initialScale.x / parentobject.localScale.x, initialScale.y / parentobject.localScale.y, grassTransform.localScale.z);
        grassTransform.GetComponent<SpriteRenderer>().size = new Vector2(initialSize.x * parentobject.localScale.x, grassTransform.GetComponent<SpriteRenderer>().size.y);
        //grassTransform.localPosition = new Vector3(grassTransform.localPosition.x, (parentobject.localScale.y/2 - initialScale.y/2)/parentobject.localScale.y, grassTransform.localPosition.z);
    }
}
