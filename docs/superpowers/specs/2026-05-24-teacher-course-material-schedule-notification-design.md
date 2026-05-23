# Teacher Course, Material, Schedule, Overview, and Notification Design

## Purpose

Hoan thien luong nghiep vu giao vien trong CourseGuard de Teacher co the tao khoa hoc, gui duyet, quan ly tai lieu va lich day; Student chi nhin thay khoa hoc/tai lieu/lich hoc hop le sau khi course duoc Admin duyet va Student duoc chap nhan ghi danh. Dong thoi lam ro block tong quan cua Teacher va page thong bao theo cac su kien can biet/can xu ly.

## Scope

Trong pham vi:

- Teacher tao/sua course nhung khong duoc tu set course thanh `ACTIVE`.
- Admin duyet course moi co the dua course sang `ACTIVE`.
- Student page `Tim khoa hoc` chi hien course `ACTIVE`.
- Student gui yeu cau ghi danh se tao notification cho Teacher.
- Teacher duyet/tu choi ghi danh se tao notification cho Student.
- Teacher upload tai lieu vao course cu the.
- Teacher Materials co dropdown loc theo `Toan bo khoa hoc` hoac tung course cua Teacher.
- Student Materials chi hien tai lieu cua course ma Student da duoc duyet/dang hoc.
- Teacher tao course kem lich lap mac dinh; he thong sinh cac `online_sessions` cu the.
- Teacher co the sua/xoa/them tung buoi trong page `Lich day`.
- Teacher Overview doi block `Thong bao gan day` thanh `Lich day & viec can xu ly`.
- Teacher Notifications gom notification nghiep vu, co category va UX type `Informational`/`ActionRequired`.
- Empty state cho overview task block, notifications, materials, schedules, student course search.
- Ownership filtering bat buoc cho moi thao tac Teacher va Student.

Ngoai pham vi:

- Khong rewrite lai Student role UI.
- Khong xay event bus day du.
- Khong them realtime notification push neu repo chua co nen tang.
- Khong doi toan bo schema lon neu co the dung migration nho.
- Khong dua login/logout/mo dashboard/xem khoa hoc vao notification.

## Course Approval Flow

Course status nen gom:

- `DRAFT`: Teacher dang soan, Student khong thay.
- `PENDING`: Teacher da gui duyet, cho Admin xu ly, Student khong thay.
- `ACTIVE`: Admin da duyet, Student co the thay trong `Tim khoa hoc`.
- `REJECTED`: Admin tu choi, Student khong thay; Teacher thay ly do neu schema/UI ho tro.
- `CLOSED`: Course da dong/ket thuc, Student khong the ghi danh moi.

Teacher UI:

- Tao course moi mac dinh `DRAFT`.
- Nut `Gui duyet` chuyen course tu `DRAFT` hoac `REJECTED` sang `PENDING`.
- Neu implementation phase dau muon toi gian UI, form tao course co the tao thang `PENDING`, nhung Teacher van khong duoc tao `ACTIVE`.
- Teacher khong duoc chon `ACTIVE`.
- Status hien thi dang badge read-only.
- Neu course `REJECTED`, Teacher co the sua thong tin va gui duyet lai thanh `PENDING`.

Admin UI/backend:

- Admin la nguoi duy nhat chuyen `PENDING -> ACTIVE`.
- Admin co the chuyen `PENDING -> REJECTED`.
- Neu Admin tu choi, nen tao notification cho Teacher.

Student UI/backend:

- `Tim khoa hoc` chi query course co `status = ACTIVE`.
- Student khong thay `DRAFT`, `PENDING`, `REJECTED`, `CLOSED`.

## Course Creation And Generated Schedule

Teacher course form co hai nhom thong tin:

1. Thong tin course:
   - Ten khoa hoc
   - Mo ta
   - Ngay bat dau
   - Ngay ket thuc
   - Trang thai read-only

2. Lich day mac dinh:
   - Mot hoac nhieu thu trong tuan
   - Gio bat dau
   - Gio ket thuc
   - Link hoc/meeting link

Rule:

- Lich mac dinh chi la dau vao luc tao/gia han course.
- `online_sessions` la cac buoi hoc cu the da duoc sinh ra tu lich mac dinh.
- Sau khi sinh `online_sessions`, Teacher sua tung buoi rieng le trong page `Lich day`.
- Sua tung buoi khong can sua lai lich mac dinh ban dau.
- Neu Teacher thay doi ngay bat dau/ket thuc course sau khi da sinh lich, can co hanh vi ro: chi cap nhat course metadata, khong tu dong xoa/sinh lai sessions tru khi UI co command rieng.

