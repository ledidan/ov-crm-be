using System.Text.RegularExpressions;
using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration;


namespace ServerLibrary.Services
{
    public class S3Service : IS3Service
    {
        private readonly IAmazonS3 _s3Client;
        private readonly string _bucketName;

        public S3Service(IConfiguration configuration)
        {
            var awsConfig = configuration.GetSection("AWS");
            _bucketName = awsConfig["BucketName"];
            var accessKey = awsConfig["AccessKey"];
            var secretKey = awsConfig["SecretKey"];
            var region = awsConfig["Region"];
            if (string.IsNullOrEmpty(_bucketName))
            {
                throw new ArgumentNullException(nameof(_bucketName), "AWS S3 BucketName is missing in configuration (AWS:S3Bucket).");
            }
            if (string.IsNullOrEmpty(accessKey))
            {
                throw new ArgumentNullException(nameof(accessKey), "AWS AccessKey is missing in configuration (AWS:AccessKey).");
            }
            if (string.IsNullOrEmpty(secretKey))
            {
                throw new ArgumentNullException(nameof(secretKey), "AWS SecretKey is missing in configuration (AWS:SecretKey).");
            }
            if (string.IsNullOrEmpty(region))
            {
                throw new ArgumentNullException(nameof(region), "AWS Region is missing in configuration (AWS:Region).");
            }
            var credentials = new BasicAWSCredentials(accessKey, secretKey);
            var s3Config = new AmazonS3Config { RegionEndpoint = RegionEndpoint.GetBySystemName(region) };
            _s3Client = new AmazonS3Client(credentials, s3Config);
        }
        public async Task<bool> TestConnectionAsync()
        {
            var request = new ListObjectsV2Request { BucketName = _bucketName };
            await _s3Client.ListObjectsV2Async(request);
            return true;
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
            var formattedFileName = $"{fileName}";
            var partnerRoute = $"organizations/partner_{partnerId}/user_{userId}/{app}/{formattedFileName}";
            var individualRoute = $"individuals/user_{userId}/{app}/{formattedFileName}";

            string location = userType == "partner" ? partnerRoute : individualRoute;

            var request = new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = location,
                InputStream = fileStream,
                ContentType = contentType,
                // CannedACL = S3CannedACL.PublicRead
            };
            Console.WriteLine($"Uploading image to {_bucketName} at {location}");
            await _s3Client.PutObjectAsync(request);
            return location; // Return S3 file URL
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

                Console.WriteLine("Handling file size memory stream");
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
        public async Task RemoveFileFromS3(string fileKey)
        {
            try
            {
                if (string.IsNullOrEmpty(fileKey))
                {
                    throw new ArgumentException("File key cannot be null or empty.");
                }

                var request = new DeleteObjectRequest
                {
                    BucketName = _bucketName,
                    Key = fileKey // Đường dẫn file trên S3 (ví dụ: "organizations/partner_1/user_1/crm/product_20231010120000.jpg")
                };

                Console.WriteLine($"Removing file from S3: {_bucketName}/{fileKey}");
                await _s3Client.DeleteObjectAsync(request);
                Console.WriteLine($"Successfully removed file: {fileKey}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to remove file from S3: {ex.Message}");
                throw new Exception($"Failed to remove file from S3: {ex.Message}");
            }
        }
    }
}

