using Microsoft.EntityFrameworkCore;

namespace ScreenTimeTracker.BuildingBlocks.Persistence;

public abstract class ModuleDbContext(DbContextOptions options, string schema) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 遍历所有实体，统一添加表前缀
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            if (entity.IsOwned())
                continue;
            // 获取原始表名
            var currentTableName = entity.GetTableName();
            // 设置新表名：前缀_原始表名
            if (!string.IsNullOrEmpty(currentTableName))
                entity.SetTableName($"{schema}_{currentTableName}");
        }
    }
}
