import api from './client'

export const getConversations = () =>
  api.get('/message/conversations')

export const getMessages = (otherUserId, page = 1, pageSize = 50) =>
  api.get(`/message/${otherUserId}`, { params: { page, pageSize } })

export const sendMessage = (dto) =>
  api.post('/message', dto)

export const updateMessage = (messageId, dto) =>
  api.put(`/message/${messageId}`, dto)

export const deleteMessage = (messageId) =>
  api.delete(`/message/${messageId}`)

export const markMessageAsRead = (messageId) =>
  api.put(`/message/${messageId}/read`)
