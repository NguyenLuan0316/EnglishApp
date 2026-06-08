using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using WordWave.Domain.Models;
using WordWave.Infrastructure.Data;

namespace WordWave.Api.Data;

public static class IeltsDataSeeder
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static async Task SeedIeltsDataAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var existing = await db.IeltsTests.CountAsync();
        if (existing >= 10)
        {
            return;
        }

        var createdAt = new DateTime(2026, 5, 18, 0, 0, 0, DateTimeKind.Utc);
        var tests = Enumerable.Range(existing + 1, 10 - existing)
            .Select(index => CreateTest(index, createdAt.AddMinutes(index)))
            .ToList();

        db.IeltsTests.AddRange(tests);
        await db.SaveChangesAsync();
    }

    private static IeltsTest CreateTest(int index, DateTime createdAt)
    {
        var data = CreateTestData(index);
        return new IeltsTest
        {
            Title = $"IELTS Academic Practice Test {index}",
            Description = "WordWave generated IELTS practice test with all four skills.",
            SourceType = "seed",
            SourceName = "WordWave IELTS generator",
            TestData = JsonSerializer.Serialize(data, JsonOptions),
            QuestionCount = 80,
            IsPublic = true,
            CreatedAt = createdAt
        };
    }

    private static object CreateTestData(int index)
    {
        var offset = index - 1;
        var speaking = SpeakingBanks[offset % SpeakingBanks.Length];

        return new
        {
            listening = new
            {
                parts = new[]
                {
                    WithQuestionNumbers(Pick(ListeningPart1, offset), "L", 1),
                    WithQuestionNumbers(Pick(ListeningPart2, offset / 2), "L", 11),
                    WithQuestionNumbers(Pick(ListeningPart3, offset / 3), "L", 21),
                    WithQuestionNumbers(Pick(ListeningPart4, offset / 4), "L", 31)
                }
            },
            reading = new
            {
                parts = new[]
                {
                    WithQuestionNumbers(Pick(ReadingPassage1, offset), "R", 1),
                    WithQuestionNumbers(Pick(ReadingPassage2, offset / 2), "R", 14),
                    WithQuestionNumbers(Pick(ReadingPassage3, offset / 3), "R", 27)
                }
            },
            writing = new
            {
                tasks = new[]
                {
                    WithWritingId(Pick(WritingTask1, offset), "W1"),
                    WithWritingId(Pick(WritingTask2, offset / 2), "W2")
                }
            },
            speaking = new
            {
                keyTerms = speaking.KeyTerms,
                parts = speaking.Parts.Select((part, partIndex) => new
                {
                    id = $"S{partIndex + 1}",
                    part.Title,
                    part.Time,
                    prompts = part.Prompts,
                    instruction = "",
                    transcript = "",
                    passage = Array.Empty<string>(),
                    questions = Array.Empty<object>()
                })
            }
        };
    }

    private static T Pick<T>(IReadOnlyList<T> list, int index) => list[index % list.Count];

    private static object WithQuestionNumbers(QuestionPartTemplate template, string prefix, int startNumber)
    {
        return new
        {
            id = $"{prefix}{startNumber}",
            template.Title,
            template.Instruction,
            template.Transcript,
            passage = template.Passage,
            time = "",
            prompts = Array.Empty<string>(),
            questions = template.Questions.Select((question, index) => new
            {
                id = $"{prefix}{startNumber + index}",
                number = startNumber + index,
                question.Type,
                question.Prompt,
                question.Options,
                answer = question.Answers
            })
        };
    }

    private static object WithWritingId(WritingTaskTemplate template, string id)
    {
        return new
        {
            id,
            template.Title,
            template.MinWords,
            template.Minutes,
            template.KeyTerms,
            template.Prompt
        };
    }

    private static QuestionTemplate Text(string prompt, params string[] answers) => new("text", prompt, [], answers);

    private static QuestionTemplate Choice(string prompt, string answer, params string[] options) => new("choice", prompt, options, [answer]);

    private static readonly QuestionPartTemplate[] ListeningPart1 =
    [
        new(
            "Part 1: Community Centre Course Booking",
            "Questions 1-10. Complete the notes with no more than two words and/or a number.",
            "You will hear a conversation at a community centre. The caller is Emma Clarke. Her membership number is C47291. The course begins next Tuesday and runs from six thirty to eight. The level is intermediate. The class meets in Studio 2. The teacher is Marco. The fee is eighty-five pounds for six weeks. Students should bring a water bottle. The website is northpark.org.",
            [],
            [
                Text("Full name:", "Emma Clarke"),
                Text("Membership number:", "C47291"),
                Text("Course begins on:", "Tuesday"),
                Text("Time:", "6.30", "6:30", "six thirty"),
                Text("Level:", "intermediate"),
                Text("Room:", "Studio 2"),
                Text("Teacher:", "Marco"),
                Text("Fee:", "85", "eighty five", "eighty-five"),
                Text("Students should bring a:", "water bottle"),
                Text("Website:", "northpark.org")
            ]
        ),
        new(
            "Part 1: Hotel Reservation",
            "Questions 1-10. Complete the booking form.",
            "A guest is booking a room at the Riverside Hotel. His name is Daniel Morris. He wants to arrive on April 18th and stay for three nights. He chooses a double room with a river view. Breakfast is included. His phone number is 07935 884210. He asks for parking. The total deposit is 60 pounds. The confirmation will be sent by email. He plans to check in after seven pm.",
            [],
            [
                Text("Guest name:", "Daniel Morris"),
                Text("Arrival date:", "April 18th", "18 April", "April 18"),
                Text("Length of stay:", "three nights", "3 nights"),
                Text("Room type:", "double room"),
                Text("Room view:", "river view"),
                Text("Meal included:", "breakfast"),
                Text("Phone number:", "07935 884210", "07935884210"),
                Text("Extra service requested:", "parking"),
                Text("Deposit:", "60", "sixty pounds", "60 pounds"),
                Text("Check-in time:", "after 7 pm", "after seven pm", "7 pm")
            ]
        ),
        new(
            "Part 1: Clinic Appointment",
            "Questions 1-10. Complete the appointment notes.",
            "A patient is calling Oak Street Clinic. The patient is Priya Shah. Her date of birth is 12 June 1998. She has a sore throat and a mild fever. The receptionist offers an appointment on Thursday at eleven fifteen. The doctor is Dr Bennett. The clinic is on the third floor. Priya should bring her insurance card. The appointment fee is 25 pounds. The nearest bus stop is Market Lane.",
            [],
            [
                Text("Patient name:", "Priya Shah"),
                Text("Date of birth:", "12 June 1998", "June 12 1998"),
                Text("Main problem:", "sore throat"),
                Text("Second symptom:", "mild fever"),
                Text("Appointment day:", "Thursday"),
                Text("Appointment time:", "11.15", "11:15", "eleven fifteen"),
                Text("Doctor:", "Dr Bennett", "Bennett"),
                Text("Clinic location:", "third floor"),
                Text("Document to bring:", "insurance card"),
                Text("Nearest bus stop:", "Market Lane")
            ]
        )
    ];

    private static readonly QuestionPartTemplate[] ListeningPart2 =
    [
        new(
            "Part 2: Library Orientation",
            "Questions 11-20. Answer the questions using no more than three words and/or a number.",
            "Welcome to Greenford Library. For quick help by phone, use extension 204. The science collection is on the second floor. On Sundays, the library opens at 10 am. Group study rooms should be booked online. In the media room you can borrow headsets. The magazines were moved nearer the cafe. The quietest desks are in the north wing. Database help is available at the research desk. This Friday there is a workshop on referencing. Membership forms must be completed by Friday.",
            [],
            [
                Text("Phone extension for quick help:", "204"),
                Text("Science collection location:", "second floor"),
                Text("Sunday opening time:", "10 am", "10:00 am", "10"),
                Choice("Group study rooms should be booked:", "online", "at the front desk", "online", "by email"),
                Text("Equipment available in the media room:", "headsets"),
                Text("Recently moved items:", "magazines"),
                Text("Quietest area:", "north wing"),
                Text("Place for database help:", "research desk"),
                Text("Workshop topic:", "referencing"),
                Text("Membership form deadline:", "Friday")
            ]
        ),
        new(
            "Part 2: Museum Volunteer Briefing",
            "Questions 11-20. Complete the notes.",
            "The City Museum needs weekend volunteers. The main exhibition this month is about local transport. Volunteers should meet in Gallery 4 at nine fifteen. Their first task is to welcome school groups. They must wear a blue badge. Lunch is served in the staff room. The emergency exit is behind the bookshop. A short training video is available online. The most popular item is an old tram ticket machine. Volunteers should report broken displays to security.",
            [],
            [
                Text("Main exhibition topic:", "local transport"),
                Text("Meeting place:", "Gallery 4"),
                Text("Meeting time:", "9.15", "9:15", "nine fifteen"),
                Text("First task:", "welcome school groups"),
                Text("Badge colour:", "blue", "blue badge"),
                Text("Lunch location:", "staff room"),
                Text("Emergency exit location:", "behind the bookshop"),
                Text("Training video location:", "online"),
                Text("Most popular item:", "ticket machine"),
                Text("Broken displays should be reported to:", "security")
            ]
        )
    ];

    private static readonly QuestionPartTemplate[] ListeningPart3 =
    [
        new(
            "Part 3: Student Project Discussion",
            "Questions 21-30. Complete the notes.",
            "Two students are planning a research project. Their topic will be urban gardens. They changed the survey method because the first version had a low response rate. Their sample group will be commuters. For analysis, they will use a spreadsheet. The consultant meeting has moved to Wednesday. Several photos cannot be included because of copyright. The final report will focus on recommendations. Sofia will handle the budget. Amir will write the background. Their lecturer liked the clear timeline. The final report must be submitted by midnight.",
            [],
            [
                Text("Research topic:", "urban gardens"),
                Text("Reason for changing method:", "low response rate"),
                Text("Sample group:", "commuters"),
                Text("Analysis tool:", "spreadsheet"),
                Text("Consultant meeting day:", "Wednesday"),
                Text("Problem with some photos:", "copyright"),
                Text("Final report section focus:", "recommendations"),
                Text("Sofia is responsible for:", "budget"),
                Text("Lecturer liked the:", "clear timeline"),
                Text("Submission time:", "midnight")
            ]
        ),
        new(
            "Part 3: Product Design Seminar",
            "Questions 21-30. Complete the notes.",
            "Mina and Robert are discussing a product design seminar. Their case study is a folding bicycle. The strongest feature is portability. They need to add user interviews because the tutor said the evidence was too narrow. The prototype is made from aluminium. Testing will happen in the sports hall on Monday. Robert will prepare the slides. Mina will write the conclusion. The biggest risk is cost. They will compare the bicycle with electric scooters. Their presentation should last twelve minutes.",
            [],
            [
                Text("Case study product:", "folding bicycle"),
                Text("Strongest feature:", "portability"),
                Text("Extra evidence needed:", "user interviews"),
                Text("Prototype material:", "aluminium"),
                Text("Testing location:", "sports hall"),
                Text("Testing day:", "Monday"),
                Text("Robert will prepare:", "slides"),
                Text("Mina will write the:", "conclusion"),
                Text("Biggest risk:", "cost"),
                Text("Presentation length:", "12 minutes", "twelve minutes")
            ]
        )
    ];

    private static readonly QuestionPartTemplate[] ListeningPart4 =
    [
        new(
            "Part 4: Lecture on Renewable Energy",
            "Questions 31-40. Complete the summary.",
            "This lecture looks at renewable energy in coastal regions. The main source today is tidal power. An early research site was the Bay of Fundy. Modern turbines often use carbon fibre because it is strong and light. One environmental concern is fish migration. Energy can be stored in batteries. Swansea is often discussed in relation to tidal lagoon projects. Installation costs have fallen by 18 percent. Government support often comes through tax credits. The future challenge is maintenance. Mixed systems are more reliable than any single source.",
            [],
            [
                Text("Main energy source:", "tidal power"),
                Text("Early research site:", "Bay of Fundy"),
                Text("Turbine material:", "carbon fibre", "carbon fiber"),
                Text("Environmental concern:", "fish migration"),
                Text("Storage method:", "batteries"),
                Text("City example:", "Swansea"),
                Text("Cost reduction:", "18 percent", "18%"),
                Text("Policy support:", "tax credits"),
                Text("Future challenge:", "maintenance"),
                Text("Best long-term approach:", "mixed systems")
            ]
        ),
        new(
            "Part 4: Lecture on Urban Trees",
            "Questions 31-40. Complete the summary.",
            "The lecture discusses urban trees and public health. The main benefit is cooling. Trees reduce surface temperatures by creating shade. A useful species in dry areas is the plane tree. The biggest threat is soil compaction. Sensors can measure moisture levels. Copenhagen is given as a successful example. Trees near busy roads can trap pollution. Maintenance plans should include pruning. Community groups often help with watering. The lecturer concludes that tree networks are more effective than isolated planting.",
            [],
            [
                Text("Main public health benefit:", "cooling"),
                Text("Trees reduce surface temperatures by creating:", "shade"),
                Text("Useful species in dry areas:", "plane tree"),
                Text("Biggest threat:", "soil compaction"),
                Text("Sensors measure:", "moisture levels"),
                Text("Successful city example:", "Copenhagen"),
                Text("Trees near busy roads can trap:", "pollution"),
                Text("Maintenance activity:", "pruning"),
                Text("Community groups help with:", "watering"),
                Text("Most effective planting pattern:", "tree networks")
            ]
        )
    ];

    private static readonly QuestionPartTemplate[] ReadingPassage1 =
    [
        new(
            "Passage 1: Urban Rooftop Farms",
            "Questions 1-13. Read the passage and answer the questions.",
            "",
            [
                "A. Rooftop farming has moved from novelty to practical urban planning tool. In dense cities, unused roofs can become productive spaces, but the best projects begin with careful structural checks.",
                "B. A roof has a distinct microclimate. It may be warmer, windier and drier than the street below. Successful farms use light soil, shade cloth and drip irrigation to protect plants.",
                "C. The social value of rooftop farms is often as important as the harvest. Some buildings add beehives, compost points and weekend workshops.",
                "D. However, rooftop agriculture is not free from problems. Insurance can be expensive, and restaurants often buy delicate herbs because they receive them within hours of picking."
            ],
            [
                Choice("Rooftop farming is still mainly treated as a novelty.", "FALSE", "TRUE", "FALSE", "NOT GIVEN"),
                Choice("Structural checks should happen before planting begins.", "TRUE", "TRUE", "FALSE", "NOT GIVEN"),
                Choice("Rooftop farms always produce more food than street-level farms.", "NOT GIVEN", "TRUE", "FALSE", "NOT GIVEN"),
                Choice("Shade cloth can help protect rooftop crops.", "TRUE", "TRUE", "FALSE", "NOT GIVEN"),
                Choice("Most volunteers are professional gardeners.", "NOT GIVEN", "TRUE", "FALSE", "NOT GIVEN"),
                Choice("Restaurants may buy herbs from rooftop farms because delivery is fast.", "TRUE", "TRUE", "FALSE", "NOT GIVEN"),
                Text("A roof creates a special ______ for plants.", "microclimate"),
                Text("Some farms use light ______.", "soil"),
                Text("Some projects include ______ for pollination and education.", "beehives"),
                Text("Weekend activities may include ______.", "workshops"),
                Text("One financial difficulty is ______.", "insurance"),
                Text("Rooftop agriculture can supply delicate ______.", "herbs"),
                Text("One customer group is ______.", "restaurants")
            ]
        ),
        new(
            "Passage 1: Repair Cafes",
            "Questions 1-13. Read the passage and answer the questions.",
            "",
            [
                "A. Repair cafes are community events where volunteers help people fix household items. The idea began as a response to waste, but it has become a way to share practical knowledge.",
                "B. The most common repairs involve lamps, small kitchen devices and clothing. Tools are usually donated, while spare parts are bought through a small fund.",
                "C. Organisers say the social benefits are as valuable as the environmental ones. Older residents often bring specialist skills, and students learn how products are assembled.",
                "D. Repair cafes cannot solve every problem. Dangerous electrical items are refused, and some modern products are sealed shut."
            ],
            [
                Choice("Repair cafes only focus on environmental goals.", "FALSE", "TRUE", "FALSE", "NOT GIVEN"),
                Choice("Visitors are usually encouraged to book a table.", "NOT GIVEN", "TRUE", "FALSE", "NOT GIVEN"),
                Choice("All repairs are completed by professional engineers.", "NOT GIVEN", "TRUE", "FALSE", "NOT GIVEN"),
                Choice("Small kitchen devices are commonly repaired.", "TRUE", "TRUE", "FALSE", "NOT GIVEN"),
                Choice("Students can learn how products are assembled.", "TRUE", "TRUE", "FALSE", "NOT GIVEN"),
                Choice("Dangerous electrical items are accepted.", "FALSE", "TRUE", "FALSE", "NOT GIVEN"),
                Text("Repair cafes help reduce ______.", "waste"),
                Text("Tools are often ______.", "donated"),
                Text("Spare parts are paid for through a small ______.", "fund"),
                Text("Older residents may bring specialist ______.", "skills"),
                Text("Common repairs include lamps and ______.", "clothing"),
                Text("Some products are difficult because they are ______ shut.", "sealed"),
                Text("Repair cafes share practical ______.", "knowledge")
            ]
        )
    ];

    private static readonly QuestionPartTemplate[] ReadingPassage2 =
    [
        new(
            "Passage 2: The Science of Sleep and Memory",
            "Questions 14-26. Choose the correct answer or complete the notes.",
            "",
            [
                "A. Sleep is not a passive state. During the night, the brain sorts recent experiences and decides which details should remain available.",
                "B. Deep sleep appears to strengthen factual memories. Slow waves pass across the cortex, and information is repeatedly reactivated.",
                "C. Rapid eye movement sleep is linked with emotional learning and flexible problem solving.",
                "D. Short naps can help, especially for shift workers, but they can lead to overconfidence.",
                "E. Many sleep devices estimate sleep stages from movement. Experts still recommend a consistent schedule, a cool room of about 18 degrees and reduced caffeine."
            ],
            [
                Choice("Paragraph A mainly explains that sleep:", "sorts recent experiences", "sorts recent experiences", "prevents all forgetting", "is fully understood"),
                Choice("Paragraph B focuses on:", "deep sleep and facts", "dream reports", "deep sleep and facts", "exercise routines"),
                Choice("Paragraph C links REM sleep with:", "emotional learning", "emotional learning", "physical growth", "hunger control"),
                Choice("Paragraph D says naps are useful but:", "can create overconfidence", "can create overconfidence", "should last all afternoon", "are harmful for shift workers"),
                Choice("Paragraph E warns that devices estimate sleep from:", "movement", "movement", "blood pressure", "temperature"),
                Text("Caffeine ______ sleep.", "delays"),
                Text("The recommended room temperature is about ______.", "18 degrees", "18"),
                Text("Many devices estimate sleep stages from ______.", "movement"),
                Text("Deep sleep includes electrical patterns called ______.", "slow waves"),
                Text("Naps may be especially useful for ______.", "shift workers"),
                Text("A possible problem after naps is ______.", "overconfidence"),
                Text("Experts still recommend keeping a ______.", "consistent schedule"),
                Text("Sleep supports memory ______.", "consolidation")
            ]
        ),
        new(
            "Passage 2: Digital Maps and Navigation",
            "Questions 14-26. Choose the correct answer or complete the notes.",
            "",
            [
                "A. Digital maps combine satellite positioning, road data and user reports to suggest routes.",
                "B. Navigation apps often send drivers through residential streets when main roads are crowded.",
                "C. Pedestrians need landmarks, crossings and information about public spaces.",
                "D. Airports, hospitals and universities are testing indoor mapping systems.",
                "E. Experts say digital maps should be treated as advice, not authority."
            ],
            [
                Choice("Paragraph A mainly presents digital maps as tools for:", "route planning", "route planning", "language learning", "weather prediction"),
                Choice("Paragraph B mentions complaints about:", "noise and safety", "noise and safety", "ticket prices", "poor lighting"),
                Choice("Paragraph C says pedestrians benefit from:", "visible landmarks", "visible landmarks", "faster engines", "private roads"),
                Choice("Paragraph D focuses on:", "indoor mapping", "indoor mapping", "online shopping", "airport design history"),
                Choice("Paragraph E says maps should be treated as:", "advice", "advice", "law", "entertainment"),
                Text("Digital maps combine positioning, road data and user ______.", "reports"),
                Text("Their main advantage is ______.", "speed"),
                Text("Drivers may be sent through ______ streets.", "residential"),
                Text("Pedestrian instructions may mention visible ______.", "objects", "landmarks"),
                Text("Indoor systems are tested in airports, hospitals and ______.", "universities"),
                Text("Indoor accuracy is difficult because satellite signals are ______.", "weak"),
                Text("Cyclists still need user ______.", "judgement"),
                Text("Maps should not be treated as ______.", "authority")
            ]
        )
    ];

    private static readonly QuestionPartTemplate[] ReadingPassage3 =
    [
        new(
            "Passage 3: Materials That Repair Themselves",
            "Questions 27-40. Complete the notes and answer TRUE, FALSE or NOT GIVEN.",
            "",
            [
                "A. Self-healing materials were first developed for aerospace components. Early systems placed tiny microcapsules inside a polymer.",
                "B. Later designs copied natural repair systems. Some polymers form new links when heated or exposed to ultraviolet light.",
                "C. The technology could lower maintenance costs, but large gaps still require engineers and cost is a barrier.",
                "D. Researchers see self-healing materials as a complement to traditional maintenance."
            ],
            [
                Text("The first self-healing materials were developed for ______.", "aerospace"),
                Text("Early capsules contained ______.", "resin"),
                Text("Cracks broke the tiny ______.", "microcapsules"),
                Text("Some polymers are compared to ______.", "vines"),
                Text("A limitation is that ______ still need engineers.", "large gaps"),
                Text("The technology could reduce ______ costs.", "maintenance"),
                Text("Another barrier is ______.", "cost"),
                Choice("Laboratory performance is always the same as field performance.", "FALSE", "TRUE", "FALSE", "NOT GIVEN"),
                Choice("Some polymers respond to ultraviolet light.", "TRUE", "TRUE", "FALSE", "NOT GIVEN"),
                Choice("All self-healing products are already universal.", "FALSE", "TRUE", "FALSE", "NOT GIVEN"),
                Choice("Large gaps may still require engineers.", "TRUE", "TRUE", "FALSE", "NOT GIVEN"),
                Choice("Self-healing materials remove all inspection needs.", "FALSE", "TRUE", "FALSE", "NOT GIVEN"),
                Text("Researchers describe the materials as a ______ to traditional maintenance.", "complement"),
                Text("One possible future application is ______.", "bridges")
            ]
        ),
        new(
            "Passage 3: Public Clocks and City Time",
            "Questions 27-40. Complete the notes and answer TRUE, FALSE or NOT GIVEN.",
            "",
            [
                "A. Before personal watches became common, public clocks organised urban life.",
                "B. Accuracy improved when cities adopted standard time. Telegraph networks allowed time signals to travel quickly.",
                "C. Public clocks shaped behaviour. Workers could be fined for arriving late.",
                "D. Many public clocks lost importance when people carried watches and later phones, but restored clocks remain landmarks."
            ],
            [
                Text("Before personal watches, public clocks organised ______ life.", "urban"),
                Text("Railway stations depended on shared time ______.", "signals"),
                Text("Clock towers could symbolise civic ______.", "pride"),
                Text("Accuracy improved after cities adopted ______ time.", "standard"),
                Text("Earlier towns set noon by the ______.", "sun"),
                Text("Time signals travelled through ______ networks.", "telegraph"),
                Text("Workers could be fined for arriving ______.", "late"),
                Choice("Public clocks were only decorative.", "FALSE", "TRUE", "FALSE", "NOT GIVEN"),
                Choice("Standard time was helpful for railways.", "TRUE", "TRUE", "FALSE", "NOT GIVEN"),
                Choice("All critics supported mechanical time.", "FALSE", "TRUE", "FALSE", "NOT GIVEN"),
                Choice("Phones contributed to the reduced importance of public clocks.", "TRUE", "TRUE", "FALSE", "NOT GIVEN"),
                Choice("Restored clocks are never used during festivals.", "FALSE", "TRUE", "FALSE", "NOT GIVEN"),
                Text("Restored clocks remain popular ______.", "landmarks"),
                Text("Some cities use clocks as meeting ______.", "points")
            ]
        )
    ];

    private static readonly WritingTaskTemplate[] WritingTask1 =
    [
        new("Task 1", 150, 20, ["increase", "decrease", "percentage", "commuters", "metro", "bus", "car", "bicycle", "walking"], "The chart below shows the percentage of commuters using five types of transport in a city in 2010 and 2025. Data: bus 38% to 29%, metro 22% to 31%, bicycle 6% to 15%, car 30% to 20%, walking 4% to 5%."),
        new("Task 1", 150, 20, ["library", "visitors", "adults", "children", "increase", "decline", "weekend"], "The table below shows the number of visitors to a public library in 2012 and 2022 by age group. Data: children 18,000 to 31,000; teenagers 14,500 to 12,000; adults 46,000 to 52,500; seniors 9,000 to 17,500."),
        new("Task 1", 150, 20, ["energy", "solar", "wind", "coal", "gas", "renewable", "proportion"], "The pie charts compare the sources of electricity in a country in 2005 and 2025. Data 2005: coal 45%, gas 30%, hydro 15%, wind 5%, solar 5%. Data 2025: coal 22%, gas 24%, hydro 14%, wind 20%, solar 20%.")
    ];

    private static readonly WritingTaskTemplate[] WritingTask2 =
    [
        new("Task 2", 250, 40, ["university", "employability", "education", "knowledge", "critical", "thinking", "opinion"], "Some people believe universities should focus on employability, while others think higher education should develop broader knowledge and critical thinking. Discuss both views and give your own opinion."),
        new("Task 2", 250, 40, ["remote", "work", "productivity", "office", "employees", "companies", "balance"], "In many countries, more employees are working remotely. Some people think this improves productivity and work-life balance, while others believe it weakens teamwork. Discuss both views and give your own opinion."),
        new("Task 2", 250, 40, ["environment", "government", "individuals", "responsibility", "pollution", "policy", "behaviour"], "Some people think environmental problems should be solved mainly by governments, while others believe individuals must change their behaviour. Discuss both views and give your own opinion.")
    ];

    private static readonly SpeakingTemplate[] SpeakingBanks =
    [
        new(
            ["city", "transport", "community", "government", "technology", "planning"],
            [
                new("Part 1: Interview", "4-5 minutes", ["Do you work or study?", "What part of your daily routine do you enjoy most?", "How often do you use public transport?", "Do you prefer studying alone or with other people?"]),
                new("Part 2: Long Turn", "1 minute preparation, 2 minutes speaking", ["Describe a place in your city that you think should be improved.", "You should say where it is, what problems it has, how it could be improved, and why these changes would matter."]),
                new("Part 3: Discussion", "4-5 minutes", ["What makes a city a good place to live?", "Should governments prioritise public transport over roads?", "How can local communities influence urban planning?", "Do you think technology will solve most city problems?"])
            ]
        ),
        new(
            ["skill", "learning", "teacher", "practice", "online", "motivation"],
            [
                new("Part 1: Interview", "4-5 minutes", ["What skill would you like to learn in the future?", "Do you prefer learning online or in a classroom?", "How do you usually stay motivated?", "Did you learn many practical skills at school?"]),
                new("Part 2: Long Turn", "1 minute preparation, 2 minutes speaking", ["Describe a skill that was difficult for you to learn.", "You should say what the skill was, why it was difficult, how you practised it, and how you felt when you improved."]),
                new("Part 3: Discussion", "4-5 minutes", ["What skills are most important for young people today?", "Can online learning replace teachers?", "Why do some adults stop learning new skills?", "Should employers pay for staff training?"])
            ]
        ),
        new(
            ["park", "public", "space", "community", "design", "families"],
            [
                new("Part 1: Interview", "4-5 minutes", ["Do you often visit parks or public squares?", "What do people usually do in public spaces in your area?", "Do you prefer quiet or busy places?", "Are there enough green spaces where you live?"]),
                new("Part 2: Long Turn", "1 minute preparation, 2 minutes speaking", ["Describe a public place that you enjoy visiting.", "You should say where it is, what it looks like, what people do there, and why you like it."]),
                new("Part 3: Discussion", "4-5 minutes", ["Why are public spaces important in cities?", "Who should pay for maintaining parks?", "How can public spaces be safer for children?", "Do modern cities provide enough places for people to meet?"])
            ]
        )
    ];

    private sealed record QuestionPartTemplate(string Title, string Instruction, string Transcript, string[] Passage, QuestionTemplate[] Questions);
    private sealed record QuestionTemplate(string Type, string Prompt, string[] Options, string[] Answers);
    private sealed record WritingTaskTemplate(string Title, int MinWords, int Minutes, string[] KeyTerms, string Prompt);
    private sealed record SpeakingTemplate(string[] KeyTerms, SpeakingPartTemplate[] Parts);
    private sealed record SpeakingPartTemplate(string Title, string Time, string[] Prompts);
}
