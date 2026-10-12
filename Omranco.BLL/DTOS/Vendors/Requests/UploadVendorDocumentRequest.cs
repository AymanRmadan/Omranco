namespace Omranco.BLL.DTOS.Vendors.Requests;

public sealed record UploadVendorDocumentRequest(
    string FileName,
    string ContentType,
    long Length,
    Stream Content
);