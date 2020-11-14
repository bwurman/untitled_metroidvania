using UnityEngine;
using System.Collections;
using System;
using UnityEngine.InputSystem;

public class CameraMovementController : MonoBehaviour
{
    public Transform player;
    private BoxCollider2D cameraBox;
    private bool looking = false;

    private const float camera_push_distance = 30;
    private const float camera_max_snap_velocity = 0.3f;
    private const float camera_max_look_velocity = 0.1f;
    private const float camera_acceleration = 0.05f;
    private float camera_current_velocity = 0f;
    private GameObject previous_boundary;
    bool snapping_back = false;

    // Use this for initialization
    void Start()
    {
        cameraBox = GetComponent<BoxCollider2D>();
        player = GameObject.Find("Player").transform;

        transform.position = player.position + new Vector3(0, 6, -27);

        previous_boundary = GameObject.Find("Boundary");
    }

    // Update is called once per frame
    void Update()
    {
        AspectRatioBoxChange();
        FollowPlayer();
    }

    void AspectRatioBoxChange() // For camera size 7.1!!!!
    {
        // 16:10
        if (Camera.main.aspect >= 1.6f && Camera.main.aspect < 1.7f)
            cameraBox.size = new Vector2(23f, 14.3f);

        // 16:9
        if (Camera.main.aspect >= 1.7f && Camera.main.aspect < 1.8f)
            cameraBox.size = new Vector2(25.47f, 14.3f);

        // 5:4
        if (Camera.main.aspect >= 1.25f && Camera.main.aspect < 1.3f)
            cameraBox.size = new Vector2(18f, 14.3f);

        // 4:3
        if (Camera.main.aspect >= 1.3f && Camera.main.aspect < 1.4f)
            cameraBox.size = new Vector2(19.13f, 14.3f);

        // 3:2
        if (Camera.main.aspect >= 1.5f && Camera.main.aspect < 1.6f)
            cameraBox.size = new Vector2(21.6f, 14.3f);

    }

    void FollowPlayer()
    {
        if (!looking)
        {
            GameObject current_boundary = GameObject.Find("Boundary");
            if (current_boundary)
            {
                BoxCollider2D current_boundBox = current_boundary.GetComponent<BoxCollider2D>();
                Vector3 new_pos = new Vector3(Mathf.Clamp(player.position.x, current_boundBox.bounds.min.x + cameraBox.size.x / 2, current_boundBox.bounds.max.x - cameraBox.size.x / 2),
                                              Mathf.Clamp(player.position.y + 6, current_boundBox.bounds.min.y + cameraBox.size.y / 2, current_boundBox.bounds.max.y - cameraBox.size.y / 2),
                                              transform.position.z);
                if (current_boundary != previous_boundary)
                {
                    // Have to snap back
                    snapping_back = true;
                }

                if (snapping_back)
                {
                    transform.position = Vector3.Lerp(transform.position, new_pos, Mathf.SmoothStep(0, 1, camera_current_velocity));
                    camera_current_velocity = Math.Min(camera_current_velocity + camera_acceleration, camera_max_snap_velocity);
                    if (Vector3.Distance(new_pos, transform.position) < 0.1f)
                    {
                        camera_current_velocity = 0;
                        snapping_back = false;
                    }
                }
                else
                {
                    transform.position = new_pos;
                }

                previous_boundary = current_boundary;
            }
            else
            {
                Debug.Log("ERROR: CAMERA SEES NO BOUNDARY");
            }
        }
    }

    public void Look(InputAction.CallbackContext context)
    {
        if (context.phase != InputActionPhase.Canceled && context.phase != InputActionPhase.Disabled && context.phase != InputActionPhase.Waiting)
        {
            Debug.Log("Input Action Look() called");
            looking = true;
            snapping_back = false;

            Vector2 direction = context.ReadValue<Vector2>();
            GameObject current_boundary = GameObject.Find("Boundary");
            if (current_boundary)
            {
                BoxCollider2D current_boundBox = current_boundary.GetComponent<BoxCollider2D>();
                Vector3 new_pos = new Vector3(Mathf.Clamp(player.position.x + camera_push_distance * direction.x, current_boundBox.bounds.min.x + cameraBox.size.x / 2, current_boundBox.bounds.max.x - cameraBox.size.x / 2),
                                              Mathf.Clamp(player.position.y + 6 + camera_push_distance * direction.y, current_boundBox.bounds.min.y + cameraBox.size.y / 2, current_boundBox.bounds.max.y - cameraBox.size.y / 2),
                                              transform.position.z);

                transform.position = Vector3.Lerp(transform.position, new_pos, Mathf.SmoothStep(0, 1, camera_current_velocity));
                camera_current_velocity = Math.Min(camera_current_velocity + camera_acceleration, camera_max_look_velocity);
            }
            else
            {
                Debug.Log("ERROR: CAMERA SEES NO BOUNDARY");
            }

        }
        else
        {
            looking = false;
            snapping_back = true;
        }

    }


}
