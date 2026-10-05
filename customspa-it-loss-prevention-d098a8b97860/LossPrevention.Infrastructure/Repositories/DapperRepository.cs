//using Dapper;
//using System.Data;
//using System.Data.Common;

//namespace CRM.Infrastructure.Repositories
//{
//    public class DapperRepository<TEntity, TKey> : IDapperRepository<TEntity, TKey> where TEntity : class
//    {
//        private readonly string _connectionString;
//        private readonly string _tableName;
//        private readonly string _primaryKeyName;
//        private readonly IDbConnection _connection;

//        public DapperRepository(IDbConnection connection, string tableName, string primaryKeyName = "Id")
//        {
//            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
//            _connectionString = connection.ConnectionString;
//            _tableName = tableName;
//            _primaryKeyName = primaryKeyName;
//        }

//        public async Task<TEntity> FindAsync(TKey id, string idfield)
//        {
//            OpenConnection();
//            return await _connection.QueryFirstOrDefaultAsync<TEntity>($"SELECT * FROM {_tableName} WHERE {idfield} = @Id", new { Id = id });
//        }

//        public async Task<IEnumerable<TEntity>> FindByFieldAsync(string fieldName, object value)
//        {
//            OpenConnection();
//            return await _connection.QueryAsync<TEntity>($"SELECT * FROM {_tableName} WHERE {fieldName} = @Value", new { Value = value });
//        }

//        public async Task<IEnumerable<TEntity>> FindAllAsync(TKey id, string idfield)
//        {
//            OpenConnection();
//            return await _connection.QueryAsync<TEntity>($"SELECT * FROM {_tableName} WHERE {idfield} = @Id", new { Id = id });
//        }

//        public async Task<IEnumerable<TEntity>> GetAllAsync()
//        {
//            OpenConnection();
//            return await _connection.QueryAsync<TEntity>($"SELECT * FROM {_tableName}");
//        }

//        public async Task InsertAsync(TEntity entity)
//        {
//            OpenConnection();
//            if (_connection is Microsoft.Data.SqlClient.SqlConnection)
//            {
//                var columns = GetColumnNames(entity, excludePrimaryKey: true);
//                var parameters = string.Join(", ", columns.Select(c => $"@{c}"));
//                string sql = $"INSERT INTO {_tableName} ({string.Join(", ", columns)}) VALUES ({parameters}); SELECT CAST(SCOPE_IDENTITY() as int)";
//                var id = await _connection.ExecuteScalarAsync<int>(sql, entity);
//                SetPrimaryKey(entity, id);
//            }
//            else if (_connection is System.Data.SQLite.SQLiteConnection)
//            {
//                //TODO: Fix removal of forign keys. i.e customerId
//                string sql;
//                var columnNames = GetColumnNames(entity, excludePrimaryKey: true);
//                if (columnNames.Any())
//                {
//                    sql = $"INSERT INTO {_tableName} ({string.Join(", ", columnNames)}) VALUES (@{string.Join(", @", columnNames)}); SELECT last_insert_rowid()";
//                }
//                else
//                {
//                    sql = $"INSERT INTO {_tableName}  DEFAULT VALUES; SELECT last_insert_rowid()";
//                }
//                var id = await _connection.ExecuteScalarAsync<long>(sql, entity);
//                SetPrimaryKey(entity, id);
//            }
//            else
//            {
//                throw new NotSupportedException("Identity/Autoincrement retrieval not supported for this database type.");
//            }

//        }

//        private void SetPrimaryKey(TEntity entity, object id)
//        {
//            if (id != null && typeof(TKey) != typeof(void))
//            {
//                var idProperty = entity.GetType().GetProperty(_primaryKeyName);
//                if (idProperty != null && idProperty.CanWrite)
//                {
//                    try
//                    {
//                        idProperty.SetValue(entity, Convert.ChangeType(id, typeof(TKey)));
//                    }
//                    catch (Exception ex)
//                    {
//                        System.Diagnostics.Debug.WriteLine($"Error setting primary key: {ex.Message}");
//                    }
//                }
//            }
//        }

