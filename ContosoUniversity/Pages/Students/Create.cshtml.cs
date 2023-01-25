using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using CommandQuery;
using ContosoUniversity.Data;
using ContosoUniversity.Models;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ContosoUniversity.Pages.Students;

public class Create : PageModel
{
    private readonly ICommandProcessor _commandProcessor;

    public Create(ICommandProcessor commandProcessor) => _commandProcessor = commandProcessor;

    [BindProperty]
    public StudentCreateCommand Data { get; set; }

    public void OnGet() => Data = new StudentCreateCommand();

    public async Task<IActionResult> OnPostAsync()
    {
        await _commandProcessor.ProcessAsync(Data);

        return this.RedirectToPageJson(nameof(Index));
    }

    public record StudentCreateCommand : ICommand<int>
    {
        public string LastName { get; init; }

        [Display(Name = "First Name")]
        public string FirstMidName { get; init; }

        public DateTime? EnrollmentDate { get; init; }
    }

    public class Validator : AbstractValidator<StudentCreateCommand>
    {
        public Validator()
        {
            RuleFor(m => m.LastName).NotNull().Length(1, 50);
            RuleFor(m => m.FirstMidName).NotNull().Length(1, 50);
            RuleFor(m => m.EnrollmentDate).NotNull();
        }
    }

    public class Handler : ICommandHandler<StudentCreateCommand, int>
    {
        private readonly SchoolContext _db;

        public Handler(IDbContextFactory<SchoolContext> db) => _db = db.CreateDbContext();

        public async Task<int> HandleAsync(StudentCreateCommand message, CancellationToken token)
        {
            var student = new Student
            {
                FirstMidName = message.FirstMidName,
                LastName = message.LastName,
                EnrollmentDate = message.EnrollmentDate!.Value
            };

            await _db.Students.AddAsync(student, token);

            await _db.SaveChangesAsync(token);

            return student.Id;
        }
    }
}