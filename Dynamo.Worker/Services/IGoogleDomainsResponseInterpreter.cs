using Dynamo.Worker.GoogleDomains;

namespace Dynamo.Worker.Services;

public interface IGoogleDomainsResponseInterpreter
{
    GoogleDomainsResponse InterpretResponseString(string responsePayload);
}