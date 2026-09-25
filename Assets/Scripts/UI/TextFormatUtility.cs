using System;
using UnityEngine;

public static class TextFormatUtility
{
    public static bool IsValid(string format, params object[] sampleArguments)
    {
        try
        {
            string.Format(format, sampleArguments);
            return true;
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
