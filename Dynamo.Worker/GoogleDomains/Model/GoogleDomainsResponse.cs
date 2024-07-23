namespace Dynamo.Worker.GoogleDomains.Model;

public enum GoogleDomainsResponse
{
    Unknown,
    Good,
    IpAlreadySet,
    InvalidHost,
    AuthFailed,
    NonFullyQualifiedHostname,
    BannedUserAgent,
    Abuse,
    TemporaryProblem,
    ConflictingRecords,
}