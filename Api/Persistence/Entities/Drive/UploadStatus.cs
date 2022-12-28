namespace Hushify.Api.Persistence.Entities.Drive;

public enum UploadStatus
{
    UploadStarted,
    UploadFailed,
    UploadCancelled,
    UploadFinished,
    Deleted,
    PermanentlyDeleted,
    SizeMismatch,
    MaliciousUpload
}