using System.Collections.Generic;

public class Question
{
    public int Id { get; set; }
    public string Text { get; set; }
    public string[] Options { get; set; }
    public int CorrectOptionIndex { get; set; }
}