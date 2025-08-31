using System;
using System.Collections.Generic;
using System.Linq;
using Robust.Shared.Serialization;

namespace Content.Shared.ADT.CommandConsole;

[Serializable, NetSerializable]
public sealed class SnakeGame
{
    public enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }

    public enum CellType
    {
        Empty,
        Snake,
        Food,
        Wall
    }

    public const int Width = 20;
    public const int Height = 15;

    public List<(int x, int y)> Snake { get; private set; } = new();
    public (int x, int y) Food { get; private set; }
    public Direction CurrentDirection { get; private set; } = Direction.Right;
    public bool IsGameOver { get; private set; } = false;
    public int Score { get; private set; } = 0;
    public bool IsRunning { get; private set; } = false;

    [NonSerialized]
    private readonly System.Random _random = new();

    public SnakeGame()
    {
        InitializeGame();
    }

    public void InitializeGame()
    {
        Snake.Clear();
        Snake.Add((Width / 2, Height / 2));
        CurrentDirection = Direction.Right;
        IsGameOver = false;
        Score = 0;
        IsRunning = true;
        GenerateFood();
    }

    public string GetGameDisplay()
    {
        if (!IsRunning)
        {
            return "Snake game is not running. Type 'snake start' to begin.";
        }

        var display = new System.Text.StringBuilder();
        display.AppendLine("Score: " + Score);
        display.AppendLine("Use WASD to control the snake. Press 'q' to quit.");
        display.AppendLine();

        // Создаем игровое поле
        var grid = new CellType[Width, Height];

        // Добавляем змейку
        foreach (var segment in Snake)
        {
            if (segment.x >= 0 && segment.x < Width && segment.y >= 0 && segment.y < Height)
            {
                grid[segment.x, segment.y] = CellType.Snake;
            }
        }

        // Добавляем еду
        if (Food.x >= 0 && Food.x < Width && Food.y >= 0 && Food.y < Height)
        {
            grid[Food.x, Food.y] = CellType.Food;
        }

        // Рисуем границы
        display.AppendLine("+" + new string('-', Width) + "+");

        for (int y = 0; y < Height; y++)
        {
            display.Append("|");
            for (int x = 0; x < Width; x++)
            {
                switch (grid[x, y])
                {
                    case CellType.Empty:
                        display.Append(" ");
                        break;
                    case CellType.Snake:
                        display.Append("█");
                        break;
                    case CellType.Food:
                        display.Append("●");
                        break;
                    case CellType.Wall:
                        display.Append("█");
                        break;
                }
            }
            display.AppendLine("|");
        }

        display.AppendLine("+" + new string('-', Width) + "+");

        if (IsGameOver)
        {
            display.AppendLine("Game Over! Final Score: " + Score.ToString());
        }

        return display.ToString();
    }

    public string ProcessInput(string input)
    {
        if (!IsRunning)
        {
            return "Game is not running. Type 'snake start' to begin.";
        }

        if (IsGameOver)
        {
            return "Game Over! Type 'snake restart' to play again.";
        }

        var command = input.ToLower().Trim();

        switch (command)
        {
            case "w":
            case "up":
                if (CurrentDirection != Direction.Down)
                    CurrentDirection = Direction.Up;
                break;
            case "s":
            case "down":
                if (CurrentDirection != Direction.Up)
                    CurrentDirection = Direction.Down;
                break;
            case "a":
            case "left":
                if (CurrentDirection != Direction.Right)
                    CurrentDirection = Direction.Left;
                break;
            case "d":
            case "right":
                if (CurrentDirection != Direction.Left)
                    CurrentDirection = Direction.Right;
                break;
            case "q":
            case "quit":
                IsRunning = false;
                return "Game stopped.";
            default:
                return "Invalid command. Use WASD to move, Q to quit.";
        }

        // Обновляем игру
        UpdateGame();

        return GetGameDisplay();
    }

    private void UpdateGame()
    {
        if (IsGameOver || !IsRunning)
            return;

        var head = Snake[0];
        var newHead = CurrentDirection switch
        {
            Direction.Up => (head.x, head.y - 1),
            Direction.Down => (head.x, head.y + 1),
            Direction.Left => (head.x - 1, head.y),
            Direction.Right => (head.x + 1, head.y),
            _ => head
        };

        // Проверяем границы
        if (newHead.Item1 < 0 || newHead.Item1 >= Width || newHead.Item2 < 0 || newHead.Item2 >= Height)
        {
            IsGameOver = true;
            return;
        }

        // Проверяем столкновение с собой
        if (Snake.Contains(newHead))
        {
            IsGameOver = true;
            return;
        }

        // Добавляем новую голову
        Snake.Insert(0, newHead);

        // Проверяем, съели ли еду
        if (newHead == Food)
        {
            Score += 10;
            GenerateFood();
        }
        else
        {
            // Убираем хвост, если не съели еду
            Snake.RemoveAt(Snake.Count - 1);
        }
    }

    private void GenerateFood()
    {
        var availablePositions = new List<(int x, int y)>();

        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                if (!Snake.Contains((x, y)))
                {
                    availablePositions.Add((x, y));
                }
            }
        }

        if (availablePositions.Count > 0)
        {
            Food = availablePositions[_random.Next(availablePositions.Count)];
        }
    }

    public string Start()
    {
        InitializeGame();
        return GetGameDisplay();
    }

    public string Restart()
    {
        InitializeGame();
        return GetGameDisplay();
    }

    public string Stop()
    {
        IsRunning = false;
        return "Game stopped.";
    }
}
