using Chat_App_Server.Hubs;

var builder = WebApplication.CreateBuilder(args);
//enable cors for accessing from the ui
builder.Services.AddCors(options => options.AddDefaultPolicy(policy =>policy
        .AllowCredentials()
        .AllowAnyHeader()
        .AllowAnyMethod()
        .SetIsOriginAllowed(x => true)
));
//add signalR service
builder.Services.AddSignalR();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
//use cors
app.UseCors();
app.UseRouting();

app.UseAuthorization();

//For using signalR
app.UseEndpoints(endpoints =>
{
    endpoints.MapHub<ChatHub>("/chathub");
});

app.Run();
