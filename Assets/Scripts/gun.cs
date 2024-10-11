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
    public GameObject firePoint;

    public Camera Camera;

    public Vector2 mousePos;    
    


    private bool canShoot = true;
    private float nextTimeToFire = 0;

    // update is called once per frame
    void Update()
    {
        if (Time.time >= nextTimeToFire)
        {
            canShoot = true;
        }

        Debug.Log(mousePos);
        //update mousePOS
        mousePos = Camera.ScreenToWorldPoint(Input.mousePosition);
        Debug.Log(Input.mousePosition);
    }

    public void Shoot()
    {
        //Check if the gun can shoot
        if (!canShoot)
        {
            return;
        }

        // Ensure firePoint and bulletPrefab are assigned
        if (firePoint == null || bulletPrefab == null)
        {
            Debug.LogError("FirePoint or BulletPrefab is not assigned.");
            return;
        }
        
        //  Set cooldown
        nextTimeToFire = Time.time + 1f / fireRate;
        //aim the gun at the point where the mouse and direction of the gun meet
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 direction = (mousePos - transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
        //create the bullet
        GameObject bullet = Instantiate(bulletPrefab, firePoint.transform.position, transform.rotation);
        //set the bullet speed
        bullet.GetComponent<Rigidbody2D>().velocity = direction * bulletSpeed;
        //set the bullet damage
        bullet.GetComponent<bullet>().damage = damage;

        canShoot = false;
    }

}
