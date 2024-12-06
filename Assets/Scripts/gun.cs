using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
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
    //public GameObject bulletPrefab;
    public NetworkObject discReference;
    private bool canShoot = true;
    private float nextTimeToFire = 0;
    public Animator anim;
    public GameObject player;
    public NetworkObjectReference playerReference;

    void Start(){
        anim = GameObject.Find("PlayerModel").GetComponent<Animator>();
        playerReference = new NetworkObjectReference(player.GetComponent<NetworkObject>());
    }

    // Activate Recall
    public void Recall(){
        GameManager.Instance.ReturnDiscRpc(discReference.GetComponent<NetworkObjectReference>());
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
        GameManager.Instance.SpawnDiscRpc(launchPoint.transform.position, launchPoint.transform.rotation, playerReference);
    }

    public void ResetDisc()
    {
        if (discReference != null)
        {
            Destroy(discReference); // Destroy any existing disc
        }
        canShoot = true; // Allow the player to throw a new disc
    }


}
