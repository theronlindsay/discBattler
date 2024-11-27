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
    public float fireRate;
    public float impactForce;
    public float bulletSpeed;
    public GameObject bulletPrefab;
    public GameObject disc;
    private bool canShoot = true;
    private float nextTimeToFire = 0;
    public Animator anim;
    public GameObject player;

    void Start(){
        anim = GameObject.Find("PlayerModel").GetComponent<Animator>();
    }

    // Activate Recall
    public void Recall(){
        if(disc != null){
            disc.GetComponent<bullet>().Recall();
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
        //Play the shooting animation
        anim.SetTrigger("Throw");
        //Throw the disc
        Invoke("ThrowDisc", 0.5f);
        
    }

    //Recall disk sends the disc back to the player, when the player collides with it, enable CanShoot
    public void ReturnDisc(){
        // Launch the disc back to the player
        canShoot = true;
    }

    private void ThrowDisc(){
        //Instantiate the bullet
        disc = Instantiate(bulletPrefab, launchPoint.transform.position, transform.rotation);
        //Give the bullet a reference to the player
        disc.GetComponent<bullet>().player = player;
        Debug.Log(launchPoint.transform.position);
        Debug.DrawLine(launchPoint.transform.position, launchPoint.transform.position + transform.forward, Color.red, 2);
        //Get the bullet script
        bullet bulletScript = disc.GetComponent<bullet>();
        //Add force to the bullet
        bulletScript.Shoot(transform.forward, bulletSpeed);
    }

}
