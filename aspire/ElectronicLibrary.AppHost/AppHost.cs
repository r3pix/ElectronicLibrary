var builder = DistributedApplication.CreateBuilder(args);

var storage = builder.AddAzureStorage("storage")
    .RunAsEmulator();

var blobs = storage.AddBlobs("blobs");

var sql = builder.AddSqlServer("sql")
    .WithDataVolume()
    .AddDatabase("sqldb");

var api = builder.AddProject<Projects.ElectronicLibrary_Api>("api")
    .WithReference(blobs)
    .WithReference(sql)
    .WaitFor(sql);

var functions = builder.AddAzureFunctionsProject<Projects.ElectronicLibrary_Functions>("functions")
    .WithReference(blobs)
    .WithReference(sql)
    .WaitFor(sql);

builder.Build().Run();