Recommended initial behavior:

- Khi tao course moi va co lich lap, backend sinh `online_sessions` trong transaction cung voi course.
- Neu sinh lich that bai, rollback course de tranh course khong co lich khi user da nhap lich.
- Gio ket thuc phai lon hon gio bat dau.
- Ngay ket thuc phai lon hon hoac bang ngay bat dau.
- Neu khong chon thu trong tuan, cho phep tao course khong co lich va hien empty state o page `Lich day`.

## Materials Flow

Teacher Materials:

- Them dropdown loc course:
  - `Toan bo khoa hoc`
  - tung course thuoc Teacher
- Nut upload/tai lieu moi bat buoc chon course cu the.
- Backend insert/update/delete material phai kiem tra course ownership:
  - `materials.course_id -> courses.id`
  - `courses.teacher_id = teacherId`
- Tai lieu co the dung bang `materials` hien co neu du truong `course_id`, `file_name`, `file_path`, `uploaded_by`, `uploaded_at`.

Student Materials:

- Chi hien material cua course co enrollment `ACTIVE` hoac `APPROVED`.
- Course phai la `ACTIVE`.
- Dropdown Student hien cac course Student dang hoc.
- Empty state:
  - Chua tham gia course nao.
  - Course da chon chua co tai lieu.
  - Khong tim thay tai lieu phu hop neu co search/filter.

## Enrollment Flow And Notifications

Student request:

- Student bam tham gia course trong `Tim khoa hoc`.
- Enrollment duoc tao voi `PENDING`, khong nen tao `ACTIVE` truc tiep.
- Tao notification cho Teacher cua course.
- Notification category: `Enrollment`.
- Notification UX type: `ActionRequired`.

Teacher approval:

- Teacher chi duyet/tu choi enrollment cua course minh so huu.
- Approve: enrollment `PENDING -> ACTIVE`.
- Reject: enrollment `PENDING -> REJECTED`.
- Neu schema hien tai dang xoa request khi tu choi, migration nen uu tien giu row voi `REJECTED` de Student/Teacher co lich su trang thai ro rang.
- Tao notification cho Student:
  - Approve: `Informational`.
  - Reject: `Informational`, kem course name.

Neu co nguoi khac xu ly cung request:

- Teacher page refresh khong nen hien request da duoc xu ly.
- Neu can notification cho Teacher khac/admin, dung `Informational`; hien tai co the bo qua neu chi mot Teacher owner.

## Teacher Overview: Teaching Tasks Block

Doi block `Thong bao gan day` thanh `Lich day & viec can xu ly`.

Nguon du lieu uu tien:

1. Lich day hom nay / sap toi:
   - Lay tu `online_sessions` cua course ma Teacher so huu.
   - Uu tien hom nay, sau do 7 ngay toi.

2. Bai tap can cham:
   - Lay tu assignments/submissions neu schema co submission.
   - Neu chua co submission schema, hien duoc assignments sap den han/da qua han nhu task tham khao.

3. Bai kiem tra sap mo / dang mo:
   - Lay tu `exams` cua course Teacher so huu.
   - `Upcoming`: `open_time > now`.
   - `Open`: `open_time <= now <= close_time`.

4. Canh bao thi can xem lai:
   - Lay tu monitoring/attempt alerts neu co bang luu.
   - Neu hien tai monitoring chi la runtime stream, task block chi hien canh bao dang co trong session hien tai hoac de empty cho nhom nay.

UI:

- Giu style Student dashboard/Teacher theme hien tai.
- Moi row nen co title, subtitle, time/status badge.
- ActionRequired row nen co mau warning/attention.
- Empty state: `Khong co lich day hoac viec can xu ly gan day.`
- Error state: hien message trong card, khong crash page.

## Teacher Notifications Page

Notification la su kien can biet/can xu ly, khong phai activity log.

Categories:

- `Enrollment`
  - Student gui yeu cau tham gia course.
  - Yeu cau duoc duyet/tu choi neu co luong can thong bao lai.

- `Assignment`
  - Co bai tap can cham.
  - Bai tap sap het han.

- `Exam`
  - Bai kiem tra sap mo.
  - Bai kiem tra dang mo.
  - Bai kiem tra da ket thuc.

- `Monitoring`
  - Student roi cua so bai thi.
  - Alt-Tab/mat focus.
  - Mat ket noi.
  - Ket noi lai.
  - Canh bao dang chu y.

