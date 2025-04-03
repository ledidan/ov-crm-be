public interface IS3Service
{
    Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType, string app, string userType, int userId, int partnerId);
    Task<string> UploadBase64ImageToS3(string base64Image, string categoryName, int partnerId, int userId, string app, string userType);
    Task RemoveFileFromS3(string fileKey);
}