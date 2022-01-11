using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using CommandQuery;
using ContosoUniversity.Data;
using ContosoUniversity.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ContosoUniversity.Pages.Departments;

public class Delete : PageModel
{
    private readonly ICommandProcessor _commandProcessor;
    private readonly IQueryProcessor _queryProcessor;

    public Delete(ICommandProcessor commandProcessor, IQueryProcessor queryProcessor)
    {
        _commandProcessor = commandProcessor;
        _queryProcessor = queryProcessor;
    }

    [BindProperty]
    public DepartmentDeleteCommand Data { get; set; }

    public async Task OnGetAsync(DepartmentDeleteQuery query)
        => Data = await _queryProcessor.ProcessAsync(query);

    public async Task<ActionResult> OnPostAsync()
    {
        await _commandProcessor.ProcessAsync(Data);

        return this.RedirectToPageJson("Index");
    }

    public record DepartmentDeleteQuery : IQuery<DepartmentDeleteCommand>
    {
        public int Id { get; init; }
    }

    public record DepartmentDeleteCommand : ICommand
    {
        public string Name { get; init; }

        public decimal Budget { get; init; }

        public DateTime StartDate { get; init; }

        public int Id { get; init; }

        [Display(Name = "Administrator")]
        public string AdministratorFullName { get; init; }

        public byte[] RowVersion { get; init; }
    }

    public class MappingProfile : Profile
    {
        public MappingProfile() => CreateProjection<Department, DepartmentDeleteCommand>();
    }

    public class QueryHandler : IQueryHandler<DepartmentDeleteQuery, DepartmentDeleteCommand>
    {
        private readonly SchoolContext _db;
        private readonly IConfigurationProvider _configuration;

        public QueryHandler(SchoolContext db, IConfigurationProvider configuration)
        {
            _db = db;
            _configuration = configuration;
        }

        public async Task<DepartmentDeleteCommand> HandleAsync(DepartmentDeleteQuery message, CancellationToken token) => await _db
            .Departments
            .Where(d => d.Id == message.Id)
            .ProjectTo<DepartmentDeleteCommand>(_configuration)
            .SingleOrDefaultAsync(token);
    }

    public class CommandHandler : ICommandHandler<DepartmentDeleteCommand>
    {
        private readonly SchoolContext _db;

        public CommandHandler(SchoolContext db) => _db = db;

        public async Task HandleAsync(DepartmentDeleteCommand message, CancellationToken token)
        {
            var department = await _db.Departments.FindAsync(message.Id);

            _db.Departments.Remove(department);
        }
    }
}