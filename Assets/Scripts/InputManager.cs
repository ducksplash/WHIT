using System.Collections.Generic;
using UnityEngine;
using System.Linq;

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
        {"respawn", KeyCode.R},
        {"phone", KeyCode.P},
        {"camera", KeyCode.X},
        {"torch", KeyCode.H},
        {"special", KeyCode.Z}
    };

    void Start()
    {
        // Get all of the saved key mappings from player prefs
        string[] savedKeys = PlayerPrefs.GetString("InputKeys", "").Split(',');

        // Remove any duplicates in the savedKeys array
        savedKeys = savedKeys.Distinct().ToArray();

        // Print out the saved keys and values
        foreach (string savedKey in savedKeys)
        {
            int savedKeyCode = PlayerPrefs.GetInt(savedKey.ToLower(), (int)keys[savedKey]);
            Debug.Log("Saved key: " + savedKey.ToLower() + " - KeyCode: " + (KeyCode)savedKeyCode);
        }

        // Loop through each key and assign the saved keycode to the input manager
        foreach (string savedKey in savedKeys)
        {
            // Retrieve the saved keycode from player prefs
            KeyCode keyCode = (KeyCode)PlayerPrefs.GetInt(savedKey.ToLower(), (int)keys[savedKey]);

            // Check if the saved keycode is a valid KeyCode value
            if (!System.Enum.IsDefined(typeof(KeyCode), keyCode))
            {
                Debug.LogWarning("Saved keycode for key " + savedKey + " is not valid. Using default value instead.");
                keyCode = keys[savedKey];
            }

            // Set the keycode for the input manager
            keys[savedKey] = keyCode;

            Debug.Log(keys[savedKey]);
        }
    }


    public static bool SetKey(string key, KeyCode keyCode, CanvasGroup BadKeyMsg)
    {
        // Check if the given keyCode is already assigned to another action
        if (keys.ContainsValue(keyCode))
        {
            string existingKey = keys.FirstOrDefault(x => x.Value == keyCode).Key;
            Debug.LogWarning("KeyCode " + keyCode + " is already assigned to the " + existingKey + " action. Using default value instead.");
            keyCode = keys[key];
            
            return false;
        }

        // Log the input key name and KeyCode value
        Debug.Log("Input key: " + key + " - KeyCode: " + keyCode);

        // Save the KeyCode value to PlayerPrefs using the input key name as the key
        keys[key] = keyCode;
        PlayerPrefs.SetInt(key, (int)keyCode); // Use the original key string
        Debug.Log("Saved key: " + key + " - KeyCode: " + keyCode);

        // Update the PlayerPrefs data string
        string[] savedKeys = keys.Keys.ToArray();
        string savedKeysString = string.Join(",", savedKeys);
        PlayerPrefs.SetString("InputKeys", savedKeysString);

		BadKeyMsg.alpha = 0;
        // Print out the PlayerPrefs data for debugging purposes
        Debug.Log("PlayerPrefs data: " + PlayerPrefs.GetString("InputKeys", ""));
        return true;
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

    public static string GetKeyName(string actionName)
    {
        if (keys.TryGetValue(actionName, out KeyCode keyCode))
        {
            return keyCode.ToString();
        }
        return null;
    }

    public static void SetDefault()
    {
        Dictionary<string, KeyCode> defaultKeys = new Dictionary<string, KeyCode>()
        {
            {"jump", KeyCode.Space},
            {"up", KeyCode.W},
            {"down", KeyCode.S},
            {"left", KeyCode.A},
            {"right", KeyCode.D},
            {"crouch", KeyCode.LeftControl},
            {"sprint", KeyCode.LeftShift},
            {"respawn", KeyCode.R},
            {"phone", KeyCode.P},
            {"camera", KeyCode.X},
            {"torch", KeyCode.H},
            {"special", KeyCode.Z}
        };

        // Loop through each key and assign the default keycode to the input manager
        foreach (KeyValuePair<string, KeyCode> pair in defaultKeys)
        {
            // Set the keycode for the input manager
            keys[pair.Key] = pair.Value;
            PlayerPrefs.SetInt(pair.Key, (int)pair.Value);
        }

        PlayerPrefs.SetString("InputKeys", string.Join(",", keys.Keys));
    }
}
