using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Ticketing.Web.Data;
using Xunit;

namespace Ticketing.Web.Tests;

public sealed class IdentityMigrationTests
{
    [Fact]
    public void EmptyDatabaseScript_ContainsIdentitySchema()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer("Server=localhost;Database=ScriptOnly;Integrated Security=True")
            .Options;
        using var context = new ApplicationDbContext(options);
        Assert.Contains("20261110090000_AddIdentity", context.Database.GetMigrations());
        string script = context.GetService<IMigrator>().GenerateScript();
        Assert.Contains("CREATE TABLE [AspNetUsers]", script);
        Assert.Contains("CREATE TABLE [AspNetRoles]", script);
        Assert.Contains("CREATE TABLE [AspNetUserRoles]", script);
    }
}
