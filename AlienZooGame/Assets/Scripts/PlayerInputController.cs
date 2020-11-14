using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.InputSystem;

public class PlayerInputController: MonoBehaviour
{
    private Rigidbody2D rb; // rigidbody
    private Animator anim; // sprite animator
    private LayerMask groundLayer; // platforms you can stand on are part of this layer
    private Transform GroundChecker; // empty object we draw a circle around to detect platforms below the player
    private float checkGroundRadius; // radius of circle to detect platforms below the player
    private Transform FrontChecker; // empty object we draw a circle around to detect platforms in front of the player
    private float checkFrontRadius; // radius of circle to detect platforms in front of the player
    public PlayerModel pm;
    public Transform soft_spawn_point; // last piece of stable ground

    public int current_num_jumps = 2; // how many jumps can the player do right now?

    public int current_num_dashes = 1; // how many dashes does the player have left?
    public int dash_direction = 1; // what direction did the player dash in?
    public float time_dash_pressed = 0; // for debugging

    public int wall_direction = 1; // left = 1, right = -1
    private bool on_ground = true; // is platform below player's feet?
    private bool on_wall = false; // is platform in front of player?
    private float last_time_on_ground = 0;
    private float last_time_on_wall = 0;

    private bool moving_horizontally = false; // is player pushing joystick left/right?
    private bool jump_pressed = false; // is player currently pressing down the jump button?
    private bool jumping = false; // is player mid-jump animation?
    private bool dashing = false; // is player mid-dash animation?

    private GameObject disk;
    private DiskController diskScript;
    private LayerMask diskLayer; // your thrown disk is part of this layer, you can bounce off it
    private bool can_throw_disk = true;
    private bool is_throwing_disk = false;
    private bool on_disk = false;
    private float checkDiskRadius; // radius of circle to detect disk below the player
    private int disk_throw_direction = 0;

    void Awake()
    {
        pm = new PlayerModel(2, 1, 10);
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        groundLayer = LayerMask.GetMask("Ground");

        GroundChecker = transform.Find("GroundChecker");
        checkGroundRadius = 0.5f;

        FrontChecker = transform.Find("FrontChecker");
        checkFrontRadius = 0.6f;

        soft_spawn_point = GameObject.Find("SoftSpawnPoint").transform;

        disk = GameObject.Find("PlayerThrownDisk");
        diskScript = disk.GetComponent<DiskController>();
        diskLayer = LayerMask.GetMask("PlayerDisk");
        checkDiskRadius = 0.7f;
    }

    void FixedUpdate()
    {
        // TODO: figure out how to replicate & what causes bug that makes you move really fast in the air without being able to slow down until you land

        CheckIfGrounded();
        CheckIfWallGrab();
        CheckIfBounceOffDisk();

        BetterHorizontalBehavior();
        BetterJumpBehavior();
        BetterWallSlideBehavior();

        DashBehavior();
        DiskThrowBehavior();
    }


    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////// BASIC MOVEMENT //////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    public void HorizontalBehavior(float x)
    {
        //Debug.Log("Pushing on joystick");
        if (Math.Abs(x) > 0.15)
        {
            anim.SetBool("pushed_left_or_right", true);
            moving_horizontally = true;
            // If x != 0, move
            //Debug.Log("Current x velocity: " + rb.velocity.x + " vs max velocity: " + Constants.PLAYER_H_MAX_VELOCITY);
            if (x > 0)
            {
                //Debug.Log("Pushing right " + Time.time + " with current speed = " + rb.velocity.x + " and max speed " + Constants.PLAYER_H_MAX_VELOCITY);
                //Debug.Log("Currently dashing = " + dashing + " and on_wall = " + on_wall);

                // pushing right
                if (rb.velocity.x < Constants.PLAYER_H_MAX_VELOCITY)
                    rb.velocity += new Vector2(Constants.PLAYER_H_ACCEL, 0);
                if (!dashing && !on_wall && !is_throwing_disk)
                {
                    transform.localScale = new Vector3(-Constants.PLAYER_SCALE, Constants.PLAYER_SCALE, 1);
                    //FrontChecker.localPosition = new Vector3(Math.Abs(FrontChecker.localPosition.x) * Constants.PLAYER_SCALE, FrontChecker.localPosition.y, 0);
                }
            }
            else if (x < 0)
            {
                //Debug.Log("Pushing left " + Time.time + " with current speed = " + rb.velocity.x + " and max speed " + Constants.PLAYER_H_MAX_VELOCITY);
                //Debug.Log("Currently dashing = " + dashing + " and on_wall = " + on_wall);

                // pushing left
                if (rb.velocity.x > -Constants.PLAYER_H_MAX_VELOCITY)
                    rb.velocity -= new Vector2(Constants.PLAYER_H_ACCEL, 0);
                if (!dashing && !on_wall && !is_throwing_disk)
                {
                    transform.localScale = new Vector3(Constants.PLAYER_SCALE, Constants.PLAYER_SCALE, 1);
                    //FrontChecker.localPosition = new Vector3(-Math.Abs(FrontChecker.localPosition.x) * Constants.PLAYER_SCALE, FrontChecker.localPosition.y, 0);
                }
            }

        }
        else
        {
            anim.SetBool("pushed_left_or_right", false);
            moving_horizontally = false;
        }

    }

