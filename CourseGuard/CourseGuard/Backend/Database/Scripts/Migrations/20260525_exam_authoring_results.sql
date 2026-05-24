-- 2026-05-25: exam draft authoring, MCQ questions, and per-student result hiding

ALTER TABLE exams
    ADD COLUMN IF NOT EXISTS status VARCHAR(20) NOT NULL DEFAULT 'DRAFT';

ALTER TABLE exams
    ALTER COLUMN status SET DEFAULT 'DRAFT';

UPDATE exams
SET status = CASE
    WHEN close_time IS NOT NULL AND close_time < CURRENT_TIMESTAMP THEN 'CLOSED'
    ELSE 'DRAFT'
END
WHERE status IS NULL OR TRIM(status) = '';

CREATE TABLE IF NOT EXISTS exam_questions (
    id SERIAL PRIMARY KEY,
    exam_id INT NOT NULL REFERENCES exams(id) ON DELETE CASCADE,
    question_text TEXT NOT NULL,
    option_a TEXT NOT NULL,
    option_b TEXT NOT NULL,
    option_c TEXT NOT NULL,
    option_d TEXT NOT NULL,
    correct_option CHAR(1) NOT NULL DEFAULT 'A',
    points NUMERIC(6,2) NOT NULL DEFAULT 0,
    display_order INT NOT NULL DEFAULT 1,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

DO $$
BEGIN
    ALTER TABLE exam_questions
        ADD COLUMN IF NOT EXISTS id INTEGER;

    CREATE SEQUENCE IF NOT EXISTS exam_questions_id_seq;

    UPDATE exam_questions
    SET id = nextval('exam_questions_id_seq')
    WHERE id IS NULL;

    PERFORM setval(
        'exam_questions_id_seq',
        COALESCE((SELECT MAX(id) FROM exam_questions), 1),
        (SELECT COALESCE(MAX(id), 0) > 0 FROM exam_questions)
    );

    ALTER TABLE exam_questions
        ALTER COLUMN id SET DEFAULT nextval('exam_questions_id_seq');

    IF NOT EXISTS (
        SELECT 1
        FROM pg_index i
        WHERE i.indrelid = 'exam_questions'::regclass
          AND i.indisprimary
    ) THEN
        ALTER TABLE exam_questions
            ADD CONSTRAINT exam_questions_pkey PRIMARY KEY (id);
    END IF;
END $$;

ALTER TABLE exam_questions
    ADD COLUMN IF NOT EXISTS question_text TEXT,
    ADD COLUMN IF NOT EXISTS option_a TEXT,
    ADD COLUMN IF NOT EXISTS option_b TEXT,
    ADD COLUMN IF NOT EXISTS option_c TEXT,
    ADD COLUMN IF NOT EXISTS option_d TEXT,
    ADD COLUMN IF NOT EXISTS correct_option CHAR(1) DEFAULT 'A',
    ADD COLUMN IF NOT EXISTS points NUMERIC(6,2) DEFAULT 0,
    ADD COLUMN IF NOT EXISTS display_order INT DEFAULT 1;

CREATE INDEX IF NOT EXISTS idx_exams_status ON exams(status);
CREATE INDEX IF NOT EXISTS idx_exam_questions_exam ON exam_questions(exam_id);
CREATE UNIQUE INDEX IF NOT EXISTS ux_exam_questions_exam_order ON exam_questions(exam_id, display_order);

CREATE TABLE IF NOT EXISTS student_hidden_results (
    student_id INT NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    attempt_id INT NOT NULL REFERENCES exam_attempts(id) ON DELETE CASCADE,
    hidden_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (student_id, attempt_id)
);

CREATE INDEX IF NOT EXISTS idx_student_hidden_results_student ON student_hidden_results(student_id);
