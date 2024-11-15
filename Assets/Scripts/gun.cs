using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class gun : NetworkBehaviour
{
    //gun attributes
    [Header("Gun Attributes")]
    public float damage;
    public GameObject launchPoint;

    public float range; //How far the bullet can go
    public float fireRate;
    public float impactForce;
    public float bulletSpeed;
    public GameObject bulletPrefab;
    private bool canShoot = true;
    private float nextTimeToFire = 0;
    public Animator anim;

    void Start(){
        anim = GameObject.Find("PlayerModel").GetComponent<Animator>();
    }

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
        //reset the canShoot variable
        canShoot = false;
        //Set the next time to fire
        nextTimeToFire = Time.time + 1f / fireRate;
        //Play the shooting animation
        anim.SetTrigger("Throw");
        //Throw the disc
        Invoke("ThrowDisc", 0.5f);
        
    }

    private void ThrowDisc(){
        //Instantiate the bullet
        GameObject bullet = Instantiate(bulletPrefab, launchPoint.transform.position, transform.rotation);
        Debug.Log(launchPoint.transform.position);
        Debug.DrawLine(launchPoint.transform.position, launchPoint.transform.position + transform.forward * range, Color.red, 2);
        //Get the bullet script
        bullet bulletScript = bullet.GetComponent<bullet>();
        //Set the bullet damage
        bulletScript.SetStats(damage, range);
        //Add force to the bullet
        bulletScript.Shoot(transform.forward, bulletSpeed);
    }

}
