using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared.ADT.MathConsole;

/// <summary>
/// Component для математической консоли РНД
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MathConsoleComponent : Component
{
    [AutoNetworkedField, ViewVariables(VVAccess.ReadOnly)]
    public string? LastOpenedBy;

    [DataField, AutoNetworkedField]
    public List<MathConsoleRecord> Records = new();

    [DataField("soundCorrect")]
    public SoundSpecifier? SoundCorrect;

    [DataField("soundIncorrect")]
    public SoundSpecifier? SoundIncorrect;

    [DataField("soundComplete")]
    public SoundSpecifier? SoundComplete;

    /// <summary>
    /// Минимальное количество очков за правильный ответ
    /// </summary>
    [DataField("minPointsPerAnswer")]
    public int MinPointsPerAnswer = 5;

    /// <summary>
    /// Максимальное количество очков за правильный ответ
    /// </summary>
    [DataField("maxPointsPerAnswer")]
    public int MaxPointsPerAnswer = 15;

    /// <summary>
    /// Бонус за сложность уравнения
    /// </summary>
    [DataField("difficultyBonus")]
    public int DifficultyBonus = 3;

    /// <summary>
    /// Текущее уравнение
    /// </summary>
    [AutoNetworkedField]
    public string CurrentEquation = string.Empty;

    /// <summary>
    /// Правильный ответ на текущее уравнение
    /// </summary>
    [AutoNetworkedField]
    public string CurrentAnswer = string.Empty;
}

[Serializable, NetSerializable]
public sealed class MathConsoleRecord
{
    public string Equation = string.Empty;
    public string Answer = string.Empty;
    public string EntityName = string.Empty;
    public DateTime SolvedAt = DateTime.Now;
    public int PointsEarned = 0;

    public override string ToString()
    {
        return Loc.GetString("math-console-record-format",
            ("name", EntityName),
            ("equation", Equation),
            ("answer", Answer),
            ("points", PointsEarned.ToString()),
            ("time", SolvedAt.ToString("HH:mm:ss")));
    }
}

[Serializable, NetSerializable]
public sealed class MathConsoleSubmitAnswerMessage : BoundUserInterfaceMessage
{
    public string Answer { get; }

    public MathConsoleSubmitAnswerMessage(string answer)
    {
        Answer = answer;
    }
}

[Serializable, NetSerializable]
public sealed class MathConsoleRequestNewEquationMessage : BoundUserInterfaceMessage
{
    public MathConsoleRequestNewEquationMessage()
    {
    }
}

[Serializable, NetSerializable]
public sealed class MathConsoleCorrectAnswerMessage : BoundUserInterfaceMessage
{
    public string Equation { get; }
    public string Answer { get; }
    public int PointsEarned { get; }

    public MathConsoleCorrectAnswerMessage(string equation, string answer, int pointsEarned)
    {
        Equation = equation;
        Answer = answer;
        PointsEarned = pointsEarned;
    }
}

[Serializable, NetSerializable]
public sealed class MathConsoleIncorrectAnswerMessage : BoundUserInterfaceMessage
{
    public string CorrectAnswer { get; }

    public MathConsoleIncorrectAnswerMessage(string correctAnswer)
    {
        CorrectAnswer = correctAnswer;
    }
}

[Serializable, NetSerializable]
public sealed class MathConsoleState : BoundUserInterfaceState
{
    public string CurrentEquation { get; }
    public string CurrentAnswer { get; }
    public List<MathConsoleRecord> Records { get; }
    public int TotalPointsEarned { get; }

    public MathConsoleState(string currentEquation, string currentAnswer, List<MathConsoleRecord> records, int totalPointsEarned)
    {
        CurrentEquation = currentEquation;
        CurrentAnswer = currentAnswer;
        Records = records;
        TotalPointsEarned = totalPointsEarned;
    }
}
