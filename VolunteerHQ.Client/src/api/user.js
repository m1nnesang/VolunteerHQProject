import api from './client'

export const getMe = () => api.get('/user/me')
export const getMyStats = () => api.get('/user/me/stats')
