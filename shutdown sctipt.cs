bool isShutdownOn = false;
int shutdownStep = 0;
int tickCounter = 0;
IMyBroadcastListener broadcastListener;

public void Main(string argument, UpdateType updateSource)
{
    if (broadcastListener == null)
    {
        broadcastListener = IGC.RegisterBroadcastListener("RHMC Coast Guard Capital");
    }

    if (argument.ToLower() == "shutdown")
    {
        isShutdownOn = true;
        shutdownStep = 1;
        tickCounter = 0;
        Runtime.UpdateFrequency = UpdateFrequency.Update10; // Update every 10 ticks (0.166 seconds)
        UpdateLCDs("Initiating shutdown sequence...");
    }
    else if (argument.ToLower() == "authcode-06229-stop")
    {
        StopShutdown();
    }

    if (isShutdownOn)
    {
        PerformShutdown();
    }

    // Handle received messages
    while (broadcastListener.HasPendingMessage)
    {
        MyIGCMessage message = broadcastListener.AcceptMessage();
        if (message.Tag == "RHMC Coast Guard Capital")
        {
            string content = message.Data.ToString();
            Echo("Received message: " + content);
        }
    }
}

void PerformShutdown()
{
    tickCounter++;

    // Each step has a delay of 3 seconds (30 ticks with Update10)
    if (tickCounter < 30)
    {
        return;
    }

    tickCounter = 0; // Reset counter for next step

    switch (shutdownStep)
    {
        case 1:
            ShutdownLights();
            break;
        case 2:
            ShutdownDoors();
            break;
        case 3:
            ShutdownIONThrusters();
            break;
        case 4:
            StopBuilding();
            break;
        case 5:
            StopAntenna();
            break;
        case 6:
            ShutdownPlasmaBlocks();
            break;
        case 7:
            ShutdownWeapons();
            break;
        case 8:
            ShutdownBatteries();
            break;
        case 9:
            ShutdownLCDs();
            break;
        case 10:
            SendShutdownMessage();
            isShutdownOn = false; // Shutdown complete
            Runtime.UpdateFrequency = UpdateFrequency.None; // Stop updates
            break;
    }

    shutdownStep++;
}

void ShutdownLights()
{
    var lights = new List<IMyLightingBlock>();
    GridTerminalSystem.GetBlocksOfType(lights);
    foreach (var light in lights)
    {
        light.ApplyAction("OnOff_Off");
    }
    UpdateLCDs("Lights shutdown.");
}

void ShutdownDoors()
{
    var doors = new List<IMyDoor>();
    GridTerminalSystem.GetBlocksOfType(doors);
    foreach (var door in doors)
    {
        door.CloseDoor();
        door.ApplyAction("OnOff_Off");
    }
    UpdateLCDs("Doors shutdown.");
}

void ShutdownPlasmaBlocks()
{
    var blocks = new List<IMyTerminalBlock>();
    GridTerminalSystem.GetBlocks(blocks);
    foreach (var block in blocks)
    {
        if (block.CustomData.Contains("plasma"))
        {
            block.ApplyAction("OnOff_Off");
        }
    }
    UpdateLCDs("Plasma blocks shutdown.");
}

void ShutdownWeapons()
{
    var weapons = new List<IMyUserControllableGun>();
    GridTerminalSystem.GetBlocksOfType(weapons);
    foreach (var weapon in weapons)
    {
        weapon.ApplyAction("OnOff_Off");
    }
    UpdateLCDs("Weapons shutdown.");
}

void ShutdownBatteries()
{
    var batteries = new List<IMyBatteryBlock>();
    GridTerminalSystem.GetBlocksOfType(batteries);
    foreach (var battery in batteries)
    {
        if (string.IsNullOrWhiteSpace(battery.CustomData))
        {
            battery.ApplyAction("OnOff_Off");
        }
        else if (battery.CustomData.Contains("Back Up"))
        {
            battery.ApplyAction("OnOff_On");
        }
    }
    UpdateLCDs("Batteries shutdown.");
}

void ShutdownLCDs()
{
    var lcds = new List<IMyTextPanel>();
    GridTerminalSystem.GetBlocksOfType(lcds);
    foreach (var lcd in lcds)
    {
        if (!lcd.CustomData.Contains("Main LCD"))
        {
            lcd.ApplyAction("OnOff_Off");
        }
    }
    UpdateLCDs("Ship is now Shutdown, please wait for the captain to re-auth for ship use.");
}

