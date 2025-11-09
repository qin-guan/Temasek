var builder = DistributedApplication.CreateBuilder(args);

var auth = builder.AddProject<Projects.Temasek_Auth>("temasek-auth");
builder.AddNpmApp("temasek-auth-client", "../Temasek.Auth//Temasek.Auth.Client", "dev")
    .WithReference(auth)
    .WaitFor(auth)
    .WithHttpEndpoint(env:"PORT")
    .WithExternalHttpEndpoints();

var operatorr = builder.AddProject<Projects.Temasek_Operatorr>("temasek-operatorr");
builder.AddNpmApp("temasek-operatorr-client", "../Temasek.Operatorr/Temasek.Operatorr.Client", "dev")
    .WithReference(operatorr)
    .WaitFor(operatorr)
    .WithHttpEndpoint(env: "PORT")
    .WithExternalHttpEndpoints();

builder.AddProject<Projects.Temasek_Calendarr>("temasek-calendarr");

builder.Build().Run();
