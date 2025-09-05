using Content.Server.DeviceNetwork.Systems;
using Content.Server.Power.EntitySystems;
using Content.Shared.ADT.CommandConsole;
using Content.Shared.DeviceNetwork;
using Content.Shared.DeviceNetwork.Components;
using Content.Shared.DeviceNetwork.Events;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Robust.Server.GameObjects;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using System.Collections.Generic;

namespace Content.Server.ADT.CommandConsole;

public sealed partial class CommandConsoleSystem : EntitySystem
{
    [Dependency] private readonly DeviceNetworkSystem _deviceNetwork = default!;
    [Dependency] private readonly UserInterfaceSystem _ui = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CommandConsoleComponent, ComponentInit>(OnComponentInit);
        SubscribeLocalEvent<CommandConsoleComponent, BoundUIOpenedEvent>(OnBoundUiOpened);
        SubscribeLocalEvent<CommandConsoleComponent, BoundUIClosedEvent>(OnBoundUiClosed);
        SubscribeLocalEvent<CommandConsoleComponent, InteractHandEvent>(OnInteractHand);
        SubscribeLocalEvent<CommandConsoleComponent, DeviceNetworkPacketEvent>(OnPacketReceived);

        Subs.BuiEvents<CommandConsoleComponent>(CommandConsoleUiKey.Key, subs =>
        {
            subs.Event<CommandConsoleExecuteMessage>(OnExecuteMessage);
            subs.Event<CommandConsoleSaveStateMessage>(OnSaveStateMessage);
            subs.Event<CommandConsoleLoadStateMessage>(OnLoadStateMessage);
        });
    }

    private void OnComponentInit(EntityUid uid, CommandConsoleComponent component, ComponentInit args)
    {
        // Инициализируем файловую систему по умолчанию
        InitializeDefaultFileSystem(component);
    }

    private void OnBoundUiOpened(EntityUid uid, CommandConsoleComponent component, BoundUIOpenedEvent args)
    {
        // Отправляем текущее состояние файловой системы клиенту
        var state = new CommandConsoleState(
            component.RootDirectory,
            component.CurrentPath,
            component.Input ?? ""
        );

        _ui.SetUiState(uid, CommandConsoleUiKey.Key, state);
    }

    private void OnBoundUiClosed(EntityUid uid, CommandConsoleComponent component, BoundUIClosedEvent args)
    {
        // Сохраняем состояние при закрытии UI
        Dirty(uid, component);
    }

    private void OnInteractHand(EntityUid uid, CommandConsoleComponent component, InteractHandEvent args)
    {
        if (args.Handled)
            return;

        if (!TryComp<DeviceNetworkComponent>(uid, out var deviceNetwork))
            return;

        // Проверяем доступ к сети устройств
        if (deviceNetwork.DeviceLists.Count > 0)
        {
            _popup.PopupEntity("Консоль подключена к сети устройств.", uid, args.User);
        }

        args.Handled = true;
    }

    private void OnExecuteMessage(EntityUid uid, CommandConsoleComponent component, CommandConsoleExecuteMessage msg)
    {
        // Обрабатываем команду на сервере
        var result = ProcessCommandOnServer(msg.Command, component);

        // Отправляем результат обратно клиенту
        var response = new CommandConsoleExecuteResponseMessage(result, true);

        _ui.ServerSendUiMessage(uid, CommandConsoleUiKey.Key, response);
    }

    private void OnSaveStateMessage(EntityUid uid, CommandConsoleComponent component, CommandConsoleSaveStateMessage msg)
    {
        // Сохраняем состояние файловой системы
        component.RootDirectory = msg.RootDirectory;
        component.CurrentPath = msg.CurrentPath;
        component.Input = msg.Input;

        Dirty(uid, component);

        // Отправляем подтверждение
        var response = new CommandConsoleSaveStateResponseMessage(true);
        _ui.ServerSendUiMessage(uid, CommandConsoleUiKey.Key, response);
    }

    private void OnLoadStateMessage(EntityUid uid, CommandConsoleComponent component, CommandConsoleLoadStateMessage msg)
    {
        // Отправляем текущее состояние клиенту
        var state = new CommandConsoleState(
            component.RootDirectory,
            component.CurrentPath,
            component.Input ?? ""
        );

        _ui.SetUiState(uid, CommandConsoleUiKey.Key, state);
    }

    private void OnPacketReceived(EntityUid uid, CommandConsoleComponent component, DeviceNetworkPacketEvent args)
    {
        // Обрабатываем сетевые пакеты от других устройств
        if (args.Data.TryGetValue("command", out var commandData) && commandData is string command)
        {
            var result = ProcessCommandOnServer(command, component);

            // Отправляем результат обратно в сеть
            var response = new NetworkPayload
            {
                ["response"] = result,
                ["source"] = uid,
                ["timestamp"] = _timing.CurTick.Value
            };

            _deviceNetwork.QueuePacket(uid, args.SenderAddress, response, args.Frequency);
        }
    }

    private string ProcessCommandOnServer(string command, CommandConsoleComponent component)
    {
        // Создаем временный CommandManager для обработки команды
        var manager = new CommandManager();
        manager.SetState(component.RootDirectory, component.CurrentPath);

        var result = manager.Execute(command);

        // Обновляем состояние компонента
        component.RootDirectory = manager.GetRootDirectory();
        component.CurrentPath = manager.CurrentPath;

        return result;
    }

    private void InitializeDefaultFileSystem(CommandConsoleComponent component)
    {
        var root = new Directory { Name = "" };

        // Создаем стандартную структуру файловой системы
        var etc = new Directory { Name = "etc" };
        var home = new Directory { Name = "home" };
        var usr = new Directory { Name = "usr" };
        var varDir = new Directory { Name = "var" };
        var bin = new Directory { Name = "bin" };
        var lib = new Directory { Name = "lib" };
        var tmp = new Directory { Name = "tmp" };
        var dev = new Directory { Name = "dev" };
        var proc = new Directory { Name = "proc" };

        // Пользовательская директория
        var userDir = new Directory { Name = "user" };
        var local = new Directory { Name = "local" };
        usr.Add(local);

        // Файлы в /var
        var log = new Directory { Name = "log" };
        var syslog = new File { Name = "syslog.log", Content = "[INFO] System started\n[WARNING] Low disk space\n" };
        log.Add(syslog);
        var spool = new Directory { Name = "spool" };

        varDir.Add(log);
        varDir.Add(spool);

        // Файлы в /bin
        bin.Add(new File { Name = "cat.elf", Content = "Concatenate files executable placeholder" });
        bin.Add(new File { Name = "echo.elf", Content = "Echo arguments executable placeholder" });
        bin.Add(new File { Name = "mkdir.elf", Content = "Make directories executable placeholder" });

        // /lib — библиотеки
        lib.Add(new File { Name = "libm.so", Content = "Math library placeholder" });
        lib.Add(new File { Name = "libpthread.so", Content = "POSIX threads library placeholder" });

        // Файл в корне
        var motd = new File { Name = "motd.elf", Content = "Welcome to Mini Command Console OS!\nHave a nice day!" };

        root.Add(etc);
        root.Add(home);
        root.Add(usr);
        root.Add(varDir);
        root.Add(bin);
        root.Add(lib);
        root.Add(tmp);
        root.Add(dev);
        root.Add(proc);
        root.Add(motd);

        home.Add(userDir);

        component.RootDirectory = root;
        component.CurrentPath = "/";
    }
}
