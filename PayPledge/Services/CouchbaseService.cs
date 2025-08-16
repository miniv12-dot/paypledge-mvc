using Couchbase;
using Couchbase.KeyValue;

namespace PayPledge.Services
{
    public interface ICouchbaseService
    {
        Task<ICluster> GetClusterAsync();
        Task<IBucket> GetBucketAsync();
        Task<ICouchbaseCollection> GetCollectionAsync(string? scopeName = null, string? collectionName = null);
    }

    public class CouchbaseService : ICouchbaseService
    {
        private readonly ICluster _cluster;
        private readonly IConfiguration _configuration;
        private readonly string _bucketName;

        public CouchbaseService(ICluster cluster, IConfiguration configuration)
        {
            _cluster = cluster;
            _configuration = configuration;
            _bucketName = _configuration["Couchbase:BucketName"] ?? "paypledge";
        }

        public async Task<ICluster> GetClusterAsync()
        {
            return _cluster;
        }

        public async Task<IBucket> GetBucketAsync()
        {
            return await _cluster.BucketAsync(_bucketName);
        }

        public async Task<ICouchbaseCollection> GetCollectionAsync(string? scopeName = null, string? collectionName = null)
        {
            var bucket = await GetBucketAsync();
            var scope = string.IsNullOrEmpty(scopeName) ? bucket.DefaultScope() : bucket.Scope(scopeName);
            return string.IsNullOrEmpty(collectionName) ? scope.Collection("_default") : scope.Collection(collectionName);
        }
    }
}
