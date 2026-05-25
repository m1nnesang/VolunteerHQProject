import api from './client'

export const createReport = (dto) => api.post('/report', dto)
