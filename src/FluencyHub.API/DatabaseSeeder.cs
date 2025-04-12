using FluencyHub.Domain.StudentManagement;
using FluencyHub.Domain.ContentManagement;
using FluencyHub.Domain.PaymentProcessing;
using FluencyHub.Infrastructure.Identity;
using FluencyHub.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FluencyHub.API;

public static class DatabaseSeeder
{
    public static async Task SeedData(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<Program>>();

        try
        {
            var context = services.GetRequiredService<FluencyHubDbContext>();
            if (context.Database.EnsureCreated())
            {
                logger.LogInformation("Domain database created successfully");
            }

            var identityContext = services.GetRequiredService<ApplicationDbContext>();
            if (identityContext.Database.EnsureCreated())
            {
                logger.LogInformation("Identity database created successfully");
            }

            await SeedRoles(services);
            await SeedUsers(services);
            await SeedStudents(services);
            await SeedCourses(services);
            await SeedLessons(services);
            await SeedEnrollments(services);
            await SeedPayments(services);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database");
        }
    }

    private static async Task SeedRoles(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var roles = new[] { "Administrator", "Student", "Teacher" };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }

    private static async Task SeedUsers(IServiceProvider services)
    {
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        // Admin user
        var adminEmail = "admin@fluencyhub.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = "Admin",
                LastName = "User",
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(adminUser, "Test@123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Administrator");
            }
        }

        // Teacher users
        var teacherEmails = new[]
        {
            "john.teacher@fluencyhub.com",
            "mary.teacher@fluencyhub.com"
        };

        foreach (var email in teacherEmails)
        {
            var teacherUser = await userManager.FindByEmailAsync(email);
            if (teacherUser == null)
            {
                var firstName = email.Split('@')[0].Split('.')[0];
                teacherUser = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    FirstName = char.ToUpper(firstName[0]) + firstName.Substring(1),
                    LastName = "Teacher",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(teacherUser, "Test@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(teacherUser, "Teacher");
                }
            }
        }

        // Student users
        var studentEmails = new[]
        {
            "maria.silva@fluencyhub.com",
            "joao.santos@fluencyhub.com",
            "ana.oliveira@fluencyhub.com",
            "pedro.souza@fluencyhub.com",
            "julia.costa@fluencyhub.com"
        };

        foreach (var email in studentEmails)
        {
            var studentUser = await userManager.FindByEmailAsync(email);
            if (studentUser == null)
            {
                var nameParts = email.Split('@')[0].Split('.');
                studentUser = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    FirstName = char.ToUpper(nameParts[0][0]) + nameParts[0].Substring(1),
                    LastName = char.ToUpper(nameParts[1][0]) + nameParts[1].Substring(1),
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(studentUser, "Test@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(studentUser, "Student");
                }
            }
        }
    }

    private static async Task SeedStudents(IServiceProvider services)
    {
        var studentRepository = services.GetRequiredService<FluencyHub.Application.Common.Interfaces.IStudentRepository>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        var studentEmails = new[]
        {
            "maria.silva@fluencyhub.com",
            "joao.santos@fluencyhub.com",
            "ana.oliveira@fluencyhub.com",
            "pedro.souza@fluencyhub.com",
            "julia.costa@fluencyhub.com"
        };

        foreach (var email in studentEmails)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user != null)
            {
                var existingStudent = await studentRepository.GetByEmailAsync(email);
                if (existingStudent == null)
                {
                    var student = new Student(
                        user.FirstName,
                        user.LastName,
                        user.Email,
                        DateTime.Now.AddYears(-20 - Array.IndexOf(studentEmails, email)),
                        $"+55119{new Random().Next(10000000, 99999999)}"
                    );

                    await studentRepository.AddAsync(student);
                }
            }
        }
        await studentRepository.SaveChangesAsync();
    }

    private static async Task SeedCourses(IServiceProvider services)
    {
        var context = services.GetRequiredService<FluencyHubDbContext>();
        
        if (!await context.Courses.AnyAsync())
        {
            var courses = new[]
            {
                new Course(
                    "Inglês Básico",
                    "Curso completo de inglês para iniciantes",
                    new CourseContent(
                        "Módulo 1: Introdução\nMódulo 2: Gramática Básica\nMódulo 3: Vocabulário\nMódulo 4: Conversação",
                        "Aprender vocabulário básico, Desenvolver conversação simples, Compreender textos básicos",
                        "Nenhum conhecimento prévio necessário",
                        "Iniciantes em inglês",
                        "Inglês",
                        "Básico"
                    ),
                    299.99m
                ),
                new Course(
                    "Inglês Intermediário",
                    "Aprofunde seus conhecimentos em inglês",
                    new CourseContent(
                        "Módulo 1: Revisão\nMódulo 2: Gramática Avançada\nMódulo 3: Vocabulário Expandido\nMódulo 4: Conversação Fluente\nMódulo 5: Escrita",
                        "Aperfeiçoar gramática, Expandir vocabulário, Melhorar fluência",
                        "Conhecimento básico de inglês, Ter completado o curso básico ou equivalente",
                        "Estudantes com conhecimento básico",
                        "Inglês",
                        "Intermediário"
                    ),
                    399.99m
                ),
                new Course(
                    "Inglês Avançado",
                    "Domine o inglês em nível avançado",
                    new CourseContent(
                        "Módulo 1: Expressões Idiomáticas\nMódulo 2: Negócios\nMódulo 3: Literatura\nMódulo 4: Cultura\nMódulo 5: Apresentações\nMódulo 6: Certificações",
                        "Atingir fluência nativa, Dominar expressões idiomáticas, Preparação para certificações",
                        "Nível intermediário de inglês, Boa base gramatical",
                        "Estudantes avançados",
                        "Inglês",
                        "Avançado"
                    ),
                    499.99m
                ),
                new Course(
                    "Inglês para Negócios",
                    "Inglês focado em ambiente corporativo",
                    new CourseContent(
                        "Módulo 1: Comunicação Empresarial\nMódulo 2: Reuniões\nMódulo 3: Apresentações\nMódulo 4: Negociações\nMódulo 5: E-mails\nMódulo 6: Relatórios",
                        "Vocabulário empresarial, Comunicação profissional, Negociações em inglês",
                        "Inglês intermediário, Experiência profissional",
                        "Profissionais e executivos",
                        "Inglês",
                        "Profissional"
                    ),
                    599.99m
                )
            };

            await context.Courses.AddRangeAsync(courses);
            await context.SaveChangesAsync();
        }
    }

    private static async Task SeedLessons(IServiceProvider services)
    {
        var context = services.GetRequiredService<FluencyHubDbContext>();
        
        if (!await context.Lessons.AnyAsync())
        {
            var courses = await context.Courses.ToListAsync();
            
            foreach (var course in courses)
            {
                var numberOfLessons = course.Name switch
                {
                    "Inglês Básico" => 20,
                    "Inglês Intermediário" => 25,
                    "Inglês Avançado" => 30,
                    "Inglês para Negócios" => 15,
                    _ => 10
                };

                for (int i = 1; i <= numberOfLessons; i++)
                {
                    var title = $"Lição {i} - {course.Name}";
                    var content = $"Conteúdo detalhado da lição {i} do curso {course.Name}";
                    var materialUrl = i % 3 == 0 ? $"https://materials.fluencyhub.com/{course.Name.ToLower()}/lesson{i}.pdf" : null;
                    
                    course.AddLesson(title, content, materialUrl);
                }
            }
            
            await context.SaveChangesAsync();
        }
    }

    private static async Task SeedEnrollments(IServiceProvider services)
    {
        var context = services.GetRequiredService<FluencyHubDbContext>();
        var studentRepository = services.GetRequiredService<FluencyHub.Application.Common.Interfaces.IStudentRepository>();

        if (!await context.Enrollments.AnyAsync())
        {
            var students = await studentRepository.GetAllAsync();
            var courses = await context.Courses.ToListAsync();
            var enrollments = new List<Enrollment>();

            foreach (var student in students)
            {
                // Cada aluno se matricula em 2-3 cursos aleatórios
                var numberOfCourses = new Random().Next(2, 4);
                var selectedCourses = courses.OrderBy(x => Guid.NewGuid()).Take(numberOfCourses);

                foreach (var course in selectedCourses)
                {
                    var enrollment = new Enrollment(
                        student.Id,
                        course.Id,
                        course.Price
                    );
                    enrollments.Add(enrollment);
                }
            }

            await context.Enrollments.AddRangeAsync(enrollments);
            await context.SaveChangesAsync();
        }
    }

    private static async Task SeedPayments(IServiceProvider services)
    {
        var context = services.GetRequiredService<FluencyHubDbContext>();

        if (!await context.Payments.AnyAsync())
        {
            var enrollments = await context.Enrollments
                .Include(e => e.Course)
                .ToListAsync();

            var payments = new List<Payment>();
            var random = new Random();

            foreach (var enrollment in enrollments)
            {
                var cardDetails = new CardDetails(
                    "John Doe",
                    "4532715326849421",
                    $"{random.Next(1, 13):00}",
                    $"{random.Next(2024, 2027)}"
                );

                var payment = new Payment(
                    enrollment.StudentId,
                    enrollment.Id,
                    enrollment.Course.Price,
                    cardDetails
                );

                // Simula diferentes status de pagamento
                var randomStatus = random.Next(1, 10);
                if (randomStatus <= 7) // 70% aprovados
                {
                    payment.MarkAsSuccess($"TXN_{Guid.NewGuid():N}");
                }
                else if (randomStatus == 8) // 10% pendentes
                {
                    // Mantém como pendente
                }
                else // 20% recusados
                {
                    payment.MarkAsFailed("Pagamento não autorizado pela operadora");
                }

                payments.Add(payment);
            }

            await context.Payments.AddRangeAsync(payments);
            await context.SaveChangesAsync();
        }
    }
} 