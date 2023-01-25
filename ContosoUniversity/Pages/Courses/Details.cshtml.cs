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
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ContosoUniversity.Pages.Courses;

public class Details : PageModel
{
    private readonly IQueryProcessor _queryProcessor;

    public Details(IQueryProcessor queryProcessor) => _queryProcessor = queryProcessor;

    public Model Data { get; private set; }

    public async Task OnGetAsync(CourseDetailsQuery query) => Data = await _queryProcessor.ProcessAsync(query);

    public record CourseDetailsQuery : IQuery<Model>
    {
        public int? Id { get; init; }
    }

    public class Validator : AbstractValidator<CourseDetailsQuery>
    {
        public Validator()
        {
            RuleFor(m => m.Id).NotNull();
        }
    }

    public record Model
    {
        public int Id { get; init; }
        public string Title { get; init; }
        public int Credits { get; init; }
        [Display(Name = "Department")]
        public string DepartmentName { get; init; }
    }

    public class MappingProfile : Profile
    {
        public MappingProfile() => CreateProjection<Course, Model>();
    }

    public class Handler : IQueryHandler<CourseDetailsQuery, Model>
    {
        private readonly SchoolContext _db;
        private readonly IConfigurationProvider _configuration;

        public Handler(IDbContextFactory<SchoolContext> db, IConfigurationProvider configuration)
        {
            _db = db.CreateDbContext();
            _configuration = configuration;
        }

        public Task<Model> HandleAsync(CourseDetailsQuery message, CancellationToken token) => 
            _db.Courses
                .Where(i => i.Id == message.Id)
                .ProjectTo<Model>(_configuration)
                .SingleOrDefaultAsync(token);
    }
}