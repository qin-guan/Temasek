var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.Temasek_Operatorr>("temasek-operatorr");

builder.AddProject<Projects.Temasek_Auth>("temasek-auth");

builder.AddProject<Projects.Temasek_Calendarr>("temasek-calendarr");

builder.Build().Run();
