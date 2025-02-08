using System.Text.RegularExpressions;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration;


namespace ServerLibrary.Services
{
    public class S3Service
    {
        private readonly IAmazonS3 _s3Client;
        private readonly string _bucketName;

        public S3Service(IConfiguration configuration)
        {
            _s3Client = new AmazonS3Client(
                configuration["AWS:AccessKey"],
                configuration["AWS:SecretKey"],
                RegionEndpoint.GetBySystemName(configuration["AWS:Region"])
            );
            _bucketName = configuration["AWS:BucketName"];
        }

        public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType, string app, string userType,
         int userId, int partnerId)
        {
            if (fileStream.Length > 2 * 1024 * 1024) // 2MB limit
            {
                throw new Exception("File size exceeds the 2MB limit.");
            }
            var allowedExtensions = new List<string> { ".jpg", ".png", ".jpeg", ".gif", ".pdf" };
            var fileExtension = Path.GetExtension(fileName).ToLower();
            if (!allowedExtensions.Contains(fileExtension))
            {
                throw new Exception("Invalid file type. Only JPG, PNG, GIF, and PDF files are allowed.");
            }
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            var partnerRoute = $"organizations/partnerId_{partnerId}/userId_{userId}/{app}/{timestamp}_{fileName}";
            var individualRoute = $"individuals/userId_{userId}/{app}/{timestamp}_{fileName}";

            string location = userType == "partner" ? partnerRoute : individualRoute;

            var request = new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = location,
                InputStream = fileStream,
                ContentType = contentType,
                // CannedACL = S3CannedACL.PublicRead
            };

            await _s3Client.PutObjectAsync(request);

            return $"https://{_bucketName}.s3.amazonaws.com/{request.Key}"; // Return S3 file URL
        }

           public async Task<string> UploadBase64ImageToS3(string base64Image, string categoryName, int partnerId, int userId, string app, string userType)
        {
            try
            {
                var match = Regex.Match(base64Image, @"data:image/(?<type>.+?);base64,(?<data>.+)");
                if (!match.Success)
                {
                    throw new Exception("Invalid base64 image format.");
                }

                string fileType = match.Groups["type"].Value; 
                string base64Data = match.Groups["data"].Value;
                
                byte[] imageBytes = Convert.FromBase64String(base64Data);
                using var memoryStream = new MemoryStream(imageBytes);

                if (memoryStream.Length > 2 * 1024 * 1024)
                {
                    throw new Exception("File size exceeds the 2MB limit.");
                }

                string timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
                string fileName = $"{categoryName}_{timestamp}.{fileType}";

                return await UploadFileAsync(memoryStream, fileName, $"image/{fileType}", app, userType, userId, partnerId);
            }
            catch (Exception ex)
            {
                throw new Exception($"Image upload failed: {ex.Message}");
            }
        }
    }
}

