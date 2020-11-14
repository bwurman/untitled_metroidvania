using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHitboxController : MonoBehaviour
{
    BoxCollider2D hitbox;
    PlayerHealthController phc;
    private int groundLayer; // platforms you can stand on are part of this layer
    private int hazardLayer; // platforms you can stand on are part of this layer

    // Start is called before the first frame update
    void Start()
    {
        hitbox = GetComponent<BoxCollider2D>();
        groundLayer = LayerMask.NameToLayer("Ground");
        hazardLayer = LayerMask.NameToLayer("Hazards");
        Debug.Log("hazardLayer = " + hazardLayer);
        Debug.Log("grounddLayer = " + groundLayer);

        phc = GetComponent<PlayerHealthController>();
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        //Debug.Log("Collision with object " + collision.gameObject.name + " with layer " + collision.gameObject.layer);
        if (collision.gameObject.layer == hazardLayer)
        {
            phc.HazardDamage();
        }
        else if (collision.gameObject.layer == groundLayer)
        {
            //Debug.Log("Touching ground");
        }
    }
}
