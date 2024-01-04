using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeadlyPlatformScript : MonoBehaviour
{
    [SerializeField] private float damage = 200;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision == null) return;

        if (collision.gameObject.CompareTag("player"))
        {
            collision.gameObject.GetComponent<IPlayerMain>().Damage(damage);
        }
    }
}
