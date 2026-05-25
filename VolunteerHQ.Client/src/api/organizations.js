import api from './client'

export const getOrganizations = (page = 1, pageSize = 12) =>
  api.get('/organization', { params: { page, pageSize } })

export const getOrganization = (id) => api.get(`/organization/${id}`)

export const getMyManagedOrg = () => api.get('/organization/my-managed')

export const createJoinRequest = (orgId, dto) => api.post(`/joinrequest/${orgId}`, dto)

export const getOrganizationMembers = (orgId, page = 1, pageSize = 50) =>
  api.get(`/organization/${orgId}/members`, { params: { page, pageSize } })

export const removeMember = (orgId, targetId) =>
  api.delete(`/organization/${orgId}/members/${targetId}`)

export const updateMemberRole = (orgId, targetId, newRole) =>
  api.put(`/organization/${orgId}/members/${targetId}/role`, { newRole })

export const getJoinRequestsForOrg = (orgId, page = 1, pageSize = 50) =>
  api.get(`/joinrequest/org/${orgId}`, { params: { page, pageSize } })

export const reviewJoinRequest = (requestId, orgId, dto) =>
  api.put(`/joinrequest/${requestId}/review/${orgId}`, dto)
