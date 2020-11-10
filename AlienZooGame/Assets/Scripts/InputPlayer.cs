using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.InputSystem;

public class InputPlayer : MonoBehaviour
{
    public const float h_acceleration = 0.75f;
    public const float h_air_deceleration = 0.25f;
    public const float h_ground_deceleration = 0.75f;
    public const float h_max_speed = 12f;
    private bool moving_horizontally = false;

    public const float jump_force = 12.5f;
    public const float double_jump_force = 9f;
    public const float fall_multiplier = 2.75f;
    public const float low_jump_multiplier = 2f;
    public const float terminal_velocity = -40f;

    public const int max_num_jumps = 2;
    public int current_num_jumps = 2;
    private bool jumping = false;
    private bool jump_pressed = false;
    private Transform trans;
    private Rigidbody2D rb;
    private Animator anim;

    private bool on_ground = true;
    private float checkGroundRadius;
    private Transform GroundChecker;
    private LayerMask groundLayer;

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
        BetterHorizontalBehavior();
        BetterJumpBehavior();
    }

    public void HorizontalBehavior(float x)
    {
        //Debug.Log("Pushing on joystick");
        if (Math.Abs(x) > 0.15)
        {
            float old_velocity = rb.velocity.x;
            anim.SetBool("pushed_left_or_right", true);
            moving_horizontally = true;
            // If x != 0, move
            if (Math.Abs(rb.velocity.x) < h_max_speed)
            {
                rb.velocity += new Vector2(x * h_acceleration, 0);
            }

            float new_velocity = rb.velocity.x;
            if ((old_velocity < 0 && new_velocity > 0) || (old_velocity > 0 && new_velocity < 0))
                anim.SetTrigger("swapped_direction");

            // Face the correct direction
            if (x < 0)
                transform.localScale = new Vector3(1, 1, 1);
            else if (x > 0)
                transform.localScale = new Vector3(-1, 1, 1);

        }
        else
        {
            anim.SetBool("pushed_left_or_right", false);
            moving_horizontally = false;
        }

    }

    void BetterHorizontalBehavior()
    {
        if (!moving_horizontally)
        {
            //Debug.Log("Not pushing on joystick");
            if (on_ground)
            {
                //Debug.Log("on ground; using ground deceleration");
                if (rb.velocity.x > h_ground_deceleration)
                    rb.velocity -= new Vector2(h_ground_deceleration, 0);
                else if (rb.velocity.x < -h_ground_deceleration)
                    rb.velocity += new Vector2(h_ground_deceleration, 0);
                else
                    rb.velocity = new Vector2(0, rb.velocity.y);
            }
            else
            {
                //Debug.Log("in air; using air deceleration");
                if (rb.velocity.x > h_air_deceleration)
                    rb.velocity -= new Vector2(h_air_deceleration, 0);
                else if (rb.velocity.x < -h_air_deceleration)
                    rb.velocity += new Vector2(h_air_deceleration, 0);
                else
                    rb.velocity = new Vector2(0, rb.velocity.y);
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
            rb.velocity += Vector2.up * Physics2D.gravity * (fall_multiplier - 1) * Time.deltaTime;
            if (rb.velocity.y < terminal_velocity)
                rb.velocity = new Vector2(rb.velocity.x, terminal_velocity);

        }
        else if (rb.velocity.y > 0 && !jump_pressed)
        {
            rb.velocity += Vector2.up * Physics2D.gravity * (low_jump_multiplier - 1) * Time.deltaTime;

        }
    }


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
            //Debug.Log("Jumps reset b/c on ground");
        }
        else
        {
            on_ground = false;
            Debug.Log("Not on ground");
        }
        anim.SetBool("on_ground", on_ground);
    }

    public void StartJumpAnimReceiver()
    {
        //Debug.Log("Jump animation triggered");
        if (current_num_jumps == 0)
            rb.velocity = new Vector2(rb.velocity.x, double_jump_force);
        else
            rb.velocity = new Vector2(rb.velocity.x, jump_force);
    }

    public void FinishedJumpAnimReceiver()
    {
        jumping = false;
        //Debug.Log("Jump animation finished");
    }

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
}