using System;
using System.Collections.Generic;
using System.Linq;
using IMS.Models;

namespace IMS.Helpers
{
    public static class QuizLoader
    {
        public static List<Question> GetQuestions(string experienceLevel, string designation)
        {
            // Normalize inputs
            string exp = (experienceLevel ?? "").ToLowerInvariant();
            string dom = (designation ?? "").ToLowerInvariant();

            // 🔹 Common basic programming questions
            var basicQuestions = new List<Question>
            {
                new Question { Text = "Which SQL command is used to retrieve data?",
                    Options = new[] { "SELECT", "INSERT", "DELETE", "UPDATE" }, CorrectOptionIndex = 0 },
                new Question { Text = "Which language not commonly uses OOP?",
                    Options = new[] { "C++", "Java", "C#", "C" }, CorrectOptionIndex = 3 },
                new Question { Text = "Which OOP principle allows method overriding?",
                    Options = new[] { "Encapsulation", "Polymorphism", "Abstraction", "Inheritance" }, CorrectOptionIndex = 1 },
                new Question { Text = "Which design pattern ensures only one instance of a class?",
                    Options = new[] { "Factory", "Observer", "Singleton", "Decorator" }, CorrectOptionIndex = 2 },
                new Question { Text = "Which of these is a value type in C#?",
                    Options = new[] { "string", "int", "object", "class" }, CorrectOptionIndex = 1 },
                new Question { Text = "Which symbol is used for comments in C#?",
                    Options = new[] { "//", "#", "/* */", "<!-- -->" }, CorrectOptionIndex = 0 }
            };

            // 🔹 Domain-specific banks
            var dotnetQuestions = new List<Question>
            {
                new Question { Text = ".NET CLR stands for?",
                    Options = new[] { "Common Language Runtime", "Core Language Runtime", "Central Logic Runtime", "Code Language Runtime" }, CorrectOptionIndex = 0 },
                new Question { Text = "Which keyword is used for async programming in C#?",
                    Options = new[] { "await", "async", "parallel", "thread" }, CorrectOptionIndex = 1 },
                new Question { Text = "Entity Framework is used for?",
                    Options = new[] { "UI Design", "Database ORM", "Testing", "Networking" }, CorrectOptionIndex = 1 },
                new Question { Text = "Which operator is used for null-coalescing in C#?",
                    Options = new[] { "??", "?:", "=>", "::" }, CorrectOptionIndex = 0 },
                new Question { Text = "Which LINQ method is used to filter data?",
                    Options = new[] { "Where()", "Select()", "OrderBy()", "GroupBy()" }, CorrectOptionIndex = 0 },
                new Question { Text = "Which access modifier makes a member visible only within its own class?",
                    Options = new[] { "public", "protected", "internal", "private" }, CorrectOptionIndex = 3 },
                new Question { Text = "Which collection is index-based in C#?",
                    Options = new[] { "Dictionary", "Queue", "List", "Stack" }, CorrectOptionIndex = 2 },
                new Question { Text = "Which attribute marks a method as test method in MSTest?",
                    Options = new[] { "[TestCase]", "[Test]", "[Fact]", "[TestMethod]" }, CorrectOptionIndex = 3 },
                new Question { Text = "Which of the following is NOT part of .NET Core?",
                    Options = new[] { "CLR", "JVM", "ASP.NET Core", "Entity Framework Core" }, CorrectOptionIndex = 1 },
                new Question { Text = "What is the root namespace in a C# project?",
                    Options = new[] { "System", "DefaultNamespace", "Global", "Root" }, CorrectOptionIndex = 2 }
            };

            var phpQuestions = new List<Question>
            {
                new Question { Text = "Which symbol is used to declare a variable in PHP?",
                    Options = new[] { "$", "@", "#", "%" }, CorrectOptionIndex = 0 },
                new Question { Text = "Which function is used to output text in PHP?",
                    Options = new[] { "print()", "echo", "console.log()", "printf()" }, CorrectOptionIndex = 1 },
                new Question { Text = "Which database is commonly used with PHP?",
                    Options = new[] { "MongoDB", "Oracle", "MySQL", "SQL Server" }, CorrectOptionIndex = 2 },
                new Question { Text = "Which PHP keyword is used to define a constant?",
                    Options = new[] { "define", "const", "constant", "set" }, CorrectOptionIndex = 0 },
                new Question { Text = "Which function is used to include another file in PHP?",
                    Options = new[] { "require()", "include()", "import()", "load()" }, CorrectOptionIndex = 1 },
                new Question { Text = "What is the correct way to start a PHP script?",
                    Options = new[] { "<php>", "<?php", "<?", "<script>" }, CorrectOptionIndex = 1 },
                new Question { Text = "Which superglobal is used to collect form data in PHP?",
                    Options = new[] { "$_POST", "$_FORM", "$_REQUEST", "$_DATA" }, CorrectOptionIndex = 0 },
                new Question { Text = "Which symbol is used for concatenation in PHP?",
                    Options = new[] { ".", "+", "&", "~" }, CorrectOptionIndex = 0 },
                new Question { Text = "Which function is used to get the length of a string in PHP?",
                    Options = new[] { "size()", "strlen()", "length()", "count()" }, CorrectOptionIndex = 1 },
                new Question { Text = "Which version introduced PHP 7?",
                    Options = new[] { "2012", "2015", "2017", "2019" }, CorrectOptionIndex = 1 }
            };

            var angularQuestions = new List<Question>
            {
                new Question { Text = "Angular is based on which programming language?",
                    Options = new[] { "Java", "C#", "TypeScript", "Python" }, CorrectOptionIndex = 2 },
                new Question { Text = "Which directive is used for looping in Angular templates?",
                    Options = new[] { "*ngIf", "*ngFor", "*ngSwitch", "*ngModel" }, CorrectOptionIndex = 1 },
                new Question { Text = "Which decorator is used to define a component in Angular?",
                    Options = new[] { "@Directive", "@NgModule", "@Component", "@Injectable" }, CorrectOptionIndex = 2 },
                new Question { Text = "What is Angular Router used for?",
                    Options = new[] { "Styling", "Navigation", "Forms", "HTTP calls" }, CorrectOptionIndex = 1 },
                new Question { Text = "Which file is the root module in Angular?",
                    Options = new[] { "main.ts", "index.html", "app.module.ts", "angular.json" }, CorrectOptionIndex = 2 },
                new Question { Text = "Which directive is used for two-way data binding?",
                    Options = new[] { "ngModel", "ngBind", "ngTwoWay", "ngData" }, CorrectOptionIndex = 0 },
                new Question { Text = "Which operator is used for safe navigation in Angular templates?",
                    Options = new[] { "?", "??", "?.", "::" }, CorrectOptionIndex = 2 },
                new Question { Text = "Which CLI command is used to serve the app?",
                    Options = new[] { "ng build", "ng serve", "npm serve", "ng run" }, CorrectOptionIndex = 1 },
                new Question { Text = "Which command creates a new Angular project?",
                    Options = new[] { "ng new", "npm start", "ng serve", "ng init" }, CorrectOptionIndex = 0 },
                new Question { Text = "What is data binding in Angular?",
                    Options = new[] { "Connecting HTML and CSS", "Connecting component and template", "Connecting modules", "Connecting directives" }, CorrectOptionIndex = 1 }
            };

            var qaQuestions = new List<Question>
            {
                new Question { Text = "Which of the following is NOT a type of software testing?",
                    Options = new[] { "Unit Testing", "Integration Testing", "System Testing", "Conversion Testing" }, CorrectOptionIndex = 3 },
                new Question { Text = "In automation testing, Selenium is mainly used for?",
                    Options = new[] { "Mobile apps", "Web apps", "API testing", "Desktop apps" }, CorrectOptionIndex = 1 },
                new Question { Text = "Which testing ensures that new code doesn’t break existing functionality?",
                    Options = new[] { "Smoke Testing", "Regression Testing", "Unit Testing", "System Testing" }, CorrectOptionIndex = 1 },
                new Question { Text = "Which tool is commonly used for bug tracking?",
                    Options = new[] { "GitHub", "JIRA", "Jenkins", "Postman" }, CorrectOptionIndex = 1 },
                new Question { Text = "Which testing checks the overall performance of an application?",
                    Options = new[] { "Load Testing", "Unit Testing", "System Testing", "Alpha Testing" }, CorrectOptionIndex = 0 },
                new Question { Text = "Which testing is performed first?",
                    Options = new[] { "Unit Testing", "Integration Testing", "System Testing", "Acceptance Testing" }, CorrectOptionIndex = 0 },
                new Question { Text = "Which type of testing is performed without planning and documentation?",
                    Options = new[] { "Ad-hoc Testing", "Unit Testing", "System Testing", "Regression Testing" }, CorrectOptionIndex = 0 },
                new Question { Text = "Which is a non-functional testing type?",
                    Options = new[] { "Performance Testing", "Unit Testing", "System Testing", "Integration Testing" }, CorrectOptionIndex = 0 },
                new Question { Text = "Which testing is performed by end users?",
                    Options = new[] { "System Testing", "Integration Testing", "Acceptance Testing", "Regression Testing" }, CorrectOptionIndex = 2 },
                new Question { Text = "What does QA stand for?",
                    Options = new[] { "Quick Access", "Quality Assurance", "Quality Adjustment", "Query Application" }, CorrectOptionIndex = 1 }
            };

            List<Question> domainQuestions = new List<Question>();
            if (dom.Contains("dotnet")) domainQuestions = dotnetQuestions;
            else if (dom.Contains("php")) domainQuestions = phpQuestions;
            else if (dom.Contains("angular")) domainQuestions = angularQuestions;
            else if (dom.Contains("qa")) domainQuestions = qaQuestions;

            // Decide how many from basic vs domain
            int basicCount, domainCount;
            if (exp.Contains("fresher")) { basicCount = 6; domainCount = 4; }
            else if (exp.Contains("less than 5")) { basicCount = 4; domainCount = 6; }
            else if (exp.Contains("less than 10")) { basicCount = 2; domainCount = 8; }
            else if (exp.Contains("less than 20")) { basicCount = 0; domainCount = 10; }
            else { basicCount = 5; domainCount = 5; }

            int totalNeeded = basicCount + domainCount;

            // Build result with uniqueness and fallback pool to always reach requested total
            var result = new List<Question>();
            var addedTexts = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            void TryAddQuestion(Question q)
            {
                if (q == null) return;
                string key = q.Text ?? Guid.NewGuid().ToString();
                if (!addedTexts.Contains(key) && result.Count < totalNeeded)
                {
                    result.Add(q);
                    addedTexts.Add(key);
                }
            }

            // Add primary picks
            foreach (var q in basicQuestions.Take(basicCount)) TryAddQuestion(q);
            foreach (var q in domainQuestions.Take(domainCount)) TryAddQuestion(q);

            // If still short, fill from a combined pool (avoid duplicates)
            if (result.Count < totalNeeded)
            {
                var pool = basicQuestions
                    .Concat(dotnetQuestions)
                    .Concat(phpQuestions)
                    .Concat(angularQuestions)
                    .Concat(qaQuestions)
                    .GroupBy(q => q.Text, StringComparer.OrdinalIgnoreCase)
                    .Select(g => g.First());

                foreach (var q in pool)
                {
                    TryAddQuestion(q);
                    if (result.Count >= totalNeeded) break;
                }
            }

            return result;
        }
    }
}