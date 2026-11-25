namespace DesafioMatematico;
public enum Operation { Add, Subtract, Multiply, Divide }
public readonly record struct Question(string Text, int Answer);
public static class GameRules
{
    public const int TotalQuestions = 10;
    public const int SecondsPerQuestion = 30;
    public static int MaxOperand(int level) => level switch { 0 => 10, 1 => 50, 2 => 100, _ => throw new ArgumentOutOfRangeException(nameof(level)) };
    public static Question Generate(int level, IReadOnlyList<Operation> operations, Random random)
    {
        if (operations.Count == 0) throw new ArgumentException("Selecione uma operação.");
        int max = MaxOperand(level);
        int a = random.Next(1, max + 1), b = random.Next(1, max + 1);
        switch (operations[random.Next(operations.Count)])
        {
            case Operation.Add: return new($"{a} + {b} = ?", a + b);
            case Operation.Subtract:
                if (a < b) (a, b) = (b, a);
                return new($"{a} − {b} = ?", a - b);
            case Operation.Multiply: return new($"{a} × {b} = ?", a * b);
            case Operation.Divide:
                // Divisão exata, com ambos os operandos no intervalo da dificuldade.
                int quotient = random.Next(1, max / b + 1);
                return new($"{b * quotient} ÷ {b} = ?", quotient);
            default: throw new ArgumentOutOfRangeException(nameof(operations));
        }
    }
    public static int Points(int level, int secondsLeft) => 10 * (level + 1) + Math.Clamp(secondsLeft, 0, SecondsPerQuestion);
    public static string Classification(int correct) => correct switch { >= 9 => "Excelente!", >= 7 => "Muito bom!", >= 5 => "Bom desempenho", _ => "Continue praticando" };
}