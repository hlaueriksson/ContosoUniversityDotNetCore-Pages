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

namespace ContosoUniversity.Pages.Students;

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
    public StudentDeleteCommand Data { get; set; }

    public async Task OnGetAsync(StudentDeleteQuery query) => Data = await _queryProcessor.ProcessAsync(query);

    public async Task<IActionResult> OnPostAsync()
    {
        await _commandProcessor.ProcessAsync(Data);

        return this.RedirectToPageJson(nameof(Index));
    }

    public record StudentDeleteQuery : IQuery<StudentDeleteCommand>
    {
        public int Id { get; init; }
    }

    public record StudentDeleteCommand : ICommand
    {
        public int Id { get; init; }
        [Display(Name = "First Name")]
        public string FirstMidName { get; init; }
        public string LastName { get; init; }
        public DateTime EnrollmentDate { get; init; }
    }

    public class MappingProfile : Profile
    {
        public MappingProfile() => CreateProjection<Student, StudentDeleteCommand>();
    }

    public class QueryHandler : IQueryHandler<StudentDeleteQuery, StudentDeleteCommand>
    {
        private readonly SchoolContext _db;
        private readonly IConfigurationProvider _configuration;

        public QueryHandler(IDbContextFactory<SchoolContext> db, IConfigurationProvider configuration)
        {
            _db = db.CreateDbContext();
            _configuration = configuration;
        }

        public async Task<StudentDeleteCommand> HandleAsync(StudentDeleteQuery message, CancellationToken token) => await _db
            .Students
            .Where(s => s.Id == message.Id)
            .ProjectTo<StudentDeleteCommand>(_configuration)
            .SingleOrDefaultAsync(token);
    }

    public class CommandHandler : ICommandHandler<StudentDeleteCommand>
    {
        private readonly SchoolContext _db;

        public CommandHandler(IDbContextFactory<SchoolContext> db) => _db = db.CreateDbContext();

        public async Task HandleAsync(StudentDeleteCommand message, CancellationToken token)
        {
            _db.Students.Remove(await _db.Students.FindAsync(message.Id));
        }
    }

}