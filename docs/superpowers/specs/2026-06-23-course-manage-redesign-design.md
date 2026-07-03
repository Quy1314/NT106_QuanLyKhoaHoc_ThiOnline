# Design Spec: UC_CoursesManage Redesign (Option B - Modal Overlay)

## Tóm tắt (Overview)
Tái thiết kế toàn bộ trang Quản lý Khóa học (`UC_CoursesManage`) theo hướng tối đa hóa không gian hiển thị danh sách khóa học và chuyển toàn bộ biểu mẫu xử lý vào cửa sổ Pop-up (Modal) hiện đại.

## Cấu trúc Giao diện (UI Structure)
### 1. Màn hình chính (Main Screen)
- **Danh sách khóa học (Data Grid)**: Chiếm 100% diện tích màn hình. Loại bỏ hoàn toàn 2 Card bên phải.
- **Thanh công cụ (Toolbar)**: Phía trên cùng bảng chứa tiêu đề "Danh sách khóa học", ô Tìm kiếm (nếu có), và nút "Thêm khóa học" màu xanh nổi bật.
- **Thao tác (Action)**: Khi người dùng nhấp đúp (Double-click) vào một hàng trong lưới, hoặc chọn hàng và bấm nút Sửa, Cửa sổ Modal sẽ hiện lên.

### 2. Cửa sổ Pop-up (Course Modal Dialog)
- **Thiết kế**: Một Form/Dialog hiển thị chính giữa màn hình, viền bo tròn (Rounded corners), đổ bóng, tắt thanh tiêu đề mặc định của Windows (Borderless).
- **Hệ thống Tab (Thẻ)**: Trên cùng của Modal có 2 nút chuyển đổi Tab:
  - **Tab 1: Thông tin khóa học**
  - **Tab 2: Học viên tham gia**

### 3. Chi tiết các Tab (Tab Contents)
#### Tab 1: Thông tin khóa học (Course Info)
- **Hiển thị**: 
  - `txtCourseName` (Tên khóa học)
  - `txtDescription` (Mô tả)
  - `cboTeacher` (Chọn giáo viên)
  - `dtpStartDate`, `dtpEndDate` (Ngày bắt đầu, Kết thúc)
  - `cboStatus` (Trạng thái)
- **Thiết kế Component**: Toàn bộ Input được bọc trong `RoundedPanel` với màu nền đồng bộ, giống hệt trang `UC_UsersManage`.
- **Nút hành động (Bottom)**: Duyệt, Từ chối, Cập nhật, Xóa.

#### Tab 2: Học viên tham gia (Enrollment Management)
- **Hiển thị**:
  - `cboRegStatus` (Bộ lọc: Chờ duyệt, Đã tham gia...)
  - `cboStudent` (Chọn học viên)
- **Nút hành động (Bottom)**: Duyệt tham gia, Xóa/Từ chối.

## Thiết kế Kỹ thuật (Technical Design)
- **Data Flow**: Khi mở Modal, truyền ID của khóa học đang chọn vào Modal. Nếu là "Thêm mới", ID = -1. Modal gọi trực tiếp hàm của `CourseService` để thêm/sửa, hoặc trả kết quả về cho `UC_CoursesManage` để Refresh lại lưới.
- **Code Organization**: Có thể tách riêng Modal thành một class Form mới (ví dụ: `CourseDetailDialog.cs`) để không làm file `UC_CoursesManage.cs` bị quá nặng, tăng tính đóng gói (Encapsulation).
