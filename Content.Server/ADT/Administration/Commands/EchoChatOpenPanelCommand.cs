using Content.Server.EUI;
using Content.Shared.Administration;
using Robust.Shared.Console;
using Content.Server.Administration;
using Content.Server.ADT.Administration.UI;

namespace Content.Server.ADT.Administration.Commands
{
    [AdminCommand(AdminFlags.Fun)]
    public sealed class OpenPermissionsCommand : IConsoleCommand
    {
        public string Command => "echo_chat_openpanel";
        public string Description => "Opens the admin echo chat panel.";
        public string Help => "Usage: fun";

        public void Execute(IConsoleShell shell, string argStr, string[] args)
        {
            var player = shell.Player;
            if (player == null)
            {
                shell.WriteLine("This does not work from the server console.");
                return;
            }

            var eui = IoCManager.Resolve<EuiManager>();
            var ui = new EchoChatEui();
            eui.OpenEui(ui, player);
        }
    }
}
