using UnityEngine;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;

public class bullet : NetworkBehaviour
{

    public bool hitAnotherPlayer = false;
    [ClientRpc]
    void UpdateReturningClientRpc(bool isReturning)
    {
        this.isReturning = isReturning;
    }

    [ServerRpc(RequireOwnership = false)]
    void UpdateReturningServerRpc(bool isReturning)
    {
        this.isReturning = isReturning;
    }
    [ClientRpc]
    public void ReturnDiscClientRpc()
    {
        if(IsOwner){

            //Find the player with the same client id as this script
            NetworkManager.Singleton.ConnectedClients.TryGetValue(OwnerClientId, out var networkClient);
            if (networkClient != null)
            {
                //find the player object with the same owner as this script
                NetworkObject playerNetworkObject = networkClient.PlayerObject;
                // Call the return disc function on the child of the player
                if (playerNetworkObject)
                {
                    playerNetworkObject.GetComponentInChildren<gun>().ReturnDiscServerRpc();
                    playerNetworkObject.GetComponentInChildren<gun>().canShoot = true;
                }
            }
            //find the player object with the same owner as this script
            player.TryGet(out NetworkObject playerObject);
            // Call the return disc function on the child of the player
            if (playerObject)
            {
                playerObject.GetComponentInChildren<gun>().ReturnDiscServerRpc();
                playerObject.GetComponentInChildren<gun>().canShoot = true;
            }
        }
    }

    // Bullet attributes
    [Header("Bullet Attributes")]
    public float damage = 1;

    public Vector3 startingPosition;
    public NetworkObjectReference player;
    public GameObject localPlayer;  

    public float returnSpeed = 10f;
    public bool isReturning = false;

    void Update()
    {
        if (isReturning)
        {
            if (player.TryGet(out NetworkObject playerObject))
            {
                localPlayer = playerObject.gameObject;
                MoveTowardsObject(playerObject.gameObject, returnSpeed);
            }
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
    public void OnCollisionEnter(Collision collision)
    {
        player.TryGet(out NetworkObject playerObject);
        if (collision.gameObject.CompareTag("Player") && collision.gameObject != playerObject.gameObject && !hitAnotherPlayer)
        {
            hitAnotherPlayer = true;
            Debug.Log("Hit another player!");

            // Notify the GameManager that a player scored
            NetworkObjectReference scoringPlayer = player;
            GameManager.Instance.PlayerScored(scoringPlayer);
            return;
        }

        Debug.Log("Hit something else: " + collision.gameObject.name + " isReturning: " + isReturning + " player: " + player);


        if (collision.gameObject == playerObject.gameObject)
        {   
            Debug.Log("Hit the player that threw it isReturning: " + isReturning);        
            if(isReturning){
                gun gunComponent = playerObject.GetComponentInChildren<gun>();
                if (gunComponent != null)
                {
                    NetworkObject gunNetworkObject = gunComponent.GetComponentInParent<NetworkObject>();
                    if (gunNetworkObject != null)
                    {
                        ReturnDiscClientRpc();
                    }
                    else
                    {
                        Debug.LogError("NetworkObject component not found in gun component.");
                    }
                }
                else
                {
                    Debug.LogError("Gun component not found in player object.");
                }
                
                
                // Call the return disc function on the child of the player
                if (playerObject)
                {
                    Debug.Log("Returning Disc");    
                    playerObject.GetComponentInChildren<gun>().ReturnDiscServerRpc();
                    playerObject.GetComponentInChildren<gun>().canShoot = true;
                }

                isReturning = false;
                
                Destroy(gameObject);
            }
            return;
        }

        Debug.Log("Hit something else: " + collision.gameObject.name);

        if (player.TryGet(out NetworkObject networkObject))
        {
            networkObject.GetComponentInChildren<gun>().Recall();
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
        //Make the bullet kinematic
        gameObject.GetComponent<Rigidbody>().isKinematic = true;
        isReturning = true;
        UpdateReturningClientRpc(isReturning);
        if (IsServer)
        {
            UpdateReturningClientRpc(isReturning);
        }
        else
        {
            UpdateReturningServerRpc(isReturning);
        }
        if (player.TryGet(out NetworkObject playerObject))
        {
            // Make sure the bullet is not kinematic
            gameObject.GetComponent<Rigidbody>().isKinematic = true;
            // Move the bullet towards the player at a set speed
            MoveTowardsObject(playerObject.gameObject, returnSpeed);
        }
    }

    public NetworkObjectReference GetReference(){
        return new NetworkObjectReference(gameObject.GetComponent<NetworkObject>());
    }
}
