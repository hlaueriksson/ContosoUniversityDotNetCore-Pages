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
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ContosoUniversity.Pages.Instructors;

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
    public InstructorDeleteCommand Data { get; set; }

    public async Task OnGetAsync(InstructorDeleteQuery query)
        => Data = await _queryProcessor.ProcessAsync(query);

    public async Task<ActionResult> OnPostAsync()
    {
        await _commandProcessor.ProcessAsync(Data);

        return this.RedirectToPageJson(nameof(Index));
    }

    public record InstructorDeleteQuery : IQuery<InstructorDeleteCommand>
    {
        public int? Id { get; init; }
    }

    public class Validator : AbstractValidator<InstructorDeleteQuery>
    {
        public Validator()
        {
            RuleFor(m => m.Id).NotNull();
        }
    }

    public record InstructorDeleteCommand : ICommand
    {
        public int? Id { get; init; }

        public string LastName { get; init; }
        [Display(Name = "First Name")]
        public string FirstMidName { get; init; }

        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime? HireDate { get; init; }

        [Display(Name = "Location")]
        public string OfficeAssignmentLocation { get; init; }
    }

    public class MappingProfile : Profile
    {
        public MappingProfile() => CreateProjection<Instructor, InstructorDeleteCommand>();
    }

    public class QueryHandler : IQueryHandler<InstructorDeleteQuery, InstructorDeleteCommand>
    {
        private readonly SchoolContext _db;
        private readonly IConfigurationProvider _configuration;

        public QueryHandler(SchoolContext db, IConfigurationProvider configuration)
        {
            _db = db;
            _configuration = configuration;
        }

        public Task<InstructorDeleteCommand> HandleAsync(InstructorDeleteQuery message, CancellationToken token) => _db
            .Instructors
            .Where(i => i.Id == message.Id)
            .ProjectTo<InstructorDeleteCommand>(_configuration)
            .SingleOrDefaultAsync(token);
    }

    public class CommandHandler : ICommandHandler<InstructorDeleteCommand>
    {
        private readonly SchoolContext _db;

        public CommandHandler(SchoolContext db) => _db = db;

        public async Task HandleAsync(InstructorDeleteCommand message, CancellationToken token)
        {
            var instructor = await _db.Instructors
                .Include(i => i.OfficeAssignment)
                .Where(i => i.Id == message.Id)
                .SingleAsync(token);

            instructor.Handle(message);

            _db.Instructors.Remove(instructor);

            var department = await _db.Departments
                .Where(d => d.InstructorId == message.Id)
                .SingleOrDefaultAsync(token);
            if (department != null)
            {
                department.InstructorId = null;
            }
        }
    }
}