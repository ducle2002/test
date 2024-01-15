using Abp.Dependency;
using Abp.EntityFrameworkCore;
using Yootek.ApbCore.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yootek.EntityFrameworkCore
{


    public class SqlExecuter : ISqlExecuter, ITransientDependency
    {
        private readonly IDbContextProvider<YootekDbContext> _dbContextProvider;
        public SqlExecuter(IDbContextProvider<YootekDbContext> dbContextProvider)
        {
            _dbContextProvider = dbContextProvider;
        }

        public int Execute(string query, params object[] parameters)
        {
            return _dbContextProvider.GetDbContext().Database.ExecuteSqlRaw(query, parameters);
        }
        public Task<int> ExecuteAsync(string query)
        {
            return _dbContextProvider.GetDbContext().Database.ExecuteSqlRawAsync(query);
        }

        //public List<K> Query<K>(string query, params object[] parameters)
        //{
        //    return _dbContextProvider.GetDbContext().Database.SqlQuery<K>(query, parameters).ToList();
        //}

        //public K QueryFirst<K>(string query, params object[] parameters)
        //{
        //    return _dbContextProvider.GetDbContext().Database.SqlQuery<K>(query, parameters).FirstOrDefault();
        //}

        //public Task<List<K>> QueryAsync<K>(string query, params object[] parameters)
        //{
        //    return _dbContextProvider.GetDbContext().Database.SqlQuery<K>(query, parameters).ToListAsync();
        //}

        //public Task<K> QueryFirstAsync<K>(string query, params object[] parameters)
        //{
        //    return _dbContextProvider.GetDbContext().Database.SqlQuery<K>(query, parameters).FirstOrDefaultAsync();
        //}

        //public K GetNextSeqVal<K>(string sequenceName)
        //{
        //    string query = $"SELECT {sequenceName}.NEXTVAL FROM DUAL";
        //    return QueryFirst<K>(query, new OracleParameter[] { });
        //}
    }
}
