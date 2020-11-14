using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerModel
{
    public int max_num_jumps = 2; // how many jumps does the player get?
    public int max_num_dashes = 1; // how many dashes does the player get in the air?
    public float max_health = 10;
    public float current_health = 10;

    public PlayerModel(int max_jumps, int max_dashes, float max_health)
    {
        max_num_jumps = max_jumps;
        max_num_dashes = max_dashes;
        this.max_health = max_health;
        this.current_health = max_health;
    }

}
