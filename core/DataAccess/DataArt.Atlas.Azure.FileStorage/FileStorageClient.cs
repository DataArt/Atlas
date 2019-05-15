#region License
// =================================================================================================
// Copyright 2018 DataArt, Inc.
// -------------------------------------------------------------------------------------------------
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this work except in compliance with the License.
// You may obtain a copy of the License in the LICENSE file, or at:
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// =================================================================================================
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DataArt.Atlas.Azure.FileStorage.Models;
using DataArt.Atlas.Infrastructure.Exceptions;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

#if NET452
using Serilog;
#endif

#if NETSTANDARD2_0
using DataArt.Atlas.Infrastructure.Logging;
using Microsoft.Extensions.Logging;
#endif

namespace DataArt.Atlas.Azure.FileStorage
{
    internal class FileStorageClient : IFileStorageClient
    {
        private const string FileNameMetadata = "FileName";

#if NETSTANDARD2_0
        private readonly ILogger logger = AtlasLogging.CreateLogger<FileStorageClient>();
#endif

        private readonly CloudStorageAccount cloudStorageAccount;

        public FileStorageClient(FileStorageSettings fileStorageSettings)
        {
            if (string.IsNullOrEmpty(fileStorageSettings?.ConnectionString))
            {
                throw GetConfigurationException();
            }

            try
            {
                cloudStorageAccount = CloudStorageAccount.Parse(fileStorageSettings.ConnectionString);
            }
            catch (Exception ex)
            {
                throw GetConfigurationException(ex);
            }
        }

        public async Task<FileStorageWriteStream> GetWriteStreamAsync(string containerName, UploadFileProperties uploadFileProperties)
        {
            var containerReference = GetBlobContainer(containerName);
            var blobName = GetBlobName(uploadFileProperties);

            var blobReference = containerReference.GetBlockBlobReference(blobName);
            blobReference.Metadata.Add(FileNameMetadata, uploadFileProperties.FileName);
            blobReference.Properties.ContentType = uploadFileProperties.ContentType;
            blobReference.Properties.ContentDisposition = uploadFileProperties.ContentDisposition;

            Func<Task<FileStorageWriteStream>> openStreamDelegate = async () =>
            {
                var stream = await blobReference.OpenWriteAsync();
#if NET452
                Log.Information("File {FileId} write stream to the storage was opened", blobName);
#endif

#if NETSTANDARD2_0
                logger.LogInformation("File {FileId} write stream to the storage was opened", blobName);
#endif
                return new FileStorageWriteStream { Id = blobName, Stream = stream };
            };

            try
            {
                // try to open stream assuming container exists
                return await openStreamDelegate();
            }
            catch (StorageException e)
            {
                // case when container was not created yet
                if (e.RequestInformation.HttpStatusCode == 404)
                {
#if NET452
                    Log.Warning(e, "Exception while opening a write stream to the storage");
#endif

#if NETSTANDARD2_0
                    logger.LogWarning(e, "Exception while opening a write stream to the storage");
#endif

                    await EnsureContainerExists(containerReference);
                    return await openStreamDelegate();
                }

#if NET452
                Log.Error(e, "Failed to open a write stream to the storage");
#endif

#if NETSTANDARD2_0
                logger.LogError(e, "Failed to open a write stream to the storage");
#endif
                throw;
            }
        }

        public async Task<string> UploadAsync(string containerName, UploadFileProperties uploadFileProperties, Stream streamToUpload)
        {
            var containerReference = GetBlobContainer(containerName);
            var blobName = GetBlobName(uploadFileProperties);

            var blobReference = containerReference.GetBlockBlobReference(blobName);
            blobReference.Metadata.Add(FileNameMetadata, uploadFileProperties.FileName);
            blobReference.Properties.ContentType = uploadFileProperties.ContentType;
            blobReference.Properties.ContentDisposition = uploadFileProperties.ContentDisposition;

            Func<Task<string>> uploadDelegate = async () =>
            {
                await blobReference.UploadFromStreamAsync(streamToUpload);
#if NET452
                Log.Information("File {FileId} was uploaded to the storage", blobName);
#endif

#if NETSTANDARD2_0
                logger.LogInformation("File {FileId} was uploaded to the storage", blobName);
#endif
                return blobName;
            };

            try
            {
                // try to upload file assuming container exists
                return await uploadDelegate();
            }
            catch (StorageException e)
            {
                // case when container was not created yet
                if (e.RequestInformation.HttpStatusCode == 404)
                {
#if NET452
                    Log.Warning(e, "Exception while uploading a file");
#endif

#if NETSTANDARD2_0
                    logger.LogWarning(e, "Exception while uploading a file");
#endif
                    await EnsureContainerExists(containerReference);
                    return await uploadDelegate();
                }

#if NET452
                Log.Error(e, "Failed to upload a file to the storage");
#endif

#if NETSTANDARD2_0
                logger.LogError(e, "Failed to upload a file to the storage");
#endif
                throw;
            }
        }

