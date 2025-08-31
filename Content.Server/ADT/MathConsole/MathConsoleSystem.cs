using Content.Server.Research.Systems;
using Content.Shared.ADT.MathConsole;
using Content.Shared.Research.Components;
using Robust.Server.GameObjects;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Random;
using Robust.Shared.Maths;
using System.Linq;

namespace Content.Server.ADT.MathConsole;

public sealed partial class MathConsoleSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _sharedAudioSystem = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly ResearchSystem _researchSystem = default!;
    [Dependency] private readonly UserInterfaceSystem _uiSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<MathConsoleComponent, ComponentInit>(OnComponentInit);
        SubscribeLocalEvent<MathConsoleComponent, BoundUIOpenedEvent>(OnBoundUiOpened);
        SubscribeLocalEvent<MathConsoleComponent, BoundUIClosedEvent>(OnBoundUiClosed);
        Subs.BuiEvents<MathConsoleComponent>(MathConsoleUiKey.Key, subs =>
        {
            subs.Event<MathConsoleSubmitAnswerMessage>(OnSubmitAnswer);
            subs.Event<MathConsoleRequestNewEquationMessage>(OnRequestNewEquation);
        });
    }

    private void OnComponentInit(EntityUid uid, MathConsoleComponent component, ComponentInit args)
    {
        // Генерируем первое уравнение при инициализации
        GenerateNewEquation(uid, component);
    }

    private void OnBoundUiOpened(EntityUid uid, MathConsoleComponent component, BoundUIOpenedEvent args)
    {
        var actor = args.Actor;
        if (_entityManager.TryGetComponent<MetaDataComponent>(actor, out var meta))
        {
            component.LastOpenedBy = meta.EntityName;
            Dirty(uid, component);
        }

        // Обновляем UI при открытии
        UpdateConsoleInterface(uid, component);
    }

    private void OnBoundUiClosed(EntityUid uid, MathConsoleComponent component, BoundUIClosedEvent args)
    {
        component.LastOpenedBy = null;
        Dirty(uid, component);
    }

    private void OnSubmitAnswer(EntityUid uid, MathConsoleComponent component, MathConsoleSubmitAnswerMessage msg)
    {
        if (string.IsNullOrEmpty(component.CurrentEquation) || string.IsNullOrEmpty(component.CurrentAnswer))
        {
            GenerateNewEquation(uid, component);
            return;
        }

        var isCorrect = CheckAnswer(msg.Answer, component.CurrentAnswer);

        if (isCorrect)
        {
            HandleCorrectAnswer(uid, component, msg.Answer);
        }
        else
        {
            HandleIncorrectAnswer(uid, component);
        }
    }

    private void OnRequestNewEquation(EntityUid uid, MathConsoleComponent component, MathConsoleRequestNewEquationMessage msg)
    {
        GenerateNewEquation(uid, component);
        UpdateConsoleInterface(uid, component);
    }

    private void GenerateNewEquation(EntityUid uid, MathConsoleComponent component)
    {
        var (equation, answer) = GenerateRandomEquation();
        component.CurrentEquation = equation;
        component.CurrentAnswer = answer;
        Dirty(uid, component);
    }

    private (string equation, string answer) GenerateRandomEquation()
    {
        var equationType = _random.Next(4); // 0: линейное, 1: квадратное, 2: кубическое, 3: геометрическое

        return equationType switch
        {
            0 => GenerateLinearEquation(),
            1 => GenerateQuadraticEquation(),
            2 => GenerateCubicEquation(),
            3 => GenerateGeometricEquation(),
            _ => GenerateLinearEquation()
        };
    }

    private (string equation, string answer) GenerateLinearEquation()
    {
        var a = _random.Next(1, 11);
        var b = _random.Next(1, 21);
        var x = _random.Next(1, 11);
        var c = a * x + b;

        var equation = $"{a}x + {b} = {c}";
        var answer = x.ToString();

        return (equation, answer);
    }

    private (string equation, string answer) GenerateQuadraticEquation()
    {
        var a = _random.Next(1, 6);
        var b = _random.Next(-10, 11);
        var c = _random.Next(-20, 21);

        // Простое квадратное уравнение вида ax² + bx + c = 0
        // Для простоты берем x = 1 или x = -1
        var x = _random.Next(2) == 0 ? 1 : -1;
        var result = a * x * x + b * x + c;

        var equation = $"{a}x² + {b}x + {c} = {result}";
        var answer = x.ToString();

        return (equation, answer);
    }

    private (string equation, string answer) GenerateCubicEquation()
    {
        var a = _random.Next(1, 4);
        var b = _random.Next(-5, 6);
        var c = _random.Next(-10, 11);
        var d = _random.Next(-15, 16);

        // Простое кубическое уравнение вида ax³ + bx² + cx + d = 0
        // Для простоты берем x = 1
        var x = 1;
        var result = a * x * x * x + b * x * x + c * x + d;

        var equation = $"{a}x³ + {b}x² + {c}x + {d} = {result}";
        var answer = x.ToString();

        return (equation, answer);
    }

    private (string equation, string answer) GenerateGeometricEquation()
    {
        var shape = _random.Next(3); // 0: круг, 1: прямоугольник, 2: треугольник

        return shape switch
        {
            0 => GenerateCircleEquation(),
            1 => GenerateRectangleEquation(),
            2 => GenerateTriangleEquation(),
            _ => GenerateCircleEquation()
        };
    }

    private (string equation, string answer) GenerateCircleEquation()
    {
        var radius = _random.Next(2, 11);
        var area = Math.PI * radius * radius;

        var equation = $"Площадь круга = πr² = {area:F2}, r = ?";
        var answer = radius.ToString();

        return (equation, answer);
    }

    private (string equation, string answer) GenerateRectangleEquation()
    {
        var width = _random.Next(2, 11);
        var height = _random.Next(2, 11);
        var area = width * height;

        var equation = $"Площадь прямоугольника = {width} × h = {area}, h = ?";
        var answer = height.ToString();

        return (equation, answer);
    }

    private (string equation, string answer) GenerateTriangleEquation()
    {
        var baseLength = _random.Next(2, 11);
        var height = _random.Next(2, 11);
        var area = 0.5 * baseLength * height;

        var equation = $"Площадь треугольника = 0.5 × {baseLength} × h = {area}, h = ?";
        var answer = height.ToString();

        return (equation, answer);
    }

    private bool CheckAnswer(string userAnswer, string correctAnswer)
    {
        // Простая проверка ответа
        return userAnswer.Trim().Equals(correctAnswer.Trim(), StringComparison.OrdinalIgnoreCase);
    }

    private void HandleCorrectAnswer(EntityUid uid, MathConsoleComponent component, string userAnswer)
    {
        // Вычисляем очки за ответ
        var basePoints = _random.Next(component.MinPointsPerAnswer, component.MaxPointsPerAnswer + 1);
        var totalPoints = basePoints + component.DifficultyBonus;

        // Добавляем запись
        var record = new MathConsoleRecord
        {
            Equation = component.CurrentEquation,
            Answer = userAnswer,
            EntityName = component.LastOpenedBy ?? "Unknown",
            SolvedAt = DateTime.Now,
            PointsEarned = totalPoints
        };

        component.Records.Add(record);

        // TODO: Добавить очки исследования на сервер РНД
        // _researchSystem.ModifyServerPoints(serverUid, totalPoints);

        // Проигрываем звук
        if (component.SoundCorrect != null)
            _sharedAudioSystem.PlayPvs(component.SoundCorrect, uid);

        // Генерируем новое уравнение
        GenerateNewEquation(uid, component);

        // Обновляем UI
        UpdateConsoleInterface(uid, component);

        // Отправляем сообщение о правильном ответе
        _uiSystem.ServerSendUiMessage(uid, MathConsoleUiKey.Key,
            new MathConsoleCorrectAnswerMessage(component.CurrentEquation, userAnswer, totalPoints));
    }

    private void HandleIncorrectAnswer(EntityUid uid, MathConsoleComponent component)
    {
        // Проигрываем звук
        if (component.SoundIncorrect != null)
            _sharedAudioSystem.PlayPvs(component.SoundIncorrect, uid);

        // Отправляем сообщение о неправильном ответе
        _uiSystem.ServerSendUiMessage(uid, MathConsoleUiKey.Key,
            new MathConsoleIncorrectAnswerMessage(component.CurrentAnswer));
    }

    private void UpdateConsoleInterface(EntityUid uid, MathConsoleComponent component)
    {
        var totalPoints = component.Records.Sum(r => r.PointsEarned);
        var state = new MathConsoleState(
            component.CurrentEquation,
            component.CurrentAnswer,
            component.Records,
            totalPoints
        );

        _uiSystem.SetUiState(uid, MathConsoleUiKey.Key, state);
    }
}
