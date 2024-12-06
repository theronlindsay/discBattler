using UnityEngine;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;

public class bullet : NetworkBehaviour
{
    // Bullet attributes
    [Header("Bullet Attributes")]
    public float damage = 1;

    public Vector3 startingPosition;
    public NetworkObjectReference player;

    public float returnSpeed = 10f;
    public bool isReturning = false;

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
        gameObject.GetComponent<Rigidbody>().isKinematic = false;
        gameObject.GetComponent<Rigidbody>().velocity = direction * speed;
        Debug.DrawLine(transform.position, transform.position + direction * 10, Color.red, 2);
    }

    // Handle collisions
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player") && collision.gameObject != player.TryGet(out NetworkObject playergameObject))
        {
            Debug.Log("Hit another player!");

            // Notify the GameManager that a player scored
            NetworkObjectReference scoringPlayer = player;
            GameManager.Instance.PlayerScored(scoringPlayer);
            return;
        }

        if (collision.gameObject == player.TryGet(out NetworkObject playerObject))
        {   
            if(isReturning){
                Debug.Log("Disc returned to player");
                isReturning = false;
                // Call the return disc function on the child of the player
                if (player.TryGet(out NetworkObject gunObject))
                {
                    gun playerGun = gunObject.GetComponentInChildren<gun>();
                    
                    if (playerGun != null)
                    {
                        playerGun.ReturnDisc();
                    }
                }
                
                Destroy(gameObject);
            }
            return;
        }

        Debug.Log("Hit something else: " + collision.gameObject.name);

            if (player.TryGet(out NetworkObject networkObject))
            {
                networkObject.GetComponentInChildren<gun>().Recall();
            }
        if (collision.gameObject.CompareTag("Ground"))
        {
            if (player.TryGet(out NetworkObject gunObject))
                {
                    gun playerGun = gunObject.GetComponentInChildren<gun>();
                    
                    if (playerGun != null)
                    {
                        playerGun.Recall();
                    }
                }
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

        player.TryGet(out NetworkObject playerObject);


        // Move the bullet towards the player at a set speed
        MoveTowardsObject(player, returnSpeed);
    }

    public NetworkObjectReference GetReference(){
        return new NetworkObjectReference(gameObject.GetComponent<NetworkObject>());
    }
}
