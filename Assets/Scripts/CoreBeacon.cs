using UnityEngine;

public class CoreBeacon : MonoBehaviour
{
    public void TakeDamage()
    {
        Debug.Log("The Core Beacon has been destroyed! GAME OVER!");
        
        // Disable the core beacon
        gameObject.SetActive(false);
        
        // TODO: Trigger your UI Game Over screen here!
        
        Destroy(gameObject);
    }
}