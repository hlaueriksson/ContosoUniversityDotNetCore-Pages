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

namespace ContosoUniversity.Pages.Students;

public class Edit : PageModel
{
    private readonly ICommandProcessor _commandProcessor;
    private readonly IQueryProcessor _queryProcessor;

    public Edit(ICommandProcessor commandProcessor, IQueryProcessor queryProcessor)
    {
        _commandProcessor = commandProcessor;
        _queryProcessor = queryProcessor;
    }

    [BindProperty]
    public StudentEditCommand Data { get; set; }

    public async Task OnGetAsync(StudentEditQuery query)
        => Data = await _queryProcessor.ProcessAsync(query);

    public async Task<IActionResult> OnPostAsync()
    {
        await _commandProcessor.ProcessAsync(Data);

        return this.RedirectToPageJson(nameof(Index));
    }

    public record StudentEditQuery : IQuery<StudentEditCommand>
    {
        public int? Id { get; init; }
    }

    public class QueryValidator : AbstractValidator<StudentEditQuery>
    {
        public QueryValidator()
        {
            RuleFor(m => m.Id).NotNull();
        }
    }

    public record StudentEditCommand : ICommand
    {
        public int Id { get; init; }
        public string LastName { get; init; }

        [Display(Name = "First Name")]
        public string FirstMidName { get; init; }

        public DateTime? EnrollmentDate { get; init; }
    }

    public class Validator : AbstractValidator<StudentEditCommand>
    {
        public Validator()
        {
            RuleFor(m => m.LastName).NotNull().Length(1, 50);
            RuleFor(m => m.FirstMidName).NotNull().Length(1, 50);
            RuleFor(m => m.EnrollmentDate).NotNull();
        }
    }

    public class MappingProfile : Profile
    {
        public MappingProfile() => CreateProjection<Student, StudentEditCommand>();
    }

    public class QueryHandler : IQueryHandler<StudentEditQuery, StudentEditCommand>
    {
        private readonly SchoolContext _db;
        private readonly IConfigurationProvider _configuration;

        public QueryHandler(SchoolContext db, IConfigurationProvider configuration)
        {
            _db = db;
            _configuration = configuration;
        }

        public async Task<StudentEditCommand> HandleAsync(StudentEditQuery message, CancellationToken token) => await _db.Students
            .Where(s => s.Id == message.Id)
            .ProjectTo<StudentEditCommand>(_configuration)
            .SingleOrDefaultAsync(token);
    }

    public class CommandHandler : ICommandHandler<StudentEditCommand>
    {
        private readonly SchoolContext _db;

        public CommandHandler(SchoolContext db) => _db = db;

        public async Task HandleAsync(StudentEditCommand message, CancellationToken token)
        {
            var student = await _db.Students.FindAsync(message.Id);

            student.FirstMidName = message.FirstMidName;
            student.LastName = message.LastName;
            student.EnrollmentDate = message.EnrollmentDate!.Value;
        }
    }
}