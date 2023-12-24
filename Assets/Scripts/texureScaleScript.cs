using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class texureScaleScript : MonoBehaviour
{
    public Vector3 initialGroudScale = new Vector3(1,1,1);
    public Vector3 initialGrassScale;

    public float deltaForTopGrassBorder;
    public Vector2 initialSize;
    public Transform parentobject;
    public Transform grassTransform;
    
  
    
    void Start()
    {
        grassTransform.localScale = new Vector3(initialGrassScale.x / parentobject.localScale.x, initialGrassScale.y / parentobject.localScale.y, grassTransform.localScale.z);
        grassTransform.GetComponent<SpriteRenderer>().size = new Vector2(initialSize.x * parentobject.localScale.x, grassTransform.GetComponent<SpriteRenderer>().size.y);
        
        grassTransform.position = new Vector3(grassTransform.position.x, parentobject.position.y + initialGroudScale.y* parentobject.localScale.y/2+ deltaForTopGrassBorder, grassTransform.position.z);

    }
}
