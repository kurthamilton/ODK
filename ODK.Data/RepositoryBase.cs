using System;
using System.Data.SqlClient;
using ODK.Data.Sql;

namespace ODK.Data
{
    public abstract class RepositoryBase
    {
        protected RepositoryBase(SqlContext context)
        {
            Context = context;
        }

        protected SqlContext Context { get; }

        protected SqlConnection OpenConnection()
        {
            throw new NotImplementedException();
        }

        protected void TryCommitTransaction(SqlTransaction transaction)
        {
            try
            {
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
    }
}
