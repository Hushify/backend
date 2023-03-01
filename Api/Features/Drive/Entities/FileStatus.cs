namespace Hushify.Api.Features.Drive.Entities;

public enum FileStatus
{
    UploadStarted,
    UploadFailed,
    UploadCancelled,
    UploadFinished,
    Deleted,
    PermanentlyDeleted,
    SizeMismatch,
    MaliciousUpload,
    OldVersion
}