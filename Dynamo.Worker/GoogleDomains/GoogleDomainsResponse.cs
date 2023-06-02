namespace Dynamo.Worker.GoogleDomains;

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