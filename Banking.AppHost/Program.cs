var builder = DistributedApplication.CreateBuilder(args);


var pg = builder.AddPostgres("banking")
    .WithPgAdmin()
    .WithOtlpExporter();


    

var bankingApi = builder.AddProject<Projects.Banking_Api>("api")
    .WithReference(pg);

builder.Build().Run();
