namespace DAL.ProcureAccess.Initialization;

public static class SampleDataInitializer
{
    #region public
    public static async Task InitializeData(ApplicationDBContext dbContext, UserManager<User> userManager)
    {
        await SeedData(dbContext, userManager);
    }

    public static async Task ClearAndReseedDatabase(ApplicationDBContext dbContext, UserManager<User> userManager)
    {
        await ClearData(dbContext, userManager);
        await SeedData(dbContext, userManager);
    }
    #endregion

    internal static async Task ClearData(ApplicationDBContext dbContext, UserManager<User> userManager)
    {
        var entities = new[]
        {
            // typeof(SomeEntity).FullName,
        };
        ServiceCollection serviceCollection = new ServiceCollection();

        //serviceCollection.AddDbContextDesignTimeServices(dbContext);
        new EntityFrameworkRelationalServicesBuilder(serviceCollection)
            .TryAdd(dbContext.GetService<IDatabaseProvider>())
            .TryAdd(_ => dbContext.GetService<IMigrationsIdGenerator>())
            .TryAdd(_ => dbContext.GetService<IRelationalTypeMappingSource>())
            .TryAdd(_ => dbContext.GetService<IModelRuntimeInitializer>())
            .TryAdd(_ => dbContext.GetService<LoggingDefinitions>())
            .TryAdd(_ => dbContext.GetService<ICurrentDbContext>())
            .TryAdd(_ => dbContext.GetService<IDbContextOptions>())
            .TryAdd(_ => dbContext.GetService<IHistoryRepository>())
            .TryAdd(_ => dbContext.GetService<IMigrationsAssembly>())
            .TryAdd(_ => dbContext.GetService<IMigrationsModelDiffer>())
            .TryAdd(_ => dbContext.GetService<IMigrator>())
            .TryAdd(_ => dbContext.GetService<IDesignTimeModel>().Model);

        var serviceProvider = serviceCollection.BuildServiceProvider();
        var designTimeModel = serviceProvider.GetService<IModel>();
        foreach (var entityName in entities)
        {
            IEntityType? entity = dbContext.Model.FindEntityType(entityName);
            string? tableName = entity.GetTableName();
            string? schemaName = entity.GetSchema();
            dbContext.Database.ExecuteSqlRaw($"DELETE FROM {schemaName}.{tableName}");
            dbContext.Database.ExecuteSqlRaw($"DBCC CHECKIDENT (\"{schemaName}.{tableName}\", RESEED, 1);");
            if (entity.IsTemporal())
            {
                IExecutionStrategy strategy = dbContext.Database.CreateExecutionStrategy();
                strategy.Execute(() =>
                {
                    using var trans = dbContext.Database.BeginTransaction();
                    IEntityType? designTimeEntity = designTimeModel.FindEntityType(entityName);
                    string? historySchema = designTimeEntity.GetHistoryTableSchema();
                    string? historyTable = designTimeEntity.GetHistoryTableName();
                    dbContext.Database.ExecuteSqlRaw($"ALTER TABLE {schemaName}.{tableName} SET (SYSTEM_VERSIONING = OFF)");
                    dbContext.Database.ExecuteSqlRaw($"DELETE FROM {historySchema}.{historyTable}");
                    dbContext.Database.ExecuteSqlRaw(
                        $"ALTER TABLE {schemaName}.{tableName} SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE={historySchema}.{historyTable}))");
                    trans.Commit();
                });
            }
        }

        foreach (User user in userManager.Users)
        {
            await userManager.DeleteAsync(user);
        }
    }

    internal static async Task SeedData(ApplicationDBContext dbContext, UserManager<User> userManager)
    {
        try
        {
            var succeeded = await ProcessUserInsert(dbContext, userManager, SampleData.Users);
            //ProcessInsert(dbContext, dbContext.SomeEntity, SampleData.SomeEntities(userManager.Users.First()));
            // insert more Entities...
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            throw;
        }

        static void ProcessInsert<TEntity>(
            ApplicationDBContext dbContext,
            DbSet<TEntity> table,
            List<TEntity> records) where TEntity : BaseEntity
        {
            if (table.Any()) return;

            IExecutionStrategy strategy = dbContext.Database.CreateExecutionStrategy();
            strategy.Execute(() =>
            {
                using var transaction = dbContext.Database.BeginTransaction();
                try
                {
                    var metaData = dbContext.Model.FindEntityType(typeof(TEntity).FullName);
                    dbContext.Database.ExecuteSqlRaw($"SET IDENTITY_INSERT {metaData.GetSchema()}.{metaData.GetTableName()} ON");
                    table.AddRange(records);
                    dbContext.SaveChanges();
                    dbContext.Database.ExecuteSqlRaw($"SET IDENTITY_INSERT {metaData.GetSchema()}.{metaData.GetTableName()} OFF");
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                }
            });
        }

        static async Task<bool> ProcessUserInsert(
            ApplicationDBContext dbContext,
            UserManager<User> userManager,
            List<User> users)
        {
            var allSucceeded = true;
            foreach (User user in users)
            {
                var result = await userManager.CreateAsync(user);
                if (!result.Succeeded)
                {
                    allSucceeded = false;
                    break;
                }
            }
            return allSucceeded;
        }
    }
}
