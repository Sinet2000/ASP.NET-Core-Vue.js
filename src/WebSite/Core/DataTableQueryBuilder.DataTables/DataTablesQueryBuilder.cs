using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace DataTableQueryBuilder.DataTables
{
    /// <summary>
    /// Builds a query according to the specific DataTable request.
    /// </summary>
    /// <typeparam name="TDataTableFields">A view model that represents the list of DataTable columns.</typeparam>
    /// <typeparam name="TEntity">The type of the data in the data source.</typeparam>
    public class DataTablesQueryBuilder<TDataTableFields, TEntity> : QueryBuilder<TDataTableFields, TEntity>
    {
        /// <summary>
        /// Creates a new query builder to be used for specific DataTable request.
        /// </summary>
        /// <param name="request">DataTables request.</param>
        public DataTablesQueryBuilder(DataTablesRequest request) : base(request)
        { }

        /// <summary>
        /// Creates a new query builder to be used on specific DataTable request.
        /// </summary>
        /// <param name="request">DataTables request.</param>
        /// <param name="optionsAction">An action to configure the QueryBuilderOptions.</param>
        public DataTablesQueryBuilder(DataTablesRequest request, Action<QueryBuilderOptions<TDataTableFields, TEntity>> optionsAction) : base(request, optionsAction)
        { }

        protected override BuildResult<TDataTableFields, TEntity> CreateBuildResult(int totalRecords, int totalRecordsFiltered, IQueryable<TEntity> buildedQuery)
        {
            return new DataTablesBuildResult<TDataTableFields, TEntity>(totalRecords, totalRecordsFiltered, buildedQuery, (DataTablesRequest)request);
        }
    }
}