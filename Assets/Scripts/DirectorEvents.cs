using System;

public static class DirectorEvents
{
    public static event Action<NPC, NPCState> OnNPCCommand = (NPC, NPCState) => { };
    
    public static void NPCCommand(NPC selectedNPC, NPCState selectedState)
    {
        OnNPCCommand.Invoke(selectedNPC, selectedState);
    }
    
}