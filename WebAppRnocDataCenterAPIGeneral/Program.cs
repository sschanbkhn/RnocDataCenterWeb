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


// Đọc cấu hình IsLocal từ appsettings.Local.json
// bool isLocal = builder.Configuration.GetValue<bool>("AppSettings:IsLocal");

// bool isLocal = true;
bool isLocal = false;
// true la local host
// false la server



// Tự động chọn connection string
// string connectionString;
string strInformationProductionConnection;
if (isLocal)
{
    // LOCAL - Database của bạn
    // connectionString = "Host=localhost;Port=5432;Database=RnocDataCenter;Username=postgres;Password=Computer123456$;SSL Mode=Prefer;Trust Server Certificate=true;";
    strInformationProductionConnection = "Host=localhost;Port=5432;Database=RnocDataCenter;Username=postgres;Password=Computer123456$;SSL Mode=Prefer;Trust Server Certificate=true;";
    // Console.WriteLine("✅ Đang chạy: LOCAL MODE");
}
else
{
    // SERVER - Database production
    strInformationProductionConnection = "Host=10.155.43.204;Port=5432;Database=rnoc1_dbthem;Username=rnoc1_dbthem;Password=Automation@123;SSL Mode=Prefer;Trust Server Certificate=true;";
    // Console.WriteLine("✅ Đang chạy: SERVER MODE");
}

// Console.WriteLine($"Connection: {strInformationProductionConnection}");
// Console.WriteLine("=== END ===");

// Sử dụng connection string đã chọn
/*
/// builder.Services.AddDbContext<ConnectionsInformationSleepingCellDbContext>(options =>
    ///options.UseNpgsql(connectionString));
*/

// string strConnectionStringsDB = builder.Configuration.GetConnectionString("strInformationProductionConnection")!;
/// var builder = WebApplication.CreateBuilder(args);
/// string strConnectionStringsDB = builder.Configuration.GetConnectionString("strInformationProductionConnection");

// var builder = WebApplication.CreateBuilder(args);

// Thêm dòng này để kiểm tra môi trường
// Console.WriteLine($"Current Environment: {builder.Environment.EnvironmentName}");
// Console.WriteLine($"Is Development: {builder.Environment.IsDevelopment()}");
// Console.WriteLine($"Is Production: {builder.Environment.IsProduction()}");

// Kiểm tra connection string đang được sử dụng
// var connectionString = builder.Configuration.GetConnectionString("strInformationProductionConnection");
// string strConnectionStringsDB = builder.Configuration.GetConnectionString("strInformationProductionConnection")!;


// ĐÚNG - dùng cái mới
builder.Services.AddDbContext<ConnectionsInformationSleepingCellDbContext>(options =>
    options.UseNpgsql(strInformationProductionConnection));

// Console.WriteLine($"Connection String: {strConnectionStringsDB}");

// Phần còn lại của code...














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




/*


builder.Services.AddDbContext<ConnectionsInformationSleepingCellDbContext>(options =>
    options.UseNpgsql(strConnectionStringsDB));

*/


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



