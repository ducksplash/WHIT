using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR

using UnityEditor;

#endif


public class NoraManager : MonoBehaviour
{
    public OutfitName CurrentOutfit = OutfitName.Work;
    private List<OutfitName> usedOutfits = new List<OutfitName>();
    public bool IsDead;
    
    
    public void InitialiseNora()
    {
        // retrieve and apply Nora's outfit

        int stored = StoredPrefs.Instance.GetInt("NorasOutfit", 0);

        CurrentOutfit = Enum.IsDefined(typeof(OutfitName), stored) ? (OutfitName)stored : OutfitName.Work;

        if (CurrentOutfit == OutfitName.None) CurrentOutfit = OutfitName.Work;
        
        GameMaster.Instance.NorasWardrobe.SetMainOutfit(CurrentOutfit);
    }


    public void KillPlayer()
    {
        Player.Instance.CauseDeath("TESTING");
    }

    public void RespawnPlayer()
    {
        Player.Instance.Respawn();
    }


    public void InitialiseAfterDeath()
    {
        
    }


    public void SetNorasOutfit()
    {
        // Define %stage%
        // Select a random outfit from %stage%
        // ensure outfit is not used

        int DiedTimes = Player.Instance.PlayerStatus.NumberOfDeaths;
        OutfitStage currentOutfitStage;

        switch (DiedTimes)
        {
            case > 85:
                currentOutfitStage = OutfitStage.StageThree;
                break;
            case > 25:
                currentOutfitStage = OutfitStage.StageTwo;
                break;
            default:
                currentOutfitStage = OutfitStage.StageOne;
                break;
        }
        
        // Select type of outfit

        OutfitType selectedOutfitType = OutfitType.Work;

        switch (GameMaster.Instance.THISLEVEL)
        {
            case GAMELEVEL.ETVStudio:
                selectedOutfitType = OutfitType.Work;
                break;


            case GAMELEVEL.NorasFlat:
                
                DateTime now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, GameMaster.Instance.GameTimeZone);

                int hour = now.Hour;

                bool isNight;
                if (GameMaster.Instance.nightTimeStartsAt < GameMaster.Instance.nightTimeEndsAt)
                    isNight = hour >= GameMaster.Instance.nightTimeStartsAt && hour < GameMaster.Instance.nightTimeEndsAt;
                else
                    isNight = hour >= GameMaster.Instance.nightTimeStartsAt || hour < GameMaster.Instance.nightTimeEndsAt;

                if (isNight)
                {
                    selectedOutfitType = OutfitType.Pyjamas;
                }
                else
                {
                    selectedOutfitType = OutfitType.Main;
                }
                
                
                break;

            case GAMELEVEL.NorasOldFlat:
                selectedOutfitType = OutfitType.Main;
                break;

            
            
            
        }



        GameMaster.Instance.NorasWardrobe.SetRandomOutfitOfType(selectedOutfitType);
    }
    
    
    
    

    public void SaveNora()
    {
        // StoredPrefs.Instance.SetInt("NORA", (int)CurrentNora);
        // StoredPrefs.Instance.Save();
        
        // Once saved and safe, then initialise the nora
        InitialiseNora();
    }
    
    

}



#if UNITY_EDITOR


[CustomEditor(typeof(NoraManager))]
public class NoraManagerEditor : Editor
{
    private NoraManager manager;

    private void OnEnable()
    {
        manager = (NoraManager)target;
    }

    public override void OnInspectorGUI()
    {
        // Draw the default inspector first (so you still see CurrentOutfit etc.)
        DrawDefaultInspector();

        EditorGUILayout.Space(12);
        EditorGUILayout.LabelField("Testing Tools", EditorStyles.boldLabel);

        using (new EditorGUI.DisabledScope(!Application.isPlaying))
        {
            EditorGUILayout.HelpBox(
                Application.isPlaying
                    ? "Runtime testing controls"
                    : "Enter Play Mode to use these buttons",
                MessageType.Info);

            EditorGUILayout.Space(4);

            // ── Kill / Respawn ──────────────────────────────────────
            using (new EditorGUILayout.HorizontalScope())
            {
                GUI.backgroundColor = new Color(0.9f, 0.3f, 0.3f);
                if (GUILayout.Button("Kill Player", GUILayout.Height(32)))
                {
                    manager.KillPlayer();
                }

                GUI.backgroundColor = new Color(0.3f, 0.75f, 0.4f);
                if (GUILayout.Button("Respawn Player", GUILayout.Height(32)))
                {
                    manager.RespawnPlayer();
                }

                GUI.backgroundColor = Color.white;
            }

            // ── Future buttons can go here ──────────────────────────
            // Example:
            // EditorGUILayout.Space(8);
            // if (GUILayout.Button("Set Random Outfit", GUILayout.Height(28)))
            // {
            //     manager.SetNorasOutfit();
            // }
        }
    }
}
#endif