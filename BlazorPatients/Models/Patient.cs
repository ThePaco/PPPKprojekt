namespace BlazorPatients.Models;

public class Patient
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public bool IsMale { get; set; }
    public string Oib { get; set; }
    public DateTime Birthday { get; set; }
}
