import api from './client'

export const getComments = (fundraiserId, page = 1, pageSize = 20) =>
  api.get(`/fundraiser/${fundraiserId}/comments`, { params: { page, pageSize } })

export const createComment = (fundraiserId, dto) =>
  api.post(`/fundraiser/${fundraiserId}/comments`, dto)

export const deleteComment = (fundraiserId, commentId) =>
  api.delete(`/fundraiser/${fundraiserId}/comments/${commentId}`)