    void BetterHorizontalBehavior()
    {
        if (!dashing && !is_throwing_disk)
        {
            if (rb.velocity.x > Constants.PLAYER_H_MAX_VELOCITY)
            {
                rb.velocity -= new Vector2(Constants.PLAYER_H_MAX_DECEL, 0);
            }
            else if (rb.velocity.x < -Constants.PLAYER_H_MAX_VELOCITY)
            {
                rb.velocity += new Vector2(Constants.PLAYER_H_MAX_DECEL, 0);
            }
            else if (!moving_horizontally)
            {
                //Debug.Log("Not pushing on joystick");
                if (on_ground)
                {
                    //Debug.Log("on ground; using ground deceleration");
                    if (rb.velocity.x > Constants.PLAYER_H_GROUND_DECEL)
                        rb.velocity -= new Vector2(Constants.PLAYER_H_GROUND_DECEL, 0);
                    else if (rb.velocity.x < -Constants.PLAYER_H_GROUND_DECEL)
                        rb.velocity += new Vector2(Constants.PLAYER_H_GROUND_DECEL, 0);
                    else
                        rb.velocity = new Vector2(0, rb.velocity.y);
                }
                else
                {
                    //Debug.Log("in air; using air deceleration");
                    if (rb.velocity.x > Constants.PLAYER_H_AIR_DECEL)
                        rb.velocity -= new Vector2(Constants.PLAYER_H_AIR_DECEL, 0);
                    else if (rb.velocity.x < -Constants.PLAYER_H_AIR_DECEL)
                        rb.velocity += new Vector2(Constants.PLAYER_H_AIR_DECEL, 0);
                    else
                        rb.velocity = new Vector2(0, rb.velocity.y);
                }
            }
        }
    }

