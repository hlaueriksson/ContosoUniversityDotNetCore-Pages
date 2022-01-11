using System;
using System.Collections.Generic;
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

namespace ContosoUniversity.Pages.Instructors;

public class CreateEdit : PageModel
{
    private readonly ICommandProcessor _commandProcessor;
    private readonly IQueryProcessor _queryProcessor;

    [BindProperty]
    public InstructorCreateEditCommand Data { get; set; }

    public CreateEdit(ICommandProcessor commandProcessor, IQueryProcessor queryProcessor)
    {
        _commandProcessor = commandProcessor;
        _queryProcessor = queryProcessor;
    }

    public async Task OnGetCreateAsync() => Data = await _queryProcessor.ProcessAsync(new InstructorCreateEditQuery());

    public async Task<IActionResult> OnPostCreateAsync()
    {
        await _commandProcessor.ProcessAsync(Data);

        return this.RedirectToPageJson(nameof(Index));
    }

    public async Task OnGetEditAsync(InstructorCreateEditQuery query) => Data = await _queryProcessor.ProcessAsync(query);

    public async Task<IActionResult> OnPostEditAsync()
    {
        await _commandProcessor.ProcessAsync(Data);

        return this.RedirectToPageJson(nameof(Index));
    }

    public record InstructorCreateEditQuery : IQuery<InstructorCreateEditCommand>
    {
        public int? Id { get; init; }
    }

    public class QueryValidator : AbstractValidator<InstructorCreateEditQuery>
    {
        public QueryValidator()
        {
            RuleFor(m => m.Id).NotNull();
        }
    }

    public record InstructorCreateEditCommand : ICommand<int>
    {
        public InstructorCreateEditCommand()
        {
            AssignedCourses = new List<AssignedCourseData>();
            CourseAssignments = new List<CourseAssignment>();
            SelectedCourses = new string[0];
        }

        public int? Id { get; init; }

        public string LastName { get; init; }
        [Display(Name = "First Name")]
        public string FirstMidName { get; init; }

        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime? HireDate { get; init; }

        [Display(Name = "Location")]
        public string OfficeAssignmentLocation { get; init; }

        public string[] SelectedCourses { get; init; }

        public List<AssignedCourseData> AssignedCourses { get; init; }
        public List<CourseAssignment> CourseAssignments { get; init; }

        public record AssignedCourseData
        {
            public int CourseId { get; init; }
            public string Title { get; init; }
            public bool Assigned { get; init; }
        }

        public record CourseAssignment
        {
            public int CourseId { get; init; }
        }
    }

    public class CommandValidator : AbstractValidator<InstructorCreateEditCommand>
    {
        public CommandValidator()
        {
            RuleFor(m => m.LastName).NotNull().Length(0, 50);
            RuleFor(m => m.FirstMidName).NotNull().Length(0, 50);
            RuleFor(m => m.HireDate).NotNull();
        }
    }

    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateProjection<Instructor, InstructorCreateEditCommand>()
                .ForMember(d => d.SelectedCourses, opt => opt.Ignore())
                .ForMember(d => d.AssignedCourses, opt => opt.Ignore());
            CreateProjection<CourseAssignment, InstructorCreateEditCommand.CourseAssignment>();
        }
    }

    public class QueryHandler : IQueryHandler<InstructorCreateEditQuery, InstructorCreateEditCommand>
    {
        private readonly SchoolContext _db;
        private readonly IConfigurationProvider _configuration;

        public QueryHandler(SchoolContext db, IConfigurationProvider configuration)
        {
            _db = db;
            _configuration = configuration;
        }

        public async Task<InstructorCreateEditCommand> HandleAsync(InstructorCreateEditQuery message, CancellationToken token)
        {
            InstructorCreateEditCommand model;
            if (message.Id == null)
            {
                model = new InstructorCreateEditCommand();
            }
            else
            {
                model = await _db.Instructors
                    .Where(i => i.Id == message.Id)
                    .ProjectTo<InstructorCreateEditCommand>(_configuration)
                    .SingleOrDefaultAsync(token);
            }

            var instructorCourses = new HashSet<int>(model.CourseAssignments.Select(c => c.CourseId));
            var viewModel = _db.Courses.Select(course => new InstructorCreateEditCommand.AssignedCourseData
            {
                CourseId = course.Id,
                Title = course.Title,
                Assigned = instructorCourses.Any() && instructorCourses.Contains(course.Id)
            }).ToList();

            model = model with { AssignedCourses = viewModel };

            return model;
        }
    }

    public class CommandHandler : ICommandHandler<InstructorCreateEditCommand, int>
    {
        private readonly SchoolContext _db;

        public CommandHandler(SchoolContext db) => _db = db;

        public async Task<int> HandleAsync(InstructorCreateEditCommand message, CancellationToken token)
        {
            Instructor instructor;
            if (message.Id == null)
            {
                instructor = new Instructor();
                await _db.Instructors.AddAsync(instructor, token);
            }
            else
            {
                instructor = await _db.Instructors
                    .Include(i => i.OfficeAssignment)
                    .Include(i => i.CourseAssignments)
                    .Where(i => i.Id == message.Id)
                    .SingleAsync(token);
            }

            var courses = await _db.Courses.ToListAsync(token);

            instructor.Handle(message, courses);

            await _db.SaveChangesAsync(token);

            return instructor.Id;
        }
    }
}