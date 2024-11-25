using UnityEngine;

public class bullet : MonoBehaviour
{

    //Bullet attributes
    [Header("Bullet Attributes")]
    public float damage = 1;

    public Vector3 startingPosition;

    // Start is called before the first frame update
    void Start()
    {
        //Destroy the bullet after 2 seconds
        Destroy(gameObject, 2);
    }
    // Update is called once per frame
    void Update()
    {
        
    }

    //on collision
    private void OnCollisionEnter(Collision collision)
    {
        //Disable the bullet
        gameObject.SetActive(false);


        //Get the normal of the collision
        Vector3 normal = collision.contacts[0].normal;
        //Reflect the bullet
        Vector3 direction = Vector3.Reflect(transform.forward, normal);
        //Add force to the bullet
        Rigidbody rb = gameObject.GetComponent<Rigidbody>();
        rb.AddForce(direction * 100, ForceMode.Impulse);

        //If the bullet hits the ground, destroy it
        if (collision.gameObject.tag == "Ground")
        {
            Destroy(gameObject);
        }
    }

    public void Shoot(Vector3 direction, float bulletSpeed){
        //Add force to the bullet
        Debug.Log("Shooting POW Speed: " + bulletSpeed + " Direction: " + direction); 
        Vector3 force = direction * bulletSpeed;
        Rigidbody rb = gameObject.GetComponent<Rigidbody>();
        rb.AddForce(force, ForceMode.Impulse);
        Debug.Log(gameObject.GetComponent<Rigidbody>().velocity);
    }

  

}
