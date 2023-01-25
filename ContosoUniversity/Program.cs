using CommandQuery;
using CommandQuery.DependencyInjection;
using ContosoUniversity.Data;
using ContosoUniversity.Infrastructure;
using ContosoUniversity.Infrastructure.Tags;
using FluentValidation;
using FluentValidation.AspNetCore;
using HtmlTags;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

RegisterServices(builder);

var app = builder.Build();

ConfigureApplication(app);

app.Run();

static void RegisterServices(WebApplicationBuilder builder)
{
    var services = builder.Services;

    services.AddMiniProfiler().AddEntityFramework();

    services.AddDbContextFactory<SchoolContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
    services.AddDbContext<SchoolContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")),
        optionsLifetime: ServiceLifetime.Singleton);

    services.AddAutoMapper(typeof(Program));

    services.AddCommands(typeof(Program).Assembly);
    services.AddQueries(typeof(Program).Assembly);

    services.AddHtmlTags(new TagConventions());

    services.AddRazorPages(opt =>
        {
            opt.Conventions.ConfigureFilter(new DbContextTransactionPageFilter());
            opt.Conventions.ConfigureFilter(new ValidatorPageFilter());
        });

    services.AddFluentValidationAutoValidation();
    services.AddFluentValidationClientsideAdapters();
    services.AddValidatorsFromAssemblyContaining<Program>();

    services.AddMvc(opt => opt.ModelBinderProviders.Insert(0, new EntityModelBinderProvider()));
}

static void ConfigureApplication(WebApplication app)
{
    app.UseMiniProfiler();

    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Error");
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();

    app.UseAuthorization();

    app.MapRazorPages();

    // Validation
    app.Services.GetService<ICommandProcessor>().AssertConfigurationIsValid();
    app.Services.GetService<IQueryProcessor>().AssertConfigurationIsValid();
}