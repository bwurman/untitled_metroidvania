using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealthController : MonoBehaviour
{
    private BoxCollider2D hitbox;
    private Rigidbody2D rb; // rigidbody
    private Transform spawn_point;
    public PlayerModel pm;
    private PlayerInputController pic;
    public Transform soft_spawn_point; // last piece of stable ground

    // TODO: Add health bar
    //       Soft respawn and hard respawn animations
    //       Hard-respawn behavior (game over screen / load last real checkpoint)
    
    // Start is called before the first frame update
    void Start()
    {
        hitbox = GetComponent<BoxCollider2D>();
        rb = GetComponent<Rigidbody2D>();
        spawn_point = GameObject.Find("SpawnPoint").transform;
        soft_spawn_point = GameObject.Find("SoftSpawnPoint").transform;
        pic = GetComponent<PlayerInputController>();
        pm = pic.pm;
        transform.position = spawn_point.position;
    }

    public void FixedUpdate()
    {
        if (pm.current_health <= 0)
        {
            Death();
        }
    }

    public void EnemyDamage()
    {

    }

    public void HazardDamage()
    {
        Debug.Log("Hazard Damage");
        pm.current_health--;
        transform.position = soft_spawn_point.position;
        rb.velocity = Vector2.zero;

    }

    public void Death()
    {
        // TODO: respawn at this game's equivalent of bonfires/benches/placable respawn points
        transform.position = spawn_point.position;
        rb.velocity = Vector2.zero;
        pm.current_health = pm.max_health;
    }

}
