using System.Globalization;
using GESTIONHOTELERA_V1_0_2.Components;
using GESTIONHOTELERA_V1_0_2.Components.Account;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using GESTIONHOTELERA_V1_0_2.Data;
using GESTIONHOTELERA_V1_0_2.Data.ENTIDADES;
using GESTIONHOTELERA_V1_0_2.Data.REPOSITORIO;
using GESTIONHOTELERA_V1_0_2.Data.REPOSITORIO.IREPOSITORIO;
using GESTIONHOTELERA_V1_0_2.Aplication.SERVICIOS;
using GESTIONHOTELERA_V1_0_2.Aplication.SERVICIOS;

var culturaPeru = new CultureInfo("es-PE");
CultureInfo.DefaultThreadCurrentCulture = culturaPeru;
CultureInfo.DefaultThreadCurrentUICulture = culturaPeru;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddIdentityCookies();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorNumbersToAdd: null);
        sqlOptions.CommandTimeout(60);
    }));

// Blazor Server puede ejecutar operaciones mientras el usuario cambia rapido de pagina.
// La fabrica permite crear un DbContext nuevo por operacion y evita reutilizar un contexto ya eliminado.
builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorNumbersToAdd: null);
        sqlOptions.CommandTimeout(60);
    }), ServiceLifetime.Scoped);
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddErrorDescriber<SpanishIdentityErrorDescriber>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

// Servicios de dominio y repositorios del sistema hotelero.
builder.Services.AddScoped<IClienteRepositorio, ClienteRepositorio>();
builder.Services.AddScoped<IHabitacionRepositorio, HabitacionRepositorio>();
builder.Services.AddScoped<ITipoHabitacionRepositorio, TipoHabitacionRepositorio>();
builder.Services.AddScoped<IReservaRepositorio, ReservaRepositorio>();
builder.Services.AddScoped<IDetalleReservaRepositorio, DetalleReservaRepositorio>();
builder.Services.AddScoped<IPagoRepositorio, PagoRepositorio>();
builder.Services.AddScoped<IMetodoPagoRepositorio, MetodoPagoRepositorio>();
builder.Services.AddScoped<ILimpiezaRepositorio, LimpiezaRepositorio>();

builder.Services.AddScoped<IClienteServicio, ClienteServicio>();
builder.Services.AddScoped<IHabitacionServicio, HabitacionServicio>();
builder.Services.AddScoped<ITipoHabitacionServicio, TipoHabitacionServicio>();
builder.Services.AddScoped<IReservaServicio, ReservaServicio>();
builder.Services.AddScoped<IPagoServicio, PagoServicio>();
builder.Services.AddScoped<ILimpiezaServicio, LimpiezaServicio>();
builder.Services.AddScoped<IReporteServicio, ReporteServicio>();


var app = builder.Build();

/*
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    await dbContext.Database.MigrateAsync();

    // Comenta temporalmente esta línea
    // await IdentityDataSeeder.SeedAsync(scope.ServiceProvider);
}*/


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

app.Run();
