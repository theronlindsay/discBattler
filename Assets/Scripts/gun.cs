using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class gun : MonoBehaviour
{
    //gun attributes
    [Header("Gun Attributes")]
    public float damage;
    public float range;
    public float fireRate;
    public float impactForce;
    public float bulletSpeed;
    public GameObject bulletPrefab;

    private bool canShoot = true;
    private float nextTimeToFire = 0;

    // update is called once per frame
    void Update()
    {
        if (Time.time >= nextTimeToFire)
        {
            canShoot = true;
        }
    }

    public void Shoot()
    {
        //Check if the gun can shoot
        if (!canShoot)
        {
            return;
        }
        //Set the next time to fire
        nextTimeToFire = Time.time + 1f / fireRate;
        Debug.Log("Shoot");
        //Instantiate the bullet
        GameObject bullet = Instantiate(bulletPrefab, transform.position, transform.rotation);
        //Put bullet in front of the gun
        bullet.transform.position = transform.position + transform.forward;
        //Get the bullet script
        bullet bulletScript = bullet.GetComponent<bullet>();
        //Set the bullet damage
        bulletScript.damage = damage;
        //Get the bullet rigidbody
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        //Add force to the bullet
        rb.AddForce(transform.forward * bulletSpeed, ForceMode.Impulse);
        //reset the canShoot variable
        canShoot = false;
    }

}
