public class PlayerState
{
    public string   SessionId { get; set; } = default!;
    public string   Name { get; set; } = default!;
    public float    X { get; set; }
    public float    Y { get; set; }
    public int      Score { get; set; }
    public DateTime LastActivity { get; set; }
}
