using System;

public static class Constants
{
    ////////////////////////////////////////////////////////////////////////////
    ///////////////////////// PLAYER-RELEVANT CONSTANTS ////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    // PLAYER MOVEMENT
    public const float PLAYER_H_ACCEL = 2f;
    public const float PLAYER_H_AIR_DECEL = 1.25f;
    public const float PLAYER_H_GROUND_DECEL = 2f;
    public const float PLAYER_H_MAX_DECEL = 3f;
    public const float PLAYER_H_MAX_VELOCITY = 15f;
    public const float PLAYER_V_MAX_VELOCITY = -40f;
    public const float PLAYER_JUMP_V_VELOCITY = 12.5f;
    public const float PLAYER_WALL_JUMP_H_VELOCITY = 30f;
    public const float PLAYER_WALL_JUMP_V_VELOCITY = 10f;
    public const float PLAYER_DOUBLE_JUMP_V_VELOCITY = 10f;
    public const float PLAYER_FALL_MULTIPLIER = 3f;
    public const float PLAYER_LOW_JUMP_MULTIPLIER = 1.75f;
    public const float PLAYER_DASH_SPEED = 40f;
    public const float PLAYER_WALL_SLIDE_SPEED = 10f;
    public const float PLAYER_JUMP_LEEWAY_TIME = 0.075f;
    public const float PLAYER_SCALE = 3f;

    // PLATFORM SAFETY CHECKS
    public const float MIN_SAFE_DISTANCE_FROM_LEDGE = 3f;
    public const float MIN_SAFE_X_DISTANCE_FROM_HAZARD = 20f;
    public const float MIN_SAFE_Y_DISTANCE_FROM_HAZARD = 2f;

    // PLAYER'S THROWN DISK
    public const float DISK_SCALE = 0.5f;
    public const float MAX_DISK_DISTANCE = 10f;
    public const float DISK_VELOCITY = 2f;
    public const float DISK_BOUNCE_VELOCITY = 22f;
    public const float DISK_RECALL_DISTANCE_FROM_PLAYER = 0.25f;
    public const float DISK_RECALL_LERP_VALUE = 0.15f;

}
