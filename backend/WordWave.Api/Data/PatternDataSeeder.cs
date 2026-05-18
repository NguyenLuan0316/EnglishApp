using Microsoft.EntityFrameworkCore;
using WordWave.Domain.Models;
using WordWave.Infrastructure.Data;

namespace WordWave.Api.Data;

public static class PatternDataSeeder
{
    private const int ExpectedCount = 500;

    public static async Task SeedPatternDataAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var seeds = BuildDailyPatterns().Take(ExpectedCount).ToList();
        var existing = await db.SentencePatterns.OrderBy(x => x.Id).ToListAsync();

        if (existing.Count == ExpectedCount
            && existing.Select(x => x.Sentence).SequenceEqual(seeds.Select(x => x.Sentence))
            && existing.Select(x => x.Type).SequenceEqual(seeds.Select(x => x.Type)))
        {
            return;
        }

        db.SentencePatterns.RemoveRange(existing);
        await db.SaveChangesAsync();

        var createdAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        await db.SentencePatterns.AddRangeAsync(seeds.Select(seed => new SentencePattern
        {
            Sentence = seed.Sentence,
            Type = seed.Type,
            Meaning = seed.Meaning,
            Explanation = seed.Explanation,
            Examples = seed.Examples,
            CreatedAt = createdAt
        }));
        await db.SaveChangesAsync();
    }

    private sealed record SeedPattern(string Sentence, string Type, string Meaning, string Explanation, string[] Examples);
    private sealed record PhraseGroup(string Type, string Meaning, string Explanation, string[] Sentences);

    private static IEnumerable<SeedPattern> BuildDailyPatterns()
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var group in Groups)
        {
            foreach (var sentence in group.Sentences)
            {
                if (!seen.Add(sentence))
                {
                    continue;
                }

                yield return new SeedPattern(
                    sentence,
                    group.Type,
                    group.Meaning,
                    group.Explanation,
                    BuildExamples(sentence));
            }
        }
    }

    private static string[] BuildExamples(string sentence)
    {
        return
        [
            $"A: {sentence}",
            "B: Sure, no problem."
        ];
    }

    private static readonly PhraseGroup[] Groups =
    [
        new("greetings", "Chào hỏi và bắt đầu cuộc trò chuyện.", "Dùng khi gặp ai đó, mở đầu một cuộc nói chuyện hoặc tạo không khí thân thiện.", [
            "Hi, how's it going?", "Hey, good to see you.", "Long time no see.", "How have you been?", "How's your day going?",
            "What's new with you?", "How are things?", "It's nice to meet you.", "I've heard a lot about you.", "Thanks for coming.",
            "I'm glad you could make it.", "What brings you here?", "I hope you're doing well.", "How was your weekend?", "How's everything at work?",
            "You look well.", "It's been a while.", "I'm happy to see you.", "What have you been up to?", "Let me introduce myself."
        ]),
        new("small-talk", "Nói chuyện xã giao hằng ngày.", "Dùng để duy trì cuộc trò chuyện nhẹ nhàng trong đời sống thường ngày.", [
            "The weather is really nice today.", "It looks like it's going to rain.", "This place is busier than usual.", "I love the atmosphere here.", "It's quieter than I expected.",
            "I haven't been here before.", "Do you come here often?", "This song sounds familiar.", "That smells amazing.", "The traffic was terrible today.",
            "I got here earlier than expected.", "I almost missed the bus.", "I need another coffee.", "Today feels like a long day.", "The weekend went by so fast.",
            "I didn't sleep well last night.", "I have a lot going on today.", "I'm taking it easy today.", "I could use a break.", "Let's catch up soon."
        ]),
        new("introductions", "Giới thiệu bản thân và hỏi thông tin cá nhân.", "Dùng khi làm quen hoặc trao đổi thông tin cơ bản với người khác.", [
            "My name is Anna.", "You can call me Alex.", "I work in marketing.", "I'm studying English at the moment.", "I live near the city center.",
            "I'm originally from Da Nang.", "I moved here last year.", "What do you do?", "Where are you from?", "How long have you lived here?",
            "Do you live around here?", "What are you studying?", "What line of work are you in?", "Do you have any hobbies?", "What do you usually do after work?",
            "I'm into photography.", "I enjoy trying new food.", "I spend a lot of time reading.", "I'm learning this for work.", "I'm here with a friend."
        ]),
        new("requests", "Nhờ giúp đỡ.", "Dùng khi cần người khác hỗ trợ một cách tự nhiên và lịch sự.", [
            "Could you give me a hand?", "Can you help me out?", "Would you mind helping me?", "I could really use your help.", "Do you have a minute to help me?",
            "Can I ask you for a favor?", "Could you do me a favor?", "I need a little help here.", "Would it be possible for you to help?", "Can you take a look at this?",
            "Could you show me how this works?", "Can you walk me through it?", "Would you help me figure this out?", "Can you help me carry this?", "Could you check this for me?",
            "Can you point me in the right direction?", "Could you lend me a hand?", "I don't know how to do this.", "I'm stuck on this.", "Could you spare a moment?"
        ]),
        new("clarification", "Làm rõ thông tin.", "Dùng khi chưa hiểu, cần người khác giải thích, nhắc lại hoặc nói chậm hơn.", [
            "What do you mean?", "Could you explain that again?", "Can you say that another way?", "I'm not sure I follow.", "I didn't quite catch that.",
            "Could you repeat the last part?", "Can you speak more slowly?", "Could you give me an example?", "What does that word mean?", "Do you mean this one?",
            "Are you saying we should wait?", "Just to clarify, are we leaving now?", "Let me make sure I understand.", "Could you be more specific?", "Which part are you talking about?",
            "Can you clarify the deadline?", "I want to double-check something.", "Could you spell that for me?", "Can you write that down?", "Is that clear to everyone?"
        ]),
        new("confirmation", "Xác nhận và kiểm tra lại.", "Dùng để xác nhận thông tin, thời gian, địa điểm hoặc quyết định.", [
            "Is that right?", "Are we still meeting today?", "Did I get that correctly?", "So the plan is confirmed?", "Can I confirm the time?",
            "Just checking, is it at three?", "Let me confirm the address.", "Can you confirm the details?", "We're meeting at the usual place, right?", "Is everyone okay with that?",
            "Does that work for you?", "Are you sure about that?", "Should I go ahead?", "Can we lock that in?", "Is this the final version?",
            "Have you received my message?", "Did you get my email?", "Can you let me know once it's done?", "Please tell me if anything changes.", "I'll check and get back to you."
        ]),
        new("opinions", "Nêu ý kiến.", "Dùng để trình bày quan điểm cá nhân trong giao tiếp hoặc thảo luận.", [
            "I think it's a good idea.", "In my opinion, we should wait.", "I feel like this is better.", "To me, it sounds reasonable.", "From my point of view, it's worth trying.",
            "I would say it's too early.", "It seems like a simple solution.", "I believe we can do better.", "Personally, I prefer the first option.", "As far as I can tell, it's working.",
            "If you ask me, we should keep it simple.", "The way I see it, we need more time.", "I'm not convinced yet.", "I have mixed feelings about it.", "I see your point, but I disagree.",
            "That makes sense to me.", "I don't think that's necessary.", "It might be a bit risky.", "It depends on the situation.", "I'd rather choose the safer option."
        ]),
        new("agreement", "Đồng ý và ủng hộ.", "Dùng để thể hiện sự đồng tình hoặc ủng hộ ý kiến của người khác.", [
            "I agree with you.", "That's exactly what I think.", "You have a good point.", "That sounds right to me.", "I'm with you on that.",
            "I couldn't agree more.", "That's a fair point.", "I see what you mean.", "That makes perfect sense.", "You're absolutely right.",
            "I was thinking the same thing.", "That works for me.", "I'm okay with that.", "Let's go with your idea.", "I support that decision.",
            "That seems reasonable.", "I'm happy with that plan.", "I think we're on the same page.", "That's a smart approach.", "I can live with that."
        ]),
        new("disagreement", "Không đồng ý một cách lịch sự.", "Dùng khi muốn phản hồi trái ý nhưng vẫn giữ lịch sự.", [
            "I'm not sure I agree.", "I see it differently.", "I understand your point, but I disagree.", "That might not be the best option.", "I'm afraid I don't see it that way.",
            "I have a different opinion.", "I'm not completely convinced.", "There may be another way to look at it.", "I don't think that's quite right.", "That doesn't really work for me.",
            "I would prefer a different approach.", "I'm hesitant about that idea.", "I don't feel comfortable with that.", "That sounds a little risky.", "Could we consider another option?",
            "I respect your view, but I disagree.", "I'm not against it, but I have concerns.", "It might cause some problems.", "I think we should rethink it.", "Let's look at the downside first."
        ]),
        new("suggestions", "Đưa ra gợi ý.", "Dùng khi muốn đề xuất hành động, lựa chọn hoặc giải pháp.", [
            "Why don't we try this?", "How about taking a short break?", "Maybe we should ask someone else.", "We could do it tomorrow.", "Let's start with the easiest part.",
            "It might help to make a list.", "I suggest we leave earlier.", "You might want to check the details.", "Have you thought about calling them?", "What if we change the plan?",
            "Let's keep it simple.", "We should compare both options.", "It would be better to wait.", "Maybe try a different way.", "Let's ask for more information.",
            "You could send a message first.", "I recommend booking in advance.", "It may be useful to practice more.", "Let's not rush into it.", "We can decide after lunch."
        ]),
        new("invitations", "Mời và rủ ai đó.", "Dùng khi mời người khác tham gia hoạt động hoặc sự kiện.", [
            "Would you like to join us?", "Do you want to grab coffee?", "Are you free tonight?", "Do you feel like going out?", "Let's have lunch together.",
            "How about dinner this weekend?", "Would you like to come with me?", "Do you want to watch a movie?", "Let's go for a walk.", "Are you up for a quick meeting?",
            "Can you make it tomorrow?", "Would you be interested in joining?", "Do you want to come over?", "Let's hang out sometime.", "Are you available after work?",
            "Would Friday work for you?", "Let's catch a drink later.", "Do you want to try that new place?", "Can I invite you to the event?", "I'd love for you to join us."
        ]),
        new("planning", "Lên kế hoạch.", "Dùng để bàn kế hoạch, sắp xếp việc cần làm hoặc thống nhất bước tiếp theo.", [
            "Let's make a plan.", "We need to decide what to do next.", "I'll handle the first part.", "You can take care of the rest.", "Let's split the work.",
            "We should set a deadline.", "I'll send you the details later.", "Let's meet at the main entrance.", "We can start at nine.", "I'll book the table.",
            "Let's leave a little earlier.", "We need to prepare in advance.", "I'll bring the documents.", "You bring the laptop.", "Let's check the schedule first.",
            "We can change the plan if needed.", "I'll call you before I leave.", "Let's keep each other updated.", "We should have a backup plan.", "Let's confirm everything tonight."
        ]),
        new("scheduling", "Thời gian và lịch hẹn.", "Dùng để hỏi, đổi, xác nhận hoặc sắp xếp thời gian.", [
            "What time works for you?", "I'm free after three.", "Can we move it to tomorrow?", "I'm running a little late.", "I'll be there in ten minutes.",
            "Can we reschedule?", "Something came up.", "Let's do it next week.", "I have another appointment.", "The timing doesn't work for me.",
            "Could we make it earlier?", "Could we make it later?", "I'm available all morning.", "I only have thirty minutes.", "Let's not take too long.",
            "I need to leave by six.", "I'll check my calendar.", "Please remind me tomorrow.", "Let's set a time now.", "Sorry for the short notice."
        ]),
        new("phone-messaging", "Gọi điện và nhắn tin.", "Dùng trong cuộc gọi, tin nhắn, email hoặc họp online.", [
            "Can you hear me?", "You're breaking up.", "The connection is not stable.", "I'll call you back.", "Can I put you on hold?",
            "Please leave a message.", "I'll text you the address.", "Send me the link, please.", "I just sent you an email.", "Did you see my message?",
            "I'll reply as soon as I can.", "Sorry for the late reply.", "Can we talk on the phone?", "Let's switch to video call.", "My camera isn't working.",
            "You're on mute.", "Could you share your screen?", "I can't open the file.", "The link doesn't work.", "I'll forward it to you."
        ]),
        new("dining", "Nhà hàng và đồ ăn.", "Dùng khi gọi món, đặt bàn, hỏi thông tin hoặc thanh toán.", [
            "A table for two, please.", "Do you have a reservation?", "Can I see the menu?", "What do you recommend?", "I'll have the chicken.",
            "Can I get this without onions?", "Is this dish spicy?", "Could we have some water?", "Can we order now?", "The food looks great.",
            "This is delicious.", "Could I get the bill?", "Can we split the bill?", "Is service included?", "Can I pay by card?",
            "I'd like this to go.", "Can you pack this for me?", "We're still deciding.", "Could we have one more minute?", "Do you have any vegetarian options?"
        ]),
        new("shopping", "Mua sắm.", "Dùng khi mua hàng, hỏi giá, đổi trả hoặc thanh toán.", [
            "How much does this cost?", "Do you have this in another size?", "Can I try this on?", "Where is the fitting room?", "Do you have a cheaper one?",
            "Is this on sale?", "Can I get a discount?", "I'll take this one.", "I'm just looking, thanks.", "Do you accept credit cards?",
            "Can I return this?", "Do you have the receipt?", "This doesn't fit me.", "I'm looking for a gift.", "Could you wrap it, please?",
            "Do you have this in blue?", "Can you check the stock?", "Where can I find shoes?", "This is a bit too expensive.", "I'll think about it."
        ]),
        new("travel", "Du lịch và hỏi đường.", "Dùng khi di chuyển, hỏi đường, đặt vé hoặc xử lý tình huống khi đi du lịch.", [
            "How do I get to the station?", "Is it far from here?", "Can I walk there?", "Which bus should I take?", "Where can I buy a ticket?",
            "Does this train go downtown?", "I think I'm lost.", "Can you show me on the map?", "Is there a taxi stand nearby?", "How long does it take to get there?",
            "Where is the nearest restroom?", "I'm looking for this address.", "Can you recommend a hotel?", "I'd like to check in.", "Can I leave my luggage here?",
            "What time is checkout?", "Is breakfast included?", "My flight was delayed.", "I missed my train.", "Could you help me find my gate?"
        ]),
        new("work", "Công việc và văn phòng.", "Dùng trong môi trường làm việc, họp hành và trao đổi công việc.", [
            "I'm working on the report.", "Can we discuss the project?", "Let's review the agenda.", "The deadline is tomorrow.", "I need your feedback.",
            "I'll update the file.", "Can you send me the latest version?", "Let's focus on the main issue.", "We need to follow up.", "I'll take notes.",
            "Could you join the meeting?", "Let's move to the next point.", "We are behind schedule.", "We are on track.", "I'll handle the client call.",
            "Let's keep this brief.", "Can you summarize the discussion?", "I'll share the minutes later.", "We need approval first.", "Let's close the loop on this."
        ]),
        new("problem-solving", "Giải quyết vấn đề.", "Dùng khi có lỗi, sự cố hoặc cần tìm giải pháp.", [
            "Something went wrong.", "There seems to be a problem.", "Let's figure it out.", "We need to fix this.", "I found the issue.",
            "The system isn't working.", "It keeps happening.", "Let's try again.", "Maybe we missed something.", "Can you reproduce the issue?",
            "I need more information.", "Let's check the settings.", "The problem is solved.", "That didn't work.", "Let's look for another solution.",
            "We should report this.", "I'll investigate it.", "Can you send me a screenshot?", "Let's test it one more time.", "I think I know what happened."
        ]),
        new("apologies", "Xin lỗi và giải thích.", "Dùng khi mắc lỗi, đến trễ, gây phiền hoặc cần giải thích lý do.", [
            "I'm sorry I'm late.", "Sorry to bother you.", "I apologize for the mistake.", "That was my fault.", "I didn't mean to do that.",
            "Sorry for the confusion.", "I should have told you earlier.", "I misunderstood the instructions.", "I forgot to send it.", "I couldn't make it on time.",
            "Please accept my apology.", "I'll make sure it doesn't happen again.", "Let me fix that.", "I take responsibility for it.", "Thanks for your patience.",
            "Sorry, I didn't notice that.", "I was caught in traffic.", "Something urgent came up.", "I hope you understand.", "I'll be more careful next time."
        ]),
        new("thanks", "Cảm ơn và đáp lại lời cảm ơn.", "Dùng để thể hiện sự biết ơn và phản hồi lịch sự.", [
            "Thank you so much.", "Thanks a lot.", "I really appreciate it.", "That's very kind of you.", "Thanks for your help.",
            "Thanks for letting me know.", "Thanks for your time.", "I appreciate your support.", "You made my day.", "I owe you one.",
            "No problem at all.", "You're welcome.", "Anytime.", "Don't mention it.", "I'm happy to help.",
            "It was nothing.", "Glad I could help.", "Thanks anyway.", "I appreciate the offer.", "That means a lot to me."
        ]),
        new("feelings", "Cảm xúc và trạng thái.", "Dùng để nói bạn đang cảm thấy thế nào hoặc phản ứng với tình huống.", [
            "I'm feeling great today.", "I'm a bit tired.", "I'm not in the mood.", "I'm excited about it.", "I'm nervous about the interview.",
            "I'm disappointed with the result.", "I'm relieved to hear that.", "I'm confused right now.", "I'm worried about the cost.", "I'm proud of you.",
            "That sounds exciting.", "That's frustrating.", "That's a relief.", "What a surprise!", "I can't believe it.",
            "I'm looking forward to it.", "I'm not feeling well.", "I need some rest.", "I'm under a lot of pressure.", "I feel much better now."
        ]),
        new("health", "Sức khỏe và đời sống.", "Dùng khi nói về sức khỏe, nghỉ ngơi, thói quen hoặc sinh hoạt hằng ngày.", [
            "I have a headache.", "I need to see a doctor.", "I don't feel well.", "I caught a cold.", "I need some medicine.",
            "You should get some rest.", "Drink plenty of water.", "I slept badly last night.", "I need to exercise more.", "I'm trying to eat healthier.",
            "I have an appointment with the dentist.", "My back hurts.", "I feel dizzy.", "I have a sore throat.", "I need to take a day off.",
            "I feel stressed lately.", "I'm trying to sleep earlier.", "I went for a run this morning.", "I need to slow down.", "Take care of yourself."
        ]),
        new("english-learning", "Học tiếng Anh.", "Dùng khi học, luyện nói, hỏi nghĩa, sửa lỗi hoặc luyện phát âm.", [
            "How do you pronounce this word?", "What does this sentence mean?", "Can you correct my pronunciation?", "I'm trying to improve my speaking.", "I need to practice more.",
            "Could you speak a little slower?", "How can I say this naturally?", "Is this sentence correct?", "What's the difference between these words?", "Can you give me a better example?",
            "I don't know how to express this.", "I want to sound more natural.", "Could you check my grammar?", "I'm learning new vocabulary.", "Let's practice this conversation.",
            "Can you repeat after me?", "I made a small mistake.", "I need feedback on my speaking.", "This phrase is useful.", "I'll try to use it today."
        ]),
        new("closings", "Kết thúc cuộc trò chuyện.", "Dùng khi muốn kết thúc, tạm biệt hoặc hẹn nói chuyện sau.", [
            "I should get going.", "I have to run.", "It was nice talking to you.", "Let's talk again soon.", "I'll see you later.",
            "Take care.", "Have a good one.", "Enjoy the rest of your day.", "Thanks for chatting.", "Let's keep in touch.",
            "I'll let you get back to work.", "Sorry, I need to go now.", "Let's continue this later.", "I'll message you later.", "See you tomorrow.",
            "Have a safe trip.", "Good luck with everything.", "Don't work too hard.", "Talk to you soon.", "Bye for now."
        ])
    ];
}
