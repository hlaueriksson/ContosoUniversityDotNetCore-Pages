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
    public CourseDeleteCommand Data { get; set; }

    public async Task OnGetAsync(CourseDeleteQuery query) => Data = await _queryProcessor.ProcessAsync(query);

    public async Task<IActionResult> OnPostAsync()
    {
        await _commandProcessor.ProcessAsync(Data);

        return this.RedirectToPageJson(nameof(Index));
    }

    public record CourseDeleteQuery : IQuery<CourseDeleteCommand>
    {
        public int? Id { get; init; }
    }

    public class QueryValidator : AbstractValidator<CourseDeleteQuery>
    {
        public QueryValidator()
        {
            RuleFor(m => m.Id).NotNull();
        }
    }

    public class MappingProfile : Profile
    {
        public MappingProfile() => CreateProjection<Course, CourseDeleteCommand>();
    }

    public class QueryHandler : IQueryHandler<CourseDeleteQuery, CourseDeleteCommand>
    {
        private readonly SchoolContext _db;
        private readonly IConfigurationProvider _configuration;

        public QueryHandler(SchoolContext db, IConfigurationProvider configuration)
        {
            _db = db;
            _configuration = configuration;
        }

        public Task<CourseDeleteCommand> HandleAsync(CourseDeleteQuery message, CancellationToken token) =>
            _db.Courses
                .Where(c => c.Id == message.Id)
                .ProjectTo<CourseDeleteCommand>(_configuration)
                .SingleOrDefaultAsync(token);
    }

    public record CourseDeleteCommand : ICommand
    {
        [Display(Name = "Number")]
        public int Id { get; init; }
        public string Title { get; init; }
        public int Credits { get; init; }

        [Display(Name = "Department")]
        public string DepartmentName { get; init; }
    }

    public class CommandHandler : ICommandHandler<CourseDeleteCommand>
    {
        private readonly SchoolContext _db;

        public CommandHandler(SchoolContext db) => _db = db;

        public async Task HandleAsync(CourseDeleteCommand message, CancellationToken token)
        {
            var course = await _db.Courses.FindAsync(message.Id);

            _db.Courses.Remove(course);
        }
    }
}