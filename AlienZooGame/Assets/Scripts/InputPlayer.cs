using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.InputSystem;

public class InputPlayer : MonoBehaviour
{
    private Transform trans;
    private Rigidbody2D rb;
    private Animator anim;
    private float checkGroundRadius;
    private Transform GroundChecker;
    private LayerMask groundLayer;

    public const int max_num_jumps = 2;
    public int current_num_jumps = 2;

    public const int max_num_dashes = 1;
    public int current_num_dashes = 1;
    public int dash_direction = 1;
    public float time_dash_pressed = 0;

    private bool on_ground = true;
    private bool moving_horizontally = false;
    private bool jumping = false;
    private bool jump_pressed = false;
    private bool dashing = false;
    

    // Start is called before the first frame update
    void Start()
    {
        trans = transform.parent;
        rb = trans.GetComponent<Rigidbody2D>();
        anim = transform.GetComponent<Animator>();
        GroundChecker = trans.Find("GroundChecker");
        groundLayer = LayerMask.GetMask("Ground");
        checkGroundRadius = 0.5f;

    }

    // Update is called once per frame
    void Update()
    {
        // TODO: figure out how to replicate & what causes bug that makes you move really fast in the air without being able to slow down until you land

        CheckIfGrounded();
        DashBehavior();
        BetterHorizontalBehavior();
        BetterJumpBehavior();
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
            if (x > 0 && rb.velocity.x < Constants.PLAYER_H_MAX_VELOCITY)
            {
                rb.velocity += new Vector2(Constants.PLAYER_H_ACCEL, 0);
                transform.localScale = new Vector3(-1, 1, 1);
            }
            else if (x < 0 && rb.velocity.x < Constants.PLAYER_H_MAX_VELOCITY)
            {
                rb.velocity -= new Vector2(Constants.PLAYER_H_ACCEL, 0);
                transform.localScale = new Vector3(1, 1, 1);
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
        if (!dashing)
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

    public void JumpBehavior()
    {
        if (on_ground && !jumping)
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
            if (!jumping)
                current_num_jumps = max_num_jumps;
            if (!dashing)
                current_num_dashes = max_num_dashes;
            //Debug.Log("Jumps reset b/c on ground and not jumping");
            //Debug.Log("Dash reset b/c on ground and not dashing");
        }
        else
        {
            on_ground = false;
            //Debug.Log("Not on ground");
        }
        anim.SetBool("on_ground", on_ground);
    }


    ////////////////////////////////////////////////////////////////////////////
    /////////////////////////////// INPUT ACTIONS //////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    public void Move(InputAction.CallbackContext context)
    {
        //Debug.Log("Input Action Move() called");

        float x_direction = context.ReadValue<Vector2>().x;
        HorizontalBehavior(x_direction);
    }

    public void Jump(InputAction.CallbackContext context)
    {
        //Debug.Log("Jump is in phase " + context.phase + " and jumping = " + jumping + " and current_num_jumps = " + current_num_jumps);
        if (context.started || context.performed)
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
        if (context.started || context.performed)
        {
            //Debug.Log("Dash pressed");
            if (current_num_dashes > 0)
            {
                dashing = true;
                current_num_dashes--;
                dash_direction = -(int)transform.localScale.x;
                anim.SetTrigger("dash_pressed");
                //Debug.Log("Dash initiated; animation trigger sent");
                time_dash_pressed = Time.time;
            }

        }
    }

    ////////////////////////////////////////////////////////////////////////////
    //////////////////////////// ANIMATION RECEIVERS ///////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    public void StartJumpAnimReceiver()
    {
        //Debug.Log("Jump animation triggered");
        if (current_num_jumps == 0)
            rb.velocity = new Vector2(rb.velocity.x, Constants.PLAYER_DOUBLE_JUMP_VELOCITY);
        else
            rb.velocity = new Vector2(rb.velocity.x, Constants.PLAYER_JUMP_VELOCITY);
    }

    public void FinishedJumpAnimReceiver() { jumping = false; }
    public void FinishedDashAnimReceiver()
    {
        dashing = false;
        //    Debug.Log("Speed: " + rb.velocity.x);
        //    Debug.Log("Time since dash was pressed: " + (Time.time - time_dash_pressed));
    }

}