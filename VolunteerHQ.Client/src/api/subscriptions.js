import api from './client'

export const getSubscriptions = (page = 1, pageSize = 100) =>
  api.get('/subscription', { params: { page, pageSize } })

export const subscribe = (target, targetId) =>
  api.post('/subscription', { target, targetId })

export const unsubscribe = (subscriptionId) =>
  api.delete(`/subscription/${subscriptionId}`)
