using System;

public static class DirectorEvents
{
    public static event Action<NPC, NPCState> OnNPCCommand = (NPC, NPCState) => { };
    public static event Action<NPC, string> OnNPCUpperBodyAnimation = (NPC, Trigger) => { };
    public static event Action<DirectedRoutines> OnStartDirector = (SelectedRoutine) => { };
    
    public static void NPCCommand(NPC selectedNPC, NPCState selectedState)
    {
        OnNPCCommand.Invoke(selectedNPC, selectedState);
    }
    public static void UpperBodyAnimation(NPC selectedNPC, string selectedTrigger)
    {
        OnNPCUpperBodyAnimation.Invoke(selectedNPC, selectedTrigger);
    }
    public static void StartDirector(DirectedRoutines selectedRoutine)
    {
        OnStartDirector.Invoke(selectedRoutine);
    }
    
}