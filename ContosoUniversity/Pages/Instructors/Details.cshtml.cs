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
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ContosoUniversity.Pages.Instructors;

public class Details : PageModel
{
    private readonly IQueryProcessor _queryProcessor;

    public Details(IQueryProcessor queryProcessor) => _queryProcessor = queryProcessor;

    public Model Data { get; private set; }

    public async Task OnGetAsync(Query query) => Data = await _queryProcessor.ProcessAsync(query);

    public record Query : IQuery<Model>
    {
        public int? Id { get; init; }
    }

    public class Validator : AbstractValidator<Query>
    {
        public Validator()
        {
            RuleFor(m => m.Id).NotNull();
        }
    }

    public record Model
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
        public MappingProfile() => CreateProjection<Instructor, Model>();
    }

    public class Handler : IQueryHandler<Query, Model>
    {
        private readonly SchoolContext _db;
        private readonly IConfigurationProvider _configuration;

        public Handler(SchoolContext db, IConfigurationProvider configuration)
        {
            _db = db;
            _configuration = configuration;
        }

        public Task<Model> HandleAsync(Query message, CancellationToken token) => _db
            .Instructors
            .Where(i => i.Id == message.Id)
            .ProjectTo<Model>(_configuration)
            .SingleOrDefaultAsync(token);
    }
}