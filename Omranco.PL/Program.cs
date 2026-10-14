using Omranco.BLL;
using Omranco.DAL;
using Omranco.DAL.Database.Seeders;
using Omranco.PL;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();

builder.Services.AddBLL(builder.Configuration);
builder.Services.AddDAL(builder.Configuration);
builder.Services.AddPL(builder.Configuration);



var app = builder.Build();

//Add Seeders
await IdentitySeeder.SeedAsync(app.Services, builder.Configuration);


app.UseExceptionHandler();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();



app.Run();
