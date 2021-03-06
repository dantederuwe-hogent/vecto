﻿using Bogus;
using Vecto.Application.Helpers;
using Vecto.Application.Login;
using Vecto.Application.Profile;
using Vecto.Application.Register;
using Vecto.Application.Sections;
using Vecto.Core.Entities;

namespace Vecto.Infrastructure.Data
{
    public static class DummyData
    {
        public static readonly Faker<User> UserFaker = new Faker<User>()
            .RuleFor(u => u.FirstName, f => f.Person.FirstName)
            .RuleFor(u => u.LastName, f => f.Person.LastName)
            .RuleFor(u => u.Email, f => f.Person.Email);

        public static readonly Faker<RegisterDTO> RegisterDTOFaker = new Faker<RegisterDTO>()
            .RuleFor(r => r.FirstName, f => f.Person.FirstName)
            .RuleFor(r => r.LastName, f => f.Person.LastName)
            .RuleFor(r => r.Email, f => f.Person.Email)
            .RuleFor(r => r.Password, f => f.Internet.Password(8))
            .RuleFor(r => r.ConfirmPassword, (f, r) => r.Password);

        public static readonly Faker<LoginDTO> LoginDTOFaker = new Faker<LoginDTO>()
            .RuleFor(r => r.Email, f => f.Person.Email)
            .RuleFor(r => r.Password, f => f.Internet.Password(8));

        public static readonly Faker<ProfileDTO> ProfileDTOFaker = new Faker<ProfileDTO>()
            .RuleFor(u => u.FirstName, f => f.Person.FirstName)
            .RuleFor(u => u.LastName, f => f.Person.LastName)
            .RuleFor(u => u.Email, f => f.Person.Email);

        public static readonly Faker<Trip> TripFaker = new Faker<Trip>()
            .RuleFor(u => u.StartDateTime, f => f.Date.Future(5))
            .RuleFor(u => u.EndDateTime, (f, trip) => trip.StartDateTime?.AddDays(f.Random.Int(2, 20)))
            .RuleFor(u => u.Name, (f, trip) => $"{f.Address.City()}, {trip.StartDateTime?.Year}");

        public static readonly Faker<SectionDTO> SectionDTOFaker = new Faker<SectionDTO>()
            .RuleFor(s => s.Name, f => f.Random.Word())
            .RuleFor(s => s.SectionType, f => f.PickRandom(typeof(Section).GetSubtypeNamesInSameAssembly()));
    }
}
