var builder = DistributedApplication.CreateBuilder(args);

var auth = builder.AddProject<Projects.Temasek_Auth>("temasek-auth");
builder.AddJavaScriptApp("temasek-auth-client", "../Temasek.Auth/Temasek.Auth.Client")
    .WithNpm(false)
    .WithHttpEndpoint(port: 3000, env: "PORT")
    .WithReference(auth)
    .WaitFor(auth);

var operatorr = builder.AddProject<Projects.Temasek_Operatorr>("temasek-operatorr");
builder.AddJavaScriptApp("temasek-operatorr-client", "../Temasek.Operatorr/Temasek.Operatorr.Client")
    .WithNpm(false)
    .WithHttpEndpoint(port: 3001, env: "PORT")
    .WithReference(operatorr)
    .WaitFor(operatorr);

builder.AddProject<Projects.Temasek_Calendarr>("temasek-calendarr");

builder.Build().Run();
