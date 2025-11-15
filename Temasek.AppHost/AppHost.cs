var builder = DistributedApplication.CreateBuilder(args);

var clerkPublishableKey = builder.AddParameter("clerk-publishable-key");
var clerkSecretKey = builder.AddParameter("clerk-secret-key", secret: true);
var formSgSecretKey = builder.AddParameter("formsg-secret-key", secret: true);
var formSgCallbackApiKey = builder.AddParameter("formsg-callback-api-key", secret: true);

var auth = builder.AddProject<Projects.Temasek_Auth>("temasek-auth")
    .WithEnvironment("FormSg:CallbackApiKey", formSgSecretKey)
    .WithEnvironment("FormSg:SecretKey", formSgSecretKey)
    .WithEnvironment("Clerk:SecretKey", clerkSecretKey);

builder.AddJavaScriptApp("temasek-auth-client", "../Temasek.Auth/Temasek.Auth.Client")
    .WithNpm(false)
    .WithEnvironment("NUXT_PUBLIC_CLERK_PUBLISHABLE_KEY", clerkPublishableKey)
    .WithEnvironment("NUXT_CLERK_SECRET_KEY", clerkSecretKey)
    .WithHttpEndpoint(port: 3000, env: "PORT")
    .WithReference(auth)
    .WaitFor(auth);

var operatorr = builder.AddProject<Projects.Temasek_Operatorr>("temasek-operatorr");
builder.AddJavaScriptApp("temasek-operatorr-client", "../Temasek.Operatorr/Temasek.Operatorr.Client")
    .WithNpm(false)
    .WithEnvironment("NUXT_PUBLIC_CLERK_PUBLISHABLE_KEY", clerkPublishableKey)
    .WithEnvironment("NUXT_CLERK_SECRET_KEY", clerkSecretKey)
    .WithHttpEndpoint(port: 3001, env: "PORT")
    .WithReference(operatorr)
    .WaitFor(operatorr);

builder.AddProject<Projects.Temasek_Calendarr>("temasek-calendarr");

builder.Build().Run();
