using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class gun : NetworkBehaviour
{
    //gun attributes
    [Header("Gun Attributes")]
    public float damage;

    public float range; //How far the bullet can go
    public float fireRate;
    public float impactForce;
    public float bulletSpeed;
    public GameObject bulletPrefab;

    private bool canShoot = true;
    private float nextTimeToFire = 0;

    // update is called once per frame (Pretty Much just a timer)
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
        if (!canShoot || !IsOwner)
        {
            return;
        }
        //Set the next time to fire
        nextTimeToFire = Time.time + 1f / fireRate;
        Debug.Log("Shoot");
        //Instantiate the bullet
        GameObject bullet = Instantiate(bulletPrefab, transform.position + transform.forward, transform.rotation);
        //Get the bullet script
        bullet bulletScript = bullet.GetComponent<bullet>();
        //Set the bullet damage
        bulletScript.SetStats(damage, range);
        //Add force to the bullet
        bulletScript.Shoot(transform.forward, bulletSpeed);
        //reset the canShoot variable
        canShoot = false;
    }

}