        public async Task<FileStorageReadStream> GetReadStreamAsync(string containerName, string id)
        {
            var containerReference = GetBlobContainer(containerName);
            var blobReference = containerReference.GetBlockBlobReference(id);

            try
            {
                var downloadFileStream = await blobReference.OpenReadAsync();

                return new FileStorageReadStream
                {
                    FileName = GetFileName(blobReference),
                    ContentType = blobReference.Properties.ContentType,
                    ContentLength = blobReference.Properties.Length,
                    ContentDisposition = blobReference.Properties.ContentDisposition,
                    Id = id,
                    Stream = downloadFileStream
                };
            }
            catch (Exception e)
            {
                var handledException = HandleStorageException(e);

                if (handledException != null)
                {
                    throw handledException;
                }

#if NET452
                Log.Error(e, "Failed to open file {FileId} read stream at the storage", id);
#endif

#if NETSTANDARD2_0
                logger.LogError(e, "Failed to open file {FileId} read stream at the storage", id);
#endif
                throw;
            }
        }

        public IEnumerable<string> GetList(string containerName)
        {
            var containerReference = GetBlobContainer(containerName);

#if NET452
            var listBlobItems = containerReference.ListBlobs();

            return listBlobItems.Select(b => Path.GetFileName(b.Uri.LocalPath));
#endif

#if NETSTANDARD2_0
            var list = new List<IListBlobItem>();
            BlobContinuationToken token = null;
            do
            {
                var resultSegment =
                    containerReference.ListBlobsSegmentedAsync(
                        string.Empty,
                        true,
                        default(BlobListingDetails),
                        null,
                        token,
                        null,
                        null).GetAwaiter().GetResult();
                token = resultSegment.ContinuationToken;

                list.AddRange(resultSegment.Results);
            }
            while (token != null);

            return list.Select(b => Path.GetFileName(b.Uri.LocalPath));
#endif
        }

        public async Task DeleteAsync(string containerName, string id, bool silent)
        {
            var containerReference = GetBlobContainer(containerName);
            var blobReference = containerReference.GetBlockBlobReference(id);

            try
            {
                await blobReference.DeleteAsync();
#if NET452
                Log.Information("File {FileId} was deleted from the storage", id);
#endif

#if NETSTANDARD2_0
                logger.LogInformation("File {FileId} was deleted from the storage", id);
#endif
            }
            catch (Exception e)
            {
                var handledException = HandleStorageException(e);

                if (handledException != null)
                {
#if NET452
                    Log.Warning(e, "Failed to delete file {FileId} from the storage", id);
#endif

#if NETSTANDARD2_0
                    logger.LogWarning(e, "Failed to delete file {FileId} from the storage", id);
#endif

                    if (silent)
                    {
                        return;
                    }

                    throw handledException;
                }

#if NET452
                Log.Error(e, "Failed to delete file {FileId} from the storage", id);
#endif

#if NETSTANDARD2_0
                logger.LogError(e, "Failed to delete file {FileId} from the storage", id);
#endif

                if (silent)
                {
                    return;
                }

                throw;
            }
        }

        private static string GetBlobName(UploadFileProperties uploadFileProperties)
        {
            var fileName = uploadFileProperties.FileName;
            if (!uploadFileProperties.EnforceNameUniqueness)
            {
                if (string.IsNullOrEmpty(fileName))
                {
                    throw new ArgumentException("File name should be set");
                }

                return fileName;
            }

            try
            {
                if (!string.IsNullOrEmpty(fileName))
                {
                    // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
                    Path.GetFileName(fileName);
                }
            }
            catch (Exception)
            {
                throw new ArgumentException("Invalid file name for upload");
            }

            return $"{Guid.NewGuid().ToString().ToLower()}_{fileName}";
        }

        private static Exception GetConfigurationException(Exception innerException = null)
        {
            return new InvalidOperationException("Invalid configuration of file storage client", innerException);
        }

        private static Exception HandleStorageException(Exception exception)
        {
            var storageException = exception as StorageException;

            if (storageException?.RequestInformation.HttpStatusCode == 404)
            {
                return new NotFoundException();
            }

            return null;
        }

        private async Task EnsureContainerExists(CloudBlobContainer containerReference)
        {
            try
            {
                await containerReference.CreateIfNotExistsAsync();
#if NET452
                Log.Information("Storage container {ContainerName} was created", containerReference.Name);
#endif

#if NETSTANDARD2_0
                logger.LogInformation("Storage container {ContainerName} was created", containerReference.Name);
#endif
            }
            catch (StorageException ex)
            {
                // case when other thread created container beforehand
                if (ex.RequestInformation.HttpStatusCode != 409)
                {
#if NET452
                    Log.Error(ex, "Failed to create container for uploading a file into the storage");
#endif

#if NETSTANDARD2_0
                    logger.LogError(ex, "Failed to create container for uploading a file into the storage");
#endif
                    throw;
                }

#if NET452
                Log.Warning(ex, "Failed to create container for uploading a file into the storage");
#endif

#if NETSTANDARD2_0
                logger.LogWarning(ex, "Failed to create container for uploading a file into the storage");
#endif
            }
        }

        private string GetFileName(CloudBlockBlob blobReference)
        {
            // as we have old items without metadata
            return blobReference.Metadata.ContainsKey(FileNameMetadata)
                ? blobReference.Metadata[FileNameMetadata]
                : blobReference.Name;
        }

        private CloudBlobContainer GetBlobContainer(string containerName)
        {
            if (string.IsNullOrEmpty(containerName))
            {
                throw new ArgumentException("Container name should be set");
            }

            var blobClient = cloudStorageAccount.CreateCloudBlobClient();
            return blobClient.GetContainerReference(containerName.ToLower());
        }
    }
}
