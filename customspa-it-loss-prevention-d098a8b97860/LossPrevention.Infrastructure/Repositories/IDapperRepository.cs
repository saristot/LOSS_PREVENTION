
//using System.Security.Cryptography;
//using static Dapper.SqlMapper;

//namespace CRM.Infrastructure.Repositories
//{
//    public interface IDapperRepository<TEntity, TKey> where TEntity : class
//    {
//        public Task<TEntity> FindAsync(TKey id, string idField);
//        public Task<IEnumerable<TEntity>> FindByFieldAsync(string fieldName, object value);
//        public Task<IEnumerable<TEntity>> FindAllAsync(TKey id, string idfield);
//        public Task<IEnumerable<TEntity>> GetAllAsync();
//        public Task InsertAsync(TEntity entity);
//        public Task UpdateAsync(TEntity entity);
//        public Task UpsertAsync(TEntity entity);
//        public Task<int> DeleteAsync(TKey id);

//        public Task<IEnumerable<TJoin>> GetJoinEntitiesAsync<TJoin>(string joinTableName, string leftIdName, TKey leftIdValue, string rightIdName, string rightEntityType);
//        public Task InsertJoinEntityAsync(string joinTableName, string leftIdName, TKey leftIdValue, string rightIdName, object rightIdValue);
//        public Task<int> DeleteJoinEntityAsync(string joinTableName, string leftIdName, TKey leftIdValue, string rightIdName, object rightIdValue);
//    }
//}