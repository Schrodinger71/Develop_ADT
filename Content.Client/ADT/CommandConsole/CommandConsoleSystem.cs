using Content.Shared.ADT.CommandConsole;
using Robust.Client.GameObjects;

namespace Content.Client.ADT.CommandConsole;

public sealed partial class CommandConsoleSystem : EntitySystem
{
    [Dependency] private readonly UserInterfaceSystem _ui = default!;

    public override void Initialize()
    {
        base.Initialize();
    }
}
