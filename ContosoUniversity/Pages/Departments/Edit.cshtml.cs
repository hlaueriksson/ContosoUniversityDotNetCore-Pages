using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using CommandQuery;
using ContosoUniversity.Data;
using ContosoUniversity.Models;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ContosoUniversity.Pages.Departments;

public class Edit : PageModel
{
    private readonly ICommandProcessor _commandProcessor;
    private readonly IQueryProcessor _queryProcessor;

    [BindProperty]
    public DepartmentEditCommand Data { get; set; }

    public Edit(ICommandProcessor commandProcessor, IQueryProcessor queryProcessor)
    {
        _commandProcessor = commandProcessor;
        _queryProcessor = queryProcessor;
    }

    public async Task OnGetAsync(DepartmentEditQuery query)
        => Data = await _queryProcessor.ProcessAsync(query);

    public async Task<ActionResult> OnPostAsync(int id)
    {
        await _commandProcessor.ProcessAsync(Data);

        return this.RedirectToPageJson("Index");
    }

    public record DepartmentEditQuery : IQuery<DepartmentEditCommand>
    {
        public int Id { get; init; }
    }

    public record DepartmentEditCommand : ICommand
    {
        public string Name { get; init; }

        public decimal? Budget { get; init; }

        public DateTime? StartDate { get; init; }

        public Instructor Administrator { get; init; }
        public int Id { get; init; }
        public byte[] RowVersion { get; init; }
    }

    public class Validator : AbstractValidator<DepartmentEditCommand>
    {
        public Validator()
        {
            RuleFor(m => m.Name).NotNull().Length(3, 50);
            RuleFor(m => m.Budget).NotNull();
            RuleFor(m => m.StartDate).NotNull();
            RuleFor(m => m.Administrator).NotNull();
        }
    }

    public class MappingProfile : Profile
    {
        public MappingProfile() => CreateProjection<Department, DepartmentEditCommand>();
    }

    public class QueryHandler : IQueryHandler<DepartmentEditQuery, DepartmentEditCommand>
    {
        private readonly SchoolContext _db;
        private readonly IConfigurationProvider _configuration;

        public QueryHandler(SchoolContext db, 
            IConfigurationProvider configuration)
        {
            _db = db;
            _configuration = configuration;
        }

        public async Task<DepartmentEditCommand> HandleAsync(DepartmentEditQuery message, 
            CancellationToken token) => await _db
            .Departments
            .Where(d => d.Id == message.Id)
            .ProjectTo<DepartmentEditCommand>(_configuration)
            .SingleOrDefaultAsync(token);
    }

    public class CommandHandler : ICommandHandler<DepartmentEditCommand>
    {
        private readonly SchoolContext _db;

        public CommandHandler(SchoolContext db) => _db = db;

        public async Task HandleAsync(DepartmentEditCommand message, 
            CancellationToken token)
        {
            var dept = await _db.Departments.FindAsync(message.Id);

            dept.Name = message.Name;
            dept.StartDate = message.StartDate!.Value;
            dept.Budget = message.Budget!.Value;
            dept.RowVersion = message.RowVersion;
            dept.Administrator = await _db.Instructors.FindAsync(message.Administrator.Id);
        }
    }
}