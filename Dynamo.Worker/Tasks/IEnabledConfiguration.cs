namespace Dynamo.Worker.Tasks;

public interface IEnabledConfiguration
{
    public bool Enabled { get; set; }

    public string Identifier { get; }
}