using Infrastructure;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;

namespace OrderFlowII.Tests;

public abstract class SqliteTestBase : IDisposable
{
    private readonly SqliteConnection _connection;
    protected readonly OrderDbContext Context; 

    protected SqliteTestBase()
    {
        // :memory: يعني DB في الرامات، بتتمسح لما الـ connection تتقفل
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open(); // لازم تفضل مفتوحة طول الـ test

        var options = new DbContextOptionsBuilder<OrderDbContext>()
            .UseSqlite(_connection)
            .Options;  

        Context = new OrderDbContext(options);
        Context.Database.EnsureCreated(); // بيعمل الجداول من الـ entities
    }

    public void Dispose()
    {
        Context.Dispose();
        _connection.Dispose();
    }
}