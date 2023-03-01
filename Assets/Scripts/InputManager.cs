using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static Dictionary<string, KeyCode> keys = new Dictionary<string, KeyCode>()
    {
        {"jump", KeyCode.Space},
        {"up", KeyCode.W},
        {"down", KeyCode.S},
        {"left", KeyCode.A},
        {"right", KeyCode.D},
        {"crouch", KeyCode.LeftControl},
        {"sprint", KeyCode.LeftShift},
        {"respawn", KeyCode.R}
    };

    void Start()
    {
        // Get all of the saved key mappings from player prefs
        string[] savedKeys = PlayerPrefs.GetString("InputKeys", "").Split(',');

        // Loop through each key and assign the saved keycode to the input manager
        foreach (string savedKey in savedKeys)
        {
            // Retrieve the saved keycode from player prefs
            KeyCode keyCode = (KeyCode)PlayerPrefs.GetInt(savedKey, (int)keys[savedKey]);

            // Set the keycode for the input manager
            keys[savedKey] = keyCode;
        }
    }

    public static void SetKey(string key, KeyCode keyCode)
    {
        keys[key] = keyCode;
        PlayerPrefs.SetInt(key, (int)keyCode);
    }

    public static bool GetKey(string key)
    {
        if (keys.ContainsKey(key))
        {
            return Input.GetKey(keys[key]);
        }
        else
        {
            Debug.LogError("Key " + key + " not found in InputManager!");
            return false;
        }
    }

    public static bool GetKeyUp(string key)
    {
        if (keys.ContainsKey(key))
        {
            return Input.GetKeyUp(keys[key]);
        }
        else
        {
            Debug.LogError("Key " + key + " not found in InputManager!");
            return false;
        }
    }

    public static bool GetKeyDown(string key)
    {
        if (keys.ContainsKey(key))
        {
            return Input.GetKeyDown(keys[key]);
        }
        else
        {
            Debug.LogError("Key " + key + " not found in InputManager!");
            return false;
        }
    }
}
