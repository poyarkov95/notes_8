using Dapper;
using Postgres.Attributes;
using Postgres.Filter;

namespace Postgres.Repository.Implementation;

public class BaseRepository
    {
        private readonly DapperContext _context;

        public BaseRepository(DapperContext context)
        {
            _context = context;
        }

        protected async Task<IEnumerable<T>> ExecuteSelect<T>(string query, DynamicParameters parameters)
        {
            using (var connection = _context.CreateConnection())
            {
                return await connection.QueryAsync<T>(query, parameters);
            }
        }
        
        protected async Task<T> ExecuteSingle<T>(string query, DynamicParameters parameters)
        {
            using (var connection = _context.CreateConnection())
            {
                return await connection.QuerySingleAsync<T>(PrepareSelect<T>(query), parameters);
            }
        }

        protected async Task<IEnumerable<T>> ExecuteSelect<T>( string tableName, BaseFilterModel filter)
        {
            using (var connection = _context.CreateConnection())
            {
                return await connection.QueryAsync<T>(PrepareSelect<T>(tableName, filter));
            }
        }

        protected async Task ExecuteUpdate<T>(string tableName, T entity)
        {
            var set = new List<string>();
            var where = new List<string>();

            var parameters = new DynamicParameters();
            
            var props = typeof(T)
                .GetProperties()
                .Where(s => Attribute.IsDefined(s, typeof(ColumnName)));
            
            var isPrimaryKey = false;
            
            foreach (var property in props)
            {
                foreach (var attribute in property.GetCustomAttributes(true))
                {
                    var primaryKeyAttribute = attribute as PrimaryKey;
                    if (primaryKeyAttribute != null)
                    {
                        isPrimaryKey = true;
                    }
                    if (attribute is ColumnName columnNameAttribute)
                    {
                        if (isPrimaryKey)
                        {
                            isPrimaryKey = false;
                            where.Add($"{columnNameAttribute.Name} = @{columnNameAttribute.Name}");
                            parameters.Add($"@{columnNameAttribute.Name}", property.GetValue(entity));
                            continue;
                        }

                        set.Add($"{columnNameAttribute.Name} = @{columnNameAttribute.Name}");
                        parameters.Add($"@{columnNameAttribute.Name}", property.GetValue(entity));
                    }
                }
            }
            
            var query = $@"update {tableName}
                                 set {string.Join(", ", set)}
                                 where {string.Join(" and ", where)}";
            
            using (var connection = _context.CreateConnection())
            {
                await connection.ExecuteAsync(query, parameters);
            }
        }

        protected async Task<R> ExecuteInsert<T,R>(string tableName, T entity)
        {
            var columnNames = new List<string>();
            var paramNames = new List<string>();

            var returningSection = string.Empty;
            
            var parameters = new DynamicParameters();
            
            var props = typeof(T)
                .GetProperties()
                .Where(s => Attribute.IsDefined(s, typeof(ColumnName)));

            var isPrimaryKey = false;
            
            foreach (var property in props)
            {
                foreach (var attribute in property.GetCustomAttributes(true))
                {
                    var primaryKeyAttribute = attribute as PrimaryKey;
                    if (primaryKeyAttribute != null)
                    {
                        isPrimaryKey = true;
                    }
                    if (attribute is ColumnName columnNameAttribute)
                    {
                        if (isPrimaryKey)
                        {
                            returningSection = $"returning {columnNameAttribute.Name}";
                            isPrimaryKey = false;
                            continue;
                        }

                        columnNames.Add(columnNameAttribute.Name);
                        paramNames.Add($"@{columnNameAttribute.Name}");
                        parameters.Add($"@{columnNameAttribute.Name}", property.GetValue(entity));
                    }
                }
            }

            var query =
                $@"insert into {tableName}
                 ({string.Join(" , ", columnNames)})
                 values
                 ({string.Join(" , ", paramNames)}) {returningSection}";
            
            using (var connection = _context.CreateConnection())
            {
                return await connection.ExecuteScalarAsync<R>(query, parameters);
            }
        }

        protected string PrepareSkip(BaseFilterModel filter)
        {
            return $"offset {filter.Skip} fetch next {filter.Take} rows only";
        }

        private string PrepareSelect<T>(string tableName, BaseFilterModel filter)
        {
            var props = typeof(T)
                .GetProperties()
                .Where(s => Attribute.IsDefined(s, typeof(ColumnName)));
            
            var columnNames = new List<string>();

            foreach (var property in props)
            {
                foreach (var attribute in property.GetCustomAttributes(true))
                {
                    var columnNameAttribute = attribute as ColumnName;
                    if (columnNameAttribute != null)
                    {
                        columnNames.Add(columnNameAttribute.Name);
                    }
                }
            }

            var query = $@"select
                      {string.Join(" , ", columnNames)}
                      from {tableName} 
                      {PrepareSkip(filter)}";

            return query;
        }
        
        private string PrepareSelect<T>(string query)
        {
            var props = typeof(T)
                .GetProperties()
                .Where(s => Attribute.IsDefined(s, typeof(ColumnName)));
            
            var columnNames = new List<string>();

            foreach (var property in props)
            {
                foreach (var attribute in property.GetCustomAttributes(true))
                {
                    var columnNameAttribute = attribute as ColumnName;
                    if (columnNameAttribute != null)
                    {
                        columnNames.Add($"{columnNameAttribute.Name} as {property.Name}");
                    }
                }
            }

            var select = $@"select
                      {string.Join(" , ", columnNames)}
                      from ({query}) T ";

            return select;
        }
    }