//        public async Task UpdateAsync(TEntity entity)
//        {
//            OpenConnection();
//            var columns = GetColumnNames(entity, excludePrimaryKey: false);
//            var setClauses = string.Join(", ", columns.Select(c => $"{c} = @{c}"));
//            var sql = $"UPDATE {_tableName} SET {setClauses} WHERE {_primaryKeyName} = @{_primaryKeyName}";
//            await _connection.ExecuteAsync(sql, entity);
//            // No DisposeConnection() here
//        }

//        public async Task UpsertAsync(TEntity entity)
//        {
//            OpenConnection();
//            var primaryKeyValue = entity.GetType().GetProperty(_primaryKeyName)?.GetValue(entity);
//            if (primaryKeyValue == null || (primaryKeyValue is int intId && intId.Equals(default(int))) || (primaryKeyValue is long longId && longId.Equals(default(long))))
//            {
//                await InsertAsync(entity);
//            }
//            else
//            {
//                var existing = await FindAsync((TKey)primaryKeyValue, _primaryKeyName);
//                if (existing == null)
//                {
//                    await InsertAsync(entity);
//                }
//                else
//                {
//                    await UpdateAsync(entity);
//                }
//            }
//            // No DisposeConnection() here
//        }

//        public async Task<int> DeleteAsync(TKey id)
//        {
//            OpenConnection();
//            return await _connection.ExecuteAsync($"DELETE FROM {_tableName} WHERE {_primaryKeyName} = @Id", new { Id = id });
//        }

//        private IEnumerable<string> GetColumnNames(TEntity entity, bool excludePrimaryKey = false)
//        {
//            var properties = entity.GetType().GetProperties();
//            var columns = new List<string>();

//            foreach (var property in properties)
//            {
//                if (excludePrimaryKey && property.Name.Equals(_primaryKeyName, StringComparison.OrdinalIgnoreCase))
//                {
//                    continue;
//                }

//                Type propertyType = property.PropertyType;
//                Type underlyingType = Nullable.GetUnderlyingType(propertyType);
//                Type effectiveType = underlyingType ?? propertyType;

//                if (effectiveType.IsPrimitive ||
//                    effectiveType == typeof(string) ||
//                    effectiveType == typeof(DateTime) ||
//                    effectiveType == typeof(decimal) ||
//                    effectiveType == typeof(Guid) ||
//                    effectiveType.IsEnum)
//                {
//                    columns.Add(property.Name);
//                }
//            }

//            return columns;
//        }

//        private void OpenConnection()
//        {
//            if (_connection.State != ConnectionState.Open)
//            {
//                _connection.Open();
//            }
//        }

//        public async Task<IEnumerable<TJoin>> GetJoinEntitiesAsync<TJoin>(string joinTableName, string leftIdName, TKey leftIdValue, string rightIdName, string rightEntityType)
//        {
//            OpenConnection();
//            var sql = $@"
//                SELECT r.*
//                FROM {rightEntityType} r
//                INNER JOIN {joinTableName} j ON r.{rightIdName} = j.{rightIdName}
//                WHERE j.{leftIdName} = @LeftIdValue;";
//            return await _connection.QueryAsync<TJoin>(sql, new { LeftIdValue = leftIdValue });
//        }

//        public async Task InsertJoinEntityAsync(string joinTableName, string leftIdName, TKey leftIdValue, string rightIdName, object rightIdValue)
//        {
//            OpenConnection();
//            var sql = $@"
//                INSERT INTO {joinTableName} ({leftIdName}, {rightIdName})
//                VALUES (@LeftIdValue, @RightIdValue);";
//            await _connection.ExecuteAsync(sql, new { LeftIdValue = leftIdValue, RightIdValue = rightIdValue });
//        }

//        public async Task<int> DeleteJoinEntityAsync(string joinTableName, string leftIdName, TKey leftIdValue, string rightIdName, object rightIdValue)
//        {
//            OpenConnection();
//            var sql = $@"
//                DELETE FROM {joinTableName}
//                WHERE {leftIdName} = @LeftIdValue AND {rightIdName} = @RightIdValue;";
//            return await _connection.ExecuteAsync(sql, new { LeftIdValue = leftIdValue, RightIdValue = rightIdValue });
//        }
//    }
//}