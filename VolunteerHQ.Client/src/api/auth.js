import api from './client'

export const register = (dto) => api.post('/auth/register', dto)
export const login = (dto) => api.post('/auth/login', dto)
export const refresh = (refreshToken) => api.post('/auth/refresh', refreshToken)
