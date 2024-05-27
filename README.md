I created an xUnit Test Project.

*Not* the Aspire test project. With that, it has a host that will host your API from the POV of the apphost in your solution.

This means you can't use Alba because you can't get a reference to just the hosted application. This is good for "real" integration testing, I suppose,
but doesn't (easily) allow you to switch out Aspire components for other things (a different Postgres database, for example).

You also can't stub authentication, etc. 

I want to test the actual API (for example). So just an XUnit Test Project.

My sample API is just using Postgres, Marten.

Here is the `.csproj` for my test project:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <IsPackable>false</IsPackable>
	<IsAspireHost>true</IsAspireHost>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Alba" Version="7.4.1" />
    <PackageReference Include="Aspire.Hosting" Version="8.0.1" />
    <PackageReference Include="Aspire.Hosting.AppHost" Version="8.0.1" />
    <PackageReference Include="Aspire.Hosting.PostgreSQL" Version="8.0.1" />
    <PackageReference Include="coverlet.collector" Version="6.0.2">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.10.0" />
    <PackageReference Include="xunit" Version="2.8.1" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.8.1">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\Banking.Api\Banking.Api.csproj" IsAspireProjectResource="false"/>
  </ItemGroup>

  <ItemGroup>
    <Using Include="Xunit" />
  </ItemGroup>

</Project>
```

The two weird things are in the top PropertyGroup, the `<IsAspireHost>true</IsAspireHost>`, and in the project reference for my API
project, adding the `IsAspireProjectResource="false"` attribute.

Even with all that, I kind of got it to work, but we ended up with the same old problem of a race condition - the `await _app.StartAsync()` 
doesn't wait for the containers to start. I had to add a `await Task.Delay(1000)` as a fudge to get it to work.

