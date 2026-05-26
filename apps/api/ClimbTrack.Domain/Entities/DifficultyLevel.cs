namespace ClimbTrack.Domain.Entities;

public class DifficultyLevel
{
    private DifficultyLevel() { }

    public DifficultyLevel(string code, string name, int maxDaysWeek, string? gradeRange = null)
    {
        Code = code;
        Name = name;
        MaxDaysWeek = maxDaysWeek;
        GradeRange = gradeRange;
    }

    public int Id { get; private set; }
    public string Code { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public string? GradeRange { get; private set; }
    public int MaxDaysWeek { get; private set; }
}
