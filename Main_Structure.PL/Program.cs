

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();

builder.Services.AddBLL(builder.Configuration);
builder.Services.AddDAL(builder.Configuration);
builder.Services.AddPL(builder.Configuration);



var app = builder.Build();
app.UseExceptionHandler();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();



app.Run();
