using System.Diagnostics;
using Microsoft.Maui.Dispatching;
namespace DesafioMatematico;
public partial class MainPage : ContentPage
{
    private readonly Random random = new();
    private readonly Stopwatch clock = new();
    private readonly IDispatcherTimer timer;
    private readonly List<Operation> operations = new();
    private int level, number, score, correct, wrong, expired;
    private double totalTime;
    private Question current;
    private bool active, filtering;
    public MainPage()
    {
        InitializeComponent();
        DifficultyPicker.SelectedIndex = 0;
        timer = Dispatcher.CreateTimer();
        timer.Interval = TimeSpan.FromMilliseconds(100);
        timer.Tick += OnTick;
    }
    private void OnStart(object sender, EventArgs e)
    {
        operations.Clear();
        if (AddBox.IsChecked) operations.Add(Operation.Add);
        if (SubBox.IsChecked) operations.Add(Operation.Subtract);
        if (MulBox.IsChecked) operations.Add(Operation.Multiply);
        if (DivBox.IsChecked) operations.Add(Operation.Divide);
        if (operations.Count == 0 || DifficultyPicker.SelectedIndex < 0)
        { SetupMessage.Text = "Selecione a dificuldade e pelo menos uma operação."; return; }
        SetupMessage.Text = string.Empty;
        level = DifficultyPicker.SelectedIndex;
        number = score = correct = wrong = expired = 0; totalTime = 0;
        Setup.IsVisible = Summary.IsVisible = false; Game.IsVisible = true;
        ShowQuestion();
    }
    private void ShowQuestion()
    {
        number++;
        current = GameRules.Generate(level, operations, random);
        QuestionCount.Text = $"Questão {number}/{GameRules.TotalQuestions}";
        GameProgress.Progress = (number - 1) / (double)GameRules.TotalQuestions;
        ScoreLabel.Text = $"{score} pts";
        QuestionLabel.Text = current.Text; Answer.Text = string.Empty;
        Feedback.Text = string.Empty; Answer.IsEnabled = SubmitButton.IsEnabled = true;
        NextButton.IsVisible = false; active = true;
        clock.Restart(); UpdateTime(GameRules.SecondsPerQuestion); timer.Start();
    }
    private int Remaining => Math.Max(0, (int)Math.Ceiling(GameRules.SecondsPerQuestion - clock.Elapsed.TotalSeconds));
    private void UpdateTime(int seconds)
    {
        TimeLabel.Text = $"{seconds} s";
        TimeProgress.Progress = seconds / (double)GameRules.SecondsPerQuestion;
        var color = Color.FromArgb(seconds <= 5 ? "#DC2626" : seconds <= 10 ? "#D97706" : "#0D9488");
        TimeProgress.ProgressColor = color; TimeLabel.TextColor = color;
    }
    private void OnTick(object? sender, EventArgs e)
    {
        if (!active) return;
        int remaining = Remaining; UpdateTime(remaining);
        if (remaining == 0) FinishQuestion(null);
    }
    private void OnAnswerChanged(object sender, TextChangedEventArgs e)
    {
        if (filtering) return;
        string value = e.NewTextValue ?? string.Empty;
        string digits = new(value.Where(c => c >= '0' && c <= '9').ToArray());
        if (value != digits) { filtering = true; Answer.Text = digits; filtering = false; }
    }
    private void OnSubmit(object sender, EventArgs e)
    {
        if (!active) return;
        if (Remaining == 0) { FinishQuestion(null); return; }
        if (!int.TryParse(Answer.Text, out int answer))
        { Feedback.TextColor = Color.FromArgb("#B91C1C"); Feedback.Text = "Digite uma resposta numérica."; return; }
        FinishQuestion(answer);
    }
    private void FinishQuestion(int? answer)
    {
        if (!active) return;
        int remaining = Remaining;
        if (remaining == 0) answer = null;
        active = false; timer.Stop(); clock.Stop();
        totalTime += Math.Min(GameRules.SecondsPerQuestion, clock.Elapsed.TotalSeconds);
        Answer.IsEnabled = SubmitButton.IsEnabled = false; UpdateTime(remaining);
        if (answer == current.Answer)
        {
            int earned = GameRules.Points(level, remaining);
            correct++; score += earned; Feedback.TextColor = Color.FromArgb("#15803D");
            Feedback.Text = $"Correto! +{earned} pontos.";
        }
        else
        {
            if (answer is null) expired++; else wrong++;
            Feedback.TextColor = Color.FromArgb("#B91C1C");
            Feedback.Text = $"{(answer is null ? "Tempo esgotado!" : "Resposta incorreta!")} Resposta: {current.Answer}.";
        }
        ScoreLabel.Text = $"{score} pts"; GameProgress.Progress = number / (double)GameRules.TotalQuestions;
        NextButton.Text = number == GameRules.TotalQuestions ? "Ver resultado" : "Próxima questão";
        NextButton.IsVisible = true;
    }
    private void OnNext(object sender, EventArgs e)
    {
        if (active || !NextButton.IsVisible) return;
        NextButton.IsVisible = false;
        if (number < GameRules.TotalQuestions) { ShowQuestion(); return; }
        Game.IsVisible = false; Summary.IsVisible = true;
        PerformanceLabel.Text = GameRules.Classification(correct);
        FinalScore.Text = $"Pontuação: {score} pontos";
        StatsLabel.Text = $"Acertos: {correct}/10 ({correct * 10}%)\nErros: {wrong}\nTempo esgotado: {expired}";
        AccuracyProgress.Progress = correct / 10.0;
        TimeStats.Text = $"Tempo médio por questão: {totalTime / 10:F1} s";
    }
    private void OnRestart(object sender, EventArgs e)
    {
        active = false; timer.Stop(); clock.Reset();
        Game.IsVisible = Summary.IsVisible = false; Setup.IsVisible = true;
    }
    protected override void OnDisappearing() { timer.Stop(); base.OnDisappearing(); }
    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (active) { if (Remaining == 0) FinishQuestion(null); else timer.Start(); }
    }
}