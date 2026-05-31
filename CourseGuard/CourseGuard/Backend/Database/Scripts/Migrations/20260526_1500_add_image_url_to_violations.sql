-- Migration: Thêm cột IMAGE_URL vào bảng VIOLATIONS
-- Ngày: 2026-05-26
-- Mục đích: Lưu trữ ảnh chụp màn hình vi phạm (upload lên Supabase Storage bucket 'exam-violations')

BEGIN;

ALTER TABLE "VIOLATIONS" ADD COLUMN IF NOT EXISTS "IMAGE_URL" VARCHAR(500) DEFAULT NULL;

COMMENT ON COLUMN "VIOLATIONS"."IMAGE_URL" IS 'URL ảnh chụp màn hình vi phạm lấy từ Supabase Storage';

COMMIT;
