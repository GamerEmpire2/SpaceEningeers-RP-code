bool isLockdownActive = false;

void Main(string argument, UpdateType updateSource)
{
    if (argument.ToLower() == "lockdown")
    {
        Lockdown();
    }
    else if (argument.ToLower() == "unlock")
    {
        Unlock();
    }
    else if (isLockdownActive && (updateSource & (UpdateType.Update10 | UpdateType.Update100)) != 0)
    {
        MaintainLockdown();
    }
}

void Lockdown()
{
    isLockdownActive = true;

    // Close all doors
    List<IMyDoor> doors = new List<IMyDoor>();
    GridTerminalSystem.GetBlocksOfType(doors);
    foreach (IMyDoor door in doors)
    {
        door.CloseDoor();
        door.Enabled = true;
    }

    // Turn all lights to red
    List<IMyLightingBlock> lights = new List<IMyLightingBlock>();
    GridTerminalSystem.GetBlocksOfType(lights);
    foreach (IMyLightingBlock light in lights)
    {
        light.Enabled = true;
        light.Color = Color.Red;
    }

    // Disable certain blocks like assemblers and refineries
    List<IMyAssembler> assemblers = new List<IMyAssembler>();
    GridTerminalSystem.GetBlocksOfType(assemblers);
    foreach (IMyAssembler assembler in assemblers)
    {
        assembler.Enabled = false;
    }

    List<IMyRefinery> refineries = new List<IMyRefinery>();
    GridTerminalSystem.GetBlocksOfType(refineries);
    foreach (IMyRefinery refinery in refineries)
    {
        refinery.Enabled = false;
    }

    // Enable all turrets
    List<IMyLargeTurretBase> turrets = new List<IMyLargeTurretBase>();
    GridTerminalSystem.GetBlocksOfType(turrets);
    foreach (IMyLargeTurretBase turret in turrets)
    {
        turret.Enabled = true;
    }

    // Display Lockdown status on LCD panels
    List<IMyTextPanel> lcds = new List<IMyTextPanel>();
    GridTerminalSystem.GetBlocksOfType(lcds);
    foreach (IMyTextPanel lcd in lcds)
    {
        lcd.WriteText("Lockdown Active");
        lcd.BackgroundColor = Color.Black;
        lcd.FontColor = Color.Red;
    }

    // Play lockdown sound on sound blocks
    List<IMySoundBlock> soundBlocks = new List<IMySoundBlock>();
    GridTerminalSystem.GetBlocksOfType(soundBlocks);
    foreach (IMySoundBlock soundBlock in soundBlocks)
    {
        soundBlock.Play();
    }

    // Start timer block to keep the lockdown active
    List<IMyTimerBlock> timers = new List<IMyTimerBlock>();
    GridTerminalSystem.GetBlocksOfType(timers, t => t.CustomName.Contains("Lockdown Timer"));
    foreach (IMyTimerBlock timer in timers)
    {
        timer.ApplyAction("OnOff_On"); // Correct the action name to "OnOff_On" and add a semicolon
        timer.StartCountdown();
        timer.Trigger();
    }
}

void MaintainLockdown()
{
    // Close all doors again to ensure they stay closed
    List<IMyDoor> doors = new List<IMyDoor>();
    GridTerminalSystem.GetBlocksOfType(doors);
    foreach (IMyDoor door in doors)
    {
        door.CloseDoor();
        door.Enabled = true;
    }
}

void Unlock()
{
    isLockdownActive = false;

    // Open all doors that are not airlock doors
    List<IMyDoor> doors = new List<IMyDoor>();
    GridTerminalSystem.GetBlocksOfType(doors);
    foreach (IMyDoor door in doors)
    {
        if (!door.CustomName.ToLower().Contains("airlock"))
        {
            door.OpenDoor();
        }
    }

    // Reset all lights to white
    List<IMyLightingBlock> lights = new List<IMyLightingBlock>();
    GridTerminalSystem.GetBlocksOfType(lights);
    foreach (IMyLightingBlock light in lights)
    {
        light.Enabled = true;
        light.Color = Color.White;
    }

    // Enable certain blocks like assemblers and refineries
    List<IMyAssembler> assemblers = new List<IMyAssembler>();
    GridTerminalSystem.GetBlocksOfType(assemblers);
    foreach (IMyAssembler assembler in assemblers)
    {
        assembler.Enabled = true;
    }

    List<IMyRefinery> refineries = new List<IMyRefinery>();
    GridTerminalSystem.GetBlocksOfType(refineries);
    foreach (IMyRefinery refinery in refineries)
    {
        refinery.Enabled = true;
    }

    // Disable all turrets
    List<IMyLargeTurretBase> turrets = new List<IMyLargeTurretBase>();
    GridTerminalSystem.GetBlocksOfType(turrets);
    foreach (IMyLargeTurretBase turret in turrets)
    {
        turret.Enabled = false;
    }

    // Display Unlock status on LCD panels
    List<IMyTextPanel> lcds = new List<IMyTextPanel>();
    GridTerminalSystem.GetBlocksOfType(lcds);
    foreach (IMyTextPanel lcd in lcds)
    {
        lcd.WriteText("Lockdown Inactive");
        lcd.BackgroundColor = Color.Black;
        lcd.FontColor = Color.Green;
    }

    // Stop playing sound on sound blocks
    List<IMySoundBlock> soundBlocks = new List<IMySoundBlock>();
    GridTerminalSystem.GetBlocksOfType(soundBlocks);
    foreach (IMySoundBlock soundBlock in soundBlocks)
    {
        soundBlock.Stop();
    }

    // Stop and turn off timer block
    List<IMyTimerBlock> timers = new List<IMyTimerBlock>();
    GridTerminalSystem.GetBlocksOfType(timers, t => t.CustomName.Contains("Lockdown Timer"));
    foreach (IMyTimerBlock timer in timers)
    {
        timer.StopCountdown();
        timer.ApplyAction("OnOff_Off");
    }
}

// Ensure the script runs periodically to maintain lockdown
public Program()
{
    Runtime.UpdateFrequency = UpdateFrequency.Update10;
}
