using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using CommandQuery;
using ContosoUniversity.Data;
using ContosoUniversity.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ContosoUniversity.Pages.Courses;

public class Index : PageModel
{
    private readonly IQueryProcessor _queryProcessor;

    public Index(IQueryProcessor queryProcessor) => _queryProcessor = queryProcessor;

    public Result Data { get; private set; }

    public async Task OnGetAsync() => Data = await _queryProcessor.ProcessAsync(new CourseIndexQuery());

    public record CourseIndexQuery : IQuery<Result>
    {
    }

    public record Result
    {
        public List<Course> Courses { get; init; }

        public record Course
        {
            public int Id { get; init; }
            public string Title { get; init; }
            public int Credits { get; init; }
            public string DepartmentName { get; init; }
        }
    }

    public class MappingProfile : Profile
    {
        public MappingProfile() => CreateProjection<Course, Result.Course>();
    }

    public class Handler : IQueryHandler<CourseIndexQuery, Result>
    {
        private readonly SchoolContext _db;
        private readonly IConfigurationProvider _configuration;

        public Handler(IDbContextFactory<SchoolContext> db, IConfigurationProvider configuration)
        {
            _db = db.CreateDbContext();
            _configuration = configuration;
        }

        public async Task<Result> HandleAsync(CourseIndexQuery message, CancellationToken token)
        {
            var courses = await _db.Courses
                .OrderBy(d => d.Id)
                .ProjectToListAsync<Result.Course>(_configuration);

            return new Result
            {
                Courses = courses
            };
        }
    }
}