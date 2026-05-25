import api from './client'

export const getMilitaryUnits = (page = 1, pageSize = 12) =>
  api.get('/militaryunit', { params: { page, pageSize } })

export const getMilitaryUnit = (id) => api.get(`/militaryunit/${id}`)

export const loginMilitaryUnit = (dto) => api.post('/militaryunit/login', dto)
