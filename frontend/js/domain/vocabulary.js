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
};

export function topicLabel(topic) {
  return TOPIC_LABELS[topic] || topic;
}
