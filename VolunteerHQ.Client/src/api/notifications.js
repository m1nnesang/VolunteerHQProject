import api from './client'

export const getNotifications = (page = 1, pageSize = 20) =>
  api.get('/notification', { params: { page, pageSize } })

export const markAsRead = (notificationId) =>
  api.put(`/notification/${notificationId}/read`)
