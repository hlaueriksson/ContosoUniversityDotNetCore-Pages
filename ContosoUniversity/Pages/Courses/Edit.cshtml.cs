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

namespace ContosoUniversity.Pages.Courses;

public class Edit : PageModel
{
    private readonly ICommandProcessor _commandProcessor;
    private readonly IQueryProcessor _queryProcessor;

    [BindProperty]
    public CourseEditCommand Data { get; set; }

    public Edit(ICommandProcessor commandProcessor, IQueryProcessor queryProcessor)
    {
        _commandProcessor = commandProcessor;
        _queryProcessor = queryProcessor;
    }

    public async Task OnGetAsync(CourseEditQuery query) => Data = await _queryProcessor.ProcessAsync(query);

    public async Task<IActionResult> OnPostAsync()
    {
        await _commandProcessor.ProcessAsync(Data);

        return this.RedirectToPageJson(nameof(Index));
    }

    public record CourseEditQuery : IQuery<CourseEditCommand>
    {
        public int? Id { get; init; }
    }

    public class QueryValidator : AbstractValidator<CourseEditQuery>
    {
        public QueryValidator()
        {
            RuleFor(m => m.Id).NotNull();
        }
    }

    public class QueryHandler : IQueryHandler<CourseEditQuery, CourseEditCommand>
    {
        private readonly SchoolContext _db;
        private readonly IConfigurationProvider _configuration;

        public QueryHandler(SchoolContext db, IConfigurationProvider configuration)
        {
            _db = db;
            _configuration = configuration;
        }

        public Task<CourseEditCommand> HandleAsync(CourseEditQuery message, CancellationToken token) =>
            _db.Courses
                .Where(c => c.Id == message.Id)
                .ProjectTo<CourseEditCommand>(_configuration)
                .SingleOrDefaultAsync(token);
    }

    public record CourseEditCommand : ICommand
    {
        [Display(Name = "Number")]
        public int Id { get; init; }
        public string Title { get; init; }
        public int? Credits { get; init; }
        public Department Department { get; init; }
    }

    public class MappingProfile : Profile
    {
        public MappingProfile() => CreateProjection<Course, CourseEditCommand>();
    }

    public class CommandValidator : AbstractValidator<CourseEditCommand>
    {
        public CommandValidator()
        {
            RuleFor(m => m.Title).NotNull().Length(3, 50);
            RuleFor(m => m.Credits).NotNull().InclusiveBetween(0, 5);
        }
    }

    public class CommandHandler : ICommandHandler<CourseEditCommand>
    {
        private readonly SchoolContext _db;

        public CommandHandler(SchoolContext db) => _db = db;

        public async Task HandleAsync(CourseEditCommand request, CancellationToken cancellationToken)
        {
            var course = await _db.Courses.FindAsync(request.Id);

            course.Title = request.Title;
            course.Department = request.Department;
            course.Credits = request.Credits!.Value;
        }
    }
}