using Couchbase;
using Couchbase.KeyValue;
using Couchbase.Query;
using Couchbase.Core.Exceptions.KeyValue;
using Newtonsoft.Json;

namespace PayPledge.Services
{
    public class CouchbaseRepository<T> : IRepository<T> where T : class
    {
        private readonly ICouchbaseService _couchbaseService;
        private readonly ILogger<CouchbaseRepository<T>> _logger;
        private readonly IConfiguration _configuration;
        private readonly string _bucketName;

        public CouchbaseRepository(ICouchbaseService couchbaseService, ILogger<CouchbaseRepository<T>> logger, IConfiguration configuration)
        {
            _couchbaseService = couchbaseService;
            _logger = logger;
            _configuration = configuration;
            _bucketName = _configuration["Couchbase:BucketName"] ?? "pay_pledge";
        }

        public async Task<T?> GetByIdAsync(string id)
        {
            try
            {
                var collection = await _couchbaseService.GetCollectionAsync();
                var result = await collection.GetAsync(id);
                return result.ContentAs<T>();
            }
            catch (DocumentNotFoundException)
            {
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting document with id {Id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            try
            {
                var cluster = await _couchbaseService.GetClusterAsync();
                var typeName = typeof(T).Name.ToLower();
                var query = $"SELECT * FROM `{_bucketName}` WHERE type = '{typeName}'";

                var result = await cluster.QueryAsync<T>(query);
                return await result.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all documents of type {Type}", typeof(T).Name);
                throw;
            }
        }

        public async Task<IEnumerable<T>> FindAsync(string query, object? parameters = null)
        {
            try
            {
                var cluster = await _couchbaseService.GetClusterAsync();
                var options = new QueryOptions();

                if (parameters != null)
                {
                    var paramDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(
                        JsonConvert.SerializeObject(parameters));

                    if (paramDict != null)
                    {
                        foreach (var param in paramDict)
                        {
                            options.Parameter(param.Key, param.Value);
                        }
                    }
                }

                var result = await cluster.QueryAsync<T>(query, options);
                return await result.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing query: {Query}", query);
                throw;
            }
        }

        public async Task<string> CreateAsync(T entity)
        {
            try
            {
                var collection = await _couchbaseService.GetCollectionAsync();

                // Extract ID from entity using reflection or JSON
                var json = JsonConvert.SerializeObject(entity);
                var doc = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                var id = doc?["id"]?.ToString() ?? Guid.NewGuid().ToString();

                await collection.InsertAsync(id, entity);
                return id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating document of type {Type}", typeof(T).Name);
                throw;
            }
        }

        public async Task<bool> UpdateAsync(string id, T entity)
        {
            try
            {
                var collection = await _couchbaseService.GetCollectionAsync();
                await collection.ReplaceAsync(id, entity);
                return true;
            }
            catch (DocumentNotFoundException)
            {
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating document with id {Id}", id);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(string id)
        {
            try
            {
                var collection = await _couchbaseService.GetCollectionAsync();
                await collection.RemoveAsync(id);
                return true;
            }
            catch (DocumentNotFoundException)
            {
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting document with id {Id}", id);
                throw;
            }
        }

        public async Task<bool> ExistsAsync(string id)
        {
            try
            {
                var collection = await _couchbaseService.GetCollectionAsync();
                var result = await collection.ExistsAsync(id);
                return result.Exists;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking existence of document with id {Id}", id);
                throw;
            }
        }

        public async Task<long> CountAsync()
        {
            try
            {
                var cluster = await _couchbaseService.GetClusterAsync();
                var typeName = typeof(T).Name.ToLower();
                var query = $"SELECT COUNT(*) as count FROM `{_bucketName}` WHERE type = '{typeName}'";

                var result = await cluster.QueryAsync<dynamic>(query);
                var rows = await result.ToListAsync();
                return rows.FirstOrDefault()?.count ?? 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error counting documents of type {Type}", typeof(T).Name);
                throw;
            }
        }
    }
}
