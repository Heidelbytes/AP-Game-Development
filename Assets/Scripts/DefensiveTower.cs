using UnityEngine;

public class DefensiveTower : MonoBehaviour
{
    [Header("Stats")]
    public int health;
    public int attackDamage;

    // This is called by the EnemyAI script when it attacks
    public void TakeDamage(EnemyAI enemy)
    {   

        health -= enemy.attackDamage;

        if(health <= 0) 
        {
            // 1. Instantly hide and disable the tower so NavMesh doesn't see it
            gameObject.SetActive(false); 

            // 2. Trigger the NavMesh to rebake paths now that this obstacle is gone
            RuntimeNavMesh runtimeNav = FindObjectOfType<RuntimeNavMesh>();
            if (runtimeNav != null)
            {
                runtimeNav.BakeMapAtRuntime();
            }

            // 3. Completely delete the object from memory
            Destroy(gameObject);
            }
    
    }

}