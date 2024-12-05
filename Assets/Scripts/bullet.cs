using UnityEngine;
using Unity.Netcode;

public class bullet : MonoBehaviour
{
    // Bullet attributes
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

    void Update()
    {
        if (isReturning)
        {
            MoveTowardsObject(player, returnSpeed);
        }
    }

    void MoveTowardsObject(GameObject target, float speed)
    {
        Vector3 direction = target.transform.position - transform.position;
        direction.Normalize();
        gameObject.GetComponent<Rigidbody>().velocity = direction * speed;
        Debug.DrawLine(transform.position, transform.position + direction * 10, Color.red, 2);
    }

    // Handle collisions
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player") && collision.gameObject != player)
        {
            Debug.Log("Hit another player!");

            // Notify the GameManager that a player scored
            NetworkObjectReference scoringPlayer = new NetworkObjectReference(player.GetComponent<NetworkObject>());
            GameManager.Instance.PlayerScored(scoringPlayer);

            Destroy(gameObject); // Destroy the disc after scoring
            return;
        }

        if (collision.gameObject == player && isReturning)
        {
            Debug.Log("Disc returned to player");
            isReturning = false;
            // Call the return disc function on the child of the player
            player.GetComponentInChildren<gun>().ReturnDisc();

            Destroy(gameObject);
            return;
        }

        Debug.Log("Hit something else: " + collision.gameObject.name);

        // If the bullet hits the ground, destroy it
        if (collision.gameObject.CompareTag("Ground"))
        {
            Destroy(gameObject);
            player.GetComponent<gun>().Recall();
        }
    }

    public void Shoot(Vector3 direction, float bulletSpeed)
    {
        // Add force to the bullet
        Debug.Log("Shooting POW Speed: " + bulletSpeed + " Direction: " + direction);
        Vector3 force = direction * bulletSpeed;
        Rigidbody rb = gameObject.GetComponent<Rigidbody>();
        rb.AddForce(force, ForceMode.Impulse);
        Debug.Log("Bullet Speed: " + rb.velocity.magnitude);
        Debug.DrawLine(transform.position, transform.position + direction * 10, Color.red, 2);
    }

    public void Recall()
    {
        // Return the bullet to the player
        Debug.Log("Recalling");
        isReturning = true;

        // Move the bullet towards the player at a set speed
        MoveTowardsObject(player, returnSpeed);
    }
}
