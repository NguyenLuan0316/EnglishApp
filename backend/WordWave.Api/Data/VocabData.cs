// wordwave/backend/WordWave.Api/Data/VocabData.cs
using WordWave.Api.Models;

namespace WordWave.Api.Data;

public static class VocabData
{
    public static readonly List<VocabWord> Words = new()
    {
        // ── A1 · DAILY ──────────────────────────────────────────────────────
        new() { Id=1,  Word="hello",      Phonetic="/həˈloʊ/",         Meaning="xin chào",              Example="Hello, how are you?",                   ExampleMeaning="Xin chào, bạn có khỏe không?",        Level="A1", Topic="daily" },
        new() { Id=2,  Word="goodbye",    Phonetic="/ɡʊdˈbaɪ/",        Meaning="tạm biệt",              Example="Goodbye, see you tomorrow!",             ExampleMeaning="Tạm biệt, hẹn gặp lại ngày mai!",    Level="A1", Topic="daily" },
        new() { Id=3,  Word="please",     Phonetic="/pliːz/",           Meaning="làm ơn",                Example="Could you please help me?",              ExampleMeaning="Bạn có thể làm ơn giúp tôi không?",  Level="A1", Topic="daily" },
        new() { Id=4,  Word="thank you",  Phonetic="/θæŋk juː/",        Meaning="cảm ơn",                Example="Thank you for your help.",               ExampleMeaning="Cảm ơn vì sự giúp đỡ của bạn.",      Level="A1", Topic="daily" },
        new() { Id=5,  Word="sorry",      Phonetic="/ˈsɒri/",           Meaning="xin lỗi",               Example="I'm sorry I'm late.",                    ExampleMeaning="Tôi xin lỗi vì đến trễ.",            Level="A1", Topic="daily" },
        new() { Id=6,  Word="happy",      Phonetic="/ˈhæpi/",           Meaning="vui vẻ, hạnh phúc",    Example="She looks very happy today.",            ExampleMeaning="Cô ấy trông rất vui hôm nay.",       Level="A1", Topic="daily" },
        new() { Id=7,  Word="house",      Phonetic="/haʊs/",            Meaning="ngôi nhà",              Example="I live in a big house.",                 ExampleMeaning="Tôi sống trong một ngôi nhà lớn.",   Level="A1", Topic="daily" },
        new() { Id=8,  Word="family",     Phonetic="/ˈfæməli/",         Meaning="gia đình",              Example="My family is very important to me.",     ExampleMeaning="Gia đình rất quan trọng với tôi.",   Level="A1", Topic="daily" },
        new() { Id=9,  Word="friend",     Phonetic="/frend/",            Meaning="bạn bè",                Example="She is my best friend.",                 ExampleMeaning="Cô ấy là người bạn thân nhất.",     Level="A1", Topic="daily" },
        new() { Id=10, Word="day",        Phonetic="/deɪ/",             Meaning="ngày",                  Example="It is a beautiful day.",                 ExampleMeaning="Đây là một ngày đẹp.",               Level="A1", Topic="daily" },
        new() { Id=11, Word="morning",    Phonetic="/ˈmɔːrnɪŋ/",        Meaning="buổi sáng",             Example="Good morning!",                          ExampleMeaning="Chào buổi sáng!",                    Level="A1", Topic="daily" },
        new() { Id=12, Word="money",      Phonetic="/ˈmʌni/",           Meaning="tiền",                  Example="I need money to buy food.",              ExampleMeaning="Tôi cần tiền để mua thức ăn.",       Level="A1", Topic="daily" },
        new() { Id=13, Word="time",       Phonetic="/taɪm/",            Meaning="thời gian, giờ",        Example="What time is it?",                       ExampleMeaning="Mấy giờ rồi?",                       Level="A1", Topic="daily" },
        new() { Id=14, Word="big",        Phonetic="/bɪɡ/",             Meaning="lớn",                   Example="That is a big dog!",                     ExampleMeaning="Đó là một con chó lớn!",             Level="A1", Topic="daily" },
        new() { Id=15, Word="good",       Phonetic="/ɡʊd/",             Meaning="tốt",                   Example="That is a good idea.",                   ExampleMeaning="Đó là một ý tưởng tốt.",             Level="A1", Topic="daily" },
        new() { Id=16, Word="love",       Phonetic="/lʌv/",             Meaning="tình yêu, yêu",         Example="I love my family.",                      ExampleMeaning="Tôi yêu gia đình tôi.",              Level="A1", Topic="daily" },
        new() { Id=17, Word="help",       Phonetic="/help/",            Meaning="giúp đỡ",               Example="Can you help me?",                       ExampleMeaning="Bạn có thể giúp tôi không?",         Level="A1", Topic="daily" },
        new() { Id=18, Word="name",       Phonetic="/neɪm/",            Meaning="tên",                   Example="What is your name?",                     ExampleMeaning="Tên bạn là gì?",                     Level="A1", Topic="daily" },
        new() { Id=19, Word="year",       Phonetic="/jɪər/",            Meaning="năm",                   Example="Happy New Year!",                        ExampleMeaning="Chúc mừng năm mới!",                 Level="A1", Topic="daily" },
        new() { Id=20, Word="city",       Phonetic="/ˈsɪti/",           Meaning="thành phố",             Example="Hanoi is a big city.",                   ExampleMeaning="Hà Nội là một thành phố lớn.",       Level="A1", Topic="daily" },

        // ── A1 · FOOD ───────────────────────────────────────────────────────
        new() { Id=21, Word="water",      Phonetic="/ˈwɔːtər/",         Meaning="nước",                  Example="I drink water every day.",               ExampleMeaning="Tôi uống nước mỗi ngày.",            Level="A1", Topic="food" },
        new() { Id=22, Word="food",       Phonetic="/fuːd/",            Meaning="thức ăn",               Example="The food here is delicious.",            ExampleMeaning="Thức ăn ở đây rất ngon.",            Level="A1", Topic="food" },
        new() { Id=23, Word="rice",       Phonetic="/raɪs/",            Meaning="cơm, gạo",              Example="Rice is the main food in Asia.",          ExampleMeaning="Cơm là thức ăn chính ở châu Á.",     Level="A1", Topic="food" },
        new() { Id=24, Word="bread",      Phonetic="/bred/",            Meaning="bánh mì",               Example="I eat bread for breakfast.",             ExampleMeaning="Tôi ăn bánh mì vào bữa sáng.",      Level="A1", Topic="food" },
        new() { Id=25, Word="chicken",    Phonetic="/ˈtʃɪkɪn/",         Meaning="thịt gà",               Example="I ordered chicken soup.",                ExampleMeaning="Tôi gọi súp gà.",                    Level="A1", Topic="food" },
        new() { Id=26, Word="fruit",      Phonetic="/fruːt/",           Meaning="trái cây",              Example="Fruits are healthy snacks.",             ExampleMeaning="Trái cây là đồ ăn nhẹ lành mạnh.",  Level="A1", Topic="food" },
        new() { Id=27, Word="coffee",     Phonetic="/ˈkɒfi/",           Meaning="cà phê",                Example="I drink coffee every morning.",           ExampleMeaning="Tôi uống cà phê mỗi buổi sáng.",    Level="A1", Topic="food" },
        new() { Id=28, Word="milk",       Phonetic="/mɪlk/",            Meaning="sữa",                   Example="Children need to drink milk.",           ExampleMeaning="Trẻ em cần uống sữa.",               Level="A1", Topic="food" },
        new() { Id=29, Word="egg",        Phonetic="/eɡ/",              Meaning="quả trứng",             Example="I eat two eggs every morning.",          ExampleMeaning="Tôi ăn hai quả trứng mỗi sáng.",    Level="A1", Topic="food" },
        new() { Id=30, Word="vegetable",  Phonetic="/ˈvedʒtəbl/",       Meaning="rau củ",                Example="Eat more vegetables every day.",         ExampleMeaning="Hãy ăn nhiều rau hơn mỗi ngày.",    Level="A1", Topic="food" },

        // ── A1 · TRAVEL ─────────────────────────────────────────────────────
        new() { Id=31, Word="car",        Phonetic="/kɑːr/",            Meaning="xe hơi",                Example="He drives a red car.",                   ExampleMeaning="Anh ấy lái một chiếc xe màu đỏ.",   Level="A1", Topic="travel" },
        new() { Id=32, Word="bus",        Phonetic="/bʌs/",             Meaning="xe buýt",               Example="I take the bus to work.",                ExampleMeaning="Tôi đi xe buýt đến nơi làm việc.",  Level="A1", Topic="travel" },
        new() { Id=33, Word="ticket",     Phonetic="/ˈtɪkɪt/",          Meaning="vé",                    Example="I need a ticket to Hanoi.",              ExampleMeaning="Tôi cần vé đến Hà Nội.",             Level="A1", Topic="travel" },
        new() { Id=34, Word="map",        Phonetic="/mæp/",             Meaning="bản đồ",                Example="I need a map of the city.",              ExampleMeaning="Tôi cần bản đồ thành phố.",          Level="A1", Topic="travel" },
        new() { Id=35, Word="beach",      Phonetic="/biːtʃ/",           Meaning="bãi biển",              Example="I love going to the beach.",             ExampleMeaning="Tôi thích đi biển.",                 Level="A1", Topic="travel" },

        // ── A1 · EDUCATION ──────────────────────────────────────────────────
        new() { Id=36, Word="school",     Phonetic="/skuːl/",           Meaning="trường học",            Example="She goes to school every day.",          ExampleMeaning="Cô ấy đi học mỗi ngày.",            Level="A1", Topic="education" },
        new() { Id=37, Word="book",       Phonetic="/bʊk/",             Meaning="cuốn sách",             Example="I read a good book last night.",         ExampleMeaning="Tôi đọc một cuốn sách hay tối qua.", Level="A1", Topic="education" },
        new() { Id=38, Word="teacher",    Phonetic="/ˈtiːtʃər/",        Meaning="giáo viên",             Example="My teacher is very kind.",               ExampleMeaning="Giáo viên của tôi rất tốt bụng.",   Level="A1", Topic="education" },
        new() { Id=39, Word="learn",      Phonetic="/lɜːrn/",           Meaning="học, học tập",          Example="I learn English every day.",             ExampleMeaning="Tôi học tiếng Anh mỗi ngày.",       Level="A1", Topic="education" },
        new() { Id=40, Word="write",      Phonetic="/raɪt/",            Meaning="viết",                  Example="She writes in her diary.",               ExampleMeaning="Cô ấy viết trong nhật ký.",          Level="A1", Topic="education" },

        // ── A1 · HEALTH ─────────────────────────────────────────────────────
        new() { Id=41, Word="sleep",      Phonetic="/sliːp/",           Meaning="ngủ",                   Example="I sleep eight hours a day.",             ExampleMeaning="Tôi ngủ tám tiếng mỗi ngày.",       Level="A1", Topic="health" },
        new() { Id=42, Word="doctor",     Phonetic="/ˈdɒktər/",         Meaning="bác sĩ",                Example="The doctor said I need rest.",           ExampleMeaning="Bác sĩ nói tôi cần nghỉ ngơi.",     Level="A1", Topic="health" },
        new() { Id=43, Word="sick",       Phonetic="/sɪk/",             Meaning="ốm, bệnh",              Example="I feel sick today.",                     ExampleMeaning="Hôm nay tôi cảm thấy không khỏe.",  Level="A1", Topic="health" },
        new() { Id=44, Word="run",        Phonetic="/rʌn/",             Meaning="chạy",                  Example="He runs every morning.",                 ExampleMeaning="Anh ấy chạy mỗi buổi sáng.",        Level="A1", Topic="health" },
        new() { Id=45, Word="exercise",   Phonetic="/ˈeksəsaɪz/",       Meaning="tập thể dục",           Example="Exercise is good for health.",           ExampleMeaning="Tập thể dục tốt cho sức khỏe.",     Level="A1", Topic="health" },

        // ── A1 · WORK ───────────────────────────────────────────────────────
        new() { Id=46, Word="work",       Phonetic="/wɜːrk/",           Meaning="làm việc, công việc",   Example="He works hard every day.",               ExampleMeaning="Anh ấy làm việc chăm chỉ mỗi ngày.", Level="A1", Topic="work" },
        new() { Id=47, Word="job",        Phonetic="/dʒɒb/",            Meaning="công việc, nghề",       Example="She has a good job.",                    ExampleMeaning="Cô ấy có công việc tốt.",            Level="A1", Topic="work" },
        new() { Id=48, Word="team",       Phonetic="/tiːm/",            Meaning="đội nhóm",              Example="Our team works well together.",          ExampleMeaning="Đội của chúng tôi làm việc ăn ý.",  Level="A1", Topic="work" },
        new() { Id=49, Word="office",     Phonetic="/ˈɒfɪs/",           Meaning="văn phòng",             Example="He works in an office.",                 ExampleMeaning="Anh ấy làm việc trong văn phòng.",  Level="A1", Topic="work" },
        new() { Id=50, Word="phone",      Phonetic="/foʊn/",            Meaning="điện thoại",            Example="My phone is out of battery.",            ExampleMeaning="Điện thoại tôi hết pin.",            Level="A1", Topic="technology" },

        // ── A2 · DAILY ──────────────────────────────────────────────────────
        new() { Id=51, Word="weather",    Phonetic="/ˈweðər/",          Meaning="thời tiết",             Example="The weather is nice today.",             ExampleMeaning="Thời tiết hôm nay đẹp.",             Level="A2", Topic="daily" },
        new() { Id=52, Word="beautiful",  Phonetic="/ˈbjuːtɪfl/",       Meaning="đẹp",                   Example="What a beautiful sunset!",               ExampleMeaning="Hoàng hôn thật đẹp!",                Level="A2", Topic="daily" },
        new() { Id=53, Word="interesting",Phonetic="/ˈɪntrəstɪŋ/",      Meaning="thú vị",                Example="This book is very interesting.",         ExampleMeaning="Cuốn sách này rất thú vị.",          Level="A2", Topic="daily" },
        new() { Id=54, Word="difficult",  Phonetic="/ˈdɪfɪkəlt/",       Meaning="khó khăn",              Example="This exercise is difficult.",            ExampleMeaning="Bài tập này khó.",                   Level="A2", Topic="daily" },
        new() { Id=55, Word="important",  Phonetic="/ɪmˈpɔːrtnt/",      Meaning="quan trọng",            Example="Health is the most important thing.",    ExampleMeaning="Sức khỏe là điều quan trọng nhất.",  Level="A2", Topic="daily" },

        // ── A2 · TRAVEL ─────────────────────────────────────────────────────
        new() { Id=56, Word="airport",    Phonetic="/ˈeərpɔːrt/",       Meaning="sân bay",               Example="We need to be at the airport early.",    ExampleMeaning="Chúng ta cần đến sân bay sớm.",      Level="A2", Topic="travel" },
        new() { Id=57, Word="hotel",      Phonetic="/hoʊˈtel/",          Meaning="khách sạn",             Example="The hotel has a nice pool.",             ExampleMeaning="Khách sạn có hồ bơi đẹp.",           Level="A2", Topic="travel" },
        new() { Id=58, Word="passport",   Phonetic="/ˈpɑːspɔːrt/",      Meaning="hộ chiếu",              Example="Don't forget your passport.",            ExampleMeaning="Đừng quên hộ chiếu.",                Level="A2", Topic="travel" },
        new() { Id=59, Word="travel",     Phonetic="/ˈtrævəl/",         Meaning="du lịch, đi lại",       Example="I love to travel to new places.",        ExampleMeaning="Tôi thích du lịch đến những nơi mới.", Level="A2", Topic="travel" },
        new() { Id=60, Word="tour",       Phonetic="/tʊər/",            Meaning="chuyến tham quan",      Example="We took a city tour.",                   ExampleMeaning="Chúng tôi đi tham quan thành phố.",  Level="A2", Topic="travel" },

        // ── A2 · FOOD ───────────────────────────────────────────────────────
        new() { Id=61, Word="restaurant", Phonetic="/ˈrestərɑːnt/",     Meaning="nhà hàng",              Example="Let's have dinner at a restaurant.",     ExampleMeaning="Hãy ăn tối tại nhà hàng.",           Level="A2", Topic="food" },
        new() { Id=62, Word="delicious",  Phonetic="/dɪˈlɪʃəs/",        Meaning="ngon tuyệt",            Example="This food is delicious!",                ExampleMeaning="Món ăn này ngon tuyệt!",              Level="A2", Topic="food" },
        new() { Id=63, Word="recipe",     Phonetic="/ˈresɪpi/",          Meaning="công thức nấu ăn",      Example="This is my grandmother's recipe.",       ExampleMeaning="Đây là công thức của bà tôi.",       Level="A2", Topic="food" },
        new() { Id=64, Word="spicy",      Phonetic="/ˈspaɪsi/",          Meaning="cay",                   Example="Vietnamese food is often spicy.",        ExampleMeaning="Đồ ăn Việt Nam thường cay.",         Level="A2", Topic="food" },
        new() { Id=65, Word="sweet",      Phonetic="/swiːt/",            Meaning="ngọt",                  Example="This mango is very sweet.",              ExampleMeaning="Quả xoài này rất ngọt.",             Level="A2", Topic="food" },

        // ── A2 · WORK ───────────────────────────────────────────────────────
        new() { Id=66, Word="meeting",    Phonetic="/ˈmiːtɪŋ/",         Meaning="cuộc họp",              Example="We have a meeting at 9 AM.",             ExampleMeaning="Chúng tôi có cuộc họp lúc 9 giờ.",  Level="A2", Topic="work" },
        new() { Id=67, Word="project",    Phonetic="/ˈprɒdʒekt/",        Meaning="dự án",                 Example="We started a new project.",              ExampleMeaning="Chúng tôi bắt đầu một dự án mới.",  Level="A2", Topic="work" },
        new() { Id=68, Word="manager",    Phonetic="/ˈmænɪdʒər/",        Meaning="quản lý",               Example="She is the project manager.",            ExampleMeaning="Cô ấy là quản lý dự án.",            Level="A2", Topic="work" },
        new() { Id=69, Word="deadline",   Phonetic="/ˈdedlaɪn/",         Meaning="hạn chót",              Example="The deadline is Friday.",                ExampleMeaning="Hạn chót là thứ Sáu.",               Level="A2", Topic="work" },
        new() { Id=70, Word="salary",     Phonetic="/ˈsæləri/",          Meaning="lương",                 Example="He got a salary increase.",              ExampleMeaning="Anh ấy được tăng lương.",            Level="A2", Topic="work" },

        // ── A2 · TECHNOLOGY ─────────────────────────────────────────────────
        new() { Id=71, Word="computer",   Phonetic="/kəmˈpjuːtər/",     Meaning="máy tính",              Example="I use a computer for work.",             ExampleMeaning="Tôi dùng máy tính để làm việc.",    Level="A2", Topic="technology" },
        new() { Id=72, Word="internet",   Phonetic="/ˈɪntərnet/",        Meaning="mạng internet",         Example="The internet connects us all.",          ExampleMeaning="Internet kết nối tất cả chúng ta.",  Level="A2", Topic="technology" },
        new() { Id=73, Word="email",      Phonetic="/ˈiːmeɪl/",          Meaning="thư điện tử",           Example="Send me an email please.",               ExampleMeaning="Hãy gửi email cho tôi.",             Level="A2", Topic="technology" },
        new() { Id=74, Word="app",        Phonetic="/æp/",               Meaning="ứng dụng",              Example="Download the app on your phone.",        ExampleMeaning="Tải ứng dụng về điện thoại.",        Level="A2", Topic="technology" },
        new() { Id=75, Word="password",   Phonetic="/ˈpæswɜːrd/",        Meaning="mật khẩu",              Example="Don't share your password.",             ExampleMeaning="Đừng chia sẻ mật khẩu.",             Level="A2", Topic="technology" },

        // ── B1 · WORK ───────────────────────────────────────────────────────
        new() { Id=76, Word="experience",    Phonetic="/ɪkˈspɪəriəns/",  Meaning="kinh nghiệm, trải nghiệm", Example="She has five years of experience.", ExampleMeaning="Cô ấy có năm năm kinh nghiệm.",      Level="B1", Topic="work" },
        new() { Id=77, Word="opportunity",   Phonetic="/ˌɒpəˈtjuːnɪti/", Meaning="cơ hội",               Example="This is a great opportunity.",          ExampleMeaning="Đây là một cơ hội tuyệt vời.",       Level="B1", Topic="work" },
        new() { Id=78, Word="communication", Phonetic="/kəˌmjuːnɪˈkeɪʃn/",Meaning="giao tiếp, truyền thông",Example="Good communication is essential.",    ExampleMeaning="Giao tiếp tốt là điều cần thiết.",   Level="B1", Topic="work" },
        new() { Id=79, Word="solution",      Phonetic="/səˈluːʃn/",       Meaning="giải pháp",             Example="We need to find a solution quickly.",   ExampleMeaning="Chúng ta cần tìm giải pháp.",        Level="B1", Topic="work" },
        new() { Id=80, Word="challenge",     Phonetic="/ˈtʃælɪndʒ/",      Meaning="thách thức",            Example="Learning a language is a challenge.",   ExampleMeaning="Học ngôn ngữ là một thách thức.",    Level="B1", Topic="work" },

        // ── B1 · SOCIETY & SCIENCE ──────────────────────────────────────────
        new() { Id=81, Word="environment",   Phonetic="/ɪnˈvaɪrənmənt/",  Meaning="môi trường",            Example="We must protect the environment.",      ExampleMeaning="Chúng ta phải bảo vệ môi trường.",  Level="B1", Topic="society" },
        new() { Id=82, Word="government",    Phonetic="/ˈɡʌvənmənt/",      Meaning="chính phủ",             Example="The government announced new policies.", ExampleMeaning="Chính phủ công bố chính sách mới.", Level="B1", Topic="society" },
        new() { Id=83, Word="technology",    Phonetic="/tekˈnɒlədʒi/",     Meaning="công nghệ",             Example="Technology changes our lives.",         ExampleMeaning="Công nghệ thay đổi cuộc sống.",      Level="B1", Topic="technology" },
        new() { Id=84, Word="research",      Phonetic="/rɪˈsɜːrtʃ/",       Meaning="nghiên cứu",            Example="She is doing research on cancer.",      ExampleMeaning="Cô ấy đang nghiên cứu về ung thư.", Level="B1", Topic="science" },
        new() { Id=85, Word="culture",       Phonetic="/ˈkʌltʃər/",        Meaning="văn hóa",               Example="I am interested in Japanese culture.",  ExampleMeaning="Tôi quan tâm đến văn hóa Nhật Bản.", Level="B1", Topic="society" },
        new() { Id=86, Word="relationship",  Phonetic="/rɪˈleɪʃnʃɪp/",    Meaning="mối quan hệ",           Example="They have a good relationship.",        ExampleMeaning="Họ có mối quan hệ tốt.",             Level="B1", Topic="daily" },
        new() { Id=87, Word="improve",       Phonetic="/ɪmˈpruːv/",        Meaning="cải thiện",             Example="I want to improve my English.",         ExampleMeaning="Tôi muốn cải thiện tiếng Anh.",     Level="B1", Topic="education" },
        new() { Id=88, Word="achieve",       Phonetic="/əˈtʃiːv/",         Meaning="đạt được",              Example="She achieved her goal.",                ExampleMeaning="Cô ấy đã đạt được mục tiêu.",       Level="B1", Topic="work" },

        // ── B2 ──────────────────────────────────────────────────────────────
        new() { Id=89, Word="ambitious",     Phonetic="/æmˈbɪʃəs/",        Meaning="có tham vọng",          Example="She is an ambitious young woman.",      ExampleMeaning="Cô ấy là người phụ nữ đầy tham vọng.", Level="B2", Topic="work" },
        new() { Id=90, Word="consequence",   Phonetic="/ˈkɒnsɪkwəns/",     Meaning="hệ quả, hậu quả",       Example="Think about the consequences.",         ExampleMeaning="Hãy suy nghĩ về hậu quả.",           Level="B2", Topic="daily" },
        new() { Id=91, Word="negotiate",     Phonetic="/nɪˈɡoʊʃieɪt/",     Meaning="đàm phán",              Example="They need to negotiate a deal.",        ExampleMeaning="Họ cần đàm phán một thỏa thuận.",   Level="B2", Topic="work" },
        new() { Id=92, Word="perspective",   Phonetic="/pərˈspektɪv/",      Meaning="quan điểm, góc nhìn",   Example="Try to see it from her perspective.",   ExampleMeaning="Hãy thử nhìn từ góc độ của cô ấy.", Level="B2", Topic="daily" },
        new() { Id=93, Word="sustainable",   Phonetic="/səˈsteɪnəbl/",      Meaning="bền vững",              Example="We need sustainable development.",      ExampleMeaning="Chúng ta cần phát triển bền vững.",  Level="B2", Topic="science" },
        new() { Id=94, Word="innovation",    Phonetic="/ˌɪnəˈveɪʃn/",       Meaning="đổi mới, sáng tạo",     Example="Innovation drives economic growth.",    ExampleMeaning="Đổi mới thúc đẩy tăng trưởng KT.",  Level="B2", Topic="technology" },
        new() { Id=95, Word="collaborate",   Phonetic="/kəˈlæbəreɪt/",      Meaning="cộng tác, hợp tác",     Example="Scientists collaborate across borders.", ExampleMeaning="Các nhà khoa học hợp tác xuyên biên giới.", Level="B2", Topic="work" },

        // ── C1 ──────────────────────────────────────────────────────────────
        new() { Id=96,  Word="resilience",   Phonetic="/rɪˈzɪliəns/",       Meaning="sự kiên cường",         Example="Resilience is key to success.",         ExampleMeaning="Sự kiên cường là chìa khóa.",        Level="C1", Topic="work" },
        new() { Id=97,  Word="meticulous",   Phonetic="/mɪˈtɪkjələs/",      Meaning="tỉ mỉ, cẩn thận",       Example="She is meticulous in her work.",        ExampleMeaning="Cô ấy rất tỉ mỉ trong công việc.",  Level="C1", Topic="work" },
        new() { Id=98,  Word="eloquent",     Phonetic="/ˈeləkwənt/",         Meaning="hùng hồn, lưu loát",    Example="She gave an eloquent speech.",          ExampleMeaning="Cô ấy đọc bài phát biểu hùng hồn.", Level="C1", Topic="daily" },
        new() { Id=99,  Word="pragmatic",    Phonetic="/præɡˈmætɪk/",        Meaning="thực dụng, thực tế",    Example="Take a pragmatic approach.",            ExampleMeaning="Hãy tiếp cận vấn đề thực dụng.",    Level="C1", Topic="daily" },
        new() { Id=100, Word="integrity",    Phonetic="/ɪnˈteɡrɪti/",        Meaning="tính liêm chính",       Example="He is known for his integrity.",        ExampleMeaning="Anh ấy nổi tiếng về sự liêm chính.", Level="C1", Topic="work" },
    };
}
