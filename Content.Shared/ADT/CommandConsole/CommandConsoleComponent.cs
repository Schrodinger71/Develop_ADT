using Robust.Shared.Utility;
using Robust.Shared.Serialization;
using Robust.Shared.GameStates;

namespace Content.Shared.ADT.CommandConsole;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class CommandConsoleComponent : Component
{
    [DataField, AutoNetworkedField]
    public string? Input;

    [DataField, AutoNetworkedField]
    public Directory RootDirectory = new() { Name = "" };

    [DataField, AutoNetworkedField]
    public string CurrentPath = "/";
}

[Serializable, NetSerializable]
public sealed class CommandConsoleState : BoundUserInterfaceState
{
    public Directory RootDirectory;
    public string CurrentPath;
    public string Input;

    public CommandConsoleState(Directory rootDirectory, string currentPath, string input)
    {
        RootDirectory = rootDirectory;
        CurrentPath = currentPath;
        Input = input;
    }
}

[Serializable, NetSerializable]
public sealed class CommandConsoleExecuteMessage : BoundUserInterfaceMessage
{
    public string Command;

    public CommandConsoleExecuteMessage(string command)
    {
        Command = command;
    }
}

[Serializable, NetSerializable]
public sealed class CommandConsoleExecuteResponseMessage : BoundUserInterfaceMessage
{
    public string Output;
    public bool Success;

    public CommandConsoleExecuteResponseMessage(string output, bool success)
    {
        Output = output;
        Success = success;
    }
}

[Serializable, NetSerializable]
public sealed class CommandConsoleSaveStateMessage : BoundUserInterfaceMessage
{
    public Directory RootDirectory;
    public string CurrentPath;
    public string Input;

    public CommandConsoleSaveStateMessage(Directory rootDirectory, string currentPath, string input)
    {
        RootDirectory = rootDirectory;
        CurrentPath = currentPath;
        Input = input;
    }
}

[Serializable, NetSerializable]
public sealed class CommandConsoleSaveStateResponseMessage : BoundUserInterfaceMessage
{
    public bool Success;

    public CommandConsoleSaveStateResponseMessage(bool success)
    {
        Success = success;
    }
}

[Serializable, NetSerializable]
public sealed class CommandConsoleLoadStateMessage : BoundUserInterfaceMessage
{
    public CommandConsoleLoadStateMessage()
    {
    }
}
