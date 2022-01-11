using System;
using System.Threading.Tasks;
using ContosoUniversity.Models;
using ContosoUniversity.Pages.Students;
using Shouldly;
using Xunit;

namespace ContosoUniversity.IntegrationTests.Pages.Students;

[Collection(nameof(SliceFixture))]
public class EditTests
{
    private readonly SliceFixture _fixture;

    public EditTests(SliceFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task Should_get_edit_details()
    {
        var cmd = new Create.StudentCreateCommand
        {
            FirstMidName = "Joe",
            LastName = "Schmoe",
            EnrollmentDate = DateTime.Today
        };

        var studentId = await _fixture.ProcessAsync(cmd);

        var query = new Edit.StudentEditQuery
        {
            Id = studentId
        };

        var result = await _fixture.ProcessAsync(query);

        result.FirstMidName.ShouldBe(cmd.FirstMidName);
        result.LastName.ShouldBe(cmd.LastName);
        result.EnrollmentDate.ShouldBe(cmd.EnrollmentDate);
    }

    [Fact]
    public async Task Should_edit_student()
    {
        var createCommand = new Create.StudentCreateCommand
        {
            FirstMidName = "Joe",
            LastName = "Schmoe",
            EnrollmentDate = DateTime.Today
        };

        var studentId = await _fixture.ProcessAsync(createCommand);

        var editCommand = new Edit.StudentEditCommand
        {
            Id = studentId,
            FirstMidName = "Mary",
            LastName = "Smith",
            EnrollmentDate = DateTime.Today.AddYears(-1)
        };

        await _fixture.ProcessAsync(editCommand);

        var student = await _fixture.FindAsync<Student>(studentId);

        student.ShouldNotBeNull();
        student.FirstMidName.ShouldBe(editCommand.FirstMidName);
        student.LastName.ShouldBe(editCommand.LastName);
        student.EnrollmentDate.ShouldBe(editCommand.EnrollmentDate.GetValueOrDefault());
    }
}