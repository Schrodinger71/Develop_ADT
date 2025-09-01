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
        var equationType = _random.Next(9); // 0: линейное, 1: квадратное, 2: кубическое, 3: геометрическое, 4: интеграл, 5: дифференциальное, 6: координатная геометрия, 7: графики функций, 8: точки пересечения

        return equationType switch
        {
            0 => GenerateLinearEquation(),
            1 => GenerateQuadraticEquation(),
            2 => GenerateCubicEquation(),
            3 => GenerateGeometricEquation(),
            4 => GenerateIntegralEquation(),
            5 => GenerateDifferentialEquation(),
            6 => GenerateCoordinateGeometry(),
            7 => GenerateFunctionGraph(),
            8 => GenerateIntersectionPoint(),
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

    private (string equation, string answer) GenerateIntegralEquation()
    {
        var integralType = _random.Next(4); // 0: степенная функция, 1: тригонометрическая, 2: экспоненциальная, 3: рациональная

        return integralType switch
        {
            0 => GeneratePowerIntegral(),
            1 => GenerateTrigIntegral(),
            2 => GenerateExpIntegral(),
            3 => GenerateRationalIntegral(),
            _ => GeneratePowerIntegral()
        };
    }

    private (string equation, string answer) GeneratePowerIntegral()
    {
        var power = _random.Next(2, 6);
        var coefficient = _random.Next(1, 5);
        var lowerBound = _random.Next(0, 3);
        var upperBound = _random.Next(lowerBound + 1, lowerBound + 4);

        // ∫(coefficient * x^power)dx от lowerBound до upperBound
        var result = coefficient * (Math.Pow(upperBound, power + 1) - Math.Pow(lowerBound, power + 1)) / (power + 1);

        var equation = $"∫{coefficient}x^{power}dx от {lowerBound} до {upperBound} = ?";
        var answer = result.ToString("F2");

        return (equation, answer);
    }

    private (string equation, string answer) GenerateTrigIntegral()
    {
        var trigType = _random.Next(2); // 0: sin, 1: cos
        var coefficient = _random.Next(1, 4);
        var lowerBound = 0;
        var upperBound = _random.Next(1, 4);

        if (trigType == 0)
        {
            // ∫sin(x)dx = -cos(x)
            var result = coefficient * (-Math.Cos(upperBound) + Math.Cos(lowerBound));
            var equation = $"∫{coefficient}sin(x)dx от {lowerBound} до {upperBound} = ?";
            var answer = result.ToString("F2");
            return (equation, answer);
        }
        else
        {
            // ∫cos(x)dx = sin(x)
            var result = coefficient * (Math.Sin(upperBound) - Math.Sin(lowerBound));
            var equation = $"∫{coefficient}cos(x)dx от {lowerBound} до {upperBound} = ?";
            var answer = result.ToString("F2");
            return (equation, answer);
        }
    }

    private (string equation, string answer) GenerateExpIntegral()
    {
        var coefficient = _random.Next(1, 4);
        var lowerBound = 0;
        var upperBound = _random.Next(1, 4);

        // ∫e^x dx = e^x
        var result = coefficient * (Math.Exp(upperBound) - Math.Exp(lowerBound));

        var equation = $"∫{coefficient}e^x dx от {lowerBound} до {upperBound} = ?";
        var answer = result.ToString("F2");

        return (equation, answer);
    }

    private (string equation, string answer) GenerateRationalIntegral()
    {
        var coefficient = _random.Next(1, 4);
        var lowerBound = _random.Next(1, 4);
        var upperBound = _random.Next(lowerBound + 1, lowerBound + 4);

        // ∫(1/x)dx = ln(x)
        var result = coefficient * (Math.Log(upperBound) - Math.Log(lowerBound));

        var equation = $"∫{coefficient}/x dx от {lowerBound} до {upperBound} = ?";
        var answer = result.ToString("F2");

        return (equation, answer);
    }

    private (string equation, string answer) GenerateDifferentialEquation()
    {
        var diffType = _random.Next(3); // 0: простое разделение переменных, 1: линейное первого порядка, 2: однородное

        return diffType switch
        {
            0 => GenerateSeparableDifferential(),
            1 => GenerateLinearFirstOrderDifferential(),
            2 => GenerateHomogeneousDifferential(),
            _ => GenerateSeparableDifferential()
        };
    }

    private (string equation, string answer) GenerateSeparableDifferential()
    {
        var coefficient = _random.Next(1, 4);
        var initialValue = _random.Next(1, 4);
        var xValue = _random.Next(2, 5);

        // dy/dx = coefficient * y, y(0) = initialValue
        // Решение: y = initialValue * e^(coefficient * x)
        var result = initialValue * Math.Exp(coefficient * xValue);

        var equation = $"dy/dx = {coefficient}y, y(0) = {initialValue}, y({xValue}) = ?";
        var answer = result.ToString("F2");

        return (equation, answer);
    }

    private (string equation, string answer) GenerateLinearFirstOrderDifferential()
    {
        var a = _random.Next(1, 4);
        var b = _random.Next(1, 4);
        var initialValue = _random.Next(1, 4);
        var xValue = _random.Next(1, 4);

        // dy/dx + ay = b, y(0) = initialValue
        // Решение: y = (b/a) + (initialValue - b/a) * e^(-a*x)
        var result = (b / (double)a) + (initialValue - (b / (double)a)) * Math.Exp(-a * xValue);

        var equation = $"dy/dx + {a}y = {b}, y(0) = {initialValue}, y({xValue}) = ?";
        var answer = result.ToString("F2");

        return (equation, answer);
    }

    private (string equation, string answer) GenerateHomogeneousDifferential()
    {
        var coefficient = _random.Next(1, 4);
        var initialValue = _random.Next(1, 4);
        var xValue = _random.Next(1, 4);

        // dy/dx = coefficient * y/x, y(1) = initialValue
        // Решение: y = initialValue * x^coefficient
        var result = initialValue * Math.Pow(xValue, coefficient);

        var equation = $"dy/dx = {coefficient}y/x, y(1) = {initialValue}, y({xValue}) = ?";
        var answer = result.ToString("F2");

        return (equation, answer);
    }

    // Координатная геометрия
    private (string equation, string answer) GenerateCoordinateGeometry()
    {
        var taskType = _random.Next(3);

        return taskType switch
        {
            0 => GenerateDistanceBetweenPoints(),
            1 => GenerateMidpointCalculation(),
            2 => GenerateSlopeCalculation(),
            _ => GenerateDistanceBetweenPoints()
        };
    }

    private (string equation, string answer) GenerateDistanceBetweenPoints()
    {
        var x1 = _random.Next(-10, 11);
        var y1 = _random.Next(-10, 11);
        var x2 = _random.Next(-10, 11);
        var y2 = _random.Next(-10, 11);

        var distance = Math.Sqrt((x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1));
        var equation = $"Найдите расстояние между точками A({x1}, {y1}) и B({x2}, {y2})";
        var answer = distance.ToString("F2");

        return (equation, answer);
    }

    private (string equation, string answer) GenerateMidpointCalculation()
    {
        var x1 = _random.Next(-10, 11);
        var y1 = _random.Next(-10, 11);
        var x2 = _random.Next(-10, 11);
        var y2 = _random.Next(-10, 11);

        var midX = (x1 + x2) / 2.0;
        var midY = (y1 + y2) / 2.0;
        var equation = $"Найдите середину отрезка между точками A({x1}, {y1}) и B({x2}, {y2})";
        var answer = $"({midX}, {midY})";

        return (equation, answer);
    }

    private (string equation, string answer) GenerateSlopeCalculation()
    {
        var x1 = _random.Next(-10, 11);
        var y1 = _random.Next(-10, 11);
        var x2 = _random.Next(-10, 11);
        var y2 = _random.Next(-10, 11);

        if (x2 == x1) // вертикальная линия
        {
            var slopeEquationText = $"Найдите угловой коэффициент прямой, проходящей через точки A({x1}, {y1}) и B({x2}, {y2})";
            var slopeAnswerText = "неопределен (вертикальная линия)";
            return (slopeEquationText, slopeAnswerText);
        }

        var slope = (y2 - y1) / (double)(x2 - x1);
        var slopeEquation = $"Найдите угловой коэффициент прямой, проходящей через точки A({x1}, {y1}) и B({x2}, {y2})";
        var slopeAnswer = slope.ToString("F2");

        return (slopeEquation, slopeAnswer);
    }

    // Графики функций
    private (string equation, string answer) GenerateFunctionGraph()
    {
        var taskType = _random.Next(3);

        return taskType switch
        {
            0 => GenerateLinearFunctionGraph(),
            1 => GenerateQuadraticFunctionGraph(),
            2 => GeneratePointOnGraph(),
            _ => GenerateLinearFunctionGraph()
        };
    }

    private (string equation, string answer) GenerateLinearFunctionGraph()
    {
        var m = _random.Next(-5, 6);
        var b = _random.Next(-10, 11);

        var equation = $"Постройте график функции y = {m}x + {b} и найдите точку пересечения с осью Y";
        var answer = $"(0, {b})";

        return (equation, answer);
    }

    private (string equation, string answer) GenerateQuadraticFunctionGraph()
    {
        var a = _random.Next(1, 4);
        var b = _random.Next(-5, 6);
        var c = _random.Next(-10, 11);

        var equation = $"Постройте график функции y = {a}x² + {b}x + {c} и найдите координаты вершины";
        var vertexX = -b / (2.0 * a);
        var vertexY = a * vertexX * vertexX + b * vertexX + c;
        var answer = $"({vertexX:F2}, {vertexY:F2})";

        return (equation, answer);
    }

    private (string equation, string answer) GeneratePointOnGraph()
    {
        var m = _random.Next(-5, 6);
        var b = _random.Next(-10, 11);
        var x = _random.Next(-5, 6);
        var y = m * x + b;

        var equation = $"Проверьте, лежит ли точка ({x}, {y}) на графике функции y = {m}x + {b}";
        var answer = "да";

        return (equation, answer);
    }

    // Точки пересечения
    private (string equation, string answer) GenerateIntersectionPoint()
    {
        var taskType = _random.Next(2);

        return taskType switch
        {
            0 => GenerateLinesIntersection(),
            1 => GenerateLineCircleIntersection(),
            _ => GenerateLinesIntersection()
        };
    }

    private (string equation, string answer) GenerateLinesIntersection()
    {
        var m1 = _random.Next(-5, 6);
        var b1 = _random.Next(-10, 11);
        var m2 = _random.Next(-5, 6);
        var b2 = _random.Next(-10, 11);

        if (m1 == m2) // параллельные линии
        {
            var intersectionEquationText = $"Найдите точку пересечения прямых y = {m1}x + {b1} и y = {m2}x + {b2}";
            var intersectionAnswerText = "прямые параллельны, не пересекаются";
            return (intersectionEquationText, intersectionAnswerText);
        }

        var x = (b2 - b1) / (double)(m1 - m2);
        var y = m1 * x + b1;
        var intersectionEquation = $"Найдите точку пересечения прямых y = {m1}x + {b1} и y = {m2}x + {b2}";
        var intersectionAnswer = $"({x:F2}, {y:F2})";

        return (intersectionEquation, intersectionAnswer);
    }

    private (string equation, string answer) GenerateLineCircleIntersection()
    {
        var h = _random.Next(-5, 6);
        var k = _random.Next(-5, 6);
        var r = _random.Next(2, 6);
        var m = _random.Next(-3, 4);
        var b = _random.Next(-8, 9);

        // Подставляем y = mx + b в уравнение окружности (x-h)² + (y-k)² = r²
        // Получаем: (x-h)² + (mx + b - k)² = r²
        // Раскрываем: x² - 2hx + h² + m²x² + 2m(b-k)x + (b-k)² = r²
        // Приводим к виду: (1 + m²)x² + 2(m(b-k) - h)x + (h² + (b-k)² - r²) = 0

        var A = 1 + m * m;
        var B = 2 * (m * (b - k) - h);
        var C = h * h + (b - k) * (b - k) - r * r;

        // Вычисляем дискриминант
        var discriminant = B * B - 4 * A * C;

        string answer;
        if (discriminant < 0)
        {
            answer = "нет точек пересечения";
        }
        else if (discriminant == 0)
        {
            var x = -B / (2.0 * A);
            var y = m * x + b;
            answer = $"({x:F2}, {y:F2})";
        }
        else
        {
            var x1 = (-B + Math.Sqrt(discriminant)) / (2.0 * A);
            var y1 = m * x1 + b;
            var x2 = (-B - Math.Sqrt(discriminant)) / (2.0 * A);
            var y2 = m * x2 + b;
            answer = $"({x1:F2}, {y1:F2}) и ({x2:F2}, {y2:F2})";
        }

        var equation = $"Найдите точки пересечения прямой y = {m}x + {b} с окружностью (x-{h})² + (y-{k})² = {r}²";

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
