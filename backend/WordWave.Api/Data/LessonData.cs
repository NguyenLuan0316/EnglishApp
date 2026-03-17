// wordwave/backend/WordWave.Api/Data/LessonData.cs
using WordWave.Api.Models;

namespace WordWave.Api.Data;

public static class LessonData
{
    public static readonly List<GrammarLesson> Grammar = new()
    {
        new() { Id=1, Title="Present Simple Tense", Level="A1",
            Description="Diễn tả hành động thường xuyên, thói quen hoặc sự thật hiển nhiên",
            Formula="S + V(s/es)  |  S + do/does + not + V",
            Examples=[ new(){En="I work every day.",Vi="Tôi làm việc mỗi ngày."},
                       new(){En="She doesn't like spicy food.",Vi="Cô ấy không thích đồ ăn cay."},
                       new(){En="Do you speak English?",Vi="Bạn có nói tiếng Anh không?"} ],
            Tips="Thêm 's/es' cho he/she/it. Dùng do/does cho câu hỏi và phủ định." },

        new() { Id=2, Title="Past Simple Tense", Level="A1",
            Description="Diễn tả hành động đã xảy ra và kết thúc trong quá khứ",
            Formula="S + V-ed (hoặc V bất quy tắc)  |  S + did + not + V",
            Examples=[ new(){En="I visited Hanoi last year.",Vi="Tôi đã đến Hà Nội năm ngoái."},
                       new(){En="He didn't go to school yesterday.",Vi="Anh ấy đã không đi học hôm qua."},
                       new(){En="Did you see that movie?",Vi="Bạn đã xem bộ phim đó chưa?"} ],
            Tips="Động từ bất quy tắc: go→went, see→saw, have→had." },

        new() { Id=3, Title="Present Continuous", Level="A2",
            Description="Diễn tả hành động đang xảy ra ngay lúc nói",
            Formula="S + am/is/are + V-ing",
            Examples=[ new(){En="I am studying English now.",Vi="Tôi đang học tiếng Anh bây giờ."},
                       new(){En="She is not sleeping.",Vi="Cô ấy không đang ngủ."},
                       new(){En="Are they watching TV?",Vi="Họ có đang xem TV không?"} ],
            Tips="am với I, is với he/she/it, are với you/we/they." },

        new() { Id=4, Title="Future Simple (will)", Level="A2",
            Description="Diễn tả quyết định tức thì hoặc dự đoán về tương lai",
            Formula="S + will + V (bare infinitive)",
            Examples=[ new(){En="I will help you.",Vi="Tôi sẽ giúp bạn."},
                       new(){En="She won't come tomorrow.",Vi="Cô ấy sẽ không đến ngày mai."},
                       new(){En="Will it rain today?",Vi="Hôm nay trời có mưa không?"} ],
            Tips="'will not' viết tắt là 'won't'. Dùng will cho quyết định ngay lúc nói." },

        new() { Id=5, Title="Present Perfect", Level="B1",
            Description="Diễn tả hành động đã xảy ra nhưng có liên quan đến hiện tại",
            Formula="S + have/has + V3 (past participle)",
            Examples=[ new(){En="I have lived here for 5 years.",Vi="Tôi đã sống ở đây được 5 năm."},
                       new(){En="She has never been to Paris.",Vi="Cô ấy chưa bao giờ đến Paris."},
                       new(){En="Have you finished your homework?",Vi="Bạn đã làm xong bài tập chưa?"} ],
            Tips="'for' + khoảng thời gian, 'since' + mốc thời gian. ever/never hay dùng với thì này." },

        new() { Id=6, Title="Conditional Type 1", Level="B1",
            Description="Điều kiện có thể xảy ra trong tương lai (khả năng thực tế)",
            Formula="If + S + V (present), S + will + V",
            Examples=[ new(){En="If it rains, I will stay home.",Vi="Nếu trời mưa, tôi sẽ ở nhà."},
                       new(){En="She will pass if she studies hard.",Vi="Cô ấy sẽ đậu nếu học chăm."} ],
            Tips="Mệnh đề if dùng present simple, mệnh đề chính dùng will." },

        new() { Id=7, Title="Passive Voice", Level="B1",
            Description="Câu bị động – nhấn mạnh vào đối tượng chịu tác động",
            Formula="S + be + V3 (+ by + agent)",
            Examples=[ new(){En="The book was written by him.",Vi="Cuốn sách được viết bởi anh ấy."},
                       new(){En="English is spoken worldwide.",Vi="Tiếng Anh được nói trên toàn thế giới."} ],
            Tips="'be' biến đổi theo thì, V3 không đổi. Agent bỏ khi không quan trọng." },

        new() { Id=8, Title="Reported Speech", Level="B2",
            Description="Lời nói gián tiếp – thuật lại lời của người khác",
            Formula="S + said (that) + S + V (backshift)",
            Examples=[ new(){En="She said, \"I am tired.\" → She said she was tired.",Vi="Cô ấy nói cô ấy mệt."},
                       new(){En="He said, \"I will come.\" → He said he would come.",Vi="Anh ấy nói anh ấy sẽ đến."} ],
            Tips="Thì lùi một bậc: present→past, will→would, can→could." },
    };

