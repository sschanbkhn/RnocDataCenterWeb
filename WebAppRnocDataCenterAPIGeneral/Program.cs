using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.SqlServer;
using Microsoft.EntityFrameworkCore;
/// using WebAppRnocDataCenterAPIGeneral.WebAPIASPModels.NSN.SleepingCell;  // Import models namespace
/// using ClassLibraryRnocDataCenterWebDataClass.WebAPIASPModelsEntities.NSN.SleepingCell;




using ClassLibraryRnocDataCenterWebDataClass.WebAPIASPModelsEntities.NSN.SleepingCell;
using ClassLibraryRnocDataCenterWebBusiness.Services.Interfaces.NSN.SleepingCell;
using ClassLibraryRnocDataCenterWebBusiness.Services.Implementations.NSN.SleepingCell;
using ClassLibraryRnocDataCenterWebDataClass.Repositories.Interfaces.NSN.SleepingCell;


using ClassLibraryRnocDataCenterWebDataClass.Repositories.Implementations.NSN.SleepingCell;
using ClassLibraryRnocDataCenterWebDataClass.Repositories.Interfaces.NSN;
using Microsoft.Win32;






var builder = WebApplication.CreateBuilder(args);
string strConnectionStringsDB = builder.Configuration.GetConnectionString("strInformationProductionConnection")!;
/// var builder = WebApplication.CreateBuilder(args);
/// string strConnectionStringsDB = builder.Configuration.GetConnectionString("strInformationProductionConnection");

// Register Business Services
builder.Services.AddScoped<InterfaceSleepingCellService, ImplementationSleepingCellService>();
builder.Services.AddScoped<InterfaceResetService, ImplementationResetService>();
builder.Services.AddScoped<InterfaceValidationService, ImplementationValidationService>();
builder.Services.AddScoped<InterfaceDashboardService, ImplementationDashboardService>();

builder.Services.AddScoped<InterfaceMonitorService, ImplementationKpiMonitorService>();





// Register Repositories
builder.Services.AddScoped<InterfaceSleepingCellKpiRepository, ImplementationSleepingCellKpiRepository>();
builder.Services.AddScoped<InterfaceBtsInfoRepository, ImplementationBtsInfoRepository>();
builder.Services.AddScoped<InterfaceSleepingCellResultRepository, ImplementationSleepingCellResultRepository>();
builder.Services.AddScoped<InterfaceResetLimitRepository, ImplementationResetLimitRepository>();
builder.Services.AddScoped<InterfaceSshAccountRepository, ImplementationSshAccountRepository>();
builder.Services.AddScoped<InterfaceDashboardRepository, ImplementationDashboardRepository>();
builder.Services.AddScoped<InterfaceFilePathRepository, ImplementationFilePathRepository>();
builder.Services.AddScoped<InterfaceSleepingCellArchiveRepository, ImplementationSleepingCellArchiveRepository>();

// Ho?c n?u dùng builder pattern:
builder.Services.AddScoped<InterfaceFilterTableRepository, ImplementationFilterTableRepository>();

// Ho?c n?u dùng builder pattern:
builder.Services.AddScoped<InterfaceDetailTableRepository, ImplementationDetailTableRepository>();



// Add services to the container.
// Configure Entity Framework v?i PostgreSQL
/// builder.Services.AddDbContext<ConnectionsInformationSleepingCellDbContext>(options =>
/// options.UseNpgsql(builder.Configuration.GetConnectionString("strConnectionStringsDB")));
/// builder.Services.AddDbContext<ConnectionsInformationSleepingCellDbContext>(options =>
/// options.UseNpgsql(builder.Configuration.GetConnectionString("strInformationProductionConnection")));

/*

builder.Services.AddDbContext<ConnectionsInformationSleepingCellDbContext>(options =>
    options.UseNpgsql(strConnectionStringsDB));

*/







builder.Services.AddDbContext<ConnectionsInformationSleepingCellDbContext>(options =>
    options.UseNpgsql(strConnectionStringsDB));


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add CORS (optional - for frontend integration)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});



/// app.UseCors(builder => builder.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());
var app = builder.Build();



// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Use CORS
app.UseCors("AllowAllOrigins");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

/// app.Run();
await app.RunAsync();
