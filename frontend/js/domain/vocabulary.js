export const LEVELS = ['A1', 'A2', 'B1', 'B2', 'C1'];

export const TOPIC_LABELS = {
  daily: 'Hàng ngày',
  food: 'Ẩm thực',
  travel: 'Du lịch',
  work: 'Công việc',
  technology: 'Công nghệ',
  health: 'Sức khỏe',
  education: 'Giáo dục',
  science: 'Khoa học',
  society: 'Xã hội',
  shopping: 'Mua sắm',
  family: 'Gia đình',
  weather: 'Thời tiết',
  animals: 'Động vật',
  environment: 'Môi trường & thực vật',
  clothing: 'Quần áo & thời trang',
  personality: 'Tính cách',
  emotions: 'Cảm xúc',
  sports: 'Thể thao',
  arts: 'Nghệ thuật & giải trí',
  home: 'Nhà cửa & đồ dùng',
  finance: 'Tài chính & ngân hàng',
  transport: 'Giao thông',
  communication: 'Giao tiếp & ngôn ngữ',
  numbers: 'Số đếm & đo lường',
  time: 'Thời gian',
  people: 'Con người',
  actions: 'Hành động',
  colors: 'Màu sắc',
  geography: 'Địa lý & địa điểm',
  culture: 'Văn hóa & lễ hội',
  properties: 'Đặc điểm & trạng thái',
  manner: 'Cách thức & mức độ',
};

export function topicLabel(topic) {
  return TOPIC_LABELS[topic] || topic;
}
