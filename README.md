# CourseGuard Runbook

Runbook này dùng để setup, chạy, và troubleshoot nhanh project `CourseGuard` trên máy local.

## 1) Yêu cầu môi trường

- Windows 10/11
- .NET SDK 10.x (theo `TargetFramework: net10.0-windows`)
- Internet ổn định để kết nối Supabase
- Quyền chạy WinForms app trên máy local

## 2) Cấu trúc chính

- Solution: `CourseGuard/CourseGuard/CourseGuard.sln`
- Project chính: `CourseGuard/CourseGuard/CourseGuard.csproj`
- Entry point: `CourseGuard/CourseGuard/Frontend/Program.cs`
- Form điều hướng sau login: `CourseGuard/CourseGuard/Frontend/Forms/Admin/RedirectForm.cs`

## 3) Cách chạy nhanh

### Cách 1 - Dùng Visual Studio / Rider

1. Mở `CourseGuard.sln`
2. Chọn startup project: `CourseGuard`
3. Run (`F5` hoặc `Ctrl+F5`)

### Cách 2 - Dùng terminal

Từ thư mục gốc repo:

```powershell
dotnet restore "CourseGuard/CourseGuard/CourseGuard.csproj"
dotnet build "CourseGuard/CourseGuard/CourseGuard.csproj"
dotnet run --project "CourseGuard/CourseGuard/CourseGuard.csproj"
```

## 4) Tài khoản test

- Username: `admin`
- Password: `admin123`

## 5) Flow kiểm thử tối thiểu sau khi chạy

1. Login với tài khoản admin
2. Kiểm tra `Admin Dashboard`:
   - KPI hiển thị số liệu thực từ DB
   - `Recent User Requests` lấy user có `status = PENDING`
   - `Recent Activities` hiển thị `IP + action + thời gian cụ thể`
3. Mở `User Management` và bấm Search
4. Mở `Course Management` và kiểm tra load danh sách
5. Mở `Reports` và kiểm tra lọc dữ liệu theo ngày
6. Login role `teacher` và `student`, kiểm tra layout dashboard đồng bộ style admin
7. Logout và xác nhận quay về màn login

## 6) Lưu ý vận hành hiện tại

- Một số màn admin đang để placeholder nếu chưa có logic đầy đủ (`Device Monitoring`, `Audit Logs`, `Settings`).
- `UC_AdminDashboard` đã dùng dữ liệu thật cho khu vực monitor (KPI + hoạt động auth).
- `TeacherDashboard` và `StudentDashboard` đã được đồng bộ UI theo design system của admin.
- `Recent Activities` hiển thị timestamp tuyệt đối theo format `dd/MM/yyyy HH:mm:ss`.

## 7) Troubleshooting nhanh

### A. Lỗi build kiểu `MSB3021/MSB3027` (file exe bị lock)

Triệu chứng: build báo không copy được `CourseGuard.exe`.

Nguyên nhân: app đang chạy.

Cách xử lý:
1. Đóng app `CourseGuard` đang mở
2. Build lại:

```powershell
dotnet build "CourseGuard/CourseGuard/CourseGuard.csproj"
```

### B. Lỗi `Cannot access a disposed object` khi login / search

Nguyên nhân phổ biến:
- Control bị dispose trong lúc đang load dữ liệu async
- DB load bị gọi quá sớm khi control chưa hiển thị ổn định

Cách xử lý hiện tại đã áp dụng:
- `UC_AdminReports` chỉ load dữ liệu khi control thực sự visible (lazy load)
- `UC_CoursesManage` có guard dispose khi refresh dữ liệu
- Luồng login đã bỏ các điểm block `.Result` để giảm race condition trên UI thread

Nếu còn tái hiện:
1. Ghi lại thao tác gây lỗi (bấm gì trước/sau)
2. Chụp stack trace
3. Gửi kèm ảnh để debug tiếp

### C. Không kết nối được database

Kiểm tra:
- Máy có internet
- Connection string trong:
  - `Backend/Data/CourseGuardDbContext.cs`
  - `Backend/Data/DatabaseAction.cs`
- Không bị firewall/antivirus chặn outbound

## 8) Ghi chú bảo mật

- Project hiện vẫn dùng connection string hardcoded phục vụ môi trường học tập/dev.
- Không nên dùng cấu hình này cho production.
- Khi deploy thật, chuyển sang biến môi trường hoặc secret manager.

