using Microsoft.EntityFrameworkCore;
using WordWave.Domain.Models;
using WordWave.Infrastructure.Data;

namespace WordWave.Api.Data;

public static class GrammarDataSeeder
{
    public static async Task SeedGrammarDataAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var createdAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var existingLessons = await db.GrammarLessons
            .Include(x => x.GrammarExamples)
            .ToListAsync();
        var existingByTitle = existingLessons.ToDictionary(x => x.Title, StringComparer.OrdinalIgnoreCase);
        var newLessons = new List<GrammarLesson>();

        foreach (var item in SeedLessons)
        {
            if (existingByTitle.TryGetValue(item.Title, out var lesson))
            {
                lesson.Level = item.Level;
                lesson.Description = item.Description;
                lesson.Formula = item.Formula;
                lesson.Tips = item.Tips;

                var example = lesson.GrammarExamples.FirstOrDefault();
                if (example is null)
                {
                    lesson.GrammarExamples.Add(new GrammarExample
                    {
                        En = item.ExampleEn,
                        Vi = ToVietnamese(item.ExampleVi)
                    });
                }
                else
                {
                    example.En = item.ExampleEn;
                    example.Vi = ToVietnamese(item.ExampleVi);
                }

                continue;
            }

            newLessons.Add(NewLesson(item));
        }

        if (newLessons.Count > 0)
        {
            await db.GrammarLessons.AddRangeAsync(newLessons);
        }

        await db.SaveChangesAsync();

