using System;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace DevOps.Tests;

public class ClientTests
{
    [Fact]
    public void PersonalAccessTokenMustNotBeNull()
    {
        var ex = Assert.Throws<ArgumentNullException>(() => new Client("default", null));
        Assert.Equal("personalAccessToken", ex.ParamName);
    }

    [Fact]
    public void PersonalAccessTokenMustNotBeEmpty()
    {
        var ex = Assert.Throws<ArgumentNullException>(() => new Client("default", string.Empty));
        Assert.Equal("personalAccessToken", ex.ParamName);
    }

    [Fact]
    public void PersonalAccessTokenMustNotBeWhitespace()
    {
        var ex = Assert.Throws<ArgumentNullException>(() => new Client("default", " "));
        Assert.Equal("personalAccessToken", ex.ParamName);
    }

    [Fact]
    public void OrganizationMustNotBeNull()
    {
        var ex = Assert.Throws<ArgumentNullException>(() => new Client(null, "default"));
        Assert.Equal("organization", ex.ParamName);
    }

    [Fact]
    public void OrganizationMustNotBeEmpty()
    {
        var ex = Assert.Throws<ArgumentNullException>(() => new Client(string.Empty, "default"));
        Assert.Equal("organization", ex.ParamName);
    }

    [Fact]
    public void OrganizationMustNotBeWhitespace()
    {
        var ex = Assert.Throws<ArgumentNullException>(() => new Client(" ", "default"));
        Assert.Equal("organization", ex.ParamName);
    }
}