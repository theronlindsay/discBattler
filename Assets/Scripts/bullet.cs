using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bullet : MonoBehaviour
{

    //Bullet attributes
    [Header("Bullet Attributes")]
    public float damage = 1;

    // Start is called before the first frame update
    void Start()
    {
        //Destroy the bullet after 2 seconds
        Destroy(gameObject, 2);
    }

    //on collision
    private void OnCollisionEnter2D(Collision2D collision)
    {
        //Disable the bullet
        gameObject.SetActive(false);
        //Destroy the bullet
        Destroy(gameObject);
    }
}