void UpdateLCDs(string message)
{
    var lcds = new List<IMyTextPanel>();
    GridTerminalSystem.GetBlocksOfType(lcds);
    foreach (var lcd in lcds)
    {
        if (lcd.CustomData.Contains("Main LCD"))
        {
            lcd.WriteText(message);
        }
    }
    Echo(message); // Also output to the programmable block terminal
}

void ShutdownIONThrusters()
{
    var ionThrusters = new List<IMyThrust>();
    GridTerminalSystem.GetBlocksOfType<IMyThrust>(ionThrusters, block => block.BlockDefinition.SubtypeName.Contains("Ion"));
    
    foreach (var thruster in ionThrusters)
    {
        thruster.Enabled = false;
    }
    UpdateLCDs("ION Thrusters shutdown.");
}

void SendShutdownMessage()
{
    IGC.SendBroadcastMessage("RHMC Coast Guard Capital", "RHMC Coast Guard Capital ship is now shutdown. Please do not send any message to this ship.", TransmissionDistance.TransmissionDistanceMax);

    // Optionally, echo the message for debugging purposes
    Echo("Shutdown message sent to faction members.");
}

void StopBuilding()
{
    var assemblers = new List<IMyAssembler>();
    GridTerminalSystem.GetBlocksOfType(assemblers);
    foreach (var assembler in assemblers)
    {
        assembler.ApplyAction("OnOff_Off");
    }
    var refineries = new List<IMyRefinery>();
    GridTerminalSystem.GetBlocksOfType(refineries);
    foreach (var refinery in refineries)
    {
        refinery.ApplyAction("OnOff_Off");
    }
    UpdateLCDs("Assemblers and Refineries stopping.");
}

void StopAntenna()
{
    var antennas = new List<IMyRadioAntenna>();
    GridTerminalSystem.GetBlocksOfType(antennas);
    foreach (var antenna in antennas)
    {
        antenna.ApplyAction("OnOff_Off");
    }
}

void StopShutdown()
{
    isShutdownOn = false;
    shutdownStep = 0;
    tickCounter = 0;
    Runtime.UpdateFrequency = UpdateFrequency.None;

    // Turn everything back on
    var lights = new List<IMyLightingBlock>();
    GridTerminalSystem.GetBlocksOfType(lights);
    foreach (var light in lights)
    {
        light.ApplyAction("OnOff_On");
    }

    var doors = new List<IMyDoor>();
    GridTerminalSystem.GetBlocksOfType(doors);
    foreach (var door in doors)
    {
        door.ApplyAction("OnOff_On");
    }

    var ionThrusters = new List<IMyThrust>();
    GridTerminalSystem.GetBlocksOfType<IMyThrust>(ionThrusters, block => block.BlockDefinition.SubtypeName.Contains("Ion"));
    foreach (var thruster in ionThrusters)
    {
        thruster.Enabled = true;
    }

    var assemblers = new List<IMyAssembler>();
    GridTerminalSystem.GetBlocksOfType(assemblers);
    foreach (var assembler in assemblers)
    {
        assembler.ApplyAction("OnOff_On");
    }

    var refineries = new List<IMyRefinery>();
    GridTerminalSystem.GetBlocksOfType(refineries);
    foreach (var refinery in refineries)
    {
        refinery.ApplyAction("OnOff_On");
    }

    var antennas = new List<IMyRadioAntenna>();
    GridTerminalSystem.GetBlocksOfType(antennas);
    foreach (var antenna in antennas)
    {
        antenna.ApplyAction("OnOff_On");
    }

    var blocks = new List<IMyTerminalBlock>();
    GridTerminalSystem.GetBlocks(blocks);
    foreach (var block in blocks)
    {
        if (block.CustomData.Contains("plasma"))
        {
            block.ApplyAction("OnOff_On");
        }
    }

    var weapons = new List<IMyUserControllableGun>();
    GridTerminalSystem.GetBlocksOfType(weapons);
    foreach (var weapon in weapons)
    {
        weapon.ApplyAction("OnOff_On");
    }

    var batteries = new List<IMyBatteryBlock>();
    GridTerminalSystem.GetBlocksOfType(batteries);
    foreach (var battery in batteries)
    {
        battery.ApplyAction("OnOff_On");
    }

    var lcds = new List<IMyTextPanel>();
    GridTerminalSystem.GetBlocksOfType(lcds);
    foreach (var lcd in lcds)
    {
        lcd.ApplyAction("OnOff_On");
    }

    UpdateLCDs("Shutdown stopped. Systems reactivated.");
}