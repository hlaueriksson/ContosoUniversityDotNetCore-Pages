using System.Threading;
using System.Threading.Tasks;
using CommandQuery;
using ContosoUniversity.Data;
using ContosoUniversity.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ContosoUniversity.Pages.Courses;

public class Create : PageModel
{
    private readonly ICommandProcessor _commandProcessor;

    public Create(ICommandProcessor commandProcessor) => _commandProcessor = commandProcessor;

    [BindProperty]
    public CourseCreateCommand Data { get; set; }

    public async Task<IActionResult> OnPostAsync()
    {
        await _commandProcessor.ProcessAsync(Data);

        return this.RedirectToPageJson("Index");
    }

    public record CourseCreateCommand : ICommand<int>
    {
        public int Number { get; init; }
        public string Title { get; init; }
        public int Credits { get; init; }
        public Department Department { get; init; }
    }

    public class Handler : ICommandHandler<CourseCreateCommand, int>
    {
        private readonly SchoolContext _db;

        public Handler(IDbContextFactory<SchoolContext> db) => _db = db.CreateDbContext();

        public async Task<int> HandleAsync(CourseCreateCommand message, CancellationToken token)
        {
            var course = new Course
            {
                Id = message.Number,
                Credits = message.Credits,
                Department = message.Department,
                Title = message.Title
            };

            await _db.Courses.AddAsync(course, token);

            await _db.SaveChangesAsync(token);

            return course.Id;
        }
    }
}