- `SystemAdmin`
  - Admin duyet/tu choi course.
  - Admin cap nhat quyen.
  - Bao tri.
  - Thong bao he thong quan trong.

UX type:

- `Informational`: chi de biet, vi du course duoc admin duyet, student enrollment duoc chap nhan.
- `ActionRequired`: can giao vien xu ly, vi du enrollment pending, bai tap can cham, monitoring alert dang chu y.

Khong dua vao notification:

- Login success
- Logout
- Mo dashboard
- Xem khoa hoc

Nhung muc tren neu can thi nam o `Hoat dong gan day` hoac log he thong.

UI:

- Co filter/group theo category.
- Co filter `Tat ca`, `Can xu ly`, `Da doc` neu phu hop voi UI hien co.
- Giu action `Danh dau da doc`.
- Empty state rieng:
  - Khong co thong bao.
  - Khong co thong bao can xu ly.
  - Khong co ket qua sau filter/search.

## Data Model And Migration

Neu bang `notifications` hien chi co `id`, `user_id`, `title`, `content`, `is_read`, `created_at`, them migration nho:

- `category VARCHAR(32) DEFAULT 'SystemAdmin'`
- `notification_type VARCHAR(32) DEFAULT 'Informational'`
- `source_type VARCHAR(64) NULL`
- `source_id INT NULL`

Neu can ly do tu choi course:

- Them `courses.rejection_reason TEXT NULL` neu chua co.
- Neu khong them field nay trong phase dau, van cho phep `REJECTED` nhung content notification chua co ly do chi tiet.

Cho lich mac dinh:

- Phase dau co the khong luu recurring template rieng; chi sinh `online_sessions`.
- Neu can edit recurring template ve sau, them bang rieng o phase khac.

Cho enrollment rejection:

- Neu schema cho phep status tu do, dung `REJECTED`.
- Neu co constraint status hien co, migration phai bo sung `REJECTED`.

## Backend Boundaries

Teacher backend methods:

- Course create/update/delete/send approval phai nhan `teacherId`.
- Material CRUD phai join course va check `teacher_id`.
- Schedule CRUD phai join course va check `teacher_id`.
- Enrollment approval/rejection phai join course va check `teacher_id`.
- Exam/result/monitoring queries phai join course va check `teacher_id`.

Student backend methods:

- Available courses: `courses.status = ACTIVE`.
- Student materials: enrollment `ACTIVE/APPROVED` va course `ACTIVE`.
- Student schedule: enrollment `ACTIVE/APPROVED` va course `ACTIVE`.
- Student enrollment request: chi cho course `ACTIVE`.

Admin backend methods:

- Approve course: `PENDING -> ACTIVE`.
- Reject course: `PENDING -> REJECTED`.
- Tao notification cho Teacher khi admin xu ly course.

Notification service/repository:

- Nen them helper tao notification thay vi chen SQL scattered o UI.
- UI chi goi controller/repository action; backend action tu tao notification phu hop.

## Error Handling

- Neu Teacher tao course nhung schedule generation fail, rollback transaction.
- Neu notification insert fail sau khi action chinh thanh cong, khong nen rollback nghiep vu chinh tru khi transaction da bao gom notification. Log/return warning neu hien co co co che.
- Neu file material path khong ton tai, Student van thay metadata nhung khi mo/download can bao loi ro.
- Neu filter khong co du lieu, hien empty state thay vi grid trong.

## Testing And Verification

Build:

- `dotnet build CourseGuard/CourseGuard/CourseGuard.csproj`
- Neu output bi lock boi app dang chay, build ra thu muc rieng voi `-o artifacts/codex-build/...`.

Backend verification:

- Teacher tao course khong the tao `ACTIVE`.
- Admin approve course thi Student moi thay course.
- Student request enrollment sinh notification cho Teacher.
- Teacher approve/reject sinh notification cho Student.
- Teacher khong sua material/schedule/enrollment cua course khac.
- Student khong thay material/schedule neu enrollment chua active.

UI verification:

- Teacher Overview hien task block moi va empty state.
- Teacher Notifications loc/group dung category va type.
- Teacher Materials dropdown loc dung course.
- Student Course Search chi hien approved course.
- Student Materials/Schedule hien dung du lieu theo enrollment.

## Open Implementation Notes

- Can inspect constraint status hien tai cua `courses` va `enrollments` truoc khi viet migration.
- Can xac dinh Admin course approval page hien tai dang dung controller nao de them approve/reject course ma khong rewrite Admin.
- Can giu Student UI visual hien co, chi sua query/logic khi can.
