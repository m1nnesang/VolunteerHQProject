import api from './client'

export const getJoinRequest = (requestId, orgId) =>
  api.get(`/joinrequest/${requestId}`, { params: { orgId } })
