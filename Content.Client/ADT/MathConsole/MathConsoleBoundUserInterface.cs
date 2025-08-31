using Content.Shared.ADT.MathConsole;
using Robust.Client.GameObjects;
using Robust.Client.UserInterface;

namespace Content.Client.ADT.MathConsole;

public sealed class MathConsoleBoundUserInterface : BoundUserInterface
{
    private MathConsoleWindow? _window;

    public MathConsoleBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        _window = new MathConsoleWindow();
        _window.OnClose += Close;
        _window.SubmitAnswer += OnSubmitAnswer;
        _window.RequestNewEquation += OnRequestNewEquation;

        _window.OpenCentered();
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is MathConsoleState consoleState)
        {
            _window?.UpdateState(consoleState);
        }
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
        {
            _window?.Dispose();
        }
    }

    private void OnSubmitAnswer(string answer)
    {
        SendMessage(new MathConsoleSubmitAnswerMessage(answer));
    }

    private void OnRequestNewEquation()
    {
        SendMessage(new MathConsoleRequestNewEquationMessage());
    }
}
