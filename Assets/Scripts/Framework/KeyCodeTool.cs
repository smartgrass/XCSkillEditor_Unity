using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public static class KeyCodeTool
{
    public static bool IsAlpha(this KeyCode keyCode)
    {
        int keyNum = (int)keyCode;
        if (keyNum >= (int)KeyCode.Alpha0 && keyNum <= (int)KeyCode.Alpha9)
            return true;
        else
            return false;
    }
}
