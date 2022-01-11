using System;
using System.Linq;
using System.Threading.Tasks;
using ContosoUniversity.Models;
using ContosoUniversity.Pages.Courses;
using ContosoUniversity.Pages.Instructors;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;

namespace ContosoUniversity.IntegrationTests.Pages.Courses;

[Collection(nameof(SliceFixture))]
public class CreateTests
{
    private readonly SliceFixture _fixture;

    public CreateTests(SliceFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task Should_create_new_course()
    {
        var adminId = await _fixture.ProcessAsync(new CreateEdit.InstructorCreateEditCommand
        {
            FirstMidName = "George",
            LastName = "Costanza",
            HireDate = DateTime.Today
        });

        var dept = new Department
        {
            Name = "History",
            InstructorId = adminId,
            Budget = 123m,
            StartDate = DateTime.Today
        };

        Create.CourseCreateCommand command = null;

        await _fixture.ExecuteDbContextAsync(async (ctxt, processor) =>
        {
            await ctxt.Departments.AddAsync(dept);
            command = new Create.CourseCreateCommand
            {
                Credits = 4,
                Department = dept,
                Number = _fixture.NextCourseNumber(),
                Title = "English 101"
            };
            await processor.ProcessAsync(command);
        });

        var created = await _fixture.ExecuteDbContextAsync(db => db.Courses.Where(c => c.Id == command.Number).SingleOrDefaultAsync());

        created.ShouldNotBeNull();
        created.DepartmentId.ShouldBe(dept.Id);
        created.Credits.ShouldBe(command.Credits);
        created.Title.ShouldBe(command.Title);
    }
}