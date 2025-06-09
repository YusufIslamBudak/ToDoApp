using MongoDB.Driver;
using StackExchange.Redis;
using Newtonsoft.Json;
using ToDoApp.Models;

namespace ToDoApp.Services
{
    public class TaskService
    {
        private readonly IMongoCollection<TaskItem> _taskCollection;
        private readonly IDatabase? _redisDb;

        public TaskService(IConfiguration config)
        {
            var mongoClient = new MongoClient(config["MongoDB:ConnectionString"]);
            var database = mongoClient.GetDatabase("ToDoDb");
            _taskCollection = database.GetCollection<TaskItem>("Tasks");

            try
            {
                var redis = ConnectionMultiplexer.Connect(config["Redis:ConnectionString"]);
                _redisDb = redis.GetDatabase();
                Console.WriteLine("✅ Redis bağlantısı başarılı.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("⚠️ Redis bağlantı hatası: " + ex.Message);
                _redisDb = null;
            }
        }

        public async Task<List<TaskItem>> GetAllAsync() =>
            await _taskCollection.Find(_ => true).ToListAsync();

        public async Task<TaskItem?> GetByIdAsync(string id) =>
            await _taskCollection.Find(t => t.Id == id).FirstOrDefaultAsync();

        public async Task CreateAsync(TaskItem task)
        {
            await _taskCollection.InsertOneAsync(task);
            await AddToRedisCache(task);
        }

        public async Task UpdateAsync(string id, TaskItem task) =>
            await _taskCollection.ReplaceOneAsync(t => t.Id == id, task);

        public async Task DeleteAsync(string id) =>
            await _taskCollection.DeleteOneAsync(t => t.Id == id);

        private async Task AddToRedisCache(TaskItem task)
        {
            if (_redisDb == null) return;

            try
            {
                string json = JsonConvert.SerializeObject(task);
                await _redisDb.ListLeftPushAsync("latestTasks", json);
                await _redisDb.ListTrimAsync("latestTasks", 0, 4);
            }
            catch (Exception ex)
            {
                Console.WriteLine("⚠️ Redis cache hatası: " + ex.Message);
            }
        }

        public async Task<List<TaskItem>> GetLatestFromRedisAsync()
        {
            if (_redisDb == null) return new List<TaskItem>();

            try
            {
                var values = await _redisDb.ListRangeAsync("latestTasks", 0, 4);
                return values
                    .Select(v => JsonConvert.DeserializeObject<TaskItem>(v!))
                    .Where(x => x != null)
                    .ToList()!;
            }
            catch
            {
                return new List<TaskItem>();
            }
        }
    }
}
