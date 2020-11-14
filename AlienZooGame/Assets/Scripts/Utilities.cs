using System;
public static class Utilities
{
    public static int FloatToIntCode(float f)
    {
        if (f < 0) return -1;
        else if (f > 0) return 1;
        else return 0;
    }

}
