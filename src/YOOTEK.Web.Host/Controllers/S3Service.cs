using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using IMAX.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ImaxFileUploaderServer.Services
{

    public interface IS3Service
    {
        Task<string> UploadToPublic(string keyName, string filePath);
        Task<string> UploadToPublic(string keyName, IFormFile file);
    }

    public class S3Service : IS3Service
    {
        private readonly IAmazonS3 _s3Client;
        private readonly TransferUtility _transferUtility;
        private readonly string _bucketName;
        private readonly string _baseUrl;
        private readonly IConfigurationRoot _appConfiguration;

        public S3Service(IWebHostEnvironment env)
        {
            _appConfiguration = env.GetAppConfiguration();
            var accessKey = _appConfiguration["S3:AccessKey"];
            var secretKey = _appConfiguration["S3:SecretKey"];
            var bucketName = _appConfiguration["S3:BucketName"];
            var region = _appConfiguration["S3:Region"];
            var baseUrl = _appConfiguration["S3:BaseUrl"];


            _bucketName = bucketName;
            _baseUrl = baseUrl;

            _s3Client = new AmazonS3Client(new BasicAWSCredentials(accessKey, secretKey),
                RegionEndpoint.GetBySystemName(region));

            _transferUtility = new TransferUtility(_s3Client);
        }

        public async Task<string> UploadToPublic(string keyName, string filePath)
        {
            try
            {
                await _transferUtility.UploadAsync(filePath, _bucketName, $"public/{keyName}");

                return $"{_baseUrl}/public/{keyName}";
            }
            catch (AmazonS3Exception ex)
            {

                return "";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unknown error encountered on client. Message:'{ex.Message}' when writing an object");
                return "";
            }
        }

        public async Task<string> UploadToPublic(string keyName, IFormFile file)
        {
            try
            {
                using (var stream = file.OpenReadStream())
                {
                    await _transferUtility.UploadAsync(stream, _bucketName, $"public/{keyName}");
                }

                return $"{_baseUrl}/public/{keyName}";
            }
            catch (AmazonS3Exception ex)
            {
                Console.WriteLine($"Error encountered on server. Message:'{ex.Message}' when writing an object");
                return "";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unknown error encountered on client. Message:'{ex.Message}' when writing an object");
                return "";
            }
        }

        public async Task<List<Amazon.S3.Model.S3Object>> GetListObject(string keyName, string fileName)
        {
            var listResult = await _s3Client.ListObjectsAsync(new ListObjectsRequest()
            {
                BucketName = _bucketName,
            });
            return listResult.S3Objects;
        }
    }
}
