using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Threading;
using System.Threading.Tasks;
using CommandQuery;
using ContosoUniversity.Data;
using ContosoUniversity.Models;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ContosoUniversity.Pages.Departments;

public class Create : PageModel
{
    private readonly ICommandProcessor _commandProcessor;

    [BindProperty]
    public DepartmentCreateCommand Data { get; set; }

    public Create(ICommandProcessor commandProcessor) => _commandProcessor = commandProcessor;

    public async Task<ActionResult> OnPostAsync()
    {
        await _commandProcessor.ProcessAsync(Data);

        return this.RedirectToPageJson("Index");
    }

    public class Validator : AbstractValidator<DepartmentCreateCommand>
    {
        public Validator()
        {
            RuleFor(m => m.Name).NotNull().Length(3, 50);
            RuleFor(m => m.Budget).NotNull();
            RuleFor(m => m.StartDate).NotNull();
            RuleFor(m => m.Administrator).NotNull();
        }
    }

    public record DepartmentCreateCommand : ICommand<int>
    {
        [StringLength(50, MinimumLength = 3)]
        public string Name { get; init; }

        [DataType(DataType.Currency)]
        [Column(TypeName = "money")]
        public decimal? Budget { get; init; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? StartDate { get; init; }

        public Instructor Administrator { get; init; }
    }

    public class CommandHandler : ICommandHandler<DepartmentCreateCommand, int>
    {
        private readonly SchoolContext _context;

        public CommandHandler(SchoolContext context) => _context = context;

        public async Task<int> HandleAsync(DepartmentCreateCommand message, CancellationToken token)
        {
            var department = new Department
            {
                Administrator = message.Administrator,
                Budget = message.Budget!.Value,
                Name = message.Name,
                StartDate = message.StartDate!.Value
            };

            await _context.Departments.AddAsync(department, token);

            await _context.SaveChangesAsync(token);

            return department.Id;
        }
    }
}