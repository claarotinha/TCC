using UnityEngine;

public static class PauseHelper
{
    public static bool BlockInput()
    {
        return UniversalPauseManager.IsPaused;
    }
}