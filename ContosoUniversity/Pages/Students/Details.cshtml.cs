﻿using System;
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
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ContosoUniversity.Pages.Students;

public class Details : PageModel
{
    private readonly IQueryProcessor _queryProcessor;

    public Details(IQueryProcessor queryProcessor) => _queryProcessor = queryProcessor;

    public Model Data { get; private set; }

    public async Task OnGetAsync(StudentDetailsQuery query)
        => Data = await _queryProcessor.ProcessAsync(query);

    public record StudentDetailsQuery : IQuery<Model>
    {
        public int Id { get; init; }
    }

    public record Model
    {
        public int Id { get; init; }
        [Display(Name = "First Name")]
        public string FirstMidName { get; init; }
        public string LastName { get; init; }
        public DateTime EnrollmentDate { get; init; }
        public List<Enrollment> Enrollments { get; init; }

        public record Enrollment
        {
            public string CourseTitle { get; init; }
            public Grade? Grade { get; init; }
        }
    }

    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateProjection<Student, Model>();
            CreateProjection<Enrollment, Model.Enrollment>();
        }
    }

    public class Handler : IQueryHandler<StudentDetailsQuery, Model>
    {
        private readonly SchoolContext _db;
        private readonly IConfigurationProvider _configuration;

        public Handler(IDbContextFactory<SchoolContext> db, IConfigurationProvider configuration)
        {
            _db = db.CreateDbContext();
            _configuration = configuration;
        }

        public Task<Model> HandleAsync(StudentDetailsQuery message, CancellationToken token) => _db
            .Students
            .Where(s => s.Id == message.Id)
            .ProjectTo<Model>(_configuration)
            .SingleOrDefaultAsync(token);
    }
}