        GrammarLesson NewLesson(SeedLesson item)
        {
            return new GrammarLesson
            {
                Title = item.Title,
                Level = item.Level,
                Description = item.Description,
                Formula = item.Formula,
                Tips = item.Tips,
                CreatedAt = createdAt,
                GrammarExamples =
                [
                    new GrammarExample
                    {
                        En = item.ExampleEn,
                        Vi = ToVietnamese(item.ExampleVi)
                    }
                ]
            };
        }
    }

    private sealed record SeedLesson(
        string Title,
        string Level,
        string Description,
        string Formula,
        string Tips,
        string ExampleEn,
        string ExampleVi);

    private static string ToVietnamese(string value)
    {
        return value switch
        {
            "Co ay hoc tieng Anh moi buoi sang." => "Cô ấy học tiếng Anh mỗi buổi sáng.",
            "Ho dang chuan bi cho cuoc hop." => "Họ đang chuẩn bị cho cuộc họp.",
            "Chung toi da tham bao tang hom qua." => "Chúng tôi đã thăm bảo tàng hôm qua.",
            "Toi se goi cho ban sau bua trua." => "Tôi sẽ gọi cho bạn sau bữa trưa.",
            "Co ay se thi vao tuan sau." => "Cô ấy sẽ thi vào tuần sau.",
            "Co ba quyen sach tren ban." => "Có ba quyển sách trên bàn.",
            "Toi da mua mot quyen vo va mot cuc tay." => "Tôi đã mua một quyển vở và một cục tẩy.",
            "Chung toi can mot vai thong tin ve khoa hoc." => "Chúng tôi cần một vài thông tin về khóa học.",
            "Ban co cau hoi nao khong?" => "Bạn có câu hỏi nào không?",
            "Co nhieu nguoi o sanh." => "Có nhiều người ở sảnh.",
            "Van phong cua ho o tang hai." => "Văn phòng của họ ở tầng hai.",
            "Lam on gui tep cho toi." => "Làm ơn gửi tệp cho tôi.",
            "Nhung tai lieu nay da san sang." => "Những tài liệu này đã sẵn sàng.",
            "Anh ay co the noi hai ngon ngu." => "Anh ấy có thể nói hai ngôn ngữ.",
            "Lam on dong cua so." => "Làm ơn đóng cửa sổ.",
            "Co ay thuong di xe buyt." => "Cô ấy thường đi xe buýt.",
            "Lop hoc bat dau luc chin gio." => "Lớp học bắt đầu lúc chín giờ.",
            "Chia khoa o tren ban." => "Chìa khóa ở trên bàn.",
            "Toi thich doc truyen ngan." => "Tôi thích đọc truyện ngắn.",
            "Chung toi can on lai bai hoc." => "Chúng tôi cần ôn lại bài học.",
            "Co ay co mot quyen tu dien moi." => "Cô ấy có một quyển từ điển mới.",
            "Cac hoc sinh co hai bai tap." => "Các học sinh có hai bài tập.",
            "Day co phai vo cua ban khong?" => "Đây có phải vở của bạn không?",
            "Bai tap nay de hon bai truoc." => "Bài tập này dễ hơn bài trước.",
            "Ban song o dau?" => "Bạn sống ở đâu?",
            "Toi da hoan thanh bao cao." => "Tôi đã hoàn thành báo cáo.",
            "Co ay dang doc khi toi den." => "Cô ấy đang đọc khi tôi đến.",
            "Toi tung di bo den truong." => "Tôi từng đi bộ đến trường.",
            "Toi se gui email khi toi ve nha." => "Tôi sẽ gửi email khi tôi về nhà.",
            "Neu troi mua, chung toi se o trong nha." => "Nếu trời mưa, chúng tôi sẽ ở trong nhà.",
            "Ban nen xem lai ghi chu cua minh." => "Bạn nên xem lại ghi chú của mình.",
            "Nhan vien phai deo the." => "Nhân viên phải đeo thẻ.",
            "Chuyen tau co the bi tre." => "Chuyến tàu có thể bị trễ.",
            "Day la vi du huu ich nhat." => "Đây là ví dụ hữu ích nhất.",
            "Phong qua nho cho lop hoc." => "Phòng quá nhỏ cho lớp học.",
            "Cam on ban da giup toi." => "Cảm ơn bạn đã giúp tôi.",
            "Co ay len mang de dat ve." => "Cô ấy lên mạng để đặt vé.",
            "Ca hai dap an deu dung." => "Cả hai đáp án đều đúng.",
            "Nguoi phu nu da goi ban la quan ly cua toi." => "Người phụ nữ đã gọi bạn là quản lý của tôi.",
            "Neu nuoc dong bang, no tro thanh da." => "Nếu nước đóng băng, nó trở thành đá.",
            "Chung toi da song o day tu nam 2020." => "Chúng tôi đã sống ở đây từ năm 2020.",
            "Toi da xem bo phim do toi qua." => "Tôi đã xem bộ phim đó tối qua.",
            "Van phong duoc don moi toi." => "Văn phòng được dọn mỗi tối.",
            "Email da duoc gui luc giua trua." => "Email đã được gửi lúc giữa trưa.",
            "Giao vien yeu cau chung toi mo sach." => "Giáo viên yêu cầu chúng tôi mở sách.",
            "Co ay tu hoc tieng Tay Ban Nha." => "Cô ấy tự học tiếng Tây Ban Nha.",
            "Ai do da de lai loi nhan cho ban." => "Ai đó đã để lại lời nhắn cho bạn.",
            "Do la mot ngay rat ban ron." => "Đó là một ngày rất bận rộn.",
            "Dau tien, mo tep; sau do, luu mot ban sao." => "Đầu tiên, mở tệp; sau đó, lưu một bản sao.",
            "Ban den tu Ha Noi, phai khong?" => "Bạn đến từ Hà Nội, phải không?",
            "Neu toi co nhieu thoi gian hon, toi se hoc nhieu hon." => "Nếu tôi có nhiều thời gian hơn, tôi sẽ học nhiều hơn.",
            "Co ay da lam viec o day duoc nam nam." => "Cô ấy đã làm việc ở đây được năm năm.",
            "Ho da roi di truoc khi toi den." => "Họ đã rời đi trước khi tôi đến.",
            "Anh ay met vi da lai xe ca ngay." => "Anh ấy mệt vì đã lái xe cả ngày.",
            "Gio nay ngay mai, toi se dang bay den Seoul." => "Giờ này ngày mai, tôi sẽ đang bay đến Seoul.",
            "Truoc thu Sau, chung toi se hoan thanh du an." => "Trước thứ Sáu, chúng tôi sẽ hoàn thành dự án.",
            "Anh ay noi rang anh ay ban." => "Anh ấy nói rằng anh ấy bận.",
            "Co ay hoi toi song o dau." => "Cô ấy hỏi tôi sống ở đâu.",
            "Mau nay phai duoc nop hom nay." => "Mẫu này phải được nộp hôm nay.",
            "Toi da mang may tinh xach tay di sua." => "Tôi đã mang máy tính xách tay đi sửa.",
            "Quyen sach toi mua o tren ke." => "Quyển sách tôi mua ở trên kệ.",
            "Anh trai toi, nguoi song o Da Nang, la bac si." => "Anh trai tôi, người sống ở Đà Nẵng, là bác sĩ.",
            "Du cam thay met, co ay da hoan thanh cong viec." => "Dù cảm thấy mệt, cô ấy đã hoàn thành công việc.",
            "Mac du da muon, chung toi van tiep tuc lam viec." => "Mặc dù đã muộn, chúng tôi vẫn tiếp tục làm việc.",
            "Toi ghi no lai de khong quen." => "Tôi ghi nó lại để không quên.",
            "Bai giang ro den muc moi nguoi deu hieu." => "Bài giảng rõ đến mức mọi người đều hiểu.",
            "Nhiem vu kho. Tuy nhien, chung toi da hoan thanh." => "Nhiệm vụ khó. Tuy nhiên, chúng tôi đã hoàn thành.",
            "Chung toi con mot it thoi gian truoc cuoc hop." => "Chúng tôi còn một ít thời gian trước cuộc họp.",
            "Hoac quan ly hoac cac tro ly dang ranh." => "Hoặc quản lý hoặc các trợ lý đang rảnh.",
            "Toi uoc gi minh biet cau tra loi." => "Tôi ước gì mình biết câu trả lời.",
            "Co ay uoc rang minh da hoc cham hon." => "Cô ấy ước rằng mình đã học chăm hơn.",
            "Ban nen luu tep ngay bay gio." => "Bạn nên lưu tệp ngay bây giờ.",
            "Chung toi du kien phai den truoc tam gio." => "Chúng tôi dự kiến phải đến trước tám giờ.",
            "Toi thich o nha toi nay hon." => "Tôi thích ở nhà tối nay hơn.",
            "Chac han bay gio co ay dang o cho lam." => "Chắc hẳn bây giờ cô ấy đang ở chỗ làm.",
            "Chac han anh ay da lo chuyen xe buyt." => "Chắc hẳn anh ấy đã lỡ chuyến xe buýt.",
            "Toi chua bao gio thay tien bo nhanh nhu vay." => "Tôi chưa bao giờ thấy tiến bộ nhanh như vậy.",
            "Chinh han chot da lam chung toi lo lang." => "Chính hạn chót đã làm chúng tôi lo lắng.",
            "Khi buoc vao phong, co ay nhan ra loi." => "Khi bước vào phòng, cô ấy nhận ra lỗi.",
            "Cac tai lieu duoc luu truc tuyen deu an toan." => "Các tài liệu được lưu trực tuyến đều an toàn.",
            "He thong duoc cho la dang tin cay." => "Hệ thống được cho là đáng tin cậy.",
            "Den thang Sau, co ay se da day hoc duoc muoi nam." => "Đến tháng Sáu, cô ấy sẽ đã dạy học được mười năm.",
            "Toi muon ban goi truoc hon." => "Tôi muốn bạn gọi trước hơn.",
            "Gia ma chung toi da kiem tra dia chi." => "Giá mà chúng tôi đã kiểm tra địa chỉ.",
            "Chung toi vua den thi cuoc hop bat dau." => "Chúng tôi vừa đến thì cuộc họp bắt đầu.",
            "Ban cang luyen tap, ban cang tu tin." => "Bạn càng luyện tập, bạn càng tự tin.",
            "Da den luc chung ta cap nhat lich trinh." => "Đã đến lúc chúng ta cập nhật lịch trình.",
            "Quan ly de nghi anh ay tham gia cuoc goi." => "Quản lý đề nghị anh ấy tham gia cuộc gọi.",
            "Toi goi ca phe, con co ay goi tra." => "Tôi gọi cà phê, còn cô ấy gọi trà.",
            "Dieu lam toi ngac nhien la ket qua cuoi cung." => "Điều làm tôi ngạc nhiên là kết quả cuối cùng.",
            "Toi that su hieu moi quan ngai cua ban." => "Tôi thật sự hiểu mối quan ngại của bạn.",
            "Khoa hoc vua thuc te vua phai chang." => "Khóa học vừa thực tế vừa phải chăng.",
            "Co ay da nho khoa cua." => "Cô ấy đã nhớ khóa cửa.",
            "Anh ay mong duoc moi." => "Anh ấy mong được mời.",
            "Co ay co ve da quen cuoc hen." => "Cô ấy có vẻ đã quên cuộc hẹn.",
            "Van de nay, chung ta co the giai quyet hom nay." => "Vấn đề này, chúng ta có thể giải quyết hôm nay.",
            "Du lieu con han che; vi vay, chung ta can them thu nghiem." => "Dữ liệu còn hạn chế; vì vậy, chúng ta cần thêm thử nghiệm.",
            "Trong khi doanh so tang, loi nhuan van thap." => "Trong khi doanh số tăng, lợi nhuận vẫn thấp.",
            "Ban co the tham gia voi dieu kien dang ky hom nay." => "Bạn có thể tham gia với điều kiện đăng ký hôm nay.",
            "Neu toi da nhan cong viec do, bay gio toi se dang song o nuoc ngoai." => "Nếu tôi đã nhận công việc đó, bây giờ tôi sẽ đang sống ở nước ngoài.",
            _ => value
        };
    }

    private static readonly SeedLesson[] SeedLessons =
    [
        new("Present Simple", "A1", "Use for habits, facts, and repeated actions.", "Subject + V/Vs-es + object.", "Add s or es for he, she, and it.", "She studies English every morning.", "Co ay hoc tieng Anh moi buoi sang."),
        new("Present Continuous", "A1", "Use for actions happening now or around now.", "Subject + am/is/are + V-ing.", "Use now, right now, or at the moment for current actions.", "They are preparing for the meeting.", "Ho dang chuan bi cho cuoc hop."),
        new("Past Simple", "A1", "Use for finished actions in the past.", "Subject + V2/ed + object.", "Use did not plus base verb for negatives.", "We visited the museum yesterday.", "Chung toi da tham bao tang hom qua."),
        new("Future With Will", "A1", "Use for quick decisions, promises, and predictions.", "Subject + will + base verb.", "Do not add s after will.", "I will call you after lunch.", "Toi se goi cho ban sau bua trua."),
        new("Be Going To", "A1", "Use for plans and evidence-based predictions.", "Subject + am/is/are going to + base verb.", "Use going to when the plan already exists.", "She is going to take the test next week.", "Co ay se thi vao tuan sau."),
        new("There Is And There Are", "A1", "Use to say that something exists.", "There is + singular noun; There are + plural noun.", "Match is or are with the noun after it.", "There are three books on the desk.", "Co ba quyen sach tren ban."),
        new("Articles A An The", "A1", "Use articles before singular countable nouns.", "a/an + new noun; the + known noun.", "Use an before vowel sounds, not just vowel letters.", "I bought a notebook and an eraser.", "Toi da mua mot quyen vo va mot cuc tay."),
        new("Countable And Uncountable Nouns", "A1", "Use countable nouns with numbers and uncountable nouns without plural s.", "many + countable; much + uncountable.", "Do not add s to uncountable nouns like information.", "We need some information about the course.", "Chung toi can mot vai thong tin ve khoa hoc."),
        new("Some And Any", "A1", "Use some in positive sentences and any in negatives or questions.", "some/any + plural or uncountable noun.", "Use some in offers and requests when you expect yes.", "Do you have any questions?", "Ban co cau hoi nao khong?"),
        new("Much And Many", "A1", "Use much with uncountable nouns and many with countable nouns.", "much + uncountable; many + plural countable.", "In positive sentences, use a lot of more naturally.", "There are many people in the lobby.", "Co nhieu nguoi o sanh."),
        new("Possessive Adjectives", "A1", "Use my, your, his, her, its, our, and their before nouns.", "Possessive adjective + noun.", "Do not use an apostrophe with possessive adjectives.", "Their office is on the second floor.", "Van phong cua ho o tang hai."),
        new("Subject And Object Pronouns", "A1", "Use subject pronouns before verbs and object pronouns after verbs.", "I/he/she/we/they + verb; verb + me/him/her/us/them.", "Choose the pronoun by its job in the sentence.", "Please send the file to me.", "Lam on gui tep cho toi."),
        new("Demonstratives", "A1", "Use this, that, these, and those to point to things.", "this/that + singular; these/those + plural.", "Use this and these for things near you.", "These documents are ready.", "Nhung tai lieu nay da san sang."),
        new("Can And Cannot", "A1", "Use can for ability and permission.", "Subject + can/cannot + base verb.", "The verb after can never changes form.", "He can speak two languages.", "Anh ay co the noi hai ngon ngu."),
        new("Imperatives", "A1", "Use imperatives for instructions and commands.", "Base verb + object; Do not + base verb.", "Use please to make an imperative polite.", "Please close the window.", "Lam on dong cua so."),
        new("Adverbs Of Frequency", "A1", "Use always, usually, often, sometimes, and never for routines.", "Subject + adverb + main verb.", "Put the adverb after be but before most other verbs.", "She usually takes the bus.", "Co ay thuong di xe buyt."),
        new("Prepositions Of Time", "A1", "Use at, on, and in with time expressions.", "at + time; on + day/date; in + month/year/period.", "Use at for exact times.", "The class starts at nine o'clock.", "Lop hoc bat dau luc chin gio."),
        new("Prepositions Of Place", "A1", "Use in, on, and at to describe location.", "in/on/at + place.", "Use at for a point and in for an area.", "The keys are on the table.", "Chia khoa o tren ban."),
        new("Like Plus Ing", "A1", "Use like plus gerund to talk about enjoyment.", "Subject + like/likes + V-ing.", "After like, a gerund is common for general activities.", "I like reading short stories.", "Toi thich doc truyen ngan."),
        new("Need And Want", "A1", "Use need and want to talk about requirements and wishes.", "Subject + need/want + noun; need/want + to + verb.", "Use to before a verb after need or want.", "We need to review the lesson.", "Chung toi can on lai bai hoc."),
        new("Have Got", "A1", "Use have got to talk about possession.", "Subject + have/has got + noun.", "Have got is common in British English.", "She has got a new dictionary.", "Co ay co mot quyen tu dien moi."),
        new("Plural Nouns", "A1", "Use plural nouns for more than one thing.", "Singular noun + s/es/ies.", "Check irregular plurals like children and people.", "The students have two exercises.", "Cac hoc sinh co hai bai tap."),
        new("Basic This And That Questions", "A1", "Use this and that in simple questions.", "Is this/that + noun?", "Answer with it is or it is not.", "Is this your notebook?", "Day co phai vo cua ban khong?"),
        new("Basic Comparatives", "A1", "Use comparatives to compare two people or things.", "Adjective-er/more adjective + than.", "Use more with many two-syllable and longer adjectives.", "This exercise is easier than the last one.", "Bai tap nay de hon bai truoc."),
        new("Question Words", "A1", "Use question words to ask for specific information.", "Wh-word + auxiliary + subject + verb?", "Use who for people and where for places.", "Where do you live?", "Ban song o dau?"),
        new("Present Perfect", "A2", "Use for experiences and actions connected to now.", "Subject + have/has + past participle.", "Use ever and never for life experiences.", "I have finished the report.", "Toi da hoan thanh bao cao."),
        new("Past Continuous", "A2", "Use for actions in progress at a past time.", "Subject + was/were + V-ing.", "Use it with past simple to show interruption.", "She was reading when I arrived.", "Co ay dang doc khi toi den."),
        new("Used To", "A2", "Use for past habits or states that are no longer true.", "Subject + used to + base verb.", "Use did not use to in negatives.", "I used to walk to school.", "Toi tung di bo den truong."),
        new("Future Time Clauses", "A2", "Use present simple after when, before, after, and until for future meaning.", "Future clause + when/before/after + present simple.", "Do not use will in the time clause.", "I will email you when I get home.", "Toi se gui email khi toi ve nha."),
        new("First Conditional", "A2", "Use for real future possibilities.", "If + present simple, will + base verb.", "Use a comma when the if clause comes first.", "If it rains, we will stay inside.", "Neu troi mua, chung toi se o trong nha."),
        new("Should", "A2", "Use should for advice and recommendations.", "Subject + should/should not + base verb.", "Should is softer than must.", "You should review your notes.", "Ban nen xem lai ghi chu cua minh."),
        new("Must And Have To", "A2", "Use must and have to for obligation.", "Subject + must/have to + base verb.", "Use do not have to when something is not necessary.", "Employees have to wear ID cards.", "Nhan vien phai deo the."),
        new("May And Might", "A2", "Use may and might for possibility.", "Subject + may/might + base verb.", "Might often sounds less certain than may.", "The train might be late.", "Chuyen tau co the bi tre."),
        new("Superlatives", "A2", "Use superlatives to compare one item with a whole group.", "the + adjective-est; the most + adjective.", "Always use the before a superlative noun phrase.", "This is the most useful example.", "Day la vi du huu ich nhat."),
        new("Too And Enough", "A2", "Use too for excess and enough for sufficiency.", "too + adjective; adjective + enough.", "Enough goes after adjectives but before nouns.", "The room is too small for the class.", "Phong qua nho cho lop hoc."),
        new("Gerunds After Prepositions", "A2", "Use gerunds after prepositions.", "preposition + V-ing.", "After about, before, after, and without, use V-ing.", "Thank you for helping me.", "Cam on ban da giup toi."),
        new("Infinitive Of Purpose", "A2", "Use to plus verb to explain purpose.", "Subject + verb + to + base verb.", "This answers the question why.", "She went online to book a ticket.", "Co ay len mang de dat ve."),
        new("Both Either Neither", "A2", "Use both, either, and neither for two choices.", "both + plural; either/neither + singular.", "Neither means not one and not the other.", "Both answers are correct.", "Ca hai dap an deu dung."),
        new("Defining Relative Clauses", "A2", "Use who, which, and that to identify a noun.", "noun + who/which/that + clause.", "Do not use commas in defining clauses.", "The woman who called you is my manager.", "Nguoi phu nu da goi ban la quan ly cua toi."),
        new("Zero Conditional", "A2", "Use for facts and general truths.", "If + present simple, present simple.", "Use when instead of if for things that always happen.", "If water freezes, it becomes ice.", "Neu nuoc dong bang, no tro thanh da."),
        new("For And Since", "A2", "Use for with duration and since with starting points.", "have/has + past participle + for/since.", "For answers how long; since answers from when.", "We have lived here since 2020.", "Chung toi da song o day tu nam 2020."),
        new("Present Perfect Vs Past Simple", "A2", "Use present perfect without a finished time and past simple with one.", "have/has + V3; V2/ed + past time.", "Use yesterday, last week, and in 2020 with past simple.", "I saw that film last night.", "Toi da xem bo phim do toi qua."),
        new("Present Passive", "A2", "Use present passive when the action is more important than the doer.", "am/is/are + past participle.", "Use by only when the doer matters.", "The office is cleaned every evening.", "Van phong duoc don moi toi."),
        new("Past Passive", "A2", "Use past passive for completed actions in the past.", "was/were + past participle.", "Keep the past participle after was or were.", "The email was sent at noon.", "Email da duoc gui luc giua trua."),
        new("Reported Commands", "A2", "Use reported commands to say what someone told another person to do.", "tell/ask + object + to + base verb.", "Use not to for negative commands.", "The teacher asked us to open our books.", "Giao vien yeu cau chung toi mo sach."),
        new("Reflexive Pronouns", "A2", "Use reflexive pronouns when subject and object are the same.", "subject + verb + myself/yourself/himself.", "Do not use reflexive pronouns for normal objects.", "She taught herself Spanish.", "Co ay tu hoc tieng Tay Ban Nha."),
        new("Indefinite Pronouns", "A2", "Use someone, anything, nobody, and similar words for non-specific people or things.", "some/any/no/every + one/body/thing.", "Use anyone and anything in questions and negatives.", "Someone left a message for you.", "Ai do da de lai loi nhan cho ban."),
        new("So And Such", "A2", "Use so with adjectives and such with noun phrases.", "so + adjective; such + a/an + adjective + noun.", "Use such before a noun phrase.", "It was such a busy day.", "Do la mot ngay rat ban ron."),
        new("Sequencers", "A2", "Use sequencers to show order in a process or story.", "first, then, next, after that, finally.", "Sequencers help readers follow steps.", "First, open the file; then, save a copy.", "Dau tien, mo tep; sau do, luu mot ban sao."),
        new("Basic Tag Questions", "A2", "Use tag questions to check information.", "positive sentence + negative tag; negative sentence + positive tag.", "Match the auxiliary in the sentence.", "You are from Hanoi, aren't you?", "Ban den tu Ha Noi, phai khong?"),
        new("Second Conditional", "B1", "Use for imaginary present or future situations.", "If + past simple, would + base verb.", "The past form shows distance from reality, not past time.", "If I had more time, I would study more.", "Neu toi co nhieu thoi gian hon, toi se hoc nhieu hon."),
        new("Present Perfect Continuous", "B1", "Use for actions that started in the past and continue now.", "Subject + have/has been + V-ing.", "Use it to emphasize duration or activity.", "She has been working here for five years.", "Co ay da lam viec o day duoc nam nam."),
        new("Past Perfect", "B1", "Use for an action before another past action.", "Subject + had + past participle.", "Use it to make the earlier past action clear.", "They had left before I arrived.", "Ho da roi di truoc khi toi den."),
        new("Past Perfect Continuous", "B1", "Use for an ongoing action before a past point.", "Subject + had been + V-ing.", "Use it to explain a past result.", "He was tired because he had been driving all day.", "Anh ay met vi da lai xe ca ngay."),
        new("Future Continuous", "B1", "Use for an action in progress at a future time.", "Subject + will be + V-ing.", "Use it for polite questions about plans too.", "This time tomorrow, I will be flying to Seoul.", "Gio nay ngay mai, toi se dang bay den Seoul."),
        new("Future Perfect", "B1", "Use for an action completed before a future time.", "Subject + will have + past participle.", "Use by plus time to mark the deadline.", "By Friday, we will have finished the project.", "Truoc thu Sau, chung toi se hoan thanh du an."),
        new("Reported Speech Statements", "B1", "Use reported speech to repeat what someone said.", "said/told + that + shifted clause.", "Backshift the tense when the reporting verb is in the past.", "He said that he was busy.", "Anh ay noi rang anh ay ban."),
        new("Reported Speech Questions", "B1", "Use reported questions with statement word order.", "asked + wh/if/whether + subject + verb.", "Do not use question word order after asked.", "She asked where I lived.", "Co ay hoi toi song o dau."),
        new("Passive With Modals", "B1", "Use modal passive when the subject receives an action.", "modal + be + past participle.", "The main verb is always a past participle.", "The form must be submitted today.", "Mau nay phai duoc nop hom nay."),
        new("Causative Have And Get", "B1", "Use causative forms when someone arranges a service.", "have/get + object + past participle.", "Use get for a more informal style.", "I had my laptop repaired.", "Toi da mang may tinh xach tay di sua."),
        new("Defining Clauses With Omission", "B1", "Omit who, which, or that when it is the object of a defining clause.", "noun + subject + verb.", "Do not omit the relative pronoun when it is the subject.", "The book I bought is on the shelf.", "Quyen sach toi mua o tren ke."),
        new("Non Defining Relative Clauses", "B1", "Use non-defining clauses to add extra information.", "noun, who/which + clause, main clause.", "Use commas and do not use that.", "My brother, who lives in Da Nang, is a doctor.", "Anh trai toi, nguoi song o Da Nang, la bac si."),
        new("Despite And In Spite Of", "B1", "Use despite and in spite of to show contrast.", "despite/in spite of + noun/V-ing.", "Do not put a full subject-verb clause directly after despite.", "Despite feeling tired, she finished the work.", "Du cam thay met, co ay da hoan thanh cong viec."),
        new("Although And Even Though", "B1", "Use although and even though before contrast clauses.", "although/even though + subject + verb.", "Even though is stronger than although.", "Although it was late, we continued working.", "Mac du da muon, chung toi van tiep tuc lam viec."),
        new("Purpose Clauses", "B1", "Use purpose clauses to explain intention.", "so that + subject + can/could/will/would.", "Use so that when the purpose has its own subject.", "I wrote it down so that I would not forget.", "Toi ghi no lai de khong quen."),
        new("Result Clauses", "B1", "Use result clauses to show the effect of a situation.", "so + adjective/adverb + that + clause.", "Use such with noun phrases.", "The lecture was so clear that everyone understood.", "Bai giang ro den muc moi nguoi deu hieu."),
        new("Contrast Connectors", "B1", "Use however, nevertheless, and on the other hand to contrast ideas.", "sentence. However, sentence.", "Use punctuation carefully around connectors.", "The task was difficult. However, we completed it.", "Nhiem vu kho. Tuy nhien, chung toi da hoan thanh."),
        new("Advanced Quantifiers", "B1", "Use few, a few, little, and a little accurately.", "few/a few + countable; little/a little + uncountable.", "A few and a little are more positive than few and little.", "We have a little time before the meeting.", "Chung toi con mot it thoi gian truoc cuoc hop."),
        new("Neither Nor And Either Or", "B1", "Use paired conjunctions to connect two alternatives.", "neither A nor B; either A or B.", "The verb often agrees with the closer subject.", "Either the manager or the assistants are available.", "Hoac quan ly hoac cac tro ly dang ranh."),
        new("Wish For Present Situations", "B1", "Use wish plus past simple for unreal present situations.", "wish + subject + past simple.", "Use were for all subjects in formal English.", "I wish I knew the answer.", "Toi uoc gi minh biet cau tra loi."),
        new("Wish For Past Situations", "B1", "Use wish plus past perfect for regrets about the past.", "wish + subject + had + past participle.", "This structure talks about something that did not happen.", "She wishes she had studied harder.", "Co ay uoc rang minh da hoc cham hon."),
        new("Had Better", "B1", "Use had better for strong advice.", "Subject + had better + base verb.", "Had better often suggests a negative result if ignored.", "You had better save the file now.", "Ban nen luu tep ngay bay gio."),
        new("Be Supposed To", "B1", "Use be supposed to for expectations and rules.", "Subject + am/is/are supposed to + base verb.", "It can describe what should happen according to a plan.", "We are supposed to arrive before eight.", "Chung toi du kien phai den truoc tam gio."),
        new("Would Rather And Prefer", "B1", "Use these forms to talk about preferences.", "would rather + base verb; prefer + V-ing/to verb.", "Would rather is followed by the base verb.", "I would rather stay home tonight.", "Toi thich o nha toi nay hon."),
        new("Modal Deduction Present", "B1", "Use must, might, and can't to guess about the present.", "must/might/can't + base verb.", "Must means you are almost sure it is true.", "She must be at work now.", "Chac han bay gio co ay dang o cho lam."),
        new("Modal Deduction Past", "B2", "Use modal perfect forms to guess about past events.", "must/might/can't + have + past participle.", "Use can't have when you are sure something was impossible.", "He must have missed the bus.", "Chac han anh ay da lo chuyen xe buyt."),
        new("Negative Inversion", "B2", "Use inversion after negative adverbials for emphasis.", "Never/Rarely/Not only + auxiliary + subject + verb.", "This is formal and emphatic.", "Never have I seen such fast progress.", "Toi chua bao gio thay tien bo nhanh nhu vay."),
        new("Cleft Sentences", "B2", "Use cleft sentences to emphasize one part of a sentence.", "It is/was + focus + that/who + clause.", "Put the important information after it is or it was.", "It was the deadline that worried us.", "Chinh han chot da lam chung toi lo lang."),
        new("Participle Clauses", "B2", "Use participle clauses to shorten related information.", "V-ing/V3 clause, main clause.", "Make sure the participle refers to the subject of the main clause.", "Walking into the room, she noticed the error.", "Khi buoc vao phong, co ay nhan ra loi."),
        new("Reduced Relative Clauses", "B2", "Use reduced clauses to make writing more concise.", "noun + V-ing/V3 phrase.", "Use V-ing for active meaning and V3 for passive meaning.", "The documents stored online are secure.", "Cac tai lieu duoc luu truc tuyen deu an toan."),
        new("Passive Reporting Verbs", "B2", "Use passive reporting to report beliefs or claims.", "It is said that; subject + is said to + verb.", "This structure is common in formal writing.", "The system is believed to be reliable.", "He thong duoc cho la dang tin cay."),
        new("Future Perfect Continuous", "B2", "Use for duration before a future point.", "Subject + will have been + V-ing.", "Use by then or by plus time to show the future point.", "By June, she will have been teaching for ten years.", "Den thang Sau, co ay se da day hoc duoc muoi nam."),
        new("Unreal Past", "B2", "Use past forms after wish, if only, and would rather for unreal meaning.", "wish/if only/would rather + past form.", "The past tense can show distance, not actual past time.", "I would rather you called first.", "Toi muon ban goi truoc hon."),
        new("If Only", "B2", "Use if only for strong wishes and regrets.", "If only + past simple/past perfect/would + verb.", "Choose the tense by the time you mean.", "If only we had checked the address.", "Gia ma chung toi da kiem tra dia chi."),
        new("No Sooner And Hardly", "B2", "Use these structures to show one event happened immediately after another.", "No sooner had + subject + V3 than; Hardly had + subject + V3 when.", "Use inversion after no sooner and hardly.", "No sooner had we arrived than the meeting began.", "Chung toi vua den thi cuoc hop bat dau."),
        new("The More The More", "B2", "Use parallel comparatives to show linked changes.", "The + comparative, the + comparative.", "Keep the structure balanced on both sides.", "The more you practice, the more confident you become.", "Ban cang luyen tap, ban cang tu tin."),
        new("It Is Time", "B2", "Use it is time to say something should happen now.", "It is time + subject + past simple.", "The past form makes the meaning less direct.", "It is time we updated the schedule.", "Da den luc chung ta cap nhat lich trinh."),
        new("Subjunctive", "B2", "Use the subjunctive after certain verbs and adjectives.", "suggest/insist/essential that + subject + base verb.", "The verb does not take s in the subjunctive.", "The manager suggested that he join the call.", "Quan ly de nghi anh ay tham gia cuoc goi."),
        new("Ellipsis", "B2", "Use ellipsis to avoid repeating words that are understood.", "clause + and/but + shortened clause.", "Only omit words when the meaning stays clear.", "I ordered coffee, and she tea.", "Toi goi ca phe, con co ay goi tra."),
        new("Nominal Clauses", "B2", "Use noun clauses as subjects, objects, or complements.", "what/that/whether + clause.", "A noun clause can act like a noun phrase.", "What surprised me was the final result.", "Dieu lam toi ngac nhien la ket qua cuoi cung."),
        new("Emphatic Do", "B2", "Use do, does, or did to add emphasis in positive sentences.", "Subject + do/does/did + base verb.", "Stress the auxiliary when speaking.", "I do understand your concern.", "Toi that su hieu moi quan ngai cua ban."),
        new("Correlative Conjunctions", "B2", "Use paired conjunctions to connect balanced ideas.", "not only A but also B; both A and B.", "Keep the grammar after each part parallel.", "The course is both practical and affordable.", "Khoa hoc vua thuc te vua phai chang."),
        new("Complex Gerund And Infinitive Patterns", "B2", "Use gerunds and infinitives after specific verbs.", "verb + V-ing; verb + to + base verb.", "Some verbs change meaning depending on the pattern.", "She remembered to lock the door.", "Co ay da nho khoa cua."),
        new("Passive Gerund And Infinitive", "B2", "Use passive forms after verbs that need gerunds or infinitives.", "being + V3; to be + V3.", "Choose the passive when the subject receives the action.", "He expects to be invited.", "Anh ay mong duoc moi."),
        new("Perfect Infinitive", "B2", "Use perfect infinitives to refer to earlier actions.", "to have + past participle.", "It often appears after seem, appear, and claim.", "She seems to have forgotten the appointment.", "Co ay co ve da quen cuoc hen."),
        new("Fronting", "B2", "Move information to the front for emphasis or flow.", "fronted phrase + subject + verb.", "Use fronting to connect with the previous idea.", "This problem, we can solve today.", "Van de nay, chung ta co the giai quyet hom nay."),
        new("Discourse Markers", "B2", "Use discourse markers to organize complex ideas.", "however, moreover, therefore, in contrast + clause.", "Choose markers by the logical relationship.", "The data is limited; therefore, we need more tests.", "Du lieu con han che; vi vay, chung ta can them thu nghiem."),
        new("Concession Clauses", "B2", "Use concession clauses to admit a contrast before the main point.", "while/whereas/although + clause.", "Whereas is useful for formal comparisons.", "Whereas sales rose, profit remained low.", "Trong khi doanh so tang, loi nhuan van thap."),
        new("Conditional Alternatives", "B2", "Use unless, provided that, as long as, and otherwise as alternatives to if.", "unless/provided that/as long as + clause.", "Unless means if not.", "You can join provided that you register today.", "Ban co the tham gia voi dieu kien dang ky hom nay."),
        new("Mixed Conditionals", "B2", "Use mixed conditionals when the if clause and result refer to different times.", "If + past perfect, would + base verb; If + past simple, would have + V3.", "Identify the time of the condition and the result separately.", "If I had taken that job, I would be living abroad now.", "Neu toi da nhan cong viec do, bay gio toi se dang song o nuoc ngoai."),
        new("IELTS Complex Sentences", "B2", "Use complex sentences to connect ideas clearly in IELTS Writing and Speaking.", "Main clause + subordinating conjunction + dependent clause.", "Use because, although, while, whereas, if, and when to show logical relationships.", "Although online learning is convenient, it cannot fully replace classroom interaction.", "Mặc dù học trực tuyến tiện lợi, nó không thể thay thế hoàn toàn sự tương tác trong lớp học."),
        new("IELTS Academic Hedging", "B2", "Use hedging to make academic claims careful and balanced.", "Subject + may/might/tend to/appear to + base verb.", "Avoid absolute claims unless the evidence is very strong.", "Public transport may reduce traffic congestion in large cities.", "Giao thông công cộng có thể giảm ùn tắc giao thông ở các thành phố lớn."),
        new("IELTS Concession Sentences", "B2", "Use concession to acknowledge an opposing idea before giving your main point.", "Although/Even though + clause, main clause.", "This helps Task 2 essays sound balanced and developed.", "Even though the policy is expensive, it may bring long-term benefits.", "Mặc dù chính sách này tốn kém, nó có thể mang lại lợi ích lâu dài."),
        new("IELTS Cause And Effect Grammar", "B2", "Use cause and effect grammar to explain reasons and consequences.", "Because/Since/As + clause; therefore/as a result + clause.", "Use these structures to develop explanations in body paragraphs.", "Because housing is expensive, many young adults continue living with their parents.", "Vì nhà ở đắt đỏ, nhiều người trẻ tiếp tục sống với cha mẹ."),
        new("IELTS Comparison Structures", "B2", "Use comparison structures for charts, opinions, and contrasting groups.", "more/less/fewer + noun + than; as + adjective + as.", "Use precise comparatives in Task 1 and Task 2.", "The number of commuters was significantly higher in 2020 than in 2010.", "Số người đi làm hằng ngày cao hơn đáng kể vào năm 2020 so với năm 2010."),
        new("IELTS Noun Clauses", "B2", "Use noun clauses to make complex ideas act as subjects or objects.", "What/That/Whether + subject + verb.", "Noun clauses help create mature sentence openings.", "What governments should prioritize is affordable education.", "Điều các chính phủ nên ưu tiên là giáo dục có chi phí hợp lý."),
        new("IELTS Advanced Relative Clauses", "B2", "Use advanced relative clauses to add accurate detail about people, places, and ideas.", "noun + who/which/where/whose + clause.", "Use commas for extra information and no commas for defining information.", "Renewable energy, which is becoming cheaper, is a practical solution.", "Năng lượng tái tạo, vốn đang trở nên rẻ hơn, là một giải pháp thực tế."),
        new("IELTS Participle Phrases", "B2", "Use participle phrases to shorten clauses and improve sentence flow.", "V-ing/V3 phrase, main clause.", "Use V-ing for active meaning and V3 for passive meaning.", "Facing higher costs, many families reduce non-essential spending.", "Đối mặt với chi phí cao hơn, nhiều gia đình cắt giảm chi tiêu không thiết yếu."),
        new("IELTS Nominalisation", "B2", "Use nominalisation to make writing more formal and concise.", "verb/adjective idea -> noun phrase.", "Academic writing often uses noun phrases to discuss abstract ideas.", "The expansion of public transport can improve urban mobility.", "Việc mở rộng giao thông công cộng có thể cải thiện khả năng di chuyển trong đô thị."),
        new("IELTS Referencing With This And These", "B2", "Use this and these with nouns to link back to previous ideas clearly.", "This/These + summary noun + verb.", "Always add a noun after this or these in formal writing.", "This trend may create pressure on public services.", "Xu hướng này có thể tạo áp lực lên các dịch vụ công."),
        new("IELTS Parallel Structures", "B2", "Use parallel structures to keep lists and comparisons grammatically balanced.", "same grammar form + and/or/but + same grammar form.", "Parallel grammar makes arguments clearer and more professional.", "The proposal is practical, affordable, and easy to implement.", "Đề xuất này thực tế, phải chăng và dễ triển khai."),
        new("IELTS Sentence Variety", "B2", "Use varied sentence patterns to avoid repetitive writing.", "simple sentence + compound sentence + complex sentence.", "A good IELTS essay mixes sentence types without losing clarity.", "Some people support the change, but others oppose it because it may increase costs.", "Một số người ủng hộ sự thay đổi, nhưng những người khác phản đối vì nó có thể làm tăng chi phí."),
        new("IELTS Stance Adverbs", "B2", "Use stance adverbs to show certainty, attitude, and evaluation.", "Arguably/Generally/Clearly/Undoubtedly + clause.", "Use these adverbs carefully to avoid sounding too extreme.", "Arguably, education is the most effective way to reduce inequality.", "Có thể lập luận rằng giáo dục là cách hiệu quả nhất để giảm bất bình đẳng."),
        new("IELTS Complex Prepositional Phrases", "B2", "Use academic prepositional phrases to organize and qualify ideas.", "in terms of/with regard to/due to/in response to + noun.", "These phrases are useful for formal topic development.", "In terms of cost, renewable energy is becoming more competitive.", "Xét về chi phí, năng lượng tái tạo đang trở nên cạnh tranh hơn."),
        new("IELTS Passive For Processes", "B2", "Use passive voice to describe processes without focusing on the doer.", "subject + is/are + past participle.", "This is especially useful in IELTS Writing Task 1 process diagrams.", "The raw materials are heated before they are shaped into bricks.", "Nguyên liệu thô được nung nóng trước khi được tạo hình thành gạch."),
        new("IELTS Data Description Grammar", "B2", "Use data description grammar to report trends and figures accurately.", "figure + rose/fell/increased/decreased + adverb/preposition phrase.", "Use tense and prepositions carefully when describing charts.", "Sales increased sharply from 20 million to 45 million dollars.", "Doanh số tăng mạnh từ 20 triệu lên 45 triệu đô la."),
        new("IELTS Opinion Grammar", "B2", "Use opinion grammar to present a clear position in essays.", "I would argue that/It is clear that/There is evidence that + clause.", "Use impersonal structures for a more academic tone.", "It is clear that early investment in education benefits society.", "Rõ ràng là đầu tư sớm vào giáo dục mang lại lợi ích cho xã hội."),
        new("IELTS Problem Solution Grammar", "B2", "Use problem-solution grammar to explain issues and propose responses.", "The main problem is that + clause; one solution is to + verb.", "Use cause, result, and solution structures in the same paragraph.", "One solution is to invest more money in public transport.", "Một giải pháp là đầu tư nhiều tiền hơn vào giao thông công cộng."),
        new("IELTS Advantage Disadvantage Grammar", "B2", "Use balanced grammar to discuss benefits and drawbacks.", "One advantage is that + clause; a drawback is that + clause.", "Develop both sides clearly before giving your evaluation.", "One advantage is that remote work saves time, but a drawback is that it can reduce teamwork.", "Một lợi ích là làm việc từ xa tiết kiệm thời gian, nhưng một hạn chế là nó có thể làm giảm tinh thần làm việc nhóm."),
        new("IELTS Reduced Adverb Clauses", "B2", "Use reduced adverb clauses to make complex sentences shorter.", "After/Before/While + V-ing; V3 phrase + main clause.", "Only reduce the clause when both clauses have the same subject.", "After completing the survey, researchers analyzed the results.", "Sau khi hoàn thành khảo sát, các nhà nghiên cứu phân tích kết quả.")
    ];
}