    public void BetterWallSlideBehavior()
    {
        if (on_wall && !dashing && !jumping)
        {
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Clamp(rb.velocity.y, -Constants.PLAYER_WALL_SLIDE_SPEED, 10f));
        }
    }

    public void JumpBehavior()
    {
        if ((on_ground || on_wall) && !jumping)
        {
            anim.SetTrigger("jump_pressed");
            current_num_jumps--;
            jumping = true;
            //Debug.Log("Ground jump");
        }
        else
        {
            if (current_num_jumps >= 2)
                current_num_jumps--; // if fell, can't double jump
            if (current_num_jumps > 0 && !jumping)
            {
                //Debug.Log("Air jump");
                anim.SetTrigger("air_jump_pressed");
                current_num_jumps--;
                jumping = true;
            }
        }
    }


    public void BetterJumpBehavior()
    {
        if (rb.velocity.y < 0)
        {
            rb.velocity += Vector2.up * Physics2D.gravity * (Constants.PLAYER_FALL_MULTIPLIER - 1) * Time.deltaTime;
            if (rb.velocity.y < Constants.PLAYER_V_MAX_VELOCITY)
                rb.velocity = new Vector2(rb.velocity.x, Constants.PLAYER_V_MAX_VELOCITY);

        }
        else if (rb.velocity.y > 0 && !jump_pressed)
        {
            rb.velocity += Vector2.up * Physics2D.gravity * (Constants.PLAYER_LOW_JUMP_MULTIPLIER - 1) * Time.deltaTime;

        }
    }

    public void DashBehavior()
    {
        if (dashing)
        {
            rb.velocity = new Vector2(Constants.PLAYER_DASH_SPEED * dash_direction, 0);
            //Debug.Log("Dashing");
        }
    }
    public void DiskThrowBehavior()
    {
        if (is_throwing_disk)
        {
            rb.velocity = rb.velocity / 1.1f;
            //if (!on_ground && !on_wall)
            //{
            //    rb.velocity += new Vector2(-Utilities.FloatToIntCode(transform.localScale.x) * 5, 0);
            //    Debug.Log("In air; moving sideways while throwing");
            //}
        }
    }

    ////////////////////////////////////////////////////////////////////////////
    //////////////////////////// SURFACE DETECTION /////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    public void CheckIfGrounded()
    {
        //Debug.Log("Ground checker position: " + GroundChecker.position);
        //Debug.Log("Ground checker radius: " + checkGroundRadius);

        Collider2D collider = Physics2D.OverlapCircle(GroundChecker.position, checkGroundRadius, groundLayer);
        if (collider != null)
        {
            on_ground = true;
            last_time_on_ground = Time.time;
            if (!jumping)
                current_num_jumps = pm.max_num_jumps;
            if (!dashing)
                current_num_dashes = pm.max_num_dashes;
            //Debug.Log("Jumps reset b/c on ground and not jumping");
            //Debug.Log("Dash reset b/c on ground and not dashing");
            CheckIfStableGround(collider);
        }
        else
        {
            if (Time.time - last_time_on_ground >= Constants.PLAYER_JUMP_LEEWAY_TIME)
                on_ground = false;
            else
                on_ground = true;
            //Debug.Log("Not on ground");
        }
        anim.SetBool("on_ground", on_ground);
    }

    public void CheckIfStableGround(Collider2D ground)
    {
        Vector3 closest_point = ground.ClosestPoint(transform.position);
        Vector3 top_right = new Vector3(ground.bounds.center.x + ground.bounds.extents.x, ground.bounds.center.y + ground.bounds.extents.y, 0);
        Vector3 top_left = new Vector3(ground.bounds.center.x - ground.bounds.extents.x, ground.bounds.center.y + ground.bounds.extents.y, 0);
        Vector3 nearest_hazard = GetClosestHazard().GetComponent<BoxCollider2D>().ClosestPoint(transform.position);
        bool far_from_ledges = Vector3.Distance(closest_point, top_right) > Constants.MIN_SAFE_DISTANCE_FROM_LEDGE &&
            Vector3.Distance(closest_point, top_left) > Constants.MIN_SAFE_DISTANCE_FROM_LEDGE;
        bool on_small_platform = Vector3.Distance(top_right, top_left) < 2 * Constants.MIN_SAFE_DISTANCE_FROM_LEDGE;

        bool far_from_hazard = Mathf.Abs(nearest_hazard.x - transform.position.x) > Constants.MIN_SAFE_X_DISTANCE_FROM_HAZARD ||
                               Mathf.Abs(nearest_hazard.y - transform.position.y) > Constants.MIN_SAFE_Y_DISTANCE_FROM_HAZARD;

        if (far_from_ledges && far_from_hazard)
            soft_spawn_point.position = transform.position;
        else if (on_small_platform)
        {
            float x_val = top_right.x + (top_left.x - top_right.x) / 2;
            soft_spawn_point.position = new Vector3(x_val, transform.position.y);
        }
    }

    public void CheckIfWallGrab()
    {
        //Debug.Log("Front checker position: " + FrontChecker.position);
        //Debug.Log("Front checker radius: " + checkFrontRadius);

        Collider2D collider = Physics2D.OverlapCircle(FrontChecker.position, checkFrontRadius, groundLayer);
        if (collider != null && !on_ground && moving_horizontally)
        {
            on_wall = true;
            last_time_on_wall = Time.time;
            if (!jumping)
                current_num_jumps = pm.max_num_jumps;
            if (!dashing)
                current_num_dashes = pm.max_num_dashes;
            //Debug.Log("Jumps reset b/c on wall and not jumping");
            //Debug.Log("Dash reset b/c on wall and not dashing");
            wall_direction = -Utilities.FloatToIntCode(transform.localScale.x);
        }
        else
        {
            if (Time.time - last_time_on_wall >= Constants.PLAYER_JUMP_LEEWAY_TIME)
            {
                on_wall = false;
            }
            else
            {
                on_wall = true;
                wall_direction = -Utilities.FloatToIntCode(transform.localScale.x);
                //transform.localScale = new Vector3(last_wall_direction * Constants.PLAYER_SCALE, transform.localScale.y * Constants.PLAYER_SCALE);
            }
            //Debug.Log("Not on ground");
        }
        anim.SetBool("on_wall", on_wall);
    }

    public void CheckIfBounceOffDisk()
    {
        //Debug.Log("Ground checker position: " + GroundChecker.position);
        //Debug.Log("Ground checker radius: " + checkGroundRadius);

        if (!jumping)
        {
            Debug.Log("Not jumping");
            Collider2D collider = Physics2D.OverlapCircle(GroundChecker.position, checkDiskRadius, diskLayer);
            if (collider != null)
            {
                Debug.Log("On disk");
                if (diskScript.Bounce())
                {
                    Debug.Log("diskScript.Bounce() returned true");
                    anim.SetTrigger("bounced_off_disk");
                    jumping = true;
                    on_disk = true;
                }
                else
                {
                    Debug.Log("diskScript.Bounce() returned false");
                    on_disk = false;
                }
            }
            else
            {
                on_disk = false;
            }
        }
    }

    GameObject GetClosestHazard()
    {
        GameObject[] hazards = GameObject.FindGameObjectsWithTag("Hazard");
        GameObject closest_hazard = null;
        float closestDistanceSqr = Mathf.Infinity;
        Vector3 currentPosition = transform.position;
        foreach (GameObject current_nearest in hazards)
        {
            Vector3 directionToTarget = current_nearest.transform.position - currentPosition;
            float dSqrToTarget = directionToTarget.sqrMagnitude;
            if (dSqrToTarget < closestDistanceSqr)
            {
                closestDistanceSqr = dSqrToTarget;
                closest_hazard = current_nearest;
            }
        }

        return closest_hazard;
    }



    ////////////////////////////////////////////////////////////////////////////
    /////////////////////////////// INPUT ACTIONS //////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    public void Move(InputAction.CallbackContext context)
    {

        float x_direction = context.ReadValue<Vector2>().x;
        //Debug.Log("Input Action Move() called with x_direction = " + x_direction + " and on_wall = " + on_wall);

        HorizontalBehavior(x_direction);
    }

    public void Jump(InputAction.CallbackContext context)
    {
        //Debug.Log("Jump is in phase " + context.phase + " and jumping = " + jumping + " and current_num_jumps = " + current_num_jumps);
        //Debug.Log("Pressed jump");
        if ((context.started || context.performed) && !dashing)
        {
            jump_pressed = true;
            JumpBehavior();
        }
        else 
        {
            jump_pressed = false;
        }
    }

    public void Dash(InputAction.CallbackContext context)
    {
        if ((context.started || context.performed) && !jumping)
        {
            //Debug.Log("Dash pressed");
            if (current_num_dashes > 0)
            {
                dashing = true;
                current_num_dashes--;

                if (on_wall)
                {
                    // have to dash away from wall if on wall
                    dash_direction = -wall_direction;
                    if (Utilities.FloatToIntCode(transform.localScale.x) == dash_direction)
                        transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
                }
                else
                {
                    dash_direction = -Utilities.FloatToIntCode(transform.localScale.x);
                }


                anim.SetTrigger("dash_pressed");
                //Debug.Log("Dash initiated; animation trigger sent");
                time_dash_pressed = Time.time;
            }

        }
    }

    public void ThrowDisk(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (can_throw_disk)
            {
                if (on_wall)
                {
                    // have to dash away from wall if on wall
                    disk_throw_direction = -wall_direction;
                    //Debug.Log("On wall and throwing disk:");
                    //if (wall_direction == 1)
                    //    Debug.Log("wall direction is left  <<<------ and x-velocity is " + rb.velocity.x);
                    //else if (wall_direction == -1)
                    //    Debug.Log("wall direction is right ------>>> and x-velocity is " + rb.velocity.x);
                    //else
                    //    Debug.Log("wall direction is neutral????");

                    //if (disk_throw_direction == 1)
                    //    Debug.Log("disk_throw_direction direction is left  <<<------ and x-velocity is " + rb.velocity.x);
                    //else if (disk_throw_direction == -1)
                    //    Debug.Log("disk_throw_direction direction is right ------>>> and x-velocity is " + rb.velocity.x);
                    //else
                    //    Debug.Log("disk_throw_direction direction is neutral????");

                    if (Utilities.FloatToIntCode(transform.localScale.x) == disk_throw_direction)
                    {
                        transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
                        //Debug.Log("After change, facing direction " + Utilities.FloatToIntCode(transform.localScale.x) + ";    1 = left;    -1 = right");
                    }
                }
                else
                {
                    disk_throw_direction = -Utilities.FloatToIntCode(transform.localScale.x);
                }

                anim.SetTrigger("disk_throw_pressed");
                //Debug.Log("Right trigger pressed and can throw disk; starting animation");
            }
            else
            {
                diskScript.Recall();
                //Debug.Log("Right trigger pressed and can't throw disk; recalling disk");
            }
        }
    }
    public void ReceiveDisk()
    {
        can_throw_disk = true;
        disk.SetActive(false);
        //Debug.Log("Disk has returned to player");
    }

    ////////////////////////////////////////////////////////////////////////////
    //////////////////////////// ANIMATION RECEIVERS ///////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    public void StartJumpAnimReceiver()
    {
        //Debug.Log("Jump animation triggered");
        if (on_ground)
        {
            rb.velocity = new Vector2(rb.velocity.x, Constants.PLAYER_JUMP_V_VELOCITY);
        }
        else if (on_wall)
        {
            rb.velocity = new Vector2(-wall_direction * Constants.PLAYER_WALL_JUMP_H_VELOCITY, Constants.PLAYER_WALL_JUMP_V_VELOCITY);
            //if (wall_direction == 1)
            //    Debug.Log("wall direction is left  <<<------ and x-velocity is " + rb.velocity.x);
            //else if (wall_direction == -1)
            //    Debug.Log("wall direction is right ------>>> and x-velocity is " + rb.velocity.x);
            //else
            //    Debug.Log("wall direction is neutral????");
        }
        else if (on_disk)
        {
            rb.velocity = new Vector2(rb.velocity.x, Constants.DISK_BOUNCE_VELOCITY);
        }
        else
        {
            rb.velocity = new Vector2(rb.velocity.x, Constants.PLAYER_DOUBLE_JUMP_V_VELOCITY);
        }
    }

    public void FinishedJumpAnimReceiver() { jumping = false; }
    public void FinishedDashAnimReceiver()
    {
        dashing = false;
        //    Debug.Log("Speed: " + rb.velocity.x);
        //    Debug.Log("Time since dash was pressed: " + (Time.time - time_dash_pressed));
    }

    public void StartedDiskThrowAnimReceiver()
    {
        can_throw_disk = false;
        is_throwing_disk = true;
    }
    public void FinishedDiskThrowAnimReceiver()
    {
        disk.SetActive(true);
        diskScript.Activate(disk_throw_direction);
        is_throwing_disk = false;
        //Debug.Log("Disk-throwing animation finished; activating disk");
    }

}