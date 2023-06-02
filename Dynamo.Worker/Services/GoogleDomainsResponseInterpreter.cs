using Dynamo.Worker.GoogleDomains;

namespace Dynamo.Worker.Services;

public class GoogleDomainsResponseInterpreter : IGoogleDomainsResponseInterpreter
{
    private const string GoodResponse = "Good";
    private const string IpAlreadySetResponse = "nochg";
    private const string InvalidHostResponse = "nohost";
    private const string AuthFailedResponse = "badauth";
    private const string NonFullyQualifiedHostnameResponse = "notfqdn";
    private const string BannedUserAgentResponse = "badagent";
    private const string AbuseResponse = "abuse";
    private const string TemporaryProblemResponse = "911";
    private const string ConflictingRecordsResponse = "conflict";

    public GoogleDomainsResponse InterpretResponseString(string responsePayload)
    {
        if (responsePayload.StartsWith(GoodResponse))
            return GoogleDomainsResponse.Good;
        if (responsePayload.StartsWith(ConflictingRecordsResponse))
            return GoogleDomainsResponse.ConflictingRecords;
        if (responsePayload.StartsWith(IpAlreadySetResponse))
            return GoogleDomainsResponse.IpAlreadySet;

        return responsePayload switch
        {
            InvalidHostResponse => GoogleDomainsResponse.InvalidHost,
            AuthFailedResponse => GoogleDomainsResponse.AuthFailed,
            NonFullyQualifiedHostnameResponse => GoogleDomainsResponse.NonFullyQualifiedHostname,
            BannedUserAgentResponse => GoogleDomainsResponse.BannedUserAgent,
            AbuseResponse => GoogleDomainsResponse.Abuse,
            TemporaryProblemResponse => GoogleDomainsResponse.TemporaryProblem,
            _ => GoogleDomainsResponse.Unknown
        };
    }
}