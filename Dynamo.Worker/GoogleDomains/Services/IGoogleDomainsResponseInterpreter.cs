using Dynamo.Worker.GoogleDomains.Model;

namespace Dynamo.Worker.GoogleDomains.Services;

public interface IGoogleDomainsResponseInterpreter
{
    GoogleDomainsResponse InterpretResponseString(string responsePayload);
}