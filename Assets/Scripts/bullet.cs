using UnityEngine;

public class bullet : MonoBehaviour
{

    //Bullet attributes
    [Header("Bullet Attributes")]
    public float damage = 1;

    public Vector3 startingPosition;
    public GameObject player;

    public float returnSpeed = 10f;  
    public bool isReturning = false;

    // Start is called before the first frame update
    void Start()
    {
    }

    void Update(){
        if(isReturning){
            MoveTowardsObject(player, returnSpeed);
        }
    }

    void MoveTowardsObject(GameObject target, float speed){
        Vector3 direction = target.transform.position - transform.position;
        direction.Normalize();
        gameObject.GetComponent<Rigidbody>().velocity = direction * speed;
        Debug.DrawLine(transform.position, transform.position + direction * 10, Color.red, 2);
    }   

    //on collision
    private void OnCollisionEnter(Collision collision)
    {
        //If the bullet hits the player, return it to the player
        if (collision.gameObject == player && isReturning == true)
        {
            Debug.Log("Hit Player");
            isReturning = false;
            //Call the return disc function on the child of the player
            player.GetComponentInChildren<gun>().ReturnDisc();


            Destroy(gameObject);
            return;
        } else {
            Debug.Log("Hit Something Else");
            Debug.Log(collision.gameObject);
            Debug.Log(isReturning);
        }

        // //Get the normal of the collision
        // Vector3 normal = collision.contacts[0].normal;
        // //Reflect the bullet
        // Vector3 direction = Vector3.Reflect(transform.forward, normal);
        // //Add force to the bullet
        // Rigidbody rb = gameObject.GetComponent<Rigidbody>();
        // rb.AddForce(direction * 100, ForceMode.Impulse);

        //If the bullet hits the ground, destroy it
        if (collision.gameObject.tag == "Ground")
        {
            Destroy(gameObject);
            player.GetComponent<gun>().Recall();
        }
    }

    public void Shoot(Vector3 direction, float bulletSpeed){
        //Add force to the bullet
        Debug.Log("Shooting POW Speed: " + bulletSpeed + " Direction: " + direction); 
        Vector3 force = direction * bulletSpeed;
        Rigidbody rb = gameObject.GetComponent<Rigidbody>();
        rb.AddForce(force, ForceMode.Impulse);
        Debug.Log("Bullet Speed: " + rb.velocity.magnitude);
        Debug.DrawLine(transform.position, transform.position + direction * 10, Color.red, 2);
    }

    public void Recall(){
        //Return the bullet to the player
        Debug.Log("Recalling");
        isReturning = true;

        // Move the bullet towards the player at a set speed
        MoveTowardsObject(player, returnSpeed);
    }

  

}
