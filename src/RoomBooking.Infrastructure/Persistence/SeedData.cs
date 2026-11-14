using Microsoft.EntityFrameworkCore;
using RoomBooking.Domain.Entities;
using RoomBooking.Domain.Enums;

namespace RoomBooking.Infrastructure.Persistence;

internal static class SeedData
{
    private static readonly DateTime Stamp = new(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public static readonly Guid StudentId = Id(1, 1);
    public static readonly Guid TeacherId = Id(1, 2);
    public static readonly Guid AdminId = Id(1, 3);

    private static Guid Id(int prefix, int n) => new($"{prefix}0000000-0000-0000-0000-{n:D12}");

    private static List<UserRole> Everyone() => new() { UserRole.Student, UserRole.Teacher, UserRole.Admin };
    private static List<UserRole> Staff() => new() { UserRole.Teacher, UserRole.Admin };

    public static void Apply(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().HasData(
            new User
            {
                Id = StudentId,
                FullName = "Deniz Akhmadjanov",
                Email = "student@roombooking.kz",
                PasswordHash = "SEED_HASH_PLACEHOLDER",
                Role = UserRole.Student,
                CreatedAtUtc = Stamp
            },
            new User
            {
                Id = TeacherId,
                FullName = "Tr. Adilet",
                Email = "teacher@roombooking.kz",
                PasswordHash = "SEED_HASH_PLACEHOLDER",
                Role = UserRole.Teacher,
                CreatedAtUtc = Stamp
            },
            new User
            {
                Id = AdminId,
                FullName = "System Administrator",
                Email = "admin@roombooking.kz",
                PasswordHash = "SEED_HASH_PLACEHOLDER",
                Role = UserRole.Admin,
                CreatedAtUtc = Stamp
            });

        var equipmentNames = new[]
        {
            "Projector", "Microphone", "Whiteboard", "PC", "Smart TV",
            "High-speed Internet", "Wi-Fi", "Power Outlets at Every Desk", "Coffee Point",
            "Soundproofing", "External Monitor", "Video Conferencing System", "Flipchart"
        };

        modelBuilder.Entity<Equipment>().HasData(
            equipmentNames.Select((name, i) => new Equipment
            {
                Id = Id(2, i + 1),
                Name = name,
                CreatedAtUtc = Stamp
            }).ToArray());

        modelBuilder.Entity<Space>().HasData(
            // University Campus
            new Space
            {
                Id = Id(3, 1),
                Name = "Room C1.2.235 (Lecture Hall)",
                Location = "Campus, Building C1, 2nd floor [Free]",
                Category = SpaceCategory.LectureHall,
                Capacity = 60,
                Description = "University lecture hall equipped with a projector and microphone.",
                AllowedRoles = Staff(),
                CreatedAtUtc = Stamp
            },
            new Space
            {
                Id = Id(3, 2),
                Name = "Computer Lab C1.1.312 (Dev Lab)",
                Location = "Campus, Building C1, 1st floor [Free]",
                Category = SpaceCategory.ComputerLab,
                Capacity = 25,
                Description = "Lab for software development and practical classes.",
                AllowedRoles = Everyone(),
                CreatedAtUtc = Stamp
            },

            // Astana Public Spaces & Coworking Hubs
            new Space
            {
                Id = Id(3, 3),
                Name = "National Academic Library of RK",
                Location = "11 Dostyk St, Astana [Free]",
                Category = SpaceCategory.CoworkingOpenSpace,
                Capacity = 80,
                Description = "Free reading halls and quiet study zones for students and researchers.",
                AllowedRoles = Everyone(),
                CreatedAtUtc = Stamp
            },
            new Space
            {
                Id = Id(3, 4),
                Name = "Forte Kulanshi Art Space",
                Location = "8/1 Dostyk St (ForteBank), Astana [Free]",
                Category = SpaceCategory.MeetingRoom,
                Capacity = 30,
                Description = "Free creative and exhibition space for meetings and presentations.",
                AllowedRoles = Everyone(),
                CreatedAtUtc = Stamp
            },
            new Space
            {
                Id = Id(3, 5),
                Name = "Yourt Community Centers",
                Location = "Youth Hubs, Astana [Free]",
                Category = SpaceCategory.CoworkingOpenSpace,
                Capacity = 35,
                Description = "Free community spaces for students and project teams (subject to branch terms).",
                AllowedRoles = Everyone(),
                CreatedAtUtc = Stamp
            },
            new Space
            {
                Id = Id(3, 6),
                Name = "Astana Hub — Open Space Zone",
                Location = "55/8 Mangilik El Ave (EXPO C4.6), Astana [Free / Paid]",
                Category = SpaceCategory.CoworkingOpenSpace,
                Capacity = 50,
                Description = "Free for tech park program participants, paid access for guests.",
                AllowedRoles = Everyone(),
                CreatedAtUtc = Stamp
            },
            new Space
            {
                Id = Id(3, 7),
                Name = "IQ Coworking",
                Location = "Left Bank, Astana [Paid]",
                Category = SpaceCategory.MeetingRoom,
                Capacity = 16,
                Description = "Paid business coworking hub with meeting rooms and workspaces.",
                AllowedRoles = Everyone(),
                CreatedAtUtc = Stamp
            },
            new Space
            {
                Id = Id(3, 8),
                Name = "iHUB",
                Location = "Business Center, Astana [Paid]",
                Category = SpaceCategory.CoworkingOpenSpace,
                Capacity = 30,
                Description = "Paid IT coworking space with 24/7 access and meeting rooms.",
                AllowedRoles = Everyone(),
                CreatedAtUtc = Stamp
            },
            new Space
            {
                Id = Id(3, 9),
                Name = "Coworking TEAM",
                Location = "Business Center, Astana [Paid]",
                Category = SpaceCategory.CoworkingOpenSpace,
                Capacity = 24,
                Description = "Paid coworking space for teamwork, workshops, and meetups.",
                AllowedRoles = Everyone(),
                CreatedAtUtc = Stamp
            });

        var links = new (int Space, int[] Equipment)[]
        {
            (1, new[] { 1, 2, 3 }),          // Lecture Hall
            (2, new[] { 4, 5, 6 }),          // Computer Lab
            (3, new[] { 7, 8, 10 }),         // National Academic Library of RK
            (4, new[] { 1, 7, 13 }),         // Forte Kulanshi Art Space
            (5, new[] { 3, 7, 8 }),          // Yourt Community Centers
            (6, new[] { 6, 8, 9 }),          // Astana Hub
            (7, new[] { 8, 9, 12 }),         // IQ Coworking
            (8, new[] { 6, 8, 11 }),         // iHUB
            (9, new[] { 1, 3, 9, 13 })       // Coworking TEAM
        };

        modelBuilder.Entity("SpaceEquipment").HasData(
            links.SelectMany(l => l.Equipment.Select(e => new
            {
                SpaceId = Id(3, l.Space),
                EquipmentId = Id(2, e)
            })).Cast<object>().ToArray());
    }
}