    public static readonly List<SentencePattern> Patterns = new()
    {
        new() { Id=1,  Sentence="How are you?",               Meaning="Bạn có khỏe không?",        Explanation="Câu hỏi thăm sức khỏe phổ biến nhất",        Examples=["How are you today?","How are you feeling?"] },
        new() { Id=2,  Sentence="I would like to...",          Meaning="Tôi muốn...",               Explanation="Diễn đạt mong muốn lịch sự",                  Examples=["I would like to order coffee.","I would like to learn more."] },
        new() { Id=3,  Sentence="Could you please...?",        Meaning="Bạn có thể làm ơn...?",     Explanation="Yêu cầu lịch sự, trang trọng hơn 'can'",      Examples=["Could you please help me?","Could you please repeat that?"] },
        new() { Id=4,  Sentence="What do you think about...?", Meaning="Bạn nghĩ gì về...?",        Explanation="Hỏi ý kiến hoặc quan điểm",                   Examples=["What do you think about this idea?","What do you think about the news?"] },
        new() { Id=5,  Sentence="I'm not sure, but...",        Meaning="Tôi không chắc, nhưng...",  Explanation="Nói khi chưa chắc chắn về thông tin",         Examples=["I'm not sure, but I think he's right.","I'm not sure, but let me check."] },
        new() { Id=6,  Sentence="It depends on...",            Meaning="Điều đó phụ thuộc vào...",  Explanation="Trả lời khi không có đáp án rõ ràng",         Examples=["It depends on the situation.","It depends on what you mean."] },
        new() { Id=7,  Sentence="I'm looking forward to...",   Meaning="Tôi mong đợi...",           Explanation="Diễn đạt sự mong chờ điều tích cực",          Examples=["I'm looking forward to the weekend.","I'm looking forward to meeting you."] },
        new() { Id=8,  Sentence="As far as I know...",         Meaning="Theo như tôi biết...",       Explanation="Diễn đạt giới hạn kiến thức của mình",        Examples=["As far as I know, he's still in Hanoi.","As far as I know, it's free."] },
        new() { Id=9,  Sentence="Would you mind...?",          Meaning="Bạn có phiền...?",           Explanation="Yêu cầu rất lịch sự",                         Examples=["Would you mind opening the window?","Would you mind waiting a moment?"] },
        new() { Id=10, Sentence="I can't help but...",         Meaning="Tôi không thể không...",     Explanation="Diễn tả điều không thể tránh khỏi",           Examples=["I can't help but smile.","I can't help but agree with you."] },
    };
}
