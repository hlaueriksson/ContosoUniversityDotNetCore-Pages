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
using DelegateDecompiler.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ContosoUniversity.Pages.Departments;

public class Details : PageModel
{
    private readonly IQueryProcessor _queryProcessor;

    public Model Data { get; private set; }

    public Details(IQueryProcessor queryProcessor) => _queryProcessor = queryProcessor;

    public async Task OnGetAsync(DepartmentDetailsQuery query)
        => Data = await _queryProcessor.ProcessAsync(query);

    public record DepartmentDetailsQuery : IQuery<Model>
    {
        public int Id { get; init; }
    }

    public record Model
    {
        public string Name { get; init; }

        public decimal Budget { get; init; }

        public DateTime StartDate { get; init; }

        public int Id { get; init; }

        [Display(Name = "Administrator")]
        public string AdministratorFullName { get; init; }
    }

    public class MappingProfile : Profile
    {
        public MappingProfile() => CreateProjection<Department, Model>();
    }
        
    public class QueryHandler : IQueryHandler<DepartmentDetailsQuery, Model>
    {
        private readonly SchoolContext _context;
        private readonly IConfigurationProvider _configuration;

        public QueryHandler(SchoolContext context, IConfigurationProvider configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public Task<Model> HandleAsync(DepartmentDetailsQuery message, 
            CancellationToken token) => 
            _context.Departments
                .Where(m => m.Id == message.Id)
                .ProjectTo<Model>(_configuration)
                .DecompileAsync()
                .SingleOrDefaultAsync(token);
    }
}