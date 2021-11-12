namespace NEventStore.Persistence.Sql.SqlDialects
{
    using System;
    using System.Data;

    public class PostgreSqlDialect : CommonSqlDialect
    {
        public override string InitializeStorage
        {
            get { return PostgreSqlStatements.InitializeStorage; }
        }

        public override string PersistCommit
        {
            get { return PostgreSqlStatements.PersistCommits; }
        }

        public override bool IsDuplicate(Exception exception)
        {
            string message = exception.Message.ToUpperInvariant();
            return message.Contains("23505") || message.Contains("IX_COMMITS_COMMITSEQUENCE");
        }
    }

    public class PostgreNpgsql6Dialect : CommonSqlDialect
    {
        private readonly bool _npgsql6Timestamp;

        /// <summary>
        /// Create an instance of the PostgreSQL dialect
        /// </summary>
        /// <param name="npgsql6timestamp">
        /// There's a breaking change in Npgsql Version 6.x that changes the way timestamps should be persisted.
        /// - false: to disable the new behavior for the whole application (see the driver release notes) this should be done before using the database.
        /// - true: enable the new behavior, you might need to migrate the data manually (see Npgsql release notes).
        /// </param>
        public PostgreNpgsql6Dialect(
            bool npgsql6timestamp = true
            )
        {
            _npgsql6Timestamp = npgsql6timestamp;
            if (!_npgsql6Timestamp)
            {
                AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
            }
        }

        public override string InitializeStorage
        {
            get
            {
                var initStorage = PostgreSqlStatements.InitializeStorage;
                if (_npgsql6Timestamp)
                {
                    initStorage = initStorage.Replace("timestamp", "timestamptz");
                }
                return initStorage;
            }
        }

        public override string PersistCommit
        {
            get { return PostgreSqlStatements.PersistCommits; }
        }

        public override bool IsDuplicate(Exception exception)
        {
            string message = exception.Message.ToUpperInvariant();
            return message.Contains("23505") || message.Contains("IX_COMMITS_COMMITSEQUENCE");
        }

        public override DbType GetDateTimeDbType()
        {
            if (_npgsql6Timestamp)
            {
                return DbType.DateTimeOffset;
            }
            return base.GetDateTimeDbType();
        }
    }
}