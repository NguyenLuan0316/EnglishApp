START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__AuthMigrationsHistory" WHERE "MigrationId" = '20261116163243_AddDailyLearning') THEN
    CREATE TABLE user_content_progress (
        "UserId" uuid NOT NULL,
        "ContentType" character varying(20) NOT NULL,
        "ContentId" integer NOT NULL,
        "CorrectCount" integer NOT NULL,
        "WrongCount" integer NOT NULL,
        "NextReview" timestamp with time zone NOT NULL,
        "LastReviewed" timestamp with time zone,
        CONSTRAINT "PK_user_content_progress" PRIMARY KEY ("UserId", "ContentType", "ContentId"),
        CONSTRAINT "CK_user_content_progress_type" CHECK ("ContentType" IN ('grammar', 'patterns')),
        CONSTRAINT "FK_user_content_progress_app_users_UserId" FOREIGN KEY ("UserId") REFERENCES app_users ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__AuthMigrationsHistory" WHERE "MigrationId" = '20261116163243_AddDailyLearning') THEN
    CREATE TABLE user_daily_lessons (
        "UserId" uuid NOT NULL,
        "StudyDate" date NOT NULL,
        "ContentType" character varying(20) NOT NULL,
        "ContentId" integer NOT NULL,
        "Position" integer NOT NULL,
        "IsNew" boolean NOT NULL,
        "CompletedAt" timestamp with time zone,
        "Correct" boolean,
        CONSTRAINT "PK_user_daily_lessons" PRIMARY KEY ("UserId", "StudyDate", "ContentType", "ContentId"),
        CONSTRAINT "CK_user_daily_lessons_type" CHECK ("ContentType" IN ('vocabulary', 'grammar', 'patterns')),
        CONSTRAINT "FK_user_daily_lessons_app_users_UserId" FOREIGN KEY ("UserId") REFERENCES app_users ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__AuthMigrationsHistory" WHERE "MigrationId" = '20261116163243_AddDailyLearning') THEN
    CREATE TABLE user_study_activity (
        "UserId" uuid NOT NULL,
        "StudyDate" date NOT NULL,
        "Attempts" integer NOT NULL,
        "CorrectCount" integer NOT NULL,
        "Xp" integer NOT NULL,
        CONSTRAINT "PK_user_study_activity" PRIMARY KEY ("UserId", "StudyDate"),
        CONSTRAINT "FK_user_study_activity_app_users_UserId" FOREIGN KEY ("UserId") REFERENCES app_users ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__AuthMigrationsHistory" WHERE "MigrationId" = '20261116163243_AddDailyLearning') THEN
    INSERT INTO "__AuthMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20261116163243_AddDailyLearning', '8.0.25');
    END IF;
END $EF$;
COMMIT;

