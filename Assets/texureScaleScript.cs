using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class texureScaleScript : MonoBehaviour
{
    public Vector3 initialScale = new Vector3(1,0.2f,1);
    public Transform parentobject;
    public Transform grassTransform;

    // Start is called before the first frame update
    void Start()
    {
        float deltaL0 = 1 - initialScale.y;
        grassTransform.localScale = new Vector3(grassTransform.localScale.x, initialScale.y / parentobject.localScale.y, grassTransform.localScale.z);
        Debug.Log(parentobject.localScale.y / 2 - initialScale.y / 2);
        grassTransform.localPosition = new Vector3(grassTransform.localPosition.x, (parentobject.localScale.y/2 - initialScale.y/2)/parentobject.localScale.y, grassTransform.localPosition.z);

        Debug.Log(grassTransform.position.y);
    }
}
