using System.Text;
using Azure.Storage.Blobs;

namespace CloudCtrl.Kubernetes.Agent.Services;

public sealed class BlobStorageWriter
{
    public static async Task Write(string sasToken, string blob, string blobName)
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(blob), false);

        var blobClient = new BlobContainerClient(new Uri(sasToken));
        await blobClient.DeleteBlobIfExistsAsync(blobName);
        await blobClient.UploadBlobAsync(blobName, stream);
    }
}
