namespace Hushify.Api.Features.Drive.Messaging.Events;

public class S3EventMessage
{
    public Record[] Records { get; set; } = default!;
}

public class Record
{
    public string EventVersion { get; set; } = default!;
    public string EventSource { get; set; } = default!;
    public string AwsRegion { get; set; } = default!;
    public DateTimeOffset EventTime { get; set; } = default!;
    public string EventName { get; set; } = default!;
    public OwnerIdentity UserIdentity { get; set; } = default!;
    public RequestParameters RequestParameters { get; set; } = default!;
    public ResponseElements ResponseElements { get; set; } = default!;
    public S3 S3 { get; set; } = default!;
    public GlacierEventData? GlacierEventData { get; set; } = default!;
}

public class GlacierEventData
{
    public RestoreEventData RestoreEventData { get; set; } = default!;
}

public class RestoreEventData
{
    public DateTimeOffset LifecycleRestorationExpiryTime { get; set; }
    public string LifecycleRestoreStorageClass { get; set; } = default!;
}

public class RequestParameters
{
    public string SourceIpAddress { get; set; } = default!;
}

public class ResponseElements
{
    public string XAmzRequestId { get; set; } = default!;
    public string XAmzId2 { get; set; } = default!;
}

public class S3
{
    public string S3SchemaVersion { get; set; } = default!;
    public string ConfigurationId { get; set; } = default!;
    public Bucket Bucket { get; set; } = default!;
    public Object Object { get; set; } = default!;
}

public class Bucket
{
    public string Name { get; set; } = default!;
    public OwnerIdentity OwnerIdentity { get; set; } = default!;
    public string Arn { get; set; } = default!;
}

public class OwnerIdentity
{
    public string PrincipalId { get; set; } = default!;
}

public class Object
{
    public string Key { get; set; } = default!;
    public long Size { get; set; } = default!;
    public string ETag { get; set; } = default!;
    public string? VersionId { get; set; }
    public string Sequencer { get; set; } = default!;
}