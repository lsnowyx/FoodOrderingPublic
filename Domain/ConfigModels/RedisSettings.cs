namespace Domain.ConfigModels;

public sealed class RedisSettings
{
    public bool Enabled { get; set; }
    public string ConnectionString { get; set; } = string.Empty;
    public string InstanceName { get; set; } = string.Empty;
}
