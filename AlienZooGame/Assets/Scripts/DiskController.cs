using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiskController : MonoBehaviour
{
    private LayerMask groundLayer; // platforms you can stand on are part of this layer
    private Transform FrontChecker; // empty object we draw a circle around to detect platforms in front of the player
    private float checkFrontRadius; // radius of circle to detect platforms in front of the player
    private BoxCollider2D hitbox;
    private GameObject player;
    private PlayerInputController playerScript;
    private Animator anim;

    private Vector3 start_pos;
    private bool can_move = false;
    private bool recalling = false;
    private int throw_direction = 0;
    private bool stopped = false;

    //private frames_until_spawned = 

    // Start is called before the first frame update
    

    void Start()
    {
        transform.localScale = new Vector3(Constants.DISK_SCALE, Constants.DISK_SCALE, 1);
        anim = GetComponent<Animator>();

        FrontChecker = transform.Find("FrontChecker");
        groundLayer = LayerMask.GetMask("Ground");
        checkFrontRadius = 0.5f;
        hitbox = GetComponent<BoxCollider2D>();
        player = GameObject.Find("Player");
        playerScript = player.GetComponent<PlayerInputController>();

        gameObject.SetActive(false);

    }

    // Update is called once per frame
    void Update()
    {
        CheckIfCanMove();
        RecallBehavior();
        ThrownBehavior();
    }

    public void ThrownBehavior()
    {
        if (!recalling)
        {
            if (can_move && !stopped && Vector3.Distance(start_pos, transform.position) < Constants.MAX_DISK_DISTANCE)
            {
                transform.position += new Vector3(throw_direction * Constants.DISK_VELOCITY, 0);
            }
            else if (!can_move)
            {
                transform.position -= new Vector3(throw_direction, 0);
                stopped = true;
            }
        }
    }

    public void CheckIfCanMove()
    {
        if (!recalling)
        {
            Collider2D collider = Physics2D.OverlapCircle(FrontChecker.position, checkFrontRadius, groundLayer);
            if (collider != null && collider != hitbox)
            {
                //Debug.Log(collider.name);
                //Debug.Log("Ground in front of disk");
                can_move = false;
            }
            else
            {
                can_move = true;
            }
        }
        else
        {
            can_move = false;
        }
    }

    public void Activate(int direction)
    {
        throw_direction = direction;
        transform.localScale = new Vector3(throw_direction * Constants.DISK_SCALE, Constants.DISK_SCALE, 1);

        //Debug.Log("Throw direction: " + throw_direction);
        Vector3 displacement = new Vector3(throw_direction * 2.5f, -0.5f, 0);
        transform.position = player.transform.position + displacement;
        start_pos = transform.position;
        stopped = false;
    }

    public void Recall()
    {
        recalling = true;
        stopped = false;
        //Debug.Log("Recalling disk");
    }

    private void RecallBehavior()
    {
        if (recalling)
        {
            Vector3 closest_player_pt = player.GetComponent<BoxCollider2D>().ClosestPoint(transform.position);
            Vector3 closest_disk_pt = hitbox.ClosestPoint(closest_player_pt);
            if (Vector3.Distance(closest_disk_pt, closest_player_pt) > Constants.DISK_RECALL_DISTANCE_FROM_PLAYER)
            {
                transform.position = Vector3.Lerp(transform.position, player.transform.position, Constants.DISK_RECALL_LERP_VALUE);
            }
            else
            {
                recalling = false;
                playerScript.ReceiveDisk();
            }
        }
    }

    public bool Bounce()
    {
        if (!recalling)
        {
            anim.SetTrigger("bounce");
            return true;
        }
        else
        {
            return false;
        }
    }
}
