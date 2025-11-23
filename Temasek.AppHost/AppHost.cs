var builder = DistributedApplication.CreateBuilder(args);

var clerkPublishableKey = builder.AddParameter("clerk-publishable-key");
var clerkSecretKey = builder.AddParameter("clerk-secret-key", secret: true);
var formSgSecretKey = builder.AddParameter("formsg-secret-key", secret: true);
var formSgCallbackApiKey = builder.AddParameter("formsg-callback-api-key", secret: true);

var auth = builder.AddProject<Projects.Temasek_Auth>("temasek-auth")
    .WithEnvironment("FormSg:CallbackApiKey", formSgCallbackApiKey)
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

var calendarrServiceAccountJsonCredential = builder.AddParameter("calendarr-service-account-json-credential", secret: true);
var calendarrParentCalendarId = builder.AddParameter("calendarr-parent-calendar-id");
var calendarrChildCalendarId = builder.AddParameter("calendarr-child-calendar-id");
var calendarrSyncInterval = builder.AddParameter("calendarr-sync-interval", "1:0:0");
var calendarrBdeComdSourceCalendarId = builder.AddParameter("calendarr-bde-comd-source-calendar-id");
var calendarrBdeComdTargetCalendarId = builder.AddParameter("calendarr-bde-comd-target-calendar-id");
var calendarrBdeComdSyncInterval = builder.AddParameter("calendarr-bde-comd-sync-interval", "1:0:0");
var calendarr = builder.AddProject<Projects.Temasek_Calendarr>("temasek-calendarr")
    .WithEnvironment("Sync:ServiceAccountJsonCredential", calendarrServiceAccountJsonCredential)
    .WithEnvironment("Sync:ParentCalendarId", calendarrParentCalendarId)
    .WithEnvironment("Sync:ChildCalendarId", calendarrChildCalendarId)
    .WithEnvironment("Sync:SyncInterval", calendarrSyncInterval)
    .WithEnvironment("BdeComd:SourceCalendarId", calendarrBdeComdSourceCalendarId)
    .WithEnvironment("BdeComd:TargetCalendarId", calendarrBdeComdTargetCalendarId)
    .WithEnvironment("BdeComd:SyncInterval", calendarrBdeComdSyncInterval);

builder.AddJavaScriptApp("temasek-calendarr-client", "../Temasek.Calendarr/Temasek.Calendarr.Client")
    .WithNpm(false)
    .WithEnvironment("NUXT_PUBLIC_CLERK_PUBLISHABLE_KEY", clerkPublishableKey)
    .WithEnvironment("NUXT_CLERK_SECRET_KEY", clerkSecretKey)
    .WithHttpEndpoint(port: 3002, env: "PORT")
    .WithReference(calendarr)
    .WaitFor(calendarr);

var facilities = builder.AddProject<Projects.Temasek_Facilities>("temasek-facilities");
builder.AddJavaScriptApp("temasek-facilities-client", "../Temasek.Facilities/Temasek.Facilities.Client")
    .WithNpm(false)
    .WithEnvironment("NUXT_PUBLIC_CLERK_PUBLISHABLE_KEY", clerkPublishableKey)
    .WithEnvironment("NUXT_CLERK_SECRET_KEY", clerkSecretKey)
    .WithHttpEndpoint(port: 3003, env: "PORT")
    .WithReference(facilities)
    .WaitFor(facilities);

builder.Build().Run();
